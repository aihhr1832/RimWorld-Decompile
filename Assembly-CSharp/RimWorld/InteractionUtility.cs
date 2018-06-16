﻿using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	// Token: 0x0200052B RID: 1323
	public static class InteractionUtility
	{
		// Token: 0x06001832 RID: 6194 RVA: 0x000D33F8 File Offset: 0x000D17F8
		public static bool CanInitiateInteraction(Pawn pawn)
		{
			return pawn.interactions != null && pawn.health.capacities.CapableOf(PawnCapacityDefOf.Talking) && pawn.Awake() && !pawn.IsBurning();
		}

		// Token: 0x06001833 RID: 6195 RVA: 0x000D3468 File Offset: 0x000D1868
		public static bool CanReceiveInteraction(Pawn pawn)
		{
			return pawn.Awake() && !pawn.IsBurning();
		}

		// Token: 0x06001834 RID: 6196 RVA: 0x000D34A4 File Offset: 0x000D18A4
		public static bool CanInitiateRandomInteraction(Pawn p)
		{
			return InteractionUtility.CanInitiateInteraction(p) && p.RaceProps.Humanlike && !p.Downed && !p.InAggroMentalState && p.Faction != null;
		}

		// Token: 0x06001835 RID: 6197 RVA: 0x000D350C File Offset: 0x000D190C
		public static bool CanReceiveRandomInteraction(Pawn p)
		{
			return InteractionUtility.CanReceiveInteraction(p) && p.RaceProps.Humanlike && !p.Downed && !p.InAggroMentalState;
		}

		// Token: 0x06001836 RID: 6198 RVA: 0x000D3564 File Offset: 0x000D1964
		public static bool IsGoodPositionForInteraction(Pawn p, Pawn recipient)
		{
			return InteractionUtility.IsGoodPositionForInteraction(p.Position, recipient.Position, p.Map);
		}

		// Token: 0x06001837 RID: 6199 RVA: 0x000D3590 File Offset: 0x000D1990
		public static bool IsGoodPositionForInteraction(IntVec3 cell, IntVec3 recipientCell, Map map)
		{
			return cell.InHorDistOf(recipientCell, 6f) && GenSight.LineOfSight(cell, recipientCell, map, true, null, 0, 0);
		}

		// Token: 0x06001838 RID: 6200 RVA: 0x000D35C8 File Offset: 0x000D19C8
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

		// Token: 0x06001839 RID: 6201 RVA: 0x000D3640 File Offset: 0x000D1A40
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

		// Token: 0x0600183A RID: 6202 RVA: 0x000D36B4 File Offset: 0x000D1AB4
		public static bool HasAnySocialFightProvokingThought(Pawn pawn, Pawn otherPawn)
		{
			Thought thought;
			return InteractionUtility.TryGetRandomSocialFightProvokingThought(pawn, otherPawn, out thought);
		}

		// Token: 0x0600183B RID: 6203 RVA: 0x000D36D4 File Offset: 0x000D1AD4
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

		// Token: 0x04000E69 RID: 3689
		public const float MaxInteractRange = 6f;

		// Token: 0x04000E6A RID: 3690
		private static List<ISocialThought> tmpSocialThoughts = new List<ISocialThought>();
	}
}
