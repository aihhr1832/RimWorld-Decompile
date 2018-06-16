﻿using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	// Token: 0x0200014C RID: 332
	public class WorkGiver_HunterHunt : WorkGiver_Scanner
	{
		// Token: 0x060006D9 RID: 1753 RVA: 0x00046358 File Offset: 0x00044758
		public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
		{
			foreach (Designation des in pawn.Map.designationManager.SpawnedDesignationsOfDef(DesignationDefOf.Hunt))
			{
				yield return des.target.Thing;
			}
			yield break;
		}

		// Token: 0x17000109 RID: 265
		// (get) Token: 0x060006DA RID: 1754 RVA: 0x00046384 File Offset: 0x00044784
		public override PathEndMode PathEndMode
		{
			get
			{
				return PathEndMode.OnCell;
			}
		}

		// Token: 0x060006DB RID: 1755 RVA: 0x0004639C File Offset: 0x0004479C
		public override Danger MaxPathDanger(Pawn pawn)
		{
			return Danger.Deadly;
		}

		// Token: 0x060006DC RID: 1756 RVA: 0x000463B4 File Offset: 0x000447B4
		public override bool ShouldSkip(Pawn pawn, bool forced = false)
		{
			return !WorkGiver_HunterHunt.HasHuntingWeapon(pawn) || WorkGiver_HunterHunt.HasShieldAndRangedWeapon(pawn);
		}

		// Token: 0x060006DD RID: 1757 RVA: 0x000463F0 File Offset: 0x000447F0
		public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			Pawn pawn2 = t as Pawn;
			bool result;
			if (pawn2 == null || !pawn2.AnimalOrWildMan())
			{
				result = false;
			}
			else
			{
				LocalTargetInfo target = t;
				result = (pawn.CanReserve(target, 1, -1, null, forced) && pawn.Map.designationManager.DesignationOn(t, DesignationDefOf.Hunt) != null);
			}
			return result;
		}

		// Token: 0x060006DE RID: 1758 RVA: 0x0004646C File Offset: 0x0004486C
		public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			return new Job(JobDefOf.Hunt, t);
		}

		// Token: 0x060006DF RID: 1759 RVA: 0x00046494 File Offset: 0x00044894
		public static bool HasHuntingWeapon(Pawn p)
		{
			return p.equipment.Primary != null && p.equipment.Primary.def.IsRangedWeapon && p.equipment.PrimaryEq.PrimaryVerb.HarmsHealth() && !p.equipment.PrimaryEq.PrimaryVerb.UsesExplosiveProjectiles();
		}

		// Token: 0x060006E0 RID: 1760 RVA: 0x00046510 File Offset: 0x00044910
		public static bool HasShieldAndRangedWeapon(Pawn p)
		{
			if (p.equipment.Primary != null && p.equipment.Primary.def.IsWeaponUsingProjectiles)
			{
				List<Apparel> wornApparel = p.apparel.WornApparel;
				for (int i = 0; i < wornApparel.Count; i++)
				{
					if (wornApparel[i] is ShieldBelt)
					{
						return true;
					}
				}
			}
			return false;
		}
	}
}
