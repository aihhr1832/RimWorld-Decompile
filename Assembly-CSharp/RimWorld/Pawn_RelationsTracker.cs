﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Pawn_RelationsTracker : IExposable
	{
		private Pawn pawn;

		private List<DirectPawnRelation> directRelations = new List<DirectPawnRelation>();

		public bool everSeenByPlayer;

		public bool canGetRescuedThought = true;

		public Pawn relativeInvolvedInRescueQuest;

		private HashSet<Pawn> pawnsWithDirectRelationsWithMe = new HashSet<Pawn>();

		private List<Pawn> cachedFamilyByBlood = new List<Pawn>();

		private bool familyByBloodIsCached;

		private bool canCacheFamilyByBlood;

		private const int CheckDevelopBondRelationIntervalTicks = 2500;

		private const float MaxBondRelationCheckDist = 12f;

		private const float BondRelationPerIntervalChance = 0.001f;

		public const int FriendOpinionThreshold = 20;

		public const int RivalOpinionThreshold = -20;

		private static List<ISocialThought> tmpSocialThoughts = new List<ISocialThought>();

		[CompilerGenerated]
		private static Predicate<DirectPawnRelation> <>f__am$cache0;

		[CompilerGenerated]
		private static Predicate<Pawn> <>f__am$cache1;

		public Pawn_RelationsTracker(Pawn pawn)
		{
			this.pawn = pawn;
		}

		public List<DirectPawnRelation> DirectRelations
		{
			get
			{
				return this.directRelations;
			}
		}

		public IEnumerable<Pawn> Children
		{
			get
			{
				foreach (Pawn p in this.pawnsWithDirectRelationsWithMe)
				{
					List<DirectPawnRelation> hisDirectRels = p.relations.directRelations;
					for (int i = 0; i < hisDirectRels.Count; i++)
					{
						if (hisDirectRels[i].otherPawn == this.pawn && hisDirectRels[i].def == PawnRelationDefOf.Parent)
						{
							yield return p;
							break;
						}
					}
				}
				yield break;
			}
		}

		public int ChildrenCount
		{
			get
			{
				return this.Children.Count<Pawn>();
			}
		}

		public bool RelatedToAnyoneOrAnyoneRelatedToMe
		{
			get
			{
				return this.directRelations.Any<DirectPawnRelation>() || this.pawnsWithDirectRelationsWithMe.Any<Pawn>();
			}
		}

		public IEnumerable<Pawn> FamilyByBlood
		{
			get
			{
				IEnumerable<Pawn> familyByBlood_Internal;
				if (this.canCacheFamilyByBlood)
				{
					if (!this.familyByBloodIsCached)
					{
						this.cachedFamilyByBlood.Clear();
						this.cachedFamilyByBlood.AddRange(this.FamilyByBlood_Internal);
						this.familyByBloodIsCached = true;
					}
					familyByBlood_Internal = this.cachedFamilyByBlood;
				}
				else
				{
					familyByBlood_Internal = this.FamilyByBlood_Internal;
				}
				return familyByBlood_Internal;
			}
		}

		private IEnumerable<Pawn> FamilyByBlood_Internal
		{
			get
			{
				if (!this.RelatedToAnyoneOrAnyoneRelatedToMe)
				{
					yield break;
				}
				List<Pawn> familyStack = null;
				List<Pawn> familyChildrenStack = null;
				HashSet<Pawn> familyVisited = null;
				try
				{
					familyStack = SimplePool<List<Pawn>>.Get();
					familyChildrenStack = SimplePool<List<Pawn>>.Get();
					familyVisited = SimplePool<HashSet<Pawn>>.Get();
					familyStack.Add(this.pawn);
					familyVisited.Add(this.pawn);
					while (familyStack.Any<Pawn>())
					{
						Pawn p = familyStack[familyStack.Count - 1];
						familyStack.RemoveLast<Pawn>();
						if (p != this.pawn)
						{
							yield return p;
						}
						Pawn father = p.GetFather();
						if (father != null && !familyVisited.Contains(father))
						{
							familyStack.Add(father);
							familyVisited.Add(father);
						}
						Pawn mother = p.GetMother();
						if (mother != null && !familyVisited.Contains(mother))
						{
							familyStack.Add(mother);
							familyVisited.Add(mother);
						}
						familyChildrenStack.Clear();
						familyChildrenStack.Add(p);
						while (familyChildrenStack.Any<Pawn>())
						{
							Pawn child = familyChildrenStack[familyChildrenStack.Count - 1];
							familyChildrenStack.RemoveLast<Pawn>();
							if (child != p && child != this.pawn)
							{
								yield return child;
							}
							IEnumerable<Pawn> children = child.relations.Children;
							foreach (Pawn item in children)
							{
								if (!familyVisited.Contains(item))
								{
									familyChildrenStack.Add(item);
									familyVisited.Add(item);
								}
							}
						}
					}
				}
				finally
				{
					familyStack.Clear();
					SimplePool<List<Pawn>>.Return(familyStack);
					familyChildrenStack.Clear();
					SimplePool<List<Pawn>>.Return(familyChildrenStack);
					familyVisited.Clear();
					SimplePool<HashSet<Pawn>>.Return(familyVisited);
				}
				yield break;
			}
		}

		public IEnumerable<Pawn> PotentiallyRelatedPawns
		{
			get
			{
				if (!this.RelatedToAnyoneOrAnyoneRelatedToMe)
				{
					yield break;
				}
				List<Pawn> stack = null;
				HashSet<Pawn> visited = null;
				try
				{
					stack = SimplePool<List<Pawn>>.Get();
					visited = SimplePool<HashSet<Pawn>>.Get();
					stack.Add(this.pawn);
					visited.Add(this.pawn);
					while (stack.Any<Pawn>())
					{
						Pawn p = stack[stack.Count - 1];
						stack.RemoveLast<Pawn>();
						if (p != this.pawn)
						{
							yield return p;
						}
						for (int i = 0; i < p.relations.directRelations.Count; i++)
						{
							Pawn otherPawn = p.relations.directRelations[i].otherPawn;
							if (!visited.Contains(otherPawn))
							{
								stack.Add(otherPawn);
								visited.Add(otherPawn);
							}
						}
						foreach (Pawn item in p.relations.pawnsWithDirectRelationsWithMe)
						{
							if (!visited.Contains(item))
							{
								stack.Add(item);
								visited.Add(item);
							}
						}
					}
				}
				finally
				{
					stack.Clear();
					SimplePool<List<Pawn>>.Return(stack);
					visited.Clear();
					SimplePool<HashSet<Pawn>>.Return(visited);
				}
				yield break;
			}
		}

		public IEnumerable<Pawn> RelatedPawns
		{
			get
			{
				this.canCacheFamilyByBlood = true;
				this.familyByBloodIsCached = false;
				this.cachedFamilyByBlood.Clear();
				try
				{
					foreach (Pawn p in this.PotentiallyRelatedPawns)
					{
						if ((this.familyByBloodIsCached && this.cachedFamilyByBlood.Contains(p)) || this.pawn.GetRelations(p).Any<PawnRelationDef>())
						{
							yield return p;
						}
					}
				}
				finally
				{
					this.canCacheFamilyByBlood = false;
					this.familyByBloodIsCached = false;
					this.cachedFamilyByBlood.Clear();
				}
				yield break;
			}
		}

		public void ExposeData()
		{
			Scribe_Collections.Look<DirectPawnRelation>(ref this.directRelations, "directRelations", LookMode.Deep, new object[0]);
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				for (int i = 0; i < this.directRelations.Count; i++)
				{
					if (this.directRelations[i].otherPawn == null)
					{
						Log.Warning(string.Concat(new object[]
						{
							"Pawn ",
							this.pawn,
							" has relation \"",
							this.directRelations[i].def.defName,
							"\" with null pawn after loading. This means that we forgot to serialize pawns somewhere (e.g. pawns from passing trade ships)."
						}), false);
					}
				}
				this.directRelations.RemoveAll((DirectPawnRelation x) => x.otherPawn == null);
				for (int j = 0; j < this.directRelations.Count; j++)
				{
					this.directRelations[j].otherPawn.relations.pawnsWithDirectRelationsWithMe.Add(this.pawn);
				}
			}
			Scribe_Values.Look<bool>(ref this.everSeenByPlayer, "everSeenByPlayer", true, false);
			Scribe_Values.Look<bool>(ref this.canGetRescuedThought, "canGetRescuedThought", true, false);
			Scribe_References.Look<Pawn>(ref this.relativeInvolvedInRescueQuest, "relativeInvolvedInRescueQuest", false);
		}

		public void RelationsTrackerTick()
		{
			if (!this.pawn.Dead)
			{
				this.Tick_CheckStartMarriageCeremony();
				this.Tick_CheckDevelopBondRelation();
			}
		}

		public DirectPawnRelation GetDirectRelation(PawnRelationDef def, Pawn otherPawn)
		{
			DirectPawnRelation result;
			if (def.implied)
			{
				Log.Warning(def + " is not a direct relation.", false);
				result = null;
			}
			else
			{
				result = this.directRelations.Find((DirectPawnRelation x) => x.def == def && x.otherPawn == otherPawn);
			}
			return result;
		}

		public Pawn GetFirstDirectRelationPawn(PawnRelationDef def, Predicate<Pawn> predicate = null)
		{
			Pawn result;
			if (def.implied)
			{
				Log.Warning(def + " is not a direct relation.", false);
				result = null;
			}
			else
			{
				for (int i = 0; i < this.directRelations.Count; i++)
				{
					DirectPawnRelation directPawnRelation = this.directRelations[i];
					if (directPawnRelation.def == def && (predicate == null || predicate(directPawnRelation.otherPawn)))
					{
						return directPawnRelation.otherPawn;
					}
				}
				result = null;
			}
			return result;
		}

		public bool DirectRelationExists(PawnRelationDef def, Pawn otherPawn)
		{
			bool result;
			if (def.implied)
			{
				Log.Warning(def + " is not a direct relation.", false);
				result = false;
			}
			else
			{
				for (int i = 0; i < this.directRelations.Count; i++)
				{
					DirectPawnRelation directPawnRelation = this.directRelations[i];
					if (directPawnRelation.def == def && directPawnRelation.otherPawn == otherPawn)
					{
						return true;
					}
				}
				result = false;
			}
			return result;
		}

		public void AddDirectRelation(PawnRelationDef def, Pawn otherPawn)
		{
			if (def.implied)
			{
				Log.Warning(string.Concat(new object[]
				{
					"Tried to directly add implied pawn relation ",
					def,
					", pawn=",
					this.pawn,
					", otherPawn=",
					otherPawn
				}), false);
			}
			else if (otherPawn == this.pawn)
			{
				Log.Warning(string.Concat(new object[]
				{
					"Tried to add pawn relation ",
					def,
					" with self, pawn=",
					this.pawn
				}), false);
			}
			else if (this.DirectRelationExists(def, otherPawn))
			{
				Log.Warning(string.Concat(new object[]
				{
					"Tried to add the same relation twice: ",
					def,
					", pawn=",
					this.pawn,
					", otherPawn=",
					otherPawn
				}), false);
			}
			else
			{
				int startTicks = (Current.ProgramState != ProgramState.Playing) ? 0 : Find.TickManager.TicksGame;
				this.directRelations.Add(new DirectPawnRelation(def, otherPawn, startTicks));
				otherPawn.relations.pawnsWithDirectRelationsWithMe.Add(this.pawn);
				if (def.reflexive)
				{
					otherPawn.relations.directRelations.Add(new DirectPawnRelation(def, this.pawn, startTicks));
					this.pawnsWithDirectRelationsWithMe.Add(otherPawn);
				}
				this.GainedOrLostDirectRelation();
				otherPawn.relations.GainedOrLostDirectRelation();
			}
		}

		public void RemoveDirectRelation(DirectPawnRelation relation)
		{
			this.RemoveDirectRelation(relation.def, relation.otherPawn);
		}

		public void RemoveDirectRelation(PawnRelationDef def, Pawn otherPawn)
		{
			if (!this.TryRemoveDirectRelation(def, otherPawn))
			{
				Log.Warning(string.Concat(new object[]
				{
					"Could not remove relation ",
					def,
					" because it's not here. pawn=",
					this.pawn,
					", otherPawn=",
					otherPawn
				}), false);
			}
		}

		public bool TryRemoveDirectRelation(PawnRelationDef def, Pawn otherPawn)
		{
			bool result;
			if (def.implied)
			{
				Log.Warning(string.Concat(new object[]
				{
					"Tried to remove implied pawn relation ",
					def,
					", pawn=",
					this.pawn,
					", otherPawn=",
					otherPawn
				}), false);
				result = false;
			}
			else
			{
				for (int i = 0; i < this.directRelations.Count; i++)
				{
					if (this.directRelations[i].def == def && this.directRelations[i].otherPawn == otherPawn)
					{
						if (def.reflexive)
						{
							List<DirectPawnRelation> list = otherPawn.relations.directRelations;
							DirectPawnRelation item = list.Find((DirectPawnRelation x) => x.def == def && x.otherPawn == this.pawn);
							list.Remove(item);
							if (list.Find((DirectPawnRelation x) => x.otherPawn == this.pawn) == null)
							{
								this.pawnsWithDirectRelationsWithMe.Remove(otherPawn);
							}
						}
						this.directRelations.RemoveAt(i);
						if (this.directRelations.Find((DirectPawnRelation x) => x.otherPawn == otherPawn) == null)
						{
							otherPawn.relations.pawnsWithDirectRelationsWithMe.Remove(this.pawn);
						}
						this.GainedOrLostDirectRelation();
						otherPawn.relations.GainedOrLostDirectRelation();
						return true;
					}
				}
				result = false;
			}
			return result;
		}

		public int OpinionOf(Pawn other)
		{
			int result;
			if (!other.RaceProps.Humanlike || this.pawn == other)
			{
				result = 0;
			}
			else if (this.pawn.Dead)
			{
				result = 0;
			}
			else
			{
				int num = 0;
				foreach (PawnRelationDef pawnRelationDef in this.pawn.GetRelations(other))
				{
					num += pawnRelationDef.opinionOffset;
				}
				if (this.pawn.RaceProps.Humanlike)
				{
					num += this.pawn.needs.mood.thoughts.TotalOpinionOffset(other);
				}
				if (num != 0)
				{
					float num2 = 1f;
					List<Hediff> hediffs = this.pawn.health.hediffSet.hediffs;
					for (int i = 0; i < hediffs.Count; i++)
					{
						if (hediffs[i].CurStage != null)
						{
							num2 *= hediffs[i].CurStage.opinionOfOthersFactor;
						}
					}
					num = Mathf.RoundToInt((float)num * num2);
				}
				if (num > 0 && this.pawn.HostileTo(other))
				{
					num = 0;
				}
				result = Mathf.Clamp(num, -100, 100);
			}
			return result;
		}

		public string OpinionExplanation(Pawn other)
		{
			string result;
			if (!other.RaceProps.Humanlike || this.pawn == other)
			{
				result = "";
			}
			else
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.AppendLine("OpinionOf".Translate(new object[]
				{
					other.LabelShort
				}) + ": " + this.OpinionOf(other).ToStringWithSign());
				string pawnSituationLabel = SocialCardUtility.GetPawnSituationLabel(other, this.pawn);
				if (!pawnSituationLabel.NullOrEmpty())
				{
					stringBuilder.AppendLine(pawnSituationLabel);
				}
				stringBuilder.AppendLine("--------------");
				bool flag = false;
				if (this.pawn.Dead)
				{
					stringBuilder.AppendLine("IAmDead".Translate());
					flag = true;
				}
				else
				{
					IEnumerable<PawnRelationDef> relations = this.pawn.GetRelations(other);
					foreach (PawnRelationDef pawnRelationDef in relations)
					{
						stringBuilder.AppendLine(pawnRelationDef.GetGenderSpecificLabelCap(other) + ": " + pawnRelationDef.opinionOffset.ToStringWithSign());
						flag = true;
					}
					if (this.pawn.RaceProps.Humanlike)
					{
						ThoughtHandler thoughts = this.pawn.needs.mood.thoughts;
						thoughts.GetDistinctSocialThoughtGroups(other, Pawn_RelationsTracker.tmpSocialThoughts);
						for (int i = 0; i < Pawn_RelationsTracker.tmpSocialThoughts.Count; i++)
						{
							ISocialThought socialThought = Pawn_RelationsTracker.tmpSocialThoughts[i];
							int num = 1;
							Thought thought = (Thought)socialThought;
							if (thought.def.IsMemory)
							{
								num = thoughts.memories.NumMemoriesInGroup((Thought_MemorySocial)socialThought);
							}
							stringBuilder.Append(thought.LabelCapSocial);
							if (num != 1)
							{
								stringBuilder.Append(" x" + num);
							}
							stringBuilder.AppendLine(": " + thoughts.OpinionOffsetOfGroup(socialThought, other).ToStringWithSign());
							flag = true;
						}
					}
					List<Hediff> hediffs = this.pawn.health.hediffSet.hediffs;
					for (int j = 0; j < hediffs.Count; j++)
					{
						HediffStage curStage = hediffs[j].CurStage;
						if (curStage != null && curStage.opinionOfOthersFactor != 1f)
						{
							stringBuilder.Append(hediffs[j].LabelBase.CapitalizeFirst());
							if (curStage.opinionOfOthersFactor != 0f)
							{
								stringBuilder.AppendLine(": x" + curStage.opinionOfOthersFactor.ToStringPercent());
							}
							else
							{
								stringBuilder.AppendLine();
							}
							flag = true;
						}
					}
					if (this.pawn.HostileTo(other))
					{
						stringBuilder.AppendLine("Hostile".Translate());
						flag = true;
					}
				}
				if (!flag)
				{
					stringBuilder.AppendLine("NoneBrackets".Translate());
				}
				result = stringBuilder.ToString().TrimEndNewlines();
			}
			return result;
		}

		public float SecondaryLovinChanceFactor(Pawn otherPawn)
		{
			float result;
			if (this.pawn.def != otherPawn.def || this.pawn == otherPawn)
			{
				result = 0f;
			}
			else
			{
				if (Rand.ValueSeeded(this.pawn.thingIDNumber ^ 3273711) >= 0.015f)
				{
					if (this.pawn.RaceProps.Humanlike && this.pawn.story.traits.HasTrait(TraitDefOf.Gay))
					{
						if (otherPawn.gender != this.pawn.gender)
						{
							return 0f;
						}
					}
					else if (otherPawn.gender == this.pawn.gender)
					{
						return 0f;
					}
				}
				float ageBiologicalYearsFloat = this.pawn.ageTracker.AgeBiologicalYearsFloat;
				float ageBiologicalYearsFloat2 = otherPawn.ageTracker.AgeBiologicalYearsFloat;
				float num = 1f;
				if (this.pawn.gender == Gender.Male)
				{
					if (ageBiologicalYearsFloat2 < 16f)
					{
						return 0f;
					}
					float min = Mathf.Max(16f, ageBiologicalYearsFloat - 30f);
					float lower = Mathf.Max(20f, ageBiologicalYearsFloat - 10f);
					num = GenMath.FlatHill(0.15f, min, lower, ageBiologicalYearsFloat, ageBiologicalYearsFloat + 10f, 0.15f, ageBiologicalYearsFloat2);
				}
				else if (this.pawn.gender == Gender.Female)
				{
					if (ageBiologicalYearsFloat2 < 16f)
					{
						return 0f;
					}
					if (ageBiologicalYearsFloat2 < ageBiologicalYearsFloat - 10f)
					{
						return 0.15f;
					}
					if (ageBiologicalYearsFloat2 < ageBiologicalYearsFloat - 3f)
					{
						num = Mathf.InverseLerp(ageBiologicalYearsFloat - 10f, ageBiologicalYearsFloat - 3f, ageBiologicalYearsFloat2) * 0.3f;
					}
					else
					{
						num = GenMath.FlatHill(0.3f, ageBiologicalYearsFloat - 3f, ageBiologicalYearsFloat, ageBiologicalYearsFloat + 10f, ageBiologicalYearsFloat + 30f, 0.15f, ageBiologicalYearsFloat2);
					}
				}
				float num2 = 1f;
				num2 *= Mathf.Lerp(0.2f, 1f, otherPawn.health.capacities.GetLevel(PawnCapacityDefOf.Talking));
				num2 *= Mathf.Lerp(0.2f, 1f, otherPawn.health.capacities.GetLevel(PawnCapacityDefOf.Manipulation));
				num2 *= Mathf.Lerp(0.2f, 1f, otherPawn.health.capacities.GetLevel(PawnCapacityDefOf.Moving));
				int num3 = 0;
				if (otherPawn.RaceProps.Humanlike)
				{
					num3 = otherPawn.story.traits.DegreeOfTrait(TraitDefOf.Beauty);
				}
				float num4 = 1f;
				if (num3 < 0)
				{
					num4 = 0.3f;
				}
				else if (num3 > 0)
				{
					num4 = 2.3f;
				}
				float num5 = Mathf.InverseLerp(15f, 18f, ageBiologicalYearsFloat);
				float num6 = Mathf.InverseLerp(15f, 18f, ageBiologicalYearsFloat2);
				float num7 = num * num2 * num5 * num6 * num4;
				result = num7;
			}
			return result;
		}

		public float SecondaryRomanceChanceFactor(Pawn otherPawn)
		{
			float num = 1f;
			foreach (PawnRelationDef pawnRelationDef in this.pawn.GetRelations(otherPawn))
			{
				num *= pawnRelationDef.attractionFactor;
			}
			return this.SecondaryLovinChanceFactor(otherPawn) * num;
		}

		public float CompatibilityWith(Pawn otherPawn)
		{
			float result;
			if (this.pawn.def != otherPawn.def || this.pawn == otherPawn)
			{
				result = 0f;
			}
			else
			{
				float x = Mathf.Abs(this.pawn.ageTracker.AgeBiologicalYearsFloat - otherPawn.ageTracker.AgeBiologicalYearsFloat);
				float num = GenMath.LerpDouble(0f, 20f, 0.45f, -0.45f, x);
				num = Mathf.Clamp(num, -0.45f, 0.45f);
				float num2 = this.ConstantPerPawnsPairCompatibilityOffset(otherPawn.thingIDNumber);
				result = num + num2;
			}
			return result;
		}

		public float ConstantPerPawnsPairCompatibilityOffset(int otherPawnID)
		{
			Rand.PushState();
			Rand.Seed = (this.pawn.thingIDNumber ^ otherPawnID) * 37;
			float result = Rand.GaussianAsymmetric(0.3f, 1f, 1.4f);
			Rand.PopState();
			return result;
		}

		public void ClearAllRelations()
		{
			List<DirectPawnRelation> list = this.directRelations.ToList<DirectPawnRelation>();
			for (int i = 0; i < list.Count; i++)
			{
				this.RemoveDirectRelation(list[i]);
			}
			List<Pawn> list2 = this.pawnsWithDirectRelationsWithMe.ToList<Pawn>();
			for (int j = 0; j < list2.Count; j++)
			{
				List<DirectPawnRelation> list3 = list2[j].relations.directRelations.ToList<DirectPawnRelation>();
				for (int k = 0; k < list3.Count; k++)
				{
					if (list3[k].otherPawn == this.pawn)
					{
						list2[j].relations.RemoveDirectRelation(list3[k]);
					}
				}
			}
		}

		internal void Notify_PawnKilled(DamageInfo? dinfo, Map mapBeforeDeath)
		{
			foreach (Pawn pawn in this.PotentiallyRelatedPawns)
			{
				if (!pawn.Dead && pawn.needs.mood != null)
				{
					pawn.needs.mood.thoughts.situational.Notify_SituationalThoughtsDirty();
				}
			}
			this.RemoveMySpouseMarriageRelatedThoughts();
			if (this.everSeenByPlayer && !PawnGenerator.IsBeingGenerated(this.pawn))
			{
				if (!this.pawn.RaceProps.Animal)
				{
					this.AffectBondedAnimalsOnMyDeath();
				}
			}
			this.Notify_FailedRescueQuest();
		}

		public void Notify_PassedToWorld()
		{
			if (!this.pawn.Dead)
			{
				this.relativeInvolvedInRescueQuest = null;
			}
		}

		public void Notify_ExitedMap()
		{
			this.CheckRescued();
		}

		public void Notify_ChangedFaction()
		{
			if (this.pawn.Faction == Faction.OfPlayer)
			{
				this.CheckRescued();
			}
		}

		public void Notify_PawnSold(Pawn playerNegotiator)
		{
			foreach (Pawn pawn in this.PotentiallyRelatedPawns)
			{
				if (!pawn.Dead && pawn.needs.mood != null)
				{
					PawnRelationDef mostImportantRelation = pawn.GetMostImportantRelation(this.pawn);
					if (mostImportantRelation != null && mostImportantRelation.soldThought != null)
					{
						pawn.needs.mood.thoughts.memories.TryGainMemory(mostImportantRelation.soldThought, playerNegotiator);
					}
				}
			}
			this.RemoveMySpouseMarriageRelatedThoughts();
		}

		public void Notify_PawnKidnapped()
		{
			this.RemoveMySpouseMarriageRelatedThoughts();
		}

		public void Notify_RescuedBy(Pawn rescuer)
		{
			if (rescuer.RaceProps.Humanlike && this.canGetRescuedThought)
			{
				this.pawn.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.RescuedMe, rescuer);
				this.canGetRescuedThought = false;
			}
		}

		public void Notify_FailedRescueQuest()
		{
			if (this.relativeInvolvedInRescueQuest != null && !this.relativeInvolvedInRescueQuest.Dead && this.relativeInvolvedInRescueQuest.needs.mood != null)
			{
				Messages.Message("MessageFailedToRescueRelative".Translate(new object[]
				{
					this.pawn.LabelShort,
					this.relativeInvolvedInRescueQuest.LabelShort
				}), this.relativeInvolvedInRescueQuest, MessageTypeDefOf.PawnDeath, true);
				this.relativeInvolvedInRescueQuest.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.FailedToRescueRelative, this.pawn);
			}
			this.relativeInvolvedInRescueQuest = null;
		}

		private void CheckRescued()
		{
			if (this.relativeInvolvedInRescueQuest != null && !this.relativeInvolvedInRescueQuest.Dead && this.relativeInvolvedInRescueQuest.needs.mood != null)
			{
				Messages.Message("MessageRescuedRelative".Translate(new object[]
				{
					this.pawn.LabelShort,
					this.relativeInvolvedInRescueQuest.LabelShort
				}), this.relativeInvolvedInRescueQuest, MessageTypeDefOf.PositiveEvent, true);
				this.relativeInvolvedInRescueQuest.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.RescuedRelative, this.pawn);
			}
			this.relativeInvolvedInRescueQuest = null;
		}

		public float GetFriendDiedThoughtPowerFactor(int opinion)
		{
			return Mathf.Lerp(0.15f, 1f, Mathf.InverseLerp(20f, 100f, (float)opinion));
		}

		public float GetRivalDiedThoughtPowerFactor(int opinion)
		{
			return Mathf.Lerp(0.15f, 1f, Mathf.InverseLerp(-20f, -100f, (float)opinion));
		}

		private void RemoveMySpouseMarriageRelatedThoughts()
		{
			Pawn spouse = this.pawn.GetSpouse();
			if (spouse != null && !spouse.Dead && spouse.needs.mood != null)
			{
				MemoryThoughtHandler memories = spouse.needs.mood.thoughts.memories;
				memories.RemoveMemoriesOfDef(ThoughtDefOf.GotMarried);
				memories.RemoveMemoriesOfDef(ThoughtDefOf.HoneymoonPhase);
			}
		}

		public void CheckAppendBondedAnimalDiedInfo(ref string letter, ref string label)
		{
			if (this.pawn.RaceProps.Animal && this.everSeenByPlayer && !PawnGenerator.IsBeingGenerated(this.pawn))
			{
				Predicate<Pawn> isAffected = (Pawn x) => !x.Dead && (!x.RaceProps.Humanlike || !x.story.traits.HasTrait(TraitDefOf.Psychopath));
				int num = 0;
				for (int i = 0; i < this.directRelations.Count; i++)
				{
					if (this.directRelations[i].def == PawnRelationDefOf.Bond && isAffected(this.directRelations[i].otherPawn))
					{
						num++;
					}
				}
				if (num != 0)
				{
					string str;
					if (num == 1)
					{
						Pawn firstDirectRelationPawn = this.GetFirstDirectRelationPawn(PawnRelationDefOf.Bond, (Pawn x) => isAffected(x));
						str = "LetterPartBondedAnimalDied".Translate(new object[]
						{
							this.pawn.LabelDefinite(),
							firstDirectRelationPawn.LabelShort
						}).CapitalizeFirst();
					}
					else
					{
						StringBuilder stringBuilder = new StringBuilder();
						for (int j = 0; j < this.directRelations.Count; j++)
						{
							if (this.directRelations[j].def == PawnRelationDefOf.Bond && isAffected(this.directRelations[j].otherPawn))
							{
								stringBuilder.AppendLine("  - " + this.directRelations[j].otherPawn.LabelShort);
							}
						}
						str = "LetterPartBondedAnimalDiedMulti".Translate(new object[]
						{
							stringBuilder.ToString().TrimEndNewlines()
						});
					}
					label = label + " (" + "LetterLabelSuffixBondedAnimalDied".Translate() + ")";
					if (!letter.NullOrEmpty())
					{
						letter += "\n\n";
					}
					letter += str;
				}
			}
		}

		private void AffectBondedAnimalsOnMyDeath()
		{
			int num = 0;
			Pawn pawn = null;
			for (int i = 0; i < this.directRelations.Count; i++)
			{
				if (this.directRelations[i].def == PawnRelationDefOf.Bond && this.directRelations[i].otherPawn.Spawned)
				{
					pawn = this.directRelations[i].otherPawn;
					num++;
					float value = Rand.Value;
					MentalStateDef stateDef;
					if (value < 0.25f)
					{
						stateDef = MentalStateDefOf.Wander_Sad;
					}
					if (value < 0.5f)
					{
						stateDef = MentalStateDefOf.Wander_Psychotic;
					}
					else if (value < 0.75f)
					{
						stateDef = MentalStateDefOf.Berserk;
					}
					else
					{
						stateDef = MentalStateDefOf.Manhunter;
					}
					this.directRelations[i].otherPawn.mindState.mentalStateHandler.TryStartMentalState(stateDef, null, true, false, null, false);
				}
			}
			if (num == 1)
			{
				string str;
				if (pawn.Name != null && !pawn.Name.Numerical)
				{
					str = "MessageNamedBondedAnimalMentalBreak".Translate(new object[]
					{
						pawn.KindLabelIndefinite(),
						pawn.Name.ToStringShort,
						this.pawn.LabelShort
					});
				}
				else
				{
					str = "MessageBondedAnimalMentalBreak".Translate(new object[]
					{
						pawn.LabelIndefinite(),
						this.pawn.LabelShort
					});
				}
				Messages.Message(str.CapitalizeFirst(), pawn, MessageTypeDefOf.ThreatSmall, true);
			}
			else if (num > 1)
			{
				Messages.Message("MessageBondedAnimalsMentalBreak".Translate(new object[]
				{
					num,
					this.pawn.LabelShort
				}).CapitalizeFirst(), pawn, MessageTypeDefOf.ThreatSmall, true);
			}
		}

		private void Tick_CheckStartMarriageCeremony()
		{
			if (this.pawn.Spawned && !this.pawn.RaceProps.Animal)
			{
				if (this.pawn.IsHashIntervalTick(1017))
				{
					int ticksGame = Find.TickManager.TicksGame;
					for (int i = 0; i < this.directRelations.Count; i++)
					{
						float num = (float)(ticksGame - this.directRelations[i].startTicks) / 60000f;
						if (this.directRelations[i].def == PawnRelationDefOf.Fiance && this.pawn.thingIDNumber < this.directRelations[i].otherPawn.thingIDNumber && num > 10f && Rand.MTBEventOccurs(2f, 60000f, 1017f) && this.pawn.Map == this.directRelations[i].otherPawn.Map && this.pawn.Map.IsPlayerHome && MarriageCeremonyUtility.AcceptableGameConditionsToStartCeremony(this.pawn.Map) && MarriageCeremonyUtility.FianceReadyToStartCeremony(this.pawn) && MarriageCeremonyUtility.FianceReadyToStartCeremony(this.directRelations[i].otherPawn))
						{
							this.pawn.Map.lordsStarter.TryStartMarriageCeremony(this.pawn, this.directRelations[i].otherPawn);
						}
					}
				}
			}
		}

		private void Tick_CheckDevelopBondRelation()
		{
			if (this.pawn.Spawned && this.pawn.RaceProps.Animal && this.pawn.Faction == Faction.OfPlayer && this.pawn.playerSettings.RespectedMaster != null)
			{
				Pawn respectedMaster = this.pawn.playerSettings.RespectedMaster;
				if (this.pawn.IsHashIntervalTick(2500) && this.pawn.Position.InHorDistOf(respectedMaster.Position, 12f) && GenSight.LineOfSight(this.pawn.Position, respectedMaster.Position, this.pawn.Map, false, null, 0, 0))
				{
					RelationsUtility.TryDevelopBondRelation(respectedMaster, this.pawn, 0.001f);
				}
			}
		}

		private void GainedOrLostDirectRelation()
		{
			if (Current.ProgramState == ProgramState.Playing && !this.pawn.Dead && this.pawn.needs.mood != null)
			{
				this.pawn.needs.mood.thoughts.situational.Notify_SituationalThoughtsDirty();
			}
		}

		// Note: this type is marked as 'beforefieldinit'.
		static Pawn_RelationsTracker()
		{
		}

		[CompilerGenerated]
		private static bool <ExposeData>m__0(DirectPawnRelation x)
		{
			return x.otherPawn == null;
		}

		[CompilerGenerated]
		private static bool <CheckAppendBondedAnimalDiedInfo>m__1(Pawn x)
		{
			return !x.Dead && (!x.RaceProps.Humanlike || !x.story.traits.HasTrait(TraitDefOf.Psychopath));
		}

		[CompilerGenerated]
		private sealed class <>c__Iterator0 : IEnumerable, IEnumerable<Pawn>, IEnumerator, IDisposable, IEnumerator<Pawn>
		{
			internal HashSet<Pawn>.Enumerator $locvar0;

			internal Pawn <p>__1;

			internal List<DirectPawnRelation> <hisDirectRels>__2;

			internal int <i>__3;

			internal Pawn_RelationsTracker $this;

			internal Pawn $current;

			internal bool $disposing;

			internal int $PC;

			[DebuggerHidden]
			public <>c__Iterator0()
			{
			}

			public bool MoveNext()
			{
				uint num = (uint)this.$PC;
				this.$PC = -1;
				bool flag = false;
				switch (num)
				{
				case 0u:
					enumerator = this.pawnsWithDirectRelationsWithMe.GetEnumerator();
					num = 4294967293u;
					break;
				case 1u:
					break;
				default:
					return false;
				}
				try
				{
					switch (num)
					{
					}
					IL_117:
					if (enumerator.MoveNext())
					{
						p = enumerator.Current;
						hisDirectRels = p.relations.directRelations;
						for (i = 0; i < hisDirectRels.Count; i++)
						{
							if (hisDirectRels[i].otherPawn == this.pawn && hisDirectRels[i].def == PawnRelationDefOf.Parent)
							{
								this.$current = p;
								if (!this.$disposing)
								{
									this.$PC = 1;
								}
								flag = true;
								return true;
							}
						}
						goto IL_117;
					}
				}
				finally
				{
					if (!flag)
					{
						((IDisposable)enumerator).Dispose();
					}
				}
				this.$PC = -1;
				return false;
			}

			Pawn IEnumerator<Pawn>.Current
			{
				[DebuggerHidden]
				get
				{
					return this.$current;
				}
			}

			object IEnumerator.Current
			{
				[DebuggerHidden]
				get
				{
					return this.$current;
				}
			}

			[DebuggerHidden]
			public void Dispose()
			{
				uint num = (uint)this.$PC;
				this.$disposing = true;
				this.$PC = -1;
				switch (num)
				{
				case 1u:
					try
					{
					}
					finally
					{
						((IDisposable)enumerator).Dispose();
					}
					break;
				}
			}

			[DebuggerHidden]
			public void Reset()
			{
				throw new NotSupportedException();
			}

			[DebuggerHidden]
			IEnumerator IEnumerable.GetEnumerator()
			{
				return this.System.Collections.Generic.IEnumerable<Verse.Pawn>.GetEnumerator();
			}

			[DebuggerHidden]
			IEnumerator<Pawn> IEnumerable<Pawn>.GetEnumerator()
			{
				if (Interlocked.CompareExchange(ref this.$PC, 0, -2) == -2)
				{
					return this;
				}
				Pawn_RelationsTracker.<>c__Iterator0 <>c__Iterator = new Pawn_RelationsTracker.<>c__Iterator0();
				<>c__Iterator.$this = this;
				return <>c__Iterator;
			}
		}

		[CompilerGenerated]
		private sealed class <>c__Iterator1 : IEnumerable, IEnumerable<Pawn>, IEnumerator, IDisposable, IEnumerator<Pawn>
		{
			internal List<Pawn> <familyStack>__0;

			internal List<Pawn> <familyChildrenStack>__0;

			internal HashSet<Pawn> <familyVisited>__0;

			internal Pawn <p>__1;

			internal Pawn <father>__1;

			internal Pawn <mother>__1;

			internal Pawn <child>__2;

			internal IEnumerable<Pawn> <children>__2;

			internal IEnumerator<Pawn> $locvar0;

			internal Pawn_RelationsTracker $this;

			internal Pawn $current;

			internal bool $disposing;

			internal int $PC;

			[DebuggerHidden]
			public <>c__Iterator1()
			{
			}

			public bool MoveNext()
			{
				uint num = (uint)this.$PC;
				this.$PC = -1;
				bool flag = false;
				switch (num)
				{
				case 0u:
					if (!base.RelatedToAnyoneOrAnyoneRelatedToMe)
					{
						return false;
					}
					familyStack = null;
					familyChildrenStack = null;
					familyVisited = null;
					num = 4294967293u;
					break;
				case 1u:
				case 2u:
					break;
				default:
					return false;
				}
				try
				{
					switch (num)
					{
					case 1u:
						IL_11B:
						father = p.GetFather();
						if (father != null && !familyVisited.Contains(father))
						{
							familyStack.Add(father);
							familyVisited.Add(father);
						}
						mother = p.GetMother();
						if (mother != null && !familyVisited.Contains(mother))
						{
							familyStack.Add(mother);
							familyVisited.Add(mother);
						}
						familyChildrenStack.Clear();
						familyChildrenStack.Add(p);
						break;
					case 2u:
						IL_25D:
						children = child.relations.Children;
						enumerator = children.GetEnumerator();
						try
						{
							while (enumerator.MoveNext())
							{
								Pawn item = enumerator.Current;
								if (!familyVisited.Contains(item))
								{
									familyChildrenStack.Add(item);
									familyVisited.Add(item);
								}
							}
						}
						finally
						{
							if (enumerator != null)
							{
								enumerator.Dispose();
							}
						}
						break;
					default:
						familyStack = SimplePool<List<Pawn>>.Get();
						familyChildrenStack = SimplePool<List<Pawn>>.Get();
						familyVisited = SimplePool<HashSet<Pawn>>.Get();
						familyStack.Add(this.pawn);
						familyVisited.Add(this.pawn);
						goto IL_302;
					}
					if (familyChildrenStack.Any<Pawn>())
					{
						child = familyChildrenStack[familyChildrenStack.Count - 1];
						familyChildrenStack.RemoveLast<Pawn>();
						if (child != p && child != this.pawn)
						{
							this.$current = child;
							if (!this.$disposing)
							{
								this.$PC = 2;
							}
							flag = true;
							return true;
						}
						goto IL_25D;
					}
					IL_302:
					if (familyStack.Any<Pawn>())
					{
						p = familyStack[familyStack.Count - 1];
						familyStack.RemoveLast<Pawn>();
						if (p != this.pawn)
						{
							this.$current = p;
							if (!this.$disposing)
							{
								this.$PC = 1;
							}
							flag = true;
							return true;
						}
						goto IL_11B;
					}
				}
				finally
				{
					if (!flag)
					{
						this.<>__Finally0();
					}
				}
				this.$PC = -1;
				return false;
			}

			Pawn IEnumerator<Pawn>.Current
			{
				[DebuggerHidden]
				get
				{
					return this.$current;
				}
			}

			object IEnumerator.Current
			{
				[DebuggerHidden]
				get
				{
					return this.$current;
				}
			}

			[DebuggerHidden]
			public void Dispose()
			{
				uint num = (uint)this.$PC;
				this.$disposing = true;
				this.$PC = -1;
				switch (num)
				{
				case 1u:
				case 2u:
					try
					{
					}
					finally
					{
						this.<>__Finally0();
					}
					break;
				}
			}

			[DebuggerHidden]
			public void Reset()
			{
				throw new NotSupportedException();
			}

			[DebuggerHidden]
			IEnumerator IEnumerable.GetEnumerator()
			{
				return this.System.Collections.Generic.IEnumerable<Verse.Pawn>.GetEnumerator();
			}

			[DebuggerHidden]
			IEnumerator<Pawn> IEnumerable<Pawn>.GetEnumerator()
			{
				if (Interlocked.CompareExchange(ref this.$PC, 0, -2) == -2)
				{
					return this;
				}
				Pawn_RelationsTracker.<>c__Iterator1 <>c__Iterator = new Pawn_RelationsTracker.<>c__Iterator1();
				<>c__Iterator.$this = this;
				return <>c__Iterator;
			}

			private void <>__Finally0()
			{
				familyStack.Clear();
				SimplePool<List<Pawn>>.Return(familyStack);
				familyChildrenStack.Clear();
				SimplePool<List<Pawn>>.Return(familyChildrenStack);
				familyVisited.Clear();
				SimplePool<HashSet<Pawn>>.Return(familyVisited);
			}
		}

		[CompilerGenerated]
		private sealed class <>c__Iterator2 : IEnumerable, IEnumerable<Pawn>, IEnumerator, IDisposable, IEnumerator<Pawn>
		{
			internal List<Pawn> <stack>__0;

			internal HashSet<Pawn> <visited>__0;

			internal Pawn <p>__1;

			internal HashSet<Pawn>.Enumerator $locvar0;

			internal Pawn_RelationsTracker $this;

			internal Pawn $current;

			internal bool $disposing;

			internal int $PC;

			[DebuggerHidden]
			public <>c__Iterator2()
			{
			}

			public bool MoveNext()
			{
				uint num = (uint)this.$PC;
				this.$PC = -1;
				bool flag = false;
				switch (num)
				{
				case 0u:
					if (!base.RelatedToAnyoneOrAnyoneRelatedToMe)
					{
						return false;
					}
					stack = null;
					visited = null;
					num = 4294967293u;
					break;
				case 1u:
					break;
				default:
					return false;
				}
				try
				{
					switch (num)
					{
					case 1u:
						IL_101:
						for (int i = 0; i < p.relations.directRelations.Count; i++)
						{
							Pawn otherPawn = p.relations.directRelations[i].otherPawn;
							if (!visited.Contains(otherPawn))
							{
								stack.Add(otherPawn);
								visited.Add(otherPawn);
							}
						}
						enumerator = p.relations.pawnsWithDirectRelationsWithMe.GetEnumerator();
						try
						{
							while (enumerator.MoveNext())
							{
								Pawn item = enumerator.Current;
								if (!visited.Contains(item))
								{
									stack.Add(item);
									visited.Add(item);
								}
							}
						}
						finally
						{
							((IDisposable)enumerator).Dispose();
						}
						break;
					default:
						stack = SimplePool<List<Pawn>>.Get();
						visited = SimplePool<HashSet<Pawn>>.Get();
						stack.Add(this.pawn);
						visited.Add(this.pawn);
						break;
					}
					if (stack.Any<Pawn>())
					{
						p = stack[stack.Count - 1];
						stack.RemoveLast<Pawn>();
						if (p != this.pawn)
						{
							this.$current = p;
							if (!this.$disposing)
							{
								this.$PC = 1;
							}
							flag = true;
							return true;
						}
						goto IL_101;
					}
				}
				finally
				{
					if (!flag)
					{
						this.<>__Finally0();
					}
				}
				this.$PC = -1;
				return false;
			}

			Pawn IEnumerator<Pawn>.Current
			{
				[DebuggerHidden]
				get
				{
					return this.$current;
				}
			}

			object IEnumerator.Current
			{
				[DebuggerHidden]
				get
				{
					return this.$current;
				}
			}

			[DebuggerHidden]
			public void Dispose()
			{
				uint num = (uint)this.$PC;
				this.$disposing = true;
				this.$PC = -1;
				switch (num)
				{
				case 1u:
					try
					{
					}
					finally
					{
						this.<>__Finally0();
					}
					break;
				}
			}

			[DebuggerHidden]
			public void Reset()
			{
				throw new NotSupportedException();
			}

			[DebuggerHidden]
			IEnumerator IEnumerable.GetEnumerator()
			{
				return this.System.Collections.Generic.IEnumerable<Verse.Pawn>.GetEnumerator();
			}

			[DebuggerHidden]
			IEnumerator<Pawn> IEnumerable<Pawn>.GetEnumerator()
			{
				if (Interlocked.CompareExchange(ref this.$PC, 0, -2) == -2)
				{
					return this;
				}
				Pawn_RelationsTracker.<>c__Iterator2 <>c__Iterator = new Pawn_RelationsTracker.<>c__Iterator2();
				<>c__Iterator.$this = this;
				return <>c__Iterator;
			}

			private void <>__Finally0()
			{
				stack.Clear();
				SimplePool<List<Pawn>>.Return(stack);
				visited.Clear();
				SimplePool<HashSet<Pawn>>.Return(visited);
			}
		}

		[CompilerGenerated]
		private sealed class <>c__Iterator3 : IEnumerable, IEnumerable<Pawn>, IEnumerator, IDisposable, IEnumerator<Pawn>
		{
			internal IEnumerator<Pawn> $locvar0;

			internal Pawn <p>__1;

			internal bool <definitelyKin>__2;

			internal Pawn_RelationsTracker $this;

			internal Pawn $current;

			internal bool $disposing;

			internal int $PC;

			[DebuggerHidden]
			public <>c__Iterator3()
			{
			}

			public bool MoveNext()
			{
				uint num = (uint)this.$PC;
				this.$PC = -1;
				bool flag = false;
				switch (num)
				{
				case 0u:
					this.canCacheFamilyByBlood = true;
					this.familyByBloodIsCached = false;
					this.cachedFamilyByBlood.Clear();
					num = 4294967293u;
					break;
				case 1u:
					break;
				default:
					return false;
				}
				try
				{
					switch (num)
					{
					case 1u:
						break;
					default:
						enumerator = base.PotentiallyRelatedPawns.GetEnumerator();
						num = 4294967293u;
						break;
					}
					try
					{
						switch (num)
						{
						case 1u:
							IL_115:
							break;
						}
						if (enumerator.MoveNext())
						{
							p = enumerator.Current;
							bool definitelyKin = this.familyByBloodIsCached && this.cachedFamilyByBlood.Contains(p);
							if (definitelyKin || this.pawn.GetRelations(p).Any<PawnRelationDef>())
							{
								this.$current = p;
								if (!this.$disposing)
								{
									this.$PC = 1;
								}
								flag = true;
								return true;
							}
							goto IL_115;
						}
					}
					finally
					{
						if (!flag)
						{
							if (enumerator != null)
							{
								enumerator.Dispose();
							}
						}
					}
				}
				finally
				{
					if (!flag)
					{
						this.<>__Finally0();
					}
				}
				this.$PC = -1;
				return false;
			}

			Pawn IEnumerator<Pawn>.Current
			{
				[DebuggerHidden]
				get
				{
					return this.$current;
				}
			}

			object IEnumerator.Current
			{
				[DebuggerHidden]
				get
				{
					return this.$current;
				}
			}

			[DebuggerHidden]
			public void Dispose()
			{
				uint num = (uint)this.$PC;
				this.$disposing = true;
				this.$PC = -1;
				switch (num)
				{
				case 1u:
					try
					{
						try
						{
						}
						finally
						{
							if (enumerator != null)
							{
								enumerator.Dispose();
							}
						}
					}
					finally
					{
						this.<>__Finally0();
					}
					break;
				}
			}

			[DebuggerHidden]
			public void Reset()
			{
				throw new NotSupportedException();
			}

			[DebuggerHidden]
			IEnumerator IEnumerable.GetEnumerator()
			{
				return this.System.Collections.Generic.IEnumerable<Verse.Pawn>.GetEnumerator();
			}

			[DebuggerHidden]
			IEnumerator<Pawn> IEnumerable<Pawn>.GetEnumerator()
			{
				if (Interlocked.CompareExchange(ref this.$PC, 0, -2) == -2)
				{
					return this;
				}
				Pawn_RelationsTracker.<>c__Iterator3 <>c__Iterator = new Pawn_RelationsTracker.<>c__Iterator3();
				<>c__Iterator.$this = this;
				return <>c__Iterator;
			}

			private void <>__Finally0()
			{
				this.canCacheFamilyByBlood = false;
				this.familyByBloodIsCached = false;
				this.cachedFamilyByBlood.Clear();
			}
		}

		[CompilerGenerated]
		private sealed class <GetDirectRelation>c__AnonStorey4
		{
			internal PawnRelationDef def;

			internal Pawn otherPawn;

			public <GetDirectRelation>c__AnonStorey4()
			{
			}

			internal bool <>m__0(DirectPawnRelation x)
			{
				return x.def == this.def && x.otherPawn == this.otherPawn;
			}
		}

		[CompilerGenerated]
		private sealed class <TryRemoveDirectRelation>c__AnonStorey5
		{
			internal PawnRelationDef def;

			internal Pawn otherPawn;

			internal Pawn_RelationsTracker $this;

			public <TryRemoveDirectRelation>c__AnonStorey5()
			{
			}

			internal bool <>m__0(DirectPawnRelation x)
			{
				return x.def == this.def && x.otherPawn == this.$this.pawn;
			}

			internal bool <>m__1(DirectPawnRelation x)
			{
				return x.otherPawn == this.$this.pawn;
			}

			internal bool <>m__2(DirectPawnRelation x)
			{
				return x.otherPawn == this.otherPawn;
			}
		}

		[CompilerGenerated]
		private sealed class <CheckAppendBondedAnimalDiedInfo>c__AnonStorey6
		{
			internal Predicate<Pawn> isAffected;

			public <CheckAppendBondedAnimalDiedInfo>c__AnonStorey6()
			{
			}

			internal bool <>m__0(Pawn x)
			{
				return this.isAffected(x);
			}
		}
	}
}
