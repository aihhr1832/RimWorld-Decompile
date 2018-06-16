﻿using System;
using Verse;

namespace RimWorld
{
	// Token: 0x02000208 RID: 520
	public class ThoughtWorker_AnnoyingVoice : ThoughtWorker
	{
		// Token: 0x060009DE RID: 2526 RVA: 0x0005865C File Offset: 0x00056A5C
		protected override ThoughtState CurrentSocialStateInternal(Pawn pawn, Pawn other)
		{
			ThoughtState result;
			if (!other.RaceProps.Humanlike || !RelationsUtility.PawnsKnowEachOther(pawn, other))
			{
				result = false;
			}
			else if (!other.story.traits.HasTrait(TraitDefOf.AnnoyingVoice))
			{
				result = false;
			}
			else if (!pawn.health.capacities.CapableOf(PawnCapacityDefOf.Hearing))
			{
				result = false;
			}
			else
			{
				result = true;
			}
			return result;
		}
	}
}
