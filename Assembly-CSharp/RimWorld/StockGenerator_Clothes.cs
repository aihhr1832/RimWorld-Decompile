﻿using System;
using Verse;

namespace RimWorld
{
	// Token: 0x0200077A RID: 1914
	public class StockGenerator_Clothes : StockGenerator_MiscItems
	{
		// Token: 0x06002A36 RID: 10806 RVA: 0x00165E14 File Offset: 0x00164214
		public override bool HandlesThingDef(ThingDef td)
		{
			return td != ThingDefOf.Apparel_ShieldBelt && (base.HandlesThingDef(td) && td.IsApparel) && (td.GetStatValueAbstract(StatDefOf.ArmorRating_Blunt, null) < 0.15f || td.GetStatValueAbstract(StatDefOf.ArmorRating_Sharp, null) < 0.15f);
		}

		// Token: 0x06002A37 RID: 10807 RVA: 0x00165E84 File Offset: 0x00164284
		protected override float SelectionWeight(ThingDef thingDef)
		{
			return StockGenerator_Clothes.SelectionWeightMarketValueCurve.Evaluate(thingDef.BaseMarketValue);
		}

		// Token: 0x040016C5 RID: 5829
		private static readonly SimpleCurve SelectionWeightMarketValueCurve = new SimpleCurve
		{
			{
				new CurvePoint(0f, 1f),
				true
			},
			{
				new CurvePoint(500f, 0.5f),
				true
			},
			{
				new CurvePoint(1500f, 0.2f),
				true
			},
			{
				new CurvePoint(5000f, 0.1f),
				true
			}
		};
	}
}
