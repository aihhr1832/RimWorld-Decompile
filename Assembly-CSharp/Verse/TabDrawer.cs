﻿using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using RimWorld;
using UnityEngine;
using Verse.Sound;

namespace Verse
{
	public static class TabDrawer
	{
		private const float MaxTabWidth = 200f;

		public const float TabHeight = 32f;

		public const float TabHoriztonalOverlap = 10f;

		private static List<TabRecord> tmpTabs = new List<TabRecord>();

		[CompilerGenerated]
		private static Predicate<TabRecord> <>f__am$cache0;

		public static TabRecord DrawTabs(Rect baseRect, List<TabRecord> tabs, int rows)
		{
			TabRecord result;
			if (rows <= 1)
			{
				result = TabDrawer.DrawTabs(baseRect, tabs, 200f);
			}
			else
			{
				int num = Mathf.FloorToInt((float)(tabs.Count / rows));
				int num2 = 0;
				TabRecord tabRecord = null;
				Rect rect = baseRect;
				baseRect.yMin -= (float)(rows - 1) * 31f;
				Rect rect2 = baseRect;
				rect2.yMax = rect.y;
				Widgets.DrawMenuSection(rect2);
				for (int i = 0; i < rows; i++)
				{
					int num3 = (i != 0) ? num : (tabs.Count - (rows - 1) * num);
					TabDrawer.tmpTabs.Clear();
					for (int j = num2; j < num2 + num3; j++)
					{
						TabDrawer.tmpTabs.Add(tabs[j]);
					}
					TabRecord tabRecord2 = TabDrawer.DrawTabs(baseRect, TabDrawer.tmpTabs, baseRect.width);
					if (tabRecord2 != null)
					{
						tabRecord = tabRecord2;
					}
					baseRect.yMin += 31f;
					num2 += num3;
				}
				TabDrawer.tmpTabs.Clear();
				result = tabRecord;
			}
			return result;
		}

		public static TabRecord DrawTabs(Rect baseRect, List<TabRecord> tabs, float maxTabWidth = 200f)
		{
			TabRecord tabRecord = null;
			TabRecord tabRecord2 = tabs.Find((TabRecord t) => t.Selected);
			float num = baseRect.width + (float)(tabs.Count - 1) * 10f;
			float tabWidth = num / (float)tabs.Count;
			if (tabWidth > maxTabWidth)
			{
				tabWidth = maxTabWidth;
			}
			Rect position = new Rect(baseRect);
			position.y -= 32f;
			position.height = 9999f;
			GUI.BeginGroup(position);
			Text.Anchor = TextAnchor.MiddleCenter;
			Text.Font = GameFont.Small;
			Func<TabRecord, Rect> func = delegate(TabRecord tab)
			{
				int num2 = tabs.IndexOf(tab);
				float x = (float)num2 * (tabWidth - 10f);
				return new Rect(x, 1f, tabWidth, 32f);
			};
			List<TabRecord> list = tabs.ListFullCopy<TabRecord>();
			if (tabRecord2 != null)
			{
				list.Remove(tabRecord2);
				list.Add(tabRecord2);
			}
			TabRecord tabRecord3 = null;
			List<TabRecord> list2 = list.ListFullCopy<TabRecord>();
			list2.Reverse();
			for (int i = 0; i < list2.Count; i++)
			{
				TabRecord tabRecord4 = list2[i];
				Rect rect = func(tabRecord4);
				if (tabRecord3 == null && Mouse.IsOver(rect))
				{
					tabRecord3 = tabRecord4;
				}
				MouseoverSounds.DoRegion(rect, SoundDefOf.Mouseover_Tab);
				if (Widgets.ButtonInvisible(rect, false))
				{
					tabRecord = tabRecord4;
				}
			}
			foreach (TabRecord tabRecord5 in list)
			{
				Rect rect2 = func(tabRecord5);
				tabRecord5.Draw(rect2);
			}
			Text.Anchor = TextAnchor.UpperLeft;
			GUI.EndGroup();
			if (tabRecord != null && tabRecord != tabRecord2)
			{
				SoundDefOf.RowTabSelect.PlayOneShotOnCamera(null);
				if (tabRecord.clickedAction != null)
				{
					tabRecord.clickedAction();
				}
			}
			return tabRecord;
		}

		// Note: this type is marked as 'beforefieldinit'.
		static TabDrawer()
		{
		}

		[CompilerGenerated]
		private static bool <DrawTabs>m__0(TabRecord t)
		{
			return t.Selected;
		}

		[CompilerGenerated]
		private sealed class <DrawTabs>c__AnonStorey0
		{
			internal List<TabRecord> tabs;

			internal float tabWidth;

			public <DrawTabs>c__AnonStorey0()
			{
			}

			internal Rect <>m__0(TabRecord tab)
			{
				int num = this.tabs.IndexOf(tab);
				float x = (float)num * (this.tabWidth - 10f);
				return new Rect(x, 1f, this.tabWidth, 32f);
			}
		}
	}
}
