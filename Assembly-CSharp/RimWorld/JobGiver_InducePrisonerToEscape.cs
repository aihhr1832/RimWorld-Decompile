﻿using System;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobGiver_InducePrisonerToEscape : ThinkNode_JobGiver
	{
		public JobGiver_InducePrisonerToEscape()
		{
		}

		protected override Job TryGiveJob(Pawn pawn)
		{
			Pawn pawn2 = JailbreakerMentalStateUtility.FindPrisoner(pawn);
			Job result;
			if (pawn2 == null || !pawn.CanReach(pawn2, PathEndMode.Touch, Danger.Deadly, false, TraverseMode.ByPawn))
			{
				result = null;
			}
			else
			{
				result = new Job(JobDefOf.InducePrisonerToEscape, pawn2)
				{
					interaction = InteractionDefOf.SparkJailbreak
				};
			}
			return result;
		}
	}
}
