using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	internal class MedicalRecipesUtility
	{
		public static bool IsCleanAndDroppable(Pawn pawn, BodyPartRecord part)
		{
			if (pawn.Dead)
			{
				return false;
			}
			if (pawn.RaceProps.Animal)
			{
				return false;
			}
			return part.def.spawnThingOnRemoved != null && MedicalRecipesUtility.IsClean(pawn, part);
		}

		public static bool IsClean(Pawn pawn, BodyPartRecord part)
		{
			if (pawn.Dead)
			{
				return false;
			}
			return !(from x in pawn.health.hediffSet.hediffs
			where x.Part == part
			select x).Any();
		}

		public static void RestorePartAndSpawnAllPreviousParts(Pawn pawn, BodyPartRecord part, IntVec3 pos, Map map)
		{
			MedicalRecipesUtility.SpawnNaturalPartIfClean(pawn, part, pos, map);
			MedicalRecipesUtility.SpawnThingsFromHediffs(pawn, part, pos, map);
			pawn.health.RestorePart(part, null, true);
		}

		public static Thing SpawnNaturalPartIfClean(Pawn pawn, BodyPartRecord part, IntVec3 pos, Map map)
		{
			if (MedicalRecipesUtility.IsCleanAndDroppable(pawn, part))
			{
				return GenSpawn.Spawn(part.def.spawnThingOnRemoved, pos, map);
			}
			return null;
		}

		public static void SpawnThingsFromHediffs(Pawn pawn, BodyPartRecord part, IntVec3 pos, Map map)
		{
			if (pawn.health.hediffSet.GetNotMissingParts(BodyPartHeight.Undefined, BodyPartDepth.Undefined).Contains(part))
			{
				IEnumerable<Hediff> enumerable = from x in pawn.health.hediffSet.hediffs
				where x.Part == part
				select x;
				foreach (Hediff item in enumerable)
				{
					if (item.def.spawnThingOnRemoved != null)
					{
						GenSpawn.Spawn(item.def.spawnThingOnRemoved, pos, map);
					}
				}
				for (int i = 0; i < part.parts.Count; i++)
				{
					MedicalRecipesUtility.SpawnThingsFromHediffs(pawn, part.parts[i], pos, map);
				}
			}
		}
	}
}
