﻿using System;
using System.Runtime.CompilerServices;
using RimWorld;

namespace Verse
{
	public class Pawn_CallTracker
	{
		public Pawn pawn;

		private int ticksToNextCall = -1;

		private static readonly IntRange CallOnAggroDelayRange = new IntRange(0, 120);

		private static readonly IntRange CallOnMeleeDelayRange = new IntRange(0, 20);

		private const float AngryCallOnMeleeChance = 0.5f;

		private const int AggressiveDurationAfterEngagingTarget = 360;

		[CompilerGenerated]
		private static Func<LifeStageAge, SoundDef> <>f__am$cache0;

		[CompilerGenerated]
		private static Func<LifeStageAge, SoundDef> <>f__am$cache1;

		public Pawn_CallTracker(Pawn pawn)
		{
			this.pawn = pawn;
		}

		private bool PawnAggressive
		{
			get
			{
				return this.pawn.InAggroMentalState || (this.pawn.mindState.enemyTarget != null && this.pawn.mindState.enemyTarget.Spawned && Find.TickManager.TicksGame - this.pawn.mindState.lastEngageTargetTick <= 360) || (this.pawn.CurJob != null && this.pawn.CurJob.def == JobDefOf.AttackMelee);
			}
		}

		private float IdleCallVolumeFactor
		{
			get
			{
				float result;
				switch (Find.TickManager.CurTimeSpeed)
				{
				case TimeSpeed.Paused:
					result = 1f;
					break;
				case TimeSpeed.Normal:
					result = 1f;
					break;
				case TimeSpeed.Fast:
					result = 1f;
					break;
				case TimeSpeed.Superfast:
					result = 0.25f;
					break;
				case TimeSpeed.Ultrafast:
					result = 0.25f;
					break;
				default:
					throw new NotImplementedException();
				}
				return result;
			}
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
			if (Find.CameraDriver.CurrentViewRect.ExpandedBy(10).Contains(this.pawn.Position))
			{
				if (!this.pawn.Downed && this.pawn.Awake())
				{
					if (!this.pawn.Position.Fogged(this.pawn.Map))
					{
						this.DoCall();
					}
				}
			}
		}

		public void DoCall()
		{
			if (this.pawn.Spawned)
			{
				if (this.PawnAggressive)
				{
					LifeStageUtility.PlayNearestLifestageSound(this.pawn, (LifeStageAge ls) => ls.soundAngry, 1f);
				}
				else
				{
					LifeStageUtility.PlayNearestLifestageSound(this.pawn, (LifeStageAge ls) => ls.soundCall, this.IdleCallVolumeFactor);
				}
			}
		}

		public void Notify_InAggroMentalState()
		{
			this.ticksToNextCall = Pawn_CallTracker.CallOnAggroDelayRange.RandomInRange;
		}

		public void Notify_DidMeleeAttack()
		{
			if (Rand.Value < 0.5f)
			{
				this.ticksToNextCall = Pawn_CallTracker.CallOnMeleeDelayRange.RandomInRange;
			}
		}

		public void Notify_Released()
		{
			if (Rand.Value < 0.75f)
			{
				this.ticksToNextCall = Pawn_CallTracker.CallOnAggroDelayRange.RandomInRange;
			}
		}

		// Note: this type is marked as 'beforefieldinit'.
		static Pawn_CallTracker()
		{
		}

		[CompilerGenerated]
		private static SoundDef <DoCall>m__0(LifeStageAge ls)
		{
			return ls.soundAngry;
		}

		[CompilerGenerated]
		private static SoundDef <DoCall>m__1(LifeStageAge ls)
		{
			return ls.soundCall;
		}
	}
}
