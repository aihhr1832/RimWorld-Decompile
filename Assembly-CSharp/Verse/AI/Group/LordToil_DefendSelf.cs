﻿using System;
using RimWorld;

namespace Verse.AI.Group
{
	// Token: 0x020009F5 RID: 2549
	public class LordToil_DefendSelf : LordToil
	{
		// Token: 0x0600393C RID: 14652 RVA: 0x001E68C8 File Offset: 0x001E4CC8
		public override void UpdateAllDuties()
		{
			for (int i = 0; i < this.lord.ownedPawns.Count; i++)
			{
				this.lord.ownedPawns[i].mindState.duty = new PawnDuty(DutyDefOf.Defend, this.lord.ownedPawns[i].Position, -1f);
				this.lord.ownedPawns[i].mindState.duty.radius = 28f;
			}
		}
	}
}
