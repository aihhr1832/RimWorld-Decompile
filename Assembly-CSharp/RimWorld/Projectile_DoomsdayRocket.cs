﻿using System;
using Verse;

namespace RimWorld
{
	public class Projectile_DoomsdayRocket : Projectile
	{
		public Projectile_DoomsdayRocket()
		{
		}

		protected override void Impact(Thing hitThing)
		{
			Map map = base.Map;
			base.Impact(hitThing);
			IntVec3 position = base.Position;
			Map map2 = map;
			float explosionRadius = this.def.projectile.explosionRadius;
			DamageDef bomb = DamageDefOf.Bomb;
			Thing launcher = this.launcher;
			int damageAmount = this.def.projectile.DamageAmount;
			float armorPenetration = this.def.projectile.ArmorPenetration;
			ThingDef equipmentDef = this.equipmentDef;
			GenExplosion.DoExplosion(position, map2, explosionRadius, bomb, launcher, damageAmount, armorPenetration, null, equipmentDef, this.def, this.intendedTarget.Thing, null, 0f, 1, false, null, 0f, 1, 0f, false);
			CellRect cellRect = CellRect.CenteredOn(base.Position, 10);
			cellRect.ClipInsideMap(map);
			for (int i = 0; i < 5; i++)
			{
				IntVec3 randomCell = cellRect.RandomCell;
				this.FireExplosion(randomCell, map, 3.9f);
			}
		}

		protected void FireExplosion(IntVec3 pos, Map map, float radius)
		{
			DamageDef flame = DamageDefOf.Flame;
			Thing launcher = this.launcher;
			int damageAmount = this.def.projectile.DamageAmount;
			float armorPenetration = this.def.projectile.ArmorPenetration;
			ThingDef filth_Fuel = ThingDefOf.Filth_Fuel;
			GenExplosion.DoExplosion(pos, map, radius, flame, launcher, damageAmount, armorPenetration, null, this.equipmentDef, this.def, this.intendedTarget.Thing, filth_Fuel, 0.2f, 1, false, null, 0f, 1, 0f, false);
		}
	}
}
