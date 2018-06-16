﻿using System;
using System.Collections.Generic;
using RimWorld.Planet;
using Verse;

namespace RimWorld
{
	// Token: 0x020002CF RID: 719
	public class SiteCoreWorker : SiteWorkerBase
	{
		// Token: 0x170001C3 RID: 451
		// (get) Token: 0x06000BE2 RID: 3042 RVA: 0x00069E74 File Offset: 0x00068274
		public SiteCoreDef Def
		{
			get
			{
				return (SiteCoreDef)this.def;
			}
		}

		// Token: 0x06000BE3 RID: 3043 RVA: 0x00069E94 File Offset: 0x00068294
		public virtual void SiteCoreWorkerTick(Site site)
		{
		}

		// Token: 0x06000BE4 RID: 3044 RVA: 0x00069E97 File Offset: 0x00068297
		public virtual void VisitAction(Caravan caravan, Site site)
		{
			this.Enter(caravan, site);
		}

		// Token: 0x06000BE5 RID: 3045 RVA: 0x00069EA4 File Offset: 0x000682A4
		public IEnumerable<FloatMenuOption> GetFloatMenuOptions(Caravan caravan, Site site)
		{
			if (!site.HasMap)
			{
				foreach (FloatMenuOption f in CaravanArrivalAction_VisitSite.GetFloatMenuOptions(caravan, site))
				{
					yield return f;
				}
			}
			yield break;
		}

		// Token: 0x06000BE6 RID: 3046 RVA: 0x00069ED8 File Offset: 0x000682D8
		public virtual IEnumerable<FloatMenuOption> GetTransportPodsFloatMenuOptions(IEnumerable<IThingHolder> pods, CompLaunchable representative, Site site)
		{
			foreach (FloatMenuOption f in TransportPodsArrivalAction_VisitSite.GetFloatMenuOptions(representative, pods, site))
			{
				yield return f;
			}
			yield break;
		}

		// Token: 0x06000BE7 RID: 3047 RVA: 0x00069F10 File Offset: 0x00068310
		protected void Enter(Caravan caravan, Site site)
		{
			if (!site.HasMap)
			{
				LongEventHandler.QueueLongEvent(delegate()
				{
					this.DoEnter(caravan, site);
				}, "GeneratingMapForNewEncounter", false, null);
			}
			else
			{
				this.DoEnter(caravan, site);
			}
		}

		// Token: 0x06000BE8 RID: 3048 RVA: 0x00069F78 File Offset: 0x00068378
		private void DoEnter(Caravan caravan, Site site)
		{
			Pawn t = caravan.PawnsListForReading[0];
			bool flag = site.Faction == null || site.Faction.HostileTo(Faction.OfPlayer);
			bool flag2 = !site.HasMap;
			Map orGenerateMap = GetOrGenerateMapUtility.GetOrGenerateMap(site.Tile, SiteCoreWorker.MapSize, null);
			if (flag)
			{
				Find.TickManager.CurTimeSpeed = TimeSpeed.Paused;
			}
			Messages.Message("MessageCaravanArrivedAtDestination".Translate(new object[]
			{
				caravan.Label
			}).CapitalizeFirst(), t, MessageTypeDefOf.TaskCompletion, true);
			if (flag2)
			{
				PawnRelationUtility.Notify_PawnsSeenByPlayer_Letter_Send(orGenerateMap.mapPawns.AllPawns, "LetterRelatedPawnsSite".Translate(new object[]
				{
					Faction.OfPlayer.def.pawnsPlural
				}), LetterDefOf.NeutralEvent, true, true);
			}
			Map map = orGenerateMap;
			CaravanEnterMode enterMode = CaravanEnterMode.Edge;
			bool draftColonists = flag;
			CaravanEnterMapUtility.Enter(caravan, map, enterMode, CaravanDropInventoryMode.DoNotDrop, draftColonists, null);
		}

		// Token: 0x04000722 RID: 1826
		public static readonly IntVec3 MapSize = new IntVec3(120, 1, 120);
	}
}
