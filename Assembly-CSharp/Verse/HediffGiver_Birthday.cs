﻿using System;
using System.Collections.Generic;
using UnityEngine;

namespace Verse
{
	public class HediffGiver_Birthday : HediffGiver
	{
		public SimpleCurve ageFractionChanceCurve = null;

		public float averageSeverityPerDayBeforeGeneration = 0f;

		private static List<Hediff> addedHediffs = new List<Hediff>();

		public HediffGiver_Birthday()
		{
		}

		public void TryApplyAndSimulateSeverityChange(Pawn pawn, float gotAtAge, bool tryNotToKillPawn)
		{
			HediffGiver_Birthday.addedHediffs.Clear();
			if (base.TryApply(pawn, HediffGiver_Birthday.addedHediffs))
			{
				if (this.averageSeverityPerDayBeforeGeneration != 0f)
				{
					float num = (pawn.ageTracker.AgeBiologicalYearsFloat - gotAtAge) * 60f;
					if (num < 0f)
					{
						Log.Error(string.Concat(new object[]
						{
							"daysPassed < 0, pawn=",
							pawn,
							", gotAtAge=",
							gotAtAge
						}), false);
						return;
					}
					for (int i = 0; i < HediffGiver_Birthday.addedHediffs.Count; i++)
					{
						this.SimulateSeverityChange(pawn, HediffGiver_Birthday.addedHediffs[i], num, tryNotToKillPawn);
					}
				}
				HediffGiver_Birthday.addedHediffs.Clear();
			}
		}

		private void SimulateSeverityChange(Pawn pawn, Hediff hediff, float daysPassed, bool tryNotToKillPawn)
		{
			float num = this.averageSeverityPerDayBeforeGeneration * daysPassed;
			num *= Rand.Range(0.5f, 1.4f);
			num += hediff.def.initialSeverity;
			if (tryNotToKillPawn)
			{
				this.AvoidLifeThreateningStages(ref num, hediff.def.stages);
			}
			hediff.Severity = num;
			pawn.health.Notify_HediffChanged(hediff);
		}

		private void AvoidLifeThreateningStages(ref float severity, List<HediffStage> stages)
		{
			if (!stages.NullOrEmpty<HediffStage>())
			{
				int num = -1;
				for (int i = 0; i < stages.Count; i++)
				{
					if (stages[i].lifeThreatening)
					{
						num = i;
						break;
					}
				}
				if (num >= 0)
				{
					if (num == 0)
					{
						severity = Mathf.Min(severity, stages[num].minSeverity);
					}
					else
					{
						severity = Mathf.Min(severity, (stages[num].minSeverity + stages[num - 1].minSeverity) / 2f);
					}
				}
			}
		}

		public float DebugChanceToHaveAtAge(Pawn pawn, int age)
		{
			float num = 1f;
			for (int i = 1; i <= age; i++)
			{
				float x = (float)i / pawn.RaceProps.lifeExpectancy;
				num *= 1f - this.ageFractionChanceCurve.Evaluate(x);
			}
			return 1f - num;
		}

		// Note: this type is marked as 'beforefieldinit'.
		static HediffGiver_Birthday()
		{
		}
	}
}
