namespace Verse.AI
{
	public class JobGiver_WanderNearDutyLocation : JobGiver_Wander
	{
		public JobGiver_WanderNearDutyLocation()
		{
			base.wanderRadius = 7f;
			base.ticksBetweenWandersRange = new IntRange(125, 200);
		}

		protected override IntVec3 GetWanderRoot(Pawn pawn)
		{
			return WanderUtility.BestCloseWanderRoot(pawn.mindState.duty.focus.Cell, pawn);
		}
	}
}
