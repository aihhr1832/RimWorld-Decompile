using Verse;

namespace RimWorld
{
	public class StatPart_Outdoors : StatPart
	{
		private float factorIndoors = 1f;

		private float factorOutdoors = 1f;

		public override void TransformValue(StatRequest req, ref float val)
		{
			val *= this.OutdoorsFactor(req);
		}

		public override string ExplanationPart(StatRequest req)
		{
			if (req.HasThing)
			{
				Room room = req.Thing.GetRoom(RegionType.Set_All);
				if (room != null)
				{
					string str = (!room.PsychologicallyOutdoors) ? "Indoors".Translate() : "Outdoors".Translate();
					return str + ": x" + this.OutdoorsFactor(req).ToStringPercent();
				}
			}
			return (string)null;
		}

		private float OutdoorsFactor(StatRequest req)
		{
			if (req.HasThing)
			{
				Room room = req.Thing.GetRoom(RegionType.Set_All);
				if (room != null)
				{
					if (room.PsychologicallyOutdoors)
					{
						return this.factorOutdoors;
					}
					return this.factorIndoors;
				}
			}
			return 1f;
		}
	}
}
