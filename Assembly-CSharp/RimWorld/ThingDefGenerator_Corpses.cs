﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Verse;

namespace RimWorld
{
	public static class ThingDefGenerator_Corpses
	{
		private const float DaysToStartRot = 2.5f;

		private const float DaysToDessicate = 5f;

		private const float RotDamagePerDay = 2f;

		private const float DessicatedDamagePerDay = 0.7f;

		public static IEnumerable<ThingDef> ImpliedCorpseDefs()
		{
			foreach (ThingDef raceDef in DefDatabase<ThingDef>.AllDefs.ToList<ThingDef>())
			{
				if (raceDef.category == ThingCategory.Pawn)
				{
					ThingDef d = new ThingDef();
					d.category = ThingCategory.Item;
					d.thingClass = typeof(Corpse);
					d.selectable = true;
					d.tickerType = TickerType.Rare;
					d.altitudeLayer = AltitudeLayer.ItemImportant;
					d.scatterableOnMapGen = false;
					d.SetStatBaseValue(StatDefOf.Beauty, -50f);
					d.SetStatBaseValue(StatDefOf.DeteriorationRate, 2f);
					d.SetStatBaseValue(StatDefOf.FoodPoisonChanceFixedHuman, 0.05f);
					d.alwaysHaulable = true;
					d.soundDrop = SoundDefOf.Corpse_Drop;
					d.pathCost = 15;
					d.socialPropernessMatters = false;
					d.tradeability = Tradeability.None;
					d.inspectorTabs = new List<Type>();
					d.inspectorTabs.Add(typeof(ITab_Pawn_Health));
					d.inspectorTabs.Add(typeof(ITab_Pawn_Character));
					d.inspectorTabs.Add(typeof(ITab_Pawn_Gear));
					d.inspectorTabs.Add(typeof(ITab_Pawn_Social));
					d.inspectorTabs.Add(typeof(ITab_Pawn_Log));
					d.comps.Add(new CompProperties_Forbiddable());
					d.recipes = new List<RecipeDef>();
					if (!raceDef.race.IsMechanoid)
					{
						d.recipes.Add(RecipeDefOf.RemoveBodyPart);
					}
					d.defName = "Corpse_" + raceDef.defName;
					d.label = "CorpseLabel".Translate(new object[]
					{
						raceDef.label
					});
					d.description = "CorpseDesc".Translate(new object[]
					{
						raceDef.label
					});
					d.soundImpactDefault = raceDef.soundImpactDefault;
					d.SetStatBaseValue(StatDefOf.Flammability, raceDef.GetStatValueAbstract(StatDefOf.Flammability, null));
					d.SetStatBaseValue(StatDefOf.MaxHitPoints, (float)raceDef.BaseMaxHitPoints);
					d.SetStatBaseValue(StatDefOf.Mass, raceDef.statBases.GetStatOffsetFromList(StatDefOf.Mass));
					d.SetStatBaseValue(StatDefOf.Nutrition, 5.2f);
					d.modContentPack = raceDef.modContentPack;
					d.ingestible = new IngestibleProperties();
					d.ingestible.parent = d;
					IngestibleProperties ing = d.ingestible;
					ing.foodType = FoodTypeFlags.Corpse;
					ing.sourceDef = raceDef;
					ing.preferability = ((!raceDef.race.IsFlesh) ? FoodPreferability.NeverForNutrition : FoodPreferability.DesperateOnly);
					DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(ing, "tasteThought", ThoughtDefOf.AteCorpse.defName);
					ing.maxNumToIngestAtOnce = 1;
					ing.ingestEffect = EffecterDefOf.EatMeat;
					ing.ingestSound = SoundDefOf.RawMeat_Eat;
					ing.specialThoughtDirect = raceDef.race.FleshType.ateDirect;
					if (raceDef.race.IsFlesh)
					{
						CompProperties_Rottable compProperties_Rottable = new CompProperties_Rottable();
						compProperties_Rottable.daysToRotStart = 2.5f;
						compProperties_Rottable.daysToDessicated = 5f;
						compProperties_Rottable.rotDamagePerDay = 2f;
						compProperties_Rottable.dessicatedDamagePerDay = 0.7f;
						d.comps.Add(compProperties_Rottable);
						CompProperties_SpawnerFilth compProperties_SpawnerFilth = new CompProperties_SpawnerFilth();
						compProperties_SpawnerFilth.filthDef = ThingDefOf.Filth_CorpseBile;
						compProperties_SpawnerFilth.spawnCountOnSpawn = 0;
						compProperties_SpawnerFilth.spawnMtbHours = 0f;
						compProperties_SpawnerFilth.spawnRadius = 0.1f;
						compProperties_SpawnerFilth.spawnEveryDays = 1f;
						compProperties_SpawnerFilth.requiredRotStage = new RotStage?(RotStage.Rotting);
						d.comps.Add(compProperties_SpawnerFilth);
					}
					if (d.thingCategories == null)
					{
						d.thingCategories = new List<ThingCategoryDef>();
					}
					if (raceDef.race.Humanlike)
					{
						DirectXmlCrossRefLoader.RegisterListWantsCrossRef<ThingCategoryDef>(d.thingCategories, ThingCategoryDefOf.CorpsesHumanlike.defName, d);
					}
					else
					{
						DirectXmlCrossRefLoader.RegisterListWantsCrossRef<ThingCategoryDef>(d.thingCategories, raceDef.race.FleshType.corpseCategory.defName, d);
					}
					raceDef.race.corpseDef = d;
					yield return d;
				}
			}
			yield break;
		}

		[CompilerGenerated]
		private sealed class <ImpliedCorpseDefs>c__Iterator0 : IEnumerable, IEnumerable<ThingDef>, IEnumerator, IDisposable, IEnumerator<ThingDef>
		{
			internal List<ThingDef>.Enumerator $locvar0;

			internal ThingDef <raceDef>__1;

			internal ThingDef <d>__2;

			internal IngestibleProperties <ing>__2;

			internal ThingDef $current;

			internal bool $disposing;

			internal int $PC;

			[DebuggerHidden]
			public <ImpliedCorpseDefs>c__Iterator0()
			{
			}

			public bool MoveNext()
			{
				uint num = (uint)this.$PC;
				this.$PC = -1;
				bool flag = false;
				switch (num)
				{
				case 0u:
					enumerator = DefDatabase<ThingDef>.AllDefs.ToList<ThingDef>().GetEnumerator();
					num = 4294967293u;
					break;
				case 1u:
					break;
				default:
					return false;
				}
				try
				{
					switch (num)
					{
					}
					while (enumerator.MoveNext())
					{
						raceDef = enumerator.Current;
						if (raceDef.category == ThingCategory.Pawn)
						{
							d = new ThingDef();
							d.category = ThingCategory.Item;
							d.thingClass = typeof(Corpse);
							d.selectable = true;
							d.tickerType = TickerType.Rare;
							d.altitudeLayer = AltitudeLayer.ItemImportant;
							d.scatterableOnMapGen = false;
							d.SetStatBaseValue(StatDefOf.Beauty, -50f);
							d.SetStatBaseValue(StatDefOf.DeteriorationRate, 2f);
							d.SetStatBaseValue(StatDefOf.FoodPoisonChanceFixedHuman, 0.05f);
							d.alwaysHaulable = true;
							d.soundDrop = SoundDefOf.Corpse_Drop;
							d.pathCost = 15;
							d.socialPropernessMatters = false;
							d.tradeability = Tradeability.None;
							d.inspectorTabs = new List<Type>();
							d.inspectorTabs.Add(typeof(ITab_Pawn_Health));
							d.inspectorTabs.Add(typeof(ITab_Pawn_Character));
							d.inspectorTabs.Add(typeof(ITab_Pawn_Gear));
							d.inspectorTabs.Add(typeof(ITab_Pawn_Social));
							d.inspectorTabs.Add(typeof(ITab_Pawn_Log));
							d.comps.Add(new CompProperties_Forbiddable());
							d.recipes = new List<RecipeDef>();
							if (!raceDef.race.IsMechanoid)
							{
								d.recipes.Add(RecipeDefOf.RemoveBodyPart);
							}
							d.defName = "Corpse_" + raceDef.defName;
							d.label = "CorpseLabel".Translate(new object[]
							{
								raceDef.label
							});
							d.description = "CorpseDesc".Translate(new object[]
							{
								raceDef.label
							});
							d.soundImpactDefault = raceDef.soundImpactDefault;
							d.SetStatBaseValue(StatDefOf.Flammability, raceDef.GetStatValueAbstract(StatDefOf.Flammability, null));
							d.SetStatBaseValue(StatDefOf.MaxHitPoints, (float)raceDef.BaseMaxHitPoints);
							d.SetStatBaseValue(StatDefOf.Mass, raceDef.statBases.GetStatOffsetFromList(StatDefOf.Mass));
							d.SetStatBaseValue(StatDefOf.Nutrition, 5.2f);
							d.modContentPack = raceDef.modContentPack;
							d.ingestible = new IngestibleProperties();
							d.ingestible.parent = d;
							ing = d.ingestible;
							ing.foodType = FoodTypeFlags.Corpse;
							ing.sourceDef = raceDef;
							ing.preferability = ((!raceDef.race.IsFlesh) ? FoodPreferability.NeverForNutrition : FoodPreferability.DesperateOnly);
							DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(ing, "tasteThought", ThoughtDefOf.AteCorpse.defName);
							ing.maxNumToIngestAtOnce = 1;
							ing.ingestEffect = EffecterDefOf.EatMeat;
							ing.ingestSound = SoundDefOf.RawMeat_Eat;
							ing.specialThoughtDirect = raceDef.race.FleshType.ateDirect;
							if (raceDef.race.IsFlesh)
							{
								CompProperties_Rottable compProperties_Rottable = new CompProperties_Rottable();
								compProperties_Rottable.daysToRotStart = 2.5f;
								compProperties_Rottable.daysToDessicated = 5f;
								compProperties_Rottable.rotDamagePerDay = 2f;
								compProperties_Rottable.dessicatedDamagePerDay = 0.7f;
								d.comps.Add(compProperties_Rottable);
								CompProperties_SpawnerFilth compProperties_SpawnerFilth = new CompProperties_SpawnerFilth();
								compProperties_SpawnerFilth.filthDef = ThingDefOf.Filth_CorpseBile;
								compProperties_SpawnerFilth.spawnCountOnSpawn = 0;
								compProperties_SpawnerFilth.spawnMtbHours = 0f;
								compProperties_SpawnerFilth.spawnRadius = 0.1f;
								compProperties_SpawnerFilth.spawnEveryDays = 1f;
								compProperties_SpawnerFilth.requiredRotStage = new RotStage?(RotStage.Rotting);
								d.comps.Add(compProperties_SpawnerFilth);
							}
							if (d.thingCategories == null)
							{
								d.thingCategories = new List<ThingCategoryDef>();
							}
							if (raceDef.race.Humanlike)
							{
								DirectXmlCrossRefLoader.RegisterListWantsCrossRef<ThingCategoryDef>(d.thingCategories, ThingCategoryDefOf.CorpsesHumanlike.defName, d);
							}
							else
							{
								DirectXmlCrossRefLoader.RegisterListWantsCrossRef<ThingCategoryDef>(d.thingCategories, raceDef.race.FleshType.corpseCategory.defName, d);
							}
							raceDef.race.corpseDef = d;
							this.$current = d;
							if (!this.$disposing)
							{
								this.$PC = 1;
							}
							flag = true;
							return true;
						}
					}
				}
				finally
				{
					if (!flag)
					{
						((IDisposable)enumerator).Dispose();
					}
				}
				this.$PC = -1;
				return false;
			}

			ThingDef IEnumerator<ThingDef>.Current
			{
				[DebuggerHidden]
				get
				{
					return this.$current;
				}
			}

			object IEnumerator.Current
			{
				[DebuggerHidden]
				get
				{
					return this.$current;
				}
			}

			[DebuggerHidden]
			public void Dispose()
			{
				uint num = (uint)this.$PC;
				this.$disposing = true;
				this.$PC = -1;
				switch (num)
				{
				case 1u:
					try
					{
					}
					finally
					{
						((IDisposable)enumerator).Dispose();
					}
					break;
				}
			}

			[DebuggerHidden]
			public void Reset()
			{
				throw new NotSupportedException();
			}

			[DebuggerHidden]
			IEnumerator IEnumerable.GetEnumerator()
			{
				return this.System.Collections.Generic.IEnumerable<Verse.ThingDef>.GetEnumerator();
			}

			[DebuggerHidden]
			IEnumerator<ThingDef> IEnumerable<ThingDef>.GetEnumerator()
			{
				if (Interlocked.CompareExchange(ref this.$PC, 0, -2) == -2)
				{
					return this;
				}
				return new ThingDefGenerator_Corpses.<ImpliedCorpseDefs>c__Iterator0();
			}
		}
	}
}
