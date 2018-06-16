﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;

namespace Verse
{
	// Token: 0x02000D03 RID: 3331
	public static class HealthUtility
	{
		// Token: 0x0600492F RID: 18735 RVA: 0x00266A44 File Offset: 0x00264E44
		public static string GetGeneralConditionLabel(Pawn pawn, bool shortVersion = false)
		{
			string result;
			if (pawn.health.Dead)
			{
				result = "Dead".Translate();
			}
			else if (!pawn.health.capacities.CanBeAwake)
			{
				result = "Unconscious".Translate();
			}
			else if (pawn.health.InPainShock)
			{
				result = ((!shortVersion || !"PainShockShort".CanTranslate()) ? "PainShock".Translate() : "PainShockShort".Translate());
			}
			else if (pawn.Downed)
			{
				result = "Incapacitated".Translate();
			}
			else
			{
				bool flag = false;
				for (int i = 0; i < pawn.health.hediffSet.hediffs.Count; i++)
				{
					Hediff_Injury hediff_Injury = pawn.health.hediffSet.hediffs[i] as Hediff_Injury;
					if (hediff_Injury != null)
					{
						if (!hediff_Injury.IsPermanent())
						{
							flag = true;
						}
					}
				}
				if (flag)
				{
					result = "Injured".Translate();
				}
				else if (pawn.health.hediffSet.PainTotal > 0.3f)
				{
					result = "InPain".Translate();
				}
				else
				{
					result = "Healthy".Translate();
				}
			}
			return result;
		}

		// Token: 0x06004930 RID: 18736 RVA: 0x00266BA8 File Offset: 0x00264FA8
		public static Pair<string, Color> GetPartConditionLabel(Pawn pawn, BodyPartRecord part)
		{
			float partHealth = pawn.health.hediffSet.GetPartHealth(part);
			float maxHealth = part.def.GetMaxHealth(pawn);
			float num = partHealth / maxHealth;
			Color second = Color.white;
			string first;
			if (partHealth <= 0f)
			{
				Hediff_MissingPart hediff_MissingPart = null;
				List<Hediff_MissingPart> missingPartsCommonAncestors = pawn.health.hediffSet.GetMissingPartsCommonAncestors();
				for (int i = 0; i < missingPartsCommonAncestors.Count; i++)
				{
					if (missingPartsCommonAncestors[i].Part == part)
					{
						hediff_MissingPart = missingPartsCommonAncestors[i];
						break;
					}
				}
				if (hediff_MissingPart == null)
				{
					bool fresh = false;
					if (hediff_MissingPart != null && hediff_MissingPart.IsFreshNonSolidExtremity)
					{
						fresh = true;
					}
					bool solid = part.def.IsSolid(part, pawn.health.hediffSet.hediffs);
					first = HealthUtility.GetGeneralDestroyedPartLabel(part, fresh, solid);
					second = Color.gray;
				}
				else
				{
					first = hediff_MissingPart.LabelCap;
					second = hediff_MissingPart.LabelColor;
				}
			}
			else if (num < 0.4f)
			{
				first = "SeriouslyImpaired".Translate();
				second = HealthUtility.DarkRedColor;
			}
			else if (num < 0.7f)
			{
				first = "Impaired".Translate();
				second = HealthUtility.ImpairedColor;
			}
			else if (num < 0.999f)
			{
				first = "SlightlyImpaired".Translate();
				second = HealthUtility.SlightlyImpairedColor;
			}
			else
			{
				first = "GoodCondition".Translate();
				second = HealthUtility.GoodConditionColor;
			}
			return new Pair<string, Color>(first, second);
		}

		// Token: 0x06004931 RID: 18737 RVA: 0x00266D4C File Offset: 0x0026514C
		public static string GetGeneralDestroyedPartLabel(BodyPartRecord part, bool fresh, bool solid)
		{
			string result;
			if (part.parent == null)
			{
				result = "SeriouslyImpaired".Translate();
			}
			else if (part.depth == BodyPartDepth.Inside || fresh)
			{
				if (solid)
				{
					result = "ShatteredBodyPart".Translate();
				}
				else
				{
					result = "DestroyedBodyPart".Translate();
				}
			}
			else
			{
				result = "MissingBodyPart".Translate();
			}
			return result;
		}

		// Token: 0x06004932 RID: 18738 RVA: 0x00266DC0 File Offset: 0x002651C0
		private static IEnumerable<BodyPartRecord> HittablePartsViolence(HediffSet bodyModel)
		{
			return from x in bodyModel.GetNotMissingParts(BodyPartHeight.Undefined, BodyPartDepth.Undefined, null)
			where x.depth == BodyPartDepth.Outside || (x.depth == BodyPartDepth.Inside && x.def.IsSolid(x, bodyModel.hediffs))
			select x;
		}

		// Token: 0x06004933 RID: 18739 RVA: 0x00266E01 File Offset: 0x00265201
		public static void GiveInjuriesOperationFailureMinor(Pawn p, BodyPartRecord part)
		{
			HealthUtility.GiveRandomSurgeryInjuries(p, 20, part);
		}

		// Token: 0x06004934 RID: 18740 RVA: 0x00266E0D File Offset: 0x0026520D
		public static void GiveInjuriesOperationFailureCatastrophic(Pawn p, BodyPartRecord part)
		{
			HealthUtility.GiveRandomSurgeryInjuries(p, 65, part);
		}

		// Token: 0x06004935 RID: 18741 RVA: 0x00266E19 File Offset: 0x00265219
		public static void GiveInjuriesOperationFailureRidiculous(Pawn p)
		{
			HealthUtility.GiveRandomSurgeryInjuries(p, 65, null);
		}

		// Token: 0x06004936 RID: 18742 RVA: 0x00266E28 File Offset: 0x00265228
		public static void HealNonPermanentInjuriesAndRestoreLegs(Pawn p)
		{
			if (!p.Dead)
			{
				HealthUtility.tmpHediffs.Clear();
				HealthUtility.tmpHediffs.AddRange(p.health.hediffSet.hediffs);
				for (int i = 0; i < HealthUtility.tmpHediffs.Count; i++)
				{
					Hediff_Injury hediff_Injury = HealthUtility.tmpHediffs[i] as Hediff_Injury;
					if (hediff_Injury != null && !hediff_Injury.IsPermanent())
					{
						p.health.RemoveHediff(hediff_Injury);
					}
					else
					{
						Hediff_MissingPart hediff_MissingPart = HealthUtility.tmpHediffs[i] as Hediff_MissingPart;
						if (hediff_MissingPart != null && hediff_MissingPart.Part.def.tags.Contains(BodyPartTagDefOf.MovingLimbCore) && (hediff_MissingPart.Part.parent == null || p.health.hediffSet.GetNotMissingParts(BodyPartHeight.Undefined, BodyPartDepth.Undefined, null).Contains(hediff_MissingPart.Part.parent)))
						{
							p.health.RestorePart(hediff_MissingPart.Part, null, true);
						}
					}
				}
				HealthUtility.tmpHediffs.Clear();
			}
		}

		// Token: 0x06004937 RID: 18743 RVA: 0x00266F4C File Offset: 0x0026534C
		private static void GiveRandomSurgeryInjuries(Pawn p, int totalDamage, BodyPartRecord operatedPart)
		{
			IEnumerable<BodyPartRecord> source;
			if (operatedPart == null)
			{
				source = from x in p.health.hediffSet.GetNotMissingParts(BodyPartHeight.Undefined, BodyPartDepth.Undefined, null)
				where !x.def.conceptual
				select x;
			}
			else
			{
				source = from x in p.health.hediffSet.GetNotMissingParts(BodyPartHeight.Undefined, BodyPartDepth.Undefined, null)
				where !x.def.conceptual
				select x into pa
				where pa == operatedPart || pa.parent == operatedPart || (operatedPart != null && operatedPart.parent == pa)
				select pa;
			}
			source = from x in source
			where HealthUtility.GetMinHealthOfPartsWeWantToAvoidDestroying(x, p) >= 2f
			select x;
			BodyPartRecord brain = p.health.hediffSet.GetBrain();
			if (brain != null)
			{
				float maxBrainHealth = brain.def.GetMaxHealth(p);
				source = from x in source
				where x != brain || p.health.hediffSet.GetPartHealth(x) >= maxBrainHealth * 0.5f + 1f
				select x;
			}
			while (totalDamage > 0 && source.Any<BodyPartRecord>())
			{
				BodyPartRecord bodyPartRecord = source.RandomElementByWeight((BodyPartRecord x) => x.coverageAbs);
				float partHealth = p.health.hediffSet.GetPartHealth(bodyPartRecord);
				int num = Mathf.Max(3, GenMath.RoundRandom(partHealth * Rand.Range(0.5f, 1f)));
				float minHealthOfPartsWeWantToAvoidDestroying = HealthUtility.GetMinHealthOfPartsWeWantToAvoidDestroying(bodyPartRecord, p);
				if (minHealthOfPartsWeWantToAvoidDestroying - (float)num < 1f)
				{
					num = Mathf.RoundToInt(minHealthOfPartsWeWantToAvoidDestroying - 1f);
				}
				if (bodyPartRecord == brain && partHealth - (float)num < brain.def.GetMaxHealth(p) * 0.5f)
				{
					num = Mathf.Max(Mathf.RoundToInt(partHealth - brain.def.GetMaxHealth(p) * 0.5f), 1);
				}
				if (num <= 0)
				{
					break;
				}
				DamageDef damageDef = Rand.Element<DamageDef>(DamageDefOf.Cut, DamageDefOf.Scratch, DamageDefOf.Stab, DamageDefOf.Crush);
				Thing p2 = p;
				DamageDef def = damageDef;
				float amount = (float)num;
				BodyPartRecord hitPart = bodyPartRecord;
				p2.TakeDamage(new DamageInfo(def, amount, -1f, null, hitPart, null, DamageInfo.SourceCategory.ThingOrUnknown, null));
				totalDamage -= num;
			}
		}

		// Token: 0x06004938 RID: 18744 RVA: 0x002671D8 File Offset: 0x002655D8
		private static float GetMinHealthOfPartsWeWantToAvoidDestroying(BodyPartRecord part, Pawn pawn)
		{
			float num = 999999f;
			while (part != null)
			{
				if (HealthUtility.ShouldRandomSurgeryInjuriesAvoidDestroying(part, pawn))
				{
					num = Mathf.Min(num, pawn.health.hediffSet.GetPartHealth(part));
				}
				part = part.parent;
			}
			return num;
		}

		// Token: 0x06004939 RID: 18745 RVA: 0x00267230 File Offset: 0x00265630
		private static bool ShouldRandomSurgeryInjuriesAvoidDestroying(BodyPartRecord part, Pawn pawn)
		{
			bool result;
			if (part == pawn.RaceProps.body.corePart)
			{
				result = true;
			}
			else if (part.def.tags.Any((BodyPartTagDef x) => x.vital))
			{
				result = true;
			}
			else
			{
				for (int i = 0; i < part.parts.Count; i++)
				{
					if (HealthUtility.ShouldRandomSurgeryInjuriesAvoidDestroying(part.parts[i], pawn))
					{
						return true;
					}
				}
				result = false;
			}
			return result;
		}

		// Token: 0x0600493A RID: 18746 RVA: 0x002672D8 File Offset: 0x002656D8
		public static void DamageUntilDowned(Pawn p)
		{
			if (!p.health.Downed)
			{
				HediffSet hediffSet = p.health.hediffSet;
				p.health.forceIncap = true;
				IEnumerable<BodyPartRecord> source = from x in HealthUtility.HittablePartsViolence(hediffSet)
				where !p.health.hediffSet.hediffs.Any((Hediff y) => y.Part == x && y.CurStage != null && y.CurStage.partEfficiencyOffset < 0f)
				select x;
				int num = 0;
				while (num < 300 && !p.Downed && source.Any<BodyPartRecord>())
				{
					num++;
					BodyPartRecord bodyPartRecord = source.RandomElementByWeight((BodyPartRecord x) => x.coverageAbs);
					int num2 = Mathf.RoundToInt(hediffSet.GetPartHealth(bodyPartRecord)) - 3;
					if (num2 >= 8)
					{
						DamageDef damageDef;
						if (bodyPartRecord.depth == BodyPartDepth.Outside)
						{
							damageDef = HealthUtility.RandomViolenceDamageType();
						}
						else
						{
							damageDef = DamageDefOf.Blunt;
						}
						int num3 = Rand.RangeInclusive(Mathf.RoundToInt((float)num2 * 0.65f), num2);
						HediffDef hediffDefFromDamage = HealthUtility.GetHediffDefFromDamage(damageDef, p, bodyPartRecord);
						if (!p.health.WouldDieAfterAddingHediff(hediffDefFromDamage, bodyPartRecord, (float)num3))
						{
							DamageDef def = damageDef;
							float amount = (float)num3;
							BodyPartRecord hitPart = bodyPartRecord;
							DamageInfo dinfo = new DamageInfo(def, amount, -1f, null, hitPart, null, DamageInfo.SourceCategory.ThingOrUnknown, null);
							dinfo.SetAllowDamagePropagation(false);
							p.TakeDamage(dinfo);
						}
					}
				}
				if (p.Dead)
				{
					StringBuilder stringBuilder = new StringBuilder();
					stringBuilder.AppendLine(p + " died during GiveInjuriesToForceDowned");
					for (int i = 0; i < p.health.hediffSet.hediffs.Count; i++)
					{
						stringBuilder.AppendLine("   -" + p.health.hediffSet.hediffs[i].ToString());
					}
					Log.Error(stringBuilder.ToString(), false);
				}
				p.health.forceIncap = false;
			}
		}

		// Token: 0x0600493B RID: 18747 RVA: 0x00267514 File Offset: 0x00265914
		public static void DamageUntilDead(Pawn p)
		{
			HediffSet hediffSet = p.health.hediffSet;
			int num = 0;
			while (!p.Dead && num < 200 && HealthUtility.HittablePartsViolence(hediffSet).Any<BodyPartRecord>())
			{
				num++;
				BodyPartRecord bodyPartRecord = HealthUtility.HittablePartsViolence(hediffSet).RandomElementByWeight((BodyPartRecord x) => x.coverageAbs);
				int num2 = Rand.RangeInclusive(8, 25);
				DamageDef damageDef;
				if (bodyPartRecord.depth == BodyPartDepth.Outside)
				{
					damageDef = HealthUtility.RandomViolenceDamageType();
				}
				else
				{
					damageDef = DamageDefOf.Blunt;
				}
				DamageDef def = damageDef;
				float amount = (float)num2;
				BodyPartRecord hitPart = bodyPartRecord;
				DamageInfo dinfo = new DamageInfo(def, amount, -1f, null, hitPart, null, DamageInfo.SourceCategory.ThingOrUnknown, null);
				p.TakeDamage(dinfo);
			}
			if (!p.Dead)
			{
				Log.Error(p + " not killed during GiveInjuriesToKill", false);
			}
		}

		// Token: 0x0600493C RID: 18748 RVA: 0x002675F8 File Offset: 0x002659F8
		public static void DamageLegsUntilIncapableOfMoving(Pawn p)
		{
			HediffDef def = Rand.Element<HediffDef>(HediffDefOf.Scratch, HediffDefOf.Bruise, HediffDefOf.Bite, HediffDefOf.Cut);
			int num = 0;
			while (p.health.capacities.CapableOf(PawnCapacityDefOf.Moving) && num < 300)
			{
				num++;
				IEnumerable<BodyPartRecord> source = from x in p.health.hediffSet.GetNotMissingParts(BodyPartHeight.Undefined, BodyPartDepth.Undefined, null)
				where x.def.tags.Contains(BodyPartTagDefOf.MovingLimbCore) && p.health.hediffSet.GetPartHealth(x) >= 2f
				select x;
				if (!source.Any<BodyPartRecord>())
				{
					break;
				}
				BodyPartRecord bodyPartRecord = source.RandomElement<BodyPartRecord>();
				float maxHealth = bodyPartRecord.def.GetMaxHealth(p);
				float partHealth = p.health.hediffSet.GetPartHealth(bodyPartRecord);
				int min = Mathf.Clamp(Mathf.RoundToInt(maxHealth * 0.12f), 1, (int)partHealth - 1);
				int max = Mathf.Clamp(Mathf.RoundToInt(maxHealth * 0.27f), 1, (int)partHealth - 1);
				int num2 = Rand.RangeInclusive(min, max);
				if (p.health.WouldDieAfterAddingHediff(def, bodyPartRecord, (float)num2))
				{
					break;
				}
				Hediff_Injury hediff_Injury = (Hediff_Injury)HediffMaker.MakeHediff(def, p, bodyPartRecord);
				hediff_Injury.Severity = (float)num2;
				p.health.AddHediff(hediff_Injury, null, null, null);
			}
		}

		// Token: 0x0600493D RID: 18749 RVA: 0x00267770 File Offset: 0x00265B70
		public static DamageDef RandomViolenceDamageType()
		{
			DamageDef result;
			switch (Rand.RangeInclusive(0, 4))
			{
			case 0:
				result = DamageDefOf.Bullet;
				break;
			case 1:
				result = DamageDefOf.Blunt;
				break;
			case 2:
				result = DamageDefOf.Stab;
				break;
			case 3:
				result = DamageDefOf.Scratch;
				break;
			case 4:
				result = DamageDefOf.Cut;
				break;
			default:
				result = null;
				break;
			}
			return result;
		}

		// Token: 0x0600493E RID: 18750 RVA: 0x002677E4 File Offset: 0x00265BE4
		public static HediffDef GetHediffDefFromDamage(DamageDef dam, Pawn pawn, BodyPartRecord part)
		{
			HediffDef result = dam.hediff;
			if (part.def.IsSkinCovered(part, pawn.health.hediffSet) && dam.hediffSkin != null)
			{
				result = dam.hediffSkin;
			}
			if (part.def.IsSolid(part, pawn.health.hediffSet.hediffs) && dam.hediffSolid != null)
			{
				result = dam.hediffSolid;
			}
			return result;
		}

		// Token: 0x0600493F RID: 18751 RVA: 0x00267864 File Offset: 0x00265C64
		public static bool TryAnesthetize(Pawn pawn)
		{
			bool result;
			if (!pawn.RaceProps.IsFlesh)
			{
				result = false;
			}
			else
			{
				pawn.health.forceIncap = true;
				pawn.health.AddHediff(HediffDefOf.Anesthetic, null, null, null);
				pawn.health.forceIncap = false;
				result = true;
			}
			return result;
		}

		// Token: 0x06004940 RID: 18752 RVA: 0x002678C8 File Offset: 0x00265CC8
		public static void AdjustSeverity(Pawn pawn, HediffDef hdDef, float sevOffset)
		{
			if (sevOffset != 0f)
			{
				Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(hdDef, false);
				if (hediff != null)
				{
					hediff.Severity += sevOffset;
				}
				else if (sevOffset > 0f)
				{
					hediff = HediffMaker.MakeHediff(hdDef, pawn, null);
					hediff.Severity = sevOffset;
					pawn.health.AddHediff(hediff, null, null, null);
				}
			}
		}

		// Token: 0x06004941 RID: 18753 RVA: 0x0026794C File Offset: 0x00265D4C
		public static BodyPartRemovalIntent PartRemovalIntent(Pawn pawn, BodyPartRecord part)
		{
			BodyPartRemovalIntent result;
			if (pawn.health.hediffSet.hediffs.Any((Hediff d) => d.Visible && d.Part == part && d.def.isBad))
			{
				result = BodyPartRemovalIntent.Amputate;
			}
			else
			{
				result = BodyPartRemovalIntent.Harvest;
			}
			return result;
		}

		// Token: 0x06004942 RID: 18754 RVA: 0x0026799C File Offset: 0x00265D9C
		public static int TicksUntilDeathDueToBloodLoss(Pawn pawn)
		{
			Hediff firstHediffOfDef = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.BloodLoss, false);
			int result;
			if (firstHediffOfDef == null)
			{
				result = int.MaxValue;
			}
			else
			{
				result = (int)((1f - firstHediffOfDef.Severity) / pawn.health.hediffSet.BleedRateTotal * 60000f);
			}
			return result;
		}

		// Token: 0x040031C4 RID: 12740
		public static readonly Color GoodConditionColor = new Color(0.6f, 0.8f, 0.65f);

		// Token: 0x040031C5 RID: 12741
		public static readonly Color DarkRedColor = new Color(0.73f, 0.02f, 0.02f);

		// Token: 0x040031C6 RID: 12742
		public static readonly Color ImpairedColor = new Color(0.9f, 0.7f, 0f);

		// Token: 0x040031C7 RID: 12743
		public static readonly Color SlightlyImpairedColor = new Color(0.9f, 0.9f, 0f);

		// Token: 0x040031C8 RID: 12744
		private static List<Hediff> tmpHediffs = new List<Hediff>();
	}
}
