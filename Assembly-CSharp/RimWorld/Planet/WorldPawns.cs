﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine.Profiling;
using Verse;

namespace RimWorld.Planet
{
	public class WorldPawns : IExposable
	{
		private HashSet<Pawn> pawnsAlive = new HashSet<Pawn>();

		private HashSet<Pawn> pawnsMothballed = new HashSet<Pawn>();

		private HashSet<Pawn> pawnsDead = new HashSet<Pawn>();

		private HashSet<Pawn> pawnsForcefullyKeptAsWorldPawns = new HashSet<Pawn>();

		public WorldPawnGC gc = new WorldPawnGC();

		private Stack<Pawn> pawnsBeingDiscarded = new Stack<Pawn>();

		private const float PctOfHumanlikesAlwaysKept = 0.1f;

		private const float PctOfUnnamedColonyAnimalsAlwaysKept = 0.05f;

		private const int TendIntervalTicks = 7500;

		private const int MothballUpdateInterval = 15000;

		private static List<Pawn> tmpPawnsToTick = new List<Pawn>();

		private static List<Pawn> tmpPawnsToRemove = new List<Pawn>();

		[CompilerGenerated]
		private static Predicate<Pawn> <>f__am$cache0;

		[CompilerGenerated]
		private static Predicate<Pawn> <>f__am$cache1;

		[CompilerGenerated]
		private static Predicate<Pawn> <>f__am$cache2;

		[CompilerGenerated]
		private static Predicate<Pawn> <>f__am$cache3;

		[CompilerGenerated]
		private static Predicate<Pawn> <>f__am$cache4;

		[CompilerGenerated]
		private static Predicate<Pawn> <>f__am$cache5;

		[CompilerGenerated]
		private static Predicate<Pawn> <>f__am$cache6;

		[CompilerGenerated]
		private static Func<Pawn, int> <>f__am$cache7;

		[CompilerGenerated]
		private static Func<KeyValuePair<HediffDef, int>, int> <>f__am$cache8;

		public WorldPawns()
		{
		}

		public IEnumerable<Pawn> AllPawnsAliveOrDead
		{
			get
			{
				return this.AllPawnsAlive.Concat(this.AllPawnsDead);
			}
		}

		public IEnumerable<Pawn> AllPawnsAlive
		{
			get
			{
				return this.pawnsAlive.Concat(this.pawnsMothballed);
			}
		}

		public IEnumerable<Pawn> AllPawnsDead
		{
			get
			{
				return this.pawnsDead;
			}
		}

		public HashSet<Pawn> ForcefullyKeptPawns
		{
			get
			{
				return this.pawnsForcefullyKeptAsWorldPawns;
			}
		}

		public void WorldPawnsTick()
		{
			WorldPawns.tmpPawnsToTick.Clear();
			WorldPawns.tmpPawnsToTick.AddRange(this.pawnsAlive);
			for (int i = 0; i < WorldPawns.tmpPawnsToTick.Count; i++)
			{
				try
				{
					WorldPawns.tmpPawnsToTick[i].Tick();
				}
				catch (Exception arg)
				{
					Log.ErrorOnce("Exception ticking world pawn: " + arg, WorldPawns.tmpPawnsToTick[i].thingIDNumber ^ 1148571423, false);
				}
				if (this.ShouldAutoTendTo(WorldPawns.tmpPawnsToTick[i]))
				{
					TendUtility.DoTend(null, WorldPawns.tmpPawnsToTick[i], null);
				}
			}
			WorldPawns.tmpPawnsToTick.Clear();
			if (Find.TickManager.TicksGame % 15000 == 0)
			{
				this.DoMothballProcessing();
			}
			WorldPawns.tmpPawnsToRemove.Clear();
			foreach (Pawn pawn in this.pawnsDead)
			{
				if (pawn == null)
				{
					Log.ErrorOnce("Dead null world pawn detected, discarding.", 94424128, false);
					WorldPawns.tmpPawnsToRemove.Add(pawn);
				}
				else if (pawn.Discarded)
				{
					Log.Error("World pawn " + pawn + " has been discarded while still being a world pawn. This should never happen, because discard destroy mode means that the pawn is no longer managed by anything. Pawn should have been removed from the world first.", false);
					WorldPawns.tmpPawnsToRemove.Add(pawn);
				}
			}
			for (int j = 0; j < WorldPawns.tmpPawnsToRemove.Count; j++)
			{
				this.pawnsDead.Remove(WorldPawns.tmpPawnsToRemove[j]);
			}
			WorldPawns.tmpPawnsToRemove.Clear();
			Profiler.BeginSample("WorldPawnGCTick");
			try
			{
				this.gc.WorldPawnGCTick();
			}
			catch (Exception arg2)
			{
				Log.Error("Error in WorldPawnGCTick(): " + arg2, false);
			}
			Profiler.EndSample();
		}

		public void ExposeData()
		{
			Scribe_Collections.Look<Pawn>(ref this.pawnsForcefullyKeptAsWorldPawns, true, "pawnsForcefullyKeptAsWorldPawns", LookMode.Reference);
			Scribe_Collections.Look<Pawn>(ref this.pawnsAlive, "pawnsAlive", LookMode.Deep);
			Scribe_Collections.Look<Pawn>(ref this.pawnsMothballed, "pawnsMothballed", LookMode.Deep);
			Scribe_Collections.Look<Pawn>(ref this.pawnsDead, true, "pawnsDead", LookMode.Deep);
			Scribe_Deep.Look<WorldPawnGC>(ref this.gc, "gc", new object[0]);
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				BackCompatibility.WorldPawnPostLoadInit(this, ref this.pawnsMothballed);
				if (this.pawnsForcefullyKeptAsWorldPawns.RemoveWhere((Pawn x) => x == null) != 0)
				{
					Log.Error("Some pawnsForcefullyKeptAsWorldPawns were null after loading.", false);
				}
				if (this.pawnsAlive.RemoveWhere((Pawn x) => x == null) != 0)
				{
					Log.Error("Some pawnsAlive were null after loading.", false);
				}
				if (this.pawnsMothballed.RemoveWhere((Pawn x) => x == null) != 0)
				{
					Log.Error("Some pawnsMothballed were null after loading.", false);
				}
				if (this.pawnsDead.RemoveWhere((Pawn x) => x == null) != 0)
				{
					Log.Error("Some pawnsDead were null after loading.", false);
				}
				if (this.pawnsAlive.RemoveWhere((Pawn x) => x.def == null || x.kindDef == null) != 0)
				{
					Log.Error("Some pawnsAlive had null def after loading.", false);
				}
				if (this.pawnsMothballed.RemoveWhere((Pawn x) => x.def == null || x.kindDef == null) != 0)
				{
					Log.Error("Some pawnsMothballed had null def after loading.", false);
				}
				if (this.pawnsDead.RemoveWhere((Pawn x) => x.def == null || x.kindDef == null) != 0)
				{
					Log.Error("Some pawnsDead had null def after loading.", false);
				}
			}
		}

		public bool Contains(Pawn p)
		{
			return this.pawnsAlive.Contains(p) || this.pawnsMothballed.Contains(p) || this.pawnsDead.Contains(p);
		}

		public void PassToWorld(Pawn pawn, PawnDiscardDecideMode discardMode = PawnDiscardDecideMode.Decide)
		{
			if (pawn.Spawned)
			{
				Log.Error("Tried to call PassToWorld with spawned pawn: " + pawn + ". Despawn him first.", false);
			}
			else if (this.Contains(pawn))
			{
				Log.Error("Tried to pass pawn " + pawn + " to world, but it's already here.", false);
			}
			else
			{
				if (discardMode == PawnDiscardDecideMode.KeepForever && pawn.Discarded)
				{
					Log.Error("Tried to pass a discarded pawn " + pawn + " to world with discardMode=Keep. Discarded pawns should never be stored in WorldPawns.", false);
					discardMode = PawnDiscardDecideMode.Decide;
				}
				if (PawnComponentsUtility.HasSpawnedComponents(pawn))
				{
					PawnComponentsUtility.RemoveComponentsOnDespawned(pawn);
				}
				if (discardMode != PawnDiscardDecideMode.Decide)
				{
					if (discardMode != PawnDiscardDecideMode.KeepForever)
					{
						if (discardMode == PawnDiscardDecideMode.Discard)
						{
							this.DiscardPawn(pawn, false);
						}
					}
					else
					{
						this.pawnsForcefullyKeptAsWorldPawns.Add(pawn);
						this.AddPawn(pawn);
					}
				}
				else
				{
					this.AddPawn(pawn);
				}
			}
		}

		public void RemovePawn(Pawn p)
		{
			if (!this.Contains(p))
			{
				Log.Error(string.Concat(new object[]
				{
					"Tried to remove pawn ",
					p,
					" from ",
					base.GetType(),
					", but it's not here."
				}), false);
			}
			this.gc.CancelGCPass();
			if (this.pawnsMothballed.Contains(p))
			{
				int num = Find.TickManager.TicksGame % 15000;
				if (num != 0)
				{
					try
					{
						p.TickMothballed(Find.TickManager.TicksGame % 15000);
					}
					catch (Exception arg)
					{
						Log.Error("Exception ticking mothballed world pawn (just before removing): " + arg, false);
					}
				}
			}
			this.pawnsAlive.Remove(p);
			this.pawnsMothballed.Remove(p);
			this.pawnsDead.Remove(p);
			this.pawnsForcefullyKeptAsWorldPawns.Remove(p);
		}

		public void RemoveAndDiscardPawnViaGC(Pawn p)
		{
			this.RemovePawn(p);
			this.DiscardPawn(p, true);
		}

		public WorldPawnSituation GetSituation(Pawn p)
		{
			WorldPawnSituation result;
			if (!this.Contains(p))
			{
				result = WorldPawnSituation.None;
			}
			else if (p.Dead || p.Destroyed)
			{
				result = WorldPawnSituation.Dead;
			}
			else if (PawnUtility.IsFactionLeader(p))
			{
				result = WorldPawnSituation.FactionLeader;
			}
			else if (PawnUtility.IsKidnappedPawn(p))
			{
				result = WorldPawnSituation.Kidnapped;
			}
			else if (p.IsCaravanMember())
			{
				result = WorldPawnSituation.CaravanMember;
			}
			else if (PawnUtility.IsTravelingInTransportPodWorldObject(p))
			{
				result = WorldPawnSituation.InTravelingTransportPod;
			}
			else if (PawnUtility.ForSaleBySettlement(p))
			{
				result = WorldPawnSituation.ForSaleBySettlement;
			}
			else
			{
				result = WorldPawnSituation.Free;
			}
			return result;
		}

		public IEnumerable<Pawn> GetPawnsBySituation(WorldPawnSituation situation)
		{
			return from x in this.AllPawnsAliveOrDead
			where this.GetSituation(x) == situation
			select x;
		}

		public int GetPawnsBySituationCount(WorldPawnSituation situation)
		{
			int num = 0;
			foreach (Pawn p in this.pawnsAlive)
			{
				if (this.GetSituation(p) == situation)
				{
					num++;
				}
			}
			foreach (Pawn p2 in this.pawnsDead)
			{
				if (this.GetSituation(p2) == situation)
				{
					num++;
				}
			}
			return num;
		}

		private bool ShouldAutoTendTo(Pawn pawn)
		{
			return !pawn.Dead && !pawn.Destroyed && pawn.IsHashIntervalTick(7500) && !pawn.IsCaravanMember() && !PawnUtility.IsTravelingInTransportPodWorldObject(pawn);
		}

		public bool IsBeingDiscarded(Pawn p)
		{
			return this.pawnsBeingDiscarded.Contains(p);
		}

		public void Notify_PawnDestroyed(Pawn p)
		{
			if (this.pawnsAlive.Contains(p) || this.pawnsMothballed.Contains(p))
			{
				this.pawnsAlive.Remove(p);
				this.pawnsMothballed.Remove(p);
				this.pawnsDead.Add(p);
			}
		}

		private bool ShouldMothball(Pawn p)
		{
			return this.DefPreventingMothball(p) == null && !p.IsCaravanMember() && !PawnUtility.IsTravelingInTransportPodWorldObject(p);
		}

		private HediffDef DefPreventingMothball(Pawn p)
		{
			List<Hediff> hediffs = p.health.hediffSet.hediffs;
			for (int i = 0; i < hediffs.Count; i++)
			{
				if (!hediffs[i].def.AlwaysAllowMothball)
				{
					if (!hediffs[i].IsPermanent())
					{
						return hediffs[i].def;
					}
				}
			}
			return null;
		}

		private void AddPawn(Pawn p)
		{
			this.gc.CancelGCPass();
			if (p.Dead || p.Destroyed)
			{
				this.pawnsDead.Add(p);
			}
			else
			{
				this.pawnsAlive.Add(p);
			}
			p.Notify_PassedToWorld();
		}

		private void DiscardPawn(Pawn p, bool silentlyRemoveReferences = false)
		{
			this.pawnsBeingDiscarded.Push(p);
			try
			{
				if (!p.Destroyed)
				{
					p.Destroy(DestroyMode.Vanish);
				}
				if (!p.Discarded)
				{
					p.Discard(silentlyRemoveReferences);
				}
			}
			finally
			{
				this.pawnsBeingDiscarded.Pop();
			}
		}

		private void DoMothballProcessing()
		{
			WorldPawns.tmpPawnsToTick.AddRange(this.pawnsMothballed);
			for (int i = 0; i < WorldPawns.tmpPawnsToTick.Count; i++)
			{
				try
				{
					WorldPawns.tmpPawnsToTick[i].TickMothballed(15000);
				}
				catch (Exception arg)
				{
					Log.ErrorOnce("Exception ticking mothballed world pawn: " + arg, WorldPawns.tmpPawnsToTick[i].thingIDNumber ^ 1535437893, false);
				}
			}
			WorldPawns.tmpPawnsToTick.Clear();
			WorldPawns.tmpPawnsToTick.AddRange(this.pawnsAlive);
			for (int j = 0; j < WorldPawns.tmpPawnsToTick.Count; j++)
			{
				Pawn pawn = WorldPawns.tmpPawnsToTick[j];
				if (this.ShouldMothball(pawn))
				{
					this.pawnsAlive.Remove(pawn);
					this.pawnsMothballed.Add(pawn);
				}
			}
			WorldPawns.tmpPawnsToTick.Clear();
		}

		public void DebugRunMothballProcessing()
		{
			this.DoMothballProcessing();
			Log.Message(string.Format("World pawn mothball run complete", new object[0]), false);
		}

		public void UnpinAllForcefullyKeptPawns()
		{
			this.pawnsForcefullyKeptAsWorldPawns.Clear();
		}

		public void LogWorldPawns()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("======= World Pawns =======");
			stringBuilder.AppendLine("Count: " + this.AllPawnsAliveOrDead.Count<Pawn>());
			stringBuilder.AppendLine(string.Format("(Live: {0} - Mothballed: {1} - Dead: {2}; {3} forcefully kept)", new object[]
			{
				this.pawnsAlive.Count,
				this.pawnsMothballed.Count,
				this.pawnsDead.Count,
				this.pawnsForcefullyKeptAsWorldPawns.Count
			}));
			foreach (WorldPawnSituation worldPawnSituation in (WorldPawnSituation[])Enum.GetValues(typeof(WorldPawnSituation)))
			{
				if (worldPawnSituation != WorldPawnSituation.None)
				{
					stringBuilder.AppendLine();
					stringBuilder.AppendLine("== " + worldPawnSituation + " ==");
					foreach (Pawn pawn in from x in this.GetPawnsBySituation(worldPawnSituation)
					orderby (x.Faction != null) ? x.Faction.loadID : -1
					select x)
					{
						string text = (pawn.Name == null) ? pawn.LabelCap : pawn.Name.ToStringFull;
						stringBuilder.AppendLine(string.Concat(new object[]
						{
							text,
							", ",
							pawn.KindLabel,
							", ",
							pawn.Faction
						}));
					}
				}
			}
			stringBuilder.AppendLine("===========================");
			Log.Message(stringBuilder.ToString(), false);
		}

		public void LogWorldPawnMothballPrevention()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("======= World Pawns Mothball Prevention =======");
			stringBuilder.AppendLine(string.Format("Count: {0}", this.pawnsAlive.Count<Pawn>()));
			int num = 0;
			Dictionary<HediffDef, int> dictionary = new Dictionary<HediffDef, int>();
			foreach (Pawn p in this.pawnsAlive)
			{
				HediffDef hediffDef = this.DefPreventingMothball(p);
				if (hediffDef == null)
				{
					num++;
				}
				else
				{
					if (!dictionary.ContainsKey(hediffDef))
					{
						dictionary[hediffDef] = 0;
					}
					Dictionary<HediffDef, int> dictionary2;
					HediffDef key;
					(dictionary2 = dictionary)[key = hediffDef] = dictionary2[key] + 1;
				}
			}
			stringBuilder.AppendLine(string.Format("Will be mothballed: {0}", num));
			stringBuilder.AppendLine();
			stringBuilder.AppendLine("Reasons to avoid mothballing:");
			foreach (KeyValuePair<HediffDef, int> keyValuePair in from kvp in dictionary
			orderby kvp.Value descending
			select kvp)
			{
				stringBuilder.AppendLine(string.Format("{0}: {1}", keyValuePair.Value, keyValuePair.Key));
			}
			Log.Message(stringBuilder.ToString(), false);
		}

		// Note: this type is marked as 'beforefieldinit'.
		static WorldPawns()
		{
		}

		[CompilerGenerated]
		private static bool <ExposeData>m__0(Pawn x)
		{
			return x == null;
		}

		[CompilerGenerated]
		private static bool <ExposeData>m__1(Pawn x)
		{
			return x == null;
		}

		[CompilerGenerated]
		private static bool <ExposeData>m__2(Pawn x)
		{
			return x == null;
		}

		[CompilerGenerated]
		private static bool <ExposeData>m__3(Pawn x)
		{
			return x == null;
		}

		[CompilerGenerated]
		private static bool <ExposeData>m__4(Pawn x)
		{
			return x.def == null || x.kindDef == null;
		}

		[CompilerGenerated]
		private static bool <ExposeData>m__5(Pawn x)
		{
			return x.def == null || x.kindDef == null;
		}

		[CompilerGenerated]
		private static bool <ExposeData>m__6(Pawn x)
		{
			return x.def == null || x.kindDef == null;
		}

		[CompilerGenerated]
		private static int <LogWorldPawns>m__7(Pawn x)
		{
			return (x.Faction != null) ? x.Faction.loadID : -1;
		}

		[CompilerGenerated]
		private static int <LogWorldPawnMothballPrevention>m__8(KeyValuePair<HediffDef, int> kvp)
		{
			return kvp.Value;
		}

		[CompilerGenerated]
		private sealed class <GetPawnsBySituation>c__AnonStorey0
		{
			internal WorldPawnSituation situation;

			internal WorldPawns $this;

			public <GetPawnsBySituation>c__AnonStorey0()
			{
			}

			internal bool <>m__0(Pawn x)
			{
				return this.$this.GetSituation(x) == this.situation;
			}
		}
	}
}
