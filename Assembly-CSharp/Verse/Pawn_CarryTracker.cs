﻿using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse.AI;
using Verse.Sound;

namespace Verse
{
	public class Pawn_CarryTracker : IThingHolder, IExposable
	{
		public Pawn pawn;

		public ThingOwner<Thing> innerContainer;

		public Pawn_CarryTracker(Pawn pawn)
		{
			this.pawn = pawn;
			this.innerContainer = new ThingOwner<Thing>(this, true, LookMode.Deep);
		}

		public Thing CarriedThing
		{
			get
			{
				Thing result;
				if (this.innerContainer.Count == 0)
				{
					result = null;
				}
				else
				{
					result = this.innerContainer[0];
				}
				return result;
			}
		}

		public bool Full
		{
			get
			{
				return this.AvailableStackSpace(this.CarriedThing.def) <= 0;
			}
		}

		public IThingHolder ParentHolder
		{
			get
			{
				return this.pawn;
			}
		}

		public void ExposeData()
		{
			Scribe_Deep.Look<ThingOwner<Thing>>(ref this.innerContainer, "innerContainer", new object[]
			{
				this
			});
		}

		public ThingOwner GetDirectlyHeldThings()
		{
			return this.innerContainer;
		}

		public void GetChildHolders(List<IThingHolder> outChildren)
		{
			ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, this.GetDirectlyHeldThings());
		}

		public int AvailableStackSpace(ThingDef td)
		{
			int num = this.MaxStackSpaceEver(td);
			if (this.CarriedThing != null)
			{
				num -= this.CarriedThing.stackCount;
			}
			return num;
		}

		public int MaxStackSpaceEver(ThingDef td)
		{
			float f = this.pawn.GetStatValue(StatDefOf.CarryingCapacity, true) / td.VolumePerUnit;
			int b = Mathf.RoundToInt(f);
			return Mathf.Min(td.stackLimit, b);
		}

		public bool TryStartCarry(Thing item)
		{
			bool result;
			if (this.pawn.Dead || this.pawn.Downed)
			{
				Log.Error("Dead/downed pawn " + this.pawn + " tried to start carry item.", false);
				result = false;
			}
			else if (this.innerContainer.TryAdd(item, true))
			{
				item.def.soundPickup.PlayOneShot(new TargetInfo(item.Position, this.pawn.Map, false));
				result = true;
			}
			else
			{
				result = false;
			}
			return result;
		}

		public int TryStartCarry(Thing item, int count, bool reserve = true)
		{
			int result;
			if (this.pawn.Dead || this.pawn.Downed)
			{
				Log.Error(string.Concat(new object[]
				{
					"Dead/downed pawn ",
					this.pawn,
					" tried to start carry ",
					item.ToStringSafe<Thing>()
				}), false);
				result = 0;
			}
			else
			{
				count = Mathf.Min(count, this.AvailableStackSpace(item.def));
				count = Mathf.Min(count, item.stackCount);
				int num = this.innerContainer.TryAdd(item.SplitOff(count), count, true);
				if (num > 0)
				{
					item.def.soundPickup.PlayOneShot(new TargetInfo(item.Position, this.pawn.Map, false));
					if (reserve)
					{
						this.pawn.Reserve(this.CarriedThing, this.pawn.CurJob, 1, -1, null);
					}
				}
				result = num;
			}
			return result;
		}

		public bool TryDropCarriedThing(IntVec3 dropLoc, ThingPlaceMode mode, out Thing resultingThing, Action<Thing, int> placedAction = null)
		{
			bool result;
			if (this.innerContainer.TryDrop(this.CarriedThing, dropLoc, this.pawn.MapHeld, mode, out resultingThing, placedAction, null))
			{
				if (resultingThing != null && this.pawn.Faction.HostileTo(Faction.OfPlayer))
				{
					resultingThing.SetForbidden(true, false);
				}
				result = true;
			}
			else
			{
				result = false;
			}
			return result;
		}

		public bool TryDropCarriedThing(IntVec3 dropLoc, int count, ThingPlaceMode mode, out Thing resultingThing, Action<Thing, int> placedAction = null)
		{
			bool result;
			if (this.innerContainer.TryDrop(this.CarriedThing, dropLoc, this.pawn.MapHeld, mode, count, out resultingThing, placedAction, null))
			{
				if (resultingThing != null && this.pawn.Faction.HostileTo(Faction.OfPlayer))
				{
					resultingThing.SetForbidden(true, false);
				}
				result = true;
			}
			else
			{
				result = false;
			}
			return result;
		}

		public void DestroyCarriedThing()
		{
			this.innerContainer.ClearAndDestroyContents(DestroyMode.Vanish);
		}

		public void CarryHandsTick()
		{
			this.innerContainer.ThingOwnerTick(true);
		}
	}
}
