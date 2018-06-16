﻿using System;
using RimWorld.Planet;

namespace RimWorld
{
	// Token: 0x02000550 RID: 1360
	public class BiomeWorker_Ocean : BiomeWorker
	{
		// Token: 0x06001958 RID: 6488 RVA: 0x000DBD94 File Offset: 0x000DA194
		public override float GetScore(Tile tile, int tileID)
		{
			float result;
			if (!tile.WaterCovered)
			{
				result = -100f;
			}
			else
			{
				result = 0f;
			}
			return result;
		}
	}
}
