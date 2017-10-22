using System.Collections.Generic;

namespace Verse.AI
{
	public class JobDriver_Kill : JobDriver
	{
		private const TargetIndex VictimInd = TargetIndex.A;

		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.EndOnDespawnedOrNull(TargetIndex.A, JobCondition.Succeeded);
			yield return Toils_Reserve.Reserve(TargetIndex.A, 1, -1, null);
			yield return Toils_Combat.TrySetJobToUseAttackVerb();
			Toil gotoCastPos = Toils_Combat.GotoCastPosition(TargetIndex.A, false);
			yield return gotoCastPos;
			Toil jumpIfCannotHit = Toils_Jump.JumpIfTargetNotHittable(TargetIndex.A, gotoCastPos);
			yield return jumpIfCannotHit;
			yield return Toils_Combat.CastVerb(TargetIndex.A, true);
			yield return Toils_Jump.Jump(jumpIfCannotHit);
		}
	}
}
