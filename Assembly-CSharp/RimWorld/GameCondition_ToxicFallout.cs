using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class GameCondition_ToxicFallout : GameCondition
	{
		private const int LerpTicks = 5000;

		private const float MaxSkyLerpFactor = 0.5f;

		private const float SkyGlow = 0.85f;

		private const int CheckInterval = 3451;

		private const float ToxicPerDay = 0.5f;

		private const float PlantKillChance = 0.0065f;

		private const float CorpseRotProgressAdd = 3000f;

		private SkyColorSet ToxicFalloutColors = new SkyColorSet(new ColorInt(216, 255, 0).ToColor, new ColorInt(234, 200, 255).ToColor, new Color(0.6f, 0.8f, 0.5f), 0.85f);

		private List<SkyOverlay> overlays = new List<SkyOverlay>
		{
			(SkyOverlay)new WeatherOverlay_Fallout()
		};

		public override void Init()
		{
			LessonAutoActivator.TeachOpportunity(ConceptDefOf.ForbiddingDoors, OpportunityType.Critical);
			LessonAutoActivator.TeachOpportunity(ConceptDefOf.AllowedAreas, OpportunityType.Critical);
		}

		public override void GameConditionTick()
		{
			if (Find.TickManager.TicksGame % 3451 == 0)
			{
				List<Pawn> allPawnsSpawned = base.Map.mapPawns.AllPawnsSpawned;
				for (int i = 0; i < allPawnsSpawned.Count; i++)
				{
					Pawn pawn = allPawnsSpawned[i];
					if (!pawn.Position.Roofed(base.Map) && pawn.def.race.IsFlesh)
					{
						float num = 0.028758334f;
						num *= pawn.GetStatValue(StatDefOf.ToxicSensitivity, true);
						if (num != 0.0)
						{
							float num2 = Mathf.Lerp(0.85f, 1.15f, Rand.ValueSeeded(pawn.thingIDNumber ^ 74374237));
							num *= num2;
							HealthUtility.AdjustSeverity(pawn, HediffDefOf.ToxicBuildup, num);
						}
					}
				}
			}
			for (int j = 0; j < this.overlays.Count; j++)
			{
				this.overlays[j].TickOverlay(base.Map);
			}
		}

		public override void DoCellSteadyEffects(IntVec3 c)
		{
			if (!c.Roofed(base.Map))
			{
				List<Thing> thingList = c.GetThingList(base.Map);
				for (int i = 0; i < thingList.Count; i++)
				{
					Thing thing = thingList[i];
					if (thing is Plant)
					{
						if (Rand.Value < 0.0065000001341104507)
						{
							thing.Kill(default(DamageInfo?));
						}
					}
					else if (thing.def.category == ThingCategory.Item)
					{
						CompRottable compRottable = thing.TryGetComp<CompRottable>();
						if (compRottable != null && (int)compRottable.Stage < 2)
						{
							compRottable.RotProgress += 3000f;
						}
					}
				}
			}
		}

		public override void GameConditionDraw()
		{
			Map map = base.Map;
			for (int i = 0; i < this.overlays.Count; i++)
			{
				this.overlays[i].DrawOverlay(map);
			}
		}

		public override float SkyTargetLerpFactor()
		{
			return GameConditionUtility.LerpInOutValue((float)base.TicksPassed, (float)base.TicksLeft, 5000f, 0.5f);
		}

		public override SkyTarget? SkyTarget()
		{
			return new SkyTarget?(new SkyTarget(0.85f, this.ToxicFalloutColors, 1f, 1f));
		}

		public override float AnimalDensityFactor()
		{
			return 0f;
		}

		public override float PlantDensityFactor()
		{
			return 0f;
		}

		public override bool AllowEnjoyableOutsideNow()
		{
			return false;
		}

		public override List<SkyOverlay> SkyOverlays()
		{
			return this.overlays;
		}
	}
}
