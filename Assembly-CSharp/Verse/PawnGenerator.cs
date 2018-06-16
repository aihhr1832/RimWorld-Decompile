﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;

namespace Verse
{
	// Token: 0x02000D51 RID: 3409
	[HasDebugOutput]
	public static class PawnGenerator
	{
		// Token: 0x06004B97 RID: 19351 RVA: 0x00276F34 File Offset: 0x00275334
		public static void Reset()
		{
			PawnGenerator.relationsGeneratableBlood = (from rel in DefDatabase<PawnRelationDef>.AllDefsListForReading
			where rel.familyByBloodRelation && rel.generationChanceFactor > 0f
			select rel).ToArray<PawnRelationDef>();
			PawnGenerator.relationsGeneratableNonblood = (from rel in DefDatabase<PawnRelationDef>.AllDefsListForReading
			where !rel.familyByBloodRelation && rel.generationChanceFactor > 0f
			select rel).ToArray<PawnRelationDef>();
		}

		// Token: 0x06004B98 RID: 19352 RVA: 0x00276FA4 File Offset: 0x002753A4
		public static Pawn GeneratePawn(PawnKindDef kindDef, Faction faction = null)
		{
			return PawnGenerator.GeneratePawn(new PawnGenerationRequest(kindDef, faction, PawnGenerationContext.NonPlayer, -1, false, false, false, false, true, false, 1f, false, true, true, false, false, false, false, null, null, null, null, null, null, null, null));
		}

		// Token: 0x06004B99 RID: 19353 RVA: 0x0027700C File Offset: 0x0027540C
		public static Pawn GeneratePawn(PawnGenerationRequest request)
		{
			Pawn result;
			try
			{
				Pawn pawn = PawnGenerator.GenerateOrRedressPawnInternal(request);
				if (pawn != null && !request.AllowDead && pawn.health.hediffSet.hediffs.Any<Hediff>())
				{
					bool dead = pawn.Dead;
					bool downed = pawn.Downed;
					pawn.health.hediffSet.DirtyCache();
					pawn.health.CheckForStateChange(null, null);
					if (pawn.Dead)
					{
						Log.Error(string.Concat(new object[]
						{
							"Pawn was generated dead but the pawn generation request specified the pawn must be alive. This shouldn't ever happen even if we ran out of tries because null pawn should have been returned instead in this case. Resetting health...\npawn.Dead=",
							pawn.Dead,
							" pawn.Downed=",
							pawn.Downed,
							" deadBefore=",
							dead,
							" downedBefore=",
							downed,
							"\nrequest=",
							request
						}), false);
						pawn.health.Reset();
					}
				}
				result = pawn;
			}
			catch (Exception arg)
			{
				Log.Error("Error while generating pawn. Rethrowing. Exception: " + arg, false);
				throw;
			}
			finally
			{
			}
			return result;
		}

		// Token: 0x06004B9A RID: 19354 RVA: 0x00277168 File Offset: 0x00275568
		private static Pawn GenerateOrRedressPawnInternal(PawnGenerationRequest request)
		{
			Pawn pawn = null;
			if (!request.Newborn && !request.ForceGenerateNewPawn)
			{
				if (request.ForceRedressWorldPawnIfFormerColonist)
				{
					IEnumerable<Pawn> validCandidatesToRedress = PawnGenerator.GetValidCandidatesToRedress(request);
					if ((from x in validCandidatesToRedress
					where PawnUtility.EverBeenColonistOrTameAnimal(x)
					select x).TryRandomElementByWeight((Pawn x) => PawnGenerator.WorldPawnSelectionWeight(x), out pawn))
					{
						PawnGenerator.RedressPawn(pawn, request);
						Find.WorldPawns.RemovePawn(pawn);
					}
				}
				if (pawn == null)
				{
					if (request.Inhabitant && request.Tile != -1)
					{
						Settlement settlement = Find.WorldObjects.WorldObjectAt<Settlement>(request.Tile);
						if (settlement != null && settlement.previouslyGeneratedInhabitants.Any<Pawn>())
						{
							IEnumerable<Pawn> validCandidatesToRedress2 = PawnGenerator.GetValidCandidatesToRedress(request);
							if ((from x in validCandidatesToRedress2
							where settlement.previouslyGeneratedInhabitants.Contains(x)
							select x).TryRandomElementByWeight((Pawn x) => PawnGenerator.WorldPawnSelectionWeight(x), out pawn))
							{
								PawnGenerator.RedressPawn(pawn, request);
								Find.WorldPawns.RemovePawn(pawn);
							}
						}
					}
				}
				if (pawn == null && Rand.Chance(PawnGenerator.ChanceToRedressAnyWorldPawn(request)))
				{
					IEnumerable<Pawn> validCandidatesToRedress3 = PawnGenerator.GetValidCandidatesToRedress(request);
					if (validCandidatesToRedress3.TryRandomElementByWeight((Pawn x) => PawnGenerator.WorldPawnSelectionWeight(x), out pawn))
					{
						PawnGenerator.RedressPawn(pawn, request);
						Find.WorldPawns.RemovePawn(pawn);
					}
				}
			}
			bool redressed;
			if (pawn == null)
			{
				redressed = false;
				pawn = PawnGenerator.GenerateNewPawnInternal(ref request);
				if (pawn == null)
				{
					return null;
				}
				if (request.Inhabitant && request.Tile != -1)
				{
					Settlement settlement2 = Find.WorldObjects.WorldObjectAt<Settlement>(request.Tile);
					if (settlement2 != null)
					{
						settlement2.previouslyGeneratedInhabitants.Add(pawn);
					}
				}
			}
			else
			{
				redressed = true;
			}
			if (Find.Scenario != null)
			{
				Find.Scenario.Notify_PawnGenerated(pawn, request.Context, redressed);
			}
			return pawn;
		}

		// Token: 0x06004B9B RID: 19355 RVA: 0x002773B4 File Offset: 0x002757B4
		public static void RedressPawn(Pawn pawn, PawnGenerationRequest request)
		{
			try
			{
				pawn.ChangeKind(request.KindDef);
				PawnGenerator.GenerateGearFor(pawn, request);
				if (pawn.Faction != request.Faction)
				{
					pawn.SetFaction(request.Faction, null);
				}
				if (pawn.guest != null)
				{
					pawn.guest.SetGuestStatus(null, false);
				}
			}
			finally
			{
			}
		}

		// Token: 0x06004B9C RID: 19356 RVA: 0x00277428 File Offset: 0x00275828
		public static bool IsBeingGenerated(Pawn pawn)
		{
			for (int i = 0; i < PawnGenerator.pawnsBeingGenerated.Count; i++)
			{
				if (PawnGenerator.pawnsBeingGenerated[i].Pawn == pawn)
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x06004B9D RID: 19357 RVA: 0x0027747C File Offset: 0x0027587C
		private static bool IsValidCandidateToRedress(Pawn pawn, PawnGenerationRequest request)
		{
			bool result;
			if (pawn.def != request.KindDef.race)
			{
				result = false;
			}
			else if (!request.WorldPawnFactionDoesntMatter && pawn.Faction != request.Faction)
			{
				result = false;
			}
			else if (!request.AllowDead && (pawn.Dead || pawn.Destroyed))
			{
				result = false;
			}
			else if (!request.AllowDowned && pawn.Downed)
			{
				result = false;
			}
			else if (pawn.health.hediffSet.BleedRateTotal > 0.001f)
			{
				result = false;
			}
			else if (!request.CanGeneratePawnRelations && pawn.RaceProps.IsFlesh && pawn.relations.RelatedToAnyoneOrAnyoneRelatedToMe)
			{
				result = false;
			}
			else if (!request.AllowGay && pawn.RaceProps.Humanlike && pawn.story.traits.HasTrait(TraitDefOf.Gay))
			{
				result = false;
			}
			else if (request.ValidatorPreGear != null && !request.ValidatorPreGear(pawn))
			{
				result = false;
			}
			else if (request.ValidatorPostGear != null && !request.ValidatorPostGear(pawn))
			{
				result = false;
			}
			else if (request.FixedBiologicalAge != null && pawn.ageTracker.AgeBiologicalYearsFloat != request.FixedBiologicalAge)
			{
				result = false;
			}
			else if (request.FixedChronologicalAge != null && (float)pawn.ageTracker.AgeChronologicalYears != request.FixedChronologicalAge)
			{
				result = false;
			}
			else if (request.FixedGender != null && pawn.gender != request.FixedGender)
			{
				result = false;
			}
			else if (request.FixedLastName != null && ((NameTriple)pawn.Name).Last != request.FixedLastName)
			{
				result = false;
			}
			else if (request.FixedMelanin != null && pawn.story != null && pawn.story.melanin != request.FixedMelanin)
			{
				result = false;
			}
			else if (request.Context == PawnGenerationContext.PlayerStarter && Find.Scenario != null && !Find.Scenario.AllowPlayerStartingPawn(pawn, true, request))
			{
				result = false;
			}
			else
			{
				if (request.MustBeCapableOfViolence)
				{
					if (pawn.story != null && pawn.story.WorkTagIsDisabled(WorkTags.Violent))
					{
						return false;
					}
					if (pawn.RaceProps.ToolUser && !pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
					{
						return false;
					}
				}
				result = true;
			}
			return result;
		}

		// Token: 0x06004B9E RID: 19358 RVA: 0x002777F0 File Offset: 0x00275BF0
		private static Pawn GenerateNewPawnInternal(ref PawnGenerationRequest request)
		{
			Pawn pawn = null;
			string text = null;
			bool ignoreScenarioRequirements = false;
			bool ignoreValidator = false;
			for (int i = 0; i < 120; i++)
			{
				if (i == 70)
				{
					Log.Error(string.Concat(new object[]
					{
						"Could not generate a pawn after ",
						70,
						" tries. Last error: ",
						text,
						" Ignoring scenario requirements."
					}), false);
					ignoreScenarioRequirements = true;
				}
				if (i == 100)
				{
					Log.Error(string.Concat(new object[]
					{
						"Could not generate a pawn after ",
						100,
						" tries. Last error: ",
						text,
						" Ignoring validator."
					}), false);
					ignoreValidator = true;
				}
				PawnGenerationRequest pawnGenerationRequest = request;
				pawn = PawnGenerator.TryGenerateNewPawnInternal(ref pawnGenerationRequest, out text, ignoreScenarioRequirements, ignoreValidator);
				if (pawn != null)
				{
					request = pawnGenerationRequest;
					break;
				}
			}
			Pawn result;
			if (pawn == null)
			{
				Log.Error(string.Concat(new object[]
				{
					"Pawn generation error: ",
					text,
					" Too many tries (",
					120,
					"), returning null. Generation request: ",
					request
				}), false);
				result = null;
			}
			else
			{
				result = pawn;
			}
			return result;
		}

		// Token: 0x06004B9F RID: 19359 RVA: 0x00277930 File Offset: 0x00275D30
		private static Pawn TryGenerateNewPawnInternal(ref PawnGenerationRequest request, out string error, bool ignoreScenarioRequirements, bool ignoreValidator)
		{
			error = null;
			Pawn pawn = (Pawn)ThingMaker.MakeThing(request.KindDef.race, null);
			PawnGenerator.pawnsBeingGenerated.Add(new PawnGenerator.PawnGenerationStatus(pawn, null));
			Pawn result;
			try
			{
				pawn.kindDef = request.KindDef;
				pawn.SetFactionDirect(request.Faction);
				PawnComponentsUtility.CreateInitialComponents(pawn);
				if (request.FixedGender != null)
				{
					pawn.gender = request.FixedGender.Value;
				}
				else if (pawn.RaceProps.hasGenders)
				{
					if (Rand.Value < 0.5f)
					{
						pawn.gender = Gender.Male;
					}
					else
					{
						pawn.gender = Gender.Female;
					}
				}
				else
				{
					pawn.gender = Gender.None;
				}
				PawnGenerator.GenerateRandomAge(pawn, request);
				pawn.needs.SetInitialLevels();
				if (!request.Newborn && request.CanGeneratePawnRelations)
				{
					PawnGenerator.GeneratePawnRelations(pawn, ref request);
				}
				if (pawn.RaceProps.Humanlike)
				{
					FactionDef def;
					Faction faction;
					if (request.Faction != null)
					{
						def = request.Faction.def;
					}
					else if (Find.FactionManager.TryGetRandomNonColonyHumanlikeFaction(out faction, false, true, TechLevel.Undefined))
					{
						def = faction.def;
					}
					else
					{
						def = Faction.OfAncients.def;
					}
					pawn.story.melanin = ((request.FixedMelanin == null) ? PawnSkinColors.RandomMelanin(request.Faction) : request.FixedMelanin.Value);
					pawn.story.crownType = ((Rand.Value >= 0.5f) ? CrownType.Narrow : CrownType.Average);
					pawn.story.hairColor = PawnHairColors.RandomHairColor(pawn.story.SkinColor, pawn.ageTracker.AgeBiologicalYears);
					PawnBioAndNameGenerator.GiveAppropriateBioAndNameTo(pawn, request.FixedLastName, def);
					pawn.story.hairDef = PawnHairChooser.RandomHairDefFor(pawn, def);
					PawnGenerator.GenerateTraits(pawn, request);
					PawnGenerator.GenerateBodyType(pawn);
					PawnGenerator.GenerateSkills(pawn);
				}
				if (pawn.RaceProps.Animal && request.Faction != null && request.Faction.IsPlayer)
				{
					pawn.training.SetWantedRecursive(TrainableDefOf.Tameness, true);
					pawn.training.Train(TrainableDefOf.Tameness, null, true);
				}
				PawnGenerator.GenerateInitialHediffs(pawn, request);
				if (pawn.workSettings != null && request.Faction != null && request.Faction.IsPlayer)
				{
					pawn.workSettings.EnableAndInitialize();
				}
				if (request.Faction != null && pawn.RaceProps.Animal)
				{
					pawn.GenerateNecessaryName();
				}
				if (Find.Scenario != null)
				{
					Find.Scenario.Notify_NewPawnGenerating(pawn, request.Context);
				}
				if (!request.AllowDead && (pawn.Dead || pawn.Destroyed))
				{
					PawnGenerator.DiscardGeneratedPawn(pawn);
					error = "Generated dead pawn.";
					result = null;
				}
				else if (!request.AllowDowned && pawn.Downed)
				{
					PawnGenerator.DiscardGeneratedPawn(pawn);
					error = "Generated downed pawn.";
					result = null;
				}
				else if (request.MustBeCapableOfViolence && ((pawn.story != null && pawn.story.WorkTagIsDisabled(WorkTags.Violent)) || (pawn.RaceProps.ToolUser && !pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))))
				{
					PawnGenerator.DiscardGeneratedPawn(pawn);
					error = "Generated pawn incapable of violence.";
					result = null;
				}
				else if (!ignoreScenarioRequirements && request.Context == PawnGenerationContext.PlayerStarter && Find.Scenario != null && !Find.Scenario.AllowPlayerStartingPawn(pawn, false, request))
				{
					PawnGenerator.DiscardGeneratedPawn(pawn);
					error = "Generated pawn doesn't meet scenario requirements.";
					result = null;
				}
				else if (!ignoreValidator && request.ValidatorPreGear != null && !request.ValidatorPreGear(pawn))
				{
					PawnGenerator.DiscardGeneratedPawn(pawn);
					error = "Generated pawn didn't pass validator check (pre-gear).";
					result = null;
				}
				else
				{
					if (!request.Newborn)
					{
						PawnGenerator.GenerateGearFor(pawn, request);
					}
					if (!ignoreValidator && request.ValidatorPostGear != null && !request.ValidatorPostGear(pawn))
					{
						PawnGenerator.DiscardGeneratedPawn(pawn);
						error = "Generated pawn didn't pass validator check (post-gear).";
						result = null;
					}
					else
					{
						for (int i = 0; i < PawnGenerator.pawnsBeingGenerated.Count - 1; i++)
						{
							if (PawnGenerator.pawnsBeingGenerated[i].PawnsGeneratedInTheMeantime == null)
							{
								PawnGenerator.pawnsBeingGenerated[i] = new PawnGenerator.PawnGenerationStatus(PawnGenerator.pawnsBeingGenerated[i].Pawn, new List<Pawn>());
							}
							PawnGenerator.pawnsBeingGenerated[i].PawnsGeneratedInTheMeantime.Add(pawn);
						}
						result = pawn;
					}
				}
			}
			finally
			{
				PawnGenerator.pawnsBeingGenerated.RemoveLast<PawnGenerator.PawnGenerationStatus>();
			}
			return result;
		}

		// Token: 0x06004BA0 RID: 19360 RVA: 0x00277E60 File Offset: 0x00276260
		private static void DiscardGeneratedPawn(Pawn pawn)
		{
			if (Find.WorldPawns.Contains(pawn))
			{
				Find.WorldPawns.RemovePawn(pawn);
			}
			Find.WorldPawns.PassToWorld(pawn, PawnDiscardDecideMode.Discard);
			List<Pawn> pawnsGeneratedInTheMeantime = PawnGenerator.pawnsBeingGenerated.Last<PawnGenerator.PawnGenerationStatus>().PawnsGeneratedInTheMeantime;
			if (pawnsGeneratedInTheMeantime != null)
			{
				for (int i = 0; i < pawnsGeneratedInTheMeantime.Count; i++)
				{
					Pawn pawn2 = pawnsGeneratedInTheMeantime[i];
					if (Find.WorldPawns.Contains(pawn2))
					{
						Find.WorldPawns.RemovePawn(pawn2);
					}
					Find.WorldPawns.PassToWorld(pawn2, PawnDiscardDecideMode.Discard);
					for (int j = 0; j < PawnGenerator.pawnsBeingGenerated.Count; j++)
					{
						PawnGenerator.pawnsBeingGenerated[j].PawnsGeneratedInTheMeantime.Remove(pawn2);
					}
				}
			}
		}

		// Token: 0x06004BA1 RID: 19361 RVA: 0x00277F38 File Offset: 0x00276338
		private static IEnumerable<Pawn> GetValidCandidatesToRedress(PawnGenerationRequest request)
		{
			IEnumerable<Pawn> enumerable = Find.WorldPawns.GetPawnsBySituation(WorldPawnSituation.Free);
			if (request.KindDef.factionLeader)
			{
				enumerable = enumerable.Concat(Find.WorldPawns.GetPawnsBySituation(WorldPawnSituation.FactionLeader));
			}
			return from x in enumerable
			where PawnGenerator.IsValidCandidateToRedress(x, request)
			select x;
		}

		// Token: 0x06004BA2 RID: 19362 RVA: 0x00277FA4 File Offset: 0x002763A4
		private static float ChanceToRedressAnyWorldPawn(PawnGenerationRequest request)
		{
			int pawnsBySituationCount = Find.WorldPawns.GetPawnsBySituationCount(WorldPawnSituation.Free);
			float num = Mathf.Min(0.02f + 0.01f * ((float)pawnsBySituationCount / 10f), 0.8f);
			if (request.MinChanceToRedressWorldPawn != null)
			{
				num = Mathf.Max(num, request.MinChanceToRedressWorldPawn.Value);
			}
			return num;
		}

		// Token: 0x06004BA3 RID: 19363 RVA: 0x00278014 File Offset: 0x00276414
		private static float WorldPawnSelectionWeight(Pawn p)
		{
			float result;
			if (p.RaceProps.IsFlesh && !p.relations.everSeenByPlayer && p.relations.RelatedToAnyoneOrAnyoneRelatedToMe)
			{
				result = 0.1f;
			}
			else
			{
				result = 1f;
			}
			return result;
		}

		// Token: 0x06004BA4 RID: 19364 RVA: 0x00278069 File Offset: 0x00276469
		private static void GenerateGearFor(Pawn pawn, PawnGenerationRequest request)
		{
			PawnApparelGenerator.GenerateStartingApparelFor(pawn, request);
			PawnWeaponGenerator.TryGenerateWeaponFor(pawn);
			PawnInventoryGenerator.GenerateInventoryFor(pawn, request);
		}

		// Token: 0x06004BA5 RID: 19365 RVA: 0x00278080 File Offset: 0x00276480
		private static void GenerateInitialHediffs(Pawn pawn, PawnGenerationRequest request)
		{
			int num = 0;
			for (;;)
			{
				AgeInjuryUtility.GenerateRandomOldAgeInjuries(pawn, !request.AllowDead);
				PawnTechHediffsGenerator.GenerateTechHediffsFor(pawn);
				PawnAddictionHediffsGenerator.GenerateAddictionsAndTolerancesFor(pawn);
				if (request.AllowDead && pawn.Dead)
				{
					break;
				}
				if (request.AllowDowned || !pawn.Downed)
				{
					break;
				}
				pawn.health.Reset();
				num++;
				if (num > 80)
				{
					goto Block_4;
				}
			}
			goto IL_DC;
			Block_4:
			Log.Warning(string.Concat(new object[]
			{
				"Could not generate old age injuries for ",
				pawn.ThingID,
				" of age ",
				pawn.ageTracker.AgeBiologicalYears,
				" that allow pawn to move after ",
				80,
				" tries. request=",
				request
			}), false);
			IL_DC:
			if (!pawn.Dead && (request.Faction == null || !request.Faction.IsPlayer))
			{
				int num2 = 0;
				while (pawn.health.HasHediffsNeedingTend(false))
				{
					num2++;
					if (num2 > 10000)
					{
						Log.Error("Too many iterations.", false);
						break;
					}
					TendUtility.DoTend(null, pawn, null);
				}
			}
		}

		// Token: 0x06004BA6 RID: 19366 RVA: 0x002781D8 File Offset: 0x002765D8
		private static void GenerateRandomAge(Pawn pawn, PawnGenerationRequest request)
		{
			if (request.FixedBiologicalAge != null && request.FixedChronologicalAge != null)
			{
				float? fixedBiologicalAge = request.FixedBiologicalAge;
				bool flag = fixedBiologicalAge != null;
				float? fixedChronologicalAge = request.FixedChronologicalAge;
				if ((flag & fixedChronologicalAge != null) && fixedBiologicalAge.GetValueOrDefault() > fixedChronologicalAge.GetValueOrDefault())
				{
					Log.Warning(string.Concat(new object[]
					{
						"Tried to generate age for pawn ",
						pawn,
						", but pawn generation request demands biological age (",
						request.FixedBiologicalAge,
						") to be greater than chronological age (",
						request.FixedChronologicalAge,
						")."
					}), false);
				}
			}
			if (request.Newborn)
			{
				pawn.ageTracker.AgeBiologicalTicks = 0L;
			}
			else if (request.FixedBiologicalAge != null)
			{
				pawn.ageTracker.AgeBiologicalTicks = (long)(request.FixedBiologicalAge.Value * 3600000f);
			}
			else
			{
				int num = 0;
				float num2;
				for (;;)
				{
					if (pawn.RaceProps.ageGenerationCurve != null)
					{
						num2 = (float)Mathf.RoundToInt(Rand.ByCurve(pawn.RaceProps.ageGenerationCurve));
					}
					else if (pawn.RaceProps.IsMechanoid)
					{
						num2 = Rand.Range(0f, 2500f);
					}
					else
					{
						num2 = Rand.ByCurve(PawnGenerator.DefaultAgeGenerationCurve) * pawn.RaceProps.lifeExpectancy;
					}
					num++;
					if (num > 300)
					{
						break;
					}
					if (num2 <= (float)pawn.kindDef.maxGenerationAge && num2 >= (float)pawn.kindDef.minGenerationAge)
					{
						goto IL_1DB;
					}
				}
				Log.Error("Tried 300 times to generate age for " + pawn, false);
				IL_1DB:
				pawn.ageTracker.AgeBiologicalTicks = (long)(num2 * 3600000f) + (long)Rand.Range(0, 3600000);
			}
			if (request.Newborn)
			{
				pawn.ageTracker.AgeChronologicalTicks = 0L;
			}
			else if (request.FixedChronologicalAge != null)
			{
				pawn.ageTracker.AgeChronologicalTicks = (long)(request.FixedChronologicalAge.Value * 3600000f);
			}
			else
			{
				int num3;
				if (request.CertainlyBeenInCryptosleep || Rand.Value < pawn.kindDef.backstoryCryptosleepCommonality)
				{
					float value = Rand.Value;
					if (value < 0.7f)
					{
						num3 = Rand.Range(0, 100);
					}
					else if (value < 0.95f)
					{
						num3 = Rand.Range(100, 1000);
					}
					else
					{
						int max = GenDate.Year((long)GenTicks.TicksAbs, 0f) - 2026 - pawn.ageTracker.AgeBiologicalYears;
						num3 = Rand.Range(1000, max);
					}
				}
				else
				{
					num3 = 0;
				}
				int ticksAbs = GenTicks.TicksAbs;
				long num4 = (long)ticksAbs - pawn.ageTracker.AgeBiologicalTicks;
				num4 -= (long)num3 * 3600000L;
				pawn.ageTracker.BirthAbsTicks = num4;
			}
			if (pawn.ageTracker.AgeBiologicalTicks > pawn.ageTracker.AgeChronologicalTicks)
			{
				pawn.ageTracker.AgeChronologicalTicks = pawn.ageTracker.AgeBiologicalTicks;
			}
		}

		// Token: 0x06004BA7 RID: 19367 RVA: 0x00278548 File Offset: 0x00276948
		public static int RandomTraitDegree(TraitDef traitDef)
		{
			int degree;
			if (traitDef.degreeDatas.Count == 1)
			{
				degree = traitDef.degreeDatas[0].degree;
			}
			else
			{
				degree = traitDef.degreeDatas.RandomElementByWeight((TraitDegreeData dd) => dd.commonality).degree;
			}
			return degree;
		}

		// Token: 0x06004BA8 RID: 19368 RVA: 0x002785B4 File Offset: 0x002769B4
		private static void GenerateTraits(Pawn pawn, PawnGenerationRequest request)
		{
			if (pawn.story != null)
			{
				if (pawn.story.childhood.forcedTraits != null)
				{
					List<TraitEntry> forcedTraits = pawn.story.childhood.forcedTraits;
					for (int i = 0; i < forcedTraits.Count; i++)
					{
						TraitEntry traitEntry = forcedTraits[i];
						if (traitEntry.def == null)
						{
							Log.Error("Null forced trait def on " + pawn.story.childhood, false);
						}
						else if (!pawn.story.traits.HasTrait(traitEntry.def))
						{
							pawn.story.traits.GainTrait(new Trait(traitEntry.def, traitEntry.degree, false));
						}
					}
				}
				if (pawn.story.adulthood != null && pawn.story.adulthood.forcedTraits != null)
				{
					List<TraitEntry> forcedTraits2 = pawn.story.adulthood.forcedTraits;
					for (int j = 0; j < forcedTraits2.Count; j++)
					{
						TraitEntry traitEntry2 = forcedTraits2[j];
						if (traitEntry2.def == null)
						{
							Log.Error("Null forced trait def on " + pawn.story.adulthood, false);
						}
						else if (!pawn.story.traits.HasTrait(traitEntry2.def))
						{
							pawn.story.traits.GainTrait(new Trait(traitEntry2.def, traitEntry2.degree, false));
						}
					}
				}
				int num = Rand.RangeInclusive(2, 3);
				if (request.AllowGay)
				{
					if (LovePartnerRelationUtility.HasAnyLovePartnerOfTheSameGender(pawn) || LovePartnerRelationUtility.HasAnyExLovePartnerOfTheSameGender(pawn))
					{
						Trait trait = new Trait(TraitDefOf.Gay, PawnGenerator.RandomTraitDegree(TraitDefOf.Gay), false);
						pawn.story.traits.GainTrait(trait);
					}
				}
				while (pawn.story.traits.allTraits.Count < num)
				{
					TraitDef newTraitDef = DefDatabase<TraitDef>.AllDefsListForReading.RandomElementByWeight((TraitDef tr) => tr.GetGenderSpecificCommonality(pawn.gender));
					if (!pawn.story.traits.HasTrait(newTraitDef))
					{
						if (newTraitDef == TraitDefOf.Gay)
						{
							if (!request.AllowGay)
							{
								continue;
							}
							if (LovePartnerRelationUtility.HasAnyLovePartnerOfTheOppositeGender(pawn) || LovePartnerRelationUtility.HasAnyExLovePartnerOfTheOppositeGender(pawn))
							{
								continue;
							}
						}
						if (request.Faction == null || Faction.OfPlayerSilentFail == null || !request.Faction.HostileTo(Faction.OfPlayer) || newTraitDef.allowOnHostileSpawn)
						{
							if (!pawn.story.traits.allTraits.Any((Trait tr) => newTraitDef.ConflictsWith(tr)) && (newTraitDef.conflictingTraits == null || !newTraitDef.conflictingTraits.Any((TraitDef tr) => pawn.story.traits.HasTrait(tr))))
							{
								if (newTraitDef.requiredWorkTypes == null || !pawn.story.OneOfWorkTypesIsDisabled(newTraitDef.requiredWorkTypes))
								{
									if (!pawn.story.WorkTagIsDisabled(newTraitDef.requiredWorkTags))
									{
										int degree = PawnGenerator.RandomTraitDegree(newTraitDef);
										if (!pawn.story.childhood.DisallowsTrait(newTraitDef, degree) && (pawn.story.adulthood == null || !pawn.story.adulthood.DisallowsTrait(newTraitDef, degree)))
										{
											Trait trait2 = new Trait(newTraitDef, degree, false);
											if (pawn.mindState != null && pawn.mindState.mentalBreaker != null)
											{
												float num2 = pawn.mindState.mentalBreaker.BreakThresholdExtreme;
												num2 += trait2.OffsetOfStat(StatDefOf.MentalBreakThreshold);
												num2 *= trait2.MultiplierOfStat(StatDefOf.MentalBreakThreshold);
												if (num2 > 0.4f)
												{
													continue;
												}
											}
											pawn.story.traits.GainTrait(trait2);
										}
									}
								}
							}
						}
					}
				}
			}
		}

		// Token: 0x06004BA9 RID: 19369 RVA: 0x00278AD4 File Offset: 0x00276ED4
		private static void GenerateBodyType(Pawn pawn)
		{
			if (pawn.story.adulthood != null)
			{
				pawn.story.bodyType = pawn.story.adulthood.BodyTypeFor(pawn.gender);
			}
			else if (Rand.Value < 0.5f)
			{
				pawn.story.bodyType = BodyTypeDefOf.Thin;
			}
			else
			{
				pawn.story.bodyType = ((pawn.gender != Gender.Female) ? BodyTypeDefOf.Male : BodyTypeDefOf.Female);
			}
		}

		// Token: 0x06004BAA RID: 19370 RVA: 0x00278B64 File Offset: 0x00276F64
		private static void GenerateSkills(Pawn pawn)
		{
			List<SkillDef> allDefsListForReading = DefDatabase<SkillDef>.AllDefsListForReading;
			for (int i = 0; i < allDefsListForReading.Count; i++)
			{
				SkillDef skillDef = allDefsListForReading[i];
				int num = PawnGenerator.FinalLevelOfSkill(pawn, skillDef);
				SkillRecord skill = pawn.skills.GetSkill(skillDef);
				skill.Level = num;
				if (!skill.TotallyDisabled)
				{
					float num2 = (float)num * 0.11f;
					float value = Rand.Value;
					if (value < num2)
					{
						if (value < num2 * 0.2f)
						{
							skill.passion = Passion.Major;
						}
						else
						{
							skill.passion = Passion.Minor;
						}
					}
					skill.xpSinceLastLevel = Rand.Range(skill.XpRequiredForLevelUp * 0.1f, skill.XpRequiredForLevelUp * 0.9f);
				}
			}
		}

		// Token: 0x06004BAB RID: 19371 RVA: 0x00278C2C File Offset: 0x0027702C
		private static int FinalLevelOfSkill(Pawn pawn, SkillDef sk)
		{
			float num;
			if (sk.usuallyDefinedInBackstories)
			{
				num = (float)Rand.RangeInclusive(0, 4);
			}
			else
			{
				num = Rand.ByCurve(PawnGenerator.LevelRandomCurve);
			}
			foreach (Backstory backstory in from bs in pawn.story.AllBackstories
			where bs != null
			select bs)
			{
				foreach (KeyValuePair<SkillDef, int> keyValuePair in backstory.skillGainsResolved)
				{
					if (keyValuePair.Key == sk)
					{
						num += (float)keyValuePair.Value * Rand.Range(1f, 1.4f);
					}
				}
			}
			for (int i = 0; i < pawn.story.traits.allTraits.Count; i++)
			{
				int num2 = 0;
				if (pawn.story.traits.allTraits[i].CurrentData.skillGains.TryGetValue(sk, out num2))
				{
					num += (float)num2;
				}
			}
			float num3 = Rand.Range(1f, PawnGenerator.AgeSkillMaxFactorCurve.Evaluate((float)pawn.ageTracker.AgeBiologicalYears));
			num *= num3;
			num = PawnGenerator.LevelFinalAdjustmentCurve.Evaluate(num);
			return Mathf.Clamp(Mathf.RoundToInt(num), 0, 20);
		}

		// Token: 0x06004BAC RID: 19372 RVA: 0x00278DEC File Offset: 0x002771EC
		public static void PostProcessGeneratedGear(Thing gear, Pawn pawn)
		{
			CompQuality compQuality = gear.TryGetComp<CompQuality>();
			if (compQuality != null)
			{
				compQuality.SetQuality(QualityUtility.GenerateQualityGeneratingPawn(pawn.kindDef), ArtGenerationContext.Outsider);
			}
			if (gear.def.useHitPoints)
			{
				float randomInRange = pawn.kindDef.gearHealthRange.RandomInRange;
				if (randomInRange < 1f)
				{
					int num = Mathf.RoundToInt(randomInRange * (float)gear.MaxHitPoints);
					num = Mathf.Max(1, num);
					gear.HitPoints = num;
				}
			}
		}

		// Token: 0x06004BAD RID: 19373 RVA: 0x00278E68 File Offset: 0x00277268
		private static void GeneratePawnRelations(Pawn pawn, ref PawnGenerationRequest request)
		{
			if (pawn.RaceProps.Humanlike)
			{
				Pawn[] array = (from x in PawnsFinder.AllMapsWorldAndTemporary_AliveOrDead
				where x.def == pawn.def
				select x).ToArray<Pawn>();
				if (array.Length != 0)
				{
					int num = 0;
					foreach (Pawn pawn2 in array)
					{
						if (pawn2.Discarded)
						{
							Log.Warning(string.Concat(new object[]
							{
								"Warning during generating pawn relations for ",
								pawn,
								": Pawn ",
								pawn2,
								" is discarded, yet he was yielded by PawnUtility. Discarding a pawn means that he is no longer managed by anything."
							}), false);
						}
						else if (pawn2.Faction != null && pawn2.Faction.IsPlayer)
						{
							num++;
						}
					}
					float num2 = 45f;
					num2 += (float)num * 2.7f;
					PawnGenerationRequest localReq = request;
					Pair<Pawn, PawnRelationDef> pair = PawnGenerator.GenerateSamples(array, PawnGenerator.relationsGeneratableBlood, 40).RandomElementByWeightWithDefault((Pair<Pawn, PawnRelationDef> x) => x.Second.generationChanceFactor * x.Second.Worker.GenerationChance(pawn, x.First, localReq), num2 * 40f / (float)(array.Length * PawnGenerator.relationsGeneratableBlood.Length));
					if (pair.First != null)
					{
						pair.Second.Worker.CreateRelation(pawn, pair.First, ref request);
					}
					Pair<Pawn, PawnRelationDef> pair2 = PawnGenerator.GenerateSamples(array, PawnGenerator.relationsGeneratableNonblood, 40).RandomElementByWeightWithDefault((Pair<Pawn, PawnRelationDef> x) => x.Second.generationChanceFactor * x.Second.Worker.GenerationChance(pawn, x.First, localReq), num2 * 40f / (float)(array.Length * PawnGenerator.relationsGeneratableNonblood.Length));
					if (pair2.First != null)
					{
						pair2.Second.Worker.CreateRelation(pawn, pair2.First, ref request);
					}
				}
			}
		}

		// Token: 0x06004BAE RID: 19374 RVA: 0x00279038 File Offset: 0x00277438
		private static Pair<Pawn, PawnRelationDef>[] GenerateSamples(Pawn[] pawns, PawnRelationDef[] relations, int count)
		{
			Pair<Pawn, PawnRelationDef>[] array = new Pair<Pawn, PawnRelationDef>[count];
			for (int i = 0; i < count; i++)
			{
				array[i] = new Pair<Pawn, PawnRelationDef>(pawns[Rand.Range(0, pawns.Length)], relations[Rand.Range(0, relations.Length)]);
			}
			return array;
		}

		// Token: 0x06004BAF RID: 19375 RVA: 0x00279090 File Offset: 0x00277490
		[DebugOutput]
		[Category("Performance")]
		public static void PawnGenerationHistogram()
		{
			DebugHistogram debugHistogram = new DebugHistogram((from x in Enumerable.Range(1, 20)
			select (float)x * 10f).ToArray<float>());
			for (int i = 0; i < 100; i++)
			{
				long timestamp = Stopwatch.GetTimestamp();
				Pawn pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(PawnKindDefOf.Colonist, null, PawnGenerationContext.NonPlayer, -1, true, false, false, false, true, false, 1f, false, true, true, false, false, false, false, null, null, null, null, null, null, null, null));
				debugHistogram.Add((float)((Stopwatch.GetTimestamp() - timestamp) * 1000L / Stopwatch.Frequency));
				pawn.Destroy(DestroyMode.Vanish);
			}
			debugHistogram.Display();
		}

		// Token: 0x040032BE RID: 12990
		private static List<PawnGenerator.PawnGenerationStatus> pawnsBeingGenerated = new List<PawnGenerator.PawnGenerationStatus>();

		// Token: 0x040032BF RID: 12991
		private static PawnRelationDef[] relationsGeneratableBlood = (from rel in DefDatabase<PawnRelationDef>.AllDefsListForReading
		where rel.familyByBloodRelation && rel.generationChanceFactor > 0f
		select rel).ToArray<PawnRelationDef>();

		// Token: 0x040032C0 RID: 12992
		private static PawnRelationDef[] relationsGeneratableNonblood = (from rel in DefDatabase<PawnRelationDef>.AllDefsListForReading
		where !rel.familyByBloodRelation && rel.generationChanceFactor > 0f
		select rel).ToArray<PawnRelationDef>();

		// Token: 0x040032C1 RID: 12993
		public const float MaxStartMentalBreakThreshold = 0.4f;

		// Token: 0x040032C2 RID: 12994
		private static SimpleCurve DefaultAgeGenerationCurve = new SimpleCurve
		{
			{
				new CurvePoint(0.05f, 0f),
				true
			},
			{
				new CurvePoint(0.1f, 100f),
				true
			},
			{
				new CurvePoint(0.675f, 100f),
				true
			},
			{
				new CurvePoint(0.75f, 30f),
				true
			},
			{
				new CurvePoint(0.875f, 18f),
				true
			},
			{
				new CurvePoint(1f, 10f),
				true
			},
			{
				new CurvePoint(1.125f, 3f),
				true
			},
			{
				new CurvePoint(1.25f, 0f),
				true
			}
		};

		// Token: 0x040032C3 RID: 12995
		public const float MaxGeneratedMechanoidAge = 2500f;

		// Token: 0x040032C4 RID: 12996
		private static readonly SimpleCurve AgeSkillMaxFactorCurve = new SimpleCurve
		{
			{
				new CurvePoint(0f, 0f),
				true
			},
			{
				new CurvePoint(10f, 0.7f),
				true
			},
			{
				new CurvePoint(35f, 1f),
				true
			},
			{
				new CurvePoint(60f, 1.6f),
				true
			}
		};

		// Token: 0x040032C5 RID: 12997
		private static readonly SimpleCurve LevelFinalAdjustmentCurve = new SimpleCurve
		{
			{
				new CurvePoint(0f, 0f),
				true
			},
			{
				new CurvePoint(10f, 10f),
				true
			},
			{
				new CurvePoint(20f, 16f),
				true
			},
			{
				new CurvePoint(27f, 20f),
				true
			}
		};

		// Token: 0x040032C6 RID: 12998
		private static readonly SimpleCurve LevelRandomCurve = new SimpleCurve
		{
			{
				new CurvePoint(0f, 0f),
				true
			},
			{
				new CurvePoint(0.5f, 150f),
				true
			},
			{
				new CurvePoint(4f, 150f),
				true
			},
			{
				new CurvePoint(5f, 25f),
				true
			},
			{
				new CurvePoint(10f, 5f),
				true
			},
			{
				new CurvePoint(15f, 0f),
				true
			}
		};

		// Token: 0x02000D52 RID: 3410
		[StructLayout(LayoutKind.Sequential, Size = 1)]
		private struct PawnGenerationStatus
		{
			// Token: 0x06004BBC RID: 19388 RVA: 0x0027955F File Offset: 0x0027795F
			public PawnGenerationStatus(Pawn pawn, List<Pawn> pawnsGeneratedInTheMeantime)
			{
				this = default(PawnGenerator.PawnGenerationStatus);
				this.Pawn = pawn;
				this.PawnsGeneratedInTheMeantime = pawnsGeneratedInTheMeantime;
			}

			// Token: 0x17000C3B RID: 3131
			// (get) Token: 0x06004BBD RID: 19389 RVA: 0x00279578 File Offset: 0x00277978
			// (set) Token: 0x06004BBE RID: 19390 RVA: 0x00279592 File Offset: 0x00277992
			public Pawn Pawn { get; private set; }

			// Token: 0x17000C3C RID: 3132
			// (get) Token: 0x06004BBF RID: 19391 RVA: 0x0027959C File Offset: 0x0027799C
			// (set) Token: 0x06004BC0 RID: 19392 RVA: 0x002795B6 File Offset: 0x002779B6
			public List<Pawn> PawnsGeneratedInTheMeantime { get; private set; }
		}
	}
}
