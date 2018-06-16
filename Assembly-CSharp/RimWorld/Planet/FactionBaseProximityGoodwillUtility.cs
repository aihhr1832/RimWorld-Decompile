﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	// Token: 0x02000559 RID: 1369
	public static class FactionBaseProximityGoodwillUtility
	{
		// Token: 0x1700039E RID: 926
		// (get) Token: 0x060019A5 RID: 6565 RVA: 0x000DE7CC File Offset: 0x000DCBCC
		public static int MaxDist
		{
			get
			{
				return Mathf.RoundToInt(DiplomacyTuning.Goodwill_PerQuadrumFromSettlementProximity.Last<CurvePoint>().x);
			}
		}

		// Token: 0x060019A6 RID: 6566 RVA: 0x000DE7F8 File Offset: 0x000DCBF8
		public static void CheckFactionBaseProximityGoodwillChange()
		{
			if (Find.TickManager.TicksGame != 0 && Find.TickManager.TicksGame % 900000 == 0)
			{
				List<FactionBase> factionBases = Find.WorldObjects.FactionBases;
				FactionBaseProximityGoodwillUtility.tmpGoodwillOffsets.Clear();
				for (int i = 0; i < factionBases.Count; i++)
				{
					FactionBase factionBase = factionBases[i];
					if (factionBase.Faction == Faction.OfPlayer)
					{
						FactionBaseProximityGoodwillUtility.AppendProximityGoodwillOffsets(factionBase.Tile, FactionBaseProximityGoodwillUtility.tmpGoodwillOffsets, true);
					}
				}
				if (FactionBaseProximityGoodwillUtility.tmpGoodwillOffsets.Any<Pair<FactionBase, int>>())
				{
					FactionBaseProximityGoodwillUtility.SortProximityGoodwillOffsets(FactionBaseProximityGoodwillUtility.tmpGoodwillOffsets);
					List<Faction> allFactionsListForReading = Find.FactionManager.AllFactionsListForReading;
					bool flag = false;
					string text = "LetterFactionBaseProximity".Translate() + "\n\n" + FactionBaseProximityGoodwillUtility.ProximityGoodwillOffsetsToString(FactionBaseProximityGoodwillUtility.tmpGoodwillOffsets);
					for (int j = 0; j < allFactionsListForReading.Count; j++)
					{
						Faction faction = allFactionsListForReading[j];
						if (faction != Faction.OfPlayer)
						{
							int num = 0;
							for (int k = 0; k < FactionBaseProximityGoodwillUtility.tmpGoodwillOffsets.Count; k++)
							{
								if (FactionBaseProximityGoodwillUtility.tmpGoodwillOffsets[k].First.Faction == faction)
								{
									num += FactionBaseProximityGoodwillUtility.tmpGoodwillOffsets[k].Second;
								}
							}
							FactionRelationKind playerRelationKind = faction.PlayerRelationKind;
							if (faction.TryAffectGoodwillWith(Faction.OfPlayer, num, false, false, null, null))
							{
								flag = true;
								faction.TryAppendRelationKindChangedInfo(ref text, playerRelationKind, faction.PlayerRelationKind, null);
							}
						}
					}
					if (flag)
					{
						Find.LetterStack.ReceiveLetter("LetterLabelFactionBaseProximity".Translate(), text, LetterDefOf.NegativeEvent, null);
					}
				}
			}
		}

		// Token: 0x060019A7 RID: 6567 RVA: 0x000DE9D4 File Offset: 0x000DCDD4
		public static void AppendProximityGoodwillOffsets(int tile, List<Pair<FactionBase, int>> outOffsets, bool ignoreIfAlreadyMinGoodwill)
		{
			int maxDist = FactionBaseProximityGoodwillUtility.MaxDist;
			List<FactionBase> factionBases = Find.WorldObjects.FactionBases;
			for (int i = 0; i < factionBases.Count; i++)
			{
				FactionBase factionBase = factionBases[i];
				if (factionBase.Faction != Faction.OfPlayer)
				{
					if (!ignoreIfAlreadyMinGoodwill || factionBase.Faction.PlayerGoodwill != -100)
					{
						int num = Find.WorldGrid.TraversalDistanceBetween(tile, factionBase.Tile, false, maxDist);
						if (num != 2147483647)
						{
							int num2 = Mathf.RoundToInt(DiplomacyTuning.Goodwill_PerQuadrumFromSettlementProximity.Evaluate((float)num));
							if (num2 != 0)
							{
								outOffsets.Add(new Pair<FactionBase, int>(factionBase, num2));
							}
						}
					}
				}
			}
		}

		// Token: 0x060019A8 RID: 6568 RVA: 0x000DEA94 File Offset: 0x000DCE94
		public static void SortProximityGoodwillOffsets(List<Pair<FactionBase, int>> offsets)
		{
			offsets.SortBy((Pair<FactionBase, int> x) => x.First.Faction.loadID, (Pair<FactionBase, int> x) => -Mathf.Abs(x.Second));
		}

		// Token: 0x060019A9 RID: 6569 RVA: 0x000DEAE4 File Offset: 0x000DCEE4
		public static string ProximityGoodwillOffsetsToString(List<Pair<FactionBase, int>> offsets)
		{
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < offsets.Count; i++)
			{
				if (stringBuilder.Length > 0)
				{
					stringBuilder.AppendLine();
				}
				stringBuilder.Append("  - " + offsets[i].First.LabelCap + ": " + "ProximitySingleGoodwillChange".Translate(new object[]
				{
					offsets[i].Second.ToStringWithSign(),
					offsets[i].First.Faction.Name
				}));
			}
			return stringBuilder.ToString();
		}

		// Token: 0x060019AA RID: 6570 RVA: 0x000DEBA4 File Offset: 0x000DCFA4
		public static void CheckConfirmSettle(int tile, Action settleAction)
		{
			FactionBaseProximityGoodwillUtility.tmpGoodwillOffsets.Clear();
			FactionBaseProximityGoodwillUtility.AppendProximityGoodwillOffsets(tile, FactionBaseProximityGoodwillUtility.tmpGoodwillOffsets, false);
			if (FactionBaseProximityGoodwillUtility.tmpGoodwillOffsets.Any<Pair<FactionBase, int>>())
			{
				FactionBaseProximityGoodwillUtility.SortProximityGoodwillOffsets(FactionBaseProximityGoodwillUtility.tmpGoodwillOffsets);
				Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("ConfirmSettleNearFactionBase".Translate(new object[]
				{
					FactionBaseProximityGoodwillUtility.MaxDist - 1,
					15
				}) + "\n\n" + FactionBaseProximityGoodwillUtility.ProximityGoodwillOffsetsToString(FactionBaseProximityGoodwillUtility.tmpGoodwillOffsets), settleAction, false, null));
			}
			else
			{
				settleAction();
			}
		}

		// Token: 0x04000F18 RID: 3864
		private static List<Pair<FactionBase, int>> tmpGoodwillOffsets = new List<Pair<FactionBase, int>>();
	}
}
