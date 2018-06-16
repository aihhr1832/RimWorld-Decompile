﻿using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	// Token: 0x02000083 RID: 131
	public class JobDriver_TradeWithPawn : JobDriver
	{
		// Token: 0x170000B6 RID: 182
		// (get) Token: 0x06000370 RID: 880 RVA: 0x0002665C File Offset: 0x00024A5C
		private Pawn Trader
		{
			get
			{
				return (Pawn)base.TargetThingA;
			}
		}

		// Token: 0x06000371 RID: 881 RVA: 0x0002667C File Offset: 0x00024A7C
		public override bool TryMakePreToilReservations()
		{
			return this.pawn.Reserve(this.Trader, this.job, 1, -1, null);
		}

		// Token: 0x06000372 RID: 882 RVA: 0x000266B0 File Offset: 0x00024AB0
		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDespawnedOrNull(TargetIndex.A);
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch).FailOn(() => !this.Trader.CanTradeNow);
			Toil trade = new Toil();
			trade.initAction = delegate()
			{
				Pawn actor = trade.actor;
				if (this.Trader.CanTradeNow)
				{
					Find.WindowStack.Add(new Dialog_Trade(actor, this.Trader, false));
				}
			};
			yield return trade;
			yield break;
		}
	}
}
