﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Verse;

namespace RimWorld
{
	public static class PawnWeaponGenerator
	{
		private static List<ThingStuffPair> allWeaponPairs;

		private static List<ThingStuffPair> workingWeapons = new List<ThingStuffPair>();

		[CompilerGenerated]
		private static Predicate<ThingDef> <>f__am$cache0;

		[CompilerGenerated]
		private static Func<ThingStuffPair, float> <>f__am$cache1;

		[CompilerGenerated]
		private static Func<ThingStuffPair, float> <>f__am$cache2;

		[CompilerGenerated]
		private static Func<ThingStuffPair, string> <>f__am$cache3;

		[CompilerGenerated]
		private static Func<ThingStuffPair, string> <>f__am$cache4;

		[CompilerGenerated]
		private static Func<ThingStuffPair, string> <>f__am$cache5;

		[CompilerGenerated]
		private static Func<ThingStuffPair, string> <>f__am$cache6;

		[CompilerGenerated]
		private static Func<ThingStuffPair, string> <>f__am$cache7;

		[CompilerGenerated]
		private static Func<ThingStuffPair, string> <>f__am$cache8;

		[CompilerGenerated]
		private static Func<ThingStuffPair, string> <>f__am$cache9;

		[CompilerGenerated]
		private static Func<ThingStuffPair, string> <>f__am$cacheA;

		public static void Reset()
		{
			Predicate<ThingDef> isWeapon = (ThingDef td) => td.equipmentType == EquipmentType.Primary && !td.weaponTags.NullOrEmpty<string>();
			PawnWeaponGenerator.allWeaponPairs = ThingStuffPair.AllWith(isWeapon);
			using (IEnumerator<ThingDef> enumerator = (from td in DefDatabase<ThingDef>.AllDefs
			where isWeapon(td)
			select td).GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					ThingDef thingDef = enumerator.Current;
					float num = (from pa in PawnWeaponGenerator.allWeaponPairs
					where pa.thing == thingDef
					select pa).Sum((ThingStuffPair pa) => pa.Commonality);
					float num2 = thingDef.generateCommonality / num;
					if (num2 != 1f)
					{
						for (int i = 0; i < PawnWeaponGenerator.allWeaponPairs.Count; i++)
						{
							ThingStuffPair thingStuffPair = PawnWeaponGenerator.allWeaponPairs[i];
							if (thingStuffPair.thing == thingDef)
							{
								PawnWeaponGenerator.allWeaponPairs[i] = new ThingStuffPair(thingStuffPair.thing, thingStuffPair.stuff, thingStuffPair.commonalityMultiplier * num2);
							}
						}
					}
				}
			}
		}

		public static void TryGenerateWeaponFor(Pawn pawn)
		{
			PawnWeaponGenerator.workingWeapons.Clear();
			if (pawn.kindDef.weaponTags != null && pawn.kindDef.weaponTags.Count != 0)
			{
				if (pawn.RaceProps.ToolUser)
				{
					if (pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
					{
						if (pawn.story == null || !pawn.story.WorkTagIsDisabled(WorkTags.Violent))
						{
							float randomInRange = pawn.kindDef.weaponMoney.RandomInRange;
							for (int i = 0; i < PawnWeaponGenerator.allWeaponPairs.Count; i++)
							{
								ThingStuffPair w = PawnWeaponGenerator.allWeaponPairs[i];
								if (w.Price <= randomInRange)
								{
									if (pawn.kindDef.weaponTags == null || pawn.kindDef.weaponTags.Any((string tag) => w.thing.weaponTags.Contains(tag)))
									{
										if (w.thing.generateAllowChance < 1f)
										{
											if (!Rand.ChanceSeeded(w.thing.generateAllowChance, pawn.thingIDNumber ^ (int)w.thing.shortHash ^ 28554824))
											{
												goto IL_16F;
											}
										}
										PawnWeaponGenerator.workingWeapons.Add(w);
									}
								}
								IL_16F:;
							}
							if (PawnWeaponGenerator.workingWeapons.Count != 0)
							{
								pawn.equipment.DestroyAllEquipment(DestroyMode.Vanish);
								ThingStuffPair thingStuffPair;
								if (PawnWeaponGenerator.workingWeapons.TryRandomElementByWeight((ThingStuffPair w) => w.Commonality * w.Price, out thingStuffPair))
								{
									ThingWithComps thingWithComps = (ThingWithComps)ThingMaker.MakeThing(thingStuffPair.thing, thingStuffPair.stuff);
									PawnGenerator.PostProcessGeneratedGear(thingWithComps, pawn);
									pawn.equipment.AddEquipment(thingWithComps);
								}
								PawnWeaponGenerator.workingWeapons.Clear();
							}
						}
					}
				}
			}
		}

		public static bool IsDerpWeapon(ThingDef thing, ThingDef stuff)
		{
			bool result;
			if (stuff == null)
			{
				result = false;
			}
			else
			{
				if (thing.IsMeleeWeapon)
				{
					if (thing.tools.NullOrEmpty<Tool>())
					{
						return false;
					}
					DamageDef damageDef = ThingUtility.PrimaryMeleeWeaponDamageType(thing);
					if (damageDef == null)
					{
						return false;
					}
					DamageArmorCategoryDef armorCategory = damageDef.armorCategory;
					if (armorCategory != null && armorCategory.multStat != null && stuff.GetStatValueAbstract(armorCategory.multStat, null) < 0.7f)
					{
						return true;
					}
				}
				result = false;
			}
			return result;
		}

		public static float CheapestNonDerpPriceFor(ThingDef weaponDef)
		{
			float num = 9999999f;
			for (int i = 0; i < PawnWeaponGenerator.allWeaponPairs.Count; i++)
			{
				ThingStuffPair thingStuffPair = PawnWeaponGenerator.allWeaponPairs[i];
				if (thingStuffPair.thing == weaponDef && !PawnWeaponGenerator.IsDerpWeapon(thingStuffPair.thing, thingStuffPair.stuff))
				{
					if (thingStuffPair.Price < num)
					{
						num = thingStuffPair.Price;
					}
				}
			}
			return num;
		}

		[DebugOutput]
		internal static void WeaponPairs()
		{
			IEnumerable<ThingStuffPair> dataSources = from p in PawnWeaponGenerator.allWeaponPairs
			orderby p.thing.defName descending
			select p;
			TableDataGetter<ThingStuffPair>[] array = new TableDataGetter<ThingStuffPair>[7];
			array[0] = new TableDataGetter<ThingStuffPair>("thing", (ThingStuffPair p) => p.thing.defName);
			array[1] = new TableDataGetter<ThingStuffPair>("stuff", (ThingStuffPair p) => (p.stuff == null) ? "" : p.stuff.defName);
			array[2] = new TableDataGetter<ThingStuffPair>("price", (ThingStuffPair p) => p.Price.ToString());
			array[3] = new TableDataGetter<ThingStuffPair>("commonality", (ThingStuffPair p) => p.Commonality.ToString("F5"));
			array[4] = new TableDataGetter<ThingStuffPair>("commMult", (ThingStuffPair p) => p.commonalityMultiplier.ToString("F5"));
			array[5] = new TableDataGetter<ThingStuffPair>("generateCommonality", (ThingStuffPair p) => p.thing.generateCommonality.ToString("F2"));
			array[6] = new TableDataGetter<ThingStuffPair>("derp", (ThingStuffPair p) => (!PawnWeaponGenerator.IsDerpWeapon(p.thing, p.stuff)) ? "" : "D");
			DebugTables.MakeTablesDialog<ThingStuffPair>(dataSources, array);
		}

		[DebugOutput]
		internal static void WeaponPairsByThing()
		{
			DebugOutputsGeneral.MakeTablePairsByThing(PawnWeaponGenerator.allWeaponPairs);
		}

		// Note: this type is marked as 'beforefieldinit'.
		static PawnWeaponGenerator()
		{
		}

		[CompilerGenerated]
		private static bool <Reset>m__0(ThingDef td)
		{
			return td.equipmentType == EquipmentType.Primary && !td.weaponTags.NullOrEmpty<string>();
		}

		[CompilerGenerated]
		private static float <Reset>m__1(ThingStuffPair pa)
		{
			return pa.Commonality;
		}

		[CompilerGenerated]
		private static float <TryGenerateWeaponFor>m__2(ThingStuffPair w)
		{
			return w.Commonality * w.Price;
		}

		[CompilerGenerated]
		private static string <WeaponPairs>m__3(ThingStuffPair p)
		{
			return p.thing.defName;
		}

		[CompilerGenerated]
		private static string <WeaponPairs>m__4(ThingStuffPair p)
		{
			return p.thing.defName;
		}

		[CompilerGenerated]
		private static string <WeaponPairs>m__5(ThingStuffPair p)
		{
			return (p.stuff == null) ? "" : p.stuff.defName;
		}

		[CompilerGenerated]
		private static string <WeaponPairs>m__6(ThingStuffPair p)
		{
			return p.Price.ToString();
		}

		[CompilerGenerated]
		private static string <WeaponPairs>m__7(ThingStuffPair p)
		{
			return p.Commonality.ToString("F5");
		}

		[CompilerGenerated]
		private static string <WeaponPairs>m__8(ThingStuffPair p)
		{
			return p.commonalityMultiplier.ToString("F5");
		}

		[CompilerGenerated]
		private static string <WeaponPairs>m__9(ThingStuffPair p)
		{
			return p.thing.generateCommonality.ToString("F2");
		}

		[CompilerGenerated]
		private static string <WeaponPairs>m__A(ThingStuffPair p)
		{
			return (!PawnWeaponGenerator.IsDerpWeapon(p.thing, p.stuff)) ? "" : "D";
		}

		[CompilerGenerated]
		private sealed class <Reset>c__AnonStorey0
		{
			internal Predicate<ThingDef> isWeapon;

			public <Reset>c__AnonStorey0()
			{
			}

			internal bool <>m__0(ThingDef td)
			{
				return this.isWeapon(td);
			}
		}

		[CompilerGenerated]
		private sealed class <Reset>c__AnonStorey1
		{
			internal ThingDef thingDef;

			public <Reset>c__AnonStorey1()
			{
			}

			internal bool <>m__0(ThingStuffPair pa)
			{
				return pa.thing == this.thingDef;
			}
		}

		[CompilerGenerated]
		private sealed class <TryGenerateWeaponFor>c__AnonStorey2
		{
			internal ThingStuffPair w;

			public <TryGenerateWeaponFor>c__AnonStorey2()
			{
			}

			internal bool <>m__0(string tag)
			{
				return this.w.thing.weaponTags.Contains(tag);
			}
		}
	}
}
