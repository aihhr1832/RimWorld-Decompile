namespace RimWorld
{
	public class ITab_Pawn_Guest : ITab_Pawn_Visitor
	{
		public override bool IsVisible
		{
			get
			{
				return base.SelPawn.HostFaction == Faction.OfPlayer && !base.SelPawn.IsPrisoner;
			}
		}

		public ITab_Pawn_Guest()
		{
			base.labelKey = "TabGuest";
			base.tutorTag = "Guest";
		}
	}
}
