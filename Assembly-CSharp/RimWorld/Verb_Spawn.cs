﻿using System;
using Verse;

namespace RimWorld
{
	public class Verb_Spawn : Verb
	{
		public Verb_Spawn()
		{
		}

		protected override bool TryCastShot()
		{
			bool result;
			if (this.currentTarget.HasThing && this.currentTarget.Thing.Map != this.caster.Map)
			{
				result = false;
			}
			else
			{
				GenSpawn.Spawn(this.verbProps.spawnDef, this.currentTarget.Cell, this.caster.Map, WipeMode.Vanish);
				if (this.verbProps.colonyWideTaleDef != null)
				{
					Pawn pawn = this.caster.Map.mapPawns.FreeColonistsSpawned.RandomElementWithFallback(null);
					TaleRecorder.RecordTale(this.verbProps.colonyWideTaleDef, new object[]
					{
						this.caster,
						pawn
					});
				}
				if (this.ownerEquipment != null && !this.ownerEquipment.Destroyed)
				{
					this.ownerEquipment.Destroy(DestroyMode.Vanish);
				}
				result = true;
			}
			return result;
		}
	}
}
