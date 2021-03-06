﻿using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class PlaceWorker_NeedsFuelingPort : PlaceWorker
	{
		public PlaceWorker_NeedsFuelingPort()
		{
		}

		public override AcceptanceReport AllowsPlacing(BuildableDef def, IntVec3 center, Rot4 rot, Map map, Thing thingToIgnore = null)
		{
			AcceptanceReport result;
			if (FuelingPortUtility.FuelingPortGiverAtFuelingPortCell(center, map) == null)
			{
				result = "MustPlaceNearFuelingPort".Translate();
			}
			else
			{
				result = true;
			}
			return result;
		}

		public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol)
		{
			Map currentMap = Find.CurrentMap;
			List<Building> allBuildingsColonist = currentMap.listerBuildings.allBuildingsColonist;
			for (int i = 0; i < allBuildingsColonist.Count; i++)
			{
				Building building = allBuildingsColonist[i];
				if (building.def.building.hasFuelingPort && !Find.Selector.IsSelected(building) && FuelingPortUtility.GetFuelingPortCell(building).Standable(currentMap))
				{
					PlaceWorker_FuelingPort.DrawFuelingPortCell(building.Position, building.Rotation);
				}
			}
		}
	}
}
