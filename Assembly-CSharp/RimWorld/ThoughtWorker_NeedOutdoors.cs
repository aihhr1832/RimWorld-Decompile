﻿using System;
using Verse;

namespace RimWorld
{
	public class ThoughtWorker_NeedOutdoors : ThoughtWorker
	{
		public ThoughtWorker_NeedOutdoors()
		{
		}

		protected override ThoughtState CurrentStateInternal(Pawn p)
		{
			ThoughtState result;
			if (p.needs.outdoors == null)
			{
				result = ThoughtState.Inactive;
			}
			else if (p.HostFaction != null)
			{
				result = ThoughtState.Inactive;
			}
			else
			{
				switch (p.needs.outdoors.CurCategory)
				{
				case OutdoorsCategory.Entombed:
					result = ThoughtState.ActiveAtStage(0);
					break;
				case OutdoorsCategory.Trapped:
					result = ThoughtState.ActiveAtStage(1);
					break;
				case OutdoorsCategory.CabinFeverSevere:
					result = ThoughtState.ActiveAtStage(2);
					break;
				case OutdoorsCategory.CabinFeverLight:
					result = ThoughtState.ActiveAtStage(3);
					break;
				case OutdoorsCategory.NeedFreshAir:
					result = ThoughtState.ActiveAtStage(4);
					break;
				case OutdoorsCategory.Free:
					result = ThoughtState.Inactive;
					break;
				default:
					throw new InvalidOperationException("Unknown OutdoorsCategory");
				}
			}
			return result;
		}
	}
}
