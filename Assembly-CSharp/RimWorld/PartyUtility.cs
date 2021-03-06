﻿using System;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld
{
	public static class PartyUtility
	{
		private const float PartyAreaRadiusIfNotWholeRoom = 10f;

		private const int MaxRoomCellsCountToUseWholeRoom = 324;

		[CompilerGenerated]
		private static Predicate<Pawn> <>f__am$cache0;

		public static bool AcceptableGameConditionsToStartParty(Map map)
		{
			bool result;
			if (!PartyUtility.AcceptableGameConditionsToContinueParty(map))
			{
				result = false;
			}
			else if (GenLocalDate.HourInteger(map) < 4 || GenLocalDate.HourInteger(map) > 21)
			{
				result = false;
			}
			else if (GatheringsUtility.AnyLordJobPreventsNewGatherings(map))
			{
				result = false;
			}
			else if (map.dangerWatcher.DangerRating != StoryDanger.None)
			{
				result = false;
			}
			else
			{
				int freeColonistsSpawnedCount = map.mapPawns.FreeColonistsSpawnedCount;
				if (freeColonistsSpawnedCount < 4)
				{
					result = false;
				}
				else
				{
					int num = 0;
					foreach (Pawn pawn in map.mapPawns.FreeColonistsSpawned)
					{
						if (pawn.health.hediffSet.BleedRateTotal > 0f)
						{
							return false;
						}
						if (pawn.Drafted)
						{
							num++;
						}
					}
					result = ((float)num / (float)freeColonistsSpawnedCount < 0.5f && PartyUtility.EnoughPotentialGuestsToStartParty(map, null));
				}
			}
			return result;
		}

		public static bool AcceptableGameConditionsToContinueParty(Map map)
		{
			return map.dangerWatcher.DangerRating != StoryDanger.High;
		}

		public static bool EnoughPotentialGuestsToStartParty(Map map, IntVec3? partySpot = null)
		{
			int num = Mathf.RoundToInt((float)map.mapPawns.FreeColonistsSpawnedCount * 0.65f);
			num = Mathf.Clamp(num, 2, 10);
			int num2 = 0;
			foreach (Pawn pawn in map.mapPawns.FreeColonistsSpawned)
			{
				if (PartyUtility.ShouldPawnKeepPartying(pawn))
				{
					if (partySpot == null || !partySpot.Value.IsForbidden(pawn))
					{
						if (partySpot == null || pawn.CanReach(partySpot.Value, PathEndMode.Touch, Danger.Some, false, TraverseMode.ByPawn))
						{
							num2++;
						}
					}
				}
			}
			return num2 >= num;
		}

		public static Pawn FindRandomPartyOrganizer(Faction faction, Map map)
		{
			Predicate<Pawn> validator = (Pawn x) => x.RaceProps.Humanlike && !x.InBed() && !x.InMentalState && x.GetLord() == null && PartyUtility.ShouldPawnKeepPartying(x);
			Pawn pawn;
			Pawn result;
			if ((from x in map.mapPawns.SpawnedPawnsInFaction(faction)
			where validator(x)
			select x).TryRandomElement(out pawn))
			{
				result = pawn;
			}
			else
			{
				result = null;
			}
			return result;
		}

		public static bool ShouldPawnKeepPartying(Pawn p)
		{
			return (p.timetable == null || p.timetable.CurrentAssignment.allowJoy) && GatheringsUtility.ShouldGuestKeepAttendingGathering(p);
		}

		public static bool InPartyArea(IntVec3 cell, IntVec3 partySpot, Map map)
		{
			bool result;
			if (PartyUtility.UseWholeRoomAsPartyArea(partySpot, map) && cell.GetRoom(map, RegionType.Set_Passable) == partySpot.GetRoom(map, RegionType.Set_Passable))
			{
				result = true;
			}
			else if (cell.InHorDistOf(partySpot, 10f))
			{
				Building edifice = cell.GetEdifice(map);
				TraverseParms traverseParams = TraverseParms.For(TraverseMode.NoPassClosedDoors, Danger.None, false);
				if (edifice != null)
				{
					result = map.reachability.CanReach(partySpot, edifice, PathEndMode.ClosestTouch, traverseParams);
				}
				else
				{
					result = map.reachability.CanReach(partySpot, cell, PathEndMode.ClosestTouch, traverseParams);
				}
			}
			else
			{
				result = false;
			}
			return result;
		}

		public static bool TryFindRandomCellInPartyArea(Pawn pawn, out IntVec3 result)
		{
			IntVec3 cell = pawn.mindState.duty.focus.Cell;
			Predicate<IntVec3> validator = (IntVec3 x) => x.Standable(pawn.Map) && !x.IsForbidden(pawn) && pawn.CanReserveAndReach(x, PathEndMode.OnCell, Danger.None, 1, -1, null, false);
			bool result2;
			if (PartyUtility.UseWholeRoomAsPartyArea(cell, pawn.Map))
			{
				Room room = cell.GetRoom(pawn.Map, RegionType.Set_Passable);
				result2 = (from x in room.Cells
				where validator(x)
				select x).TryRandomElement(out result);
			}
			else
			{
				result2 = CellFinder.TryFindRandomReachableCellNear(cell, pawn.Map, 10f, TraverseParms.For(TraverseMode.NoPassClosedDoors, Danger.Deadly, false), (IntVec3 x) => validator(x), null, out result, 10);
			}
			return result2;
		}

		public static bool UseWholeRoomAsPartyArea(IntVec3 partySpot, Map map)
		{
			Room room = partySpot.GetRoom(map, RegionType.Set_Passable);
			return room != null && !room.IsHuge && !room.PsychologicallyOutdoors && room.CellCount <= 324;
		}

		[CompilerGenerated]
		private static bool <FindRandomPartyOrganizer>m__0(Pawn x)
		{
			return x.RaceProps.Humanlike && !x.InBed() && !x.InMentalState && x.GetLord() == null && PartyUtility.ShouldPawnKeepPartying(x);
		}

		[CompilerGenerated]
		private sealed class <FindRandomPartyOrganizer>c__AnonStorey0
		{
			internal Predicate<Pawn> validator;

			public <FindRandomPartyOrganizer>c__AnonStorey0()
			{
			}

			internal bool <>m__0(Pawn x)
			{
				return this.validator(x);
			}
		}

		[CompilerGenerated]
		private sealed class <TryFindRandomCellInPartyArea>c__AnonStorey1
		{
			internal Pawn pawn;

			internal Predicate<IntVec3> validator;

			public <TryFindRandomCellInPartyArea>c__AnonStorey1()
			{
			}

			internal bool <>m__0(IntVec3 x)
			{
				return x.Standable(this.pawn.Map) && !x.IsForbidden(this.pawn) && this.pawn.CanReserveAndReach(x, PathEndMode.OnCell, Danger.None, 1, -1, null, false);
			}

			internal bool <>m__1(IntVec3 x)
			{
				return this.validator(x);
			}

			internal bool <>m__2(IntVec3 x)
			{
				return this.validator(x);
			}
		}
	}
}
