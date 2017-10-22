using Verse;

namespace RimWorld
{
	public class RoomStatWorker_ResearchSpeedFactor : RoomStatWorker
	{
		private static readonly SimpleCurve CleanlinessFactorCurve = new SimpleCurve
		{
			{
				new CurvePoint(-5f, 0.75f),
				true
			},
			{
				new CurvePoint(-2.5f, 0.85f),
				true
			},
			{
				new CurvePoint(0f, 1f),
				true
			},
			{
				new CurvePoint(1f, 1.15f),
				true
			}
		};

		public override float GetScore(Room room)
		{
			float stat = room.GetStat(RoomStatDefOf.Cleanliness);
			return RoomStatWorker_ResearchSpeedFactor.CleanlinessFactorCurve.Evaluate(stat);
		}
	}
}
