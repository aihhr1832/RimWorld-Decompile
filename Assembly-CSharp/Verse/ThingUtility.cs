﻿using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using RimWorld.Planet;
using UnityEngine;
using Verse.AI;

namespace Verse
{
	public static class ThingUtility
	{
		[CompilerGenerated]
		private static Func<Tool, float> <>f__am$cache0;

		public static bool DestroyedOrNull(this Thing t)
		{
			return t == null || t.Destroyed;
		}

		public static void DestroyOrPassToWorld(this Thing t, DestroyMode mode = DestroyMode.Vanish)
		{
			Pawn pawn = t as Pawn;
			if (pawn != null)
			{
				if (!Find.WorldPawns.Contains(pawn))
				{
					Find.WorldPawns.PassToWorld(pawn, PawnDiscardDecideMode.Decide);
				}
			}
			else
			{
				t.Destroy(mode);
			}
		}

		public static int TryAbsorbStackNumToTake(Thing thing, Thing other, bool respectStackLimit)
		{
			int result;
			if (respectStackLimit)
			{
				result = Mathf.Min(other.stackCount, thing.def.stackLimit - thing.stackCount);
			}
			else
			{
				result = other.stackCount;
			}
			return result;
		}

		public static int RoundedResourceStackCount(int stackCount)
		{
			int result;
			if (stackCount > 200)
			{
				result = GenMath.RoundTo(stackCount, 10);
			}
			else if (stackCount > 100)
			{
				result = GenMath.RoundTo(stackCount, 5);
			}
			else
			{
				result = stackCount;
			}
			return result;
		}

		public static IntVec3 InteractionCellWhenAt(ThingDef def, IntVec3 center, Rot4 rot, Map map)
		{
			IntVec3 result;
			if (def.hasInteractionCell)
			{
				IntVec3 b = def.interactionCellOffset.RotatedBy(rot);
				result = center + b;
			}
			else if (def.Size.x == 1 && def.Size.z == 1)
			{
				for (int i = 0; i < 8; i++)
				{
					IntVec3 intVec = center + GenAdj.AdjacentCells[i];
					if (intVec.Standable(map) && intVec.GetDoor(map) == null && ReachabilityImmediate.CanReachImmediate(intVec, center, map, PathEndMode.Touch, null))
					{
						return intVec;
					}
				}
				for (int j = 0; j < 8; j++)
				{
					IntVec3 intVec2 = center + GenAdj.AdjacentCells[j];
					if (intVec2.Standable(map) && ReachabilityImmediate.CanReachImmediate(intVec2, center, map, PathEndMode.Touch, null))
					{
						return intVec2;
					}
				}
				for (int k = 0; k < 8; k++)
				{
					IntVec3 intVec3 = center + GenAdj.AdjacentCells[k];
					if (intVec3.Walkable(map) && ReachabilityImmediate.CanReachImmediate(intVec3, center, map, PathEndMode.Touch, null))
					{
						return intVec3;
					}
				}
				result = center;
			}
			else
			{
				List<IntVec3> list = GenAdjFast.AdjacentCells8Way(center, rot, def.size);
				CellRect rect = GenAdj.OccupiedRect(center, rot, def.size);
				for (int l = 0; l < list.Count; l++)
				{
					if (list[l].Standable(map) && list[l].GetDoor(map) == null && ReachabilityImmediate.CanReachImmediate(list[l], rect, map, PathEndMode.Touch, null))
					{
						return list[l];
					}
				}
				for (int m = 0; m < list.Count; m++)
				{
					if (list[m].Standable(map) && ReachabilityImmediate.CanReachImmediate(list[m], rect, map, PathEndMode.Touch, null))
					{
						return list[m];
					}
				}
				for (int n = 0; n < list.Count; n++)
				{
					if (list[n].Walkable(map) && ReachabilityImmediate.CanReachImmediate(list[n], rect, map, PathEndMode.Touch, null))
					{
						return list[n];
					}
				}
				result = center;
			}
			return result;
		}

		public static DamageDef PrimaryMeleeWeaponDamageType(ThingDef thing)
		{
			return ThingUtility.PrimaryMeleeWeaponDamageType(thing.tools);
		}

		public static DamageDef PrimaryMeleeWeaponDamageType(List<Tool> tools)
		{
			DamageDef result;
			if (tools.NullOrEmpty<Tool>())
			{
				result = null;
			}
			else
			{
				Tool tool2 = tools.MaxBy((Tool tool) => tool.power);
				List<ManeuverDef> allDefsListForReading = DefDatabase<ManeuverDef>.AllDefsListForReading;
				for (int i = 0; i < allDefsListForReading.Count; i++)
				{
					ManeuverDef maneuverDef = allDefsListForReading[i];
					if (tool2.capacities.Contains(maneuverDef.requiredCapacity))
					{
						return maneuverDef.verb.meleeDamageDef;
					}
				}
				result = null;
			}
			return result;
		}

		[CompilerGenerated]
		private static float <PrimaryMeleeWeaponDamageType>m__0(Tool tool)
		{
			return tool.power;
		}
	}
}
