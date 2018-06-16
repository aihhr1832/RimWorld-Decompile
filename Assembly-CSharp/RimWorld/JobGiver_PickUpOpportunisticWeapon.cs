﻿using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	// Token: 0x020000C8 RID: 200
	public class JobGiver_PickUpOpportunisticWeapon : ThinkNode_JobGiver
	{
		// Token: 0x170000CB RID: 203
		// (get) Token: 0x06000497 RID: 1175 RVA: 0x00034384 File Offset: 0x00032784
		private float MinMeleeWeaponDPSThreshold
		{
			get
			{
				List<Tool> tools = ThingDefOf.Human.tools;
				float num = 0f;
				for (int i = 0; i < tools.Count; i++)
				{
					if (tools[i].linkedBodyPartsGroup == BodyPartGroupDefOf.LeftHand || tools[i].linkedBodyPartsGroup == BodyPartGroupDefOf.RightHand)
					{
						num = tools[i].power / tools[i].cooldownTime;
						break;
					}
				}
				return num + 2f;
			}
		}

		// Token: 0x06000498 RID: 1176 RVA: 0x00034418 File Offset: 0x00032818
		public override ThinkNode DeepCopy(bool resolve = true)
		{
			JobGiver_PickUpOpportunisticWeapon jobGiver_PickUpOpportunisticWeapon = (JobGiver_PickUpOpportunisticWeapon)base.DeepCopy(resolve);
			jobGiver_PickUpOpportunisticWeapon.preferBuildingDestroyers = this.preferBuildingDestroyers;
			return jobGiver_PickUpOpportunisticWeapon;
		}

		// Token: 0x06000499 RID: 1177 RVA: 0x00034448 File Offset: 0x00032848
		protected override Job TryGiveJob(Pawn pawn)
		{
			Job result;
			if (pawn.equipment == null)
			{
				result = null;
			}
			else if (this.AlreadySatisfiedWithCurrentWeapon(pawn))
			{
				result = null;
			}
			else if (pawn.RaceProps.Humanlike && pawn.story.WorkTagIsDisabled(WorkTags.Violent))
			{
				result = null;
			}
			else if (!pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
			{
				result = null;
			}
			else if (pawn.GetRegion(RegionType.Set_Passable) == null)
			{
				result = null;
			}
			else
			{
				Thing thing = GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForGroup(ThingRequestGroup.Weapon), PathEndMode.OnCell, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false), 8f, (Thing x) => pawn.CanReserve(x, 1, -1, null, false) && this.ShouldEquip(x, pawn), null, 0, 15, false, RegionType.Set_Passable, false);
				if (thing != null)
				{
					result = new Job(JobDefOf.Equip, thing);
				}
				else
				{
					result = null;
				}
			}
			return result;
		}

		// Token: 0x0600049A RID: 1178 RVA: 0x0003457C File Offset: 0x0003297C
		private bool AlreadySatisfiedWithCurrentWeapon(Pawn pawn)
		{
			ThingWithComps primary = pawn.equipment.Primary;
			bool result;
			if (primary == null)
			{
				result = false;
			}
			else
			{
				if (this.preferBuildingDestroyers)
				{
					if (!pawn.equipment.PrimaryEq.PrimaryVerb.verbProps.ai_IsBuildingDestroyer)
					{
						return false;
					}
				}
				else if (!primary.def.IsRangedWeapon)
				{
					return false;
				}
				result = true;
			}
			return result;
		}

		// Token: 0x0600049B RID: 1179 RVA: 0x000345FC File Offset: 0x000329FC
		private bool ShouldEquip(Thing newWep, Pawn pawn)
		{
			return this.GetWeaponScore(newWep) > this.GetWeaponScore(pawn.equipment.Primary);
		}

		// Token: 0x0600049C RID: 1180 RVA: 0x0003462C File Offset: 0x00032A2C
		private int GetWeaponScore(Thing wep)
		{
			int result;
			if (wep == null)
			{
				result = 0;
			}
			else if (wep.def.IsMeleeWeapon && wep.GetStatValue(StatDefOf.MeleeWeapon_AverageDPS, true) < this.MinMeleeWeaponDPSThreshold)
			{
				result = 0;
			}
			else if (this.preferBuildingDestroyers && wep.TryGetComp<CompEquippable>().PrimaryVerb.verbProps.ai_IsBuildingDestroyer)
			{
				result = 3;
			}
			else if (wep.def.IsRangedWeapon)
			{
				result = 2;
			}
			else
			{
				result = 1;
			}
			return result;
		}

		// Token: 0x0400029B RID: 667
		private bool preferBuildingDestroyers;
	}
}
