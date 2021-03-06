﻿using System;
using RimWorld;

namespace Verse
{
	public class PawnCapacitiesHandler
	{
		private Pawn pawn;

		private DefMap<PawnCapacityDef, PawnCapacitiesHandler.CacheElement> cachedCapacityLevels = null;

		public PawnCapacitiesHandler(Pawn pawn)
		{
			this.pawn = pawn;
		}

		public bool CanBeAwake
		{
			get
			{
				return this.GetLevel(PawnCapacityDefOf.Consciousness) >= 0.3f;
			}
		}

		public void Clear()
		{
			this.cachedCapacityLevels = null;
		}

		public float GetLevel(PawnCapacityDef capacity)
		{
			float result;
			if (this.pawn.health.Dead)
			{
				result = 0f;
			}
			else
			{
				if (this.cachedCapacityLevels == null)
				{
					this.Notify_CapacityLevelsDirty();
				}
				PawnCapacitiesHandler.CacheElement cacheElement = this.cachedCapacityLevels[capacity];
				if (cacheElement.status == PawnCapacitiesHandler.CacheStatus.Caching)
				{
					Log.Error(string.Format("Detected infinite stat recursion when evaluating {0}", capacity), false);
					result = 0f;
				}
				else
				{
					if (cacheElement.status == PawnCapacitiesHandler.CacheStatus.Uncached)
					{
						cacheElement.status = PawnCapacitiesHandler.CacheStatus.Caching;
						try
						{
							cacheElement.value = PawnCapacityUtility.CalculateCapacityLevel(this.pawn.health.hediffSet, capacity, null);
						}
						finally
						{
							cacheElement.status = PawnCapacitiesHandler.CacheStatus.Cached;
						}
					}
					result = cacheElement.value;
				}
			}
			return result;
		}

		public bool CapableOf(PawnCapacityDef capacity)
		{
			return this.GetLevel(capacity) > capacity.minForCapable;
		}

		public void Notify_CapacityLevelsDirty()
		{
			if (this.cachedCapacityLevels == null)
			{
				this.cachedCapacityLevels = new DefMap<PawnCapacityDef, PawnCapacitiesHandler.CacheElement>();
			}
			for (int i = 0; i < this.cachedCapacityLevels.Count; i++)
			{
				this.cachedCapacityLevels[i].status = PawnCapacitiesHandler.CacheStatus.Uncached;
			}
		}

		private enum CacheStatus
		{
			Uncached,
			Caching,
			Cached
		}

		private class CacheElement
		{
			public PawnCapacitiesHandler.CacheStatus status;

			public float value;

			public CacheElement()
			{
			}
		}
	}
}
