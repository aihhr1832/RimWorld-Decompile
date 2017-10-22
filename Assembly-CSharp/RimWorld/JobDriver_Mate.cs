using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_Mate : JobDriver
	{
		private const int MateDuration = 500;

		private const TargetIndex FemInd = TargetIndex.A;

		private const int TicksBetweenHeartMotes = 100;

		private Pawn Female
		{
			get
			{
				return (Pawn)base.CurJob.GetTarget(TargetIndex.A).Thing;
			}
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
			this.FailOnDowned(TargetIndex.A);
			this.FailOnNotCasualInterruptible(TargetIndex.A);
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
			Toil prepare = Toils_General.WaitWith(TargetIndex.A, 500, false, false);
			prepare.tickAction = (Action)delegate
			{
				if (((_003CMakeNewToils_003Ec__Iterator1)/*Error near IL_0082: stateMachine*/)._003C_003Ef__this.pawn.IsHashIntervalTick(100))
				{
					MoteMaker.ThrowMetaIcon(((_003CMakeNewToils_003Ec__Iterator1)/*Error near IL_0082: stateMachine*/)._003C_003Ef__this.pawn.Position, ((_003CMakeNewToils_003Ec__Iterator1)/*Error near IL_0082: stateMachine*/)._003C_003Ef__this.pawn.Map, ThingDefOf.Mote_Heart);
				}
				if (((_003CMakeNewToils_003Ec__Iterator1)/*Error near IL_0082: stateMachine*/)._003C_003Ef__this.Female.IsHashIntervalTick(100))
				{
					MoteMaker.ThrowMetaIcon(((_003CMakeNewToils_003Ec__Iterator1)/*Error near IL_0082: stateMachine*/)._003C_003Ef__this.Female.Position, ((_003CMakeNewToils_003Ec__Iterator1)/*Error near IL_0082: stateMachine*/)._003C_003Ef__this.pawn.Map, ThingDefOf.Mote_Heart);
				}
			};
			yield return prepare;
			yield return new Toil
			{
				initAction = (Action)delegate
				{
					Pawn actor = ((_003CMakeNewToils_003Ec__Iterator1)/*Error near IL_00bc: stateMachine*/)._003Cfinalize_003E__1.actor;
					Pawn female = ((_003CMakeNewToils_003Ec__Iterator1)/*Error near IL_00bc: stateMachine*/)._003C_003Ef__this.Female;
					PawnUtility.Mated(actor, female);
				},
				defaultCompleteMode = ToilCompleteMode.Instant
			};
		}
	}
}
