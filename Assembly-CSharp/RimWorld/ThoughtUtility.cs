﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Verse;

namespace RimWorld
{
	public static class ThoughtUtility
	{
		public static List<ThoughtDef> situationalSocialThoughtDefs;

		public static List<ThoughtDef> situationalNonSocialThoughtDefs;

		[CompilerGenerated]
		private static Func<ThoughtDef, bool> <>f__am$cache0;

		[CompilerGenerated]
		private static Func<ThoughtDef, bool> <>f__am$cache1;

		[CompilerGenerated]
		private static Func<Thought_Memory, bool> <>f__am$cache2;

		[CompilerGenerated]
		private static Func<Thought_Memory, bool> <>f__am$cache3;

		public static void Reset()
		{
			ThoughtUtility.situationalSocialThoughtDefs = (from x in DefDatabase<ThoughtDef>.AllDefs
			where x.IsSituational && x.IsSocial
			select x).ToList<ThoughtDef>();
			ThoughtUtility.situationalNonSocialThoughtDefs = (from x in DefDatabase<ThoughtDef>.AllDefs
			where x.IsSituational && !x.IsSocial
			select x).ToList<ThoughtDef>();
		}

		public static void GiveThoughtsForPawnExecuted(Pawn victim, PawnExecutionKind kind)
		{
			if (victim.RaceProps.Humanlike)
			{
				int forcedStage = 1;
				if (victim.guilt.IsGuilty)
				{
					forcedStage = 0;
				}
				else if (kind != PawnExecutionKind.GenericHumane)
				{
					if (kind != PawnExecutionKind.GenericBrutal)
					{
						if (kind == PawnExecutionKind.OrganHarvesting)
						{
							forcedStage = 3;
						}
					}
					else
					{
						forcedStage = 2;
					}
				}
				else
				{
					forcedStage = 1;
				}
				ThoughtDef def;
				if (victim.IsColonist)
				{
					def = ThoughtDefOf.KnowColonistExecuted;
				}
				else
				{
					def = ThoughtDefOf.KnowGuestExecuted;
				}
				foreach (Pawn pawn in PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonistsAndPrisoners)
				{
					pawn.needs.mood.thoughts.memories.TryGainMemory(ThoughtMaker.MakeThought(def, forcedStage), null);
				}
			}
		}

		public static void GiveThoughtsForPawnOrganHarvested(Pawn victim)
		{
			if (victim.RaceProps.Humanlike)
			{
				ThoughtDef thoughtDef = null;
				if (victim.IsColonist)
				{
					thoughtDef = ThoughtDefOf.KnowColonistOrganHarvested;
				}
				else if (victim.HostFaction == Faction.OfPlayer)
				{
					thoughtDef = ThoughtDefOf.KnowGuestOrganHarvested;
				}
				foreach (Pawn pawn in PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonistsAndPrisoners)
				{
					if (pawn == victim)
					{
						pawn.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.MyOrganHarvested, null);
					}
					else if (thoughtDef != null)
					{
						pawn.needs.mood.thoughts.memories.TryGainMemory(thoughtDef, null);
					}
				}
			}
		}

		public static bool IsSituationalThoughtNullifiedByHediffs(ThoughtDef def, Pawn pawn)
		{
			bool result;
			if (def.IsMemory)
			{
				result = false;
			}
			else
			{
				float num = 0f;
				List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
				for (int i = 0; i < hediffs.Count; i++)
				{
					HediffStage curStage = hediffs[i].CurStage;
					if (curStage != null && curStage.pctConditionalThoughtsNullified > num)
					{
						num = curStage.pctConditionalThoughtsNullified;
					}
				}
				if (num == 0f)
				{
					result = false;
				}
				else
				{
					Rand.PushState();
					Rand.Seed = pawn.thingIDNumber * 31 + (int)(def.index * 139);
					bool flag = Rand.Value < num;
					Rand.PopState();
					result = flag;
				}
			}
			return result;
		}

		public static bool IsThoughtNullifiedByOwnTales(ThoughtDef def, Pawn pawn)
		{
			if (def.nullifyingOwnTales != null)
			{
				for (int i = 0; i < def.nullifyingOwnTales.Count; i++)
				{
					if (Find.TaleManager.GetLatestTale(def.nullifyingOwnTales[i], pawn) != null)
					{
						return true;
					}
				}
			}
			return false;
		}

		public static void RemovePositiveBedroomThoughts(Pawn pawn)
		{
			if (pawn.needs.mood != null)
			{
				pawn.needs.mood.thoughts.memories.RemoveMemoriesOfDefIf(ThoughtDefOf.SleptInBedroom, (Thought_Memory thought) => thought.MoodOffset() > 0f);
				pawn.needs.mood.thoughts.memories.RemoveMemoriesOfDefIf(ThoughtDefOf.SleptInBarracks, (Thought_Memory thought) => thought.MoodOffset() > 0f);
			}
		}

		public static bool CanGetThought(Pawn pawn, ThoughtDef def)
		{
			try
			{
				if (!def.validWhileDespawned && !pawn.Spawned && !def.IsMemory)
				{
					return false;
				}
				if (def.nullifyingTraits != null)
				{
					for (int i = 0; i < def.nullifyingTraits.Count; i++)
					{
						if (pawn.story.traits.HasTrait(def.nullifyingTraits[i]))
						{
							return false;
						}
					}
				}
				if (!def.requiredTraits.NullOrEmpty<TraitDef>())
				{
					bool flag = false;
					for (int j = 0; j < def.requiredTraits.Count; j++)
					{
						if (pawn.story.traits.HasTrait(def.requiredTraits[j]))
						{
							if (!def.RequiresSpecificTraitsDegree || def.requiredTraitsDegree == pawn.story.traits.DegreeOfTrait(def.requiredTraits[j]))
							{
								flag = true;
								break;
							}
						}
					}
					if (!flag)
					{
						return false;
					}
				}
				if (def.nullifiedIfNotColonist && !pawn.IsColonist)
				{
					return false;
				}
				if (ThoughtUtility.IsSituationalThoughtNullifiedByHediffs(def, pawn))
				{
					return false;
				}
				if (ThoughtUtility.IsThoughtNullifiedByOwnTales(def, pawn))
				{
					return false;
				}
			}
			finally
			{
			}
			return true;
		}

		[CompilerGenerated]
		private static bool <Reset>m__0(ThoughtDef x)
		{
			return x.IsSituational && x.IsSocial;
		}

		[CompilerGenerated]
		private static bool <Reset>m__1(ThoughtDef x)
		{
			return x.IsSituational && !x.IsSocial;
		}

		[CompilerGenerated]
		private static bool <RemovePositiveBedroomThoughts>m__2(Thought_Memory thought)
		{
			return thought.MoodOffset() > 0f;
		}

		[CompilerGenerated]
		private static bool <RemovePositiveBedroomThoughts>m__3(Thought_Memory thought)
		{
			return thought.MoodOffset() > 0f;
		}
	}
}
