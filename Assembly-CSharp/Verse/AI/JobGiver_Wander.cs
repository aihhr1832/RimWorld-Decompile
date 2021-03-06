﻿using System;
using RimWorld;

namespace Verse.AI
{
	public abstract class JobGiver_Wander : ThinkNode_JobGiver
	{
		protected float wanderRadius;

		protected Func<Pawn, IntVec3, IntVec3, bool> wanderDestValidator = null;

		protected IntRange ticksBetweenWandersRange = new IntRange(20, 100);

		protected LocomotionUrgency locomotionUrgency = LocomotionUrgency.Walk;

		protected Danger maxDanger = Danger.None;

		protected int expiryInterval = -1;

		protected JobGiver_Wander()
		{
		}

		public override ThinkNode DeepCopy(bool resolve = true)
		{
			JobGiver_Wander jobGiver_Wander = (JobGiver_Wander)base.DeepCopy(resolve);
			jobGiver_Wander.wanderRadius = this.wanderRadius;
			jobGiver_Wander.wanderDestValidator = this.wanderDestValidator;
			jobGiver_Wander.ticksBetweenWandersRange = this.ticksBetweenWandersRange;
			jobGiver_Wander.locomotionUrgency = this.locomotionUrgency;
			jobGiver_Wander.maxDanger = this.maxDanger;
			jobGiver_Wander.expiryInterval = this.expiryInterval;
			return jobGiver_Wander;
		}

		protected override Job TryGiveJob(Pawn pawn)
		{
			bool flag = pawn.CurJob != null && pawn.CurJob.def == JobDefOf.GotoWander;
			bool nextMoveOrderIsWait = pawn.mindState.nextMoveOrderIsWait;
			if (!flag)
			{
				pawn.mindState.nextMoveOrderIsWait = !pawn.mindState.nextMoveOrderIsWait;
			}
			Job result;
			if (nextMoveOrderIsWait && !flag)
			{
				result = new Job(JobDefOf.Wait_Wander)
				{
					expiryInterval = this.ticksBetweenWandersRange.RandomInRange
				};
			}
			else
			{
				IntVec3 exactWanderDest = this.GetExactWanderDest(pawn);
				if (!exactWanderDest.IsValid)
				{
					pawn.mindState.nextMoveOrderIsWait = false;
					result = null;
				}
				else
				{
					result = new Job(JobDefOf.GotoWander, exactWanderDest)
					{
						locomotionUrgency = this.locomotionUrgency,
						expiryInterval = this.expiryInterval,
						checkOverrideOnExpire = true
					};
				}
			}
			return result;
		}

		protected virtual IntVec3 GetExactWanderDest(Pawn pawn)
		{
			IntVec3 wanderRoot = this.GetWanderRoot(pawn);
			return RCellFinder.RandomWanderDestFor(pawn, wanderRoot, this.wanderRadius, this.wanderDestValidator, PawnUtility.ResolveMaxDanger(pawn, this.maxDanger));
		}

		protected abstract IntVec3 GetWanderRoot(Pawn pawn);
	}
}
