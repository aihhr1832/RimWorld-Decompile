﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	// Token: 0x02000C75 RID: 3189
	public class PlaceWorker_Cooler : PlaceWorker
	{
		// Token: 0x060045E0 RID: 17888 RVA: 0x0024CAB8 File Offset: 0x0024AEB8
		public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol)
		{
			Map currentMap = Find.CurrentMap;
			IntVec3 intVec = center + IntVec3.South.RotatedBy(rot);
			IntVec3 intVec2 = center + IntVec3.North.RotatedBy(rot);
			GenDraw.DrawFieldEdges(new List<IntVec3>
			{
				intVec
			}, GenTemperature.ColorSpotCold);
			GenDraw.DrawFieldEdges(new List<IntVec3>
			{
				intVec2
			}, GenTemperature.ColorSpotHot);
			RoomGroup roomGroup = intVec2.GetRoomGroup(currentMap);
			RoomGroup roomGroup2 = intVec.GetRoomGroup(currentMap);
			if (roomGroup != null && roomGroup2 != null)
			{
				if (roomGroup == roomGroup2 && !roomGroup.UsesOutdoorTemperature)
				{
					GenDraw.DrawFieldEdges(roomGroup.Cells.ToList<IntVec3>(), new Color(1f, 0.7f, 0f, 0.5f));
				}
				else
				{
					if (!roomGroup.UsesOutdoorTemperature)
					{
						GenDraw.DrawFieldEdges(roomGroup.Cells.ToList<IntVec3>(), GenTemperature.ColorRoomHot);
					}
					if (!roomGroup2.UsesOutdoorTemperature)
					{
						GenDraw.DrawFieldEdges(roomGroup2.Cells.ToList<IntVec3>(), GenTemperature.ColorRoomCold);
					}
				}
			}
		}

		// Token: 0x060045E1 RID: 17889 RVA: 0x0024CBD0 File Offset: 0x0024AFD0
		public override AcceptanceReport AllowsPlacing(BuildableDef def, IntVec3 center, Rot4 rot, Map map, Thing thingToIgnore = null)
		{
			IntVec3 c = center + IntVec3.South.RotatedBy(rot);
			IntVec3 c2 = center + IntVec3.North.RotatedBy(rot);
			AcceptanceReport result;
			if (c.Impassable(map) || c2.Impassable(map))
			{
				result = "MustPlaceCoolerWithFreeSpaces".Translate();
			}
			else
			{
				result = true;
			}
			return result;
		}
	}
}
