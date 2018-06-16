﻿using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	// Token: 0x02000823 RID: 2083
	[StaticConstructorOnStartup]
	public static class TrainingCardUtility
	{
		// Token: 0x06002E9F RID: 11935 RVA: 0x0018E920 File Offset: 0x0018CD20
		public static void DrawTrainingCard(Rect rect, Pawn pawn)
		{
			Text.Font = GameFont.Small;
			Rect rect2 = new Rect(TrainingCardUtility.TrainabilityLeft, TrainingCardUtility.TrainabilityTop, 30f, 30f);
			TooltipHandler.TipRegion(rect2, "RenameAnimal".Translate());
			if (Widgets.ButtonImage(rect2, TexButton.Rename))
			{
				Find.WindowStack.Add(new Dialog_NamePawn(pawn));
			}
			Listing_Standard listing_Standard = new Listing_Standard();
			listing_Standard.Begin(rect);
			listing_Standard.Label("CreatureTrainability".Translate(new object[]
			{
				pawn.def.label
			}).CapitalizeFirst() + ": " + pawn.RaceProps.trainability.LabelCap, -1f, null);
			Listing_Standard listing_Standard2 = listing_Standard;
			string label = "CreatureWildness".Translate(new object[]
			{
				pawn.def.label
			}).CapitalizeFirst() + ": " + pawn.RaceProps.wildness.ToStringPercent();
			string wildnessExplanation = TrainableUtility.GetWildnessExplanation(pawn.def);
			listing_Standard2.Label(label, -1f, wildnessExplanation);
			if (pawn.training.HasLearned(TrainableDefOf.Obedience))
			{
				Rect rect3 = listing_Standard.GetRect(25f);
				Widgets.Label(rect3, "Master".Translate() + ": ");
				rect3.xMin = rect3.center.x;
				TrainableUtility.MasterSelectButton(rect3, pawn, false);
			}
			listing_Standard.Gap(12f);
			float num = 50f;
			List<TrainableDef> trainableDefsInListOrder = TrainableUtility.TrainableDefsInListOrder;
			for (int i = 0; i < trainableDefsInListOrder.Count; i++)
			{
				if (TrainingCardUtility.TryDrawTrainableRow(listing_Standard.GetRect(28f), pawn, trainableDefsInListOrder[i]))
				{
					num += 28f;
				}
			}
			listing_Standard.End();
		}

		// Token: 0x06002EA0 RID: 11936 RVA: 0x0018EAFC File Offset: 0x0018CEFC
		private static bool TryDrawTrainableRow(Rect rect, Pawn pawn, TrainableDef td)
		{
			bool flag = pawn.training.HasLearned(td);
			bool flag2;
			AcceptanceReport canTrain = pawn.training.CanAssignToTrain(td, out flag2);
			bool result;
			if (!flag2)
			{
				result = false;
			}
			else
			{
				Widgets.DrawHighlightIfMouseover(rect);
				Rect rect2 = rect;
				rect2.width -= 50f;
				rect2.xMin += (float)td.indent * 10f;
				Rect rect3 = rect;
				rect3.xMin = rect3.xMax - 50f + 17f;
				TrainingCardUtility.DoTrainableCheckbox(rect2, pawn, td, canTrain, true, false);
				if (flag)
				{
					GUI.color = Color.green;
				}
				Text.Anchor = TextAnchor.MiddleLeft;
				Widgets.Label(rect3, pawn.training.GetSteps(td) + " / " + td.steps);
				Text.Anchor = TextAnchor.UpperLeft;
				if (DebugSettings.godMode && !pawn.training.HasLearned(td))
				{
					Rect rect4 = rect3;
					rect4.yMin = rect4.yMax - 10f;
					rect4.xMin = rect4.xMax - 10f;
					if (Widgets.ButtonText(rect4, "+", true, false, true))
					{
						pawn.training.Train(td, pawn.Map.mapPawns.FreeColonistsSpawned.RandomElement<Pawn>(), false);
					}
				}
				TrainingCardUtility.DoTrainableTooltip(rect, pawn, td, canTrain);
				GUI.color = Color.white;
				result = true;
			}
			return result;
		}

		// Token: 0x06002EA1 RID: 11937 RVA: 0x0018EC78 File Offset: 0x0018D078
		public static void DoTrainableCheckbox(Rect rect, Pawn pawn, TrainableDef td, AcceptanceReport canTrain, bool drawLabel, bool doTooltip)
		{
			bool flag = pawn.training.HasLearned(td);
			bool wanted = pawn.training.GetWanted(td);
			bool flag2 = wanted;
			Texture2D texChecked = (!flag) ? null : TrainingCardUtility.LearnedTrainingTex;
			Texture2D texUnchecked = (!flag) ? null : TrainingCardUtility.LearnedNotTrainingTex;
			if (drawLabel)
			{
				Widgets.CheckboxLabeled(rect, td.LabelCap, ref wanted, !canTrain.Accepted, texChecked, texUnchecked, false);
			}
			else
			{
				Widgets.Checkbox(rect.position, ref wanted, rect.width, !canTrain.Accepted, true, texChecked, texUnchecked);
			}
			if (wanted != flag2)
			{
				PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.AnimalTraining, KnowledgeAmount.Total);
				pawn.training.SetWantedRecursive(td, wanted);
			}
			if (doTooltip)
			{
				TrainingCardUtility.DoTrainableTooltip(rect, pawn, td, canTrain);
			}
		}

		// Token: 0x06002EA2 RID: 11938 RVA: 0x0018ED44 File Offset: 0x0018D144
		private static void DoTrainableTooltip(Rect rect, Pawn pawn, TrainableDef td, AcceptanceReport canTrain)
		{
			TooltipHandler.TipRegion(rect, delegate()
			{
				string text = td.LabelCap + "\n\n" + td.description;
				if (!canTrain.Accepted)
				{
					text = text + "\n\n" + canTrain.Reason;
				}
				else if (!td.prerequisites.NullOrEmpty<TrainableDef>())
				{
					text += "\n";
					for (int i = 0; i < td.prerequisites.Count; i++)
					{
						if (!pawn.training.HasLearned(td.prerequisites[i]))
						{
							text = text + "\n" + "TrainingNeedsPrerequisite".Translate(new object[]
							{
								td.prerequisites[i].LabelCap
							});
						}
					}
				}
				return text;
			}, (int)(rect.y * 612f + rect.x));
		}

		// Token: 0x040018FC RID: 6396
		public const float RowHeight = 28f;

		// Token: 0x040018FD RID: 6397
		private const float InfoHeaderHeight = 50f;

		// Token: 0x040018FE RID: 6398
		[TweakValue("Interface", -100f, 300f)]
		private static float TrainabilityLeft = 220f;

		// Token: 0x040018FF RID: 6399
		[TweakValue("Interface", -100f, 300f)]
		private static float TrainabilityTop = 0f;

		// Token: 0x04001900 RID: 6400
		private static readonly Texture2D LearnedTrainingTex = ContentFinder<Texture2D>.Get("UI/Icons/FixedCheck", true);

		// Token: 0x04001901 RID: 6401
		private static readonly Texture2D LearnedNotTrainingTex = ContentFinder<Texture2D>.Get("UI/Icons/FixedCheckOff", true);
	}
}
