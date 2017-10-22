using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public static class DateReadout
	{
		public const float Height = 48f;

		private const float DateRightPadding = 7f;

		private static string dateString;

		private static int dateStringDay;

		private static Season dateStringSeason;

		private static Quadrum dateStringQuadrum;

		private static int dateStringYear;

		private static readonly List<string> fastHourStrings;

		static DateReadout()
		{
			DateReadout.dateStringDay = -1;
			DateReadout.dateStringSeason = Season.Undefined;
			DateReadout.dateStringQuadrum = Quadrum.Undefined;
			DateReadout.dateStringYear = -1;
			DateReadout.fastHourStrings = new List<string>();
			DateReadout.Reset();
		}

		public static void Reset()
		{
			DateReadout.dateString = (string)null;
			DateReadout.dateStringDay = -1;
			DateReadout.dateStringSeason = Season.Undefined;
			DateReadout.dateStringQuadrum = Quadrum.Undefined;
			DateReadout.dateStringYear = -1;
			DateReadout.fastHourStrings.Clear();
			for (int i = 0; i < 24; i++)
			{
				DateReadout.fastHourStrings.Add(i + "LetterHour".Translate());
			}
		}

		public static void DateOnGUI(Rect dateRect)
		{
			Vector2 location;
			if (WorldRendererUtility.WorldRenderedNow && Find.WorldSelector.selectedTile >= 0)
			{
				location = Find.WorldGrid.LongLatOf(Find.WorldSelector.selectedTile);
			}
			else if (WorldRendererUtility.WorldRenderedNow && Find.WorldSelector.NumSelectedObjects > 0)
			{
				location = Find.WorldGrid.LongLatOf(Find.WorldSelector.FirstSelectedObject.Tile);
			}
			else
			{
				if (Find.VisibleMap == null)
					return;
				location = Find.WorldGrid.LongLatOf(Find.VisibleMap.Tile);
			}
			int index = GenDate.HourInteger(Find.TickManager.TicksAbs, location.x);
			int num = GenDate.DayOfTwelfth(Find.TickManager.TicksAbs, location.x);
			Season season = GenDate.Season(Find.TickManager.TicksAbs, location);
			Quadrum quadrum = GenDate.Quadrum(Find.TickManager.TicksAbs, location.x);
			int num2 = GenDate.Year(Find.TickManager.TicksAbs, location.x);
			if (num != DateReadout.dateStringDay || season != DateReadout.dateStringSeason || quadrum != DateReadout.dateStringQuadrum || num2 != DateReadout.dateStringYear)
			{
				DateReadout.dateString = GenDate.DateReadoutStringAt(Find.TickManager.TicksAbs, location);
				DateReadout.dateStringDay = num;
				DateReadout.dateStringSeason = season;
				DateReadout.dateStringQuadrum = quadrum;
				DateReadout.dateStringYear = num2;
			}
			Text.Font = GameFont.Small;
			Vector2 vector = Text.CalcSize(DateReadout.fastHourStrings[index]);
			float x = vector.x;
			Vector2 vector2 = Text.CalcSize(DateReadout.dateString);
			float num3 = Mathf.Max(x, (float)(vector2.x + 7.0));
			dateRect.xMin = dateRect.xMax - num3;
			if (Mouse.IsOver(dateRect))
			{
				Widgets.DrawHighlight(dateRect);
			}
			GUI.BeginGroup(dateRect);
			Text.Font = GameFont.Small;
			Text.Anchor = TextAnchor.UpperRight;
			Rect rect = dateRect.AtZero();
			rect.xMax -= 7f;
			Widgets.Label(rect, DateReadout.fastHourStrings[index]);
			rect.yMin += 26f;
			Widgets.Label(rect, DateReadout.dateString);
			Text.Anchor = TextAnchor.UpperLeft;
			GUI.EndGroup();
			TooltipHandler.TipRegion(dateRect, new TipSignal((Func<string>)delegate
			{
				StringBuilder stringBuilder = new StringBuilder();
				for (int i = 0; i < 4; i++)
				{
					Quadrum quadrum2 = (Quadrum)(byte)i;
					stringBuilder.AppendLine(quadrum2.Label() + " - " + quadrum2.GetSeason(location.y).LabelCap());
				}
				return "DateReadoutTip".Translate(GenDate.DaysPassed, 15, season.LabelCap(), 15, GenDate.Quadrum(GenTicks.TicksAbs, location.x).Label(), stringBuilder.ToString());
			}, 86423));
		}
	}
}
