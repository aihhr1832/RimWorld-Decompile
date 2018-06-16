﻿using System;
using System.Collections.Generic;
using RimWorld;
using Verse.AI;

namespace Verse
{
	// Token: 0x02000B54 RID: 2900
	public class MentalBreakDef : Def
	{
		// Token: 0x1700099E RID: 2462
		// (get) Token: 0x06003F66 RID: 16230 RVA: 0x00216370 File Offset: 0x00214770
		public MentalBreakWorker Worker
		{
			get
			{
				if (this.workerInt == null)
				{
					if (this.workerClass != null)
					{
						this.workerInt = (MentalBreakWorker)Activator.CreateInstance(this.workerClass);
						this.workerInt.def = this;
					}
				}
				return this.workerInt;
			}
		}

		// Token: 0x06003F67 RID: 16231 RVA: 0x002163C8 File Offset: 0x002147C8
		public override IEnumerable<string> ConfigErrors()
		{
			foreach (string e in this.<ConfigErrors>__BaseCallProxy0())
			{
				yield return e;
			}
			if (this.intensity == MentalBreakIntensity.None)
			{
				yield return "intensity not set";
			}
			yield break;
		}

		// Token: 0x040029F7 RID: 10743
		public Type workerClass = typeof(MentalBreakWorker);

		// Token: 0x040029F8 RID: 10744
		public MentalStateDef mentalState;

		// Token: 0x040029F9 RID: 10745
		public float baseCommonality;

		// Token: 0x040029FA RID: 10746
		public SimpleCurve commonalityFactorPerPopulationCurve = null;

		// Token: 0x040029FB RID: 10747
		public MentalBreakIntensity intensity = MentalBreakIntensity.None;

		// Token: 0x040029FC RID: 10748
		public TraitDef requiredTrait;

		// Token: 0x040029FD RID: 10749
		private MentalBreakWorker workerInt = null;
	}
}
