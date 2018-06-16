﻿using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	// Token: 0x02000349 RID: 841
	public abstract class IncidentWorker_NeutralGroup : IncidentWorker_PawnsArrive
	{
		// Token: 0x1700020E RID: 526
		// (get) Token: 0x06000E7F RID: 3711 RVA: 0x0007ACD8 File Offset: 0x000790D8
		protected virtual PawnGroupKindDef PawnGroupKindDef
		{
			get
			{
				return PawnGroupKindDefOf.Peaceful;
			}
		}

		// Token: 0x06000E80 RID: 3712 RVA: 0x0007ACF4 File Offset: 0x000790F4
		protected override bool FactionCanBeGroupSource(Faction f, Map map, bool desperate = false)
		{
			return base.FactionCanBeGroupSource(f, map, desperate) && !f.def.hidden && !f.HostileTo(Faction.OfPlayer);
		}

		// Token: 0x06000E81 RID: 3713 RVA: 0x0007AD38 File Offset: 0x00079138
		protected bool TryResolveParms(IncidentParms parms)
		{
			bool result;
			if (!this.TryResolveParmsGeneral(parms))
			{
				result = false;
			}
			else
			{
				this.ResolveParmsPoints(parms);
				result = true;
			}
			return result;
		}

		// Token: 0x06000E82 RID: 3714 RVA: 0x0007AD68 File Offset: 0x00079168
		protected virtual bool TryResolveParmsGeneral(IncidentParms parms)
		{
			Map map = (Map)parms.target;
			if (!parms.spawnCenter.IsValid)
			{
				if (!RCellFinder.TryFindRandomPawnEntryCell(out parms.spawnCenter, map, CellFinder.EdgeRoadChance_Neutral, null))
				{
					return false;
				}
			}
			if (parms.faction == null)
			{
				if (!base.CandidateFactions(map, false).TryRandomElement(out parms.faction))
				{
					if (!base.CandidateFactions(map, true).TryRandomElement(out parms.faction))
					{
						return false;
					}
				}
			}
			return true;
		}

		// Token: 0x06000E83 RID: 3715
		protected abstract void ResolveParmsPoints(IncidentParms parms);

		// Token: 0x06000E84 RID: 3716 RVA: 0x0007AE00 File Offset: 0x00079200
		protected List<Pawn> SpawnPawns(IncidentParms parms)
		{
			Map map = (Map)parms.target;
			PawnGroupMakerParms defaultPawnGroupMakerParms = IncidentParmsUtility.GetDefaultPawnGroupMakerParms(this.PawnGroupKindDef, parms, false);
			List<Pawn> list = PawnGroupMakerUtility.GeneratePawns(defaultPawnGroupMakerParms, false).ToList<Pawn>();
			foreach (Pawn newThing in list)
			{
				IntVec3 loc = CellFinder.RandomClosewalkCellNear(parms.spawnCenter, map, 5, null);
				GenSpawn.Spawn(newThing, loc, map, WipeMode.Vanish);
			}
			return list;
		}
	}
}
