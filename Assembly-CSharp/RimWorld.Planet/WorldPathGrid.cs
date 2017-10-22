using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	public class WorldPathGrid
	{
		private const int ImpassableCost = 1000000;

		public int[] pathGrid;

		private int allPathCostsRecalculatedDayOfYear = -1;

		private static int DayOfYearAt0Long
		{
			get
			{
				return GenDate.DayOfYear(GenTicks.TicksAbs, 0f);
			}
		}

		public WorldPathGrid()
		{
			this.ResetPathGrid();
		}

		public void ResetPathGrid()
		{
			this.pathGrid = new int[Find.WorldGrid.TilesCount];
		}

		public void WorldPathGridTick()
		{
			if (this.allPathCostsRecalculatedDayOfYear != WorldPathGrid.DayOfYearAt0Long)
			{
				this.RecalculateAllPerceivedPathCosts(-1f);
			}
		}

		public bool Passable(int tile)
		{
			if (!Find.WorldGrid.InBounds(tile))
			{
				return false;
			}
			return this.pathGrid[tile] < 1000000;
		}

		public bool PassableFast(int tile)
		{
			return this.pathGrid[tile] < 1000000;
		}

		public int PerceivedPathCostAt(int tile)
		{
			return this.pathGrid[tile];
		}

		public void RecalculatePerceivedPathCostAt(int tile, float yearPercent = -1f)
		{
			if (Find.WorldGrid.InBounds(tile))
			{
				bool flag = this.PassableFast(tile);
				this.pathGrid[tile] = WorldPathGrid.CalculatedCostAt(tile, true, yearPercent);
				if (flag != this.PassableFast(tile))
				{
					Find.WorldReachability.ClearCache();
				}
			}
		}

		public void RecalculateAllPerceivedPathCosts(float yearPercent = -1f)
		{
			this.allPathCostsRecalculatedDayOfYear = WorldPathGrid.DayOfYearAt0Long;
			for (int i = 0; i < this.pathGrid.Length; i++)
			{
				this.RecalculatePerceivedPathCostAt(i, yearPercent);
			}
		}

		public static int CalculatedCostAt(int tile, bool perceivedStatic, float yearPercent = -1f)
		{
			int num = 0;
			Tile tile2 = Find.WorldGrid[tile];
			if (tile2.biome.impassable)
			{
				return 1000000;
			}
			if (yearPercent < 0.0)
			{
				yearPercent = (float)((float)WorldPathGrid.DayOfYearAt0Long / 60.0);
			}
			float num2 = yearPercent;
			Vector2 vector = Find.WorldGrid.LongLatOf(tile);
			if (vector.y < 0.0)
			{
				num2 = (float)((num2 + 0.5) % 1.0);
			}
			num += Mathf.RoundToInt(tile2.biome.pathCost.Evaluate(num2));
			if (tile2.hilliness == Hilliness.Impassable)
			{
				return 1000000;
			}
			return num + WorldPathGrid.CostFromTileHilliness(tile2.hilliness);
		}

		private static int CostFromTileHilliness(Hilliness hilliness)
		{
			switch (hilliness)
			{
			case Hilliness.Flat:
			{
				return 0;
			}
			case Hilliness.SmallHills:
			{
				return 2000;
			}
			case Hilliness.LargeHills:
			{
				return 6000;
			}
			case Hilliness.Mountainous:
			{
				return 30000;
			}
			case Hilliness.Impassable:
			{
				return 30000;
			}
			default:
			{
				return 0;
			}
			}
		}
	}
}
