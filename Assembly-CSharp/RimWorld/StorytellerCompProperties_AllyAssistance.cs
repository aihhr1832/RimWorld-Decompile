namespace RimWorld
{
	public class StorytellerCompProperties_AllyAssistance : StorytellerCompProperties
	{
		public float baseMtb = 99999f;

		public StorytellerCompProperties_AllyAssistance()
		{
			base.compClass = typeof(StorytellerComp_AllyAssistance);
		}
	}
}
