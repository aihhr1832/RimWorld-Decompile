﻿using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using RimWorld;

namespace Verse.AI
{
	public class AttackTargetReservationManager : IExposable
	{
		private Map map;

		private List<AttackTargetReservationManager.AttackTargetReservation> reservations = new List<AttackTargetReservationManager.AttackTargetReservation>();

		[CompilerGenerated]
		private static Predicate<AttackTargetReservationManager.AttackTargetReservation> <>f__am$cache0;

		[CompilerGenerated]
		private static Predicate<AttackTargetReservationManager.AttackTargetReservation> <>f__am$cache1;

		public AttackTargetReservationManager(Map map)
		{
			this.map = map;
		}

		public void Reserve(Pawn claimant, Job job, IAttackTarget target)
		{
			if (target == null)
			{
				Log.Warning(claimant + " tried to reserve null attack target.", false);
			}
			else if (!this.IsReservedBy(claimant, target))
			{
				AttackTargetReservationManager.AttackTargetReservation attackTargetReservation = new AttackTargetReservationManager.AttackTargetReservation();
				attackTargetReservation.target = target;
				attackTargetReservation.claimant = claimant;
				attackTargetReservation.job = job;
				this.reservations.Add(attackTargetReservation);
			}
		}

		public void Release(Pawn claimant, Job job, IAttackTarget target)
		{
			if (target == null)
			{
				Log.Warning(claimant + " tried to release reservation on null attack target.", false);
			}
			else
			{
				for (int i = 0; i < this.reservations.Count; i++)
				{
					AttackTargetReservationManager.AttackTargetReservation attackTargetReservation = this.reservations[i];
					if (attackTargetReservation.target == target && attackTargetReservation.claimant == claimant && attackTargetReservation.job == job)
					{
						this.reservations.RemoveAt(i);
						return;
					}
				}
				Log.Warning(string.Concat(new object[]
				{
					claimant,
					" with job ",
					job,
					" tried to release reservation on target ",
					target,
					", but it's not reserved by him."
				}), false);
			}
		}

		public bool CanReserve(Pawn claimant, IAttackTarget target)
		{
			bool result;
			if (this.IsReservedBy(claimant, target))
			{
				result = true;
			}
			else
			{
				int reservationsCount = this.GetReservationsCount(target, claimant.Faction);
				int maxPreferredReservationsCount = this.GetMaxPreferredReservationsCount(target);
				result = (reservationsCount < maxPreferredReservationsCount);
			}
			return result;
		}

		public bool IsReservedBy(Pawn claimant, IAttackTarget target)
		{
			for (int i = 0; i < this.reservations.Count; i++)
			{
				AttackTargetReservationManager.AttackTargetReservation attackTargetReservation = this.reservations[i];
				if (attackTargetReservation.target == target && attackTargetReservation.claimant == claimant)
				{
					return true;
				}
			}
			return false;
		}

		public void ReleaseAllForTarget(IAttackTarget target)
		{
			this.reservations.RemoveAll((AttackTargetReservationManager.AttackTargetReservation x) => x.target == target);
		}

		public void ReleaseClaimedBy(Pawn claimant, Job job)
		{
			for (int i = this.reservations.Count - 1; i >= 0; i--)
			{
				if (this.reservations[i].claimant == claimant && this.reservations[i].job == job)
				{
					this.reservations.RemoveAt(i);
				}
			}
		}

		public void ReleaseAllClaimedBy(Pawn claimant)
		{
			for (int i = this.reservations.Count - 1; i >= 0; i--)
			{
				if (this.reservations[i].claimant == claimant)
				{
					this.reservations.RemoveAt(i);
				}
			}
		}

		public IAttackTarget FirstReservationFor(Pawn claimant)
		{
			for (int i = this.reservations.Count - 1; i >= 0; i--)
			{
				if (this.reservations[i].claimant == claimant)
				{
					return this.reservations[i].target;
				}
			}
			return null;
		}

		public void ExposeData()
		{
			Scribe_Collections.Look<AttackTargetReservationManager.AttackTargetReservation>(ref this.reservations, "reservations", LookMode.Deep, new object[0]);
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				this.reservations.RemoveAll((AttackTargetReservationManager.AttackTargetReservation x) => x.target == null);
				if (this.reservations.RemoveAll((AttackTargetReservationManager.AttackTargetReservation x) => x.claimant.DestroyedOrNull()) != 0)
				{
					Log.Warning("Some attack target reservations had null or destroyed claimant.", false);
				}
			}
		}

		private int GetReservationsCount(IAttackTarget target, Faction faction)
		{
			int num = 0;
			for (int i = 0; i < this.reservations.Count; i++)
			{
				AttackTargetReservationManager.AttackTargetReservation attackTargetReservation = this.reservations[i];
				if (attackTargetReservation.target == target && attackTargetReservation.claimant.Faction == faction)
				{
					num++;
				}
			}
			return num;
		}

		private int GetMaxPreferredReservationsCount(IAttackTarget target)
		{
			int num = 0;
			Thing thing = target.Thing;
			CellRect cellRect = thing.OccupiedRect();
			foreach (IntVec3 c in cellRect.ExpandedBy(1))
			{
				if (!cellRect.Contains(c))
				{
					if (c.InBounds(this.map))
					{
						if (c.Standable(this.map))
						{
							num++;
						}
					}
				}
			}
			return num;
		}

		[CompilerGenerated]
		private static bool <ExposeData>m__0(AttackTargetReservationManager.AttackTargetReservation x)
		{
			return x.target == null;
		}

		[CompilerGenerated]
		private static bool <ExposeData>m__1(AttackTargetReservationManager.AttackTargetReservation x)
		{
			return x.claimant.DestroyedOrNull();
		}

		public class AttackTargetReservation : IExposable
		{
			public IAttackTarget target;

			public Pawn claimant;

			public Job job;

			public AttackTargetReservation()
			{
			}

			public void ExposeData()
			{
				Scribe_References.Look<IAttackTarget>(ref this.target, "target", false);
				Scribe_References.Look<Pawn>(ref this.claimant, "claimant", false);
				Scribe_References.Look<Job>(ref this.job, "job", false);
			}
		}

		[CompilerGenerated]
		private sealed class <ReleaseAllForTarget>c__AnonStorey0
		{
			internal IAttackTarget target;

			public <ReleaseAllForTarget>c__AnonStorey0()
			{
			}

			internal bool <>m__0(AttackTargetReservationManager.AttackTargetReservation x)
			{
				return x.target == this.target;
			}
		}
	}
}
