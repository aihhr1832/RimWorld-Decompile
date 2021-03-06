﻿using System;
using System.Runtime.CompilerServices;

namespace Verse.AI
{
	public class JobGiver_WanderCurrentRoom : JobGiver_Wander
	{
		[CompilerGenerated]
		private static Func<Pawn, IntVec3, IntVec3, bool> <>f__am$cache0;

		public JobGiver_WanderCurrentRoom()
		{
			this.wanderRadius = 7f;
			this.ticksBetweenWandersRange = new IntRange(125, 200);
			this.locomotionUrgency = LocomotionUrgency.Amble;
			this.wanderDestValidator = ((Pawn pawn, IntVec3 loc, IntVec3 root) => WanderRoomUtility.IsValidWanderDest(pawn, loc, root));
		}

		protected override IntVec3 GetWanderRoot(Pawn pawn)
		{
			return pawn.Position;
		}

		[CompilerGenerated]
		private static bool <JobGiver_WanderCurrentRoom>m__0(Pawn pawn, IntVec3 loc, IntVec3 root)
		{
			return WanderRoomUtility.IsValidWanderDest(pawn, loc, root);
		}
	}
}
