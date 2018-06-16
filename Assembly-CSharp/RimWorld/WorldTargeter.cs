﻿using System;
using System.Collections.Generic;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	// Token: 0x020008F4 RID: 2292
	[StaticConstructorOnStartup]
	public class WorldTargeter
	{
		// Token: 0x1700088B RID: 2187
		// (get) Token: 0x060034F1 RID: 13553 RVA: 0x001C44FC File Offset: 0x001C28FC
		public bool IsTargeting
		{
			get
			{
				return this.action != null;
			}
		}

		// Token: 0x060034F2 RID: 13554 RVA: 0x001C451D File Offset: 0x001C291D
		public void BeginTargeting(Func<GlobalTargetInfo, bool> action, bool canTargetTiles, Texture2D mouseAttachment = null, bool closeWorldTabWhenFinished = false, Action onUpdate = null, Func<GlobalTargetInfo, string> extraLabelGetter = null)
		{
			this.action = action;
			this.canTargetTiles = canTargetTiles;
			this.mouseAttachment = mouseAttachment;
			this.closeWorldTabWhenFinished = closeWorldTabWhenFinished;
			this.onUpdate = onUpdate;
			this.extraLabelGetter = extraLabelGetter;
		}

		// Token: 0x060034F3 RID: 13555 RVA: 0x001C454D File Offset: 0x001C294D
		public void StopTargeting()
		{
			if (this.closeWorldTabWhenFinished)
			{
				CameraJumper.TryHideWorld();
			}
			this.action = null;
			this.canTargetTiles = false;
			this.mouseAttachment = null;
			this.closeWorldTabWhenFinished = false;
			this.onUpdate = null;
			this.extraLabelGetter = null;
		}

		// Token: 0x060034F4 RID: 13556 RVA: 0x001C458C File Offset: 0x001C298C
		public void ProcessInputEvents()
		{
			if (Event.current.type == EventType.MouseDown)
			{
				if (Event.current.button == 0)
				{
					if (this.IsTargeting)
					{
						GlobalTargetInfo arg = this.CurrentTargetUnderMouse();
						if (this.action(arg))
						{
							SoundDefOf.Tick_High.PlayOneShotOnCamera(null);
							this.StopTargeting();
						}
						Event.current.Use();
					}
				}
				if (Event.current.button == 1)
				{
					if (this.IsTargeting)
					{
						SoundDefOf.CancelMode.PlayOneShotOnCamera(null);
						this.StopTargeting();
						Event.current.Use();
					}
				}
			}
			if (KeyBindingDefOf.Cancel.KeyDownEvent && this.IsTargeting)
			{
				SoundDefOf.CancelMode.PlayOneShotOnCamera(null);
				this.StopTargeting();
				Event.current.Use();
			}
		}

		// Token: 0x060034F5 RID: 13557 RVA: 0x001C4670 File Offset: 0x001C2A70
		public void TargeterOnGUI()
		{
			if (this.IsTargeting && !Mouse.IsInputBlockedNow)
			{
				Vector2 mousePosition = Event.current.mousePosition;
				Texture2D image = this.mouseAttachment ?? TexCommand.Attack;
				Rect position = new Rect(mousePosition.x + 8f, mousePosition.y + 8f, 32f, 32f);
				GUI.DrawTexture(position, image);
				if (this.extraLabelGetter != null)
				{
					GUI.color = Color.white;
					string text = this.extraLabelGetter(this.CurrentTargetUnderMouse());
					if (!text.NullOrEmpty())
					{
						Color color = GUI.color;
						GUI.color = Color.white;
						Rect rect = new Rect(position.xMax, position.y, 9999f, 100f);
						Vector2 vector = Text.CalcSize(text);
						GUI.DrawTexture(new Rect(rect.x - vector.x * 0.1f, rect.y, vector.x * 1.2f, vector.y), TexUI.GrayTextBG);
						GUI.color = color;
						Widgets.Label(rect, text);
					}
					GUI.color = Color.white;
				}
			}
		}

		// Token: 0x060034F6 RID: 13558 RVA: 0x001C47AC File Offset: 0x001C2BAC
		public void TargeterUpdate()
		{
			if (this.IsTargeting)
			{
				Vector3 pos = Vector3.zero;
				GlobalTargetInfo globalTargetInfo = this.CurrentTargetUnderMouse();
				if (globalTargetInfo.HasWorldObject)
				{
					pos = globalTargetInfo.WorldObject.DrawPos;
				}
				else if (globalTargetInfo.Tile >= 0)
				{
					pos = Find.WorldGrid.GetTileCenter(globalTargetInfo.Tile);
				}
				if (globalTargetInfo.IsValid && !Mouse.IsInputBlockedNow)
				{
					WorldRendererUtility.DrawQuadTangentialToPlanet(pos, 0.8f * Find.WorldGrid.averageTileSize, 0.018f, WorldMaterials.CurTargetingMat, false, false, null);
				}
				if (this.onUpdate != null)
				{
					this.onUpdate();
				}
			}
		}

		// Token: 0x060034F7 RID: 13559 RVA: 0x001C4860 File Offset: 0x001C2C60
		public bool IsTargetedNow(WorldObject o, List<WorldObject> worldObjectsUnderMouse = null)
		{
			bool result;
			if (!this.IsTargeting)
			{
				result = false;
			}
			else
			{
				if (worldObjectsUnderMouse == null)
				{
					worldObjectsUnderMouse = GenWorldUI.WorldObjectsUnderMouse(UI.MousePositionOnUI);
				}
				result = (worldObjectsUnderMouse.Any<WorldObject>() && o == worldObjectsUnderMouse[0]);
			}
			return result;
		}

		// Token: 0x060034F8 RID: 13560 RVA: 0x001C48B8 File Offset: 0x001C2CB8
		private GlobalTargetInfo CurrentTargetUnderMouse()
		{
			GlobalTargetInfo result;
			if (!this.IsTargeting)
			{
				result = GlobalTargetInfo.Invalid;
			}
			else
			{
				List<WorldObject> list = GenWorldUI.WorldObjectsUnderMouse(UI.MousePositionOnUI);
				if (list.Any<WorldObject>())
				{
					result = list[0];
				}
				else if (this.canTargetTiles)
				{
					int num = GenWorld.MouseTile(false);
					if (num >= 0)
					{
						result = new GlobalTargetInfo(num);
					}
					else
					{
						result = GlobalTargetInfo.Invalid;
					}
				}
				else
				{
					result = GlobalTargetInfo.Invalid;
				}
			}
			return result;
		}

		// Token: 0x04001C96 RID: 7318
		private Func<GlobalTargetInfo, bool> action;

		// Token: 0x04001C97 RID: 7319
		private bool canTargetTiles;

		// Token: 0x04001C98 RID: 7320
		private Texture2D mouseAttachment;

		// Token: 0x04001C99 RID: 7321
		public bool closeWorldTabWhenFinished;

		// Token: 0x04001C9A RID: 7322
		private Action onUpdate;

		// Token: 0x04001C9B RID: 7323
		private Func<GlobalTargetInfo, string> extraLabelGetter;

		// Token: 0x04001C9C RID: 7324
		private const float BaseFeedbackTexSize = 0.8f;
	}
}
