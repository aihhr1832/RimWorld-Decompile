﻿using System;
using System.Runtime.CompilerServices;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobGiver_Berserk : ThinkNode_JobGiver
	{
		private const float MaxAttackDistance = 30f;

		private const float WaitChance = 0.5f;

		private const int WaitTicks = 90;

		private const int MinMeleeChaseTicks = 420;

		private const int MaxMeleeChaseTicks = 900;

		[CompilerGenerated]
		private static Predicate<Thing> <>f__am$cache0;

		public JobGiver_Berserk()
		{
		}

		protected override Job TryGiveJob(Pawn pawn)
		{
			Job result;
			if (Rand.Value < 0.5f)
			{
				result = new Job(JobDefOf.Wait_Combat)
				{
					expiryInterval = 90
				};
			}
			else if (pawn.TryGetAttackVerb(null, false) == null)
			{
				result = null;
			}
			else
			{
				Pawn pawn2 = this.FindPawnTarget(pawn);
				if (pawn2 != null)
				{
					result = new Job(JobDefOf.AttackMelee, pawn2)
					{
						maxNumMeleeAttacks = 1,
						expiryInterval = Rand.Range(420, 900),
						canBash = true
					};
				}
				else
				{
					result = null;
				}
			}
			return result;
		}

		private Pawn FindPawnTarget(Pawn pawn)
		{
			return (Pawn)AttackTargetFinder.BestAttackTarget(pawn, TargetScanFlags.NeedReachable | TargetScanFlags.NeedThreat, (Thing x) => x is Pawn, 0f, 30f, default(IntVec3), float.MaxValue, true);
		}

		[CompilerGenerated]
		private static bool <FindPawnTarget>m__0(Thing x)
		{
			return x is Pawn;
		}
	}
}
