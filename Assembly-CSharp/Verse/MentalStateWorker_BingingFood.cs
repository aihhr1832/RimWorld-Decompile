namespace Verse
{
	public class MentalStateWorker_BingingFood : MentalStateWorker
	{
		public override bool StateCanOccur(Pawn pawn)
		{
			if (!base.StateCanOccur(pawn))
			{
				return false;
			}
			if (!pawn.Spawned)
			{
				return true;
			}
			return pawn.Map.resourceCounter.TotalHumanEdibleNutrition > 10.0;
		}
	}
}
