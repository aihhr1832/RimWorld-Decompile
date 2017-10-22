using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld
{
	public abstract class WorkGiver_Warden : WorkGiver_Scanner
	{
		public override ThingRequest PotentialWorkThingRequest
		{
			get
			{
				return ThingRequest.ForGroup(ThingRequestGroup.Pawn);
			}
		}

		public override PathEndMode PathEndMode
		{
			get
			{
				return PathEndMode.OnCell;
			}
		}

		protected bool ShouldTakeCareOfPrisoner(Pawn warden, Thing prisoner)
		{
			Pawn pawn = prisoner as Pawn;
			if (pawn != null && pawn.IsPrisonerOfColony && pawn.guest.PrisonerIsSecure && pawn.Spawned && !pawn.InAggroMentalState && !prisoner.IsForbidden(warden) && (pawn.GetLord() == null || !(pawn.GetLord().LordJob is LordJob_FormAndSendCaravan)) && warden.CanReserveAndReach((Thing)pawn, PathEndMode.OnCell, warden.NormalMaxDanger(), 1, -1, null, false))
			{
				return true;
			}
			return false;
		}
	}
}
