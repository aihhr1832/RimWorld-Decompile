﻿using System;
using Verse;

namespace RimWorld
{
	public class PawnRelationWorker_GrandnephewOrGrandniece : PawnRelationWorker
	{
		public PawnRelationWorker_GrandnephewOrGrandniece()
		{
		}

		public override bool InRelation(Pawn me, Pawn other)
		{
			bool result;
			if (me == other)
			{
				result = false;
			}
			else
			{
				PawnRelationWorker worker = PawnRelationDefOf.NephewOrNiece.Worker;
				result = ((other.GetMother() != null && worker.InRelation(me, other.GetMother())) || (other.GetFather() != null && worker.InRelation(me, other.GetFather())));
			}
			return result;
		}
	}
}
