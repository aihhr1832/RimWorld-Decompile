﻿using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class StatWorker_MarketValue : StatWorker
	{
		public const float ValuePerWork = 0.0036f;

		private const float DefaultGuessStuffCost = 2f;

		public StatWorker_MarketValue()
		{
		}

		public override float GetValueUnfinalized(StatRequest req, bool applyPostProcess = true)
		{
			float result;
			if (req.HasThing && req.Thing is Pawn)
			{
				result = base.GetValueUnfinalized(StatRequest.For(req.Def, req.StuffDef, QualityCategory.Normal), applyPostProcess) * PriceUtility.PawnQualityPriceFactor((Pawn)req.Thing);
			}
			else if (req.Def.statBases.StatListContains(StatDefOf.MarketValue))
			{
				result = base.GetValueUnfinalized(req, true);
			}
			else
			{
				result = StatWorker_MarketValue.CalculatedMarketValue(req.Def, req.StuffDef);
			}
			return result;
		}

		public static RecipeDef CalculableRecipe(BuildableDef def)
		{
			if (def.costList.NullOrEmpty<ThingDefCountClass>() && def.costStuffCount <= 0)
			{
				List<RecipeDef> allDefsListForReading = DefDatabase<RecipeDef>.AllDefsListForReading;
				for (int i = 0; i < allDefsListForReading.Count; i++)
				{
					RecipeDef recipeDef = allDefsListForReading[i];
					if (recipeDef.products != null && recipeDef.products.Count == 1 && recipeDef.products[0].thingDef == def)
					{
						for (int j = 0; j < recipeDef.ingredients.Count; j++)
						{
							if (!recipeDef.ingredients[j].IsFixedIngredient)
							{
								return null;
							}
						}
						return recipeDef;
					}
				}
			}
			return null;
		}

		public static float CalculatedMarketValue(BuildableDef def, ThingDef stuffDef)
		{
			float num = 0f;
			RecipeDef recipeDef = StatWorker_MarketValue.CalculableRecipe(def);
			float num2;
			int num3;
			if (recipeDef != null)
			{
				num2 = recipeDef.workAmount;
				num3 = recipeDef.products[0].count;
				if (recipeDef.ingredients != null)
				{
					for (int i = 0; i < recipeDef.ingredients.Count; i++)
					{
						IngredientCount ingredientCount = recipeDef.ingredients[i];
						int num4 = ingredientCount.CountRequiredOfFor(ingredientCount.FixedIngredient, recipeDef);
						num += (float)num4 * ingredientCount.FixedIngredient.BaseMarketValue;
					}
				}
			}
			else
			{
				num2 = Mathf.Max(def.GetStatValueAbstract(StatDefOf.WorkToMake, stuffDef), def.GetStatValueAbstract(StatDefOf.WorkToBuild, stuffDef));
				num3 = 1;
				if (def.costList != null)
				{
					for (int j = 0; j < def.costList.Count; j++)
					{
						ThingDefCountClass thingDefCountClass = def.costList[j];
						num += (float)thingDefCountClass.count * thingDefCountClass.thingDef.BaseMarketValue;
					}
				}
				if (def.costStuffCount > 0)
				{
					if (stuffDef != null)
					{
						num += (float)def.costStuffCount / stuffDef.VolumePerUnit * stuffDef.GetStatValueAbstract(StatDefOf.MarketValue, null);
					}
					else
					{
						num += (float)def.costStuffCount * 2f;
					}
				}
			}
			if (num2 > 2f)
			{
				num += num2 * 0.0036f;
			}
			return num / (float)num3;
		}

		public override string GetExplanationUnfinalized(StatRequest req, ToStringNumberSense numberSense)
		{
			string result;
			if (req.HasThing && req.Thing is Pawn)
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append(base.GetExplanationUnfinalized(req, numberSense));
				stringBuilder.AppendLine();
				stringBuilder.AppendLine();
				Pawn pawn = req.Thing as Pawn;
				stringBuilder.Append("StatsReport_CharacterQuality".Translate() + ": x" + PriceUtility.PawnQualityPriceFactor(pawn).ToStringPercent());
				result = stringBuilder.ToString();
			}
			else if (req.Def.statBases.StatListContains(StatDefOf.MarketValue))
			{
				result = base.GetExplanationUnfinalized(req, numberSense);
			}
			else
			{
				StringBuilder stringBuilder2 = new StringBuilder();
				stringBuilder2.AppendLine("StatsReport_MarketValueFromStuffsAndWork".Translate());
				result = stringBuilder2.ToString();
			}
			return result;
		}

		public override bool ShouldShowFor(StatRequest req)
		{
			ThingDef thingDef = req.Def as ThingDef;
			return thingDef != null && (thingDef.category == ThingCategory.Building || TradeUtility.EverPlayerSellable(thingDef) || (thingDef.tradeability.TraderCanSell() && (thingDef.category == ThingCategory.Item || thingDef.category == ThingCategory.Pawn)));
		}
	}
}
