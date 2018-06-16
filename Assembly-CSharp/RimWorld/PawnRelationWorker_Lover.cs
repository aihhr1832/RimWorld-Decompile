﻿using System;
using Verse;

namespace RimWorld
{
	// Token: 0x020004D1 RID: 1233
	public class PawnRelationWorker_Lover : PawnRelationWorker
	{
		// Token: 0x060015F5 RID: 5621 RVA: 0x000C2B64 File Offset: 0x000C0F64
		public override float GenerationChance(Pawn generated, Pawn other, PawnGenerationRequest request)
		{
			return LovePartnerRelationUtility.LovePartnerRelationGenerationChance(generated, other, request, false) * base.BaseGenerationChanceFactor(generated, other, request);
		}

		// Token: 0x060015F6 RID: 5622 RVA: 0x000C2B8C File Offset: 0x000C0F8C
		public override void CreateRelation(Pawn generated, Pawn other, ref PawnGenerationRequest request)
		{
			generated.relations.AddDirectRelation(PawnRelationDefOf.Lover, other);
			LovePartnerRelationUtility.TryToShareChildrenForGeneratedLovePartner(generated, other, request, 0.35f);
			PawnRelationWorker_Lover.ResolveMySkinColor(ref request, generated, other);
		}

		// Token: 0x060015F7 RID: 5623 RVA: 0x000C2BBC File Offset: 0x000C0FBC
		private static void ResolveMySkinColor(ref PawnGenerationRequest request, Pawn generated, Pawn other)
		{
			if (request.FixedMelanin == null)
			{
				request.SetFixedMelanin(PawnSkinColors.GetRandomMelaninSimilarTo(other.story.melanin, 0f, 1f));
			}
		}
	}
}
