using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class IncidentWorker_QuestBanditCamp : IncidentWorker
	{
		private const float RelationsImprovement = 8f;

		private static readonly IntRange RewardMarketValueRange = new IntRange(2000, 3000);

		protected override bool CanFireNowSub(IIncidentTarget target)
		{
			Faction faction = default(Faction);
			Faction faction2 = default(Faction);
			int num = default(int);
			return base.CanFireNowSub(target) && this.TryFindFactions(out faction, out faction2) && TileFinder.TryFindNewSiteTile(out num);
		}

		public override bool TryExecute(IncidentParms parms)
		{
			Faction faction = default(Faction);
			Faction faction2 = default(Faction);
			if (!this.TryFindFactions(out faction, out faction2))
			{
				return false;
			}
			int tile = default(int);
			if (!TileFinder.TryFindNewSiteTile(out tile))
			{
				return false;
			}
			Site site = (Site)WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.Site);
			site.Tile = tile;
			site.SetFaction(faction2);
			site.core = SiteCoreDefOf.Nothing;
			site.parts.Add(SitePartDefOf.Outpost);
			List<Thing> list = this.GenerateRewards(faction);
			((WorldObject)site).GetComponent<DefeatAllEnemiesQuestComp>().StartQuest(faction, 8f, list);
			Find.WorldObjects.Add(site);
			base.SendStandardLetter((WorldObject)site, faction.leader.LabelShort, faction.def.leaderTitle, faction.Name, list[0].LabelCap);
			return true;
		}

		private List<Thing> GenerateRewards(Faction alliedFaction)
		{
			ItemCollectionGeneratorParams parms = new ItemCollectionGeneratorParams
			{
				count = 1,
				totalMarketValue = (float)IncidentWorker_QuestBanditCamp.RewardMarketValueRange.RandomInRange,
				techLevel = alliedFaction.def.techLevel
			};
			return ItemCollectionGeneratorDefOf.BanditCampQuestRewards.Worker.Generate(parms);
		}

		private bool TryFindFactions(out Faction alliedFaction, out Faction enemyFaction)
		{
			if ((from x in Find.FactionManager.AllFactions
			where !x.def.hidden && !x.defeated && !x.IsPlayer && !x.HostileTo(Faction.OfPlayer) && this.CommonHumanlikeEnemyFactionExists(Faction.OfPlayer, x) && !this.AnyQuestExistsFrom(x)
			select x).TryRandomElement<Faction>(out alliedFaction))
			{
				enemyFaction = this.CommonHumanlikeEnemyFaction(Faction.OfPlayer, alliedFaction);
				return true;
			}
			alliedFaction = null;
			enemyFaction = null;
			return false;
		}

		private bool AnyQuestExistsFrom(Faction faction)
		{
			List<Site> sites = Find.WorldObjects.Sites;
			for (int i = 0; i < sites.Count; i++)
			{
				DefeatAllEnemiesQuestComp component = ((WorldObject)sites[i]).GetComponent<DefeatAllEnemiesQuestComp>();
				if (component != null && component.Active && component.requestingFaction == faction)
				{
					return true;
				}
			}
			return false;
		}

		private bool CommonHumanlikeEnemyFactionExists(Faction f1, Faction f2)
		{
			return this.CommonHumanlikeEnemyFaction(f1, f2) != null;
		}

		private Faction CommonHumanlikeEnemyFaction(Faction f1, Faction f2)
		{
			Faction result = default(Faction);
			if ((from x in Find.FactionManager.AllFactions
			where x != f1 && x != f2 && !x.def.hidden && x.def.humanlikeFaction && !x.defeated && x.HostileTo(f1) && x.HostileTo(f2)
			select x).TryRandomElement<Faction>(out result))
			{
				return result;
			}
			return null;
		}
	}
}
