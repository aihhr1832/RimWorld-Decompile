﻿using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class PawnBreathMoteMaker
	{
		private Pawn pawn;

		private bool doThisBreath = false;

		private const int BreathDuration = 80;

		private const int BreathInterval = 320;

		private const int MoteInterval = 8;

		private const float MaxBreathTemperature = 0f;

		private static readonly Vector3 BreathOffset = new Vector3(0f, 0f, -0.04f);

		private const float BreathRotationOffsetDist = 0.21f;

		public PawnBreathMoteMaker(Pawn pawn)
		{
			this.pawn = pawn;
		}

		public void BreathMoteMakerTick()
		{
			if (this.pawn.RaceProps.Humanlike && !this.pawn.RaceProps.IsMechanoid)
			{
				int num = (Find.TickManager.TicksGame + this.pawn.HashOffset()) % 320;
				if (num == 0)
				{
					this.doThisBreath = (this.pawn.AmbientTemperature < 0f && this.pawn.GetPosture() != PawnPosture.Standing);
				}
				if (this.doThisBreath && num < 80 && num % 8 == 0)
				{
					this.TryMakeBreathMote();
				}
			}
		}

		private void TryMakeBreathMote()
		{
			Vector3 loc = this.pawn.Drawer.DrawPos + this.pawn.Drawer.renderer.BaseHeadOffsetAt(this.pawn.Rotation) + this.pawn.Rotation.FacingCell.ToVector3() * 0.21f + PawnBreathMoteMaker.BreathOffset;
			Vector3 lastTickTweenedVelocity = this.pawn.Drawer.tweener.LastTickTweenedVelocity;
			MoteMaker.ThrowBreathPuff(loc, this.pawn.Map, this.pawn.Rotation.AsAngle, lastTickTweenedVelocity);
		}

		// Note: this type is marked as 'beforefieldinit'.
		static PawnBreathMoteMaker()
		{
		}
	}
}
