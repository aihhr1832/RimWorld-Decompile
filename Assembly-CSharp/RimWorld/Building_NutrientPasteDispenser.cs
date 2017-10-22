using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace RimWorld
{
	public class Building_NutrientPasteDispenser : Building
	{
		public CompPowerTrader powerComp;

		private List<IntVec3> cachedAdjCellsCardinal;

		public static int CollectDuration = 50;

		public bool CanDispenseNow
		{
			get
			{
				return this.powerComp.PowerOn && this.HasEnoughFeedstockInHoppers();
			}
		}

		private List<IntVec3> AdjCellsCardinalInBounds
		{
			get
			{
				if (this.cachedAdjCellsCardinal == null)
				{
					this.cachedAdjCellsCardinal = (from c in GenAdj.CellsAdjacentCardinal(this)
					where c.InBounds(base.Map)
					select c).ToList();
				}
				return this.cachedAdjCellsCardinal;
			}
		}

		public virtual ThingDef DispensableDef
		{
			get
			{
				return ThingDefOf.MealNutrientPaste;
			}
		}

		public override void SpawnSetup(Map map, bool respawningAfterLoad)
		{
			base.SpawnSetup(map, respawningAfterLoad);
			this.powerComp = base.GetComp<CompPowerTrader>();
		}

		public virtual Building AdjacentReachableHopper(Pawn reacher)
		{
			for (int i = 0; i < this.AdjCellsCardinalInBounds.Count; i++)
			{
				IntVec3 c = this.AdjCellsCardinalInBounds[i];
				Building edifice = c.GetEdifice(base.Map);
				if (edifice != null && edifice.def == ThingDefOf.Hopper && reacher.CanReach((Thing)edifice, PathEndMode.Touch, Danger.Deadly, false, TraverseMode.ByPawn))
				{
					return (Building_Storage)edifice;
				}
			}
			return null;
		}

		public virtual Thing TryDispenseFood()
		{
			if (!this.CanDispenseNow)
			{
				return null;
			}
			float num = (float)(base.def.building.nutritionCostPerDispense - 9.9999997473787516E-05);
			List<ThingDef> list = new List<ThingDef>();
			while (true)
			{
				Thing thing = this.FindFeedInAnyHopper();
				if (thing == null)
				{
					Log.Error("Did not find enough food in hoppers while trying to dispense.");
					return null;
				}
				int num2 = Mathf.Min(thing.stackCount, Mathf.CeilToInt(num / thing.def.ingestible.nutrition));
				num -= (float)num2 * thing.def.ingestible.nutrition;
				list.Add(thing.def);
				thing.SplitOff(num2);
				if (num <= 0.0)
					break;
			}
			base.def.building.soundDispense.PlayOneShot(new TargetInfo(base.Position, base.Map, false));
			Thing thing2 = ThingMaker.MakeThing(ThingDefOf.MealNutrientPaste, null);
			CompIngredients compIngredients = thing2.TryGetComp<CompIngredients>();
			for (int i = 0; i < list.Count; i++)
			{
				compIngredients.RegisterIngredient(list[i]);
			}
			return thing2;
		}

		protected virtual Thing FindFeedInAnyHopper()
		{
			for (int i = 0; i < this.AdjCellsCardinalInBounds.Count; i++)
			{
				Thing thing = null;
				Thing thing2 = null;
				List<Thing> thingList = this.AdjCellsCardinalInBounds[i].GetThingList(base.Map);
				for (int j = 0; j < thingList.Count; j++)
				{
					Thing thing3 = thingList[j];
					if (Building_NutrientPasteDispenser.IsAcceptableFeedstock(thing3.def))
					{
						thing = thing3;
					}
					if (thing3.def == ThingDefOf.Hopper)
					{
						thing2 = thing3;
					}
				}
				if (thing != null && thing2 != null)
				{
					return thing;
				}
			}
			return null;
		}

		public virtual bool HasEnoughFeedstockInHoppers()
		{
			float num = 0f;
			for (int i = 0; i < this.AdjCellsCardinalInBounds.Count; i++)
			{
				IntVec3 c = this.AdjCellsCardinalInBounds[i];
				Thing thing = null;
				Thing thing2 = null;
				List<Thing> thingList = c.GetThingList(base.Map);
				for (int j = 0; j < thingList.Count; j++)
				{
					Thing thing3 = thingList[j];
					if (Building_NutrientPasteDispenser.IsAcceptableFeedstock(thing3.def))
					{
						thing = thing3;
					}
					if (thing3.def == ThingDefOf.Hopper)
					{
						thing2 = thing3;
					}
				}
				if (thing != null && thing2 != null)
				{
					num += (float)thing.stackCount * thing.def.ingestible.nutrition;
				}
				if (num >= base.def.building.nutritionCostPerDispense)
				{
					return true;
				}
			}
			return false;
		}

		public static bool IsAcceptableFeedstock(ThingDef def)
		{
			return def.IsNutritionGivingIngestible && def.ingestible.preferability != 0 && ((int)def.ingestible.foodType & 64) != 64 && ((int)def.ingestible.foodType & 128) != 128;
		}
	}
}
