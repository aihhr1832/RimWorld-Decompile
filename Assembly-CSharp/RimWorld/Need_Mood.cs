﻿using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld
{
	// Token: 0x02000500 RID: 1280
	public class Need_Mood : Need_Seeker
	{
		// Token: 0x06001701 RID: 5889 RVA: 0x000CAC1A File Offset: 0x000C901A
		public Need_Mood(Pawn pawn) : base(pawn)
		{
			this.thoughts = new ThoughtHandler(pawn);
			this.observer = new PawnObserver(pawn);
			this.recentMemory = new PawnRecentMemory(pawn);
		}

		// Token: 0x17000321 RID: 801
		// (get) Token: 0x06001702 RID: 5890 RVA: 0x000CAC48 File Offset: 0x000C9048
		public override float CurInstantLevel
		{
			get
			{
				float num = this.thoughts.TotalMoodOffset();
				if (this.pawn.IsColonist || this.pawn.IsPrisonerOfColony)
				{
					num += Find.Storyteller.difficulty.colonistMoodOffset;
				}
				return Mathf.Clamp01(this.def.baseLevel + num / 100f);
			}
		}

		// Token: 0x17000322 RID: 802
		// (get) Token: 0x06001703 RID: 5891 RVA: 0x000CACB4 File Offset: 0x000C90B4
		public string MoodString
		{
			get
			{
				string result;
				if (this.pawn.MentalStateDef != null)
				{
					result = "Mood_MentalState".Translate();
				}
				else
				{
					float statValue = this.pawn.GetStatValue(StatDefOf.MentalBreakThreshold, true);
					if (this.CurLevel < statValue)
					{
						result = "Mood_AboutToBreak".Translate();
					}
					else if (this.CurLevel < statValue + 0.05f)
					{
						result = "Mood_OnEdge".Translate();
					}
					else if (this.CurLevel < this.pawn.mindState.mentalBreaker.BreakThresholdMinor)
					{
						result = "Mood_Stressed".Translate();
					}
					else if (this.CurLevel < 0.65f)
					{
						result = "Mood_Neutral".Translate();
					}
					else if (this.CurLevel < 0.9f)
					{
						result = "Mood_Content".Translate();
					}
					else
					{
						result = "Mood_Happy".Translate();
					}
				}
				return result;
			}
		}

		// Token: 0x06001704 RID: 5892 RVA: 0x000CADB4 File Offset: 0x000C91B4
		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Deep.Look<ThoughtHandler>(ref this.thoughts, "thoughts", new object[]
			{
				this.pawn
			});
			Scribe_Deep.Look<PawnRecentMemory>(ref this.recentMemory, "recentMemory", new object[]
			{
				this.pawn
			});
		}

		// Token: 0x06001705 RID: 5893 RVA: 0x000CAE06 File Offset: 0x000C9206
		public override void NeedInterval()
		{
			base.NeedInterval();
			this.recentMemory.RecentMemoryInterval();
			this.thoughts.ThoughtInterval();
			this.observer.ObserverInterval();
		}

		// Token: 0x06001706 RID: 5894 RVA: 0x000CAE30 File Offset: 0x000C9230
		public override string GetTipString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine(base.GetTipString());
			stringBuilder.AppendLine();
			stringBuilder.AppendLine("MentalBreakThresholdExtreme".Translate() + ": " + this.pawn.mindState.mentalBreaker.BreakThresholdExtreme.ToStringPercent());
			stringBuilder.AppendLine("MentalBreakThresholdMajor".Translate() + ": " + this.pawn.mindState.mentalBreaker.BreakThresholdMajor.ToStringPercent());
			stringBuilder.AppendLine("MentalBreakThresholdMinor".Translate() + ": " + this.pawn.mindState.mentalBreaker.BreakThresholdMinor.ToStringPercent());
			return stringBuilder.ToString();
		}

		// Token: 0x06001707 RID: 5895 RVA: 0x000CAF04 File Offset: 0x000C9304
		public override void DrawOnGUI(Rect rect, int maxThresholdMarkers = 2147483647, float customMargin = -1f, bool drawArrows = true, bool doTooltip = true)
		{
			if (this.threshPercents == null)
			{
				this.threshPercents = new List<float>();
			}
			this.threshPercents.Clear();
			this.threshPercents.Add(this.pawn.mindState.mentalBreaker.BreakThresholdExtreme);
			this.threshPercents.Add(this.pawn.mindState.mentalBreaker.BreakThresholdMajor);
			this.threshPercents.Add(this.pawn.mindState.mentalBreaker.BreakThresholdMinor);
			base.DrawOnGUI(rect, maxThresholdMarkers, customMargin, drawArrows, doTooltip);
		}

		// Token: 0x04000D88 RID: 3464
		public ThoughtHandler thoughts;

		// Token: 0x04000D89 RID: 3465
		public PawnObserver observer;

		// Token: 0x04000D8A RID: 3466
		public PawnRecentMemory recentMemory;
	}
}
