﻿using System;
using Verse;

namespace RimWorld
{
	// Token: 0x0200024D RID: 589
	public class CompProperties_Power : CompProperties
	{
		// Token: 0x0400049E RID: 1182
		public bool transmitsPower = false;

		// Token: 0x0400049F RID: 1183
		public float basePowerConsumption = 0f;

		// Token: 0x040004A0 RID: 1184
		public bool shortCircuitInRain = false;

		// Token: 0x040004A1 RID: 1185
		public SoundDef soundPowerOn = null;

		// Token: 0x040004A2 RID: 1186
		public SoundDef soundPowerOff = null;

		// Token: 0x040004A3 RID: 1187
		public SoundDef soundAmbientPowered = null;
	}
}
