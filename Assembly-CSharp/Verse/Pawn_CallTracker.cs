using RimWorld;
using System;

namespace Verse
{
	public class Pawn_CallTracker
	{
		private const float AngryCallOnMeleeChance = 0.5f;

		private const int AggressiveDurationAfterEngagingTarget = 360;

		public Pawn pawn;

		private int ticksToNextCall = -1;

		private static readonly IntRange CallOnAggroDelayRange = new IntRange(0, 120);

		private static readonly IntRange CallOnMeleeDelayRange = new IntRange(0, 20);

		private bool PawnAggressive
		{
			get
			{
				if (this.pawn.InAggroMentalState)
				{
					return true;
				}
				if (this.pawn.mindState.enemyTarget != null && this.pawn.mindState.enemyTarget.Spawned && Find.TickManager.TicksGame - this.pawn.mindState.lastEngageTargetTick <= 360)
				{
					return true;
				}
				if (this.pawn.CurJob != null && this.pawn.CurJob.def == JobDefOf.AttackMelee)
				{
					return true;
				}
				return false;
			}
		}

		private float IdleCallVolumeFactor
		{
			get
			{
				switch (Find.TickManager.CurTimeSpeed)
				{
				case TimeSpeed.Paused:
				{
					return 1f;
				}
				case TimeSpeed.Normal:
				{
					return 1f;
				}
				case TimeSpeed.Fast:
				{
					return 1f;
				}
				case TimeSpeed.Superfast:
				{
					return 0.25f;
				}
				case TimeSpeed.Ultrafast:
				{
					return 0.25f;
				}
				default:
				{
					throw new NotImplementedException();
				}
				}
			}
		}

		public Pawn_CallTracker(Pawn pawn)
		{
			this.pawn = pawn;
		}

		public void CallTrackerTick()
		{
			if (this.ticksToNextCall < 0)
			{
				this.ResetTicksToNextCall();
			}
			this.ticksToNextCall--;
			if (this.ticksToNextCall <= 0)
			{
				this.TryDoCall();
				this.ResetTicksToNextCall();
			}
		}

		private void ResetTicksToNextCall()
		{
			this.ticksToNextCall = this.pawn.def.race.soundCallIntervalRange.RandomInRange;
			if (this.PawnAggressive)
			{
				this.ticksToNextCall /= 4;
			}
		}

		private void TryDoCall()
		{
			if (Find.CameraDriver.CurrentViewRect.ExpandedBy(10).Contains(this.pawn.Position) && !this.pawn.Downed && this.pawn.Awake() && !this.pawn.Position.Fogged(this.pawn.Map))
			{
				this.DoCall();
			}
		}

		public void DoCall()
		{
			if (this.pawn.Spawned)
			{
				if (this.PawnAggressive)
				{
					LifeStageUtility.PlayNearestLifestageSound(this.pawn, (Func<LifeStageAge, SoundDef>)((LifeStageAge ls) => ls.soundAngry), 1f);
				}
				else
				{
					LifeStageUtility.PlayNearestLifestageSound(this.pawn, (Func<LifeStageAge, SoundDef>)((LifeStageAge ls) => ls.soundCall), this.IdleCallVolumeFactor);
				}
			}
		}

		public void Notify_InAggroMentalState()
		{
			this.ticksToNextCall = Pawn_CallTracker.CallOnAggroDelayRange.RandomInRange;
		}

		public void Notify_DidMeleeAttack()
		{
			if (Rand.Value < 0.5)
			{
				this.ticksToNextCall = Pawn_CallTracker.CallOnMeleeDelayRange.RandomInRange;
			}
		}

		public void Notify_Released()
		{
			if (Rand.Value < 0.75)
			{
				this.ticksToNextCall = Pawn_CallTracker.CallOnAggroDelayRange.RandomInRange;
			}
		}
	}
}
