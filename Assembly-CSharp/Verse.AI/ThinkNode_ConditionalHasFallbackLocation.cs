namespace Verse.AI
{
	public class ThinkNode_ConditionalHasFallbackLocation : ThinkNode_Priority
	{
		public override ThinkResult TryIssueJobPackage(Pawn pawn, JobIssueParams jobParams)
		{
			if (pawn.mindState.duty != null && pawn.mindState.duty.focusSecond.IsValid)
			{
				return base.TryIssueJobPackage(pawn, jobParams);
			}
			return ThinkResult.NoJob;
		}
	}
}
