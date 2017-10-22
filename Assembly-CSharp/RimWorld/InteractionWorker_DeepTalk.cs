using Verse;

namespace RimWorld
{
	public class InteractionWorker_DeepTalk : InteractionWorker
	{
		private const float BaseSelectionWeight = 0.075f;

		private SimpleCurve CompatibilityFactorCurve = new SimpleCurve
		{
			{
				new CurvePoint(-1.5f, 0f),
				true
			},
			{
				new CurvePoint(-0.5f, 0.1f),
				true
			},
			{
				new CurvePoint(0.5f, 1f),
				true
			},
			{
				new CurvePoint(1f, 1.8f),
				true
			},
			{
				new CurvePoint(2f, 3f),
				true
			}
		};

		public override float RandomSelectionWeight(Pawn initiator, Pawn recipient)
		{
			float num = 0.075f;
			return num * this.CompatibilityFactorCurve.Evaluate(initiator.relations.CompatibilityWith(recipient));
		}
	}
}
