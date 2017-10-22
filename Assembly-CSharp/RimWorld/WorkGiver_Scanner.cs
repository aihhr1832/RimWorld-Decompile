using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public abstract class WorkGiver_Scanner : WorkGiver
	{
		public virtual ThingRequest PotentialWorkThingRequest
		{
			get
			{
				return ThingRequest.ForGroup(ThingRequestGroup.Undefined);
			}
		}

		public virtual int LocalRegionsToScanFirst
		{
			get
			{
				return -1;
			}
		}

		public virtual bool Prioritized
		{
			get
			{
				return false;
			}
		}

		public virtual PathEndMode PathEndMode
		{
			get
			{
				return PathEndMode.Touch;
			}
		}

		public virtual IEnumerable<IntVec3> PotentialWorkCellsGlobal(Pawn pawn)
		{
			yield break;
		}

		public virtual IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
		{
			return null;
		}

		public virtual bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			return this.JobOnThing(pawn, t, forced) != null;
		}

		public virtual Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			return null;
		}

		public virtual bool HasJobOnCell(Pawn pawn, IntVec3 c)
		{
			return this.JobOnCell(pawn, c) != null;
		}

		public virtual Job JobOnCell(Pawn pawn, IntVec3 cell)
		{
			return null;
		}

		public virtual float GetPriority(Pawn pawn, TargetInfo t)
		{
			return 0f;
		}

		public float GetPriority(Pawn pawn, IntVec3 cell)
		{
			return this.GetPriority(pawn, new TargetInfo(cell, pawn.Map, false));
		}
	}
}
