﻿using System;
using Verse;

namespace RimWorld
{
	// Token: 0x02000756 RID: 1878
	public class CompProperties_TargetEffect_GoodwillImpact : CompProperties
	{
		// Token: 0x0600298E RID: 10638 RVA: 0x00161319 File Offset: 0x0015F719
		public CompProperties_TargetEffect_GoodwillImpact()
		{
			this.compClass = typeof(CompTargetEffect_GoodwillImpact);
		}

		// Token: 0x0400169A RID: 5786
		public int goodwillImpact = -200;
	}
}