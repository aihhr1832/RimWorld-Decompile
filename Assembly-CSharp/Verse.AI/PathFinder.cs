#define PFPROFILE
using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace Verse.AI
{
	public class PathFinder
	{
		internal struct CostNode
		{
			public int index;

			public int cost;

			public CostNode(int index, int cost)
			{
				this.index = index;
				this.cost = cost;
			}
		}

		private struct PathFinderNodeFast
		{
			public int knownCost;

			public int heuristicCost;

			public int parentIndex;

			public int costNodeCost;

			public ushort status;
		}

		internal class CostNodeComparer : IComparer<CostNode>
		{
			public int Compare(CostNode a, CostNode b)
			{
				return a.cost.CompareTo(b.cost);
			}
		}

		public const int DefaultMoveTicksCardinal = 13;

		private const int DefaultMoveTicksDiagonal = 18;

		private const int SearchLimit = 160000;

		private const int Cost_DoorToBash = 300;

		private const int Cost_BlockedWall = 60;

		private const float Cost_BlockedWallPerHitPoint = 0.1f;

		public const int Cost_OutsideAllowedArea = 600;

		private const int Cost_PawnCollision = 175;

		private const int NodesToOpenBeforeRegionBasedPathing = 2000;

		private const float ExtraRegionHeuristicWeight = 5f;

		private const float NonRegionBasedHeuristicStrengthAnimal = 1.75f;

		private Map map;

		private FastPriorityQueue<CostNode> openList;

		private PathFinderNodeFast[] calcGrid;

		private List<int> disallowedCornerIndices = new List<int>(4);

		private ushort statusOpenValue = (ushort)1;

		private ushort statusClosedValue = (ushort)2;

		private RegionCostCalculatorWrapper regionCostCalculator;

		private int mapSizeX;

		private int mapSizeZ;

		private PathGrid pathGrid;

		private Building[] edificeGrid;

		private CellIndices cellIndices;

		private static readonly int[] Directions = new int[16]
		{
			0,
			1,
			0,
			-1,
			1,
			1,
			-1,
			-1,
			-1,
			0,
			1,
			0,
			-1,
			1,
			1,
			-1
		};

		private static readonly SimpleCurve NonRegionBasedHeuristicStrengthHuman_DistanceCurve = new SimpleCurve
		{
			{
				new CurvePoint(40f, 1f),
				true
			},
			{
				new CurvePoint(120f, 2.8f),
				true
			}
		};

		public PathFinder(Map map)
		{
			this.map = map;
			IntVec3 size = map.Size;
			this.mapSizeX = size.x;
			IntVec3 size2 = map.Size;
			this.mapSizeZ = size2.z;
			this.calcGrid = new PathFinderNodeFast[this.mapSizeX * this.mapSizeZ];
			this.openList = new FastPriorityQueue<CostNode>(new CostNodeComparer());
			this.regionCostCalculator = new RegionCostCalculatorWrapper(map);
		}

		public PawnPath FindPath(IntVec3 start, LocalTargetInfo dest, Pawn pawn, PathEndMode peMode = PathEndMode.OnCell)
		{
			bool flag = false;
			if (pawn != null && pawn.CurJob != null && pawn.CurJob.canBash)
			{
				flag = true;
			}
			bool canBash = flag;
			return this.FindPath(start, dest, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, canBash), peMode);
		}

		public PawnPath FindPath(IntVec3 start, LocalTargetInfo dest, TraverseParms traverseParms, PathEndMode peMode = PathEndMode.OnCell)
		{
			if (DebugSettings.pathThroughWalls)
			{
				traverseParms.mode = TraverseMode.PassAllDestroyableThings;
			}
			Pawn pawn = traverseParms.pawn;
			if (pawn != null && pawn.Map != this.map)
			{
				Log.Error("Tried to FindPath for pawn which is spawned in another map. His map PathFinder should have been used, not this one. pawn=" + pawn + " pawn.Map=" + pawn.Map + " map=" + this.map);
				return PawnPath.NotFound;
			}
			if (!start.IsValid)
			{
				Log.Error("Tried to FindPath with invalid start " + start + ", pawn= " + pawn);
				return PawnPath.NotFound;
			}
			if (!dest.IsValid)
			{
				Log.Error("Tried to FindPath with invalid dest " + dest + ", pawn= " + pawn);
				return PawnPath.NotFound;
			}
			if (traverseParms.mode == TraverseMode.ByPawn)
			{
				if (!pawn.CanReach(dest, peMode, Danger.Deadly, traverseParms.canBash, traverseParms.mode))
				{
					return PawnPath.NotFound;
				}
			}
			else if (!this.map.reachability.CanReach(start, dest, peMode, traverseParms))
			{
				return PawnPath.NotFound;
			}
			this.PfProfilerBeginSample("FindPath for " + pawn + " from " + start + " to " + dest + ((!dest.HasThing) ? string.Empty : (" at " + dest.Cell)));
			this.cellIndices = this.map.cellIndices;
			this.pathGrid = this.map.pathGrid;
			this.edificeGrid = this.map.edificeGrid.InnerArray;
			IntVec3 cell = dest.Cell;
			int x = cell.x;
			IntVec3 cell2 = dest.Cell;
			int z = cell2.z;
			int num = this.cellIndices.CellToIndex(start);
			int num2 = this.cellIndices.CellToIndex(dest.Cell);
			ByteGrid byteGrid = (pawn == null) ? null : pawn.GetAvoidGrid();
			bool flag = traverseParms.mode == TraverseMode.PassAllDestroyableThings;
			bool flag2 = !flag;
			CellRect cellRect = this.CalculateDestinationRect(dest, peMode);
			bool flag3 = cellRect.Width == 1 && cellRect.Height == 1;
			int[] array = this.map.pathGrid.pathGrid;
			EdificeGrid edificeGrid = this.map.edificeGrid;
			int num3 = 0;
			int num4 = 0;
			Area allowedArea = this.GetAllowedArea(pawn);
			bool flag4 = pawn != null && PawnUtility.ShouldCollideWithPawns(pawn);
			bool flag5 = true && DebugViewSettings.drawPaths;
			bool flag6 = !flag && start.GetRegion(this.map, RegionType.Set_Passable) != null;
			bool flag7 = !flag || !flag2;
			bool flag8 = false;
			int num5 = 0;
			int num6 = 0;
			float num7 = this.DetermineHeuristicStrength(pawn, start, dest);
			int num8;
			int num9;
			if (pawn != null)
			{
				num8 = pawn.TicksPerMoveCardinal;
				num9 = pawn.TicksPerMoveDiagonal;
			}
			else
			{
				num8 = 13;
				num9 = 18;
			}
			this.CalculateAndAddDisallowedCorners(traverseParms, peMode, cellRect);
			this.InitStatusesAndPushStartNode(ref num, start);
			while (true)
			{
				this.PfProfilerBeginSample("Open cell");
				if (this.openList.Count <= 0)
				{
					string text = (pawn == null || pawn.CurJob == null) ? "null" : pawn.CurJob.ToString();
					string text2 = (pawn == null || pawn.Faction == null) ? "null" : pawn.Faction.ToString();
					Log.Warning(pawn + " pathing from " + start + " to " + dest + " ran out of cells to process.\nJob:" + text + "\nFaction: " + text2);
					this.DebugDrawRichData();
					this.PfProfilerEndSample();
					return PawnPath.NotFound;
				}
				num5 += this.openList.Count;
				num6++;
				CostNode costNode = this.openList.Pop();
				num = costNode.index;
				if (costNode.cost != this.calcGrid[num].costNodeCost)
				{
					this.PfProfilerEndSample();
					continue;
				}
				if (this.calcGrid[num].status == this.statusClosedValue)
				{
					this.PfProfilerEndSample();
					continue;
				}
				IntVec3 c = this.cellIndices.IndexToCell(num);
				int x2 = c.x;
				int z2 = c.z;
				if (flag5)
				{
					this.DebugFlash(c, (float)((float)this.calcGrid[num].knownCost / 1500.0), this.calcGrid[num].knownCost.ToString());
				}
				if (flag3)
				{
					if (num == num2)
					{
						this.PfProfilerEndSample();
						return this.FinalizedPath(num);
					}
				}
				else if (cellRect.Contains(c) && !this.disallowedCornerIndices.Contains(num))
				{
					this.PfProfilerEndSample();
					return this.FinalizedPath(num);
				}
				if (num3 <= 160000)
				{
					this.PfProfilerEndSample();
					this.PfProfilerBeginSample("Neighbor consideration");
					for (int i = 0; i < 8; i++)
					{
						uint num10 = (uint)(x2 + PathFinder.Directions[i]);
						uint num11 = (uint)(z2 + PathFinder.Directions[i + 8]);
						int num12;
						int num13;
						int num14;
						bool flag9;
						int num15;
						if (num10 < this.mapSizeX && num11 < this.mapSizeZ)
						{
							num12 = (int)num10;
							num13 = (int)num11;
							num14 = this.cellIndices.CellToIndex(num12, num13);
							if (this.calcGrid[num14].status == this.statusClosedValue && !flag8)
							{
								continue;
							}
							num15 = 0;
							flag9 = false;
							if (!this.pathGrid.WalkableFast(num14))
							{
								if (!flag)
								{
									if (flag5)
									{
										this.DebugFlash(new IntVec3(num12, 0, num13), 0.22f, "walk");
									}
								}
								else
								{
									flag9 = true;
									num15 += 60;
									Building building = edificeGrid[num14];
									if (building != null && PathFinder.IsDestroyable(building))
									{
										num15 += (int)((float)building.HitPoints * 0.10000000149011612);
										goto IL_06d6;
									}
								}
								continue;
							}
							goto IL_06d6;
						}
						continue;
						IL_06d6:
						switch (i)
						{
						case 4:
						{
							if (this.BlocksDiagonalMovement(num - this.mapSizeX))
							{
								if (flag7)
								{
									if (flag5)
									{
										this.DebugFlash(new IntVec3(x2, 0, z2 - 1), 0.9f, "corn");
									}
									continue;
								}
								num15 += 60;
							}
							if (this.BlocksDiagonalMovement(num + 1))
							{
								if (flag7)
								{
									if (flag5)
									{
										this.DebugFlash(new IntVec3(x2 + 1, 0, z2), 0.9f, "corn");
									}
									continue;
								}
								num15 += 60;
							}
							break;
						}
						case 5:
						{
							if (this.BlocksDiagonalMovement(num + this.mapSizeX))
							{
								if (flag7)
								{
									if (flag5)
									{
										this.DebugFlash(new IntVec3(x2, 0, z2 + 1), 0.9f, "corn");
									}
									continue;
								}
								num15 += 60;
							}
							if (this.BlocksDiagonalMovement(num + 1))
							{
								if (flag7)
								{
									if (flag5)
									{
										this.DebugFlash(new IntVec3(x2 + 1, 0, z2), 0.9f, "corn");
									}
									continue;
								}
								num15 += 60;
							}
							break;
						}
						case 6:
						{
							if (this.BlocksDiagonalMovement(num + this.mapSizeX))
							{
								if (flag7)
								{
									if (flag5)
									{
										this.DebugFlash(new IntVec3(x2, 0, z2 + 1), 0.9f, "corn");
									}
									continue;
								}
								num15 += 60;
							}
							if (this.BlocksDiagonalMovement(num - 1))
							{
								if (flag7)
								{
									if (flag5)
									{
										this.DebugFlash(new IntVec3(x2 - 1, 0, z2), 0.9f, "corn");
									}
									continue;
								}
								num15 += 60;
							}
							break;
						}
						case 7:
						{
							if (this.BlocksDiagonalMovement(num - this.mapSizeX))
							{
								if (flag7)
								{
									if (flag5)
									{
										this.DebugFlash(new IntVec3(x2, 0, z2 - 1), 0.9f, "corn");
									}
									continue;
								}
								num15 += 60;
							}
							if (this.BlocksDiagonalMovement(num - 1))
							{
								if (flag7)
								{
									if (flag5)
									{
										this.DebugFlash(new IntVec3(x2 - 1, 0, z2), 0.9f, "corn");
									}
									continue;
								}
								num15 += 60;
							}
							break;
						}
						}
						int num16 = (i <= 3) ? num8 : num9;
						num16 += num15;
						if (!flag9)
						{
							num16 += array[num14];
						}
						if (byteGrid != null)
						{
							num16 += byteGrid[num14] * 8;
						}
						if (allowedArea != null && !allowedArea[num14])
						{
							num16 += 600;
						}
						if (flag4 && PawnUtility.AnyPawnBlockingPathAt(new IntVec3(num12, 0, num13), pawn, false, false))
						{
							num16 += 175;
						}
						Building building2 = this.edificeGrid[num14];
						if (building2 != null)
						{
							this.PfProfilerBeginSample("Edifices");
							int buildingCost = PathFinder.GetBuildingCost(building2, traverseParms, pawn);
							if (buildingCost == 2147483647)
							{
								this.PfProfilerEndSample();
								continue;
							}
							num16 += buildingCost;
							this.PfProfilerEndSample();
						}
						int num17 = num16 + this.calcGrid[num].knownCost;
						ushort status = this.calcGrid[num14].status;
						if (status != this.statusClosedValue && status != this.statusOpenValue)
						{
							goto IL_0a96;
						}
						int num18 = 0;
						if (status == this.statusClosedValue)
						{
							num18 = num8;
						}
						if (this.calcGrid[num14].knownCost > num17 + num18)
							goto IL_0a96;
						continue;
						IL_0a96:
						if (status != this.statusClosedValue && status != this.statusOpenValue)
						{
							if (flag8)
							{
								this.calcGrid[num14].heuristicCost = Mathf.RoundToInt((float)((float)this.regionCostCalculator.GetPathCostFromDestToRegion(num14) * 5.0));
							}
							else
							{
								int dx = Math.Abs(num12 - x);
								int dz = Math.Abs(num13 - z);
								int num19 = GenMath.OctileDistance(dx, dz, num8, num9);
								this.calcGrid[num14].heuristicCost = Mathf.RoundToInt((float)num19 * num7);
							}
						}
						int num20 = num17 + this.calcGrid[num14].heuristicCost;
						this.calcGrid[num14].parentIndex = num;
						this.calcGrid[num14].knownCost = num17;
						this.calcGrid[num14].status = this.statusOpenValue;
						this.calcGrid[num14].costNodeCost = num20;
						num4++;
						this.openList.Push(new CostNode(num14, num20));
					}
					this.PfProfilerEndSample();
					num3++;
					this.calcGrid[num].status = this.statusClosedValue;
					if (num4 >= 2000 && flag6 && !flag8)
					{
						flag8 = true;
						this.regionCostCalculator.Init(cellRect, traverseParms, num8, num9, byteGrid, allowedArea, this.disallowedCornerIndices);
						this.InitStatusesAndPushStartNode(ref num, start);
					}
					continue;
				}
				break;
			}
			Log.Warning(pawn + " pathing from " + start + " to " + dest + " hit search limit of " + 160000 + " cells.");
			this.DebugDrawRichData();
			this.PfProfilerEndSample();
			return PawnPath.NotFound;
		}

		public static int GetBuildingCost(Building b, TraverseParms traverseParms, Pawn pawn)
		{
			Building_Door building_Door = b as Building_Door;
			if (building_Door != null)
			{
				switch (traverseParms.mode)
				{
				case TraverseMode.NoPassClosedDoors:
				{
					if (!building_Door.FreePassage)
					{
						return 2147483647;
					}
					break;
				}
				case TraverseMode.ByPawn:
				{
					if (!traverseParms.canBash && building_Door.IsForbiddenToPass(pawn))
					{
						if (DebugViewSettings.drawPaths)
						{
							PathFinder.DebugFlash(b.Position, b.Map, 0.77f, "forbid");
						}
						return 2147483647;
					}
					if (!building_Door.FreePassage)
					{
						if (building_Door.PawnCanOpen(pawn))
						{
							return building_Door.TicksToOpenNow;
						}
						if (traverseParms.canBash)
						{
							return 300;
						}
						if (DebugViewSettings.drawPaths)
						{
							PathFinder.DebugFlash(b.Position, b.Map, 0.34f, "cant pass");
						}
						return 2147483647;
					}
					break;
				}
				}
			}
			else if (pawn != null)
			{
				return b.PathFindCostFor(pawn);
			}
			return 0;
		}

		public static bool IsDestroyable(Thing th)
		{
			return th.def.useHitPoints && th.def.destroyable;
		}

		private bool BlocksDiagonalMovement(int x, int z)
		{
			return PathFinder.BlocksDiagonalMovement(x, z, this.map);
		}

		private bool BlocksDiagonalMovement(int index)
		{
			return PathFinder.BlocksDiagonalMovement(index, this.map);
		}

		public static bool BlocksDiagonalMovement(int x, int z, Map map)
		{
			return PathFinder.BlocksDiagonalMovement(map.cellIndices.CellToIndex(x, z), map);
		}

		public static bool BlocksDiagonalMovement(int index, Map map)
		{
			if (!map.pathGrid.WalkableFast(index))
			{
				return true;
			}
			if (map.edificeGrid[index] is Building_Door)
			{
				return true;
			}
			return false;
		}

		private void DebugFlash(IntVec3 c, float colorPct, string str)
		{
			PathFinder.DebugFlash(c, this.map, colorPct, str);
		}

		private static void DebugFlash(IntVec3 c, Map map, float colorPct, string str)
		{
			if (DebugViewSettings.drawPaths)
			{
				map.debugDrawer.FlashCell(c, colorPct, str);
			}
		}

		private PawnPath FinalizedPath(int finalIndex)
		{
			PawnPath emptyPawnPath = this.map.pawnPathPool.GetEmptyPawnPath();
			int num = finalIndex;
			while (true)
			{
				PathFinderNodeFast pathFinderNodeFast = this.calcGrid[num];
				int parentIndex = pathFinderNodeFast.parentIndex;
				emptyPawnPath.AddNode(this.map.cellIndices.IndexToCell(num));
				if (num != parentIndex)
				{
					num = parentIndex;
					continue;
				}
				break;
			}
			emptyPawnPath.SetupFound((float)this.calcGrid[finalIndex].knownCost);
			this.PfProfilerEndSample();
			return emptyPawnPath;
		}

		private void InitStatusesAndPushStartNode(ref int curIndex, IntVec3 start)
		{
			this.statusOpenValue = (ushort)(this.statusOpenValue + 2);
			this.statusClosedValue = (ushort)(this.statusClosedValue + 2);
			if (this.statusClosedValue >= 65435)
			{
				this.ResetStatuses();
			}
			curIndex = this.cellIndices.CellToIndex(start);
			this.calcGrid[curIndex].knownCost = 0;
			this.calcGrid[curIndex].heuristicCost = 0;
			this.calcGrid[curIndex].costNodeCost = 0;
			this.calcGrid[curIndex].parentIndex = curIndex;
			this.calcGrid[curIndex].status = this.statusOpenValue;
			this.openList.Clear();
			this.openList.Push(new CostNode(curIndex, 0));
		}

		private void ResetStatuses()
		{
			int num = this.calcGrid.Length;
			for (int num2 = 0; num2 < num; num2++)
			{
				this.calcGrid[num2].status = (ushort)0;
			}
			this.statusOpenValue = (ushort)1;
			this.statusClosedValue = (ushort)2;
		}

		[Conditional("PFPROFILE")]
		private void PfProfilerBeginSample(string s)
		{
			ProfilerThreadCheck.BeginSample(s);
		}

		[Conditional("PFPROFILE")]
		private void PfProfilerEndSample()
		{
			ProfilerThreadCheck.EndSample();
		}

		private void DebugDrawRichData()
		{
			if (DebugViewSettings.drawPaths)
			{
				while (this.openList.Count > 0)
				{
					CostNode costNode = this.openList.Pop();
					int index = costNode.index;
					IntVec3 c = new IntVec3(index % this.mapSizeX, 0, index / this.mapSizeX);
					this.map.debugDrawer.FlashCell(c, 0f, "open");
				}
			}
		}

		private float DetermineHeuristicStrength(Pawn pawn, IntVec3 start, LocalTargetInfo dest)
		{
			if (pawn != null && pawn.RaceProps.Animal)
			{
				return 1.75f;
			}
			float lengthHorizontal = (start - dest.Cell).LengthHorizontal;
			return (float)Mathf.RoundToInt(PathFinder.NonRegionBasedHeuristicStrengthHuman_DistanceCurve.Evaluate(lengthHorizontal));
		}

		private CellRect CalculateDestinationRect(LocalTargetInfo dest, PathEndMode peMode)
		{
			CellRect result = (dest.HasThing && peMode != PathEndMode.OnCell) ? dest.Thing.OccupiedRect() : CellRect.SingleCell(dest.Cell);
			if (peMode == PathEndMode.Touch)
			{
				result = result.ExpandedBy(1);
			}
			return result;
		}

		private Area GetAllowedArea(Pawn pawn)
		{
			if (pawn != null && pawn.playerSettings != null && !pawn.Drafted && ForbidUtility.CaresAboutForbidden(pawn, true))
			{
				Area area = pawn.playerSettings.EffectiveAreaRestrictionInPawnCurrentMap;
				if (area != null && area.TrueCount <= 0)
				{
					area = null;
				}
				return area;
			}
			return null;
		}

		private void CalculateAndAddDisallowedCorners(TraverseParms traverseParms, PathEndMode peMode, CellRect destinationRect)
		{
			this.disallowedCornerIndices.Clear();
			if (peMode == PathEndMode.Touch)
			{
				int minX = destinationRect.minX;
				int minZ = destinationRect.minZ;
				int maxX = destinationRect.maxX;
				int maxZ = destinationRect.maxZ;
				if (!this.IsCornerTouchAllowed(minX + 1, minZ + 1, minX + 1, minZ, minX, minZ + 1))
				{
					this.disallowedCornerIndices.Add(this.map.cellIndices.CellToIndex(minX, minZ));
				}
				if (!this.IsCornerTouchAllowed(minX + 1, maxZ - 1, minX + 1, maxZ, minX, maxZ - 1))
				{
					this.disallowedCornerIndices.Add(this.map.cellIndices.CellToIndex(minX, maxZ));
				}
				if (!this.IsCornerTouchAllowed(maxX - 1, maxZ - 1, maxX - 1, maxZ, maxX, maxZ - 1))
				{
					this.disallowedCornerIndices.Add(this.map.cellIndices.CellToIndex(maxX, maxZ));
				}
				if (!this.IsCornerTouchAllowed(maxX - 1, minZ + 1, maxX - 1, minZ, maxX, minZ + 1))
				{
					this.disallowedCornerIndices.Add(this.map.cellIndices.CellToIndex(maxX, minZ));
				}
			}
		}

		private bool IsCornerTouchAllowed(int cornerX, int cornerZ, int adjCardinal1X, int adjCardinal1Z, int adjCardinal2X, int adjCardinal2Z)
		{
			return TouchPathEndModeUtility.IsCornerTouchAllowed(cornerX, cornerZ, adjCardinal1X, adjCardinal1Z, adjCardinal2X, adjCardinal2Z, this.map);
		}
	}
}
