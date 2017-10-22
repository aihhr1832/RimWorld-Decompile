using System;
using System.Linq;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld
{
	public class JobGiver_AISapper : ThinkNode_JobGiver
	{
		private const float ReachDestDist = 10f;

		private const int CheckOverrideInterval = 500;

		private bool canMineNonMineables = true;

		public override ThinkNode DeepCopy(bool resolve = true)
		{
			JobGiver_AISapper jobGiver_AISapper = (JobGiver_AISapper)base.DeepCopy(resolve);
			jobGiver_AISapper.canMineNonMineables = this.canMineNonMineables;
			return jobGiver_AISapper;
		}

		protected override Job TryGiveJob(Pawn pawn)
		{
			IntVec3 intVec = (IntVec3)pawn.mindState.duty.focus;
			if (intVec.IsValid && (float)intVec.DistanceToSquared(pawn.Position) < 100.0 && intVec.GetRoom(pawn.Map, RegionType.Set_Passable) == pawn.GetRoom(RegionType.Set_Passable) && intVec.WithinRegions(pawn.Position, pawn.Map, 9, TraverseMode.NoPassClosedDoors, RegionType.Set_Passable))
			{
				pawn.GetLord().Notify_ReachedDutyLocation(pawn);
				return null;
			}
			if (!intVec.IsValid)
			{
				IAttackTarget attackTarget = default(IAttackTarget);
				if (!(from x in pawn.Map.attackTargetsCache.GetPotentialTargetsFor(pawn)
				where !x.ThreatDisabled() && x.Thing.Faction == Faction.OfPlayer && pawn.CanReach(x.Thing, PathEndMode.OnCell, Danger.Deadly, false, TraverseMode.PassAllDestroyableThings)
				select x).TryRandomElement<IAttackTarget>(out attackTarget))
				{
					return null;
				}
				intVec = attackTarget.Thing.Position;
			}
			if (!pawn.CanReach(intVec, PathEndMode.OnCell, Danger.Deadly, false, TraverseMode.PassAllDestroyableThings))
			{
				return null;
			}
			using (PawnPath path = pawn.Map.pathFinder.FindPath(pawn.Position, intVec, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.PassAllDestroyableThings, false), PathEndMode.OnCell))
			{
				IntVec3 cellBeforeBlocker = default(IntVec3);
				Thing thing = path.FirstBlockingBuilding(out cellBeforeBlocker, pawn);
				if (thing != null)
				{
					Job job = DigUtility.PassBlockerJob(pawn, thing, cellBeforeBlocker, this.canMineNonMineables);
					if (job != null)
					{
						return job;
					}
				}
			}
			return new Job(JobDefOf.Goto, intVec, 500, true);
		}
	}
}
