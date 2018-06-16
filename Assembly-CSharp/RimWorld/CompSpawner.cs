﻿using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	// Token: 0x02000739 RID: 1849
	public class CompSpawner : ThingComp
	{
		// Token: 0x1700064E RID: 1614
		// (get) Token: 0x060028C1 RID: 10433 RVA: 0x0015B700 File Offset: 0x00159B00
		public CompProperties_Spawner PropsSpawner
		{
			get
			{
				return (CompProperties_Spawner)this.props;
			}
		}

		// Token: 0x1700064F RID: 1615
		// (get) Token: 0x060028C2 RID: 10434 RVA: 0x0015B720 File Offset: 0x00159B20
		private bool PowerOn
		{
			get
			{
				CompPowerTrader comp = this.parent.GetComp<CompPowerTrader>();
				return comp != null && comp.PowerOn;
			}
		}

		// Token: 0x060028C3 RID: 10435 RVA: 0x0015B750 File Offset: 0x00159B50
		public override void PostSpawnSetup(bool respawningAfterLoad)
		{
			if (!respawningAfterLoad)
			{
				this.ResetCountdown();
			}
		}

		// Token: 0x060028C4 RID: 10436 RVA: 0x0015B75F File Offset: 0x00159B5F
		public override void CompTick()
		{
			this.TickInterval(1);
		}

		// Token: 0x060028C5 RID: 10437 RVA: 0x0015B769 File Offset: 0x00159B69
		public override void CompTickRare()
		{
			this.TickInterval(250);
		}

		// Token: 0x060028C6 RID: 10438 RVA: 0x0015B778 File Offset: 0x00159B78
		private void TickInterval(int interval)
		{
			if (this.parent.Spawned)
			{
				Hive hive = this.parent as Hive;
				if (hive != null)
				{
					if (!hive.active)
					{
						return;
					}
				}
				else if (this.parent.Position.Fogged(this.parent.Map))
				{
					return;
				}
				if (!this.PropsSpawner.requiresPower || this.PowerOn)
				{
					this.ticksUntilSpawn -= interval;
					this.CheckShouldSpawn();
				}
			}
		}

		// Token: 0x060028C7 RID: 10439 RVA: 0x0015B81D File Offset: 0x00159C1D
		private void CheckShouldSpawn()
		{
			if (this.ticksUntilSpawn <= 0)
			{
				this.TryDoSpawn();
				this.ResetCountdown();
			}
		}

		// Token: 0x060028C8 RID: 10440 RVA: 0x0015B83C File Offset: 0x00159C3C
		public bool TryDoSpawn()
		{
			bool result;
			if (!this.parent.Spawned)
			{
				result = false;
			}
			else
			{
				if (this.PropsSpawner.spawnMaxAdjacent >= 0)
				{
					int num = 0;
					for (int i = 0; i < 9; i++)
					{
						IntVec3 c = this.parent.Position + GenAdj.AdjacentCellsAndInside[i];
						if (c.InBounds(this.parent.Map))
						{
							List<Thing> thingList = c.GetThingList(this.parent.Map);
							for (int j = 0; j < thingList.Count; j++)
							{
								if (thingList[j].def == this.PropsSpawner.thingToSpawn)
								{
									num += thingList[j].stackCount;
									if (num >= this.PropsSpawner.spawnMaxAdjacent)
									{
										return false;
									}
								}
							}
						}
					}
				}
				IntVec3 center;
				if (this.TryFindSpawnCell(out center))
				{
					Thing thing = ThingMaker.MakeThing(this.PropsSpawner.thingToSpawn, null);
					thing.stackCount = this.PropsSpawner.spawnCount;
					Thing t;
					GenPlace.TryPlaceThing(thing, center, this.parent.Map, ThingPlaceMode.Direct, out t, null, null);
					if (this.PropsSpawner.spawnForbidden)
					{
						t.SetForbidden(true, true);
					}
					if (this.PropsSpawner.showMessageIfOwned && this.parent.Faction == Faction.OfPlayer)
					{
						Messages.Message("MessageCompSpawnerSpawnedItem".Translate(new object[]
						{
							this.PropsSpawner.thingToSpawn.LabelCap
						}).CapitalizeFirst(), thing, MessageTypeDefOf.PositiveEvent, true);
					}
					result = true;
				}
				else
				{
					result = false;
				}
			}
			return result;
		}

		// Token: 0x060028C9 RID: 10441 RVA: 0x0015BA14 File Offset: 0x00159E14
		private bool TryFindSpawnCell(out IntVec3 result)
		{
			foreach (IntVec3 intVec in GenAdj.CellsAdjacent8Way(this.parent).InRandomOrder(null))
			{
				if (intVec.Walkable(this.parent.Map))
				{
					Building edifice = intVec.GetEdifice(this.parent.Map);
					if (edifice == null || !this.PropsSpawner.thingToSpawn.IsEdifice())
					{
						Building_Door building_Door = edifice as Building_Door;
						if (building_Door == null || building_Door.FreePassage)
						{
							if (this.parent.def.passability == Traversability.Impassable || GenSight.LineOfSight(this.parent.Position, intVec, this.parent.Map, false, null, 0, 0))
							{
								bool flag = false;
								List<Thing> thingList = intVec.GetThingList(this.parent.Map);
								for (int i = 0; i < thingList.Count; i++)
								{
									Thing thing = thingList[i];
									if (thing.def.category == ThingCategory.Item && (thing.def != this.PropsSpawner.thingToSpawn || thing.stackCount > this.PropsSpawner.thingToSpawn.stackLimit - this.PropsSpawner.spawnCount))
									{
										flag = true;
										break;
									}
								}
								if (!flag)
								{
									result = intVec;
									return true;
								}
							}
						}
					}
				}
			}
			result = IntVec3.Invalid;
			return false;
		}

		// Token: 0x060028CA RID: 10442 RVA: 0x0015BBF4 File Offset: 0x00159FF4
		private void ResetCountdown()
		{
			this.ticksUntilSpawn = this.PropsSpawner.spawnIntervalRange.RandomInRange;
		}

		// Token: 0x060028CB RID: 10443 RVA: 0x0015BC10 File Offset: 0x0015A010
		public override void PostExposeData()
		{
			string str = (!this.PropsSpawner.saveKeysPrefix.NullOrEmpty()) ? (this.PropsSpawner.saveKeysPrefix + "_") : null;
			Scribe_Values.Look<int>(ref this.ticksUntilSpawn, str + "ticksUntilSpawn", 0, false);
		}

		// Token: 0x060028CC RID: 10444 RVA: 0x0015BC68 File Offset: 0x0015A068
		public override IEnumerable<Gizmo> CompGetGizmosExtra()
		{
			if (Prefs.DevMode)
			{
				yield return new Command_Action
				{
					defaultLabel = "DEBUG: Spawn " + this.PropsSpawner.thingToSpawn.label,
					icon = TexCommand.DesirePower,
					action = delegate()
					{
						this.TryDoSpawn();
						this.ResetCountdown();
					}
				};
			}
			yield break;
		}

		// Token: 0x060028CD RID: 10445 RVA: 0x0015BC94 File Offset: 0x0015A094
		public override string CompInspectStringExtra()
		{
			string result;
			if (this.PropsSpawner.writeTimeLeftToSpawn && (!this.PropsSpawner.requiresPower || this.PowerOn))
			{
				result = "NextSpawnedItemIn".Translate(new object[]
				{
					GenLabel.ThingLabel(this.PropsSpawner.thingToSpawn, null, this.PropsSpawner.spawnCount)
				}) + ": " + this.ticksUntilSpawn.ToStringTicksToPeriod();
			}
			else
			{
				result = null;
			}
			return result;
		}

		// Token: 0x04001653 RID: 5715
		private int ticksUntilSpawn;
	}
}
