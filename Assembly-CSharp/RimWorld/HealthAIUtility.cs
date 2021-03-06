﻿using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public static class HealthAIUtility
	{
		[CompilerGenerated]
		private static Func<Thing, float> <>f__am$cache0;

		public static bool ShouldSeekMedicalRestUrgent(Pawn pawn)
		{
			return pawn.Downed || pawn.health.HasHediffsNeedingTend(false) || HealthAIUtility.ShouldHaveSurgeryDoneNow(pawn);
		}

		public static bool ShouldSeekMedicalRest(Pawn pawn)
		{
			return HealthAIUtility.ShouldSeekMedicalRestUrgent(pawn) || pawn.health.hediffSet.HasTendedAndHealingInjury() || pawn.health.hediffSet.HasTendedImmunizableNotImmuneHediff();
		}

		public static bool ShouldBeTendedNowByPlayerUrgent(Pawn pawn)
		{
			return HealthAIUtility.ShouldBeTendedNowByPlayer(pawn) && HealthUtility.TicksUntilDeathDueToBloodLoss(pawn) < 45000;
		}

		public static bool ShouldBeTendedNowByPlayer(Pawn pawn)
		{
			return pawn.playerSettings != null && HealthAIUtility.ShouldEverReceiveMedicalCareFromPlayer(pawn) && pawn.health.HasHediffsNeedingTendByPlayer(false);
		}

		public static bool ShouldEverReceiveMedicalCareFromPlayer(Pawn pawn)
		{
			return (pawn.playerSettings == null || pawn.playerSettings.medCare != MedicalCareCategory.NoCare) && (pawn.guest == null || pawn.guest.interactionMode != PrisonerInteractionModeDefOf.Execution) && pawn.Map.designationManager.DesignationOn(pawn, DesignationDefOf.Slaughter) == null;
		}

		public static bool ShouldHaveSurgeryDoneNow(Pawn pawn)
		{
			return pawn.health.surgeryBills.AnyShouldDoNow;
		}

		public static Thing FindBestMedicine(Pawn healer, Pawn patient)
		{
			Thing result;
			if (patient.playerSettings == null || patient.playerSettings.medCare <= MedicalCareCategory.NoMeds)
			{
				result = null;
			}
			else if (Medicine.GetMedicineCountToFullyHeal(patient) <= 0)
			{
				result = null;
			}
			else
			{
				Predicate<Thing> predicate = (Thing m) => !m.IsForbidden(healer) && patient.playerSettings.medCare.AllowsMedicine(m.def) && healer.CanReserve(m, 10, 1, null, false);
				Func<Thing, float> priorityGetter = (Thing t) => t.def.GetStatValueAbstract(StatDefOf.MedicalPotency, null);
				IntVec3 position = patient.Position;
				Map map = patient.Map;
				List<Thing> searchSet = patient.Map.listerThings.ThingsInGroup(ThingRequestGroup.Medicine);
				PathEndMode peMode = PathEndMode.ClosestTouch;
				TraverseParms traverseParams = TraverseParms.For(healer, Danger.Deadly, TraverseMode.ByPawn, false);
				Predicate<Thing> validator = predicate;
				result = GenClosest.ClosestThing_Global_Reachable(position, map, searchSet, peMode, traverseParams, 9999f, validator, priorityGetter);
			}
			return result;
		}

		[CompilerGenerated]
		private static float <FindBestMedicine>m__0(Thing t)
		{
			return t.def.GetStatValueAbstract(StatDefOf.MedicalPotency, null);
		}

		[CompilerGenerated]
		private sealed class <FindBestMedicine>c__AnonStorey0
		{
			internal Pawn healer;

			internal Pawn patient;

			public <FindBestMedicine>c__AnonStorey0()
			{
			}

			internal bool <>m__0(Thing m)
			{
				return !m.IsForbidden(this.healer) && this.patient.playerSettings.medCare.AllowsMedicine(m.def) && this.healer.CanReserve(m, 10, 1, null, false);
			}
		}
	}
}
