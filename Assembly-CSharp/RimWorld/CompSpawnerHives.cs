using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class CompSpawnerHives : ThingComp
	{
		public const int MaxHivesPerMap = 30;

		private int nextHiveSpawnTick = -1;

		private CompProperties_SpawnerHives Props
		{
			get
			{
				return (CompProperties_SpawnerHives)base.props;
			}
		}

		private bool CanSpawnChildHive
		{
			get
			{
				return HivesUtility.TotalSpawnedHivesCount(base.parent.Map) < 30;
			}
		}

		public override void PostSpawnSetup(bool respawningAfterLoad)
		{
			if (!respawningAfterLoad)
			{
				this.CalculateNextHiveSpawnTick();
			}
		}

		public override void CompTickRare()
		{
			Hive hive = base.parent as Hive;
			if (hive != null && !hive.active)
				return;
			if (Find.TickManager.TicksGame >= this.nextHiveSpawnTick)
			{
				Hive hive2 = default(Hive);
				if (this.TrySpawnChildHive(false, out hive2))
				{
					hive2.nextPawnSpawnTick = Find.TickManager.TicksGame + Rand.Range(150, 350);
					Messages.Message("MessageHiveReproduced".Translate(), (Thing)hive2, MessageSound.Negative);
				}
				else
				{
					this.CalculateNextHiveSpawnTick();
				}
			}
		}

		public override string CompInspectStringExtra()
		{
			string text = (string)null;
			if (this.CanSpawnChildHive)
			{
				text = text + "HiveReproducesIn".Translate() + ": " + (this.nextHiveSpawnTick - Find.TickManager.TicksGame).ToStringTicksToPeriod(true, false, true);
			}
			return text;
		}

		public void CalculateNextHiveSpawnTick()
		{
			Room room = base.parent.GetRoom(RegionType.Set_Passable);
			int num = 0;
			int num2 = GenRadial.NumCellsInRadius(9f);
			for (int num3 = 0; num3 < num2; num3++)
			{
				IntVec3 intVec = base.parent.Position + GenRadial.RadialPattern[num3];
				if (intVec.InBounds(base.parent.Map) && intVec.GetRoom(base.parent.Map, RegionType.Set_Passable) == room && intVec.GetThingList(base.parent.Map).Any((Predicate<Thing>)((Thing t) => t is Hive)))
				{
					num++;
				}
			}
			float num4 = GenMath.LerpDouble(0f, 7f, 1f, 0.35f, (float)Mathf.Clamp(num, 0, 7));
			this.nextHiveSpawnTick = Find.TickManager.TicksGame + (int)(this.Props.HiveSpawnIntervalDays.RandomInRange * 60000.0 / (num4 * Find.Storyteller.difficulty.enemyReproductionRateFactor));
		}

		public bool TrySpawnChildHive(bool ignoreRoofedRequirement, out Hive newHive)
		{
			if (!this.CanSpawnChildHive)
			{
				newHive = null;
				return false;
			}
			IntVec3 invalid = IntVec3.Invalid;
			int num = 0;
			while (num < 3)
			{
				float minDist = this.Props.HiveSpawnPreferredMinDist;
				switch (num)
				{
				case 1:
				{
					minDist = 0f;
					break;
				}
				case 2:
				{
					newHive = null;
					return false;
				}
				}
				if (!CellFinder.TryFindRandomReachableCellNear(base.parent.Position, base.parent.Map, this.Props.HiveSpawnRadius, TraverseParms.For(TraverseMode.NoPassClosedDoors, Danger.Deadly, false), (Predicate<IntVec3>)((IntVec3 c) => this.CanSpawnHiveAt(c, minDist, ignoreRoofedRequirement)), (Predicate<Region>)null, out invalid, 999999))
				{
					num++;
					continue;
				}
				break;
			}
			newHive = (Hive)GenSpawn.Spawn(base.parent.def, invalid, base.parent.Map);
			if (newHive.Faction != base.parent.Faction)
			{
				newHive.SetFaction(base.parent.Faction, null);
			}
			Hive hive = base.parent as Hive;
			if (hive != null)
			{
				newHive.active = hive.active;
			}
			this.CalculateNextHiveSpawnTick();
			return true;
		}

		private bool CanSpawnHiveAt(IntVec3 c, float minDist, bool ignoreRoofedRequirement)
		{
			if ((ignoreRoofedRequirement || c.Roofed(base.parent.Map)) && c.Standable(base.parent.Map) && (minDist == 0.0 || (float)c.DistanceToSquared(base.parent.Position) >= minDist * minDist))
			{
				for (int i = 0; i < 8; i++)
				{
					IntVec3 c2 = c + GenAdj.AdjacentCells[i];
					if (c2.InBounds(base.parent.Map))
					{
						List<Thing> thingList = c2.GetThingList(base.parent.Map);
						for (int j = 0; j < thingList.Count; j++)
						{
							if (thingList[j] is Hive)
							{
								return false;
							}
						}
					}
				}
				List<Thing> thingList2 = c.GetThingList(base.parent.Map);
				for (int k = 0; k < thingList2.Count; k++)
				{
					Thing thing = thingList2[k];
					if ((thing.def.category == ThingCategory.Item || thing.def.category == ThingCategory.Building) && GenSpawn.SpawningWipes(base.parent.def, thing.def))
					{
						return false;
					}
				}
				return true;
			}
			return false;
		}

		public override IEnumerable<Gizmo> CompGetGizmosExtra()
		{
			if (Prefs.DevMode)
			{
				yield return (Gizmo)new Command_Action
				{
					defaultLabel = "DEBUG: Reproduce",
					icon = TexCommand.GatherSpotActive,
					action = (Action)delegate
					{
						Hive hive = default(Hive);
						((_003CCompGetGizmosExtra_003Ec__Iterator16C)/*Error near IL_005c: stateMachine*/)._003C_003Ef__this.TrySpawnChildHive(false, out hive);
					}
				};
			}
		}

		public override void PostExposeData()
		{
			Scribe_Values.Look<int>(ref this.nextHiveSpawnTick, "nextHiveSpawnTick", 0, false);
		}
	}
}
