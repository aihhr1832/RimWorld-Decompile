﻿using System;
using System.Runtime.CompilerServices;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public static class JobInBedUtility
	{
		public static void KeepLyingDown(this JobDriver driver, TargetIndex bedIndex)
		{
			driver.AddFinishAction(delegate
			{
				Pawn pawn = driver.pawn;
				if (!pawn.Drafted)
				{
					pawn.jobs.jobQueue.EnqueueFirst(new Job(JobDefOf.LayDown, pawn.CurJob.GetTarget(bedIndex)), null);
				}
			});
		}

		public static bool InBedOrRestSpotNow(Pawn pawn, LocalTargetInfo bedOrRestSpot)
		{
			bool result;
			if (!bedOrRestSpot.IsValid || !pawn.Spawned)
			{
				result = false;
			}
			else if (bedOrRestSpot.HasThing)
			{
				result = (bedOrRestSpot.Thing.Map == pawn.Map && RestUtility.GetBedSleepingSlotPosFor(pawn, (Building_Bed)bedOrRestSpot.Thing) == pawn.Position);
			}
			else
			{
				result = (bedOrRestSpot.Cell == pawn.Position);
			}
			return result;
		}

		[CompilerGenerated]
		private sealed class <KeepLyingDown>c__AnonStorey0
		{
			internal JobDriver driver;

			internal TargetIndex bedIndex;

			public <KeepLyingDown>c__AnonStorey0()
			{
			}

			internal void <>m__0()
			{
				Pawn pawn = this.driver.pawn;
				if (!pawn.Drafted)
				{
					pawn.jobs.jobQueue.EnqueueFirst(new Job(JobDefOf.LayDown, pawn.CurJob.GetTarget(this.bedIndex)), null);
				}
			}
		}
	}
}
