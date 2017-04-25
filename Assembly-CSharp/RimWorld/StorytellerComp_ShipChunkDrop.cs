using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class StorytellerComp_ShipChunkDrop : StorytellerComp
	{
		private const float BaseShipChunkDropMTBDays = 20f;

		private float ShipChunkDropMTBDays
		{
			get
			{
				float num = (float)Find.TickManager.TicksGame / 3600000f;
				if (num > 10f)
				{
					num = 2.75f;
				}
				return 20f * Mathf.Pow(2f, num);
			}
		}

		[DebuggerHidden]
		public override IEnumerable<FiringIncident> MakeIntervalIncidents(IIncidentTarget target)
		{
			StorytellerComp_ShipChunkDrop.<MakeIntervalIncidents>c__IteratorAC <MakeIntervalIncidents>c__IteratorAC = new StorytellerComp_ShipChunkDrop.<MakeIntervalIncidents>c__IteratorAC();
			<MakeIntervalIncidents>c__IteratorAC.target = target;
			<MakeIntervalIncidents>c__IteratorAC.<$>target = target;
			<MakeIntervalIncidents>c__IteratorAC.<>f__this = this;
			StorytellerComp_ShipChunkDrop.<MakeIntervalIncidents>c__IteratorAC expr_1C = <MakeIntervalIncidents>c__IteratorAC;
			expr_1C.$PC = -2;
			return expr_1C;
		}
	}
}