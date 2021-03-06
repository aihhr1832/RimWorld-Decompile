﻿using System;
using RimWorld.Planet;

namespace RimWorld
{
	public class BiomeWorker_BorealForest : BiomeWorker
	{
		public BiomeWorker_BorealForest()
		{
		}

		public override float GetScore(Tile tile, int tileID)
		{
			float result;
			if (tile.WaterCovered)
			{
				result = -100f;
			}
			else if (tile.temperature < -10f)
			{
				result = 0f;
			}
			else if (tile.rainfall < 600f)
			{
				result = 0f;
			}
			else
			{
				result = 15f;
			}
			return result;
		}
	}
}
