using Verse;
using Verse.AI;

namespace RimWorld
{
	public class WorkGiver_ConstructFinishFrames : WorkGiver_Scanner
	{
		public override ThingRequest PotentialWorkThingRequest
		{
			get
			{
				return ThingRequest.ForGroup(ThingRequestGroup.BuildingFrame);
			}
		}

		public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			if (t.Faction != pawn.Faction)
			{
				return null;
			}
			Frame frame = t as Frame;
			if (frame != null)
			{
				if (!GenConstruct.CanConstruct(frame, pawn, forced))
				{
					return null;
				}
				if (frame.MaterialsNeeded().Count > 0)
				{
					return null;
				}
				return new Job(JobDefOf.FinishFrame, (Thing)frame);
			}
			return null;
		}
	}
}
