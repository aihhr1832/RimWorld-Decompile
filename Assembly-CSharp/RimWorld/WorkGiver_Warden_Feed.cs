﻿using System;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class WorkGiver_Warden_Feed : WorkGiver_Warden
	{
		public WorkGiver_Warden_Feed()
		{
		}

		public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			Job result;
			if (!base.ShouldTakeCareOfPrisoner(pawn, t))
			{
				result = null;
			}
			else
			{
				Pawn pawn2 = (Pawn)t;
				Thing thing;
				ThingDef thingDef;
				if (!WardenFeedUtility.ShouldBeFed(pawn2))
				{
					result = null;
				}
				else if (pawn2.needs.food.CurLevelPercentage >= pawn2.needs.food.PercentageThreshHungry + 0.02f)
				{
					result = null;
				}
				else if (!FoodUtility.TryFindBestFoodSourceFor(pawn, pawn2, pawn2.needs.food.CurCategory == HungerCategory.Starving, out thing, out thingDef, false, true, false, false, false, false, false))
				{
					JobFailReason.Is("NoFood".Translate(), null);
					result = null;
				}
				else
				{
					float nutrition = FoodUtility.GetNutrition(thing, thingDef);
					result = new Job(JobDefOf.FeedPatient, thing, pawn2)
					{
						count = FoodUtility.WillIngestStackCountOf(pawn2, thingDef, nutrition)
					};
				}
			}
			return result;
		}
	}
}
