﻿using System;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobGiver_AIFightEnemies : JobGiver_AIFightEnemy
	{
		public JobGiver_AIFightEnemies()
		{
		}

		protected override bool TryFindShootingPosition(Pawn pawn, out IntVec3 dest)
		{
			Thing enemyTarget = pawn.mindState.enemyTarget;
			bool allowManualCastWeapons = !pawn.IsColonist;
			Verb verb = pawn.TryGetAttackVerb(enemyTarget, allowManualCastWeapons);
			bool result;
			if (verb == null)
			{
				dest = IntVec3.Invalid;
				result = false;
			}
			else
			{
				result = CastPositionFinder.TryFindCastPosition(new CastPositionRequest
				{
					caster = pawn,
					target = enemyTarget,
					verb = verb,
					maxRangeFromTarget = verb.verbProps.range,
					wantCoverFromTarget = (verb.verbProps.range > 5f)
				}, out dest);
			}
			return result;
		}
	}
}
