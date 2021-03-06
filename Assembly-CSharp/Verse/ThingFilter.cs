﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using RimWorld;
using UnityEngine;

namespace Verse
{
	public class ThingFilter : IExposable
	{
		[Unsaved]
		private Action settingsChangedCallback;

		[Unsaved]
		private TreeNode_ThingCategory displayRootCategoryInt = null;

		[Unsaved]
		private HashSet<ThingDef> allowedDefs = new HashSet<ThingDef>();

		[Unsaved]
		private List<SpecialThingFilterDef> disallowedSpecialFilters = new List<SpecialThingFilterDef>();

		private FloatRange allowedHitPointsPercents = FloatRange.ZeroToOne;

		public bool allowedHitPointsConfigurable = true;

		private QualityRange allowedQualities = QualityRange.All;

		public bool allowedQualitiesConfigurable = true;

		[MustTranslate]
		public string customSummary = null;

		private List<ThingDef> thingDefs = null;

		[NoTranslate]
		private List<string> categories = null;

		[NoTranslate]
		private List<string> tradeTagsToAllow = null;

		[NoTranslate]
		private List<string> tradeTagsToDisallow = null;

		[NoTranslate]
		private List<string> thingSetMakerTagsToAllow = null;

		[NoTranslate]
		private List<string> thingSetMakerTagsToDisallow = null;

		[NoTranslate]
		private List<string> disallowedCategories = null;

		[NoTranslate]
		private List<string> specialFiltersToAllow = null;

		[NoTranslate]
		private List<string> specialFiltersToDisallow = null;

		private List<StuffCategoryDef> stuffCategoriesToAllow = null;

		private List<ThingDef> allowAllWhoCanMake = null;

		private FoodPreferability disallowWorsePreferability = FoodPreferability.Undefined;

		private bool disallowInedibleByHuman = false;

		private Type allowWithComp = null;

		private Type disallowWithComp = null;

		private float disallowCheaperThan = float.MinValue;

		private List<ThingDef> disallowedThingDefs = null;

		[CompilerGenerated]
		private static Func<ThingDef, bool> <>f__am$cache0;

		[CompilerGenerated]
		private static Predicate<SpecialThingFilterDef> <>f__am$cache1;

		[CompilerGenerated]
		private static Func<ThingDef, bool> <>f__am$cache2;

		public ThingFilter()
		{
		}

		public ThingFilter(Action settingsChangedCallback)
		{
			this.settingsChangedCallback = settingsChangedCallback;
		}

		public string Summary
		{
			get
			{
				string result;
				if (!this.customSummary.NullOrEmpty())
				{
					result = this.customSummary;
				}
				else if (this.thingDefs != null && this.thingDefs.Count == 1 && this.categories.NullOrEmpty<string>() && this.tradeTagsToAllow.NullOrEmpty<string>() && this.tradeTagsToDisallow.NullOrEmpty<string>() && this.thingSetMakerTagsToAllow.NullOrEmpty<string>() && this.thingSetMakerTagsToDisallow.NullOrEmpty<string>() && this.disallowedCategories.NullOrEmpty<string>() && this.specialFiltersToAllow.NullOrEmpty<string>() && this.specialFiltersToDisallow.NullOrEmpty<string>() && this.stuffCategoriesToAllow.NullOrEmpty<StuffCategoryDef>() && this.allowAllWhoCanMake.NullOrEmpty<ThingDef>() && this.disallowWorsePreferability == FoodPreferability.Undefined && !this.disallowInedibleByHuman && this.allowWithComp == null && this.disallowWithComp == null && this.disallowCheaperThan == -3.40282347E+38f && this.disallowedThingDefs.NullOrEmpty<ThingDef>())
				{
					result = this.thingDefs[0].label;
				}
				else if (this.thingDefs.NullOrEmpty<ThingDef>() && this.categories != null && this.categories.Count == 1 && this.tradeTagsToAllow.NullOrEmpty<string>() && this.tradeTagsToDisallow.NullOrEmpty<string>() && this.thingSetMakerTagsToAllow.NullOrEmpty<string>() && this.thingSetMakerTagsToDisallow.NullOrEmpty<string>() && this.disallowedCategories.NullOrEmpty<string>() && this.specialFiltersToAllow.NullOrEmpty<string>() && this.specialFiltersToDisallow.NullOrEmpty<string>() && this.stuffCategoriesToAllow.NullOrEmpty<StuffCategoryDef>() && this.allowAllWhoCanMake.NullOrEmpty<ThingDef>() && this.disallowWorsePreferability == FoodPreferability.Undefined && !this.disallowInedibleByHuman && this.allowWithComp == null && this.disallowWithComp == null && this.disallowCheaperThan == -3.40282347E+38f && this.disallowedThingDefs.NullOrEmpty<ThingDef>())
				{
					result = DefDatabase<ThingCategoryDef>.GetNamed(this.categories[0], true).label;
				}
				else if (this.allowedDefs.Count == 1)
				{
					result = this.allowedDefs.First<ThingDef>().label;
				}
				else
				{
					result = "UsableIngredients".Translate();
				}
				return result;
			}
		}

		public ThingRequest BestThingRequest
		{
			get
			{
				ThingRequest result;
				if (this.allowedDefs.Count == 1)
				{
					result = ThingRequest.ForDef(this.allowedDefs.First<ThingDef>());
				}
				else
				{
					bool flag = true;
					bool flag2 = true;
					foreach (ThingDef thingDef in this.allowedDefs)
					{
						if (!thingDef.EverHaulable)
						{
							flag = false;
						}
						if (thingDef.category != ThingCategory.Pawn)
						{
							flag2 = false;
						}
					}
					if (flag)
					{
						result = ThingRequest.ForGroup(ThingRequestGroup.HaulableEver);
					}
					else if (flag2)
					{
						result = ThingRequest.ForGroup(ThingRequestGroup.Pawn);
					}
					else
					{
						result = ThingRequest.ForGroup(ThingRequestGroup.Everything);
					}
				}
				return result;
			}
		}

		public ThingDef AnyAllowedDef
		{
			get
			{
				return this.allowedDefs.FirstOrDefault<ThingDef>();
			}
		}

		public IEnumerable<ThingDef> AllowedThingDefs
		{
			get
			{
				return this.allowedDefs;
			}
		}

		private static IEnumerable<ThingDef> AllStorableThingDefs
		{
			get
			{
				return from def in DefDatabase<ThingDef>.AllDefs
				where def.EverStorable(true)
				select def;
			}
		}

		public int AllowedDefCount
		{
			get
			{
				return this.allowedDefs.Count;
			}
		}

		public FloatRange AllowedHitPointsPercents
		{
			get
			{
				return this.allowedHitPointsPercents;
			}
			set
			{
				this.allowedHitPointsPercents = value;
			}
		}

		public QualityRange AllowedQualityLevels
		{
			get
			{
				return this.allowedQualities;
			}
			set
			{
				this.allowedQualities = value;
			}
		}

		public TreeNode_ThingCategory DisplayRootCategory
		{
			get
			{
				if (this.displayRootCategoryInt == null)
				{
					this.RecalculateDisplayRootCategory();
				}
				TreeNode_ThingCategory rootNode;
				if (this.displayRootCategoryInt == null)
				{
					rootNode = ThingCategoryNodeDatabase.RootNode;
				}
				else
				{
					rootNode = this.displayRootCategoryInt;
				}
				return rootNode;
			}
			set
			{
				if (value != this.displayRootCategoryInt)
				{
					this.displayRootCategoryInt = value;
					this.RecalculateSpecialFilterConfigurability();
				}
			}
		}

		public virtual void ExposeData()
		{
			Scribe_Collections.Look<SpecialThingFilterDef>(ref this.disallowedSpecialFilters, "disallowedSpecialFilters", LookMode.Def, new object[0]);
			Scribe_Collections.Look<ThingDef>(ref this.allowedDefs, "allowedDefs", LookMode.Undefined);
			Scribe_Values.Look<FloatRange>(ref this.allowedHitPointsPercents, "allowedHitPointsPercents", default(FloatRange), false);
			Scribe_Values.Look<QualityRange>(ref this.allowedQualities, "allowedQualityLevels", default(QualityRange), false);
		}

		public void ResolveReferences()
		{
			for (int i = 0; i < DefDatabase<SpecialThingFilterDef>.AllDefsListForReading.Count; i++)
			{
				SpecialThingFilterDef specialThingFilterDef = DefDatabase<SpecialThingFilterDef>.AllDefsListForReading[i];
				if (!specialThingFilterDef.allowedByDefault)
				{
					this.SetAllow(specialThingFilterDef, false);
				}
			}
			if (this.thingDefs != null)
			{
				for (int j = 0; j < this.thingDefs.Count; j++)
				{
					if (this.thingDefs[j] != null)
					{
						this.SetAllow(this.thingDefs[j], true);
					}
					else
					{
						Log.Error("ThingFilter could not find thing def named " + this.thingDefs[j], false);
					}
				}
			}
			if (this.categories != null)
			{
				for (int k = 0; k < this.categories.Count; k++)
				{
					ThingCategoryDef named = DefDatabase<ThingCategoryDef>.GetNamed(this.categories[k], true);
					if (named != null)
					{
						this.SetAllow(named, true, null, null);
					}
				}
			}
			if (this.tradeTagsToAllow != null)
			{
				for (int l = 0; l < this.tradeTagsToAllow.Count; l++)
				{
					List<ThingDef> allDefsListForReading = DefDatabase<ThingDef>.AllDefsListForReading;
					for (int m = 0; m < allDefsListForReading.Count; m++)
					{
						ThingDef thingDef = allDefsListForReading[m];
						if (thingDef.tradeTags != null && thingDef.tradeTags.Contains(this.tradeTagsToAllow[l]))
						{
							this.SetAllow(thingDef, true);
						}
					}
				}
			}
			if (this.tradeTagsToDisallow != null)
			{
				for (int n = 0; n < this.tradeTagsToDisallow.Count; n++)
				{
					List<ThingDef> allDefsListForReading2 = DefDatabase<ThingDef>.AllDefsListForReading;
					for (int num = 0; num < allDefsListForReading2.Count; num++)
					{
						ThingDef thingDef2 = allDefsListForReading2[num];
						if (thingDef2.tradeTags != null && thingDef2.tradeTags.Contains(this.tradeTagsToDisallow[n]))
						{
							this.SetAllow(thingDef2, false);
						}
					}
				}
			}
			if (this.thingSetMakerTagsToAllow != null)
			{
				for (int num2 = 0; num2 < this.thingSetMakerTagsToAllow.Count; num2++)
				{
					List<ThingDef> allDefsListForReading3 = DefDatabase<ThingDef>.AllDefsListForReading;
					for (int num3 = 0; num3 < allDefsListForReading3.Count; num3++)
					{
						ThingDef thingDef3 = allDefsListForReading3[num3];
						if (thingDef3.thingSetMakerTags != null && thingDef3.thingSetMakerTags.Contains(this.thingSetMakerTagsToAllow[num2]))
						{
							this.SetAllow(thingDef3, true);
						}
					}
				}
			}
			if (this.thingSetMakerTagsToDisallow != null)
			{
				for (int num4 = 0; num4 < this.thingSetMakerTagsToDisallow.Count; num4++)
				{
					List<ThingDef> allDefsListForReading4 = DefDatabase<ThingDef>.AllDefsListForReading;
					for (int num5 = 0; num5 < allDefsListForReading4.Count; num5++)
					{
						ThingDef thingDef4 = allDefsListForReading4[num5];
						if (thingDef4.thingSetMakerTags != null && thingDef4.thingSetMakerTags.Contains(this.thingSetMakerTagsToDisallow[num4]))
						{
							this.SetAllow(thingDef4, false);
						}
					}
				}
			}
			if (this.disallowedCategories != null)
			{
				for (int num6 = 0; num6 < this.disallowedCategories.Count; num6++)
				{
					ThingCategoryDef named2 = DefDatabase<ThingCategoryDef>.GetNamed(this.disallowedCategories[num6], true);
					if (named2 != null)
					{
						this.SetAllow(named2, false, null, null);
					}
				}
			}
			if (this.specialFiltersToAllow != null)
			{
				for (int num7 = 0; num7 < this.specialFiltersToAllow.Count; num7++)
				{
					this.SetAllow(SpecialThingFilterDef.Named(this.specialFiltersToAllow[num7]), true);
				}
			}
			if (this.specialFiltersToDisallow != null)
			{
				for (int num8 = 0; num8 < this.specialFiltersToDisallow.Count; num8++)
				{
					this.SetAllow(SpecialThingFilterDef.Named(this.specialFiltersToDisallow[num8]), false);
				}
			}
			if (this.stuffCategoriesToAllow != null)
			{
				for (int num9 = 0; num9 < this.stuffCategoriesToAllow.Count; num9++)
				{
					this.SetAllow(this.stuffCategoriesToAllow[num9], true);
				}
			}
			if (this.allowAllWhoCanMake != null)
			{
				for (int num10 = 0; num10 < this.allowAllWhoCanMake.Count; num10++)
				{
					this.SetAllowAllWhoCanMake(this.allowAllWhoCanMake[num10]);
				}
			}
			if (this.disallowWorsePreferability != FoodPreferability.Undefined)
			{
				List<ThingDef> allDefsListForReading5 = DefDatabase<ThingDef>.AllDefsListForReading;
				for (int num11 = 0; num11 < allDefsListForReading5.Count; num11++)
				{
					ThingDef thingDef5 = allDefsListForReading5[num11];
					if (thingDef5.IsIngestible && thingDef5.ingestible.preferability != FoodPreferability.Undefined && thingDef5.ingestible.preferability < this.disallowWorsePreferability)
					{
						this.SetAllow(thingDef5, false);
					}
				}
			}
			if (this.disallowInedibleByHuman)
			{
				List<ThingDef> allDefsListForReading6 = DefDatabase<ThingDef>.AllDefsListForReading;
				for (int num12 = 0; num12 < allDefsListForReading6.Count; num12++)
				{
					ThingDef thingDef6 = allDefsListForReading6[num12];
					if (thingDef6.IsIngestible && !ThingDefOf.Human.race.CanEverEat(thingDef6))
					{
						this.SetAllow(thingDef6, false);
					}
				}
			}
			if (this.allowWithComp != null)
			{
				List<ThingDef> allDefsListForReading7 = DefDatabase<ThingDef>.AllDefsListForReading;
				for (int num13 = 0; num13 < allDefsListForReading7.Count; num13++)
				{
					ThingDef thingDef7 = allDefsListForReading7[num13];
					if (thingDef7.HasComp(this.allowWithComp))
					{
						this.SetAllow(thingDef7, true);
					}
				}
			}
			if (this.disallowWithComp != null)
			{
				List<ThingDef> allDefsListForReading8 = DefDatabase<ThingDef>.AllDefsListForReading;
				for (int num14 = 0; num14 < allDefsListForReading8.Count; num14++)
				{
					ThingDef thingDef8 = allDefsListForReading8[num14];
					if (thingDef8.HasComp(this.disallowWithComp))
					{
						this.SetAllow(thingDef8, false);
					}
				}
			}
			if (this.disallowCheaperThan != -3.40282347E+38f)
			{
				List<ThingDef> list = new List<ThingDef>();
				foreach (ThingDef thingDef9 in this.allowedDefs)
				{
					if (thingDef9.BaseMarketValue < this.disallowCheaperThan)
					{
						list.Add(thingDef9);
					}
				}
				for (int num15 = 0; num15 < list.Count; num15++)
				{
					this.SetAllow(list[num15], false);
				}
			}
			if (this.disallowedThingDefs != null)
			{
				for (int num16 = 0; num16 < this.disallowedThingDefs.Count; num16++)
				{
					if (this.disallowedThingDefs[num16] != null)
					{
						this.SetAllow(this.disallowedThingDefs[num16], false);
					}
					else
					{
						Log.Error("ThingFilter could not find excepted thing def named " + this.disallowedThingDefs[num16], false);
					}
				}
			}
			this.RecalculateDisplayRootCategory();
		}

		public void RecalculateDisplayRootCategory()
		{
			this.DisplayRootCategory = ThingCategoryNodeDatabase.RootNode;
			foreach (TreeNode_ThingCategory treeNode_ThingCategory in ThingCategoryNodeDatabase.AllThingCategoryNodes)
			{
				bool flag = false;
				bool flag2 = false;
				foreach (ThingDef value in this.allowedDefs)
				{
					if (treeNode_ThingCategory.catDef.DescendantThingDefs.Contains(value))
					{
						flag2 = true;
					}
					else
					{
						flag = true;
					}
				}
				if (!flag && flag2)
				{
					this.DisplayRootCategory = treeNode_ThingCategory;
				}
			}
		}

		private void RecalculateSpecialFilterConfigurability()
		{
			if (this.DisplayRootCategory == null)
			{
				this.allowedHitPointsConfigurable = true;
				this.allowedQualitiesConfigurable = true;
			}
			else
			{
				this.allowedHitPointsConfigurable = false;
				this.allowedQualitiesConfigurable = false;
				foreach (ThingDef thingDef in this.DisplayRootCategory.catDef.DescendantThingDefs)
				{
					if (thingDef.useHitPoints)
					{
						this.allowedHitPointsConfigurable = true;
					}
					if (thingDef.HasComp(typeof(CompQuality)))
					{
						this.allowedQualitiesConfigurable = true;
					}
					if (this.allowedHitPointsConfigurable && this.allowedQualitiesConfigurable)
					{
						break;
					}
				}
			}
		}

		public bool IsAlwaysDisallowedDueToSpecialFilters(ThingDef def)
		{
			for (int i = 0; i < this.disallowedSpecialFilters.Count; i++)
			{
				if (this.disallowedSpecialFilters[i].Worker.AlwaysMatches(def))
				{
					return true;
				}
			}
			return false;
		}

		public virtual void CopyAllowancesFrom(ThingFilter other)
		{
			this.allowedDefs.Clear();
			foreach (ThingDef thingDef in ThingFilter.AllStorableThingDefs)
			{
				this.SetAllow(thingDef, other.Allows(thingDef));
			}
			this.disallowedSpecialFilters = other.disallowedSpecialFilters.ListFullCopyOrNull<SpecialThingFilterDef>();
			this.allowedHitPointsPercents = other.allowedHitPointsPercents;
			this.allowedHitPointsConfigurable = other.allowedHitPointsConfigurable;
			this.allowedQualities = other.allowedQualities;
			this.allowedQualitiesConfigurable = other.allowedQualitiesConfigurable;
			this.thingDefs = other.thingDefs.ListFullCopyOrNull<ThingDef>();
			this.categories = other.categories.ListFullCopyOrNull<string>();
			this.tradeTagsToAllow = other.tradeTagsToAllow.ListFullCopyOrNull<string>();
			this.tradeTagsToDisallow = other.tradeTagsToDisallow.ListFullCopyOrNull<string>();
			this.thingSetMakerTagsToAllow = other.thingSetMakerTagsToAllow.ListFullCopyOrNull<string>();
			this.thingSetMakerTagsToDisallow = other.thingSetMakerTagsToDisallow.ListFullCopyOrNull<string>();
			this.disallowedCategories = other.disallowedCategories.ListFullCopyOrNull<string>();
			this.specialFiltersToAllow = other.specialFiltersToAllow.ListFullCopyOrNull<string>();
			this.specialFiltersToDisallow = other.specialFiltersToDisallow.ListFullCopyOrNull<string>();
			this.stuffCategoriesToAllow = other.stuffCategoriesToAllow.ListFullCopyOrNull<StuffCategoryDef>();
			this.allowAllWhoCanMake = other.allowAllWhoCanMake.ListFullCopyOrNull<ThingDef>();
			this.disallowWorsePreferability = other.disallowWorsePreferability;
			this.disallowInedibleByHuman = other.disallowInedibleByHuman;
			this.allowWithComp = other.allowWithComp;
			this.disallowWithComp = other.disallowWithComp;
			this.disallowCheaperThan = other.disallowCheaperThan;
			this.disallowedThingDefs = other.disallowedThingDefs.ListFullCopyOrNull<ThingDef>();
			if (this.settingsChangedCallback != null)
			{
				this.settingsChangedCallback();
			}
		}

		public void SetAllow(ThingDef thingDef, bool allow)
		{
			if (allow != this.Allows(thingDef))
			{
				if (allow)
				{
					this.allowedDefs.Add(thingDef);
				}
				else
				{
					this.allowedDefs.Remove(thingDef);
				}
				if (this.settingsChangedCallback != null)
				{
					this.settingsChangedCallback();
				}
			}
		}

		public void SetAllow(SpecialThingFilterDef sfDef, bool allow)
		{
			if (sfDef.configurable)
			{
				if (allow != this.Allows(sfDef))
				{
					if (allow)
					{
						if (this.disallowedSpecialFilters.Contains(sfDef))
						{
							this.disallowedSpecialFilters.Remove(sfDef);
						}
					}
					else if (!this.disallowedSpecialFilters.Contains(sfDef))
					{
						this.disallowedSpecialFilters.Add(sfDef);
					}
					if (this.settingsChangedCallback != null)
					{
						this.settingsChangedCallback();
					}
				}
			}
		}

		public void SetAllow(ThingCategoryDef categoryDef, bool allow, IEnumerable<ThingDef> exceptedDefs = null, IEnumerable<SpecialThingFilterDef> exceptedFilters = null)
		{
			if (!ThingCategoryNodeDatabase.initialized)
			{
				Log.Error("SetAllow categories won't work before ThingCategoryDatabase is initialized.", false);
			}
			foreach (ThingDef thingDef in categoryDef.DescendantThingDefs)
			{
				if (exceptedDefs == null || !exceptedDefs.Contains(thingDef))
				{
					this.SetAllow(thingDef, allow);
				}
			}
			foreach (SpecialThingFilterDef specialThingFilterDef in categoryDef.DescendantSpecialThingFilterDefs)
			{
				if (exceptedFilters == null || !exceptedFilters.Contains(specialThingFilterDef))
				{
					this.SetAllow(specialThingFilterDef, allow);
				}
			}
			if (this.settingsChangedCallback != null)
			{
				this.settingsChangedCallback();
			}
		}

		public void SetAllow(StuffCategoryDef cat, bool allow)
		{
			for (int i = 0; i < DefDatabase<ThingDef>.AllDefsListForReading.Count; i++)
			{
				ThingDef thingDef = DefDatabase<ThingDef>.AllDefsListForReading[i];
				if (thingDef.IsStuff && thingDef.stuffCategories.Contains(cat))
				{
					this.SetAllow(thingDef, true);
				}
			}
			if (this.settingsChangedCallback != null)
			{
				this.settingsChangedCallback();
			}
		}

		public void SetAllowAllWhoCanMake(ThingDef thing)
		{
			for (int i = 0; i < DefDatabase<ThingDef>.AllDefsListForReading.Count; i++)
			{
				ThingDef thingDef = DefDatabase<ThingDef>.AllDefsListForReading[i];
				if (thingDef.IsStuff && thingDef.stuffProps.CanMake(thing))
				{
					this.SetAllow(thingDef, true);
				}
			}
			if (this.settingsChangedCallback != null)
			{
				this.settingsChangedCallback();
			}
		}

		public void SetFromPreset(StorageSettingsPreset preset)
		{
			if (preset == StorageSettingsPreset.DefaultStockpile)
			{
				this.SetAllow(ThingCategoryDefOf.Foods, true, null, null);
				this.SetAllow(ThingCategoryDefOf.Manufactured, true, null, null);
				this.SetAllow(ThingCategoryDefOf.ResourcesRaw, true, null, null);
				this.SetAllow(ThingCategoryDefOf.Items, true, null, null);
				this.SetAllow(ThingCategoryDefOf.Buildings, true, null, null);
				this.SetAllow(ThingCategoryDefOf.Weapons, true, null, null);
				this.SetAllow(ThingCategoryDefOf.Apparel, true, null, null);
				this.SetAllow(ThingCategoryDefOf.BodyParts, true, null, null);
			}
			if (preset == StorageSettingsPreset.DumpingStockpile)
			{
				this.SetAllow(ThingCategoryDefOf.Corpses, true, null, null);
				this.SetAllow(ThingCategoryDefOf.Chunks, true, null, null);
			}
			if (this.settingsChangedCallback != null)
			{
				this.settingsChangedCallback();
			}
		}

		public void SetDisallowAll(IEnumerable<ThingDef> exceptedDefs = null, IEnumerable<SpecialThingFilterDef> exceptedFilters = null)
		{
			this.allowedDefs.RemoveWhere((ThingDef d) => exceptedDefs == null || !exceptedDefs.Contains(d));
			this.disallowedSpecialFilters.RemoveAll((SpecialThingFilterDef sf) => sf.configurable && (exceptedFilters == null || !exceptedFilters.Contains(sf)));
			if (this.settingsChangedCallback != null)
			{
				this.settingsChangedCallback();
			}
		}

		public void SetAllowAll(ThingFilter parentFilter)
		{
			this.allowedDefs.Clear();
			if (parentFilter != null)
			{
				foreach (ThingDef item in parentFilter.allowedDefs)
				{
					this.allowedDefs.Add(item);
				}
			}
			else
			{
				foreach (ThingDef item2 in ThingFilter.AllStorableThingDefs)
				{
					this.allowedDefs.Add(item2);
				}
			}
			this.disallowedSpecialFilters.RemoveAll((SpecialThingFilterDef sf) => sf.configurable);
			if (this.settingsChangedCallback != null)
			{
				this.settingsChangedCallback();
			}
		}

		public virtual bool Allows(Thing t)
		{
			t = t.GetInnerIfMinified();
			bool result;
			if (!this.Allows(t.def))
			{
				result = false;
			}
			else
			{
				if (t.def.useHitPoints)
				{
					float num = (float)t.HitPoints / (float)t.MaxHitPoints;
					num = GenMath.RoundedHundredth(num);
					if (!this.allowedHitPointsPercents.IncludesEpsilon(Mathf.Clamp01(num)))
					{
						return false;
					}
				}
				if (this.allowedQualities != QualityRange.All && t.def.FollowQualityThingFilter())
				{
					QualityCategory p;
					if (!t.TryGetQuality(out p))
					{
						p = QualityCategory.Normal;
					}
					if (!this.allowedQualities.Includes(p))
					{
						return false;
					}
				}
				for (int i = 0; i < this.disallowedSpecialFilters.Count; i++)
				{
					if (this.disallowedSpecialFilters[i].Worker.Matches(t))
					{
						return false;
					}
				}
				result = true;
			}
			return result;
		}

		public bool Allows(ThingDef def)
		{
			return this.allowedDefs.Contains(def);
		}

		public bool Allows(SpecialThingFilterDef sf)
		{
			return !this.disallowedSpecialFilters.Contains(sf);
		}

		public ThingRequest GetThingRequest()
		{
			ThingRequest result;
			if (this.AllowedThingDefs.Any((ThingDef def) => !def.alwaysHaulable))
			{
				result = ThingRequest.ForGroup(ThingRequestGroup.HaulableEver);
			}
			else
			{
				result = ThingRequest.ForGroup(ThingRequestGroup.HaulableAlways);
			}
			return result;
		}

		public override string ToString()
		{
			return this.Summary;
		}

		[CompilerGenerated]
		private static bool <get_AllStorableThingDefs>m__0(ThingDef def)
		{
			return def.EverStorable(true);
		}

		[CompilerGenerated]
		private static bool <SetAllowAll>m__1(SpecialThingFilterDef sf)
		{
			return sf.configurable;
		}

		[CompilerGenerated]
		private static bool <GetThingRequest>m__2(ThingDef def)
		{
			return !def.alwaysHaulable;
		}

		[CompilerGenerated]
		private sealed class <SetDisallowAll>c__AnonStorey0
		{
			internal IEnumerable<ThingDef> exceptedDefs;

			internal IEnumerable<SpecialThingFilterDef> exceptedFilters;

			public <SetDisallowAll>c__AnonStorey0()
			{
			}

			internal bool <>m__0(ThingDef d)
			{
				return this.exceptedDefs == null || !this.exceptedDefs.Contains(d);
			}

			internal bool <>m__1(SpecialThingFilterDef sf)
			{
				return sf.configurable && (this.exceptedFilters == null || !this.exceptedFilters.Contains(sf));
			}
		}
	}
}
