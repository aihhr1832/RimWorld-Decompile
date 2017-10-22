using System.Collections.Generic;
using System.Text;
using Verse;

namespace RimWorld
{
	public class StatWorker_MeleeDPS : StatWorker
	{
		public override float GetValueUnfinalized(StatRequest req, bool applyPostProcess = true)
		{
			return this.GetMeleeDamage(req, applyPostProcess) * this.GetMeleeHitChance(req, applyPostProcess) / this.GetMeleeCooldown(req, applyPostProcess);
		}

		public override string GetExplanation(StatRequest req, ToStringNumberSense numberSense)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("StatsReport_MeleeDPSExplanation".Translate());
			stringBuilder.AppendLine();
			stringBuilder.AppendLine("StatsReport_MeleeDamage".Translate() + " (" + "AverageOfAllAttacks".Translate() + ")");
			stringBuilder.AppendLine("  " + this.GetMeleeDamage(req, true).ToString("0.##"));
			stringBuilder.AppendLine();
			stringBuilder.AppendLine("StatsReport_Cooldown".Translate() + " (" + "AverageOfAllAttacks".Translate() + ")");
			stringBuilder.AppendLine("  " + "StatsReport_CooldownFormat".Translate(this.GetMeleeCooldown(req, true).ToString("0.##")));
			stringBuilder.AppendLine();
			stringBuilder.AppendLine("StatsReport_MeleeHitChance".Translate());
			stringBuilder.AppendLine();
			stringBuilder.Append(this.GetMeleeHitChanceExplanation(req));
			return stringBuilder.ToString();
		}

		public override string GetStatDrawEntryLabel(StatDef stat, float value, ToStringNumberSense numberSense, StatRequest optionalReq)
		{
			return string.Format("{0} ( {1} x {2} / {3} )", value.ToStringByStyle(stat.toStringStyle, numberSense), this.GetMeleeDamage(optionalReq, true).ToString("0.##"), StatDefOf.MeleeHitChance.ValueToString(this.GetMeleeHitChance(optionalReq, true), ToStringNumberSense.Absolute), this.GetMeleeCooldown(optionalReq, true).ToString("0.##"));
		}

		private float GetMeleeDamage(StatRequest req, bool applyPostProcess = true)
		{
			Pawn pawn = req.Thing as Pawn;
			if (pawn != null)
			{
				List<VerbEntry> updatedAvailableVerbsList = pawn.meleeVerbs.GetUpdatedAvailableVerbsList();
				if (updatedAvailableVerbsList.Count == 0)
				{
					return 0f;
				}
				float num = 0f;
				for (int i = 0; i < updatedAvailableVerbsList.Count; i++)
				{
					num += updatedAvailableVerbsList[i].SelectionWeight;
				}
				float num2 = 0f;
				for (int j = 0; j < updatedAvailableVerbsList.Count; j++)
				{
					VerbEntry verbEntry = updatedAvailableVerbsList[j];
					ThingWithComps ownerEquipment = verbEntry.verb.ownerEquipment;
					float num3 = num2;
					float num4 = updatedAvailableVerbsList[j].SelectionWeight / num;
					VerbEntry verbEntry2 = updatedAvailableVerbsList[j];
					VerbProperties verbProps = verbEntry2.verb.verbProps;
					VerbEntry verbEntry3 = updatedAvailableVerbsList[j];
					num2 = num3 + num4 * (float)verbProps.AdjustedMeleeDamageAmount(verbEntry3.verb, pawn, ownerEquipment);
				}
				return num2;
			}
			return 0f;
		}

		private float GetMeleeHitChance(StatRequest req, bool applyPostProcess = true)
		{
			if (req.HasThing)
			{
				return req.Thing.GetStatValue(StatDefOf.MeleeHitChance, applyPostProcess);
			}
			return req.Def.GetStatValueAbstract(StatDefOf.MeleeHitChance, null);
		}

		private float GetMeleeCooldown(StatRequest req, bool applyPostProcess = true)
		{
			Pawn pawn = req.Thing as Pawn;
			if (pawn != null)
			{
				List<VerbEntry> updatedAvailableVerbsList = pawn.meleeVerbs.GetUpdatedAvailableVerbsList();
				if (updatedAvailableVerbsList.Count == 0)
				{
					return 1f;
				}
				float num = 0f;
				for (int i = 0; i < updatedAvailableVerbsList.Count; i++)
				{
					num += updatedAvailableVerbsList[i].SelectionWeight;
				}
				float num2 = 0f;
				for (int j = 0; j < updatedAvailableVerbsList.Count; j++)
				{
					VerbEntry verbEntry = updatedAvailableVerbsList[j];
					ThingWithComps ownerEquipment = verbEntry.verb.ownerEquipment;
					float num3 = num2;
					float num4 = updatedAvailableVerbsList[j].SelectionWeight / num;
					VerbEntry verbEntry2 = updatedAvailableVerbsList[j];
					num2 = num3 + num4 * (float)verbEntry2.verb.verbProps.AdjustedCooldownTicks(ownerEquipment);
				}
				return (float)(num2 / 60.0);
			}
			return 1f;
		}

		private string GetMeleeHitChanceExplanation(StatRequest req)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine(StatDefOf.MeleeHitChance.Worker.GetExplanation(req, StatDefOf.MeleeHitChance.toStringNumberSense));
			StatDefOf.MeleeHitChance.Worker.FinalizeExplanation(stringBuilder, req, StatDefOf.MeleeHitChance.toStringNumberSense, this.GetMeleeHitChance(req, true));
			StringBuilder stringBuilder2 = new StringBuilder();
			string[] array = stringBuilder.ToString().Split('\n');
			for (int i = 0; i < array.Length; i++)
			{
				stringBuilder2.Append("  ");
				stringBuilder2.AppendLine(array[i]);
			}
			return stringBuilder2.ToString();
		}
	}
}
