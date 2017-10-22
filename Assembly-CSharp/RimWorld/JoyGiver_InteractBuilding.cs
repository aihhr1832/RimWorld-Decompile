using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public abstract class JoyGiver_InteractBuilding : JoyGiver
	{
		protected virtual bool CanDoDuringParty
		{
			get
			{
				return false;
			}
		}

		public override Job TryGiveJob(Pawn pawn)
		{
			Thing thing = this.FindBestGame(pawn, false, IntVec3.Invalid);
			if (thing != null)
			{
				return this.TryGivePlayJob(pawn, thing);
			}
			return null;
		}

		public override Job TryGiveJobWhileInBed(Pawn pawn)
		{
			Thing thing = this.FindBestGame(pawn, true, IntVec3.Invalid);
			if (thing != null)
			{
				return this.TryGivePlayJobWhileInBed(pawn, thing);
			}
			return null;
		}

		public override Job TryGiveJobInPartyArea(Pawn pawn, IntVec3 partySpot)
		{
			if (!this.CanDoDuringParty)
			{
				return null;
			}
			Thing thing = this.FindBestGame(pawn, false, partySpot);
			if (thing != null)
			{
				return this.TryGivePlayJob(pawn, thing);
			}
			return null;
		}

		private Thing FindBestGame(Pawn pawn, bool inBed, IntVec3 partySpot)
		{
			List<Thing> searchSet = this.GetSearchSet(pawn);
			Predicate<Thing> predicate = (Predicate<Thing>)((Thing t) => this.CanInteractWith(pawn, t, inBed));
			if (partySpot.IsValid)
			{
				Predicate<Thing> oldValidator = predicate;
				predicate = (Predicate<Thing>)((Thing x) => PartyUtility.InPartyArea(x.Position, partySpot, pawn.Map) && oldValidator(x));
			}
			Predicate<Thing> validator = predicate;
			return GenClosest.ClosestThing_Global_Reachable(pawn.Position, pawn.Map, searchSet, PathEndMode.OnCell, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false), 9999f, validator, null);
		}

		protected virtual bool CanInteractWith(Pawn pawn, Thing t, bool inBed)
		{
			if (!pawn.CanReserve(t, base.def.jobDef.joyMaxParticipants, -1, null, false))
			{
				return false;
			}
			if (t.IsForbidden(pawn))
			{
				return false;
			}
			if (!t.IsSociallyProper(pawn))
			{
				return false;
			}
			CompPowerTrader compPowerTrader = t.TryGetComp<CompPowerTrader>();
			if (compPowerTrader != null && !compPowerTrader.PowerOn)
			{
				return false;
			}
			if (base.def.unroofedOnly && t.Position.Roofed(t.Map))
			{
				return false;
			}
			return true;
		}

		protected abstract Job TryGivePlayJob(Pawn pawn, Thing bestGame);

		protected virtual Job TryGivePlayJobWhileInBed(Pawn pawn, Thing bestGame)
		{
			Building_Bed t = pawn.CurrentBed();
			return new Job(base.def.jobDef, bestGame, pawn.Position, (Thing)t);
		}
	}
}
