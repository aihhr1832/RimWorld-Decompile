﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Verse.AI
{
	// Token: 0x02000A71 RID: 2673
	public class MentalState_InsultingSpreeAll : MentalState_InsultingSpree
	{
		// Token: 0x06003B51 RID: 15185 RVA: 0x001F6D4E File Offset: 0x001F514E
		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look<int>(ref this.targetFoundTicks, "targetFoundTicks", 0, false);
		}

		// Token: 0x06003B52 RID: 15186 RVA: 0x001F6D69 File Offset: 0x001F5169
		public override void PostStart(string reason)
		{
			base.PostStart(reason);
			this.ChooseNextTarget();
		}

		// Token: 0x06003B53 RID: 15187 RVA: 0x001F6D7C File Offset: 0x001F517C
		public override void MentalStateTick()
		{
			if (this.target != null && !InsultingSpreeMentalStateUtility.CanChaseAndInsult(this.pawn, this.target, false, true))
			{
				this.ChooseNextTarget();
			}
			if (this.pawn.IsHashIntervalTick(250) && (this.target == null || this.insultedTargetAtLeastOnce))
			{
				this.ChooseNextTarget();
			}
			base.MentalStateTick();
		}

		// Token: 0x06003B54 RID: 15188 RVA: 0x001F6DEC File Offset: 0x001F51EC
		private void ChooseNextTarget()
		{
			InsultingSpreeMentalStateUtility.GetInsultCandidatesFor(this.pawn, MentalState_InsultingSpreeAll.candidates, true);
			if (!MentalState_InsultingSpreeAll.candidates.Any<Pawn>())
			{
				this.target = null;
				this.insultedTargetAtLeastOnce = false;
				this.targetFoundTicks = -1;
			}
			else
			{
				Pawn pawn;
				if (this.target != null && Find.TickManager.TicksGame - this.targetFoundTicks > 1250 && MentalState_InsultingSpreeAll.candidates.Any((Pawn x) => x != this.target))
				{
					pawn = (from x in MentalState_InsultingSpreeAll.candidates
					where x != this.target
					select x).RandomElementByWeight((Pawn x) => this.GetCandidateWeight(x));
				}
				else
				{
					pawn = MentalState_InsultingSpreeAll.candidates.RandomElementByWeight((Pawn x) => this.GetCandidateWeight(x));
				}
				if (pawn != this.target)
				{
					this.target = pawn;
					this.insultedTargetAtLeastOnce = false;
					this.targetFoundTicks = Find.TickManager.TicksGame;
				}
			}
		}

		// Token: 0x06003B55 RID: 15189 RVA: 0x001F6EEC File Offset: 0x001F52EC
		private float GetCandidateWeight(Pawn candidate)
		{
			float num = this.pawn.Position.DistanceTo(candidate.Position);
			float num2 = Mathf.Min(num / 40f, 1f);
			return 1f - num2 + 0.01f;
		}

		// Token: 0x04002565 RID: 9573
		private int targetFoundTicks;

		// Token: 0x04002566 RID: 9574
		private const int CheckChooseNewTargetIntervalTicks = 250;

		// Token: 0x04002567 RID: 9575
		private const int MaxSameTargetChaseTicks = 1250;

		// Token: 0x04002568 RID: 9576
		private static List<Pawn> candidates = new List<Pawn>();
	}
}
