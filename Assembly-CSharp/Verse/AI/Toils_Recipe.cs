﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using RimWorld;

namespace Verse.AI
{
	public static class Toils_Recipe
	{
		private const int LongCraftingProjectThreshold = 20000;

		[CompilerGenerated]
		private static Predicate<ThingDefCountClass> <>f__am$cache0;

		[CompilerGenerated]
		private static Func<Thing, bool> <>f__am$cache1;

		[CompilerGenerated]
		private static Func<Thing, float> <>f__am$cache2;

		[CompilerGenerated]
		private static Func<Thing, float> <>f__am$cache3;

		public static Toil MakeUnfinishedThingIfNeeded()
		{
			Toil toil = new Toil();
			toil.initAction = delegate()
			{
				Pawn actor = toil.actor;
				Job curJob = actor.jobs.curJob;
				if (curJob.RecipeDef.UsesUnfinishedThing)
				{
					if (!(curJob.GetTarget(TargetIndex.B).Thing is UnfinishedThing))
					{
						List<Thing> list = Toils_Recipe.CalculateIngredients(curJob, actor);
						Thing thing = Toils_Recipe.CalculateDominantIngredient(curJob, list);
						for (int i = 0; i < list.Count; i++)
						{
							Thing thing2 = list[i];
							actor.Map.designationManager.RemoveAllDesignationsOn(thing2, false);
							if (thing2.Spawned)
							{
								thing2.DeSpawn(DestroyMode.Vanish);
							}
						}
						ThingDef stuff = (!curJob.RecipeDef.unfinishedThingDef.MadeFromStuff) ? null : thing.def;
						UnfinishedThing unfinishedThing = (UnfinishedThing)ThingMaker.MakeThing(curJob.RecipeDef.unfinishedThingDef, stuff);
						unfinishedThing.Creator = actor;
						unfinishedThing.BoundBill = (Bill_ProductionWithUft)curJob.bill;
						unfinishedThing.ingredients = list;
						CompColorable compColorable = unfinishedThing.TryGetComp<CompColorable>();
						if (compColorable != null)
						{
							compColorable.Color = thing.DrawColor;
						}
						GenSpawn.Spawn(unfinishedThing, curJob.GetTarget(TargetIndex.A).Cell, actor.Map, WipeMode.Vanish);
						curJob.SetTarget(TargetIndex.B, unfinishedThing);
						actor.Reserve(unfinishedThing, curJob, 1, -1, null);
					}
				}
			};
			return toil;
		}

		public static Toil DoRecipeWork()
		{
			Toil toil = new Toil();
			toil.initAction = delegate()
			{
				Pawn actor = toil.actor;
				Job curJob = actor.jobs.curJob;
				JobDriver_DoBill jobDriver_DoBill = (JobDriver_DoBill)actor.jobs.curDriver;
				UnfinishedThing unfinishedThing = curJob.GetTarget(TargetIndex.B).Thing as UnfinishedThing;
				if (unfinishedThing != null && unfinishedThing.Initialized)
				{
					jobDriver_DoBill.workLeft = unfinishedThing.workLeft;
				}
				else
				{
					jobDriver_DoBill.workLeft = curJob.bill.recipe.WorkAmountTotal((unfinishedThing == null) ? null : unfinishedThing.Stuff);
					if (unfinishedThing != null)
					{
						unfinishedThing.workLeft = jobDriver_DoBill.workLeft;
					}
				}
				jobDriver_DoBill.billStartTick = Find.TickManager.TicksGame;
				jobDriver_DoBill.ticksSpentDoingRecipeWork = 0;
				curJob.bill.Notify_DoBillStarted(actor);
			};
			toil.tickAction = delegate()
			{
				Pawn actor = toil.actor;
				Job curJob = actor.jobs.curJob;
				JobDriver_DoBill jobDriver_DoBill = (JobDriver_DoBill)actor.jobs.curDriver;
				UnfinishedThing unfinishedThing = curJob.GetTarget(TargetIndex.B).Thing as UnfinishedThing;
				if (unfinishedThing != null && unfinishedThing.Destroyed)
				{
					actor.jobs.EndCurrentJob(JobCondition.Incompletable, true);
				}
				else
				{
					jobDriver_DoBill.ticksSpentDoingRecipeWork++;
					curJob.bill.Notify_PawnDidWork(actor);
					IBillGiverWithTickAction billGiverWithTickAction = toil.actor.CurJob.GetTarget(TargetIndex.A).Thing as IBillGiverWithTickAction;
					if (billGiverWithTickAction != null)
					{
						billGiverWithTickAction.UsedThisTick();
					}
					if (curJob.RecipeDef.workSkill != null && curJob.RecipeDef.UsesUnfinishedThing)
					{
						actor.skills.Learn(curJob.RecipeDef.workSkill, 0.11f * curJob.RecipeDef.workSkillLearnFactor, false);
					}
					float num = (curJob.RecipeDef.workSpeedStat != null) ? actor.GetStatValue(curJob.RecipeDef.workSpeedStat, true) : 1f;
					if (curJob.RecipeDef.workTableSpeedStat != null)
					{
						Building_WorkTable building_WorkTable = jobDriver_DoBill.BillGiver as Building_WorkTable;
						if (building_WorkTable != null)
						{
							num *= building_WorkTable.GetStatValue(curJob.RecipeDef.workTableSpeedStat, true);
						}
					}
					if (DebugSettings.fastCrafting)
					{
						num *= 30f;
					}
					jobDriver_DoBill.workLeft -= num;
					if (unfinishedThing != null)
					{
						unfinishedThing.workLeft = jobDriver_DoBill.workLeft;
					}
					actor.GainComfortFromCellIfPossible();
					if (jobDriver_DoBill.workLeft <= 0f)
					{
						jobDriver_DoBill.ReadyForNextToil();
					}
					if (curJob.bill.recipe.UsesUnfinishedThing)
					{
						int num2 = Find.TickManager.TicksGame - jobDriver_DoBill.billStartTick;
						if (num2 >= 3000 && num2 % 1000 == 0)
						{
							actor.jobs.CheckForJobOverride();
						}
					}
				}
			};
			toil.defaultCompleteMode = ToilCompleteMode.Never;
			toil.WithEffect(() => toil.actor.CurJob.bill.recipe.effectWorking, TargetIndex.A);
			toil.PlaySustainerOrSound(() => toil.actor.CurJob.bill.recipe.soundWorking);
			toil.WithProgressBar(TargetIndex.A, delegate
			{
				Pawn actor = toil.actor;
				Job curJob = actor.CurJob;
				UnfinishedThing unfinishedThing = curJob.GetTarget(TargetIndex.B).Thing as UnfinishedThing;
				return 1f - ((JobDriver_DoBill)actor.jobs.curDriver).workLeft / curJob.bill.recipe.WorkAmountTotal((unfinishedThing == null) ? null : unfinishedThing.Stuff);
			}, false, -0.5f);
			toil.FailOn(() => toil.actor.CurJob.bill.suspended);
			toil.activeSkill = (() => toil.actor.CurJob.bill.recipe.workSkill);
			return toil;
		}

		public static Toil FinishRecipeAndStartStoringProduct()
		{
			Toil toil = new Toil();
			toil.initAction = delegate()
			{
				Pawn actor = toil.actor;
				Job curJob = actor.jobs.curJob;
				JobDriver_DoBill jobDriver_DoBill = (JobDriver_DoBill)actor.jobs.curDriver;
				if (curJob.RecipeDef.workSkill != null && !curJob.RecipeDef.UsesUnfinishedThing)
				{
					float xp = (float)jobDriver_DoBill.ticksSpentDoingRecipeWork * 0.11f * curJob.RecipeDef.workSkillLearnFactor;
					actor.skills.GetSkill(curJob.RecipeDef.workSkill).Learn(xp, false);
				}
				List<Thing> ingredients = Toils_Recipe.CalculateIngredients(curJob, actor);
				Thing dominantIngredient = Toils_Recipe.CalculateDominantIngredient(curJob, ingredients);
				List<Thing> list = GenRecipe.MakeRecipeProducts(curJob.RecipeDef, actor, ingredients, dominantIngredient, jobDriver_DoBill.BillGiver).ToList<Thing>();
				Toils_Recipe.ConsumeIngredients(ingredients, curJob.RecipeDef, actor.Map);
				curJob.bill.Notify_IterationCompleted(actor, ingredients);
				RecordsUtility.Notify_BillDone(actor, list);
				UnfinishedThing unfinishedThing = curJob.GetTarget(TargetIndex.B).Thing as UnfinishedThing;
				if (curJob.bill.recipe.WorkAmountTotal((unfinishedThing == null) ? null : unfinishedThing.Stuff) >= 20000f && list.Count > 0)
				{
					TaleRecorder.RecordTale(TaleDefOf.CompletedLongCraftingProject, new object[]
					{
						actor,
						list[0].def
					});
				}
				if (list.Count == 0)
				{
					actor.jobs.EndCurrentJob(JobCondition.Succeeded, true);
				}
				else if (curJob.bill.GetStoreMode() == BillStoreModeDefOf.DropOnFloor)
				{
					for (int i = 0; i < list.Count; i++)
					{
						if (!GenPlace.TryPlaceThing(list[i], actor.Position, actor.Map, ThingPlaceMode.Near, null, null))
						{
							Log.Error(string.Concat(new object[]
							{
								actor,
								" could not drop recipe product ",
								list[i],
								" near ",
								actor.Position
							}), false);
						}
					}
					actor.jobs.EndCurrentJob(JobCondition.Succeeded, true);
				}
				else
				{
					if (list.Count > 1)
					{
						for (int j = 1; j < list.Count; j++)
						{
							if (!GenPlace.TryPlaceThing(list[j], actor.Position, actor.Map, ThingPlaceMode.Near, null, null))
							{
								Log.Error(string.Concat(new object[]
								{
									actor,
									" could not drop recipe product ",
									list[j],
									" near ",
									actor.Position
								}), false);
							}
						}
					}
					IntVec3 invalid = IntVec3.Invalid;
					if (curJob.bill.GetStoreMode() == BillStoreModeDefOf.BestStockpile)
					{
						StoreUtility.TryFindBestBetterStoreCellFor(list[0], actor, actor.Map, StoragePriority.Unstored, actor.Faction, out invalid, true);
					}
					else if (curJob.bill.GetStoreMode() == BillStoreModeDefOf.SpecificStockpile)
					{
						StoreUtility.TryFindBestBetterStoreCellForIn(list[0], actor, actor.Map, StoragePriority.Unstored, actor.Faction, curJob.bill.GetStoreZone().slotGroup, out invalid, true);
					}
					else
					{
						Log.ErrorOnce("Unknown store mode", 9158246, false);
					}
					if (invalid.IsValid)
					{
						actor.carryTracker.TryStartCarry(list[0]);
						curJob.targetB = invalid;
						curJob.targetA = list[0];
						curJob.count = 99999;
					}
					else
					{
						if (!GenPlace.TryPlaceThing(list[0], actor.Position, actor.Map, ThingPlaceMode.Near, null, null))
						{
							Log.Error(string.Concat(new object[]
							{
								"Bill doer could not drop product ",
								list[0],
								" near ",
								actor.Position
							}), false);
						}
						actor.jobs.EndCurrentJob(JobCondition.Succeeded, true);
					}
				}
			};
			return toil;
		}

		private static List<Thing> CalculateIngredients(Job job, Pawn actor)
		{
			UnfinishedThing unfinishedThing = job.GetTarget(TargetIndex.B).Thing as UnfinishedThing;
			List<Thing> result;
			if (unfinishedThing != null)
			{
				List<Thing> ingredients = unfinishedThing.ingredients;
				job.RecipeDef.Worker.ConsumeIngredient(unfinishedThing, job.RecipeDef, actor.Map);
				job.placedThings = null;
				result = ingredients;
			}
			else
			{
				List<Thing> list = new List<Thing>();
				if (job.placedThings != null)
				{
					for (int i = 0; i < job.placedThings.Count; i++)
					{
						if (job.placedThings[i].Count <= 0)
						{
							Log.Error(string.Concat(new object[]
							{
								"PlacedThing ",
								job.placedThings[i],
								" with count ",
								job.placedThings[i].Count,
								" for job ",
								job
							}), false);
						}
						else
						{
							Thing thing;
							if (job.placedThings[i].Count < job.placedThings[i].thing.stackCount)
							{
								thing = job.placedThings[i].thing.SplitOff(job.placedThings[i].Count);
							}
							else
							{
								thing = job.placedThings[i].thing;
							}
							job.placedThings[i].Count = 0;
							if (list.Contains(thing))
							{
								Log.Error("Tried to add ingredient from job placed targets twice: " + thing, false);
							}
							else
							{
								list.Add(thing);
								if (job.RecipeDef.autoStripCorpses)
								{
									IStrippable strippable = thing as IStrippable;
									if (strippable != null)
									{
										strippable.Strip();
									}
								}
							}
						}
					}
				}
				job.placedThings = null;
				result = list;
			}
			return result;
		}

		private static Thing CalculateDominantIngredient(Job job, List<Thing> ingredients)
		{
			UnfinishedThing uft = job.GetTarget(TargetIndex.B).Thing as UnfinishedThing;
			Thing result;
			if (uft != null && uft.def.MadeFromStuff)
			{
				result = uft.ingredients.First((Thing ing) => ing.def == uft.Stuff);
			}
			else if (!ingredients.NullOrEmpty<Thing>())
			{
				if (job.RecipeDef.productHasIngredientStuff)
				{
					result = ingredients[0];
				}
				else if (job.RecipeDef.products.Any((ThingDefCountClass x) => x.thingDef.MadeFromStuff))
				{
					result = (from x in ingredients
					where x.def.IsStuff
					select x).RandomElementByWeight((Thing x) => (float)x.stackCount);
				}
				else
				{
					result = ingredients.RandomElementByWeight((Thing x) => (float)x.stackCount);
				}
			}
			else
			{
				result = null;
			}
			return result;
		}

		private static void ConsumeIngredients(List<Thing> ingredients, RecipeDef recipe, Map map)
		{
			for (int i = 0; i < ingredients.Count; i++)
			{
				recipe.Worker.ConsumeIngredient(ingredients[i], recipe, map);
			}
		}

		[CompilerGenerated]
		private static bool <CalculateDominantIngredient>m__0(ThingDefCountClass x)
		{
			return x.thingDef.MadeFromStuff;
		}

		[CompilerGenerated]
		private static bool <CalculateDominantIngredient>m__1(Thing x)
		{
			return x.def.IsStuff;
		}

		[CompilerGenerated]
		private static float <CalculateDominantIngredient>m__2(Thing x)
		{
			return (float)x.stackCount;
		}

		[CompilerGenerated]
		private static float <CalculateDominantIngredient>m__3(Thing x)
		{
			return (float)x.stackCount;
		}

		[CompilerGenerated]
		private sealed class <MakeUnfinishedThingIfNeeded>c__AnonStorey0
		{
			internal Toil toil;

			public <MakeUnfinishedThingIfNeeded>c__AnonStorey0()
			{
			}

			internal void <>m__0()
			{
				Pawn actor = this.toil.actor;
				Job curJob = actor.jobs.curJob;
				if (curJob.RecipeDef.UsesUnfinishedThing)
				{
					if (!(curJob.GetTarget(TargetIndex.B).Thing is UnfinishedThing))
					{
						List<Thing> list = Toils_Recipe.CalculateIngredients(curJob, actor);
						Thing thing = Toils_Recipe.CalculateDominantIngredient(curJob, list);
						for (int i = 0; i < list.Count; i++)
						{
							Thing thing2 = list[i];
							actor.Map.designationManager.RemoveAllDesignationsOn(thing2, false);
							if (thing2.Spawned)
							{
								thing2.DeSpawn(DestroyMode.Vanish);
							}
						}
						ThingDef stuff = (!curJob.RecipeDef.unfinishedThingDef.MadeFromStuff) ? null : thing.def;
						UnfinishedThing unfinishedThing = (UnfinishedThing)ThingMaker.MakeThing(curJob.RecipeDef.unfinishedThingDef, stuff);
						unfinishedThing.Creator = actor;
						unfinishedThing.BoundBill = (Bill_ProductionWithUft)curJob.bill;
						unfinishedThing.ingredients = list;
						CompColorable compColorable = unfinishedThing.TryGetComp<CompColorable>();
						if (compColorable != null)
						{
							compColorable.Color = thing.DrawColor;
						}
						GenSpawn.Spawn(unfinishedThing, curJob.GetTarget(TargetIndex.A).Cell, actor.Map, WipeMode.Vanish);
						curJob.SetTarget(TargetIndex.B, unfinishedThing);
						actor.Reserve(unfinishedThing, curJob, 1, -1, null);
					}
				}
			}
		}

		[CompilerGenerated]
		private sealed class <DoRecipeWork>c__AnonStorey1
		{
			internal Toil toil;

			public <DoRecipeWork>c__AnonStorey1()
			{
			}

			internal void <>m__0()
			{
				Pawn actor = this.toil.actor;
				Job curJob = actor.jobs.curJob;
				JobDriver_DoBill jobDriver_DoBill = (JobDriver_DoBill)actor.jobs.curDriver;
				UnfinishedThing unfinishedThing = curJob.GetTarget(TargetIndex.B).Thing as UnfinishedThing;
				if (unfinishedThing != null && unfinishedThing.Initialized)
				{
					jobDriver_DoBill.workLeft = unfinishedThing.workLeft;
				}
				else
				{
					jobDriver_DoBill.workLeft = curJob.bill.recipe.WorkAmountTotal((unfinishedThing == null) ? null : unfinishedThing.Stuff);
					if (unfinishedThing != null)
					{
						unfinishedThing.workLeft = jobDriver_DoBill.workLeft;
					}
				}
				jobDriver_DoBill.billStartTick = Find.TickManager.TicksGame;
				jobDriver_DoBill.ticksSpentDoingRecipeWork = 0;
				curJob.bill.Notify_DoBillStarted(actor);
			}

			internal void <>m__1()
			{
				Pawn actor = this.toil.actor;
				Job curJob = actor.jobs.curJob;
				JobDriver_DoBill jobDriver_DoBill = (JobDriver_DoBill)actor.jobs.curDriver;
				UnfinishedThing unfinishedThing = curJob.GetTarget(TargetIndex.B).Thing as UnfinishedThing;
				if (unfinishedThing != null && unfinishedThing.Destroyed)
				{
					actor.jobs.EndCurrentJob(JobCondition.Incompletable, true);
				}
				else
				{
					jobDriver_DoBill.ticksSpentDoingRecipeWork++;
					curJob.bill.Notify_PawnDidWork(actor);
					IBillGiverWithTickAction billGiverWithTickAction = this.toil.actor.CurJob.GetTarget(TargetIndex.A).Thing as IBillGiverWithTickAction;
					if (billGiverWithTickAction != null)
					{
						billGiverWithTickAction.UsedThisTick();
					}
					if (curJob.RecipeDef.workSkill != null && curJob.RecipeDef.UsesUnfinishedThing)
					{
						actor.skills.Learn(curJob.RecipeDef.workSkill, 0.11f * curJob.RecipeDef.workSkillLearnFactor, false);
					}
					float num = (curJob.RecipeDef.workSpeedStat != null) ? actor.GetStatValue(curJob.RecipeDef.workSpeedStat, true) : 1f;
					if (curJob.RecipeDef.workTableSpeedStat != null)
					{
						Building_WorkTable building_WorkTable = jobDriver_DoBill.BillGiver as Building_WorkTable;
						if (building_WorkTable != null)
						{
							num *= building_WorkTable.GetStatValue(curJob.RecipeDef.workTableSpeedStat, true);
						}
					}
					if (DebugSettings.fastCrafting)
					{
						num *= 30f;
					}
					jobDriver_DoBill.workLeft -= num;
					if (unfinishedThing != null)
					{
						unfinishedThing.workLeft = jobDriver_DoBill.workLeft;
					}
					actor.GainComfortFromCellIfPossible();
					if (jobDriver_DoBill.workLeft <= 0f)
					{
						jobDriver_DoBill.ReadyForNextToil();
					}
					if (curJob.bill.recipe.UsesUnfinishedThing)
					{
						int num2 = Find.TickManager.TicksGame - jobDriver_DoBill.billStartTick;
						if (num2 >= 3000 && num2 % 1000 == 0)
						{
							actor.jobs.CheckForJobOverride();
						}
					}
				}
			}

			internal EffecterDef <>m__2()
			{
				return this.toil.actor.CurJob.bill.recipe.effectWorking;
			}

			internal SoundDef <>m__3()
			{
				return this.toil.actor.CurJob.bill.recipe.soundWorking;
			}

			internal float <>m__4()
			{
				Pawn actor = this.toil.actor;
				Job curJob = actor.CurJob;
				UnfinishedThing unfinishedThing = curJob.GetTarget(TargetIndex.B).Thing as UnfinishedThing;
				return 1f - ((JobDriver_DoBill)actor.jobs.curDriver).workLeft / curJob.bill.recipe.WorkAmountTotal((unfinishedThing == null) ? null : unfinishedThing.Stuff);
			}

			internal bool <>m__5()
			{
				return this.toil.actor.CurJob.bill.suspended;
			}

			internal SkillDef <>m__6()
			{
				return this.toil.actor.CurJob.bill.recipe.workSkill;
			}
		}

		[CompilerGenerated]
		private sealed class <FinishRecipeAndStartStoringProduct>c__AnonStorey2
		{
			internal Toil toil;

			public <FinishRecipeAndStartStoringProduct>c__AnonStorey2()
			{
			}

			internal void <>m__0()
			{
				Pawn actor = this.toil.actor;
				Job curJob = actor.jobs.curJob;
				JobDriver_DoBill jobDriver_DoBill = (JobDriver_DoBill)actor.jobs.curDriver;
				if (curJob.RecipeDef.workSkill != null && !curJob.RecipeDef.UsesUnfinishedThing)
				{
					float xp = (float)jobDriver_DoBill.ticksSpentDoingRecipeWork * 0.11f * curJob.RecipeDef.workSkillLearnFactor;
					actor.skills.GetSkill(curJob.RecipeDef.workSkill).Learn(xp, false);
				}
				List<Thing> ingredients = Toils_Recipe.CalculateIngredients(curJob, actor);
				Thing dominantIngredient = Toils_Recipe.CalculateDominantIngredient(curJob, ingredients);
				List<Thing> list = GenRecipe.MakeRecipeProducts(curJob.RecipeDef, actor, ingredients, dominantIngredient, jobDriver_DoBill.BillGiver).ToList<Thing>();
				Toils_Recipe.ConsumeIngredients(ingredients, curJob.RecipeDef, actor.Map);
				curJob.bill.Notify_IterationCompleted(actor, ingredients);
				RecordsUtility.Notify_BillDone(actor, list);
				UnfinishedThing unfinishedThing = curJob.GetTarget(TargetIndex.B).Thing as UnfinishedThing;
				if (curJob.bill.recipe.WorkAmountTotal((unfinishedThing == null) ? null : unfinishedThing.Stuff) >= 20000f && list.Count > 0)
				{
					TaleRecorder.RecordTale(TaleDefOf.CompletedLongCraftingProject, new object[]
					{
						actor,
						list[0].def
					});
				}
				if (list.Count == 0)
				{
					actor.jobs.EndCurrentJob(JobCondition.Succeeded, true);
				}
				else if (curJob.bill.GetStoreMode() == BillStoreModeDefOf.DropOnFloor)
				{
					for (int i = 0; i < list.Count; i++)
					{
						if (!GenPlace.TryPlaceThing(list[i], actor.Position, actor.Map, ThingPlaceMode.Near, null, null))
						{
							Log.Error(string.Concat(new object[]
							{
								actor,
								" could not drop recipe product ",
								list[i],
								" near ",
								actor.Position
							}), false);
						}
					}
					actor.jobs.EndCurrentJob(JobCondition.Succeeded, true);
				}
				else
				{
					if (list.Count > 1)
					{
						for (int j = 1; j < list.Count; j++)
						{
							if (!GenPlace.TryPlaceThing(list[j], actor.Position, actor.Map, ThingPlaceMode.Near, null, null))
							{
								Log.Error(string.Concat(new object[]
								{
									actor,
									" could not drop recipe product ",
									list[j],
									" near ",
									actor.Position
								}), false);
							}
						}
					}
					IntVec3 invalid = IntVec3.Invalid;
					if (curJob.bill.GetStoreMode() == BillStoreModeDefOf.BestStockpile)
					{
						StoreUtility.TryFindBestBetterStoreCellFor(list[0], actor, actor.Map, StoragePriority.Unstored, actor.Faction, out invalid, true);
					}
					else if (curJob.bill.GetStoreMode() == BillStoreModeDefOf.SpecificStockpile)
					{
						StoreUtility.TryFindBestBetterStoreCellForIn(list[0], actor, actor.Map, StoragePriority.Unstored, actor.Faction, curJob.bill.GetStoreZone().slotGroup, out invalid, true);
					}
					else
					{
						Log.ErrorOnce("Unknown store mode", 9158246, false);
					}
					if (invalid.IsValid)
					{
						actor.carryTracker.TryStartCarry(list[0]);
						curJob.targetB = invalid;
						curJob.targetA = list[0];
						curJob.count = 99999;
					}
					else
					{
						if (!GenPlace.TryPlaceThing(list[0], actor.Position, actor.Map, ThingPlaceMode.Near, null, null))
						{
							Log.Error(string.Concat(new object[]
							{
								"Bill doer could not drop product ",
								list[0],
								" near ",
								actor.Position
							}), false);
						}
						actor.jobs.EndCurrentJob(JobCondition.Succeeded, true);
					}
				}
			}
		}

		[CompilerGenerated]
		private sealed class <CalculateDominantIngredient>c__AnonStorey3
		{
			internal UnfinishedThing uft;

			public <CalculateDominantIngredient>c__AnonStorey3()
			{
			}

			internal bool <>m__0(Thing ing)
			{
				return ing.def == this.uft.Stuff;
			}
		}
	}
}
