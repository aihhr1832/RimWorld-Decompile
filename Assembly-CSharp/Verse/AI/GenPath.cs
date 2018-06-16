﻿using System;
using RimWorld;

namespace Verse.AI
{
	// Token: 0x02000A90 RID: 2704
	public static class GenPath
	{
		// Token: 0x06003BFC RID: 15356 RVA: 0x001FA7E4 File Offset: 0x001F8BE4
		public static TargetInfo ResolvePathMode(Pawn pawn, TargetInfo dest, ref PathEndMode peMode)
		{
			TargetInfo result;
			if (dest.HasThing && !dest.Thing.Spawned)
			{
				peMode = PathEndMode.Touch;
				result = dest;
			}
			else if (peMode == PathEndMode.InteractionCell)
			{
				if (!dest.HasThing)
				{
					Log.Error("Pathed to cell " + dest + " with PathEndMode.InteractionCell.", false);
				}
				peMode = PathEndMode.OnCell;
				result = new TargetInfo(dest.Thing.InteractionCell, dest.Thing.Map, false);
			}
			else
			{
				if (peMode == PathEndMode.ClosestTouch)
				{
					peMode = GenPath.ResolveClosestTouchPathMode(pawn, dest.Map, dest.Cell);
				}
				result = dest;
			}
			return result;
		}

		// Token: 0x06003BFD RID: 15357 RVA: 0x001FA898 File Offset: 0x001F8C98
		public static PathEndMode ResolveClosestTouchPathMode(Pawn pawn, Map map, IntVec3 target)
		{
			PathEndMode result;
			if (GenPath.ShouldNotEnterCell(pawn, map, target))
			{
				result = PathEndMode.Touch;
			}
			else
			{
				result = PathEndMode.OnCell;
			}
			return result;
		}

		// Token: 0x06003BFE RID: 15358 RVA: 0x001FA8C4 File Offset: 0x001F8CC4
		private static bool ShouldNotEnterCell(Pawn pawn, Map map, IntVec3 dest)
		{
			bool result;
			if (map.pathGrid.PerceivedPathCostAt(dest) > 30)
			{
				result = true;
			}
			else if (!dest.Walkable(map))
			{
				result = true;
			}
			else
			{
				if (pawn != null)
				{
					if (dest.IsForbidden(pawn))
					{
						return true;
					}
					Building edifice = dest.GetEdifice(map);
					if (edifice != null)
					{
						Building_Door building_Door = edifice as Building_Door;
						if (building_Door != null)
						{
							if (building_Door.IsForbidden(pawn))
							{
								return true;
							}
							if (!building_Door.PawnCanOpen(pawn))
							{
								return true;
							}
						}
					}
				}
				result = false;
			}
			return result;
		}
	}
}
