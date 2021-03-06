﻿using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	public static class ExpandableWorldObjectsUtility
	{
		private static float transitionPct;

		private static float expandMoreTransitionPct;

		private static List<WorldObject> tmpWorldObjects = new List<WorldObject>();

		private const float WorldObjectIconSize = 30f;

		private const float ExpandMoreWorldObjectIconSizeFactor = 1.35f;

		private const float TransitionSpeed = 3f;

		private const float ExpandMoreTransitionSpeed = 4f;

		[CompilerGenerated]
		private static Func<WorldObject, float> <>f__am$cache0;

		[CompilerGenerated]
		private static Func<WorldObject, int> <>f__am$cache1;

		public static float TransitionPct
		{
			get
			{
				float result;
				if (!Find.PlaySettings.showExpandingIcons)
				{
					result = 0f;
				}
				else
				{
					result = ExpandableWorldObjectsUtility.transitionPct;
				}
				return result;
			}
		}

		public static float ExpandMoreTransitionPct
		{
			get
			{
				float result;
				if (!Find.PlaySettings.showExpandingIcons)
				{
					result = 0f;
				}
				else
				{
					result = ExpandableWorldObjectsUtility.expandMoreTransitionPct;
				}
				return result;
			}
		}

		public static void ExpandableWorldObjectsUpdate()
		{
			float num = Time.deltaTime * 3f;
			if (Find.WorldCameraDriver.CurrentZoom <= WorldCameraZoomRange.VeryClose)
			{
				ExpandableWorldObjectsUtility.transitionPct -= num;
			}
			else
			{
				ExpandableWorldObjectsUtility.transitionPct += num;
			}
			ExpandableWorldObjectsUtility.transitionPct = Mathf.Clamp01(ExpandableWorldObjectsUtility.transitionPct);
			float num2 = Time.deltaTime * 4f;
			if (Find.WorldCameraDriver.CurrentZoom <= WorldCameraZoomRange.Far)
			{
				ExpandableWorldObjectsUtility.expandMoreTransitionPct -= num2;
			}
			else
			{
				ExpandableWorldObjectsUtility.expandMoreTransitionPct += num2;
			}
			ExpandableWorldObjectsUtility.expandMoreTransitionPct = Mathf.Clamp01(ExpandableWorldObjectsUtility.expandMoreTransitionPct);
		}

		public static void ExpandableWorldObjectsOnGUI()
		{
			if (ExpandableWorldObjectsUtility.TransitionPct != 0f)
			{
				ExpandableWorldObjectsUtility.tmpWorldObjects.Clear();
				ExpandableWorldObjectsUtility.tmpWorldObjects.AddRange(Find.WorldObjects.AllWorldObjects);
				ExpandableWorldObjectsUtility.SortByExpandingIconPriority(ExpandableWorldObjectsUtility.tmpWorldObjects);
				WorldTargeter worldTargeter = Find.WorldTargeter;
				List<WorldObject> worldObjectsUnderMouse = null;
				if (worldTargeter.IsTargeting)
				{
					worldObjectsUnderMouse = GenWorldUI.WorldObjectsUnderMouse(UI.MousePositionOnUI);
				}
				for (int i = 0; i < ExpandableWorldObjectsUtility.tmpWorldObjects.Count; i++)
				{
					WorldObject worldObject = ExpandableWorldObjectsUtility.tmpWorldObjects[i];
					if (worldObject.def.expandingIcon)
					{
						if (!worldObject.HiddenBehindTerrainNow())
						{
							Color expandingIconColor = worldObject.ExpandingIconColor;
							expandingIconColor.a = ExpandableWorldObjectsUtility.TransitionPct;
							if (worldTargeter.IsTargetedNow(worldObject, worldObjectsUnderMouse))
							{
								float num = GenMath.LerpDouble(-1f, 1f, 0.7f, 1f, Mathf.Sin(Time.time * 8f));
								expandingIconColor.r *= num;
								expandingIconColor.g *= num;
								expandingIconColor.b *= num;
							}
							GUI.color = expandingIconColor;
							GUI.DrawTexture(ExpandableWorldObjectsUtility.ExpandedIconScreenRect(worldObject), worldObject.ExpandingIcon);
						}
					}
				}
				ExpandableWorldObjectsUtility.tmpWorldObjects.Clear();
				GUI.color = Color.white;
			}
		}

		public static Rect ExpandedIconScreenRect(WorldObject o)
		{
			Vector2 vector = o.ScreenPos();
			float num;
			if (o.ExpandMore)
			{
				num = Mathf.Lerp(30f, 40.5f, ExpandableWorldObjectsUtility.ExpandMoreTransitionPct);
			}
			else
			{
				num = 30f;
			}
			return new Rect(vector.x - num / 2f, vector.y - num / 2f, num, num);
		}

		public static bool IsExpanded(WorldObject o)
		{
			return ExpandableWorldObjectsUtility.TransitionPct > 0.5f && o.def.expandingIcon;
		}

		public static void GetExpandedWorldObjectUnderMouse(Vector2 mousePos, List<WorldObject> outList)
		{
			outList.Clear();
			Vector2 point = mousePos;
			point.y = (float)UI.screenHeight - point.y;
			List<WorldObject> allWorldObjects = Find.WorldObjects.AllWorldObjects;
			for (int i = 0; i < allWorldObjects.Count; i++)
			{
				WorldObject worldObject = allWorldObjects[i];
				if (ExpandableWorldObjectsUtility.IsExpanded(worldObject))
				{
					if (ExpandableWorldObjectsUtility.ExpandedIconScreenRect(worldObject).Contains(point))
					{
						if (!worldObject.HiddenBehindTerrainNow())
						{
							outList.Add(worldObject);
						}
					}
				}
			}
			ExpandableWorldObjectsUtility.SortByExpandingIconPriority(outList);
			outList.Reverse();
		}

		private static void SortByExpandingIconPriority(List<WorldObject> worldObjects)
		{
			worldObjects.SortBy(delegate(WorldObject x)
			{
				float num = x.ExpandingIconPriority;
				if (x.Faction != null && x.Faction.IsPlayer)
				{
					num += 0.001f;
				}
				return num;
			}, (WorldObject x) => x.ID);
		}

		// Note: this type is marked as 'beforefieldinit'.
		static ExpandableWorldObjectsUtility()
		{
		}

		[CompilerGenerated]
		private static float <SortByExpandingIconPriority>m__0(WorldObject x)
		{
			float num = x.ExpandingIconPriority;
			if (x.Faction != null && x.Faction.IsPlayer)
			{
				num += 0.001f;
			}
			return num;
		}

		[CompilerGenerated]
		private static int <SortByExpandingIconPriority>m__1(WorldObject x)
		{
			return x.ID;
		}
	}
}
