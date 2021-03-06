﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using RimWorld;
using UnityEngine;

namespace Verse
{
	public class RaceProperties
	{
		public Intelligence intelligence = Intelligence.Animal;

		private FleshTypeDef fleshType = null;

		private ThingDef bloodDef = null;

		public bool hasGenders = true;

		public bool needsRest = true;

		public ThinkTreeDef thinkTreeMain;

		public ThinkTreeDef thinkTreeConstant;

		public PawnNameCategory nameCategory = PawnNameCategory.NoName;

		public FoodTypeFlags foodType = FoodTypeFlags.None;

		public BodyDef body = null;

		public Type deathActionWorkerClass;

		public List<AnimalBiomeRecord> wildBiomes = null;

		public SimpleCurve ageGenerationCurve = null;

		public bool makesFootprints = false;

		public int executionRange = 2;

		public float lifeExpectancy = 10f;

		public List<HediffGiverSetDef> hediffGiverSets = null;

		public bool herdAnimal = false;

		public bool packAnimal = false;

		public bool predator = false;

		public float maxPreyBodySize = 99999f;

		public float wildness = 0f;

		public float petness = 0f;

		public float nuzzleMtbHours = -1f;

		public float manhunterOnDamageChance = 0f;

		public float manhunterOnTameFailChance = 0f;

		public bool canBePredatorPrey = true;

		public bool herdMigrationAllowed = true;

		public float gestationPeriodDays = 10f;

		public SimpleCurve litterSizeCurve = null;

		public float mateMtbHours = 12f;

		[NoTranslate]
		public List<string> untrainableTags = null;

		[NoTranslate]
		public List<string> trainableTags = null;

		public TrainabilityDef trainability = null;

		private RulePackDef nameGenerator;

		private RulePackDef nameGeneratorFemale;

		public float nameOnTameChance = 0f;

		public float nameOnNuzzleChance = 0f;

		public float baseBodySize = 1f;

		public float baseHealthScale = 1f;

		public float baseHungerRate = 1f;

		public List<LifeStageAge> lifeStageAges = new List<LifeStageAge>();

		[MustTranslate]
		public string meatLabel = null;

		public Color meatColor;

		public float meatMarketValue;

		public ThingDef useMeatFrom;

		public ThingDef useLeatherFrom;

		public ShadowData specialShadowData;

		public IntRange soundCallIntervalRange;

		public SoundDef soundMeleeHitPawn;

		public SoundDef soundMeleeHitBuilding;

		public SoundDef soundMeleeMiss;

		[Unsaved]
		private DeathActionWorker deathActionWorkerInt;

		[Unsaved]
		public ThingDef meatDef;

		[Unsaved]
		public ThingDef leatherDef;

		[Unsaved]
		public ThingDef corpseDef;

		[Unsaved]
		private PawnKindDef cachedAnyPawnKind;

		public RaceProperties()
		{
			ColorInt colorInt = new ColorInt(141, 56, 52);
			this.meatColor = colorInt.ToColor;
			this.meatMarketValue = 2f;
			this.useMeatFrom = null;
			this.useLeatherFrom = null;
			this.specialShadowData = null;
			this.soundCallIntervalRange = new IntRange(2000, 4000);
			this.soundMeleeHitPawn = null;
			this.soundMeleeHitBuilding = null;
			this.soundMeleeMiss = null;
			this.deathActionWorkerInt = null;
			this.meatDef = null;
			this.leatherDef = null;
			this.corpseDef = null;
			base..ctor();
		}

		public bool Humanlike
		{
			get
			{
				return this.intelligence >= Intelligence.Humanlike;
			}
		}

		public bool ToolUser
		{
			get
			{
				return this.intelligence >= Intelligence.ToolUser;
			}
		}

		public bool Animal
		{
			get
			{
				return !this.ToolUser && this.IsFlesh;
			}
		}

		public bool EatsFood
		{
			get
			{
				return this.foodType != FoodTypeFlags.None;
			}
		}

		public float FoodLevelPercentageWantEat
		{
			get
			{
				float result;
				switch (this.ResolvedDietCategory)
				{
				case DietCategory.NeverEats:
					result = 0.3f;
					break;
				case DietCategory.Herbivorous:
					result = 0.45f;
					break;
				case DietCategory.Dendrovorous:
					result = 0.45f;
					break;
				case DietCategory.Ovivorous:
					result = 0.4f;
					break;
				case DietCategory.Omnivorous:
					result = 0.3f;
					break;
				case DietCategory.Carnivorous:
					result = 0.3f;
					break;
				default:
					throw new InvalidOperationException();
				}
				return result;
			}
		}

		public DietCategory ResolvedDietCategory
		{
			get
			{
				DietCategory result;
				if (!this.EatsFood)
				{
					result = DietCategory.NeverEats;
				}
				else if (this.Eats(FoodTypeFlags.Tree))
				{
					result = DietCategory.Dendrovorous;
				}
				else if (this.Eats(FoodTypeFlags.Meat))
				{
					if (this.Eats(FoodTypeFlags.VegetableOrFruit) || this.Eats(FoodTypeFlags.Plant))
					{
						result = DietCategory.Omnivorous;
					}
					else
					{
						result = DietCategory.Carnivorous;
					}
				}
				else if (this.Eats(FoodTypeFlags.AnimalProduct))
				{
					result = DietCategory.Ovivorous;
				}
				else
				{
					result = DietCategory.Herbivorous;
				}
				return result;
			}
		}

		public DeathActionWorker DeathActionWorker
		{
			get
			{
				if (this.deathActionWorkerInt == null)
				{
					if (this.deathActionWorkerClass != null)
					{
						this.deathActionWorkerInt = (DeathActionWorker)Activator.CreateInstance(this.deathActionWorkerClass);
					}
					else
					{
						this.deathActionWorkerInt = new DeathActionWorker_Simple();
					}
				}
				return this.deathActionWorkerInt;
			}
		}

		public FleshTypeDef FleshType
		{
			get
			{
				FleshTypeDef normal;
				if (this.fleshType != null)
				{
					normal = this.fleshType;
				}
				else
				{
					normal = FleshTypeDefOf.Normal;
				}
				return normal;
			}
		}

		public bool IsMechanoid
		{
			get
			{
				return this.FleshType == FleshTypeDefOf.Mechanoid;
			}
		}

		public bool IsFlesh
		{
			get
			{
				return this.FleshType != FleshTypeDefOf.Mechanoid;
			}
		}

		public ThingDef BloodDef
		{
			get
			{
				ThingDef result;
				if (this.bloodDef != null)
				{
					result = this.bloodDef;
				}
				else if (this.IsFlesh)
				{
					result = ThingDefOf.Filth_Blood;
				}
				else
				{
					result = null;
				}
				return result;
			}
		}

		public bool CanDoHerdMigration
		{
			get
			{
				return this.Animal && this.herdMigrationAllowed;
			}
		}

		public PawnKindDef AnyPawnKind
		{
			get
			{
				if (this.cachedAnyPawnKind == null)
				{
					List<PawnKindDef> allDefsListForReading = DefDatabase<PawnKindDef>.AllDefsListForReading;
					for (int i = 0; i < allDefsListForReading.Count; i++)
					{
						if (allDefsListForReading[i].race.race == this)
						{
							this.cachedAnyPawnKind = allDefsListForReading[i];
							break;
						}
					}
				}
				return this.cachedAnyPawnKind;
			}
		}

		public RulePackDef GetNameGenerator(Gender gender)
		{
			RulePackDef result;
			if (gender == Gender.Female && this.nameGeneratorFemale != null)
			{
				result = this.nameGeneratorFemale;
			}
			else
			{
				result = this.nameGenerator;
			}
			return result;
		}

		public bool WillAutomaticallyEat(Thing t)
		{
			return t.def.ingestible != null && this.CanEverEat(t);
		}

		public bool CanEverEat(Thing t)
		{
			return this.CanEverEat(t.def);
		}

		public bool CanEverEat(ThingDef t)
		{
			return this.EatsFood && t.ingestible != null && t.ingestible.preferability != FoodPreferability.Undefined && this.Eats(t.ingestible.foodType);
		}

		public bool Eats(FoodTypeFlags food)
		{
			return this.EatsFood && (this.foodType & food) != FoodTypeFlags.None;
		}

		public void ResolveReferencesSpecial()
		{
			if (this.useMeatFrom != null)
			{
				this.meatDef = this.useMeatFrom.race.meatDef;
			}
			if (this.useLeatherFrom != null)
			{
				this.leatherDef = this.useLeatherFrom.race.leatherDef;
			}
		}

		public IEnumerable<string> ConfigErrors()
		{
			if (this.soundMeleeHitPawn == null)
			{
				yield return "soundMeleeHitPawn is null";
			}
			if (this.soundMeleeHitBuilding == null)
			{
				yield return "soundMeleeHitBuilding is null";
			}
			if (this.soundMeleeMiss == null)
			{
				yield return "soundMeleeMiss is null";
			}
			if (this.predator && !this.Eats(FoodTypeFlags.Meat))
			{
				yield return "predator but doesn't eat meat";
			}
			for (int i = 0; i < this.lifeStageAges.Count; i++)
			{
				for (int j = 0; j < i; j++)
				{
					if (this.lifeStageAges[j].minAge > this.lifeStageAges[i].minAge)
					{
						yield return "lifeStages minAges are not in ascending order";
					}
				}
			}
			if (this.litterSizeCurve != null)
			{
				foreach (string e in this.litterSizeCurve.ConfigErrors("litterSizeCurve"))
				{
					yield return e;
				}
			}
			if (this.nameOnTameChance > 0f && this.nameGenerator == null)
			{
				yield return "can be named, but has no nameGenerator";
			}
			if (this.Animal && this.wildness < 0f)
			{
				yield return "is animal but wildness is not defined";
			}
			if (this.useMeatFrom != null && this.useMeatFrom.category != ThingCategory.Pawn)
			{
				yield return "tries to use meat from non-pawn " + this.useMeatFrom;
			}
			if (this.useMeatFrom != null && this.useMeatFrom.race.useMeatFrom != null)
			{
				yield return string.Concat(new object[]
				{
					"tries to use meat from ",
					this.useMeatFrom,
					" which uses meat from ",
					this.useMeatFrom.race.useMeatFrom
				});
			}
			if (this.useLeatherFrom != null && this.useLeatherFrom.category != ThingCategory.Pawn)
			{
				yield return "tries to use leather from non-pawn " + this.useLeatherFrom;
			}
			if (this.useLeatherFrom != null && this.useLeatherFrom.race.useLeatherFrom != null)
			{
				yield return string.Concat(new object[]
				{
					"tries to use leather from ",
					this.useLeatherFrom,
					" which uses leather from ",
					this.useLeatherFrom.race.useLeatherFrom
				});
			}
			if (this.Animal && this.trainability == null)
			{
				yield return "animal has trainability = null";
			}
			yield break;
		}

		public IEnumerable<StatDrawEntry> SpecialDisplayStats(ThingDef parentDef)
		{
			yield return new StatDrawEntry(StatCategoryDefOf.Basics, "Race".Translate(), parentDef.LabelCap, 2000, "");
			yield return new StatDrawEntry(StatCategoryDefOf.Basics, "Diet".Translate(), this.foodType.ToHumanString().CapitalizeFirst(), 0, "");
			if (parentDef.race.leatherDef != null)
			{
				yield return new StatDrawEntry(StatCategoryDefOf.Basics, "LeatherType".Translate(), parentDef.race.leatherDef.LabelCap, 0, "");
			}
			if (parentDef.race.Animal || this.wildness > 0f)
			{
				yield return new StatDrawEntry(StatCategoryDefOf.Basics, "Wildness".Translate(), this.wildness.ToStringPercent(), 0, "")
				{
					overrideReportText = TrainableUtility.GetWildnessExplanation(parentDef)
				};
			}
			yield return new StatDrawEntry(StatCategoryDefOf.Basics, "HarmedRevengeChance".Translate(), PawnUtility.GetManhunterOnDamageChance(parentDef.race).ToStringPercent(), 0, "")
			{
				overrideReportText = "HarmedRevengeChanceExplanation".Translate()
			};
			yield return new StatDrawEntry(StatCategoryDefOf.Basics, "TameFailedRevengeChance".Translate(), parentDef.race.manhunterOnTameFailChance.ToStringPercent(), 0, "");
			if (this.intelligence < Intelligence.Humanlike && this.trainability != null)
			{
				yield return new StatDrawEntry(StatCategoryDefOf.Basics, "Trainability".Translate(), this.trainability.LabelCap, 0, "");
			}
			yield return new StatDrawEntry(StatCategoryDefOf.Basics, "StatsReport_LifeExpectancy".Translate(), this.lifeExpectancy.ToStringByStyle(ToStringStyle.Integer, ToStringNumberSense.Absolute), 0, "");
			if (this.intelligence < Intelligence.Humanlike)
			{
				yield return new StatDrawEntry(StatCategoryDefOf.Basics, "AnimalFilthRate".Translate(), (PawnUtility.AnimalFilthChancePerCell(parentDef, parentDef.race.baseBodySize) * 1000f).ToString("F2"), 0, "")
				{
					overrideReportText = "AnimalFilthRateExplanation".Translate(new object[]
					{
						1000.ToString()
					})
				};
			}
			if (this.packAnimal)
			{
				yield return new StatDrawEntry(StatCategoryDefOf.Basics, "PackAnimal".Translate(), "Yes".Translate(), 0, "")
				{
					overrideReportText = "PackAnimalExplanation".Translate()
				};
			}
			if (parentDef.race.nuzzleMtbHours > 0f)
			{
				yield return new StatDrawEntry(StatCategoryDefOf.PawnSocial, "NuzzleInterval".Translate(), Mathf.RoundToInt(parentDef.race.nuzzleMtbHours * 2500f).ToStringTicksToPeriod(), 0, "")
				{
					overrideReportText = "NuzzleIntervalExplanation".Translate()
				};
			}
			yield break;
		}

		[CompilerGenerated]
		private sealed class <ConfigErrors>c__Iterator0 : IEnumerable, IEnumerable<string>, IEnumerator, IDisposable, IEnumerator<string>
		{
			internal int <i>__1;

			internal int <j>__2;

			internal IEnumerator<string> $locvar0;

			internal string <e>__3;

			internal RaceProperties $this;

			internal string $current;

			internal bool $disposing;

			internal int $PC;

			[DebuggerHidden]
			public <ConfigErrors>c__Iterator0()
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
					if (this.soundMeleeHitPawn == null)
					{
						this.$current = "soundMeleeHitPawn is null";
						if (!this.$disposing)
						{
							this.$PC = 1;
						}
						return true;
					}
					break;
				case 1u:
					break;
				case 2u:
					goto IL_B2;
				case 3u:
					goto IL_E1;
				case 4u:
					goto IL_121;
				case 5u:
					IL_195:
					j++;
					goto IL_1A4;
				case 6u:
					Block_16:
					try
					{
						switch (num)
						{
						}
						if (enumerator.MoveNext())
						{
							e = enumerator.Current;
							this.$current = e;
							if (!this.$disposing)
							{
								this.$PC = 6;
							}
							flag = true;
							return true;
						}
					}
					finally
					{
						if (!flag)
						{
							if (enumerator != null)
							{
								enumerator.Dispose();
							}
						}
					}
					goto IL_28B;
				case 7u:
					IL_2CF:
					if (base.Animal && this.wildness < 0f)
					{
						this.$current = "is animal but wildness is not defined";
						if (!this.$disposing)
						{
							this.$PC = 8;
						}
						return true;
					}
					goto IL_313;
				case 8u:
					goto IL_313;
				case 9u:
					goto IL_369;
				case 10u:
					goto IL_3EF;
				case 11u:
					goto IL_445;
				case 12u:
					goto IL_4CB;
				case 13u:
					goto IL_50B;
				default:
					return false;
				}
				if (this.soundMeleeHitBuilding == null)
				{
					this.$current = "soundMeleeHitBuilding is null";
					if (!this.$disposing)
					{
						this.$PC = 2;
					}
					return true;
				}
				IL_B2:
				if (this.soundMeleeMiss == null)
				{
					this.$current = "soundMeleeMiss is null";
					if (!this.$disposing)
					{
						this.$PC = 3;
					}
					return true;
				}
				IL_E1:
				if (this.predator && !base.Eats(FoodTypeFlags.Meat))
				{
					this.$current = "predator but doesn't eat meat";
					if (!this.$disposing)
					{
						this.$PC = 4;
					}
					return true;
				}
				IL_121:
				i = 0;
				goto IL_1C4;
				IL_1A4:
				if (j >= i)
				{
					i++;
				}
				else
				{
					if (this.lifeStageAges[j].minAge > this.lifeStageAges[i].minAge)
					{
						this.$current = "lifeStages minAges are not in ascending order";
						if (!this.$disposing)
						{
							this.$PC = 5;
						}
						return true;
					}
					goto IL_195;
				}
				IL_1C4:
				if (i < this.lifeStageAges.Count)
				{
					j = 0;
					goto IL_1A4;
				}
				if (this.litterSizeCurve != null)
				{
					enumerator = this.litterSizeCurve.ConfigErrors("litterSizeCurve").GetEnumerator();
					num = 4294967293u;
					goto Block_16;
				}
				IL_28B:
				if (this.nameOnTameChance > 0f && this.nameGenerator == null)
				{
					this.$current = "can be named, but has no nameGenerator";
					if (!this.$disposing)
					{
						this.$PC = 7;
					}
					return true;
				}
				goto IL_2CF;
				IL_313:
				if (this.useMeatFrom != null && this.useMeatFrom.category != ThingCategory.Pawn)
				{
					this.$current = "tries to use meat from non-pawn " + this.useMeatFrom;
					if (!this.$disposing)
					{
						this.$PC = 9;
					}
					return true;
				}
				IL_369:
				if (this.useMeatFrom != null && this.useMeatFrom.race.useMeatFrom != null)
				{
					this.$current = string.Concat(new object[]
					{
						"tries to use meat from ",
						this.useMeatFrom,
						" which uses meat from ",
						this.useMeatFrom.race.useMeatFrom
					});
					if (!this.$disposing)
					{
						this.$PC = 10;
					}
					return true;
				}
				IL_3EF:
				if (this.useLeatherFrom != null && this.useLeatherFrom.category != ThingCategory.Pawn)
				{
					this.$current = "tries to use leather from non-pawn " + this.useLeatherFrom;
					if (!this.$disposing)
					{
						this.$PC = 11;
					}
					return true;
				}
				IL_445:
				if (this.useLeatherFrom != null && this.useLeatherFrom.race.useLeatherFrom != null)
				{
					this.$current = string.Concat(new object[]
					{
						"tries to use leather from ",
						this.useLeatherFrom,
						" which uses leather from ",
						this.useLeatherFrom.race.useLeatherFrom
					});
					if (!this.$disposing)
					{
						this.$PC = 12;
					}
					return true;
				}
				IL_4CB:
				if (base.Animal && this.trainability == null)
				{
					this.$current = "animal has trainability = null";
					if (!this.$disposing)
					{
						this.$PC = 13;
					}
					return true;
				}
				IL_50B:
				this.$PC = -1;
				return false;
			}

			string IEnumerator<string>.Current
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
				case 6u:
					try
					{
					}
					finally
					{
						if (enumerator != null)
						{
							enumerator.Dispose();
						}
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
				return this.System.Collections.Generic.IEnumerable<string>.GetEnumerator();
			}

			[DebuggerHidden]
			IEnumerator<string> IEnumerable<string>.GetEnumerator()
			{
				if (Interlocked.CompareExchange(ref this.$PC, 0, -2) == -2)
				{
					return this;
				}
				RaceProperties.<ConfigErrors>c__Iterator0 <ConfigErrors>c__Iterator = new RaceProperties.<ConfigErrors>c__Iterator0();
				<ConfigErrors>c__Iterator.$this = this;
				return <ConfigErrors>c__Iterator;
			}
		}

		[CompilerGenerated]
		private sealed class <SpecialDisplayStats>c__Iterator1 : IEnumerable, IEnumerable<StatDrawEntry>, IEnumerator, IDisposable, IEnumerator<StatDrawEntry>
		{
			internal ThingDef parentDef;

			internal StatDrawEntry <we>__1;

			internal StatDrawEntry <hrc>__2;

			internal StatDrawEntry <af>__3;

			internal StatDrawEntry <pa>__4;

			internal StatDrawEntry <nuzzle>__5;

			internal RaceProperties $this;

			internal StatDrawEntry $current;

			internal bool $disposing;

			internal int $PC;

			[DebuggerHidden]
			public <SpecialDisplayStats>c__Iterator1()
			{
			}

			public bool MoveNext()
			{
				uint num = (uint)this.$PC;
				this.$PC = -1;
				switch (num)
				{
				case 0u:
					this.$current = new StatDrawEntry(StatCategoryDefOf.Basics, "Race".Translate(), parentDef.LabelCap, 2000, "");
					if (!this.$disposing)
					{
						this.$PC = 1;
					}
					return true;
				case 1u:
					this.$current = new StatDrawEntry(StatCategoryDefOf.Basics, "Diet".Translate(), this.foodType.ToHumanString().CapitalizeFirst(), 0, "");
					if (!this.$disposing)
					{
						this.$PC = 2;
					}
					return true;
				case 2u:
					if (parentDef.race.leatherDef != null)
					{
						this.$current = new StatDrawEntry(StatCategoryDefOf.Basics, "LeatherType".Translate(), parentDef.race.leatherDef.LabelCap, 0, "");
						if (!this.$disposing)
						{
							this.$PC = 3;
						}
						return true;
					}
					break;
				case 3u:
					break;
				case 4u:
					goto IL_1C8;
				case 5u:
					this.$current = new StatDrawEntry(StatCategoryDefOf.Basics, "TameFailedRevengeChance".Translate(), parentDef.race.manhunterOnTameFailChance.ToStringPercent(), 0, "");
					if (!this.$disposing)
					{
						this.$PC = 6;
					}
					return true;
				case 6u:
					if (this.intelligence < Intelligence.Humanlike && this.trainability != null)
					{
						this.$current = new StatDrawEntry(StatCategoryDefOf.Basics, "Trainability".Translate(), this.trainability.LabelCap, 0, "");
						if (!this.$disposing)
						{
							this.$PC = 7;
						}
						return true;
					}
					goto IL_2E2;
				case 7u:
					goto IL_2E2;
				case 8u:
					if (this.intelligence < Intelligence.Humanlike)
					{
						StatDrawEntry af = new StatDrawEntry(StatCategoryDefOf.Basics, "AnimalFilthRate".Translate(), (PawnUtility.AnimalFilthChancePerCell(parentDef, parentDef.race.baseBodySize) * 1000f).ToString("F2"), 0, "");
						af.overrideReportText = "AnimalFilthRateExplanation".Translate(new object[]
						{
							1000.ToString()
						});
						this.$current = af;
						if (!this.$disposing)
						{
							this.$PC = 9;
						}
						return true;
					}
					goto IL_3DB;
				case 9u:
					goto IL_3DB;
				case 10u:
					goto IL_44D;
				case 11u:
					goto IL_4DF;
				default:
					return false;
				}
				if (parentDef.race.Animal || this.wildness > 0f)
				{
					StatDrawEntry we = new StatDrawEntry(StatCategoryDefOf.Basics, "Wildness".Translate(), this.wildness.ToStringPercent(), 0, "");
					we.overrideReportText = TrainableUtility.GetWildnessExplanation(parentDef);
					this.$current = we;
					if (!this.$disposing)
					{
						this.$PC = 4;
					}
					return true;
				}
				IL_1C8:
				StatDrawEntry hrc = new StatDrawEntry(StatCategoryDefOf.Basics, "HarmedRevengeChance".Translate(), PawnUtility.GetManhunterOnDamageChance(parentDef.race).ToStringPercent(), 0, "");
				hrc.overrideReportText = "HarmedRevengeChanceExplanation".Translate();
				this.$current = hrc;
				if (!this.$disposing)
				{
					this.$PC = 5;
				}
				return true;
				IL_2E2:
				this.$current = new StatDrawEntry(StatCategoryDefOf.Basics, "StatsReport_LifeExpectancy".Translate(), this.lifeExpectancy.ToStringByStyle(ToStringStyle.Integer, ToStringNumberSense.Absolute), 0, "");
				if (!this.$disposing)
				{
					this.$PC = 8;
				}
				return true;
				IL_3DB:
				if (this.packAnimal)
				{
					StatDrawEntry pa = new StatDrawEntry(StatCategoryDefOf.Basics, "PackAnimal".Translate(), "Yes".Translate(), 0, "");
					pa.overrideReportText = "PackAnimalExplanation".Translate();
					this.$current = pa;
					if (!this.$disposing)
					{
						this.$PC = 10;
					}
					return true;
				}
				IL_44D:
				if (parentDef.race.nuzzleMtbHours > 0f)
				{
					StatDrawEntry nuzzle = new StatDrawEntry(StatCategoryDefOf.PawnSocial, "NuzzleInterval".Translate(), Mathf.RoundToInt(parentDef.race.nuzzleMtbHours * 2500f).ToStringTicksToPeriod(), 0, "");
					nuzzle.overrideReportText = "NuzzleIntervalExplanation".Translate();
					this.$current = nuzzle;
					if (!this.$disposing)
					{
						this.$PC = 11;
					}
					return true;
				}
				IL_4DF:
				this.$PC = -1;
				return false;
			}

			StatDrawEntry IEnumerator<StatDrawEntry>.Current
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
				this.$disposing = true;
				this.$PC = -1;
			}

			[DebuggerHidden]
			public void Reset()
			{
				throw new NotSupportedException();
			}

			[DebuggerHidden]
			IEnumerator IEnumerable.GetEnumerator()
			{
				return this.System.Collections.Generic.IEnumerable<RimWorld.StatDrawEntry>.GetEnumerator();
			}

			[DebuggerHidden]
			IEnumerator<StatDrawEntry> IEnumerable<StatDrawEntry>.GetEnumerator()
			{
				if (Interlocked.CompareExchange(ref this.$PC, 0, -2) == -2)
				{
					return this;
				}
				RaceProperties.<SpecialDisplayStats>c__Iterator1 <SpecialDisplayStats>c__Iterator = new RaceProperties.<SpecialDisplayStats>c__Iterator1();
				<SpecialDisplayStats>c__Iterator.$this = this;
				<SpecialDisplayStats>c__Iterator.parentDef = parentDef;
				return <SpecialDisplayStats>c__Iterator;
			}
		}
	}
}
