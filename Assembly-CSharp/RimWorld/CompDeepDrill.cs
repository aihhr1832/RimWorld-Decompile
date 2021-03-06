﻿using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class CompDeepDrill : ThingComp
	{
		private CompPowerTrader powerComp;

		private float portionProgress = 0f;

		private float portionYieldPct = 0f;

		private int lastUsedTick = -99999;

		private const float WorkPerPortion = 13000f;

		public CompDeepDrill()
		{
		}

		public float ProgressToNextPortionPercent
		{
			get
			{
				return this.portionProgress / 13000f;
			}
		}

		public override void PostSpawnSetup(bool respawningAfterLoad)
		{
			this.powerComp = this.parent.TryGetComp<CompPowerTrader>();
		}

		public override void PostExposeData()
		{
			Scribe_Values.Look<float>(ref this.portionProgress, "portionProgress", 0f, false);
			Scribe_Values.Look<float>(ref this.portionYieldPct, "portionYieldPct", 0f, false);
			Scribe_Values.Look<int>(ref this.lastUsedTick, "lastUsedTick", 0, false);
		}

		public void DrillWorkDone(Pawn driller)
		{
			float statValue = driller.GetStatValue(StatDefOf.MiningSpeed, true);
			this.portionProgress += statValue;
			this.portionYieldPct += statValue * driller.GetStatValue(StatDefOf.MiningYield, true) / 13000f;
			this.lastUsedTick = Find.TickManager.TicksGame;
			if (this.portionProgress > 13000f)
			{
				this.TryProducePortion(this.portionYieldPct);
				this.portionProgress = 0f;
				this.portionYieldPct = 0f;
			}
		}

		public override void PostDeSpawn(Map map)
		{
			this.portionProgress = 0f;
			this.portionYieldPct = 0f;
			this.lastUsedTick = -99999;
		}

		private void TryProducePortion(float yieldPct)
		{
			ThingDef thingDef;
			int num;
			IntVec3 c;
			bool nextResource = this.GetNextResource(out thingDef, out num, out c);
			if (thingDef != null)
			{
				int num2 = Mathf.Min(num, thingDef.deepCountPerPortion);
				if (nextResource)
				{
					this.parent.Map.deepResourceGrid.SetAt(c, thingDef, num - num2);
				}
				int stackCount = Mathf.Max(1, GenMath.RoundRandom((float)num2 * yieldPct));
				Thing thing = ThingMaker.MakeThing(thingDef, null);
				thing.stackCount = stackCount;
				GenPlace.TryPlaceThing(thing, this.parent.InteractionCell, this.parent.Map, ThingPlaceMode.Near, null, null);
				if (nextResource && !this.ValuableResourcesPresent())
				{
					if (DeepDrillUtility.GetBaseResource(this.parent.Map) == null)
					{
						Messages.Message("DeepDrillExhaustedNoFallback".Translate(), this.parent, MessageTypeDefOf.TaskCompletion, true);
					}
					else
					{
						Messages.Message("DeepDrillExhausted".Translate(new object[]
						{
							Find.ActiveLanguageWorker.Pluralize(DeepDrillUtility.GetBaseResource(this.parent.Map).label, -1)
						}), this.parent, MessageTypeDefOf.TaskCompletion, true);
						this.parent.SetForbidden(true, true);
					}
				}
			}
		}

		private bool GetNextResource(out ThingDef resDef, out int countPresent, out IntVec3 cell)
		{
			return DeepDrillUtility.GetNextResource(this.parent.Position, this.parent.Map, out resDef, out countPresent, out cell);
		}

		public bool CanDrillNow()
		{
			return (this.powerComp == null || this.powerComp.PowerOn) && (DeepDrillUtility.GetBaseResource(this.parent.Map) != null || this.ValuableResourcesPresent());
		}

		public bool ValuableResourcesPresent()
		{
			ThingDef thingDef;
			int num;
			IntVec3 intVec;
			return this.GetNextResource(out thingDef, out num, out intVec);
		}

		public bool UsedLastTick()
		{
			return this.lastUsedTick >= Find.TickManager.TicksGame - 1;
		}

		public override string CompInspectStringExtra()
		{
			string result;
			if (this.parent.Spawned)
			{
				ThingDef thingDef;
				int num;
				IntVec3 intVec;
				this.GetNextResource(out thingDef, out num, out intVec);
				if (thingDef == null)
				{
					result = "DeepDrillNoResources".Translate();
				}
				else
				{
					result = string.Concat(new string[]
					{
						"ResourceBelow".Translate(),
						": ",
						thingDef.LabelCap,
						"\n",
						"ProgressToNextPortion".Translate(),
						": ",
						this.ProgressToNextPortionPercent.ToStringPercent("F0")
					});
				}
			}
			else
			{
				result = null;
			}
			return result;
		}
	}
}
