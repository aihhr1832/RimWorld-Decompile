using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public static class StorytellerUtility
	{
		private const float WealthBase = 2000f;

		private const float PointsPer1000Wealth = 10f;

		private const float PointsPerColonist = 42f;

		private const float MinMaxSquadCost = 50f;

		private const float BuildingWealthFactor = 0.5f;

		private const float HalveLimitLo = 1000f;

		private const float HalveLimitHi = 2000f;

		private static Dictionary<IIncidentTarget, StoryState> tmpOldStoryStates = new Dictionary<IIncidentTarget, StoryState>();

		public static IncidentParms DefaultParmsNow(StorytellerDef tellerDef, IncidentCategory incCat, IIncidentTarget target)
		{
			IncidentParms incidentParms = new IncidentParms();
			incidentParms.target = target;
			switch (incCat)
			{
			case IncidentCategory.ThreatSmall:
			case IncidentCategory.ThreatBig:
			{
				Map map = target as Map;
				float num = 0f;
				if (map != null)
				{
					num = (float)(map.wealthWatcher.WealthItems + map.wealthWatcher.WealthBuildings * 0.5);
				}
				num = (float)(num - 2000.0);
				if (num < 0.0)
				{
					num = 0f;
				}
				float num2 = (float)(num / 1000.0 * 10.0);
				float num3 = 0f;
				if (map != null)
				{
					num3 = (float)((float)map.mapPawns.FreeColonistsCount * 42.0);
				}
				else
				{
					Caravan caravan = target as Caravan;
					if (caravan != null)
					{
						num3 = (float)((float)caravan.PawnsListForReading.Count((Func<Pawn, bool>)((Pawn x) => x.IsColonist && x.HostFaction == null)) * 42.0);
					}
				}
				incidentParms.points = num2 + num3;
				incidentParms.points *= Find.StoryWatcher.watcherRampUp.TotalThreatPointsFactor;
				incidentParms.points *= Find.Storyteller.difficulty.threatScale;
				switch (Find.StoryWatcher.statsRecord.numThreatBigs)
				{
				case 0:
				{
					incidentParms.points = 35f;
					incidentParms.raidForceOneIncap = true;
					incidentParms.raidNeverFleeIndividual = true;
					break;
				}
				case 1:
				{
					incidentParms.points *= 0.5f;
					break;
				}
				case 2:
				{
					incidentParms.points *= 0.7f;
					break;
				}
				case 3:
				{
					incidentParms.points *= 0.8f;
					break;
				}
				case 4:
				{
					incidentParms.points *= 0.9f;
					break;
				}
				default:
				{
					incidentParms.points *= 1f;
					break;
				}
				}
				if (incidentParms.points < 0.0)
				{
					incidentParms.points = 0f;
				}
				if (incidentParms.points > 1000.0)
				{
					if (incidentParms.points > 2000.0)
					{
						incidentParms.points = (float)(2000.0 + (incidentParms.points - 2000.0) * 0.5);
					}
					incidentParms.points = (float)(1000.0 + (incidentParms.points - 1000.0) * 0.5);
				}
				break;
			}
			case IncidentCategory.CaravanTarget:
			{
				Caravan caravan2 = incidentParms.target as Caravan;
				IEnumerable<Pawn> playerPawns;
				if (caravan2 != null)
				{
					playerPawns = caravan2.PawnsListForReading;
				}
				else
				{
					Faction playerFaction = Faction.OfPlayer;
					playerPawns = from x in ((Map)incidentParms.target).mapPawns.AllPawnsSpawned
					where x.Faction == playerFaction || x.HostFaction == playerFaction
					select x;
				}
				incidentParms.points = CaravanIncidentUtility.CalculateIncidentPoints(playerPawns);
				break;
			}
			}
			return incidentParms;
		}

		public static float AllyIncidentMTBMultiplier()
		{
			List<Faction> allFactionsListForReading = Find.FactionManager.AllFactionsListForReading;
			int num = 0;
			int num2 = 0;
			for (int i = 0; i < allFactionsListForReading.Count; i++)
			{
				if (!allFactionsListForReading[i].def.hidden && !allFactionsListForReading[i].IsPlayer)
				{
					if (allFactionsListForReading[i].def.CanEverBeNonHostile)
					{
						num2++;
					}
					if (!allFactionsListForReading[i].HostileTo(Faction.OfPlayer))
					{
						num++;
					}
				}
			}
			if (num == 0)
			{
				return -1f;
			}
			float num3 = (float)num / Mathf.Max((float)num2, 1f);
			return (float)(1.0 / num3);
		}

		public static void DebugLogTestFutureIncidents(bool visibleMapOnly)
		{
			int ticksGame = Find.TickManager.TicksGame;
			IncidentQueue incidentQueue = Find.Storyteller.incidentQueue;
			List<IIncidentTarget> allIncidentTargets = Find.Storyteller.AllIncidentTargets;
			StorytellerUtility.tmpOldStoryStates.Clear();
			for (int i = 0; i < allIncidentTargets.Count; i++)
			{
				IIncidentTarget incidentTarget = allIncidentTargets[i];
				StorytellerUtility.tmpOldStoryStates.Add(incidentTarget, incidentTarget.StoryState);
				new StoryState(incidentTarget).CopyTo(incidentTarget.StoryState);
			}
			Find.Storyteller.incidentQueue = new IncidentQueue();
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("Test future incidents for " + Find.Storyteller.def + ":");
			int[] array = new int[Find.Storyteller.storytellerComps.Count];
			int num = 0;
			for (int j = 0; j < 6000; j++)
			{
				foreach (FiringIncident item in Find.Storyteller.MakeIncidentsForInterval())
				{
					if (!visibleMapOnly || item.parms.target == Find.VisibleMap)
					{
						string text = "  ";
						if (item.def.category == IncidentCategory.ThreatBig)
						{
							num++;
							text = "T";
						}
						int num2 = Find.Storyteller.storytellerComps.IndexOf(item.source);
						array[num2]++;
						stringBuilder.AppendLine("M" + num2 + " " + text + " " + Find.TickManager.TicksGame.TicksToDays().ToString("F1") + "d      " + item);
						item.parms.target.StoryState.Notify_IncidentFired(item);
					}
				}
				Find.TickManager.DebugSetTicksGame(Find.TickManager.TicksGame + 1000);
			}
			stringBuilder.AppendLine();
			stringBuilder.AppendLine("Incident totals:");
			for (int k = 0; k < array.Length; k++)
			{
				float f = (float)array[k] / (float)array.Sum();
				float num3 = (float)((float)array[k] / 100.0);
				float num4 = (float)(1.0 / num3);
				stringBuilder.AppendLine("   M" + k + ": " + array[k] + "  (" + f.ToStringPercent("F2") + " of total, avg " + num3.ToString("F2") + " per day, avg interval " + num4 + ")");
			}
			stringBuilder.AppendLine("Total threats: " + num);
			stringBuilder.AppendLine("Total threats avg per day: " + ((float)((float)num / 100.0)).ToString("F2"));
			stringBuilder.AppendLine("Overall: " + array.Sum());
			stringBuilder.AppendLine("Overall avg per day: " + ((float)((float)array.Sum() / 100.0)).ToString("F2"));
			Log.Message(stringBuilder.ToString());
			Find.TickManager.DebugSetTicksGame(ticksGame);
			Find.Storyteller.incidentQueue = incidentQueue;
			for (int l = 0; l < allIncidentTargets.Count; l++)
			{
				StorytellerUtility.tmpOldStoryStates[allIncidentTargets[l]].CopyTo(allIncidentTargets[l].StoryState);
			}
			StorytellerUtility.tmpOldStoryStates.Clear();
		}
	}
}
