﻿using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	// Token: 0x0200007D RID: 125
	public class JobDriver_Resurrect : JobDriver
	{
		// Token: 0x170000AE RID: 174
		// (get) Token: 0x06000352 RID: 850 RVA: 0x00024F24 File Offset: 0x00023324
		private Corpse Corpse
		{
			get
			{
				return (Corpse)this.job.GetTarget(TargetIndex.A).Thing;
			}
		}

		// Token: 0x170000AF RID: 175
		// (get) Token: 0x06000353 RID: 851 RVA: 0x00024F54 File Offset: 0x00023354
		private Thing Item
		{
			get
			{
				return this.job.GetTarget(TargetIndex.B).Thing;
			}
		}

		// Token: 0x06000354 RID: 852 RVA: 0x00024F80 File Offset: 0x00023380
		public override bool TryMakePreToilReservations()
		{
			return this.pawn.Reserve(this.Corpse, this.job, 1, -1, null) && this.pawn.Reserve(this.Item, this.job, 1, -1, null);
		}

		// Token: 0x06000355 RID: 853 RVA: 0x00024FDC File Offset: 0x000233DC
		protected override IEnumerable<Toil> MakeNewToils()
		{
			yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.Touch).FailOnDespawnedOrNull(TargetIndex.B).FailOnDespawnedOrNull(TargetIndex.A);
			yield return Toils_Haul.StartCarryThing(TargetIndex.B, false, false, false);
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch).FailOnDespawnedOrNull(TargetIndex.A);
			Toil prepare = Toils_General.Wait(600);
			prepare.WithProgressBarToilDelay(TargetIndex.A, false, -0.5f);
			prepare.FailOnDespawnedOrNull(TargetIndex.A);
			prepare.FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
			yield return prepare;
			yield return Toils_General.Do(new Action(this.Resurrect));
			yield break;
		}

		// Token: 0x06000356 RID: 854 RVA: 0x00025008 File Offset: 0x00023408
		private void Resurrect()
		{
			Pawn innerPawn = this.Corpse.InnerPawn;
			ResurrectionUtility.ResurrectWithSideEffects(innerPawn);
			Messages.Message("MessagePawnResurrected".Translate(new object[]
			{
				innerPawn.LabelIndefinite()
			}).CapitalizeFirst(), innerPawn, MessageTypeDefOf.PositiveEvent, true);
			this.Item.SplitOff(1).Destroy(DestroyMode.Vanish);
		}

		// Token: 0x04000233 RID: 563
		private const TargetIndex CorpseInd = TargetIndex.A;

		// Token: 0x04000234 RID: 564
		private const TargetIndex ItemInd = TargetIndex.B;

		// Token: 0x04000235 RID: 565
		private const int DurationTicks = 600;
	}
}
