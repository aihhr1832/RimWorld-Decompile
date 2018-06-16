﻿using System;
using Verse;

namespace RimWorld
{
	// Token: 0x020001F3 RID: 499
	public class ThoughtWorker_HospitalPatientRoomStats : ThoughtWorker
	{
		// Token: 0x060009B1 RID: 2481 RVA: 0x0005741C File Offset: 0x0005581C
		protected override ThoughtState CurrentStateInternal(Pawn p)
		{
			Building_Bed building_Bed = p.CurrentBed();
			ThoughtState result;
			if (building_Bed == null || !building_Bed.Medical)
			{
				result = ThoughtState.Inactive;
			}
			else
			{
				Room room = p.GetRoom(RegionType.Set_Passable);
				if (room == null || room.Role != RoomRoleDefOf.Hospital)
				{
					result = ThoughtState.Inactive;
				}
				else
				{
					int scoreStageIndex = RoomStatDefOf.Impressiveness.GetScoreStageIndex(room.GetStat(RoomStatDefOf.Impressiveness));
					if (this.def.stages[scoreStageIndex] != null)
					{
						result = ThoughtState.ActiveAtStage(scoreStageIndex);
					}
					else
					{
						result = ThoughtState.Inactive;
					}
				}
			}
			return result;
		}
	}
}
