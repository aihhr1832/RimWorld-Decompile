﻿using System;
using Verse;

namespace RimWorld
{
	// Token: 0x020001F2 RID: 498
	public class ThoughtWorker_PrisonBarracksImpressiveness : ThoughtWorker_RoomImpressiveness
	{
		// Token: 0x060009AF RID: 2479 RVA: 0x000573B0 File Offset: 0x000557B0
		protected override ThoughtState CurrentStateInternal(Pawn p)
		{
			ThoughtState result;
			if (!p.IsPrisoner)
			{
				result = ThoughtState.Inactive;
			}
			else
			{
				ThoughtState thoughtState = base.CurrentStateInternal(p);
				if (thoughtState.Active && p.GetRoom(RegionType.Set_Passable).Role == RoomRoleDefOf.PrisonBarracks)
				{
					result = thoughtState;
				}
				else
				{
					result = ThoughtState.Inactive;
				}
			}
			return result;
		}
	}
}
