﻿using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	// Token: 0x020009B2 RID: 2482
	public class StatPart_Mood : StatPart
	{
		// Token: 0x06003793 RID: 14227 RVA: 0x001D9BD8 File Offset: 0x001D7FD8
		public override IEnumerable<string> ConfigErrors()
		{
			if (this.factorFromMoodCurve == null)
			{
				yield return "curve is null.";
			}
			yield break;
		}

		// Token: 0x06003794 RID: 14228 RVA: 0x001D9C04 File Offset: 0x001D8004
		public override void TransformValue(StatRequest req, ref float val)
		{
			if (req.HasThing)
			{
				Pawn pawn = req.Thing as Pawn;
				if (pawn != null && this.ActiveFor(pawn))
				{
					val *= this.FactorFromMood(pawn);
				}
			}
		}

		// Token: 0x06003795 RID: 14229 RVA: 0x001D9C50 File Offset: 0x001D8050
		public override string ExplanationPart(StatRequest req)
		{
			if (req.HasThing)
			{
				Pawn pawn = req.Thing as Pawn;
				if (pawn != null && this.ActiveFor(pawn))
				{
					return "StatsReport_MoodMultiplier".Translate(new object[]
					{
						pawn.needs.mood.CurLevel.ToStringPercent()
					}) + ": x" + this.FactorFromMood(pawn).ToStringPercent();
				}
			}
			return null;
		}

		// Token: 0x06003796 RID: 14230 RVA: 0x001D9CD8 File Offset: 0x001D80D8
		private bool ActiveFor(Pawn pawn)
		{
			return pawn.needs.mood != null;
		}

		// Token: 0x06003797 RID: 14231 RVA: 0x001D9D00 File Offset: 0x001D8100
		private float FactorFromMood(Pawn pawn)
		{
			return this.factorFromMoodCurve.Evaluate(pawn.needs.mood.CurLevel);
		}

		// Token: 0x040023AD RID: 9133
		private SimpleCurve factorFromMoodCurve = null;
	}
}
