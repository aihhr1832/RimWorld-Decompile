﻿using System;
using Verse;
using Verse.AI;

namespace RimWorld
{
	// Token: 0x020001E5 RID: 485
	public class ThinkNode_ConditionalAnyColonistTryingToExitMap : ThinkNode_Conditional
	{
		// Token: 0x06000986 RID: 2438 RVA: 0x00056A74 File Offset: 0x00054E74
		protected override bool Satisfied(Pawn pawn)
		{
			Map mapHeld = pawn.MapHeld;
			bool result;
			if (mapHeld == null)
			{
				result = false;
			}
			else
			{
				foreach (Pawn pawn2 in mapHeld.mapPawns.FreeColonistsSpawned)
				{
					Job curJob = pawn2.CurJob;
					if (curJob != null && curJob.exitMapOnArrival)
					{
						return true;
					}
				}
				result = false;
			}
			return result;
		}
	}
}
