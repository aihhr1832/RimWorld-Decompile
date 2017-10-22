using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld
{
	public class SignalAction_Ambush : SignalAction
	{
		public float points;

		public bool manhunters;

		public bool mechanoids;

		public IntVec3 spawnNear = IntVec3.Invalid;

		public CellRect spawnAround;

		public bool spawnPawnsOnEdge;

		private const int PawnsDelayAfterSpawnTicks = 120;

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look<float>(ref this.points, "points", 0f, false);
			Scribe_Values.Look<bool>(ref this.manhunters, "manhunters", false, false);
			Scribe_Values.Look<bool>(ref this.mechanoids, "mechanoids", false, false);
			Scribe_Values.Look<IntVec3>(ref this.spawnNear, "spawnNear", default(IntVec3), false);
			Scribe_Values.Look<CellRect>(ref this.spawnAround, "spawnAround", default(CellRect), false);
			Scribe_Values.Look<bool>(ref this.spawnPawnsOnEdge, "spawnPawnsOnEdge", false, false);
		}

		protected override void DoAction(object[] args)
		{
			if (!(this.points <= 0.0))
			{
				List<Pawn> list = new List<Pawn>();
				foreach (Pawn item in this.GenerateAmbushPawns())
				{
					IntVec3 loc = default(IntVec3);
					if (this.spawnPawnsOnEdge)
					{
						if (!CellFinder.TryFindRandomEdgeCellWith((Predicate<IntVec3>)((IntVec3 x) => x.Standable(base.Map) && !x.Fogged(base.Map) && base.Map.reachability.CanReachColony(x)), base.Map, CellFinder.EdgeRoadChance_Ignore, out loc))
						{
							Find.WorldPawns.PassToWorld(item, PawnDiscardDecideMode.Discard);
							break;
						}
					}
					else if (!SiteGenStepUtility.TryFindSpawnCellAroundOrNear(this.spawnAround, this.spawnNear, base.Map, out loc))
					{
						Find.WorldPawns.PassToWorld(item, PawnDiscardDecideMode.Discard);
						break;
					}
					GenSpawn.Spawn(item, loc, base.Map);
					if (!this.spawnPawnsOnEdge)
					{
						for (int i = 0; i < 10; i++)
						{
							MoteMaker.ThrowAirPuffUp(item.DrawPos, base.Map);
						}
					}
					list.Add(item);
				}
				if (list.Any())
				{
					if (this.manhunters)
					{
						for (int j = 0; j < list.Count; j++)
						{
							list[j].mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.ManhunterPermanent, (string)null, false, false, null);
						}
					}
					else
					{
						Faction faction = list[0].Faction;
						LordMaker.MakeNewLord(faction, new LordJob_AssaultColony(faction, true, true, false, false, true), base.Map, list);
					}
					if (!this.spawnPawnsOnEdge)
					{
						for (int k = 0; k < list.Count; k++)
						{
							list[k].jobs.StartJob(new Job(JobDefOf.Wait, 120, false), JobCondition.None, null, false, true, null, default(JobTag?), false);
							list[k].Rotation = Rot4.Random;
						}
					}
					Find.LetterStack.ReceiveLetter("LetterLabelAmbushInExistingMap".Translate(), "LetterAmbushInExistingMap".Translate(Faction.OfPlayer.def.pawnsPlural).CapitalizeFirst(), LetterDefOf.ThreatBig, (Thing)list[0], (string)null);
				}
			}
		}

		private IEnumerable<Pawn> GenerateAmbushPawns()
		{
			IEnumerable<Pawn> result;
			if (this.manhunters)
			{
				PawnKindDef animalKind = default(PawnKindDef);
				result = ((ManhunterPackIncidentUtility.TryFindManhunterAnimalKind(this.points, base.Map.Tile, out animalKind) || ManhunterPackIncidentUtility.TryFindManhunterAnimalKind(this.points, -1, out animalKind)) ? ManhunterPackIncidentUtility.GenerateAnimals(animalKind, base.Map.Tile, this.points) : Enumerable.Empty<Pawn>());
			}
			else
			{
				Faction faction = (!this.mechanoids) ? (base.Map.ParentFaction ?? Find.FactionManager.RandomEnemyFaction(false, false, false, TechLevel.Undefined)) : Faction.OfMechanoids;
				if (faction == null)
				{
					result = Enumerable.Empty<Pawn>();
				}
				else
				{
					PawnGroupMakerParms pawnGroupMakerParms = new PawnGroupMakerParms();
					pawnGroupMakerParms.tile = base.Map.Tile;
					pawnGroupMakerParms.faction = faction;
					pawnGroupMakerParms.points = this.points;
					result = PawnGroupMakerUtility.GeneratePawns(PawnGroupKindDefOf.Normal, pawnGroupMakerParms, true);
				}
			}
			return result;
		}
	}
}