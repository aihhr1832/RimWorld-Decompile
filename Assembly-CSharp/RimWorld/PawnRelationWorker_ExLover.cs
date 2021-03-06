﻿using System;
using Verse;

namespace RimWorld
{
	public class PawnRelationWorker_ExLover : PawnRelationWorker
	{
		public PawnRelationWorker_ExLover()
		{
		}

		public override float GenerationChance(Pawn generated, Pawn other, PawnGenerationRequest request)
		{
			return LovePartnerRelationUtility.LovePartnerRelationGenerationChance(generated, other, request, true) * base.BaseGenerationChanceFactor(generated, other, request);
		}

		public override void CreateRelation(Pawn generated, Pawn other, ref PawnGenerationRequest request)
		{
			generated.relations.AddDirectRelation(PawnRelationDefOf.ExLover, other);
			LovePartnerRelationUtility.TryToShareChildrenForGeneratedLovePartner(generated, other, request, 0.35f);
			PawnRelationWorker_ExLover.ResolveMySkinColor(ref request, generated, other);
		}

		private static void ResolveMySkinColor(ref PawnGenerationRequest request, Pawn generated, Pawn other)
		{
			if (request.FixedMelanin == null)
			{
				request.SetFixedMelanin(PawnSkinColors.GetRandomMelaninSimilarTo(other.story.melanin, 0f, 1f));
			}
		}
	}
}
