using UnityEngine;

namespace Verse
{
	public class Stance_Cooldown : Stance_Busy
	{
		private const float RadiusPerTick = 0.002f;

		private const float MaxRadius = 0.5f;

		public Stance_Cooldown()
		{
		}

		public Stance_Cooldown(int ticks, LocalTargetInfo focusTarg, Verb verb) : base(ticks, focusTarg, verb)
		{
		}

		public override void StanceDraw()
		{
			if (Find.Selector.IsSelected(base.stanceTracker.pawn))
			{
				float radius = Mathf.Min(0.5f, (float)((float)base.ticksLeft * 0.0020000000949949026));
				GenDraw.DrawCooldownCircle(base.stanceTracker.pawn.Drawer.DrawPos + new Vector3(0f, 0.2f, 0f), radius);
			}
		}
	}
}
