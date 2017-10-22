using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class CompTargetable_AllPawnsOnTheMap : CompTargetable
	{
		protected override bool PlayerChoosesTarget
		{
			get
			{
				return false;
			}
		}

		protected override TargetingParameters GetTargetingParameters()
		{
			TargetingParameters targetingParameters = new TargetingParameters();
			targetingParameters.canTargetPawns = true;
			targetingParameters.canTargetBuildings = false;
			targetingParameters.validator = (Predicate<TargetInfo>)((TargetInfo x) => base.BaseTargetValidator(x.Thing));
			return targetingParameters;
		}

		public override IEnumerable<Thing> GetTargets(Thing targetChosenByPlayer = null)
		{
			if (base.parent.MapHeld != null)
			{
				TargetingParameters tp = this.GetTargetingParameters();
				List<Pawn>.Enumerator enumerator = base.parent.MapHeld.mapPawns.AllPawnsSpawned.GetEnumerator();
				try
				{
					while (enumerator.MoveNext())
					{
						Pawn p = enumerator.Current;
						if (tp.CanTarget((Thing)p))
						{
							yield return (Thing)p;
						}
					}
				}
				finally
				{
					((IDisposable)(object)enumerator).Dispose();
				}
			}
		}
	}
}
