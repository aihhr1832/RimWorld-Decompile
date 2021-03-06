﻿using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Verse;

namespace RimWorld
{
	public class Pawn_NeedsTracker : IExposable
	{
		private Pawn pawn;

		private List<Need> needs = new List<Need>();

		public Need_Mood mood;

		public Need_Food food;

		public Need_Rest rest;

		public Need_Joy joy;

		public Need_Beauty beauty;

		public Need_RoomSize roomsize;

		public Need_Outdoors outdoors;

		public Need_Comfort comfort;

		[CompilerGenerated]
		private static Predicate<Need> <>f__am$cache0;

		public Pawn_NeedsTracker()
		{
		}

		public Pawn_NeedsTracker(Pawn newPawn)
		{
			this.pawn = newPawn;
			this.AddOrRemoveNeedsAsAppropriate();
		}

		public List<Need> AllNeeds
		{
			get
			{
				return this.needs;
			}
		}

		public void ExposeData()
		{
			Scribe_Collections.Look<Need>(ref this.needs, "needs", LookMode.Deep, new object[]
			{
				this.pawn
			});
			if (Scribe.mode == LoadSaveMode.LoadingVars || Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				if (this.needs.RemoveAll((Need x) => x == null) != 0)
				{
					Log.Error("Pawn " + this.pawn.Label + " had some null needs after loading.", false);
				}
				this.BindDirectNeedFields();
			}
		}

		private void BindDirectNeedFields()
		{
			this.mood = this.TryGetNeed<Need_Mood>();
			this.food = this.TryGetNeed<Need_Food>();
			this.rest = this.TryGetNeed<Need_Rest>();
			this.joy = this.TryGetNeed<Need_Joy>();
			this.beauty = this.TryGetNeed<Need_Beauty>();
			this.comfort = this.TryGetNeed<Need_Comfort>();
			this.roomsize = this.TryGetNeed<Need_RoomSize>();
			this.outdoors = this.TryGetNeed<Need_Outdoors>();
		}

		public void NeedsTrackerTick()
		{
			if (this.pawn.IsHashIntervalTick(150))
			{
				for (int i = 0; i < this.needs.Count; i++)
				{
					this.needs[i].NeedInterval();
				}
			}
		}

		public T TryGetNeed<T>() where T : Need
		{
			for (int i = 0; i < this.needs.Count; i++)
			{
				if (this.needs[i].GetType() == typeof(T))
				{
					return (T)((object)this.needs[i]);
				}
			}
			return (T)((object)null);
		}

		public Need TryGetNeed(NeedDef def)
		{
			for (int i = 0; i < this.needs.Count; i++)
			{
				if (this.needs[i].def == def)
				{
					return this.needs[i];
				}
			}
			return null;
		}

		public void SetInitialLevels()
		{
			for (int i = 0; i < this.needs.Count; i++)
			{
				this.needs[i].SetInitialLevel();
			}
		}

		public void AddOrRemoveNeedsAsAppropriate()
		{
			List<NeedDef> allDefsListForReading = DefDatabase<NeedDef>.AllDefsListForReading;
			for (int i = 0; i < allDefsListForReading.Count; i++)
			{
				NeedDef needDef = allDefsListForReading[i];
				if (this.ShouldHaveNeed(needDef))
				{
					if (this.TryGetNeed(needDef) == null)
					{
						this.AddNeed(needDef);
					}
				}
				else if (this.TryGetNeed(needDef) != null)
				{
					this.RemoveNeed(needDef);
				}
			}
		}

		private bool ShouldHaveNeed(NeedDef nd)
		{
			bool result;
			if (this.pawn.RaceProps.intelligence < nd.minIntelligence)
			{
				result = false;
			}
			else
			{
				if (nd.colonistsOnly)
				{
					if (this.pawn.Faction == null || !this.pawn.Faction.IsPlayer)
					{
						return false;
					}
				}
				if (nd.colonistAndPrisonersOnly)
				{
					if ((this.pawn.Faction == null || !this.pawn.Faction.IsPlayer) && (this.pawn.HostFaction == null || this.pawn.HostFaction != Faction.OfPlayer))
					{
						return false;
					}
				}
				if (nd.onlyIfCausedByHediff)
				{
					if (!this.pawn.health.hediffSet.hediffs.Any((Hediff x) => x.def.causesNeed == nd))
					{
						return false;
					}
				}
				if (nd.neverOnPrisoner && this.pawn.IsPrisoner)
				{
					result = false;
				}
				else if (nd == NeedDefOf.Food)
				{
					result = this.pawn.RaceProps.EatsFood;
				}
				else
				{
					result = (nd != NeedDefOf.Rest || this.pawn.RaceProps.needsRest);
				}
			}
			return result;
		}

		private void AddNeed(NeedDef nd)
		{
			Need need = (Need)Activator.CreateInstance(nd.needClass, new object[]
			{
				this.pawn
			});
			need.def = nd;
			this.needs.Add(need);
			need.SetInitialLevel();
			this.BindDirectNeedFields();
		}

		private void RemoveNeed(NeedDef nd)
		{
			Need item = this.TryGetNeed(nd);
			this.needs.Remove(item);
			this.BindDirectNeedFields();
		}

		[CompilerGenerated]
		private static bool <ExposeData>m__0(Need x)
		{
			return x == null;
		}

		[CompilerGenerated]
		private sealed class <ShouldHaveNeed>c__AnonStorey0
		{
			internal NeedDef nd;

			public <ShouldHaveNeed>c__AnonStorey0()
			{
			}

			internal bool <>m__0(Hediff x)
			{
				return x.def.causesNeed == this.nd;
			}
		}
	}
}
