using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Verse
{
	internal static class EnvironmentInspectDrawer
	{
		private const float StatLabelColumnWidth = 100f;

		private const float ScoreColumnWidth = 50f;

		private const float ScoreStageLabelColumnWidth = 160f;

		private const float DistFromMouse = 26f;

		private const float WindowPadding = 18f;

		private const float LineHeight = 23f;

		private const float SpaceBetweenLines = 2f;

		private const float SpaceBetweenColumns = 35f;

		private static readonly Color RelatedStatColor = new Color(0.85f, 0.85f, 0.85f);

		private static int DisplayedRoomStatsCount
		{
			get
			{
				int num = 0;
				List<RoomStatDef> allDefsListForReading = DefDatabase<RoomStatDef>.AllDefsListForReading;
				for (int i = 0; i < allDefsListForReading.Count; i++)
				{
					if (!allDefsListForReading[i].isHidden || DebugViewSettings.showAllRoomStats)
					{
						num++;
					}
				}
				return num;
			}
		}

		private static bool ShouldShow()
		{
			return Find.PlaySettings.showEnvironment && !Mouse.IsInputBlockedNow && UI.MouseCell().InBounds(Find.VisibleMap) && !UI.MouseCell().Fogged(Find.VisibleMap);
		}

		public static void EnvironmentInspectOnGUI()
		{
			if (Event.current.type != EventType.Repaint || !EnvironmentInspectDrawer.ShouldShow())
			{
				return;
			}
			BeautyDrawer.DrawBeautyAroundMouse();
			EnvironmentInspectDrawer.DrawInfoWindow();
		}

		private static void DrawInfoWindow()
		{
			EnvironmentInspectDrawer.<DrawInfoWindow>c__AnonStorey5DB <DrawInfoWindow>c__AnonStorey5DB = new EnvironmentInspectDrawer.<DrawInfoWindow>c__AnonStorey5DB();
			<DrawInfoWindow>c__AnonStorey5DB.room = UI.MouseCell().GetRoom(Find.VisibleMap, RegionType.Set_All);
			<DrawInfoWindow>c__AnonStorey5DB.roomValid = (<DrawInfoWindow>c__AnonStorey5DB.room != null && <DrawInfoWindow>c__AnonStorey5DB.room.Role != RoomRoleDefOf.None);
			Text.Font = GameFont.Small;
			<DrawInfoWindow>c__AnonStorey5DB.windowRect = new Rect(Event.current.mousePosition.x, Event.current.mousePosition.y, 416f, 36f);
			EnvironmentInspectDrawer.<DrawInfoWindow>c__AnonStorey5DB expr_8A_cp_0 = <DrawInfoWindow>c__AnonStorey5DB;
			expr_8A_cp_0.windowRect.height = expr_8A_cp_0.windowRect.height + 25f;
			if (<DrawInfoWindow>c__AnonStorey5DB.roomValid)
			{
				EnvironmentInspectDrawer.<DrawInfoWindow>c__AnonStorey5DB expr_AC_cp_0 = <DrawInfoWindow>c__AnonStorey5DB;
				expr_AC_cp_0.windowRect.height = expr_AC_cp_0.windowRect.height + 13f;
				EnvironmentInspectDrawer.<DrawInfoWindow>c__AnonStorey5DB expr_C3_cp_0 = <DrawInfoWindow>c__AnonStorey5DB;
				expr_C3_cp_0.windowRect.height = expr_C3_cp_0.windowRect.height + 23f;
				EnvironmentInspectDrawer.<DrawInfoWindow>c__AnonStorey5DB expr_DA_cp_0 = <DrawInfoWindow>c__AnonStorey5DB;
				expr_DA_cp_0.windowRect.height = expr_DA_cp_0.windowRect.height + (float)EnvironmentInspectDrawer.DisplayedRoomStatsCount * 25f;
			}
			EnvironmentInspectDrawer.<DrawInfoWindow>c__AnonStorey5DB expr_F8_cp_0 = <DrawInfoWindow>c__AnonStorey5DB;
			expr_F8_cp_0.windowRect.x = expr_F8_cp_0.windowRect.x + 26f;
			EnvironmentInspectDrawer.<DrawInfoWindow>c__AnonStorey5DB expr_10F_cp_0 = <DrawInfoWindow>c__AnonStorey5DB;
			expr_10F_cp_0.windowRect.y = expr_10F_cp_0.windowRect.y + 26f;
			if (<DrawInfoWindow>c__AnonStorey5DB.windowRect.xMax > (float)UI.screenWidth)
			{
				EnvironmentInspectDrawer.<DrawInfoWindow>c__AnonStorey5DB expr_13C_cp_0 = <DrawInfoWindow>c__AnonStorey5DB;
				expr_13C_cp_0.windowRect.x = expr_13C_cp_0.windowRect.x - (<DrawInfoWindow>c__AnonStorey5DB.windowRect.width + 52f);
			}
			if (<DrawInfoWindow>c__AnonStorey5DB.windowRect.yMax > (float)UI.screenHeight)
			{
				EnvironmentInspectDrawer.<DrawInfoWindow>c__AnonStorey5DB expr_175_cp_0 = <DrawInfoWindow>c__AnonStorey5DB;
				expr_175_cp_0.windowRect.y = expr_175_cp_0.windowRect.y - (<DrawInfoWindow>c__AnonStorey5DB.windowRect.height + 52f);
			}
			Find.WindowStack.ImmediateWindow(74975, <DrawInfoWindow>c__AnonStorey5DB.windowRect, WindowLayer.Super, delegate
			{
				PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.InspectRoomStats, KnowledgeAmount.FrameDisplayed);
				Text.Font = GameFont.Small;
				float num = 18f;
				float beauty = BeautyUtility.AverageBeautyPerceptible(UI.MouseCell(), Find.VisibleMap);
				Rect rect = new Rect(18f, num, <DrawInfoWindow>c__AnonStorey5DB.windowRect.width - 36f, 100f);
				GUI.color = BeautyDrawer.BeautyColor(beauty, 40f);
				Widgets.Label(rect, "BeautyHere".Translate() + ": " + beauty.ToString("F1"));
				num += 25f;
				if (<DrawInfoWindow>c__AnonStorey5DB.roomValid)
				{
					num += 5f;
					GUI.color = new Color(1f, 1f, 1f, 0.4f);
					Widgets.DrawLineHorizontal(18f, num, <DrawInfoWindow>c__AnonStorey5DB.windowRect.width - 36f);
					GUI.color = Color.white;
					num += 8f;
					Rect rect2 = new Rect(18f, num, <DrawInfoWindow>c__AnonStorey5DB.windowRect.width - 36f, 100f);
					GUI.color = Color.white;
					Widgets.Label(rect2, EnvironmentInspectDrawer.GetRoomRoleLabel(<DrawInfoWindow>c__AnonStorey5DB.room));
					num += 25f;
					Text.WordWrap = false;
					for (int i = 0; i < DefDatabase<RoomStatDef>.AllDefsListForReading.Count; i++)
					{
						RoomStatDef roomStatDef = DefDatabase<RoomStatDef>.AllDefsListForReading[i];
						if (!roomStatDef.isHidden || DebugViewSettings.showAllRoomStats)
						{
							float stat = <DrawInfoWindow>c__AnonStorey5DB.room.GetStat(roomStatDef);
							RoomStatScoreStage scoreStage = roomStatDef.GetScoreStage(stat);
							if (<DrawInfoWindow>c__AnonStorey5DB.room.Role.IsStatRelated(roomStatDef))
							{
								GUI.color = EnvironmentInspectDrawer.RelatedStatColor;
							}
							else
							{
								GUI.color = Color.gray;
							}
							Rect rect3 = new Rect(rect2.x, num, 100f, 23f);
							Widgets.Label(rect3, roomStatDef.LabelCap);
							Rect rect4 = new Rect(rect3.xMax + 35f, num, 50f, 23f);
							string label = roomStatDef.ScoreToString(stat);
							Widgets.Label(rect4, label);
							Rect rect5 = new Rect(rect4.xMax + 35f, num, 160f, 23f);
							Widgets.Label(rect5, (scoreStage != null) ? scoreStage.label : string.Empty);
							num += 25f;
						}
					}
					Text.WordWrap = true;
				}
				GUI.color = Color.white;
			}, true, false, 1f);
		}

		public static void DrawRoomOverlays()
		{
			if (!EnvironmentInspectDrawer.ShouldShow())
			{
				return;
			}
			GenUI.RenderMouseoverBracket();
			Room room = UI.MouseCell().GetRoom(Find.VisibleMap, RegionType.Set_All);
			if (room != null && room.Role != RoomRoleDefOf.None)
			{
				room.DrawFieldEdges();
			}
		}

		private static string GetRoomRoleLabel(Room room)
		{
			Pawn pawn = null;
			Pawn pawn2 = null;
			foreach (Pawn current in room.Owners)
			{
				if (pawn == null)
				{
					pawn = current;
				}
				else
				{
					pawn2 = current;
				}
			}
			string result;
			if (pawn == null)
			{
				result = room.Role.LabelCap;
			}
			else if (pawn2 == null)
			{
				result = "SomeonesRoom".Translate(new object[]
				{
					pawn.NameStringShort,
					room.Role.label
				});
			}
			else
			{
				result = "CouplesRoom".Translate(new object[]
				{
					pawn.NameStringShort,
					pawn2.NameStringShort,
					room.Role.label
				});
			}
			return result;
		}
	}
}
