﻿using System;
using System.Collections.Generic;
using System.Text;
using Verse;

namespace RimWorld
{
	public class StatWorker_MeleeDPS : StatWorker
	{
		public StatWorker_MeleeDPS()
		{
		}

		public override bool IsDisabledFor(Thing thing)
		{
			return base.IsDisabledFor(thing) || StatDefOf.MeleeHitChance.Worker.IsDisabledFor(thing);
		}

		public override float GetValueUnfinalized(StatRequest req, bool applyPostProcess = true)
		{
			if (req.Thing == null)
			{
				Log.Error("Getting MeleeDPS stat for " + req.Def + " without concrete pawn. This always returns 0.", false);
			}
			return this.GetMeleeDamage(req, applyPostProcess) * this.GetMeleeHitChance(req, applyPostProcess) / this.GetMeleeCooldown(req, applyPostProcess);
		}

		public override string GetExplanationUnfinalized(StatRequest req, ToStringNumberSense numberSense)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("StatsReport_MeleeDPSExplanation".Translate());
			stringBuilder.AppendLine();
			stringBuilder.AppendLine("StatsReport_MeleeDamage".Translate() + " (" + "AverageOfAllAttacks".Translate() + ")");
			stringBuilder.AppendLine("  " + this.GetMeleeDamage(req, true).ToString("0.##"));
			stringBuilder.AppendLine();
			stringBuilder.AppendLine("StatsReport_Cooldown".Translate() + " (" + "AverageOfAllAttacks".Translate() + ")");
			stringBuilder.AppendLine("  " + "StatsReport_CooldownFormat".Translate(new object[]
			{
				this.GetMeleeCooldown(req, true).ToString("0.##")
			}));
			stringBuilder.AppendLine();
			stringBuilder.AppendLine("StatsReport_MeleeHitChance".Translate());
			stringBuilder.AppendLine();
			stringBuilder.AppendLine(StatDefOf.MeleeHitChance.Worker.GetExplanationUnfinalized(req, StatDefOf.MeleeHitChance.toStringNumberSense).TrimEndNewlines().Indented("    "));
			stringBuilder.AppendLine();
			stringBuilder.Append(StatDefOf.MeleeHitChance.Worker.GetExplanationFinalizePart(req, StatDefOf.MeleeHitChance.toStringNumberSense, this.GetMeleeHitChance(req, true)).Indented("    "));
			return stringBuilder.ToString();
		}

		public override string GetStatDrawEntryLabel(StatDef stat, float value, ToStringNumberSense numberSense, StatRequest optionalReq)
		{
			return string.Format("{0} ( {1} x {2} / {3} )", new object[]
			{
				value.ToStringByStyle(stat.toStringStyle, numberSense),
				this.GetMeleeDamage(optionalReq, true).ToString("0.##"),
				StatDefOf.MeleeHitChance.ValueToString(this.GetMeleeHitChance(optionalReq, true), ToStringNumberSense.Absolute),
				this.GetMeleeCooldown(optionalReq, true).ToString("0.##")
			});
		}

		private float GetMeleeDamage(StatRequest req, bool applyPostProcess = true)
		{
			Pawn pawn = req.Thing as Pawn;
			float result;
			if (pawn == null)
			{
				result = 0f;
			}
			else
			{
				List<VerbEntry> updatedAvailableVerbsList = pawn.meleeVerbs.GetUpdatedAvailableVerbsList();
				if (updatedAvailableVerbsList.Count == 0)
				{
					result = 0f;
				}
				else
				{
					float num = 0f;
					for (int i = 0; i < updatedAvailableVerbsList.Count; i++)
					{
						if (updatedAvailableVerbsList[i].IsMeleeAttack)
						{
							num += updatedAvailableVerbsList[i].GetSelectionWeight(null);
						}
					}
					float num2 = 0f;
					for (int j = 0; j < updatedAvailableVerbsList.Count; j++)
					{
						if (updatedAvailableVerbsList[j].IsMeleeAttack)
						{
							ThingWithComps ownerEquipment = updatedAvailableVerbsList[j].verb.ownerEquipment;
							num2 += updatedAvailableVerbsList[j].GetSelectionWeight(null) / num * updatedAvailableVerbsList[j].verb.verbProps.AdjustedMeleeDamageAmount(updatedAvailableVerbsList[j].verb, pawn, ownerEquipment);
						}
					}
					result = num2;
				}
			}
			return result;
		}

		private float GetMeleeHitChance(StatRequest req, bool applyPostProcess = true)
		{
			float result;
			if (req.HasThing)
			{
				result = req.Thing.GetStatValue(StatDefOf.MeleeHitChance, applyPostProcess);
			}
			else
			{
				result = req.Def.GetStatValueAbstract(StatDefOf.MeleeHitChance, null);
			}
			return result;
		}

		private float GetMeleeCooldown(StatRequest req, bool applyPostProcess = true)
		{
			Pawn pawn = req.Thing as Pawn;
			float result;
			if (pawn == null)
			{
				result = 1f;
			}
			else
			{
				List<VerbEntry> updatedAvailableVerbsList = pawn.meleeVerbs.GetUpdatedAvailableVerbsList();
				if (updatedAvailableVerbsList.Count == 0)
				{
					result = 1f;
				}
				else
				{
					float num = 0f;
					for (int i = 0; i < updatedAvailableVerbsList.Count; i++)
					{
						if (updatedAvailableVerbsList[i].IsMeleeAttack)
						{
							num += updatedAvailableVerbsList[i].GetSelectionWeight(null);
						}
					}
					float num2 = 0f;
					for (int j = 0; j < updatedAvailableVerbsList.Count; j++)
					{
						if (updatedAvailableVerbsList[j].IsMeleeAttack)
						{
							ThingWithComps ownerEquipment = updatedAvailableVerbsList[j].verb.ownerEquipment;
							num2 += updatedAvailableVerbsList[j].GetSelectionWeight(null) / num * (float)updatedAvailableVerbsList[j].verb.verbProps.AdjustedCooldownTicks(updatedAvailableVerbsList[j].verb, pawn, ownerEquipment);
						}
					}
					result = num2 / 60f;
				}
			}
			return result;
		}
	}
}
