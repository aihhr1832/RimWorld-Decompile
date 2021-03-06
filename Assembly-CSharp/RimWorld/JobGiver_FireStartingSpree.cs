﻿using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Verse;
using Verse.AI;

namespace RimWorld
{
	internal class JobGiver_FireStartingSpree : ThinkNode_JobGiver
	{
		private IntRange waitTicks = new IntRange(80, 140);

		private const float FireStartChance = 0.75f;

		private static List<Thing> potentialTargets = new List<Thing>();

		public JobGiver_FireStartingSpree()
		{
		}

		public override ThinkNode DeepCopy(bool resolve = true)
		{
			JobGiver_FireStartingSpree jobGiver_FireStartingSpree = (JobGiver_FireStartingSpree)base.DeepCopy(resolve);
			jobGiver_FireStartingSpree.waitTicks = this.waitTicks;
			return jobGiver_FireStartingSpree;
		}

		protected override Job TryGiveJob(Pawn pawn)
		{
			Job result;
			if (pawn.mindState.nextMoveOrderIsWait)
			{
				Job job = new Job(JobDefOf.Wait_Wander);
				job.expiryInterval = this.waitTicks.RandomInRange;
				pawn.mindState.nextMoveOrderIsWait = false;
				result = job;
			}
			else
			{
				if (Rand.Value < 0.75f)
				{
					Thing thing = this.TryFindRandomIgniteTarget(pawn);
					if (thing != null)
					{
						pawn.mindState.nextMoveOrderIsWait = true;
						return new Job(JobDefOf.Ignite, thing);
					}
				}
				IntVec3 c = RCellFinder.RandomWanderDestFor(pawn, pawn.Position, 10f, null, Danger.Deadly);
				if (c.IsValid)
				{
					pawn.mindState.nextMoveOrderIsWait = true;
					result = new Job(JobDefOf.GotoWander, c);
				}
				else
				{
					result = null;
				}
			}
			return result;
		}

		private Thing TryFindRandomIgniteTarget(Pawn pawn)
		{
			Region region;
			Thing result;
			if (!CellFinder.TryFindClosestRegionWith(pawn.GetRegion(RegionType.Set_Passable), TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false), (Region candidateRegion) => !candidateRegion.IsForbiddenEntirely(pawn), 100, out region, RegionType.Set_Passable))
			{
				result = null;
			}
			else
			{
				JobGiver_FireStartingSpree.potentialTargets.Clear();
				List<Thing> allThings = region.ListerThings.AllThings;
				for (int i = 0; i < allThings.Count; i++)
				{
					Thing thing = allThings[i];
					if ((thing.def.category == ThingCategory.Building || thing.def.category == ThingCategory.Item || thing.def.category == ThingCategory.Plant) && thing.FlammableNow && !thing.IsBurning() && !thing.OccupiedRect().Contains(pawn.Position))
					{
						JobGiver_FireStartingSpree.potentialTargets.Add(thing);
					}
				}
				if (JobGiver_FireStartingSpree.potentialTargets.NullOrEmpty<Thing>())
				{
					result = null;
				}
				else
				{
					result = JobGiver_FireStartingSpree.potentialTargets.RandomElement<Thing>();
				}
			}
			return result;
		}

		// Note: this type is marked as 'beforefieldinit'.
		static JobGiver_FireStartingSpree()
		{
		}

		[CompilerGenerated]
		private sealed class <TryFindRandomIgniteTarget>c__AnonStorey0
		{
			internal Pawn pawn;

			public <TryFindRandomIgniteTarget>c__AnonStorey0()
			{
			}

			internal bool <>m__0(Region candidateRegion)
			{
				return !candidateRegion.IsForbiddenEntirely(this.pawn);
			}
		}
	}
}
