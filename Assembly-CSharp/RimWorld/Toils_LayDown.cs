﻿using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public static class Toils_LayDown
	{
		private const int TicksBetweenSleepZs = 100;

		private const float GroundRestEffectiveness = 0.8f;

		private const int GetUpOrStartJobWhileInBedCheckInterval = 211;

		public static Toil LayDown(TargetIndex bedOrRestSpotIndex, bool hasBed, bool lookForOtherJobs, bool canSleep = true, bool gainRestAndHealth = true)
		{
			Toil layDown = new Toil();
			layDown.initAction = delegate()
			{
				Pawn actor = layDown.actor;
				actor.pather.StopDead();
				JobDriver curDriver = actor.jobs.curDriver;
				if (hasBed)
				{
					Building_Bed t = (Building_Bed)actor.CurJob.GetTarget(bedOrRestSpotIndex).Thing;
					if (!t.OccupiedRect().Contains(actor.Position))
					{
						Log.Error("Can't start LayDown toil because pawn is not in the bed. pawn=" + actor, false);
						actor.jobs.EndCurrentJob(JobCondition.Errored, true);
						return;
					}
					actor.jobs.posture = PawnPosture.LayingInBed;
				}
				else
				{
					actor.jobs.posture = PawnPosture.LayingOnGroundNormal;
				}
				curDriver.asleep = false;
				if (actor.mindState.applyBedThoughtsTick == 0)
				{
					actor.mindState.applyBedThoughtsTick = Find.TickManager.TicksGame + Rand.Range(2500, 10000);
					actor.mindState.applyBedThoughtsOnLeave = false;
				}
				if (actor.ownership != null && actor.CurrentBed() != actor.ownership.OwnedBed)
				{
					ThoughtUtility.RemovePositiveBedroomThoughts(actor);
				}
			};
			layDown.tickAction = delegate()
			{
				Pawn actor = layDown.actor;
				Job curJob = actor.CurJob;
				JobDriver curDriver = actor.jobs.curDriver;
				Building_Bed building_Bed = (Building_Bed)curJob.GetTarget(bedOrRestSpotIndex).Thing;
				actor.GainComfortFromCellIfPossible();
				if (!curDriver.asleep)
				{
					if (canSleep)
					{
						if ((actor.needs.rest != null && actor.needs.rest.CurLevel < RestUtility.FallAsleepMaxLevel(actor)) || curJob.forceSleep)
						{
							curDriver.asleep = true;
						}
					}
				}
				else if (!canSleep)
				{
					curDriver.asleep = false;
				}
				else if ((actor.needs.rest == null || actor.needs.rest.CurLevel >= RestUtility.WakeThreshold(actor)) && !curJob.forceSleep)
				{
					curDriver.asleep = false;
				}
				if (curDriver.asleep)
				{
					if (gainRestAndHealth && actor.needs.rest != null)
					{
						float restEffectiveness;
						if (building_Bed != null && building_Bed.def.statBases.StatListContains(StatDefOf.BedRestEffectiveness))
						{
							restEffectiveness = building_Bed.GetStatValue(StatDefOf.BedRestEffectiveness, true);
						}
						else
						{
							restEffectiveness = 0.8f;
						}
						actor.needs.rest.TickResting(restEffectiveness);
					}
				}
				if (actor.mindState.applyBedThoughtsTick != 0 && actor.mindState.applyBedThoughtsTick <= Find.TickManager.TicksGame)
				{
					Toils_LayDown.ApplyBedThoughts(actor);
					actor.mindState.applyBedThoughtsTick += 60000;
					actor.mindState.applyBedThoughtsOnLeave = true;
				}
				if (actor.IsHashIntervalTick(100) && !actor.Position.Fogged(actor.Map))
				{
					if (curDriver.asleep)
					{
						MoteMaker.ThrowMetaIcon(actor.Position, actor.Map, ThingDefOf.Mote_SleepZ);
					}
					if (gainRestAndHealth && actor.health.hediffSet.GetNaturallyHealingInjuredParts().Any<BodyPartRecord>())
					{
						MoteMaker.ThrowMetaIcon(actor.Position, actor.Map, ThingDefOf.Mote_HealingCross);
					}
				}
				if (actor.ownership != null && building_Bed != null && !building_Bed.Medical && !building_Bed.owners.Contains(actor))
				{
					if (actor.Downed)
					{
						actor.Position = CellFinder.RandomClosewalkCellNear(actor.Position, actor.Map, 1, null);
					}
					actor.jobs.EndCurrentJob(JobCondition.Incompletable, true);
				}
				else if (lookForOtherJobs && actor.IsHashIntervalTick(211))
				{
					actor.jobs.CheckForJobOverride();
				}
			};
			layDown.defaultCompleteMode = ToilCompleteMode.Never;
			if (hasBed)
			{
				layDown.FailOnBedNoLongerUsable(bedOrRestSpotIndex);
			}
			layDown.AddFinishAction(delegate
			{
				Pawn actor = layDown.actor;
				JobDriver curDriver = actor.jobs.curDriver;
				if (actor.mindState.applyBedThoughtsOnLeave)
				{
					Toils_LayDown.ApplyBedThoughts(actor);
				}
				curDriver.asleep = false;
			});
			return layDown;
		}

		private static void ApplyBedThoughts(Pawn actor)
		{
			if (actor.needs.mood != null)
			{
				Building_Bed building_Bed = actor.CurrentBed();
				actor.needs.mood.thoughts.memories.RemoveMemoriesOfDef(ThoughtDefOf.SleptInBedroom);
				actor.needs.mood.thoughts.memories.RemoveMemoriesOfDef(ThoughtDefOf.SleptInBarracks);
				actor.needs.mood.thoughts.memories.RemoveMemoriesOfDef(ThoughtDefOf.SleptOutside);
				actor.needs.mood.thoughts.memories.RemoveMemoriesOfDef(ThoughtDefOf.SleptOnGround);
				actor.needs.mood.thoughts.memories.RemoveMemoriesOfDef(ThoughtDefOf.SleptInCold);
				actor.needs.mood.thoughts.memories.RemoveMemoriesOfDef(ThoughtDefOf.SleptInHeat);
				if (actor.GetRoom(RegionType.Set_Passable).PsychologicallyOutdoors)
				{
					actor.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.SleptOutside, null);
				}
				if (building_Bed == null || building_Bed.CostListAdjusted().Count == 0)
				{
					actor.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.SleptOnGround, null);
				}
				if (actor.AmbientTemperature < actor.def.GetStatValueAbstract(StatDefOf.ComfyTemperatureMin, null))
				{
					actor.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.SleptInCold, null);
				}
				if (actor.AmbientTemperature > actor.def.GetStatValueAbstract(StatDefOf.ComfyTemperatureMax, null))
				{
					actor.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.SleptInHeat, null);
				}
				if (building_Bed != null && building_Bed == actor.ownership.OwnedBed && !building_Bed.ForPrisoners && !actor.story.traits.HasTrait(TraitDefOf.Ascetic))
				{
					ThoughtDef thoughtDef = null;
					if (building_Bed.GetRoom(RegionType.Set_Passable).Role == RoomRoleDefOf.Bedroom)
					{
						thoughtDef = ThoughtDefOf.SleptInBedroom;
					}
					else if (building_Bed.GetRoom(RegionType.Set_Passable).Role == RoomRoleDefOf.Barracks)
					{
						thoughtDef = ThoughtDefOf.SleptInBarracks;
					}
					if (thoughtDef != null)
					{
						int scoreStageIndex = RoomStatDefOf.Impressiveness.GetScoreStageIndex(building_Bed.GetRoom(RegionType.Set_Passable).GetStat(RoomStatDefOf.Impressiveness));
						if (thoughtDef.stages[scoreStageIndex] != null)
						{
							actor.needs.mood.thoughts.memories.TryGainMemory(ThoughtMaker.MakeThought(thoughtDef, scoreStageIndex), null);
						}
					}
				}
			}
		}

		[CompilerGenerated]
		private sealed class <LayDown>c__AnonStorey0
		{
			internal Toil layDown;

			internal bool hasBed;

			internal TargetIndex bedOrRestSpotIndex;

			internal bool canSleep;

			internal bool gainRestAndHealth;

			internal bool lookForOtherJobs;

			public <LayDown>c__AnonStorey0()
			{
			}

			internal void <>m__0()
			{
				Pawn actor = this.layDown.actor;
				actor.pather.StopDead();
				JobDriver curDriver = actor.jobs.curDriver;
				if (this.hasBed)
				{
					Building_Bed t = (Building_Bed)actor.CurJob.GetTarget(this.bedOrRestSpotIndex).Thing;
					if (!t.OccupiedRect().Contains(actor.Position))
					{
						Log.Error("Can't start LayDown toil because pawn is not in the bed. pawn=" + actor, false);
						actor.jobs.EndCurrentJob(JobCondition.Errored, true);
						return;
					}
					actor.jobs.posture = PawnPosture.LayingInBed;
				}
				else
				{
					actor.jobs.posture = PawnPosture.LayingOnGroundNormal;
				}
				curDriver.asleep = false;
				if (actor.mindState.applyBedThoughtsTick == 0)
				{
					actor.mindState.applyBedThoughtsTick = Find.TickManager.TicksGame + Rand.Range(2500, 10000);
					actor.mindState.applyBedThoughtsOnLeave = false;
				}
				if (actor.ownership != null && actor.CurrentBed() != actor.ownership.OwnedBed)
				{
					ThoughtUtility.RemovePositiveBedroomThoughts(actor);
				}
			}

			internal void <>m__1()
			{
				Pawn actor = this.layDown.actor;
				Job curJob = actor.CurJob;
				JobDriver curDriver = actor.jobs.curDriver;
				Building_Bed building_Bed = (Building_Bed)curJob.GetTarget(this.bedOrRestSpotIndex).Thing;
				actor.GainComfortFromCellIfPossible();
				if (!curDriver.asleep)
				{
					if (this.canSleep)
					{
						if ((actor.needs.rest != null && actor.needs.rest.CurLevel < RestUtility.FallAsleepMaxLevel(actor)) || curJob.forceSleep)
						{
							curDriver.asleep = true;
						}
					}
				}
				else if (!this.canSleep)
				{
					curDriver.asleep = false;
				}
				else if ((actor.needs.rest == null || actor.needs.rest.CurLevel >= RestUtility.WakeThreshold(actor)) && !curJob.forceSleep)
				{
					curDriver.asleep = false;
				}
				if (curDriver.asleep)
				{
					if (this.gainRestAndHealth && actor.needs.rest != null)
					{
						float restEffectiveness;
						if (building_Bed != null && building_Bed.def.statBases.StatListContains(StatDefOf.BedRestEffectiveness))
						{
							restEffectiveness = building_Bed.GetStatValue(StatDefOf.BedRestEffectiveness, true);
						}
						else
						{
							restEffectiveness = 0.8f;
						}
						actor.needs.rest.TickResting(restEffectiveness);
					}
				}
				if (actor.mindState.applyBedThoughtsTick != 0 && actor.mindState.applyBedThoughtsTick <= Find.TickManager.TicksGame)
				{
					Toils_LayDown.ApplyBedThoughts(actor);
					actor.mindState.applyBedThoughtsTick += 60000;
					actor.mindState.applyBedThoughtsOnLeave = true;
				}
				if (actor.IsHashIntervalTick(100) && !actor.Position.Fogged(actor.Map))
				{
					if (curDriver.asleep)
					{
						MoteMaker.ThrowMetaIcon(actor.Position, actor.Map, ThingDefOf.Mote_SleepZ);
					}
					if (this.gainRestAndHealth && actor.health.hediffSet.GetNaturallyHealingInjuredParts().Any<BodyPartRecord>())
					{
						MoteMaker.ThrowMetaIcon(actor.Position, actor.Map, ThingDefOf.Mote_HealingCross);
					}
				}
				if (actor.ownership != null && building_Bed != null && !building_Bed.Medical && !building_Bed.owners.Contains(actor))
				{
					if (actor.Downed)
					{
						actor.Position = CellFinder.RandomClosewalkCellNear(actor.Position, actor.Map, 1, null);
					}
					actor.jobs.EndCurrentJob(JobCondition.Incompletable, true);
				}
				else if (this.lookForOtherJobs && actor.IsHashIntervalTick(211))
				{
					actor.jobs.CheckForJobOverride();
				}
			}

			internal void <>m__2()
			{
				Pawn actor = this.layDown.actor;
				JobDriver curDriver = actor.jobs.curDriver;
				if (actor.mindState.applyBedThoughtsOnLeave)
				{
					Toils_LayDown.ApplyBedThoughts(actor);
				}
				curDriver.asleep = false;
			}
		}
	}
}
