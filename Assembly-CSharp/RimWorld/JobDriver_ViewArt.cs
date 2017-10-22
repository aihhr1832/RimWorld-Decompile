using System;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_ViewArt : JobDriver_VisitJoyThing
	{
		private Thing ArtThing
		{
			get
			{
				return base.CurJob.GetTarget(TargetIndex.A).Thing;
			}
		}

		protected override Action GetWaitTickAction()
		{
			return (Action)delegate
			{
				float statValue = this.ArtThing.GetStatValue(StatDefOf.EntertainmentStrengthFactor, true);
				float num = this.ArtThing.GetStatValue(StatDefOf.Beauty, true) / this.ArtThing.def.GetStatValueAbstract(StatDefOf.Beauty, null);
				statValue = (float)(statValue * ((!(num > 0.0)) ? 0.0 : num));
				base.pawn.GainComfortFromCellIfPossible();
				float extraJoyGainFactor = statValue;
				JoyUtility.JoyTickCheckEnd(base.pawn, JoyTickFullJoyAction.EndJob, extraJoyGainFactor);
			};
		}
	}
}
