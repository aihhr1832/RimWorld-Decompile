﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using RimWorld.Planet;
using Verse.AI;

namespace Verse
{
	// Token: 0x02000C35 RID: 3125
	public sealed class MapPawns
	{
		// Token: 0x06004493 RID: 17555 RVA: 0x00240250 File Offset: 0x0023E650
		public MapPawns(Map map)
		{
			this.map = map;
		}

		// Token: 0x17000ABA RID: 2746
		// (get) Token: 0x06004494 RID: 17556 RVA: 0x002402A4 File Offset: 0x0023E6A4
		public IEnumerable<Pawn> AllPawns
		{
			get
			{
				for (int i = 0; i < this.pawnsSpawned.Count; i++)
				{
					yield return this.pawnsSpawned[i];
				}
				foreach (Pawn p in this.AllPawnsUnspawned)
				{
					yield return p;
				}
				yield break;
			}
		}

		// Token: 0x17000ABB RID: 2747
		// (get) Token: 0x06004495 RID: 17557 RVA: 0x002402D0 File Offset: 0x0023E6D0
		public IEnumerable<Pawn> AllPawnsUnspawned
		{
			get
			{
				this.tmpUnspawnedPawns.Clear();
				ThingOwnerUtility.GetAllThingsRecursively<Pawn>(this.map, ThingRequest.ForGroup(ThingRequestGroup.Pawn), this.tmpUnspawnedPawns, true, null, false);
				for (int j = this.tmpUnspawnedPawns.Count - 1; j >= 0; j--)
				{
					if (this.tmpUnspawnedPawns[j].Dead)
					{
						this.tmpUnspawnedPawns.RemoveAt(j);
					}
				}
				for (int i = this.tmpUnspawnedPawns.Count - 1; i >= 0; i--)
				{
					yield return this.tmpUnspawnedPawns[i];
				}
				yield break;
			}
		}

		// Token: 0x17000ABC RID: 2748
		// (get) Token: 0x06004496 RID: 17558 RVA: 0x002402FC File Offset: 0x0023E6FC
		public IEnumerable<Pawn> FreeColonists
		{
			get
			{
				return this.FreeHumanlikesOfFaction(Faction.OfPlayer);
			}
		}

		// Token: 0x17000ABD RID: 2749
		// (get) Token: 0x06004497 RID: 17559 RVA: 0x0024031C File Offset: 0x0023E71C
		public IEnumerable<Pawn> PrisonersOfColony
		{
			get
			{
				return from x in this.AllPawns
				where x.IsPrisonerOfColony
				select x;
			}
		}

		// Token: 0x17000ABE RID: 2750
		// (get) Token: 0x06004498 RID: 17560 RVA: 0x0024035C File Offset: 0x0023E75C
		public IEnumerable<Pawn> FreeColonistsAndPrisoners
		{
			get
			{
				return this.FreeColonists.Concat(this.PrisonersOfColony);
			}
		}

		// Token: 0x17000ABF RID: 2751
		// (get) Token: 0x06004499 RID: 17561 RVA: 0x00240384 File Offset: 0x0023E784
		public int ColonistCount
		{
			get
			{
				int result;
				if (Current.ProgramState != ProgramState.Playing)
				{
					Log.Error("ColonistCount while not playing. This should get the starting player pawn count.", false);
					result = 3;
				}
				else
				{
					result = this.AllPawns.Count((Pawn x) => x.IsColonist);
				}
				return result;
			}
		}

		// Token: 0x17000AC0 RID: 2752
		// (get) Token: 0x0600449A RID: 17562 RVA: 0x002403E0 File Offset: 0x0023E7E0
		public int AllPawnsCount
		{
			get
			{
				return this.AllPawns.Count<Pawn>();
			}
		}

		// Token: 0x17000AC1 RID: 2753
		// (get) Token: 0x0600449B RID: 17563 RVA: 0x00240400 File Offset: 0x0023E800
		public int AllPawnsUnspawnedCount
		{
			get
			{
				return this.AllPawnsUnspawned.Count<Pawn>();
			}
		}

		// Token: 0x17000AC2 RID: 2754
		// (get) Token: 0x0600449C RID: 17564 RVA: 0x00240420 File Offset: 0x0023E820
		public int FreeColonistsCount
		{
			get
			{
				return this.FreeColonists.Count<Pawn>();
			}
		}

		// Token: 0x17000AC3 RID: 2755
		// (get) Token: 0x0600449D RID: 17565 RVA: 0x00240440 File Offset: 0x0023E840
		public int PrisonersOfColonyCount
		{
			get
			{
				return this.PrisonersOfColony.Count<Pawn>();
			}
		}

		// Token: 0x17000AC4 RID: 2756
		// (get) Token: 0x0600449E RID: 17566 RVA: 0x00240460 File Offset: 0x0023E860
		public int FreeColonistsAndPrisonersCount
		{
			get
			{
				return this.PrisonersOfColony.Count<Pawn>();
			}
		}

		// Token: 0x17000AC5 RID: 2757
		// (get) Token: 0x0600449F RID: 17567 RVA: 0x00240480 File Offset: 0x0023E880
		public bool AnyPawnBlockingMapRemoval
		{
			get
			{
				Faction ofPlayer = Faction.OfPlayer;
				int i = 0;
				while (i < this.pawnsSpawned.Count)
				{
					bool result;
					if (!this.pawnsSpawned[i].Downed && this.pawnsSpawned[i].IsColonist)
					{
						result = true;
					}
					else if (this.pawnsSpawned[i].relations != null && this.pawnsSpawned[i].relations.relativeInvolvedInRescueQuest != null)
					{
						result = true;
					}
					else
					{
						if (this.pawnsSpawned[i].Faction == ofPlayer || this.pawnsSpawned[i].HostFaction == ofPlayer)
						{
							Job curJob = this.pawnsSpawned[i].CurJob;
							if (curJob != null && curJob.exitMapOnArrival)
							{
								return true;
							}
						}
						if (CaravanExitMapUtility.FindCaravanToJoinFor(this.pawnsSpawned[i]) == null || this.pawnsSpawned[i].Downed)
						{
							i++;
							continue;
						}
						result = true;
					}
					return result;
				}
				List<Thing> list = this.map.listerThings.ThingsInGroup(ThingRequestGroup.ThingHolder);
				for (int j = 0; j < list.Count; j++)
				{
					if (list[j] is IActiveDropPod || list[j].TryGetComp<CompTransporter>() != null)
					{
						IThingHolder holder = list[j].TryGetComp<CompTransporter>() ?? ((IThingHolder)list[j]);
						this.tmpThings.Clear();
						ThingOwnerUtility.GetAllThingsRecursively(holder, this.tmpThings, true, null);
						for (int k = 0; k < this.tmpThings.Count; k++)
						{
							Pawn pawn = this.tmpThings[k] as Pawn;
							if (pawn != null && !pawn.Dead && !pawn.Downed && pawn.IsColonist)
							{
								this.tmpThings.Clear();
								return true;
							}
						}
					}
				}
				this.tmpThings.Clear();
				return false;
			}
		}

		// Token: 0x17000AC6 RID: 2758
		// (get) Token: 0x060044A0 RID: 17568 RVA: 0x002406CC File Offset: 0x0023EACC
		public List<Pawn> AllPawnsSpawned
		{
			get
			{
				return this.pawnsSpawned;
			}
		}

		// Token: 0x17000AC7 RID: 2759
		// (get) Token: 0x060044A1 RID: 17569 RVA: 0x002406E8 File Offset: 0x0023EAE8
		public IEnumerable<Pawn> FreeColonistsSpawned
		{
			get
			{
				return this.FreeHumanlikesSpawnedOfFaction(Faction.OfPlayer);
			}
		}

		// Token: 0x17000AC8 RID: 2760
		// (get) Token: 0x060044A2 RID: 17570 RVA: 0x00240708 File Offset: 0x0023EB08
		public List<Pawn> PrisonersOfColonySpawned
		{
			get
			{
				return this.prisonersOfColonySpawned;
			}
		}

		// Token: 0x17000AC9 RID: 2761
		// (get) Token: 0x060044A3 RID: 17571 RVA: 0x00240724 File Offset: 0x0023EB24
		public IEnumerable<Pawn> FreeColonistsAndPrisonersSpawned
		{
			get
			{
				return this.FreeColonistsSpawned.Concat(this.PrisonersOfColonySpawned);
			}
		}

		// Token: 0x17000ACA RID: 2762
		// (get) Token: 0x060044A4 RID: 17572 RVA: 0x0024074C File Offset: 0x0023EB4C
		public int AllPawnsSpawnedCount
		{
			get
			{
				return this.pawnsSpawned.Count;
			}
		}

		// Token: 0x17000ACB RID: 2763
		// (get) Token: 0x060044A5 RID: 17573 RVA: 0x0024076C File Offset: 0x0023EB6C
		public int FreeColonistsSpawnedCount
		{
			get
			{
				return this.FreeColonistsSpawned.Count<Pawn>();
			}
		}

		// Token: 0x17000ACC RID: 2764
		// (get) Token: 0x060044A6 RID: 17574 RVA: 0x0024078C File Offset: 0x0023EB8C
		public int PrisonersOfColonySpawnedCount
		{
			get
			{
				return this.PrisonersOfColonySpawned.Count;
			}
		}

		// Token: 0x17000ACD RID: 2765
		// (get) Token: 0x060044A7 RID: 17575 RVA: 0x002407AC File Offset: 0x0023EBAC
		public int FreeColonistsAndPrisonersSpawnedCount
		{
			get
			{
				return this.FreeColonistsAndPrisonersSpawned.Count<Pawn>();
			}
		}

		// Token: 0x17000ACE RID: 2766
		// (get) Token: 0x060044A8 RID: 17576 RVA: 0x002407CC File Offset: 0x0023EBCC
		public int ColonistsSpawnedCount
		{
			get
			{
				int num = 0;
				List<Pawn> list = this.SpawnedPawnsInFaction(Faction.OfPlayer);
				for (int i = 0; i < list.Count; i++)
				{
					if (list[i].IsColonist)
					{
						num++;
					}
				}
				return num;
			}
		}

		// Token: 0x17000ACF RID: 2767
		// (get) Token: 0x060044A9 RID: 17577 RVA: 0x00240824 File Offset: 0x0023EC24
		public int FreeColonistsSpawnedOrInPlayerEjectablePodsCount
		{
			get
			{
				int num = 0;
				for (int i = 0; i < this.pawnsSpawned.Count; i++)
				{
					if (this.pawnsSpawned[i].IsFreeColonist)
					{
						num++;
					}
				}
				List<Thing> list = this.map.listerThings.ThingsInGroup(ThingRequestGroup.ThingHolder);
				for (int j = 0; j < list.Count; j++)
				{
					Building_CryptosleepCasket building_CryptosleepCasket = list[j] as Building_CryptosleepCasket;
					if ((building_CryptosleepCasket != null && building_CryptosleepCasket.def.building.isPlayerEjectable) || list[j] is IActiveDropPod || list[j].TryGetComp<CompTransporter>() != null)
					{
						IThingHolder holder = list[j].TryGetComp<CompTransporter>() ?? ((IThingHolder)list[j]);
						this.tmpThings.Clear();
						ThingOwnerUtility.GetAllThingsRecursively(holder, this.tmpThings, true, null);
						for (int k = 0; k < this.tmpThings.Count; k++)
						{
							Pawn pawn = this.tmpThings[k] as Pawn;
							if (pawn != null && !pawn.Dead && pawn.IsFreeColonist)
							{
								num++;
							}
						}
					}
				}
				this.tmpThings.Clear();
				return num;
			}
		}

		// Token: 0x17000AD0 RID: 2768
		// (get) Token: 0x060044AA RID: 17578 RVA: 0x00240994 File Offset: 0x0023ED94
		public bool AnyColonistSpawned
		{
			get
			{
				List<Pawn> list = this.SpawnedPawnsInFaction(Faction.OfPlayer);
				for (int i = 0; i < list.Count; i++)
				{
					if (list[i].IsColonist)
					{
						return true;
					}
				}
				return false;
			}
		}

		// Token: 0x17000AD1 RID: 2769
		// (get) Token: 0x060044AB RID: 17579 RVA: 0x002409E8 File Offset: 0x0023EDE8
		public bool AnyFreeColonistSpawned
		{
			get
			{
				List<Pawn> list = this.SpawnedPawnsInFaction(Faction.OfPlayer);
				for (int i = 0; i < list.Count; i++)
				{
					if (list[i].IsFreeColonist)
					{
						return true;
					}
				}
				return false;
			}
		}

		// Token: 0x060044AC RID: 17580 RVA: 0x00240A3C File Offset: 0x0023EE3C
		private void EnsureFactionsListsInit()
		{
			List<Faction> allFactionsListForReading = Find.FactionManager.AllFactionsListForReading;
			for (int i = 0; i < allFactionsListForReading.Count; i++)
			{
				if (!this.pawnsInFactionSpawned.ContainsKey(allFactionsListForReading[i]))
				{
					this.pawnsInFactionSpawned.Add(allFactionsListForReading[i], new List<Pawn>());
				}
			}
		}

		// Token: 0x060044AD RID: 17581 RVA: 0x00240A9C File Offset: 0x0023EE9C
		public IEnumerable<Pawn> PawnsInFaction(Faction faction)
		{
			IEnumerable<Pawn> result;
			if (faction == null)
			{
				Log.Error("Called PawnsInFaction with null faction.", false);
				result = new List<Pawn>();
			}
			else
			{
				result = from x in this.AllPawns
				where x.Faction == faction
				select x;
			}
			return result;
		}

		// Token: 0x060044AE RID: 17582 RVA: 0x00240AF8 File Offset: 0x0023EEF8
		public List<Pawn> SpawnedPawnsInFaction(Faction faction)
		{
			this.EnsureFactionsListsInit();
			List<Pawn> result;
			if (faction == null)
			{
				Log.Error("Called SpawnedPawnsInFaction with null faction.", false);
				result = new List<Pawn>();
			}
			else
			{
				result = this.pawnsInFactionSpawned[faction];
			}
			return result;
		}

		// Token: 0x060044AF RID: 17583 RVA: 0x00240B3C File Offset: 0x0023EF3C
		public IEnumerable<Pawn> FreeHumanlikesOfFaction(Faction faction)
		{
			return from p in this.PawnsInFaction(faction)
			where p.HostFaction == null && p.RaceProps.Humanlike
			select p;
		}

		// Token: 0x060044B0 RID: 17584 RVA: 0x00240B7C File Offset: 0x0023EF7C
		public IEnumerable<Pawn> FreeHumanlikesSpawnedOfFaction(Faction faction)
		{
			return from p in this.SpawnedPawnsInFaction(faction)
			where p.HostFaction == null && p.RaceProps.Humanlike
			select p;
		}

		// Token: 0x060044B1 RID: 17585 RVA: 0x00240BBC File Offset: 0x0023EFBC
		public void RegisterPawn(Pawn p)
		{
			if (p.Dead)
			{
				Log.Warning(string.Concat(new object[]
				{
					"Tried to register dead pawn ",
					p,
					" in ",
					base.GetType(),
					"."
				}), false);
			}
			else if (!p.Spawned)
			{
				Log.Warning(string.Concat(new object[]
				{
					"Tried to register despawned pawn ",
					p,
					" in ",
					base.GetType(),
					"."
				}), false);
			}
			else if (p.Map != this.map)
			{
				Log.Warning("Tried to register pawn " + p + " but his Map is not this one.", false);
			}
			else if (p.mindState.Active)
			{
				this.EnsureFactionsListsInit();
				if (!this.pawnsSpawned.Contains(p))
				{
					this.pawnsSpawned.Add(p);
				}
				if (p.Faction != null)
				{
					if (!this.pawnsInFactionSpawned[p.Faction].Contains(p))
					{
						this.pawnsInFactionSpawned[p.Faction].Add(p);
						if (p.Faction == Faction.OfPlayer)
						{
							this.pawnsInFactionSpawned[Faction.OfPlayer].InsertionSort(delegate(Pawn a, Pawn b)
							{
								int num = (a.playerSettings == null) ? 0 : a.playerSettings.joinTick;
								int value = (b.playerSettings == null) ? 0 : b.playerSettings.joinTick;
								return num.CompareTo(value);
							});
						}
					}
				}
				if (p.IsPrisonerOfColony)
				{
					if (!this.prisonersOfColonySpawned.Contains(p))
					{
						this.prisonersOfColonySpawned.Add(p);
					}
				}
				this.DoListChangedNotifications();
			}
		}

		// Token: 0x060044B2 RID: 17586 RVA: 0x00240D74 File Offset: 0x0023F174
		public void DeRegisterPawn(Pawn p)
		{
			this.EnsureFactionsListsInit();
			this.pawnsSpawned.Remove(p);
			List<Faction> allFactionsListForReading = Find.FactionManager.AllFactionsListForReading;
			for (int i = 0; i < allFactionsListForReading.Count; i++)
			{
				Faction key = allFactionsListForReading[i];
				this.pawnsInFactionSpawned[key].Remove(p);
			}
			this.prisonersOfColonySpawned.Remove(p);
			this.DoListChangedNotifications();
		}

		// Token: 0x060044B3 RID: 17587 RVA: 0x00240DE7 File Offset: 0x0023F1E7
		public void UpdateRegistryForPawn(Pawn p)
		{
			this.DeRegisterPawn(p);
			if (p.Spawned && p.Map == this.map)
			{
				this.RegisterPawn(p);
			}
			this.DoListChangedNotifications();
		}

		// Token: 0x060044B4 RID: 17588 RVA: 0x00240E1A File Offset: 0x0023F21A
		private void DoListChangedNotifications()
		{
			MainTabWindowUtility.NotifyAllPawnTables_PawnsChanged();
			if (Find.ColonistBar != null)
			{
				Find.ColonistBar.MarkColonistsDirty();
			}
		}

		// Token: 0x060044B5 RID: 17589 RVA: 0x00240E38 File Offset: 0x0023F238
		public void LogListedPawns()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("MapPawns:");
			stringBuilder.AppendLine("pawnsSpawned");
			foreach (Pawn pawn in this.pawnsSpawned)
			{
				stringBuilder.AppendLine("    " + pawn.ToString());
			}
			stringBuilder.AppendLine("AllPawnsUnspawned");
			foreach (Pawn pawn2 in this.AllPawnsUnspawned)
			{
				stringBuilder.AppendLine("    " + pawn2.ToString());
			}
			foreach (KeyValuePair<Faction, List<Pawn>> keyValuePair in this.pawnsInFactionSpawned)
			{
				stringBuilder.AppendLine("pawnsInFactionSpawned[" + keyValuePair.Key.ToString() + "]");
				foreach (Pawn pawn3 in keyValuePair.Value)
				{
					stringBuilder.AppendLine("    " + pawn3.ToString());
				}
			}
			stringBuilder.AppendLine("prisonersOfColonySpawned");
			foreach (Pawn pawn4 in this.prisonersOfColonySpawned)
			{
				stringBuilder.AppendLine("    " + pawn4.ToString());
			}
			Log.Message(stringBuilder.ToString(), false);
		}

		// Token: 0x04002EAC RID: 11948
		private Map map;

		// Token: 0x04002EAD RID: 11949
		private List<Pawn> pawnsSpawned = new List<Pawn>();

		// Token: 0x04002EAE RID: 11950
		private Dictionary<Faction, List<Pawn>> pawnsInFactionSpawned = new Dictionary<Faction, List<Pawn>>();

		// Token: 0x04002EAF RID: 11951
		private List<Pawn> prisonersOfColonySpawned = new List<Pawn>();

		// Token: 0x04002EB0 RID: 11952
		private List<Thing> tmpThings = new List<Thing>();

		// Token: 0x04002EB1 RID: 11953
		private List<Pawn> tmpUnspawnedPawns = new List<Pawn>();
	}
}
