using System;
using System.Collections.Generic;

namespace Verse.Sound
{
	public static class SustainerAggregatorUtility
	{
		private static float AggregateRadius = 12f;

		public static Sustainer AggregateOrSpawnSustainerFor(ISizeReporter reporter, SoundDef def, SoundInfo info)
		{
			Sustainer sustainer = null;
			List<Sustainer>.Enumerator enumerator = Find.SoundRoot.sustainerManager.AllSustainers.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					Sustainer current = enumerator.Current;
					if (current.def == def && current.info.Maker.Map == info.Maker.Map && current.info.Maker.Cell.InHorDistOf(info.Maker.Cell, SustainerAggregatorUtility.AggregateRadius))
					{
						sustainer = current;
						break;
					}
				}
			}
			finally
			{
				((IDisposable)(object)enumerator).Dispose();
			}
			if (sustainer == null)
			{
				sustainer = def.TrySpawnSustainer(info);
			}
			else
			{
				sustainer.Maintain();
			}
			if (sustainer.externalParams.sizeAggregator == null)
			{
				sustainer.externalParams.sizeAggregator = new SoundSizeAggregator();
			}
			sustainer.externalParams.sizeAggregator.RegisterReporter(reporter);
			return sustainer;
		}
	}
}
