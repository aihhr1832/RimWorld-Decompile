﻿using System;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld
{
	// Token: 0x0200018E RID: 398
	internal class LordToil_DefendTraderCaravan : LordToil_DefendPoint
	{
		// Token: 0x06000838 RID: 2104 RVA: 0x0004F201 File Offset: 0x0004D601
		public LordToil_DefendTraderCaravan() : base(true)
		{
		}

		// Token: 0x06000839 RID: 2105 RVA: 0x0004F20B File Offset: 0x0004D60B
		public LordToil_DefendTraderCaravan(IntVec3 defendPoint) : base(defendPoint, 28f)
		{
		}

		// Token: 0x1700014B RID: 331
		// (get) Token: 0x0600083A RID: 2106 RVA: 0x0004F21C File Offset: 0x0004D61C
		public override bool AllowSatisfyLongNeeds
		{
			get
			{
				return false;
			}
		}

		// Token: 0x1700014C RID: 332
		// (get) Token: 0x0600083B RID: 2107 RVA: 0x0004F234 File Offset: 0x0004D634
		public override float? CustomWakeThreshold
		{
			get
			{
				return new float?(0.5f);
			}
		}

		// Token: 0x0600083C RID: 2108 RVA: 0x0004F254 File Offset: 0x0004D654
		public override void UpdateAllDuties()
		{
			LordToilData_DefendPoint data = base.Data;
			Pawn pawn = TraderCaravanUtility.FindTrader(this.lord);
			if (pawn != null)
			{
				pawn.mindState.duty = new PawnDuty(DutyDefOf.Defend, data.defendPoint, data.defendRadius);
				int i = 0;
				while (i < this.lord.ownedPawns.Count)
				{
					Pawn pawn2 = this.lord.ownedPawns[i];
					switch (pawn2.GetTraderCaravanRole())
					{
					case TraderCaravanRole.Carrier:
						pawn2.mindState.duty = new PawnDuty(DutyDefOf.Follow, pawn, 5f);
						pawn2.mindState.duty.locomotion = LocomotionUrgency.Walk;
						break;
					case TraderCaravanRole.Guard:
						pawn2.mindState.duty = new PawnDuty(DutyDefOf.Defend, data.defendPoint, data.defendRadius);
						break;
					case TraderCaravanRole.Chattel:
						pawn2.mindState.duty = new PawnDuty(DutyDefOf.Escort, pawn, 5f);
						pawn2.mindState.duty.locomotion = LocomotionUrgency.Walk;
						break;
					}
					IL_127:
					i++;
					continue;
					goto IL_127;
				}
			}
		}
	}
}
