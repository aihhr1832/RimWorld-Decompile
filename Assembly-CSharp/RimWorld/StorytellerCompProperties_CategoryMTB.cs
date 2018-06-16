﻿using System;
using Verse;

namespace RimWorld
{
	// Token: 0x02000364 RID: 868
	public class StorytellerCompProperties_CategoryMTB : StorytellerCompProperties
	{
		// Token: 0x06000F1F RID: 3871 RVA: 0x0007FD20 File Offset: 0x0007E120
		public StorytellerCompProperties_CategoryMTB()
		{
			this.compClass = typeof(StorytellerComp_CategoryMTB);
		}

		// Token: 0x04000947 RID: 2375
		public float mtbDays = -1f;

		// Token: 0x04000948 RID: 2376
		public SimpleCurve mtbDaysFactorByDaysPassedCurve = null;

		// Token: 0x04000949 RID: 2377
		public IncidentCategoryDef category;
	}
}
