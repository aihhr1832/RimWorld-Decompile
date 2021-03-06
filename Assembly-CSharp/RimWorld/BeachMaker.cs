﻿using System;
using Verse;
using Verse.Noise;

namespace RimWorld
{
	internal static class BeachMaker
	{
		private static ModuleBase beachNoise;

		private const float PerlinFrequency = 0.03f;

		private const float MaxForDeepWater = 0.1f;

		private const float MaxForShallowWater = 0.45f;

		private const float MaxForSand = 1f;

		private static readonly FloatRange CoastWidthRange = new FloatRange(20f, 60f);

		public static void Init(Map map)
		{
			Rot4 a = Find.World.CoastDirectionAt(map.Tile);
			if (!a.IsValid)
			{
				BeachMaker.beachNoise = null;
			}
			else
			{
				ModuleBase moduleBase = new Perlin(0.029999999329447746, 2.0, 0.5, 3, Rand.Range(0, int.MaxValue), QualityMode.Medium);
				moduleBase = new ScaleBias(0.5, 0.5, moduleBase);
				NoiseDebugUI.StoreNoiseRender(moduleBase, "BeachMaker base", new IntVec2(map.Size.x, map.Size.z));
				ModuleBase moduleBase2 = new DistFromAxis(BeachMaker.CoastWidthRange.RandomInRange);
				if (a == Rot4.North)
				{
					moduleBase2 = new Rotate(0.0, 90.0, 0.0, moduleBase2);
					moduleBase2 = new Translate(0.0, 0.0, (double)(-(double)map.Size.z), moduleBase2);
				}
				else if (a == Rot4.East)
				{
					moduleBase2 = new Translate((double)(-(double)map.Size.x), 0.0, 0.0, moduleBase2);
				}
				else if (a == Rot4.South)
				{
					moduleBase2 = new Rotate(0.0, 90.0, 0.0, moduleBase2);
				}
				moduleBase2 = new ScaleBias(1.0, -1.0, moduleBase2);
				moduleBase2 = new Clamp(-1.0, 2.5, moduleBase2);
				NoiseDebugUI.StoreNoiseRender(moduleBase2, "BeachMaker axis bias");
				BeachMaker.beachNoise = new Add(moduleBase, moduleBase2);
				NoiseDebugUI.StoreNoiseRender(BeachMaker.beachNoise, "beachNoise");
			}
		}

		public static void Cleanup()
		{
			BeachMaker.beachNoise = null;
		}

		public static TerrainDef BeachTerrainAt(IntVec3 loc, BiomeDef biome)
		{
			TerrainDef result;
			if (BeachMaker.beachNoise == null)
			{
				result = null;
			}
			else
			{
				float value = BeachMaker.beachNoise.GetValue(loc);
				if (value < 0.1f)
				{
					result = TerrainDefOf.WaterOceanDeep;
				}
				else if (value < 0.45f)
				{
					result = TerrainDefOf.WaterOceanShallow;
				}
				else if (value < 1f)
				{
					result = ((biome != BiomeDefOf.SeaIce) ? TerrainDefOf.Sand : TerrainDefOf.Ice);
				}
				else
				{
					result = null;
				}
			}
			return result;
		}

		// Note: this type is marked as 'beforefieldinit'.
		static BeachMaker()
		{
		}
	}
}
