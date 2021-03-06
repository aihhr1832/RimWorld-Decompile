﻿using System;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;

namespace Verse
{
	public class CameraDriver : MonoBehaviour
	{
		public CameraShaker shaker = new CameraShaker();

		private Camera cachedCamera = null;

		private GameObject reverbDummy;

		public CameraMapConfig config = new CameraMapConfig_Normal();

		private Vector3 velocity;

		private Vector3 rootPos;

		private float rootSize;

		private float desiredSize;

		private Vector2 desiredDolly = Vector2.zero;

		private Vector2 mouseDragVect = Vector2.zero;

		private bool mouseCoveredByUI = false;

		private float mouseTouchingScreenBottomEdgeStartTime = -1f;

		private float fixedTimeStepBuffer;

		private static int lastViewRectGetFrame = -1;

		private static CellRect lastViewRect;

		public const float MaxDeltaTime = 0.1f;

		private const float ScreenDollyEdgeWidth = 20f;

		private const float ScreenDollyEdgeWidth_BottomFullscreen = 6f;

		private const float MinDurationForMouseToTouchScreenBottomEdgeToDolly = 0.28f;

		private const float MapEdgeClampMarginCells = -2f;

		public const float StartingSize = 24f;

		private const float MinSize = 11f;

		private const float MaxSize = 60f;

		private const float ZoomTightness = 0.4f;

		private const float ZoomScaleFromAltDenominator = 35f;

		private const float PageKeyZoomRate = 4f;

		private const float ScrollWheelZoomRate = 0.35f;

		public const float MinAltitude = 15f;

		private const float MaxAltitude = 65f;

		private const float ReverbDummyAltitude = 65f;

		public CameraDriver()
		{
		}

		private Camera MyCamera
		{
			get
			{
				if (this.cachedCamera == null)
				{
					this.cachedCamera = base.GetComponent<Camera>();
				}
				return this.cachedCamera;
			}
		}

		private float ScreenDollyEdgeWidthBottom
		{
			get
			{
				float result;
				if (Screen.fullScreen)
				{
					result = 6f;
				}
				else
				{
					result = 20f;
				}
				return result;
			}
		}

		public CameraZoomRange CurrentZoom
		{
			get
			{
				CameraZoomRange result;
				if (this.rootSize < 12f)
				{
					result = CameraZoomRange.Closest;
				}
				else if (this.rootSize < 13.8f)
				{
					result = CameraZoomRange.Close;
				}
				else if (this.rootSize < 42f)
				{
					result = CameraZoomRange.Middle;
				}
				else if (this.rootSize < 57f)
				{
					result = CameraZoomRange.Far;
				}
				else
				{
					result = CameraZoomRange.Furthest;
				}
				return result;
			}
		}

		private Vector3 CurrentRealPosition
		{
			get
			{
				return this.MyCamera.transform.position;
			}
		}

		private bool AnythingPreventsCameraMotion
		{
			get
			{
				return Find.WindowStack.WindowsPreventCameraMotion || WorldRendererUtility.WorldRenderedNow;
			}
		}

		public IntVec3 MapPosition
		{
			get
			{
				IntVec3 result = this.CurrentRealPosition.ToIntVec3();
				result.y = 0;
				return result;
			}
		}

		public CellRect CurrentViewRect
		{
			get
			{
				if (Time.frameCount != CameraDriver.lastViewRectGetFrame)
				{
					CameraDriver.lastViewRect = default(CellRect);
					float num = (float)UI.screenWidth / (float)UI.screenHeight;
					CameraDriver.lastViewRect.minX = Mathf.FloorToInt(this.CurrentRealPosition.x - this.rootSize * num - 1f);
					CameraDriver.lastViewRect.maxX = Mathf.CeilToInt(this.CurrentRealPosition.x + this.rootSize * num);
					CameraDriver.lastViewRect.minZ = Mathf.FloorToInt(this.CurrentRealPosition.z - this.rootSize - 1f);
					CameraDriver.lastViewRect.maxZ = Mathf.CeilToInt(this.CurrentRealPosition.z + this.rootSize);
					CameraDriver.lastViewRectGetFrame = Time.frameCount;
				}
				return CameraDriver.lastViewRect;
			}
		}

		public static float HitchReduceFactor
		{
			get
			{
				float result = 1f;
				if (Time.deltaTime > 0.1f)
				{
					result = 0.1f / Time.deltaTime;
				}
				return result;
			}
		}

		public float CellSizePixels
		{
			get
			{
				return (float)UI.screenHeight / (this.rootSize * 2f);
			}
		}

		public void Awake()
		{
			this.ResetSize();
			this.reverbDummy = GameObject.Find("ReverbZoneDummy");
			this.ApplyPositionToGameObject();
			this.MyCamera.farClipPlane = 71.5f;
		}

		public void OnPreRender()
		{
			if (!LongEventHandler.ShouldWaitForEvent)
			{
				if (Find.CurrentMap == null)
				{
				}
			}
		}

		public void OnPreCull()
		{
			if (!LongEventHandler.ShouldWaitForEvent)
			{
				if (Find.CurrentMap != null)
				{
					if (!WorldRendererUtility.WorldRenderedNow)
					{
						Find.CurrentMap.weatherManager.DrawAllWeather();
					}
				}
			}
		}

		public void OnGUI()
		{
			GUI.depth = 100;
			if (!LongEventHandler.ShouldWaitForEvent)
			{
				if (Find.CurrentMap != null)
				{
					UnityGUIBugsFixer.OnGUI();
					this.mouseCoveredByUI = false;
					if (Find.WindowStack.GetWindowAt(UI.MousePositionOnUIInverted) != null)
					{
						this.mouseCoveredByUI = true;
					}
					if (!this.AnythingPreventsCameraMotion)
					{
						if (Event.current.type == EventType.MouseDrag && Event.current.button == 2)
						{
							this.mouseDragVect = Event.current.delta;
							Event.current.Use();
						}
						float num = 0f;
						if (Event.current.type == EventType.ScrollWheel)
						{
							num -= Event.current.delta.y * 0.35f;
							PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.CameraZoom, KnowledgeAmount.TinyInteraction);
						}
						if (KeyBindingDefOf.MapZoom_In.KeyDownEvent)
						{
							num += 4f;
							PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.CameraZoom, KnowledgeAmount.SmallInteraction);
						}
						if (KeyBindingDefOf.MapZoom_Out.KeyDownEvent)
						{
							num -= 4f;
							PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.CameraZoom, KnowledgeAmount.SmallInteraction);
						}
						this.desiredSize -= num * this.config.zoomSpeed * this.rootSize / 35f;
						this.desiredSize = Mathf.Clamp(this.desiredSize, 11f, 60f);
						this.desiredDolly = Vector3.zero;
						if (KeyBindingDefOf.MapDolly_Left.IsDown)
						{
							this.desiredDolly.x = -this.config.dollyRateKeys;
						}
						if (KeyBindingDefOf.MapDolly_Right.IsDown)
						{
							this.desiredDolly.x = this.config.dollyRateKeys;
						}
						if (KeyBindingDefOf.MapDolly_Up.IsDown)
						{
							this.desiredDolly.y = this.config.dollyRateKeys;
						}
						if (KeyBindingDefOf.MapDolly_Down.IsDown)
						{
							this.desiredDolly.y = -this.config.dollyRateKeys;
						}
						if (this.mouseDragVect != Vector2.zero)
						{
							this.mouseDragVect *= CameraDriver.HitchReduceFactor;
							this.mouseDragVect.x = this.mouseDragVect.x * -1f;
							this.desiredDolly += this.mouseDragVect * this.config.dollyRateMouseDrag;
							this.mouseDragVect = Vector2.zero;
						}
						this.config.ConfigOnGUI();
					}
				}
			}
		}

		public void Update()
		{
			if (LongEventHandler.ShouldWaitForEvent)
			{
				if (Current.SubcameraDriver != null)
				{
					Current.SubcameraDriver.UpdatePositions(this.MyCamera);
				}
			}
			else if (Find.CurrentMap != null)
			{
				Vector2 lhs = this.CalculateCurInputDollyVect();
				if (lhs != Vector2.zero)
				{
					float d = (this.rootSize - 11f) / 49f * 0.7f + 0.3f;
					this.velocity = new Vector3(lhs.x, 0f, lhs.y) * d;
					PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.CameraDolly, KnowledgeAmount.FrameInteraction);
				}
				if (!this.AnythingPreventsCameraMotion)
				{
					float d2 = Time.deltaTime * CameraDriver.HitchReduceFactor;
					this.rootPos += this.velocity * d2 * this.config.moveSpeedScale;
					this.rootPos.x = Mathf.Clamp(this.rootPos.x, 2f, (float)Find.CurrentMap.Size.x + -2f);
					this.rootPos.z = Mathf.Clamp(this.rootPos.z, 2f, (float)Find.CurrentMap.Size.z + -2f);
				}
				int num = Gen.FixedTimeStepUpdate(ref this.fixedTimeStepBuffer, 60f);
				for (int i = 0; i < num; i++)
				{
					if (this.velocity != Vector3.zero)
					{
						this.velocity *= this.config.camSpeedDecayFactor;
						if (this.velocity.magnitude < 0.1f)
						{
							this.velocity = Vector3.zero;
						}
					}
					if (this.config.smoothZoom)
					{
						float num2 = Mathf.Lerp(this.rootSize, this.desiredSize, 0.05f);
						this.desiredSize += (num2 - this.rootSize) * this.config.zoomPreserveFactor;
						this.rootSize = num2;
					}
					else
					{
						float num3 = this.desiredSize - this.rootSize;
						float num4 = num3 * 0.4f;
						this.desiredSize += this.config.zoomPreserveFactor * num4;
						this.rootSize += num4;
					}
					this.config.ConfigFixedUpdate_60(ref this.velocity);
				}
				this.shaker.Update();
				this.ApplyPositionToGameObject();
				Current.SubcameraDriver.UpdatePositions(this.MyCamera);
				if (Find.CurrentMap != null)
				{
					RememberedCameraPos rememberedCameraPos = Find.CurrentMap.rememberedCameraPos;
					rememberedCameraPos.rootPos = this.rootPos;
					rememberedCameraPos.rootSize = this.rootSize;
				}
			}
		}

		private void ApplyPositionToGameObject()
		{
			this.rootPos.y = 15f + (this.rootSize - 11f) / 49f * 50f;
			this.MyCamera.orthographicSize = this.rootSize;
			this.MyCamera.transform.position = this.rootPos + this.shaker.ShakeOffset;
			Vector3 position = base.transform.position;
			position.y = 65f;
			this.reverbDummy.transform.position = position;
		}

		private Vector2 CalculateCurInputDollyVect()
		{
			Vector2 vector = this.desiredDolly;
			bool flag = false;
			if ((UnityData.isEditor || Screen.fullScreen) && Prefs.EdgeScreenScroll && !this.mouseCoveredByUI)
			{
				Vector2 mousePositionOnUI = UI.MousePositionOnUI;
				Vector2 point = mousePositionOnUI;
				point.y = (float)UI.screenHeight - point.y;
				Rect rect = new Rect(0f, 0f, 200f, 200f);
				Rect rect2 = new Rect((float)(UI.screenWidth - 250), 0f, 255f, 255f);
				Rect rect3 = new Rect(0f, (float)(UI.screenHeight - 250), 225f, 255f);
				Rect rect4 = new Rect((float)(UI.screenWidth - 250), (float)(UI.screenHeight - 250), 255f, 255f);
				MainTabWindow_Inspect mainTabWindow_Inspect = (MainTabWindow_Inspect)MainButtonDefOf.Inspect.TabWindow;
				if (Find.MainTabsRoot.OpenTab == MainButtonDefOf.Inspect && mainTabWindow_Inspect.RecentHeight > rect3.height)
				{
					rect3.yMin = (float)UI.screenHeight - mainTabWindow_Inspect.RecentHeight;
				}
				if (!rect.Contains(point) && !rect3.Contains(point) && !rect2.Contains(point) && !rect4.Contains(point))
				{
					Vector2 b = new Vector2(0f, 0f);
					if (mousePositionOnUI.x >= 0f && mousePositionOnUI.x < 20f)
					{
						b.x -= this.config.dollyRateScreenEdge;
					}
					if (mousePositionOnUI.x <= (float)UI.screenWidth && mousePositionOnUI.x > (float)UI.screenWidth - 20f)
					{
						b.x += this.config.dollyRateScreenEdge;
					}
					if (mousePositionOnUI.y <= (float)UI.screenHeight && mousePositionOnUI.y > (float)UI.screenHeight - 20f)
					{
						b.y += this.config.dollyRateScreenEdge;
					}
					if (mousePositionOnUI.y >= 0f && mousePositionOnUI.y < this.ScreenDollyEdgeWidthBottom)
					{
						if (this.mouseTouchingScreenBottomEdgeStartTime < 0f)
						{
							this.mouseTouchingScreenBottomEdgeStartTime = Time.realtimeSinceStartup;
						}
						if (Time.realtimeSinceStartup - this.mouseTouchingScreenBottomEdgeStartTime >= 0.28f)
						{
							b.y -= this.config.dollyRateScreenEdge;
						}
						flag = true;
					}
					vector += b;
				}
			}
			if (!flag)
			{
				this.mouseTouchingScreenBottomEdgeStartTime = -1f;
			}
			if (Input.GetKey(KeyCode.LeftShift))
			{
				vector *= 2.4f;
			}
			return vector;
		}

		public void Expose()
		{
			if (Scribe.EnterNode("cameraMap"))
			{
				try
				{
					Scribe_Values.Look<Vector3>(ref this.rootPos, "camRootPos", default(Vector3), false);
					Scribe_Values.Look<float>(ref this.desiredSize, "desiredSize", 0f, false);
					this.rootSize = this.desiredSize;
				}
				finally
				{
					Scribe.ExitNode();
				}
			}
		}

		public void ResetSize()
		{
			this.desiredSize = 24f;
			this.rootSize = this.desiredSize;
		}

		public void JumpToCurrentMapLoc(IntVec3 cell)
		{
			this.JumpToCurrentMapLoc(cell.ToVector3Shifted());
		}

		public void JumpToCurrentMapLoc(Vector3 loc)
		{
			this.rootPos = new Vector3(loc.x, this.rootPos.y, loc.z);
		}

		public void SetRootPosAndSize(Vector3 rootPos, float rootSize)
		{
			this.rootPos = rootPos;
			this.rootSize = rootSize;
			this.desiredDolly = Vector2.zero;
			this.desiredSize = rootSize;
			LongEventHandler.ExecuteWhenFinished(new Action(this.ApplyPositionToGameObject));
		}

		// Note: this type is marked as 'beforefieldinit'.
		static CameraDriver()
		{
		}
	}
}
