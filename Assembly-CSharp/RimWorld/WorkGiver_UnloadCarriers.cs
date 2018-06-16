﻿using System;
using Verse;
using Verse.AI;

namespace RimWorld
{
	// Token: 0x02000166 RID: 358
	public class WorkGiver_UnloadCarriers : WorkGiver_Scanner
	{
		// Token: 0x17000125 RID: 293
		// (get) Token: 0x0600075C RID: 1884 RVA: 0x00049618 File Offset: 0x00047A18
		public override ThingRequest PotentialWorkThingRequest
		{
			get
			{
				return ThingRequest.ForGroup(ThingRequestGroup.Pawn);
			}
		}

		// Token: 0x17000126 RID: 294
		// (get) Token: 0x0600075D RID: 1885 RVA: 0x00049634 File Offset: 0x00047A34
		public override PathEndMode PathEndMode
		{
			get
			{
				return PathEndMode.Touch;
			}
		}

		// Token: 0x0600075E RID: 1886 RVA: 0x0004964C File Offset: 0x00047A4C
		public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			return UnloadCarriersJobGiverUtility.HasJobOnThing(pawn, t, forced);
		}

		// Token: 0x0600075F RID: 1887 RVA: 0x0004966C File Offset: 0x00047A6C
		public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			return new Job(JobDefOf.UnloadInventory, t);
		}
	}
}
