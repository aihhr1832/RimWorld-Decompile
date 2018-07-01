﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace RimWorld
{
	public class Faction : IExposable, ILoadReferenceable, ICommunicable
	{
		public FactionDef def;

		private string name = null;

		public int loadID = -1;

		public int randomKey;

		public float colorFromSpectrum = -999f;

		public float centralMelanin = 0.5f;

		private List<FactionRelation> relations = new List<FactionRelation>();

		public Pawn leader = null;

		private FactionTacticalMemory tacticalMemoryInt = new FactionTacticalMemory();

		public KidnappedPawnsTracker kidnapped;

		private List<PredatorThreat> predatorThreats = new List<PredatorThreat>();

		public Dictionary<Map, ByteGrid> avoidGridsBasic = new Dictionary<Map, ByteGrid>();

		public Dictionary<Map, ByteGrid> avoidGridsSmart = new Dictionary<Map, ByteGrid>();

		public bool defeated;

		public int lastTraderRequestTick = -9999999;

		private int naturalGoodwillTimer;

		private List<Map> avoidGridsBasicKeysWorkingList;

		private List<ByteGrid> avoidGridsBasicValuesWorkingList;

		private List<Map> avoidGridsSmartKeysWorkingList;

		private List<ByteGrid> avoidGridsSmartValuesWorkingList;

		private static List<PawnKindDef> allPawnKinds = new List<PawnKindDef>();

		[CompilerGenerated]
		private static Predicate<PredatorThreat> <>f__am$cache0;

		[CompilerGenerated]
		private static Predicate<Settlement> <>f__am$cache1;

		[CompilerGenerated]
		private static Predicate<Settlement> <>f__am$cache2;

		[CompilerGenerated]
		private static Func<Site, GlobalTargetInfo> <>f__am$cache3;

		[CompilerGenerated]
		private static Func<PawnGroupMaker, bool> <>f__am$cache4;

		public Faction()
		{
			this.randomKey = Rand.Range(0, int.MaxValue);
			this.kidnapped = new KidnappedPawnsTracker(this);
		}

		public string Name
		{
			get
			{
				string labelCap;
				if (this.HasName)
				{
					labelCap = this.name;
				}
				else
				{
					labelCap = this.def.LabelCap;
				}
				return labelCap;
			}
			set
			{
				this.name = value;
			}
		}

		public bool HasName
		{
			get
			{
				return this.name != null;
			}
		}

		public bool IsPlayer
		{
			get
			{
				return this.def.isPlayer;
			}
		}

		public int PlayerGoodwill
		{
			get
			{
				return this.GoodwillWith(Faction.OfPlayer);
			}
		}

		public FactionRelationKind PlayerRelationKind
		{
			get
			{
				return this.RelationKindWith(Faction.OfPlayer);
			}
		}

		public FactionTacticalMemory TacticalMemory
		{
			get
			{
				if (this.tacticalMemoryInt == null)
				{
					this.tacticalMemoryInt = new FactionTacticalMemory();
				}
				return this.tacticalMemoryInt;
			}
		}

		public Color Color
		{
			get
			{
				Color result;
				if (this.def.colorSpectrum.NullOrEmpty<Color>())
				{
					result = Color.white;
				}
				else
				{
					result = ColorsFromSpectrum.Get(this.def.colorSpectrum, this.colorFromSpectrum);
				}
				return result;
			}
		}

		public void ExposeData()
		{
			Scribe_References.Look<Pawn>(ref this.leader, "leader", false);
			Scribe_Collections.Look<Map, ByteGrid>(ref this.avoidGridsBasic, "avoidGridsBasic", LookMode.Reference, LookMode.Deep, ref this.avoidGridsBasicKeysWorkingList, ref this.avoidGridsBasicValuesWorkingList);
			Scribe_Collections.Look<Map, ByteGrid>(ref this.avoidGridsSmart, "avoidGridsSmart", LookMode.Reference, LookMode.Deep, ref this.avoidGridsSmartKeysWorkingList, ref this.avoidGridsSmartValuesWorkingList);
			Scribe_Defs.Look<FactionDef>(ref this.def, "def");
			Scribe_Values.Look<string>(ref this.name, "name", null, false);
			Scribe_Values.Look<int>(ref this.loadID, "loadID", 0, false);
			Scribe_Values.Look<int>(ref this.randomKey, "randomKey", 0, false);
			Scribe_Values.Look<float>(ref this.colorFromSpectrum, "colorFromSpectrum", 0f, false);
			Scribe_Values.Look<float>(ref this.centralMelanin, "centralMelanin", 0f, false);
			Scribe_Collections.Look<FactionRelation>(ref this.relations, "relations", LookMode.Deep, new object[0]);
			Scribe_Deep.Look<FactionTacticalMemory>(ref this.tacticalMemoryInt, "tacticalMemory", new object[0]);
			Scribe_Deep.Look<KidnappedPawnsTracker>(ref this.kidnapped, "kidnapped", new object[]
			{
				this
			});
			Scribe_Collections.Look<PredatorThreat>(ref this.predatorThreats, "predatorThreats", LookMode.Deep, new object[0]);
			Scribe_Values.Look<bool>(ref this.defeated, "defeated", false, false);
			Scribe_Values.Look<int>(ref this.lastTraderRequestTick, "lastTraderRequestTick", -9999999, false);
			Scribe_Values.Look<int>(ref this.naturalGoodwillTimer, "naturalGoodwillTimer", 0, false);
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				this.predatorThreats.RemoveAll((PredatorThreat x) => x.predator == null);
			}
		}

		public void FactionTick()
		{
			this.CheckNaturalTendencyToReachGoodwillThreshold();
			this.kidnapped.KidnappedPawnsTrackerTick();
			for (int i = this.predatorThreats.Count - 1; i >= 0; i--)
			{
				PredatorThreat predatorThreat = this.predatorThreats[i];
				if (predatorThreat.Expired)
				{
					this.predatorThreats.RemoveAt(i);
					if (predatorThreat.predator.Spawned)
					{
						predatorThreat.predator.Map.attackTargetsCache.UpdateTarget(predatorThreat.predator);
					}
				}
			}
			if (Find.TickManager.TicksGame % 1000 == 200 && this.IsPlayer)
			{
				if (NamePlayerFactionAndSettlementUtility.CanNameFactionNow())
				{
					Settlement settlement = Find.WorldObjects.Settlements.Find((Settlement x) => NamePlayerFactionAndSettlementUtility.CanNameSettlementSoon(x));
					if (settlement != null)
					{
						Find.WindowStack.Add(new Dialog_NamePlayerFactionAndSettlement(settlement));
					}
					else
					{
						Find.WindowStack.Add(new Dialog_NamePlayerFaction());
					}
				}
				else
				{
					Settlement settlement2 = Find.WorldObjects.Settlements.Find((Settlement x) => NamePlayerFactionAndSettlementUtility.CanNameSettlementNow(x));
					if (settlement2 != null)
					{
						if (NamePlayerFactionAndSettlementUtility.CanNameFactionSoon())
						{
							Find.WindowStack.Add(new Dialog_NamePlayerFactionAndSettlement(settlement2));
						}
						else
						{
							Find.WindowStack.Add(new Dialog_NamePlayerSettlement(settlement2));
						}
					}
				}
			}
		}

		private void CheckNaturalTendencyToReachGoodwillThreshold()
		{
			if (!this.IsPlayer)
			{
				int playerGoodwill = this.PlayerGoodwill;
				if (this.def.naturalColonyGoodwill.Includes(playerGoodwill))
				{
					this.naturalGoodwillTimer = 0;
				}
				else
				{
					this.naturalGoodwillTimer++;
					if (playerGoodwill < this.def.naturalColonyGoodwill.min)
					{
						if (this.def.goodwillDailyGain != 0f)
						{
							int num = (int)(10f / this.def.goodwillDailyGain * 60000f);
							if (this.naturalGoodwillTimer >= num)
							{
								Faction ofPlayer = Faction.OfPlayer;
								int goodwillChange = Mathf.Min(10, this.def.naturalColonyGoodwill.min - playerGoodwill);
								string reason = "GoodwillChangedReason_NaturallyOverTime".Translate(new object[]
								{
									this.def.naturalColonyGoodwill.min.ToString()
								});
								this.TryAffectGoodwillWith(ofPlayer, goodwillChange, true, true, reason, null);
								this.naturalGoodwillTimer = 0;
							}
						}
					}
					else if (playerGoodwill > this.def.naturalColonyGoodwill.max)
					{
						if (this.def.goodwillDailyFall != 0f)
						{
							int num2 = (int)(10f / this.def.goodwillDailyFall * 60000f);
							if (this.naturalGoodwillTimer >= num2)
							{
								Faction ofPlayer = Faction.OfPlayer;
								int goodwillChange = -Mathf.Min(10, playerGoodwill - this.def.naturalColonyGoodwill.max);
								string reason = "GoodwillChangedReason_NaturallyOverTime".Translate(new object[]
								{
									this.def.naturalColonyGoodwill.max.ToString()
								});
								this.TryAffectGoodwillWith(ofPlayer, goodwillChange, true, true, reason, null);
								this.naturalGoodwillTimer = 0;
							}
						}
					}
				}
			}
		}

		public ByteGrid GetAvoidGridBasic(Map map)
		{
			ByteGrid byteGrid;
			ByteGrid result;
			if (this.avoidGridsBasic.TryGetValue(map, out byteGrid))
			{
				result = byteGrid;
			}
			else
			{
				result = null;
			}
			return result;
		}

		public ByteGrid GetAvoidGridSmart(Map map)
		{
			ByteGrid byteGrid;
			ByteGrid result;
			if (this.avoidGridsSmart.TryGetValue(map, out byteGrid))
			{
				result = byteGrid;
			}
			else
			{
				result = null;
			}
			return result;
		}

		public void Notify_MapRemoved(Map map)
		{
			this.avoidGridsBasic.Remove(map);
			this.avoidGridsSmart.Remove(map);
			this.tacticalMemoryInt.Notify_MapRemoved(map);
		}

		public void TryMakeInitialRelationsWith(Faction other)
		{
			if (this.RelationWith(other, true) == null)
			{
				int a = (!this.def.permanentEnemy) ? this.def.startingGoodwill.RandomInRange : -100;
				if (this.IsPlayer)
				{
					a = 100;
				}
				int b = (!other.def.permanentEnemy) ? other.def.startingGoodwill.RandomInRange : -100;
				if (other.IsPlayer)
				{
					b = 100;
				}
				int num = Mathf.Min(a, b);
				FactionRelationKind kind;
				if (num <= -10)
				{
					kind = FactionRelationKind.Hostile;
				}
				else if (num >= 75)
				{
					kind = FactionRelationKind.Ally;
				}
				else
				{
					kind = FactionRelationKind.Neutral;
				}
				FactionRelation factionRelation = new FactionRelation();
				factionRelation.other = other;
				factionRelation.goodwill = num;
				factionRelation.kind = kind;
				this.relations.Add(factionRelation);
				FactionRelation factionRelation2 = new FactionRelation();
				factionRelation2.other = this;
				factionRelation2.goodwill = num;
				factionRelation2.kind = kind;
				other.relations.Add(factionRelation2);
			}
		}

		public PawnKindDef RandomPawnKind()
		{
			Faction.allPawnKinds.Clear();
			if (this.def.pawnGroupMakers != null)
			{
				for (int i = 0; i < this.def.pawnGroupMakers.Count; i++)
				{
					List<PawnGenOption> options = this.def.pawnGroupMakers[i].options;
					for (int j = 0; j < options.Count; j++)
					{
						Faction.allPawnKinds.Add(options[j].kind);
					}
				}
			}
			PawnKindDef result;
			if (!Faction.allPawnKinds.Any<PawnKindDef>())
			{
				result = this.def.basicMemberKind;
			}
			else
			{
				PawnKindDef pawnKindDef = Faction.allPawnKinds.RandomElement<PawnKindDef>();
				Faction.allPawnKinds.Clear();
				result = pawnKindDef;
			}
			return result;
		}

		public FactionRelation RelationWith(Faction other, bool allowNull = false)
		{
			FactionRelation result;
			if (other == this)
			{
				Log.Error("Tried to get relation between faction " + this + " and itself.", false);
				result = new FactionRelation();
			}
			else
			{
				for (int i = 0; i < this.relations.Count; i++)
				{
					if (this.relations[i].other == other)
					{
						return this.relations[i];
					}
				}
				if (!allowNull)
				{
					Log.Error(string.Concat(new object[]
					{
						"Faction ",
						this.name,
						" has null relation with ",
						other,
						". Returning dummy relation."
					}), false);
					result = new FactionRelation();
				}
				else
				{
					result = null;
				}
			}
			return result;
		}

		public int GoodwillWith(Faction other)
		{
			return this.RelationWith(other, false).goodwill;
		}

		public FactionRelationKind RelationKindWith(Faction other)
		{
			return this.RelationWith(other, false).kind;
		}

		public bool TryAffectGoodwillWith(Faction other, int goodwillChange, bool canSendMessage = true, bool canSendHostilityLetter = true, string reason = null, GlobalTargetInfo? lookTarget = null)
		{
			bool result;
			if (this.def.hidden || other.def.hidden || this.def.permanentEnemy || other.def.permanentEnemy || this.defeated || other.defeated || other == this)
			{
				result = false;
			}
			else if (goodwillChange > 0 && ((this.IsPlayer && SettlementUtility.IsPlayerAttackingAnySettlementOf(other)) || (other.IsPlayer && SettlementUtility.IsPlayerAttackingAnySettlementOf(this))))
			{
				result = false;
			}
			else if (goodwillChange == 0)
			{
				result = true;
			}
			else
			{
				int num = this.GoodwillWith(other);
				int num2 = Mathf.Clamp(num + goodwillChange, -100, 100);
				if (num == num2)
				{
					result = true;
				}
				else
				{
					FactionRelation factionRelation = this.RelationWith(other, false);
					factionRelation.goodwill = num2;
					bool flag;
					factionRelation.CheckKindThresholds(this, canSendHostilityLetter, reason, (lookTarget == null) ? GlobalTargetInfo.Invalid : lookTarget.Value, out flag);
					FactionRelation factionRelation2 = other.RelationWith(this, false);
					FactionRelationKind kind = factionRelation2.kind;
					factionRelation2.goodwill = factionRelation.goodwill;
					factionRelation2.kind = factionRelation.kind;
					bool flag2;
					if (kind != factionRelation2.kind)
					{
						other.Notify_RelationKindChanged(this, kind, canSendHostilityLetter, reason, (lookTarget == null) ? GlobalTargetInfo.Invalid : lookTarget.Value, out flag2);
					}
					else
					{
						flag2 = false;
					}
					if (canSendMessage && !flag && !flag2 && Current.ProgramState == ProgramState.Playing && (this.IsPlayer || other.IsPlayer))
					{
						Faction faction = (!this.IsPlayer) ? this : other;
						string text;
						if (!reason.NullOrEmpty())
						{
							text = "MessageGoodwillChangedWithReason".Translate(new object[]
							{
								faction.name,
								num.ToString("F0"),
								factionRelation.goodwill.ToString("F0"),
								reason
							});
						}
						else
						{
							text = "MessageGoodwillChanged".Translate(new object[]
							{
								faction.name,
								num.ToString("F0"),
								factionRelation.goodwill.ToString("F0")
							});
						}
						Messages.Message(text, (lookTarget == null) ? GlobalTargetInfo.Invalid : lookTarget.Value, ((float)goodwillChange <= 0f) ? MessageTypeDefOf.NegativeEvent : MessageTypeDefOf.PositiveEvent, true);
					}
					result = true;
				}
			}
			return result;
		}

		public bool TrySetNotHostileTo(Faction other, bool canSendLetter = true, string reason = null, GlobalTargetInfo? lookTarget = null)
		{
			if (this.RelationKindWith(other) == FactionRelationKind.Hostile)
			{
				this.TrySetRelationKind(other, FactionRelationKind.Neutral, canSendLetter, reason, lookTarget);
			}
			return this.RelationKindWith(other) != FactionRelationKind.Hostile;
		}

		public bool TrySetNotAlly(Faction other, bool canSendLetter = true, string reason = null, GlobalTargetInfo? lookTarget = null)
		{
			if (this.RelationKindWith(other) == FactionRelationKind.Ally)
			{
				this.TrySetRelationKind(other, FactionRelationKind.Neutral, canSendLetter, reason, lookTarget);
			}
			return this.RelationKindWith(other) != FactionRelationKind.Ally;
		}

		public bool TrySetRelationKind(Faction other, FactionRelationKind kind, bool canSendLetter = true, string reason = null, GlobalTargetInfo? lookTarget = null)
		{
			FactionRelation factionRelation = this.RelationWith(other, false);
			bool result;
			if (factionRelation.kind == kind)
			{
				result = true;
			}
			else
			{
				switch (kind)
				{
				case FactionRelationKind.Hostile:
					this.TryAffectGoodwillWith(other, -75 - factionRelation.goodwill, false, canSendLetter, reason, lookTarget);
					result = (factionRelation.kind == FactionRelationKind.Hostile);
					break;
				case FactionRelationKind.Neutral:
					this.TryAffectGoodwillWith(other, -factionRelation.goodwill, false, canSendLetter, reason, lookTarget);
					result = (factionRelation.kind == FactionRelationKind.Neutral);
					break;
				case FactionRelationKind.Ally:
					this.TryAffectGoodwillWith(other, 75 - factionRelation.goodwill, false, canSendLetter, reason, lookTarget);
					result = (factionRelation.kind == FactionRelationKind.Ally);
					break;
				default:
					throw new NotSupportedException(kind.ToString());
				}
			}
			return result;
		}

		public void RemoveAllRelations()
		{
			foreach (Faction faction in Find.FactionManager.AllFactionsListForReading)
			{
				if (faction != this)
				{
					faction.relations.RemoveAll((FactionRelation x) => x.other == this);
				}
			}
			this.relations.Clear();
		}

		public void TryAppendRelationKindChangedInfo(StringBuilder text, FactionRelationKind previousKind, FactionRelationKind newKind, string reason = null)
		{
			string text2 = null;
			this.TryAppendRelationKindChangedInfo(ref text2, previousKind, newKind, reason);
			if (!text2.NullOrEmpty())
			{
				text.AppendLine();
				text.AppendLine();
				text.Append(text2);
			}
		}

		public void TryAppendRelationKindChangedInfo(ref string text, FactionRelationKind previousKind, FactionRelationKind newKind, string reason = null)
		{
			if (previousKind != newKind)
			{
				if (!text.NullOrEmpty())
				{
					text += "\n\n";
				}
				if (newKind == FactionRelationKind.Hostile)
				{
					text += "LetterRelationsChange_Hostile".Translate(new object[]
					{
						this.name,
						this.PlayerGoodwill.ToStringWithSign(),
						-75.ToStringWithSign(),
						0.ToStringWithSign()
					});
					if (!reason.NullOrEmpty())
					{
						text = text + "\n\n" + "FinalStraw".Translate(new object[]
						{
							reason.CapitalizeFirst()
						});
					}
				}
				else if (newKind == FactionRelationKind.Ally)
				{
					text += "LetterRelationsChange_Ally".Translate(new object[]
					{
						this.name,
						this.PlayerGoodwill.ToStringWithSign(),
						75.ToStringWithSign(),
						0.ToStringWithSign()
					});
					if (!reason.NullOrEmpty())
					{
						string text2 = text;
						text = string.Concat(new string[]
						{
							text2,
							"\n\n",
							"LastFactionRelationsEvent".Translate(),
							": ",
							reason.CapitalizeFirst()
						});
					}
				}
				else if (newKind == FactionRelationKind.Neutral)
				{
					if (previousKind == FactionRelationKind.Hostile)
					{
						text += "LetterRelationsChange_NeutralFromHostile".Translate(new object[]
						{
							this.name,
							this.PlayerGoodwill.ToStringWithSign(),
							0.ToStringWithSign(),
							-75.ToStringWithSign(),
							75.ToStringWithSign()
						});
						if (!reason.NullOrEmpty())
						{
							string text2 = text;
							text = string.Concat(new string[]
							{
								text2,
								"\n\n",
								"LastFactionRelationsEvent".Translate(),
								": ",
								reason.CapitalizeFirst()
							});
						}
					}
					else
					{
						text += "LetterRelationsChange_NeutralFromAlly".Translate(new object[]
						{
							this.name,
							this.PlayerGoodwill.ToStringWithSign(),
							0.ToStringWithSign(),
							-75.ToStringWithSign(),
							75.ToStringWithSign()
						});
						if (!reason.NullOrEmpty())
						{
							string text2 = text;
							text = string.Concat(new string[]
							{
								text2,
								"\n\n",
								"Reason".Translate(),
								": ",
								reason.CapitalizeFirst()
							});
						}
					}
				}
			}
		}

		public void Notify_MemberTookDamage(Pawn member, DamageInfo dinfo)
		{
			if (dinfo.Instigator != null)
			{
				if (!this.IsPlayer)
				{
					Pawn pawn = dinfo.Instigator as Pawn;
					if (pawn != null && pawn.CurJob != null && pawn.CurJob.def == JobDefOf.PredatorHunt)
					{
						this.TookDamageFromPredator(pawn);
					}
					if (dinfo.Instigator.Faction != null && dinfo.Def.externalViolence && !this.HostileTo(dinfo.Instigator.Faction))
					{
						if (!member.InAggroMentalState)
						{
							if (pawn == null || !pawn.InMentalState || pawn.MentalStateDef != MentalStateDefOf.Berserk)
							{
								if (!member.InMentalState || !member.MentalStateDef.IsExtreme || member.MentalStateDef.category != MentalStateCategory.Malicious || this.PlayerRelationKind != FactionRelationKind.Ally)
								{
									if (dinfo.Instigator.Faction != Faction.OfPlayer || !PrisonBreakUtility.IsPrisonBreaking(member))
									{
										if (dinfo.Instigator.Faction == Faction.OfPlayer && !this.IsMutuallyHostileCrossfire(dinfo))
										{
											float num = Mathf.Min(100f, dinfo.Amount);
											int num2 = (int)(-1.3f * num);
											Faction faction = dinfo.Instigator.Faction;
											int goodwillChange = num2;
											string reason = "GoodwillChangedReason_AttackedPawn".Translate(new object[]
											{
												member.LabelShort
											});
											GlobalTargetInfo? lookTarget = new GlobalTargetInfo?(member);
											this.TryAffectGoodwillWith(faction, goodwillChange, true, true, reason, lookTarget);
										}
									}
								}
							}
						}
					}
				}
			}
		}

		public void Notify_MemberCaptured(Pawn member, Faction violator)
		{
			if (violator != this)
			{
				if (this.RelationKindWith(violator) != FactionRelationKind.Hostile)
				{
					FactionRelationKind kind = FactionRelationKind.Hostile;
					string reason = "GoodwillChangedReason_CapturedPawn".Translate(new object[]
					{
						member.LabelShort
					});
					this.TrySetRelationKind(violator, kind, true, reason, new GlobalTargetInfo?(member));
				}
			}
		}

		public void Notify_MemberDied(Pawn member, DamageInfo? dinfo, bool wasWorldPawn, Map map)
		{
			if (!this.IsPlayer)
			{
				if (!wasWorldPawn && !PawnGenerator.IsBeingGenerated(member) && Current.ProgramState == ProgramState.Playing && map != null && map.IsPlayerHome && !this.HostileTo(Faction.OfPlayer))
				{
					if (dinfo != null && dinfo.Value.Category == DamageInfo.SourceCategory.Collapse)
					{
						bool flag = MessagesRepeatAvoider.MessageShowAllowed("FactionRelationAdjustmentCrushed-" + this.Name, 5f);
						Faction ofPlayer = Faction.OfPlayer;
						int goodwillChange = (!member.RaceProps.Humanlike) ? -15 : -25;
						bool canSendMessage = flag;
						string reason = "GoodwillChangedReason_PawnCrushed".Translate(new object[]
						{
							member.LabelShort
						});
						this.TryAffectGoodwillWith(ofPlayer, goodwillChange, canSendMessage, true, reason, new GlobalTargetInfo?(new TargetInfo(member.Position, map, false)));
					}
					else if (dinfo != null && (dinfo.Value.Instigator == null || dinfo.Value.Instigator.Faction == null))
					{
						Faction ofPlayer = Faction.OfPlayer;
						int goodwillChange = (!member.RaceProps.Humanlike) ? -3 : -5;
						string reason = "GoodwillChangedReason_PawnDied".Translate(new object[]
						{
							member.LabelShort
						});
						GlobalTargetInfo? lookTarget = new GlobalTargetInfo?(member);
						this.TryAffectGoodwillWith(ofPlayer, goodwillChange, true, true, reason, lookTarget);
					}
				}
				if (member == this.leader)
				{
					this.Notify_LeaderDied();
				}
			}
		}

		public void Notify_LeaderDied()
		{
			Pawn pawn = this.leader;
			this.GenerateNewLeader();
			Find.LetterStack.ReceiveLetter("LetterLeadersDeathLabel".Translate(new object[]
			{
				this.name,
				this.def.leaderTitle
			}).CapitalizeFirst(), "LetterLeadersDeath".Translate(new object[]
			{
				pawn.Name.ToStringFull,
				this.name,
				this.leader.Name.ToStringFull,
				this.def.leaderTitle
			}).CapitalizeFirst(), LetterDefOf.NeutralEvent, GlobalTargetInfo.Invalid, this, null);
		}

		public void Notify_LeaderLost()
		{
			Pawn pawn = this.leader;
			this.GenerateNewLeader();
			Find.LetterStack.ReceiveLetter("LetterLeaderChangedLabel".Translate(new object[]
			{
				this.name,
				this.def.leaderTitle
			}).CapitalizeFirst(), "LetterLeaderChanged".Translate(new object[]
			{
				pawn.Name.ToStringFull,
				this.name,
				this.leader.Name.ToStringFull,
				this.def.leaderTitle
			}).CapitalizeFirst(), LetterDefOf.NeutralEvent, GlobalTargetInfo.Invalid, this, null);
		}

		public void Notify_RelationKindChanged(Faction other, FactionRelationKind previousKind, bool canSendLetter, string reason, GlobalTargetInfo lookTarget, out bool sentLetter)
		{
			if (Current.ProgramState != ProgramState.Playing || other != Faction.OfPlayer)
			{
				canSendLetter = false;
			}
			sentLetter = false;
			FactionRelationKind factionRelationKind = this.RelationKindWith(other);
			if (factionRelationKind == FactionRelationKind.Hostile)
			{
				if (Current.ProgramState == ProgramState.Playing)
				{
					foreach (Pawn pawn in PawnsFinder.AllMapsWorldAndTemporary_Alive.ToList<Pawn>())
					{
						if ((pawn.Faction == this && pawn.HostFaction == other) || (pawn.Faction == other && pawn.HostFaction == this))
						{
							pawn.guest.SetGuestStatus(pawn.HostFaction, true);
						}
					}
				}
			}
			if (other == Faction.OfPlayer)
			{
				List<Site> list = new List<Site>();
				List<Site> sites = Find.WorldObjects.Sites;
				for (int i = 0; i < sites.Count; i++)
				{
					if (sites[i].factionMustRemainHostile && sites[i].Faction == this && !this.HostileTo(Faction.OfPlayer) && !sites[i].HasMap)
					{
						list.Add(sites[i]);
					}
				}
				if (list.Any<Site>())
				{
					string label;
					string text;
					if (list.Count == 1)
					{
						label = "LetterLabelSiteNoLongerHostile".Translate();
						text = "LetterSiteNoLongerHostile".Translate(new object[]
						{
							this.Name,
							list[0].Label
						});
					}
					else
					{
						StringBuilder stringBuilder = new StringBuilder();
						for (int j = 0; j < list.Count; j++)
						{
							if (stringBuilder.Length != 0)
							{
								stringBuilder.AppendLine();
							}
							stringBuilder.Append("  - " + list[j].LabelCap);
							ImportantPawnComp component = list[j].GetComponent<ImportantPawnComp>();
							if (component != null && component.pawn.Any)
							{
								stringBuilder.Append(" (" + component.pawn[0].LabelCap + ")");
							}
						}
						label = "LetterLabelSiteNoLongerHostileMulti".Translate();
						text = "LetterSiteNoLongerHostileMulti".Translate(new object[]
						{
							this.Name
						}) + ":\n\n" + stringBuilder;
					}
					Find.LetterStack.ReceiveLetter(label, text, LetterDefOf.NeutralEvent, new LookTargets(from x in list
					select new GlobalTargetInfo(x.Tile)), null, null);
					for (int k = 0; k < list.Count; k++)
					{
						Find.WorldObjects.Remove(list[k]);
					}
				}
			}
			if (canSendLetter)
			{
				string text2 = "";
				this.TryAppendRelationKindChangedInfo(ref text2, previousKind, factionRelationKind, reason);
				if (factionRelationKind == FactionRelationKind.Hostile)
				{
					Find.LetterStack.ReceiveLetter("LetterLabelRelationsChange_Hostile".Translate(new object[]
					{
						this.name
					}), text2, LetterDefOf.NegativeEvent, lookTarget, this, null);
					sentLetter = true;
				}
				else if (factionRelationKind == FactionRelationKind.Ally)
				{
					Find.LetterStack.ReceiveLetter("LetterLabelRelationsChange_Ally".Translate(new object[]
					{
						this.name
					}), text2, LetterDefOf.PositiveEvent, lookTarget, this, null);
					sentLetter = true;
				}
				else if (factionRelationKind == FactionRelationKind.Neutral)
				{
					if (previousKind == FactionRelationKind.Hostile)
					{
						Find.LetterStack.ReceiveLetter("LetterLabelRelationsChange_NeutralFromHostile".Translate(new object[]
						{
							this.name
						}), text2, LetterDefOf.PositiveEvent, lookTarget, this, null);
						sentLetter = true;
					}
					else
					{
						Find.LetterStack.ReceiveLetter("LetterLabelRelationsChange_NeutralFromAlly".Translate(new object[]
						{
							this.name
						}), text2, LetterDefOf.NeutralEvent, lookTarget, this, null);
						sentLetter = true;
					}
				}
			}
			if (Current.ProgramState == ProgramState.Playing)
			{
				List<Map> maps = Find.Maps;
				for (int l = 0; l < maps.Count; l++)
				{
					maps[l].attackTargetsCache.Notify_FactionHostilityChanged(this, other);
					LordManager lordManager = maps[l].lordManager;
					for (int m = 0; m < lordManager.lords.Count; m++)
					{
						Lord lord = lordManager.lords[m];
						if (lord.faction == other)
						{
							lord.Notify_FactionRelationsChanged(this, previousKind);
						}
						else if (lord.faction == this)
						{
							lord.Notify_FactionRelationsChanged(other, previousKind);
						}
					}
				}
			}
		}

		public void Notify_PlayerTraded(float marketValueSentByPlayer, Pawn playerNegotiator)
		{
			Faction ofPlayer = Faction.OfPlayer;
			int goodwillChange = (int)(marketValueSentByPlayer / 400f);
			string reason = "GoodwillChangedReason_Traded".Translate();
			GlobalTargetInfo? lookTarget = new GlobalTargetInfo?(playerNegotiator);
			this.TryAffectGoodwillWith(ofPlayer, goodwillChange, true, true, reason, lookTarget);
		}

		public void Notify_MemberExitedMap(Pawn member, bool free)
		{
			if (free && member.HostFaction != null && member.guest != null && (member.guest.Released || !member.IsPrisoner))
			{
				bool flag = false;
				float num = 0f;
				if (!member.InMentalState && member.health.hediffSet.BleedRateTotal < 0.001f)
				{
					flag = true;
					num += 15f;
					if (PawnUtility.IsFactionLeader(member))
					{
						num += 40f;
					}
				}
				num += (float)Mathf.Min(member.mindState.timesGuestTendedToByPlayer, 10) * 1f;
				Faction hostFaction = member.HostFaction;
				int goodwillChange = (int)num;
				string reason = (!flag) ? "GoodwillChangedReason_Tended".Translate(new object[]
				{
					member.LabelShort
				}) : "GoodwillChangedReason_ExitedMapHealthy".Translate(new object[]
				{
					member.LabelShort
				});
				this.TryAffectGoodwillWith(hostFaction, goodwillChange, true, true, reason, null);
			}
			member.mindState.timesGuestTendedToByPlayer = 0;
		}

		public void GenerateNewLeader()
		{
			this.leader = null;
			if (this.def.pawnGroupMakers != null)
			{
				List<PawnKindDef> list = new List<PawnKindDef>();
				foreach (PawnGroupMaker pawnGroupMaker in from x in this.def.pawnGroupMakers
				where x.kindDef == PawnGroupKindDefOf.Combat
				select x)
				{
					foreach (PawnGenOption pawnGenOption in pawnGroupMaker.options)
					{
						if (pawnGenOption.kind.factionLeader)
						{
							list.Add(pawnGenOption.kind);
						}
					}
				}
				PawnKindDef kindDef;
				if (list.TryRandomElement(out kindDef))
				{
					this.leader = PawnGenerator.GeneratePawn(kindDef, this);
					if (this.leader.RaceProps.IsFlesh)
					{
						this.leader.relations.everSeenByPlayer = true;
					}
					if (!Find.WorldPawns.Contains(this.leader))
					{
						Find.WorldPawns.PassToWorld(this.leader, PawnDiscardDecideMode.KeepForever);
					}
				}
			}
		}

		public string GetCallLabel()
		{
			return this.name;
		}

		public string GetInfoText()
		{
			string labelCap = this.def.LabelCap;
			string text = labelCap;
			return string.Concat(new string[]
			{
				text,
				"\n",
				"goodwill".Translate().CapitalizeFirst(),
				": ",
				this.PlayerGoodwill.ToStringWithSign()
			});
		}

		Faction ICommunicable.GetFaction()
		{
			return this;
		}

		public void TryOpenComms(Pawn negotiator)
		{
			Dialog_Negotiation dialog_Negotiation = new Dialog_Negotiation(negotiator, this, FactionDialogMaker.FactionDialogFor(negotiator, this), true);
			dialog_Negotiation.soundAmbient = SoundDefOf.RadioComms_Ambience;
			Find.WindowStack.Add(dialog_Negotiation);
		}

		private bool LeaderIsAvailableToTalk()
		{
			bool result;
			if (this.leader == null)
			{
				result = false;
			}
			else
			{
				if (this.leader.Spawned)
				{
					if (this.leader.Downed || this.leader.IsPrisoner || !this.leader.Awake() || this.leader.InMentalState)
					{
						return false;
					}
				}
				result = true;
			}
			return result;
		}

		public FloatMenuOption CommFloatMenuOption(Building_CommsConsole console, Pawn negotiator)
		{
			FloatMenuOption result;
			if (this.IsPlayer)
			{
				result = null;
			}
			else
			{
				string text = "CallOnRadio".Translate(new object[]
				{
					this.GetCallLabel()
				});
				string text2 = text;
				text = string.Concat(new string[]
				{
					text2,
					" (",
					this.PlayerRelationKind.GetLabel(),
					", ",
					this.PlayerGoodwill.ToStringWithSign(),
					")"
				});
				if (!this.LeaderIsAvailableToTalk())
				{
					string str;
					if (this.leader != null)
					{
						str = "LeaderUnavailable".Translate(new object[]
						{
							this.leader.LabelShort
						});
					}
					else
					{
						str = "LeaderUnavailableNoLeader".Translate();
					}
					result = new FloatMenuOption(text + " (" + str + ")", null, MenuOptionPriority.Default, null, null, 0f, null, null);
				}
				else
				{
					result = FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(text, delegate()
					{
						console.GiveUseCommsJob(negotiator, this);
					}, MenuOptionPriority.InitiateSocial, null, null, 0f, null, null), negotiator, console, "ReservedBy");
				}
			}
			return result;
		}

		private void TookDamageFromPredator(Pawn predator)
		{
			for (int i = 0; i < this.predatorThreats.Count; i++)
			{
				if (this.predatorThreats[i].predator == predator)
				{
					this.predatorThreats[i].lastAttackTicks = Find.TickManager.TicksGame;
					return;
				}
			}
			PredatorThreat predatorThreat = new PredatorThreat();
			predatorThreat.predator = predator;
			predatorThreat.lastAttackTicks = Find.TickManager.TicksGame;
			this.predatorThreats.Add(predatorThreat);
		}

		public bool HasPredatorRecentlyAttackedAnyone(Pawn predator)
		{
			for (int i = 0; i < this.predatorThreats.Count; i++)
			{
				if (this.predatorThreats[i].predator == predator)
				{
					return true;
				}
			}
			return false;
		}

		private bool IsMutuallyHostileCrossfire(DamageInfo dinfo)
		{
			return dinfo.Instigator != null && dinfo.IntendedTarget != null && dinfo.IntendedTarget.HostileTo(dinfo.Instigator) && dinfo.IntendedTarget.HostileTo(this);
		}

		public static Faction OfPlayer
		{
			get
			{
				Faction ofPlayerSilentFail = Faction.OfPlayerSilentFail;
				if (ofPlayerSilentFail == null)
				{
					Log.Error("Could not find player faction.", false);
				}
				return ofPlayerSilentFail;
			}
		}

		public static Faction OfMechanoids
		{
			get
			{
				return Find.FactionManager.OfMechanoids;
			}
		}

		public static Faction OfInsects
		{
			get
			{
				return Find.FactionManager.OfInsects;
			}
		}

		public static Faction OfAncients
		{
			get
			{
				return Find.FactionManager.OfAncients;
			}
		}

		public static Faction OfAncientsHostile
		{
			get
			{
				return Find.FactionManager.OfAncientsHostile;
			}
		}

		public static Faction OfPlayerSilentFail
		{
			get
			{
				if (Current.ProgramState != ProgramState.Playing)
				{
					GameInitData gameInitData = Find.GameInitData;
					if (gameInitData != null && gameInitData.playerFaction != null)
					{
						return gameInitData.playerFaction;
					}
				}
				return Find.FactionManager.OfPlayer;
			}
		}

		public string GetUniqueLoadID()
		{
			return "Faction_" + this.loadID;
		}

		public override string ToString()
		{
			string result;
			if (this.name != null)
			{
				result = this.name;
			}
			else if (this.def != null)
			{
				result = this.def.defName;
			}
			else
			{
				result = "[faction of no def]";
			}
			return result;
		}

		public string DebugString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			ByteGrid avoidGridSmart = this.GetAvoidGridSmart(Find.CurrentMap);
			if (avoidGridSmart != null)
			{
				stringBuilder.Append("Avoidgrid val at mouse: ");
				if (UI.MouseCell().InBounds(Find.CurrentMap))
				{
					stringBuilder.AppendLine(avoidGridSmart[UI.MouseCell()].ToString());
				}
				stringBuilder.Append("Avoidgrid pathcost at mouse: ");
				if (UI.MouseCell().InBounds(Find.CurrentMap))
				{
					stringBuilder.AppendLine(((int)(avoidGridSmart[UI.MouseCell()] * 8)).ToString());
				}
			}
			return stringBuilder.ToString();
		}

		public void DebugDrawOnMap()
		{
			ByteGrid avoidGridSmart = this.GetAvoidGridSmart(Find.CurrentMap);
			if (avoidGridSmart != null)
			{
				avoidGridSmart.DebugDraw();
			}
		}

		// Note: this type is marked as 'beforefieldinit'.
		static Faction()
		{
		}

		[CompilerGenerated]
		private static bool <ExposeData>m__0(PredatorThreat x)
		{
			return x.predator == null;
		}

		[CompilerGenerated]
		private static bool <FactionTick>m__1(Settlement x)
		{
			return NamePlayerFactionAndSettlementUtility.CanNameSettlementSoon(x);
		}

		[CompilerGenerated]
		private static bool <FactionTick>m__2(Settlement x)
		{
			return NamePlayerFactionAndSettlementUtility.CanNameSettlementNow(x);
		}

		[CompilerGenerated]
		private bool <RemoveAllRelations>m__3(FactionRelation x)
		{
			return x.other == this;
		}

		[CompilerGenerated]
		private static GlobalTargetInfo <Notify_RelationKindChanged>m__4(Site x)
		{
			return new GlobalTargetInfo(x.Tile);
		}

		[CompilerGenerated]
		private static bool <GenerateNewLeader>m__5(PawnGroupMaker x)
		{
			return x.kindDef == PawnGroupKindDefOf.Combat;
		}

		[CompilerGenerated]
		private sealed class <CommFloatMenuOption>c__AnonStorey0
		{
			internal Building_CommsConsole console;

			internal Pawn negotiator;

			internal Faction $this;

			public <CommFloatMenuOption>c__AnonStorey0()
			{
			}

			internal void <>m__0()
			{
				this.console.GiveUseCommsJob(this.negotiator, this.$this);
			}
		}
	}
}
