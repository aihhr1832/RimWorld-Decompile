﻿using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld
{
	// Token: 0x0200014B RID: 331
	public class WorkGiver_HelpGatheringItemsForCaravan : WorkGiver
	{
		// Token: 0x060006D6 RID: 1750 RVA: 0x000461F8 File Offset: 0x000445F8
		public override Job NonScanJob(Pawn pawn)
		{
			List<Lord> lords = pawn.Map.lordManager.lords;
			for (int i = 0; i < lords.Count; i++)
			{
				LordJob_FormAndSendCaravan lordJob_FormAndSendCaravan = lords[i].LordJob as LordJob_FormAndSendCaravan;
				if (lordJob_FormAndSendCaravan != null && lordJob_FormAndSendCaravan.GatheringItemsNow)
				{
					Thing thing = GatherItemsForCaravanUtility.FindThingToHaul(pawn, lords[i]);
					if (thing != null)
					{
						if (this.AnyReachableCarrierOrColonist(pawn, lords[i]))
						{
							return new Job(JobDefOf.PrepareCaravan_GatherItems, thing)
							{
								lord = lords[i]
							};
						}
					}
				}
			}
			return null;
		}

		// Token: 0x060006D7 RID: 1751 RVA: 0x000462B8 File Offset: 0x000446B8
		private bool AnyReachableCarrierOrColonist(Pawn forPawn, Lord lord)
		{
			for (int i = 0; i < lord.ownedPawns.Count; i++)
			{
				if (JobDriver_PrepareCaravan_GatherItems.IsUsableCarrier(lord.ownedPawns[i], forPawn, false))
				{
					if (!lord.ownedPawns[i].IsForbidden(forPawn))
					{
						if (forPawn.CanReach(lord.ownedPawns[i], PathEndMode.Touch, Danger.Deadly, false, TraverseMode.ByPawn))
						{
							return true;
						}
					}
				}
			}
			return false;
		}
	}
}
