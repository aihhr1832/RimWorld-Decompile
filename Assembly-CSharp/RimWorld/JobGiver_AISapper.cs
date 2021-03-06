﻿using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld
{
	public class JobGiver_AISapper : ThinkNode_JobGiver
	{
		private bool canMineMineables = true;

		private bool canMineNonMineables = true;

		private const float ReachDestDist = 10f;

		private const int CheckOverrideInterval = 500;

		public JobGiver_AISapper()
		{
		}

		public override ThinkNode DeepCopy(bool resolve = true)
		{
			JobGiver_AISapper jobGiver_AISapper = (JobGiver_AISapper)base.DeepCopy(resolve);
			jobGiver_AISapper.canMineMineables = this.canMineMineables;
			jobGiver_AISapper.canMineNonMineables = this.canMineNonMineables;
			return jobGiver_AISapper;
		}

		protected override Job TryGiveJob(Pawn pawn)
		{
			IntVec3 intVec = (IntVec3)pawn.mindState.duty.focus;
			if (intVec.IsValid)
			{
				if ((float)intVec.DistanceToSquared(pawn.Position) < 100f && intVec.GetRoom(pawn.Map, RegionType.Set_Passable) == pawn.GetRoom(RegionType.Set_Passable) && intVec.WithinRegions(pawn.Position, pawn.Map, 9, TraverseMode.NoPassClosedDoors, RegionType.Set_Passable))
				{
					pawn.GetLord().Notify_ReachedDutyLocation(pawn);
					return null;
				}
			}
			if (!intVec.IsValid)
			{
				IAttackTarget attackTarget;
				if (!(from x in pawn.Map.attackTargetsCache.GetPotentialTargetsFor(pawn)
				where !x.ThreatDisabled(pawn) && x.Thing.Faction == Faction.OfPlayer && pawn.CanReach(x.Thing, PathEndMode.OnCell, Danger.Deadly, false, TraverseMode.PassAllDestroyableThings)
				select x).TryRandomElement(out attackTarget))
				{
					return null;
				}
				intVec = attackTarget.Thing.Position;
			}
			Job result;
			if (!pawn.CanReach(intVec, PathEndMode.OnCell, Danger.Deadly, false, TraverseMode.PassAllDestroyableThings))
			{
				result = null;
			}
			else
			{
				using (PawnPath pawnPath = pawn.Map.pathFinder.FindPath(pawn.Position, intVec, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.PassAllDestroyableThings, false), PathEndMode.OnCell))
				{
					IntVec3 cellBeforeBlocker;
					Thing thing = pawnPath.FirstBlockingBuilding(out cellBeforeBlocker, pawn);
					if (thing != null)
					{
						Job job = DigUtility.PassBlockerJob(pawn, thing, cellBeforeBlocker, this.canMineMineables, this.canMineNonMineables);
						if (job != null)
						{
							return job;
						}
					}
				}
				result = new Job(JobDefOf.Goto, intVec, 500, true);
			}
			return result;
		}

		[CompilerGenerated]
		private sealed class <TryGiveJob>c__AnonStorey0
		{
			internal Pawn pawn;

			public <TryGiveJob>c__AnonStorey0()
			{
			}

			internal bool <>m__0(IAttackTarget x)
			{
				return !x.ThreatDisabled(this.pawn) && x.Thing.Faction == Faction.OfPlayer && this.pawn.CanReach(x.Thing, PathEndMode.OnCell, Danger.Deadly, false, TraverseMode.PassAllDestroyableThings);
			}
		}
	}
}
