﻿using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	// Token: 0x020007E1 RID: 2017
	public static class TameUtility
	{
		// Token: 0x06002CB1 RID: 11441 RVA: 0x001782E8 File Offset: 0x001766E8
		public static void ShowDesignationWarnings(Pawn pawn)
		{
			float manhunterOnTameFailChance = PawnUtility.GetManhunterOnTameFailChance(pawn);
			if (manhunterOnTameFailChance > 0.02f)
			{
				string text = "MessageAnimalManhuntsOnTameFailed".Translate(new object[]
				{
					pawn.kindDef.GetLabelPlural(-1).CapitalizeFirst(),
					manhunterOnTameFailChance.ToStringPercent()
				});
				Messages.Message(text, pawn, MessageTypeDefOf.CautionInput, false);
			}
			IEnumerable<Pawn> source = from c in pawn.Map.mapPawns.FreeColonistsSpawned
			where c.workSettings.WorkIsActive(WorkTypeDefOf.Handling)
			select c;
			if (!source.Any<Pawn>())
			{
				source = pawn.Map.mapPawns.FreeColonistsSpawned;
			}
			if (source.Any<Pawn>())
			{
				Pawn pawn2 = source.MaxBy((Pawn c) => c.skills.GetSkill(SkillDefOf.Animals).Level);
				int level = pawn2.skills.GetSkill(SkillDefOf.Animals).Level;
				int num = TrainableUtility.MinimumHandlingSkill(pawn);
				if (num > level)
				{
					string text2 = "MessageNoHandlerSkilledEnough".Translate(new object[]
					{
						pawn.kindDef.label,
						num.ToStringCached(),
						SkillDefOf.Animals.LabelCap,
						pawn2.LabelShort,
						level
					});
					Messages.Message(text2, pawn, MessageTypeDefOf.CautionInput, false);
				}
			}
		}
	}
}
