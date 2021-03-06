﻿using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class IncidentWorker_RefugeePodCrash : IncidentWorker
	{
		public IncidentWorker_RefugeePodCrash()
		{
		}

		protected override bool TryExecuteWorker(IncidentParms parms)
		{
			Map map = (Map)parms.target;
			List<Thing> things = ThingSetMakerDefOf.RefugeePod.root.Generate();
			IntVec3 intVec = DropCellFinder.RandomDropSpot(map);
			Pawn pawn = this.FindPawn(things);
			pawn.guest.getRescuedThoughtOnUndownedBecauseOfPlayer = true;
			string label = "LetterLabelRefugeePodCrash".Translate();
			string text = "RefugeePodCrash".Translate().AdjustedFor(pawn, "PAWN");
			if (pawn.Faction == null || !pawn.Faction.HostileTo(Faction.OfPlayer))
			{
				text = text + "\n\n" + "RefugeePodCrash_NonHostileOrFactionless".Translate(new object[]
				{
					pawn
				}).AdjustedFor(pawn, "PAWN");
			}
			if (pawn.Faction != null && pawn.Faction.HostileTo(Faction.OfPlayer))
			{
				text = text + "\n\n" + "RefugeePodCrash_Hostile".Translate(new object[]
				{
					pawn
				}).AdjustedFor(pawn, "PAWN");
			}
			if (pawn.Faction != null && !pawn.Faction.HostileTo(Faction.OfPlayer))
			{
				text = text + "\n\n" + "RefugeePodCrash_NonHostile".Translate(new object[]
				{
					pawn
				}).AdjustedFor(pawn, "PAWN");
			}
			PawnRelationUtility.TryAppendRelationsWithColonistsInfo(ref text, ref label, pawn);
			Find.LetterStack.ReceiveLetter(label, text, LetterDefOf.NeutralEvent, new TargetInfo(intVec, map, false), null, null);
			ActiveDropPodInfo activeDropPodInfo = new ActiveDropPodInfo();
			activeDropPodInfo.innerContainer.TryAddRangeOrTransfer(things, true, false);
			activeDropPodInfo.openDelay = 180;
			activeDropPodInfo.leaveSlag = true;
			DropPodUtility.MakeDropPodAt(intVec, map, activeDropPodInfo, true);
			return true;
		}

		private Pawn FindPawn(List<Thing> things)
		{
			int i = 0;
			while (i < things.Count)
			{
				Pawn pawn = things[i] as Pawn;
				Pawn result;
				if (pawn != null)
				{
					result = pawn;
				}
				else
				{
					Corpse corpse = things[i] as Corpse;
					if (corpse == null)
					{
						i++;
						continue;
					}
					result = corpse.InnerPawn;
				}
				return result;
			}
			return null;
		}
	}
}
