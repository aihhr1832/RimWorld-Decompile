﻿using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using RimWorld;
using UnityEngine;
using Verse.AI;

namespace Verse
{
	[StaticConstructorOnStartup]
	public sealed class PawnDestinationReservationManager : IExposable
	{
		private Dictionary<Faction, PawnDestinationReservationManager.PawnDestinationSet> reservedDestinations = new Dictionary<Faction, PawnDestinationReservationManager.PawnDestinationSet>();

		private static readonly Material DestinationMat = MaterialPool.MatFrom("UI/Overlays/ReservedDestination");

		private static readonly Material DestinationSelectionMat = MaterialPool.MatFrom("UI/Overlays/ReservedDestinationSelection");

		private List<Faction> reservedDestinationsKeysWorkingList;

		private List<PawnDestinationReservationManager.PawnDestinationSet> reservedDestinationsValuesWorkingList;

		public PawnDestinationReservationManager()
		{
			foreach (Faction faction in Find.FactionManager.AllFactions)
			{
				this.RegisterFaction(faction);
			}
		}

		public void RegisterFaction(Faction faction)
		{
			this.reservedDestinations.Add(faction, new PawnDestinationReservationManager.PawnDestinationSet());
		}

		public void Reserve(Pawn p, Job job, IntVec3 loc)
		{
			if (p.Faction != null)
			{
				this.ObsoleteAllClaimedBy(p);
				this.reservedDestinations[p.Faction].list.Add(new PawnDestinationReservationManager.PawnDestinationReservation
				{
					target = loc,
					claimant = p,
					job = job
				});
			}
		}

		public IntVec3 MostRecentReservationFor(Pawn p)
		{
			IntVec3 invalid;
			if (p.Faction == null)
			{
				invalid = IntVec3.Invalid;
			}
			else
			{
				List<PawnDestinationReservationManager.PawnDestinationReservation> list = this.reservedDestinations[p.Faction].list;
				for (int i = 0; i < list.Count; i++)
				{
					if (list[i].claimant == p && !list[i].obsolete)
					{
						return list[i].target;
					}
				}
				invalid = IntVec3.Invalid;
			}
			return invalid;
		}

		public IntVec3 FirstObsoleteReservationFor(Pawn p)
		{
			IntVec3 invalid;
			if (p.Faction == null)
			{
				invalid = IntVec3.Invalid;
			}
			else
			{
				List<PawnDestinationReservationManager.PawnDestinationReservation> list = this.reservedDestinations[p.Faction].list;
				for (int i = 0; i < list.Count; i++)
				{
					if (list[i].claimant == p && list[i].obsolete)
					{
						return list[i].target;
					}
				}
				invalid = IntVec3.Invalid;
			}
			return invalid;
		}

		public Job FirstObsoleteReservationJobFor(Pawn p)
		{
			Job result;
			if (p.Faction == null)
			{
				result = null;
			}
			else
			{
				List<PawnDestinationReservationManager.PawnDestinationReservation> list = this.reservedDestinations[p.Faction].list;
				for (int i = 0; i < list.Count; i++)
				{
					if (list[i].claimant == p && list[i].obsolete)
					{
						return list[i].job;
					}
				}
				result = null;
			}
			return result;
		}

		public bool IsReserved(IntVec3 loc)
		{
			foreach (KeyValuePair<Faction, PawnDestinationReservationManager.PawnDestinationSet> keyValuePair in this.reservedDestinations)
			{
				List<PawnDestinationReservationManager.PawnDestinationReservation> list = keyValuePair.Value.list;
				for (int i = 0; i < list.Count; i++)
				{
					if (list[i].target == loc)
					{
						return true;
					}
				}
			}
			return false;
		}

		public bool CanReserve(IntVec3 c, Pawn searcher, bool draftedOnly = false)
		{
			bool result;
			if (searcher.Faction == null)
			{
				result = true;
			}
			else
			{
				List<PawnDestinationReservationManager.PawnDestinationReservation> list = this.reservedDestinations[searcher.Faction].list;
				for (int i = 0; i < list.Count; i++)
				{
					if (list[i].target == c && list[i].claimant != searcher && (!draftedOnly || list[i].claimant.Drafted))
					{
						return false;
					}
				}
				result = true;
			}
			return result;
		}

		public Pawn FirstReserverOf(IntVec3 c, Faction faction)
		{
			Pawn result;
			if (faction == null)
			{
				result = null;
			}
			else
			{
				List<PawnDestinationReservationManager.PawnDestinationReservation> list = this.reservedDestinations[faction].list;
				for (int i = 0; i < list.Count; i++)
				{
					if (list[i].target == c)
					{
						return list[i].claimant;
					}
				}
				result = null;
			}
			return result;
		}

		public void ReleaseAllObsoleteClaimedBy(Pawn p)
		{
			if (p.Faction != null)
			{
				List<PawnDestinationReservationManager.PawnDestinationReservation> list = this.reservedDestinations[p.Faction].list;
				int i = 0;
				while (i < list.Count)
				{
					if (list[i].claimant == p && list[i].obsolete)
					{
						list[i] = list[list.Count - 1];
						list.RemoveLast<PawnDestinationReservationManager.PawnDestinationReservation>();
					}
					else
					{
						i++;
					}
				}
			}
		}

		public void ReleaseAllClaimedBy(Pawn p)
		{
			if (p.Faction != null)
			{
				List<PawnDestinationReservationManager.PawnDestinationReservation> list = this.reservedDestinations[p.Faction].list;
				int i = 0;
				while (i < list.Count)
				{
					if (list[i].claimant == p)
					{
						list[i] = list[list.Count - 1];
						list.RemoveLast<PawnDestinationReservationManager.PawnDestinationReservation>();
					}
					else
					{
						i++;
					}
				}
			}
		}

		public void ReleaseClaimedBy(Pawn p, Job job)
		{
			if (p.Faction != null)
			{
				List<PawnDestinationReservationManager.PawnDestinationReservation> list = this.reservedDestinations[p.Faction].list;
				for (int i = 0; i < list.Count; i++)
				{
					if (list[i].claimant == p && list[i].job == job)
					{
						list[i].job = null;
						if (list[i].obsolete)
						{
							list[i] = list[list.Count - 1];
							list.RemoveLast<PawnDestinationReservationManager.PawnDestinationReservation>();
							i--;
						}
					}
				}
			}
		}

		public void ObsoleteAllClaimedBy(Pawn p)
		{
			if (p.Faction != null)
			{
				List<PawnDestinationReservationManager.PawnDestinationReservation> list = this.reservedDestinations[p.Faction].list;
				for (int i = 0; i < list.Count; i++)
				{
					if (list[i].claimant == p)
					{
						list[i].obsolete = true;
						if (list[i].job == null)
						{
							list[i] = list[list.Count - 1];
							list.RemoveLast<PawnDestinationReservationManager.PawnDestinationReservation>();
							i--;
						}
					}
				}
			}
		}

		public void DebugDrawDestinations()
		{
			foreach (PawnDestinationReservationManager.PawnDestinationReservation pawnDestinationReservation in this.reservedDestinations[Faction.OfPlayer].list)
			{
				if (!(pawnDestinationReservation.claimant.Position == pawnDestinationReservation.target))
				{
					IntVec3 target = pawnDestinationReservation.target;
					Vector3 s = new Vector3(1f, 1f, 1f);
					Matrix4x4 matrix = default(Matrix4x4);
					matrix.SetTRS(target.ToVector3ShiftedWithAltitude(AltitudeLayer.MetaOverlays), Quaternion.identity, s);
					Graphics.DrawMesh(MeshPool.plane10, matrix, PawnDestinationReservationManager.DestinationMat, 0);
					if (Find.Selector.IsSelected(pawnDestinationReservation.claimant))
					{
						Graphics.DrawMesh(MeshPool.plane10, matrix, PawnDestinationReservationManager.DestinationSelectionMat, 0);
					}
				}
			}
		}

		public void ExposeData()
		{
			Scribe_Collections.Look<Faction, PawnDestinationReservationManager.PawnDestinationSet>(ref this.reservedDestinations, "reservedDestinations", LookMode.Reference, LookMode.Deep, ref this.reservedDestinationsKeysWorkingList, ref this.reservedDestinationsValuesWorkingList);
		}

		// Note: this type is marked as 'beforefieldinit'.
		static PawnDestinationReservationManager()
		{
		}

		public class PawnDestinationReservation : IExposable
		{
			public IntVec3 target;

			public Pawn claimant;

			public Job job;

			public bool obsolete;

			public PawnDestinationReservation()
			{
			}

			public void ExposeData()
			{
				Scribe_Values.Look<IntVec3>(ref this.target, "target", default(IntVec3), false);
				Scribe_References.Look<Pawn>(ref this.claimant, "claimant", false);
				Scribe_References.Look<Job>(ref this.job, "job", false);
				Scribe_Values.Look<bool>(ref this.obsolete, "obsolete", false, false);
			}
		}

		public class PawnDestinationSet : IExposable
		{
			public List<PawnDestinationReservationManager.PawnDestinationReservation> list = new List<PawnDestinationReservationManager.PawnDestinationReservation>();

			[CompilerGenerated]
			private static Predicate<PawnDestinationReservationManager.PawnDestinationReservation> <>f__am$cache0;

			public PawnDestinationSet()
			{
			}

			public void ExposeData()
			{
				Scribe_Collections.Look<PawnDestinationReservationManager.PawnDestinationReservation>(ref this.list, "list", LookMode.Deep, new object[0]);
				if (Scribe.mode == LoadSaveMode.PostLoadInit)
				{
					if (this.list.RemoveAll((PawnDestinationReservationManager.PawnDestinationReservation x) => x.claimant.DestroyedOrNull()) != 0)
					{
						Log.Warning("Some destination reservations had null or destroyed claimant.", false);
					}
				}
			}

			[CompilerGenerated]
			private static bool <ExposeData>m__0(PawnDestinationReservationManager.PawnDestinationReservation x)
			{
				return x.claimant.DestroyedOrNull();
			}
		}
	}
}
