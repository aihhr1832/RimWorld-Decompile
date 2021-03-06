﻿using System;
using System.Collections.Generic;
using System.Text;
using RimWorld;
using UnityEngine;

namespace Verse.AI
{
	[HasDebugOutput]
	public sealed class PathGrid
	{
		private Map map;

		public int[] pathGrid;

		public const int ImpassableCost = 10000;

		private const int MaxThingsPathCost = 450;

		public PathGrid(Map map)
		{
			this.map = map;
			this.ResetPathGrid();
		}

		public void ResetPathGrid()
		{
			this.pathGrid = new int[this.map.cellIndices.NumGridCells];
		}

		public bool Walkable(IntVec3 loc)
		{
			return loc.InBounds(this.map) && this.pathGrid[this.map.cellIndices.CellToIndex(loc)] < 10000;
		}

		public bool WalkableFast(IntVec3 loc)
		{
			return this.pathGrid[this.map.cellIndices.CellToIndex(loc)] < 10000;
		}

		public bool WalkableFast(int x, int z)
		{
			return this.pathGrid[this.map.cellIndices.CellToIndex(x, z)] < 10000;
		}

		public bool WalkableFast(int index)
		{
			return this.pathGrid[index] < 10000;
		}

		public int PerceivedPathCostAt(IntVec3 loc)
		{
			return this.pathGrid[this.map.cellIndices.CellToIndex(loc)];
		}

		public void RecalculatePerceivedPathCostUnderThing(Thing t)
		{
			if (t.def.size == IntVec2.One)
			{
				this.RecalculatePerceivedPathCostAt(t.Position);
			}
			else
			{
				CellRect cellRect = t.OccupiedRect();
				for (int i = cellRect.minZ; i <= cellRect.maxZ; i++)
				{
					for (int j = cellRect.minX; j <= cellRect.maxX; j++)
					{
						IntVec3 c = new IntVec3(j, 0, i);
						this.RecalculatePerceivedPathCostAt(c);
					}
				}
			}
		}

		public void RecalculatePerceivedPathCostAt(IntVec3 c)
		{
			if (c.InBounds(this.map))
			{
				bool flag = this.WalkableFast(c);
				this.pathGrid[this.map.cellIndices.CellToIndex(c)] = this.CalculatedCostAt(c, true, IntVec3.Invalid);
				if (this.WalkableFast(c) != flag)
				{
					this.map.reachability.ClearCache();
					this.map.regionDirtyer.Notify_WalkabilityChanged(c);
				}
			}
		}

		public void RecalculateAllPerceivedPathCosts()
		{
			foreach (IntVec3 c in this.map.AllCells)
			{
				this.RecalculatePerceivedPathCostAt(c);
			}
		}

		public int CalculatedCostAt(IntVec3 c, bool perceivedStatic, IntVec3 prevCell)
		{
			int num = 0;
			TerrainDef terrainDef = this.map.terrainGrid.TerrainAt(c);
			if (terrainDef == null || terrainDef.passability == Traversability.Impassable)
			{
				num = 10000;
			}
			else
			{
				num += terrainDef.pathCost;
			}
			int num2 = SnowUtility.MovementTicksAddOn(this.map.snowGrid.GetCategory(c));
			num += num2;
			List<Thing> list = this.map.thingGrid.ThingsListAt(c);
			int num3 = 0;
			for (int i = 0; i < list.Count; i++)
			{
				Thing thing = list[i];
				if (thing.def.passability == Traversability.Impassable)
				{
					return 10000;
				}
				if (num3 < 450 && (!PathGrid.IsPathCostIgnoreRepeater(thing.def) || !prevCell.IsValid || !this.ContainsPathCostIgnoreRepeater(prevCell)))
				{
					num += Mathf.Min(thing.def.pathCost, 450 - num3);
					num3 += thing.def.pathCost;
				}
				if (prevCell.IsValid && thing is Building_Door)
				{
					Building edifice = prevCell.GetEdifice(this.map);
					if (edifice != null && edifice is Building_Door)
					{
						num += 45;
					}
				}
			}
			if (perceivedStatic)
			{
				for (int j = 0; j < 9; j++)
				{
					IntVec3 b = GenAdj.AdjacentCellsAndInside[j];
					IntVec3 c2 = c + b;
					if (c2.InBounds(this.map))
					{
						Fire fire = null;
						list = this.map.thingGrid.ThingsListAtFast(c2);
						for (int k = 0; k < list.Count; k++)
						{
							fire = (list[k] as Fire);
							if (fire != null)
							{
								break;
							}
						}
						if (fire != null && fire.parent == null)
						{
							if (b.x == 0 && b.z == 0)
							{
								num += 1000;
							}
							else
							{
								num += 150;
							}
						}
					}
				}
			}
			return num;
		}

		private bool ContainsPathCostIgnoreRepeater(IntVec3 c)
		{
			List<Thing> list = this.map.thingGrid.ThingsListAt(c);
			for (int i = 0; i < list.Count; i++)
			{
				if (PathGrid.IsPathCostIgnoreRepeater(list[i].def))
				{
					return true;
				}
			}
			return false;
		}

		private static bool IsPathCostIgnoreRepeater(ThingDef def)
		{
			return def.pathCost >= 25 && def.pathCostIgnoreRepeat;
		}

		[DebugOutput]
		public static void ThingPathCostsIgnoreRepeaters()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("===============PATH COST IGNORE REPEATERS==============");
			foreach (ThingDef thingDef in DefDatabase<ThingDef>.AllDefs)
			{
				if (PathGrid.IsPathCostIgnoreRepeater(thingDef) && thingDef.passability != Traversability.Impassable)
				{
					stringBuilder.AppendLine(thingDef.defName + " " + thingDef.pathCost);
				}
			}
			stringBuilder.AppendLine("===============NON-PATH COST IGNORE REPEATERS that are buildings with >0 pathCost ==============");
			foreach (ThingDef thingDef2 in DefDatabase<ThingDef>.AllDefs)
			{
				if (!PathGrid.IsPathCostIgnoreRepeater(thingDef2) && thingDef2.passability != Traversability.Impassable && thingDef2.category == ThingCategory.Building && thingDef2.pathCost > 0)
				{
					stringBuilder.AppendLine(thingDef2.defName + " " + thingDef2.pathCost);
				}
			}
			Log.Message(stringBuilder.ToString(), false);
		}
	}
}
