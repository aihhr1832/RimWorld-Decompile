﻿using System;
using System.Collections.Generic;
using UnityEngine;
using Verse.AI;

namespace Verse
{
	// Token: 0x02000F2C RID: 3884
	public static class CellFinderLoose
	{
		// Token: 0x06005D2D RID: 23853 RVA: 0x002F2CFC File Offset: 0x002F10FC
		public static IntVec3 RandomCellWith(Predicate<IntVec3> validator, Map map, int maxTries = 1000)
		{
			IntVec3 result;
			CellFinderLoose.TryGetRandomCellWith(validator, map, maxTries, out result);
			return result;
		}

		// Token: 0x06005D2E RID: 23854 RVA: 0x002F2D20 File Offset: 0x002F1120
		public static bool TryGetRandomCellWith(Predicate<IntVec3> validator, Map map, int maxTries, out IntVec3 result)
		{
			for (int i = 0; i < maxTries; i++)
			{
				result = CellFinder.RandomCell(map);
				if (validator(result))
				{
					return true;
				}
			}
			result = IntVec3.Invalid;
			return false;
		}

		// Token: 0x06005D2F RID: 23855 RVA: 0x002F2D7C File Offset: 0x002F117C
		public static bool TryFindRandomNotEdgeCellWith(int minEdgeDistance, Predicate<IntVec3> validator, Map map, out IntVec3 result)
		{
			for (int i = 0; i < 1000; i++)
			{
				result = CellFinder.RandomNotEdgeCell(minEdgeDistance, map);
				if (result.IsValid && validator(result))
				{
					return true;
				}
			}
			result = IntVec3.Invalid;
			return false;
		}

		// Token: 0x06005D30 RID: 23856 RVA: 0x002F2DE8 File Offset: 0x002F11E8
		public static IntVec3 GetFleeDest(Pawn pawn, List<Thing> threats, float distance = 23f)
		{
			IntVec3 result;
			if (pawn.RaceProps.Animal)
			{
				result = CellFinderLoose.GetFleeDestAnimal(pawn, threats, distance);
			}
			else
			{
				result = CellFinderLoose.GetFleeDestToolUser(pawn, threats, distance);
			}
			return result;
		}

		// Token: 0x06005D31 RID: 23857 RVA: 0x002F2E24 File Offset: 0x002F1224
		public static IntVec3 GetFleeDestAnimal(Pawn pawn, List<Thing> threats, float distance = 23f)
		{
			Vector3 normalized = (pawn.Position - threats[0].Position).ToVector3().normalized;
			float num = distance - pawn.Position.DistanceTo(threats[0].Position);
			for (float num2 = 200f; num2 <= 360f; num2 += 10f)
			{
				IntVec3 intVec = pawn.Position + (normalized.RotatedBy(Rand.Range(-num2 / 2f, num2 / 2f)) * num).ToIntVec3();
				if (CellFinderLoose.CanFleeToLocation(pawn, intVec))
				{
					return intVec;
				}
			}
			float num3 = num;
			while (num3 * 3f > num)
			{
				IntVec3 intVec2 = pawn.Position + IntVec3Utility.RandomHorizontalOffset(num3);
				if (CellFinderLoose.CanFleeToLocation(pawn, intVec2))
				{
					return intVec2;
				}
				num3 -= distance / 10f;
			}
			return pawn.Position;
		}

		// Token: 0x06005D32 RID: 23858 RVA: 0x002F2F44 File Offset: 0x002F1344
		public static bool CanFleeToLocation(Pawn pawn, IntVec3 location)
		{
			bool result;
			if (!location.Standable(pawn.Map))
			{
				result = false;
			}
			else if (!pawn.Map.pawnDestinationReservationManager.CanReserve(location, pawn, false))
			{
				result = false;
			}
			else
			{
				Region region = location.GetRegion(pawn.Map, RegionType.Set_Passable);
				result = (region.type != RegionType.Portal && pawn.CanReach(location, PathEndMode.OnCell, Danger.Deadly, false, TraverseMode.ByPawn));
			}
			return result;
		}

		// Token: 0x06005D33 RID: 23859 RVA: 0x002F2FD0 File Offset: 0x002F13D0
		public static IntVec3 GetFleeDestToolUser(Pawn pawn, List<Thing> threats, float distance = 23f)
		{
			IntVec3 bestPos = pawn.Position;
			float bestScore = -1f;
			TraverseParms traverseParms = TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false);
			RegionTraverser.BreadthFirstTraverse(pawn.GetRegion(RegionType.Set_Passable), (Region from, Region reg) => reg.Allows(traverseParms, false), delegate(Region reg)
			{
				Danger danger = reg.DangerFor(pawn);
				Map map = pawn.Map;
				foreach (IntVec3 intVec in reg.Cells)
				{
					if (intVec.Standable(map))
					{
						if (reg.portal == null)
						{
							Thing thing = null;
							float num = 0f;
							for (int i = 0; i < threats.Count; i++)
							{
								float num2 = (float)intVec.DistanceToSquared(threats[i].Position);
								if (thing == null || num2 < num)
								{
									thing = threats[i];
									num = num2;
								}
							}
							float num3 = Mathf.Sqrt(num);
							float f = Mathf.Min(num3, distance);
							float num4 = Mathf.Pow(f, 1.2f);
							num4 *= Mathf.InverseLerp(50f, 0f, (intVec - pawn.Position).LengthHorizontal);
							if (intVec.GetRoom(map, RegionType.Set_Passable) != thing.GetRoom(RegionType.Set_Passable))
							{
								num4 *= 4.2f;
							}
							else if (num3 < 8f)
							{
								num4 *= 0.05f;
							}
							if (!map.pawnDestinationReservationManager.CanReserve(intVec, pawn, false))
							{
								num4 *= 0.5f;
							}
							if (danger == Danger.Deadly)
							{
								num4 *= 0.8f;
							}
							if (num4 > bestScore)
							{
								bestPos = intVec;
								bestScore = num4;
							}
						}
					}
				}
				return false;
			}, 20, RegionType.Set_Passable);
			return bestPos;
		}

		// Token: 0x06005D34 RID: 23860 RVA: 0x002F3064 File Offset: 0x002F1464
		public static IntVec3 TryFindCentralCell(Map map, int tightness, int minCellCount, Predicate<IntVec3> extraValidator = null)
		{
			int debug_numStand = 0;
			int debug_numRoom = 0;
			int debug_numTouch = 0;
			int debug_numRoomCellCount = 0;
			int debug_numExtraValidator = 0;
			Predicate<IntVec3> validator = delegate(IntVec3 c)
			{
				bool result2;
				if (!c.Standable(map))
				{
					debug_numStand++;
					result2 = false;
				}
				else
				{
					Room room = c.GetRoom(map, RegionType.Set_Passable);
					if (room == null)
					{
						debug_numRoom++;
						result2 = false;
					}
					else if (!room.TouchesMapEdge)
					{
						debug_numTouch++;
						result2 = false;
					}
					else if (room.CellCount < minCellCount)
					{
						debug_numRoomCellCount++;
						result2 = false;
					}
					else if (extraValidator != null && !extraValidator(c))
					{
						debug_numExtraValidator++;
						result2 = false;
					}
					else
					{
						result2 = true;
					}
				}
				return result2;
			};
			for (int i = tightness; i >= 1; i--)
			{
				int num = map.Size.x / i;
				int minEdgeDistance = (map.Size.x - num) / 2;
				IntVec3 result;
				if (CellFinderLoose.TryFindRandomNotEdgeCellWith(minEdgeDistance, validator, map, out result))
				{
					return result;
				}
			}
			Log.Error(string.Concat(new object[]
			{
				"Found no good central spot. Choosing randomly. numStand=",
				debug_numStand,
				", numRoom=",
				debug_numRoom,
				", numTouch=",
				debug_numTouch,
				", numRoomCellCount=",
				debug_numRoomCellCount,
				", numExtraValidator=",
				debug_numExtraValidator
			}), false);
			return CellFinderLoose.RandomCellWith((IntVec3 x) => x.Standable(map), map, 1000);
		}

		// Token: 0x06005D35 RID: 23861 RVA: 0x002F31C8 File Offset: 0x002F15C8
		public static bool TryFindSkyfallerCell(ThingDef skyfaller, Map map, out IntVec3 cell, int minDistToEdge = 10, IntVec3 nearLoc = default(IntVec3), int nearLocMaxDist = -1, bool allowRoofedCells = true, bool allowCellsWithItems = false, bool allowCellsWithBuildings = false, bool colonyReachable = false, bool avoidColonistsIfExplosive = true, bool alwaysAvoidColonists = false, Predicate<IntVec3> extraValidator = null)
		{
			bool avoidColonists = (avoidColonistsIfExplosive && skyfaller.skyfaller.CausesExplosion) || alwaysAvoidColonists;
			Predicate<IntVec3> validator = delegate(IntVec3 x)
			{
				CellRect.CellRectIterator iterator = GenAdj.OccupiedRect(x, Rot4.North, skyfaller.size).GetIterator();
				while (!iterator.Done())
				{
					IntVec3 c = iterator.Current;
					bool result2;
					if (!c.InBounds(map) || c.Fogged(map) || !c.Standable(map) || (c.Roofed(map) && c.GetRoof(map).isThickRoof))
					{
						result2 = false;
					}
					else if (!allowRoofedCells && c.Roofed(map))
					{
						result2 = false;
					}
					else if (!allowCellsWithItems && c.GetFirstItem(map) != null)
					{
						result2 = false;
					}
					else if (!allowCellsWithBuildings && c.GetFirstBuilding(map) != null)
					{
						result2 = false;
					}
					else
					{
						if (c.GetFirstSkyfaller(map) == null)
						{
							iterator.MoveNext();
							continue;
						}
						result2 = false;
					}
					return result2;
				}
				return (!avoidColonists || !SkyfallerUtility.CanPossiblyFallOnColonist(skyfaller, x, map)) && (minDistToEdge <= 0 || x.DistanceToEdge(map) >= minDistToEdge) && (!colonyReachable || map.reachability.CanReachColony(x)) && (extraValidator == null || extraValidator(x));
			};
			bool result;
			if (nearLocMaxDist > 0)
			{
				result = CellFinder.TryFindRandomCellNear(nearLoc, map, nearLocMaxDist, validator, out cell, -1);
			}
			else
			{
				result = CellFinderLoose.TryFindRandomNotEdgeCellWith(minDistToEdge, validator, map, out cell);
			}
			return result;
		}
	}
}
