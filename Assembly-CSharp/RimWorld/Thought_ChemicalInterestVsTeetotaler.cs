namespace RimWorld
{
	public class Thought_ChemicalInterestVsTeetotaler : Thought_SituationalSocial
	{
		public override float OpinionOffset()
		{
			int num = base.pawn.story.traits.DegreeOfTrait(TraitDefOf.DrugDesire);
			int num2 = base.otherPawn.story.traits.DegreeOfTrait(TraitDefOf.DrugDesire);
			if (num2 >= 0)
			{
				return 0f;
			}
			if (num == 1)
			{
				return -20f;
			}
			return -30f;
		}
	}
}
