﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Verse;

namespace RimWorld
{
	public static class InteractionUtility
	{
		public const float MaxInteractRange = 6f;

		private static List<ISocialThought> tmpSocialThoughts = new List<ISocialThought>();

		[CompilerGenerated]
		private static Func<ISocialThought, bool> <>f__am$cache0;

		[CompilerGenerated]
		private static Func<ISocialThought, float> <>f__am$cache1;

		public static bool CanInitiateInteraction(Pawn pawn)
		{
			return pawn.interactions != null && pawn.health.capacities.CapableOf(PawnCapacityDefOf.Talking) && pawn.Awake() && !pawn.IsBurning();
		}

		public static bool CanReceiveInteraction(Pawn pawn)
		{
			return pawn.Awake() && !pawn.IsBurning();
		}

		public static bool CanInitiateRandomInteraction(Pawn p)
		{
			return InteractionUtility.CanInitiateInteraction(p) && p.RaceProps.Humanlike && !p.Downed && !p.InAggroMentalState && p.Faction != null;
		}

		public static bool CanReceiveRandomInteraction(Pawn p)
		{
			return InteractionUtility.CanReceiveInteraction(p) && p.RaceProps.Humanlike && !p.Downed && !p.InAggroMentalState;
		}

		public static bool IsGoodPositionForInteraction(Pawn p, Pawn recipient)
		{
			return InteractionUtility.IsGoodPositionForInteraction(p.Position, recipient.Position, p.Map);
		}

		public static bool IsGoodPositionForInteraction(IntVec3 cell, IntVec3 recipientCell, Map map)
		{
			return cell.InHorDistOf(recipientCell, 6f) && GenSight.LineOfSight(cell, recipientCell, map, true, null, 0, 0);
		}

		public static bool HasAnyVerbForSocialFight(Pawn p)
		{
			bool result;
			if (p.Dead)
			{
				result = false;
			}
			else
			{
				List<Verb> allVerbs = p.verbTracker.AllVerbs;
				for (int i = 0; i < allVerbs.Count; i++)
				{
					if (allVerbs[i].IsMeleeAttack && allVerbs[i].IsStillUsableBy(p))
					{
						return true;
					}
				}
				result = false;
			}
			return result;
		}

		public static bool TryGetRandomVerbForSocialFight(Pawn p, out Verb verb)
		{
			bool result;
			if (p.Dead)
			{
				verb = null;
				result = false;
			}
			else
			{
				List<Verb> allVerbs = p.verbTracker.AllVerbs;
				result = (from x in allVerbs
				where x.IsMeleeAttack && x.IsStillUsableBy(p)
				select x).TryRandomElementByWeight((Verb x) => x.verbProps.AdjustedMeleeDamageAmount(x, p, null), out verb);
			}
			return result;
		}

		public static bool HasAnySocialFightProvokingThought(Pawn pawn, Pawn otherPawn)
		{
			Thought thought;
			return InteractionUtility.TryGetRandomSocialFightProvokingThought(pawn, otherPawn, out thought);
		}

		public static bool TryGetRandomSocialFightProvokingThought(Pawn pawn, Pawn otherPawn, out Thought thought)
		{
			bool result;
			if (pawn.needs.mood == null)
			{
				thought = null;
				result = false;
			}
			else
			{
				pawn.needs.mood.thoughts.GetSocialThoughts(otherPawn, InteractionUtility.tmpSocialThoughts);
				ISocialThought socialThought;
				bool flag = InteractionUtility.tmpSocialThoughts.Where(delegate(ISocialThought x)
				{
					ThoughtDef def = ((Thought)x).def;
					return def != ThoughtDefOf.HadAngeringFight && def != ThoughtDefOf.HadCatharticFight && x.OpinionOffset() < 0f;
				}).TryRandomElementByWeight((ISocialThought x) => -x.OpinionOffset(), out socialThought);
				InteractionUtility.tmpSocialThoughts.Clear();
				thought = (Thought)socialThought;
				result = flag;
			}
			return result;
		}

		// Note: this type is marked as 'beforefieldinit'.
		static InteractionUtility()
		{
		}

		[CompilerGenerated]
		private static bool <TryGetRandomSocialFightProvokingThought>m__0(ISocialThought x)
		{
			ThoughtDef def = ((Thought)x).def;
			return def != ThoughtDefOf.HadAngeringFight && def != ThoughtDefOf.HadCatharticFight && x.OpinionOffset() < 0f;
		}

		[CompilerGenerated]
		private static float <TryGetRandomSocialFightProvokingThought>m__1(ISocialThought x)
		{
			return -x.OpinionOffset();
		}

		[CompilerGenerated]
		private sealed class <TryGetRandomVerbForSocialFight>c__AnonStorey0
		{
			internal Pawn p;

			public <TryGetRandomVerbForSocialFight>c__AnonStorey0()
			{
			}

			internal bool <>m__0(Verb x)
			{
				return x.IsMeleeAttack && x.IsStillUsableBy(this.p);
			}

			internal float <>m__1(Verb x)
			{
				return x.verbProps.AdjustedMeleeDamageAmount(x, this.p, null);
			}
		}
	}
}
