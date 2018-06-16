﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	// Token: 0x0200064B RID: 1611
	public class ScenPart_StatFactor : ScenPart
	{
		// Token: 0x0600216E RID: 8558 RVA: 0x0011B811 File Offset: 0x00119C11
		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Defs.Look<StatDef>(ref this.stat, "stat");
			Scribe_Values.Look<float>(ref this.factor, "factor", 0f, false);
		}

		// Token: 0x0600216F RID: 8559 RVA: 0x0011B840 File Offset: 0x00119C40
		public override void DoEditInterface(Listing_ScenEdit listing)
		{
			Rect scenPartRect = listing.GetScenPartRect(this, ScenPart.RowHeight * 2f);
			Rect rect = scenPartRect.TopHalf();
			if (Widgets.ButtonText(rect, this.stat.LabelCap, true, false, true))
			{
				List<FloatMenuOption> list = new List<FloatMenuOption>();
				foreach (StatDef localSd2 in DefDatabase<StatDef>.AllDefs)
				{
					StatDef localSd = localSd2;
					list.Add(new FloatMenuOption(localSd.LabelCap, delegate()
					{
						this.stat = localSd;
					}, MenuOptionPriority.Default, null, null, 0f, null, null));
				}
				Find.WindowStack.Add(new FloatMenu(list));
			}
			Rect rect2 = scenPartRect.BottomHalf();
			Rect rect3 = rect2.LeftHalf().Rounded();
			Rect rect4 = rect2.RightHalf().Rounded();
			Text.Anchor = TextAnchor.MiddleRight;
			Widgets.Label(rect3, "multiplier".Translate());
			Text.Anchor = TextAnchor.UpperLeft;
			Widgets.TextFieldPercent(rect4, ref this.factor, ref this.factorBuf, 0f, 100f);
		}

		// Token: 0x06002170 RID: 8560 RVA: 0x0011B98C File Offset: 0x00119D8C
		public override string Summary(Scenario scen)
		{
			return "ScenPart_StatFactor".Translate(new object[]
			{
				this.stat.label,
				this.factor.ToStringPercent()
			});
		}

		// Token: 0x06002171 RID: 8561 RVA: 0x0011B9D0 File Offset: 0x00119DD0
		public override void Randomize()
		{
			this.stat = (from d in DefDatabase<StatDef>.AllDefs
			where d.scenarioRandomizable
			select d).RandomElement<StatDef>();
			this.factor = GenMath.RoundedHundredth(Rand.Range(0.1f, 3f));
		}

		// Token: 0x06002172 RID: 8562 RVA: 0x0011BA2C File Offset: 0x00119E2C
		public override bool TryMerge(ScenPart other)
		{
			ScenPart_StatFactor scenPart_StatFactor = other as ScenPart_StatFactor;
			bool result;
			if (scenPart_StatFactor != null && scenPart_StatFactor.stat == this.stat)
			{
				this.factor *= scenPart_StatFactor.factor;
				result = true;
			}
			else
			{
				result = false;
			}
			return result;
		}

		// Token: 0x06002173 RID: 8563 RVA: 0x0011BA7C File Offset: 0x00119E7C
		public float GetStatFactor(StatDef stat)
		{
			float result;
			if (stat == this.stat)
			{
				result = this.factor;
			}
			else
			{
				result = 1f;
			}
			return result;
		}

		// Token: 0x04001304 RID: 4868
		private StatDef stat;

		// Token: 0x04001305 RID: 4869
		private float factor;

		// Token: 0x04001306 RID: 4870
		private string factorBuf;
	}
}
