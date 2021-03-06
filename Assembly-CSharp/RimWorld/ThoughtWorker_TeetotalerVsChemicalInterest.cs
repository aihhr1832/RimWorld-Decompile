﻿using System;
using Verse;

namespace RimWorld
{
	public class ThoughtWorker_TeetotalerVsChemicalInterest : ThoughtWorker
	{
		public ThoughtWorker_TeetotalerVsChemicalInterest()
		{
		}

		protected override ThoughtState CurrentSocialStateInternal(Pawn p, Pawn other)
		{
			ThoughtState result;
			if (!p.RaceProps.Humanlike)
			{
				result = false;
			}
			else if (!p.IsTeetotaler())
			{
				result = false;
			}
			else if (!other.RaceProps.Humanlike)
			{
				result = false;
			}
			else if (!RelationsUtility.PawnsKnowEachOther(p, other))
			{
				result = false;
			}
			else
			{
				int num = other.story.traits.DegreeOfTrait(TraitDefOf.DrugDesire);
				if (num <= 0)
				{
					result = false;
				}
				else
				{
					result = true;
				}
			}
			return result;
		}
	}
}
