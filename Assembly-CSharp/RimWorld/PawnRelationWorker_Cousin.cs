﻿using System;
using Verse;

namespace RimWorld
{
	// Token: 0x020004C4 RID: 1220
	public class PawnRelationWorker_Cousin : PawnRelationWorker
	{
		// Token: 0x060015D4 RID: 5588 RVA: 0x000C2474 File Offset: 0x000C0874
		public override bool InRelation(Pawn me, Pawn other)
		{
			bool result;
			if (me == other)
			{
				result = false;
			}
			else
			{
				PawnRelationWorker worker = PawnRelationDefOf.UncleOrAunt.Worker;
				result = ((other.GetMother() != null && worker.InRelation(me, other.GetMother())) || (other.GetFather() != null && worker.InRelation(me, other.GetFather())));
			}
			return result;
		}
	}
}
