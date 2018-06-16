﻿using System;

namespace Verse
{
	// Token: 0x02000CF5 RID: 3317
	public class DamageWorker_Bite : DamageWorker_AddInjury
	{
		// Token: 0x06004902 RID: 18690 RVA: 0x002651DC File Offset: 0x002635DC
		protected override BodyPartRecord ChooseHitPart(DamageInfo dinfo, Pawn pawn)
		{
			return pawn.health.hediffSet.GetRandomNotMissingPart(dinfo.Def, dinfo.Height, BodyPartDepth.Outside);
		}

		// Token: 0x06004903 RID: 18691 RVA: 0x00265210 File Offset: 0x00263610
		protected override void ApplySpecialEffectsToPart(Pawn pawn, float totalDamage, DamageInfo dinfo, DamageWorker.DamageResult result)
		{
			totalDamage *= this.def.biteDamageMultiplier;
			base.FinalizeAndAddInjury(pawn, totalDamage, dinfo, result);
		}
	}
}
