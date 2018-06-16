﻿using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;

namespace Verse
{
	// Token: 0x02000B17 RID: 2839
	public class DamageWorker
	{
		// Token: 0x06003EA8 RID: 16040 RVA: 0x0020F710 File Offset: 0x0020DB10
		public virtual DamageWorker.DamageResult Apply(DamageInfo dinfo, Thing victim)
		{
			DamageWorker.DamageResult damageResult = new DamageWorker.DamageResult();
			if (victim.SpawnedOrAnyParentSpawned)
			{
				ImpactSoundUtility.PlayImpactSound(victim, dinfo.Def.impactSoundType, victim.MapHeld);
			}
			if (victim.def.useHitPoints && dinfo.Def.harmsHealth)
			{
				damageResult.totalDamageDealt = Mathf.Min((float)victim.HitPoints, dinfo.Amount);
				victim.HitPoints -= (int)damageResult.totalDamageDealt;
				if (victim.HitPoints <= 0)
				{
					victim.HitPoints = 0;
					victim.Kill(new DamageInfo?(dinfo), null);
				}
			}
			return damageResult;
		}

		// Token: 0x06003EA9 RID: 16041 RVA: 0x0020F7C4 File Offset: 0x0020DBC4
		public virtual void ExplosionStart(Explosion explosion, List<IntVec3> cellsToAffect)
		{
			if (this.def.explosionHeatEnergyPerCell > 1.401298E-45f)
			{
				GenTemperature.PushHeat(explosion.Position, explosion.Map, this.def.explosionHeatEnergyPerCell * (float)cellsToAffect.Count);
			}
			MoteMaker.MakeStaticMote(explosion.Position, explosion.Map, ThingDefOf.Mote_ExplosionFlash, explosion.radius * 6f);
			if (explosion.Map == Find.CurrentMap)
			{
				float magnitude = (explosion.Position.ToVector3Shifted() - Find.Camera.transform.position).magnitude;
				Find.CameraDriver.shaker.DoShake(4f * explosion.radius / magnitude);
			}
			this.ExplosionVisualEffectCenter(explosion);
		}

		// Token: 0x06003EAA RID: 16042 RVA: 0x0020F894 File Offset: 0x0020DC94
		protected virtual void ExplosionVisualEffectCenter(Explosion explosion)
		{
			for (int i = 0; i < 4; i++)
			{
				MoteMaker.ThrowSmoke(explosion.Position.ToVector3Shifted() + Gen.RandomHorizontalVector(explosion.radius * 0.7f), explosion.Map, explosion.radius * 0.6f);
			}
			if (this.def.explosionInteriorMote != null)
			{
				int num = Mathf.RoundToInt(3.14159274f * explosion.radius * explosion.radius / 6f);
				for (int j = 0; j < num; j++)
				{
					MoteMaker.ThrowExplosionInteriorMote(explosion.Position.ToVector3Shifted() + Gen.RandomHorizontalVector(explosion.radius * 0.7f), explosion.Map, this.def.explosionInteriorMote);
				}
			}
		}

		// Token: 0x06003EAB RID: 16043 RVA: 0x0020F974 File Offset: 0x0020DD74
		public virtual void ExplosionAffectCell(Explosion explosion, IntVec3 c, List<Thing> damagedThings, bool canThrowMotes)
		{
			if (this.def.explosionCellMote != null && canThrowMotes)
			{
				float t = Mathf.Clamp01((explosion.Position - c).LengthHorizontal / explosion.radius);
				Color color = Color.Lerp(this.def.explosionColorCenter, this.def.explosionColorEdge, t);
				MoteMaker.ThrowExplosionCell(c, explosion.Map, this.def.explosionCellMote, color);
			}
			DamageWorker.thingsToAffect.Clear();
			float num = float.MinValue;
			bool flag = false;
			List<Thing> list = explosion.Map.thingGrid.ThingsListAt(c);
			for (int i = 0; i < list.Count; i++)
			{
				Thing thing = list[i];
				if (thing.def.category != ThingCategory.Mote && thing.def.category != ThingCategory.Ethereal)
				{
					DamageWorker.thingsToAffect.Add(thing);
					if (thing.def.Fillage == FillCategory.Full && thing.def.Altitude > num)
					{
						flag = true;
						num = thing.def.Altitude;
					}
				}
			}
			for (int j = 0; j < DamageWorker.thingsToAffect.Count; j++)
			{
				if (DamageWorker.thingsToAffect[j].def.Altitude >= num)
				{
					this.defaultDamageThing(explosion, DamageWorker.thingsToAffect[j], damagedThings, c);
				}
			}
			if (!flag)
			{
				this.defaultDamageTerrain(explosion, c);
			}
			if (this.def.explosionSnowMeltAmount > 0.0001f)
			{
				float lengthHorizontal = (c - explosion.Position).LengthHorizontal;
				float num2 = 1f - lengthHorizontal / explosion.radius;
				if (num2 > 0f)
				{
					explosion.Map.snowGrid.AddDepth(c, -num2 * this.def.explosionSnowMeltAmount);
				}
			}
			if (this.def == DamageDefOf.Bomb || this.def == DamageDefOf.Flame)
			{
				List<Thing> list2 = explosion.Map.listerThings.ThingsOfDef(ThingDefOf.RectTrigger);
				for (int k = 0; k < list2.Count; k++)
				{
					RectTrigger rectTrigger = (RectTrigger)list2[k];
					if (rectTrigger.activateOnExplosion && rectTrigger.Rect.Contains(c))
					{
						rectTrigger.ActivatedBy(null);
					}
				}
			}
		}

		// Token: 0x06003EAC RID: 16044 RVA: 0x0020FC0C File Offset: 0x0020E00C
		protected virtual void defaultDamageThing(Explosion explosion, Thing t, List<Thing> damagedThings, IntVec3 cell)
		{
			if (t.def.category != ThingCategory.Mote && t.def.category != ThingCategory.Ethereal)
			{
				if (!damagedThings.Contains(t))
				{
					damagedThings.Add(t);
					if (this.def == DamageDefOf.Bomb && t.def == ThingDefOf.Fire && !t.Destroyed)
					{
						t.Destroy(DestroyMode.Vanish);
					}
					else
					{
						float num;
						if (t.Position == explosion.Position)
						{
							num = (float)Rand.RangeInclusive(0, 359);
						}
						else
						{
							num = (t.Position - explosion.Position).AngleFlat;
						}
						DamageDef damageDef = this.def;
						float amount = (float)explosion.GetDamageAmountAt(cell);
						float angle = num;
						Thing instigator = explosion.instigator;
						ThingDef weapon = explosion.weapon;
						DamageInfo dinfo = new DamageInfo(damageDef, amount, angle, instigator, null, weapon, DamageInfo.SourceCategory.ThingOrUnknown, explosion.intendedTarget);
						if (this.def.explosionAffectOutsidePartsOnly)
						{
							dinfo.SetBodyRegion(BodyPartHeight.Undefined, BodyPartDepth.Outside);
						}
						if (t.def.category == ThingCategory.Building)
						{
							int num2 = Mathf.RoundToInt(dinfo.Amount * this.def.explosionBuildingDamageFactor);
							dinfo.SetAmount((float)num2);
						}
						else if (t.def.category == ThingCategory.Plant)
						{
							int num3 = Mathf.RoundToInt(dinfo.Amount * this.def.explosionPlantDamageFactor);
							dinfo.SetAmount((float)num3);
						}
						BattleLogEntry_ExplosionImpact battleLogEntry_ExplosionImpact = null;
						Pawn pawn = t as Pawn;
						if (pawn != null)
						{
							battleLogEntry_ExplosionImpact = new BattleLogEntry_ExplosionImpact(explosion.instigator, t, explosion.weapon, explosion.projectile, this.def);
							Find.BattleLog.Add(battleLogEntry_ExplosionImpact);
						}
						DamageWorker.DamageResult damageResult = t.TakeDamage(dinfo);
						damageResult.AssociateWithLog(battleLogEntry_ExplosionImpact);
						if (pawn != null && damageResult.wounded && pawn.stances != null)
						{
							pawn.stances.StaggerFor(95);
						}
					}
				}
			}
		}

		// Token: 0x06003EAD RID: 16045 RVA: 0x0020FE20 File Offset: 0x0020E220
		protected virtual void defaultDamageTerrain(Explosion explosion, IntVec3 c)
		{
			if (this.def == DamageDefOf.Bomb)
			{
				if (explosion.Map.terrainGrid.CanRemoveTopLayerAt(c))
				{
					TerrainDef terrain = c.GetTerrain(explosion.Map);
					if (terrain.destroyOnBombDamageThreshold >= 0f)
					{
						float num = (float)explosion.GetDamageAmountAt(c);
						if (num >= terrain.destroyOnBombDamageThreshold)
						{
							explosion.Map.terrainGrid.Notify_TerrainDestroyed(c);
						}
					}
				}
			}
		}

		// Token: 0x06003EAE RID: 16046 RVA: 0x0020FEA8 File Offset: 0x0020E2A8
		public IEnumerable<IntVec3> ExplosionCellsToHit(Explosion explosion)
		{
			return this.ExplosionCellsToHit(explosion.Position, explosion.Map, explosion.radius);
		}

		// Token: 0x06003EAF RID: 16047 RVA: 0x0020FED8 File Offset: 0x0020E2D8
		public virtual IEnumerable<IntVec3> ExplosionCellsToHit(IntVec3 center, Map map, float radius)
		{
			DamageWorker.openCells.Clear();
			DamageWorker.adjWallCells.Clear();
			int num = GenRadial.NumCellsInRadius(radius);
			for (int i = 0; i < num; i++)
			{
				IntVec3 intVec = center + GenRadial.RadialPattern[i];
				if (intVec.InBounds(map))
				{
					if (GenSight.LineOfSight(center, intVec, map, true, null, 0, 0))
					{
						DamageWorker.openCells.Add(intVec);
					}
				}
			}
			for (int j = 0; j < DamageWorker.openCells.Count; j++)
			{
				IntVec3 intVec2 = DamageWorker.openCells[j];
				if (intVec2.Walkable(map))
				{
					for (int k = 0; k < 4; k++)
					{
						IntVec3 intVec3 = intVec2 + GenAdj.CardinalDirections[k];
						if (intVec3.InHorDistOf(center, radius))
						{
							if (intVec3.InBounds(map))
							{
								if (!intVec3.Standable(map))
								{
									if (intVec3.GetEdifice(map) != null)
									{
										if (!DamageWorker.openCells.Contains(intVec3) && DamageWorker.adjWallCells.Contains(intVec3))
										{
											DamageWorker.adjWallCells.Add(intVec3);
										}
									}
								}
							}
						}
					}
				}
			}
			return DamageWorker.openCells.Concat(DamageWorker.adjWallCells);
		}

		// Token: 0x0400280E RID: 10254
		public DamageDef def;

		// Token: 0x0400280F RID: 10255
		private const float ExplosionCamShakeMultiplier = 4f;

		// Token: 0x04002810 RID: 10256
		private static List<Thing> thingsToAffect = new List<Thing>();

		// Token: 0x04002811 RID: 10257
		private static List<IntVec3> openCells = new List<IntVec3>();

		// Token: 0x04002812 RID: 10258
		private static List<IntVec3> adjWallCells = new List<IntVec3>();

		// Token: 0x02000B18 RID: 2840
		public class DamageResult
		{
			// Token: 0x17000972 RID: 2418
			// (get) Token: 0x06003EB2 RID: 16050 RVA: 0x002100B8 File Offset: 0x0020E4B8
			public BodyPartRecord LastHitPart
			{
				get
				{
					BodyPartRecord result;
					if (this.parts == null)
					{
						result = null;
					}
					else if (this.parts.Count <= 0)
					{
						result = null;
					}
					else
					{
						result = this.parts[this.parts.Count - 1];
					}
					return result;
				}
			}

			// Token: 0x06003EB3 RID: 16051 RVA: 0x00210110 File Offset: 0x0020E510
			public void AddPart(Thing hitThing, BodyPartRecord part)
			{
				if (this.hitThing != null && this.hitThing != hitThing)
				{
					Log.ErrorOnce("Single damage worker referring to multiple things; will cause issues with combat log", 30667935, false);
				}
				this.hitThing = hitThing;
				if (this.parts == null)
				{
					this.parts = new List<BodyPartRecord>();
				}
				this.parts.Add(part);
			}

			// Token: 0x06003EB4 RID: 16052 RVA: 0x0021016E File Offset: 0x0020E56E
			public void AddHediff(Hediff hediff)
			{
				if (this.hediffs == null)
				{
					this.hediffs = new List<Hediff>();
				}
				this.hediffs.Add(hediff);
			}

			// Token: 0x06003EB5 RID: 16053 RVA: 0x00210194 File Offset: 0x0020E594
			public void AssociateWithLog(LogEntry_DamageResult log)
			{
				if (log != null)
				{
					Pawn hitPawn = this.hitThing as Pawn;
					if (hitPawn != null)
					{
						List<BodyPartRecord> list = null;
						List<bool> recipientPartsDestroyed = null;
						if (!this.parts.NullOrEmpty<BodyPartRecord>() && hitPawn != null)
						{
							list = this.parts.Distinct<BodyPartRecord>().ToList<BodyPartRecord>();
							recipientPartsDestroyed = (from part in list
							select hitPawn.health.hediffSet.GetPartHealth(part) <= 0f).ToList<bool>();
						}
						log.FillTargets(list, recipientPartsDestroyed, this.deflected);
					}
					if (this.hediffs != null)
					{
						for (int i = 0; i < this.hediffs.Count; i++)
						{
							this.hediffs[i].combatLogEntry = new WeakReference<LogEntry>(log);
							this.hediffs[i].combatLogText = log.ToGameStringFromPOV(null, false);
						}
					}
				}
			}

			// Token: 0x04002813 RID: 10259
			public bool wounded = false;

			// Token: 0x04002814 RID: 10260
			public bool headshot = false;

			// Token: 0x04002815 RID: 10261
			public bool deflected = false;

			// Token: 0x04002816 RID: 10262
			public bool deflectedByMetalArmor;

			// Token: 0x04002817 RID: 10263
			public Thing hitThing = null;

			// Token: 0x04002818 RID: 10264
			public List<BodyPartRecord> parts = null;

			// Token: 0x04002819 RID: 10265
			public List<Hediff> hediffs = null;

			// Token: 0x0400281A RID: 10266
			public float totalDamageDealt = 0f;
		}
	}
}
