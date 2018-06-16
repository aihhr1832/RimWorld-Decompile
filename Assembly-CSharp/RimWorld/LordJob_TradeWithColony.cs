﻿using System;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld
{
	// Token: 0x02000178 RID: 376
	public class LordJob_TradeWithColony : LordJob
	{
		// Token: 0x060007BD RID: 1981 RVA: 0x0004BB46 File Offset: 0x00049F46
		public LordJob_TradeWithColony()
		{
		}

		// Token: 0x060007BE RID: 1982 RVA: 0x0004BB4F File Offset: 0x00049F4F
		public LordJob_TradeWithColony(Faction faction, IntVec3 chillSpot)
		{
			this.faction = faction;
			this.chillSpot = chillSpot;
		}

		// Token: 0x060007BF RID: 1983 RVA: 0x0004BB68 File Offset: 0x00049F68
		public override StateGraph CreateGraph()
		{
			StateGraph stateGraph = new StateGraph();
			LordToil_Travel lordToil_Travel = new LordToil_Travel(this.chillSpot);
			stateGraph.StartingToil = lordToil_Travel;
			LordToil_DefendTraderCaravan lordToil_DefendTraderCaravan = new LordToil_DefendTraderCaravan();
			stateGraph.AddToil(lordToil_DefendTraderCaravan);
			LordToil_DefendTraderCaravan lordToil_DefendTraderCaravan2 = new LordToil_DefendTraderCaravan(this.chillSpot);
			stateGraph.AddToil(lordToil_DefendTraderCaravan2);
			LordToil_ExitMapAndEscortCarriers lordToil_ExitMapAndEscortCarriers = new LordToil_ExitMapAndEscortCarriers();
			stateGraph.AddToil(lordToil_ExitMapAndEscortCarriers);
			LordToil_ExitMap lordToil_ExitMap = new LordToil_ExitMap(LocomotionUrgency.None, false);
			stateGraph.AddToil(lordToil_ExitMap);
			LordToil_ExitMap lordToil_ExitMap2 = new LordToil_ExitMap(LocomotionUrgency.Walk, true);
			stateGraph.AddToil(lordToil_ExitMap2);
			LordToil_ExitMapTraderFighting lordToil_ExitMapTraderFighting = new LordToil_ExitMapTraderFighting();
			stateGraph.AddToil(lordToil_ExitMapTraderFighting);
			Transition transition = new Transition(lordToil_Travel, lordToil_ExitMapAndEscortCarriers, false, true);
			transition.AddSources(new LordToil[]
			{
				lordToil_DefendTraderCaravan,
				lordToil_DefendTraderCaravan2
			});
			transition.AddPreAction(new TransitionAction_Message("MessageVisitorsDangerousTemperature".Translate(new object[]
			{
				this.faction.def.pawnsPlural.CapitalizeFirst(),
				this.faction.Name
			}), null, 1f));
			transition.AddPostAction(new TransitionAction_EndAllJobs());
			transition.AddTrigger(new Trigger_PawnExperiencingDangerousTemperatures());
			stateGraph.AddTransition(transition);
			Transition transition2 = new Transition(lordToil_Travel, lordToil_ExitMap2, false, true);
			transition2.AddSources(new LordToil[]
			{
				lordToil_DefendTraderCaravan,
				lordToil_DefendTraderCaravan2,
				lordToil_ExitMapAndEscortCarriers,
				lordToil_ExitMap,
				lordToil_ExitMapTraderFighting
			});
			transition2.AddTrigger(new Trigger_PawnCannotReachMapEdge());
			transition2.AddPostAction(new TransitionAction_Message("MessageVisitorsTrappedLeaving".Translate(new object[]
			{
				this.faction.def.pawnsPlural.CapitalizeFirst(),
				this.faction.Name
			}), null, 1f));
			transition2.AddPostAction(new TransitionAction_WakeAll());
			transition2.AddPostAction(new TransitionAction_EndAllJobs());
			stateGraph.AddTransition(transition2);
			Transition transition3 = new Transition(lordToil_ExitMap2, lordToil_ExitMapTraderFighting, false, true);
			transition3.AddTrigger(new Trigger_PawnCanReachMapEdge());
			transition3.AddPostAction(new TransitionAction_EndAllJobs());
			stateGraph.AddTransition(transition3);
			Transition transition4 = new Transition(lordToil_Travel, lordToil_ExitMapTraderFighting, false, true);
			transition4.AddSources(new LordToil[]
			{
				lordToil_DefendTraderCaravan,
				lordToil_DefendTraderCaravan2,
				lordToil_ExitMapAndEscortCarriers,
				lordToil_ExitMap
			});
			transition4.AddTrigger(new Trigger_FractionPawnsLost(0.2f));
			transition4.AddPostAction(new TransitionAction_EndAllJobs());
			stateGraph.AddTransition(transition4);
			Transition transition5 = new Transition(lordToil_Travel, lordToil_DefendTraderCaravan, false, true);
			transition5.AddTrigger(new Trigger_PawnHarmed(1f, false, null));
			transition5.AddPreAction(new TransitionAction_SetDefendTrader());
			transition5.AddPostAction(new TransitionAction_WakeAll());
			transition5.AddPostAction(new TransitionAction_EndAllJobs());
			stateGraph.AddTransition(transition5);
			Transition transition6 = new Transition(lordToil_DefendTraderCaravan, lordToil_Travel, false, true);
			transition6.AddTrigger(new Trigger_TicksPassedWithoutHarm(1200));
			stateGraph.AddTransition(transition6);
			Transition transition7 = new Transition(lordToil_Travel, lordToil_DefendTraderCaravan2, false, true);
			transition7.AddTrigger(new Trigger_Memo("TravelArrived"));
			stateGraph.AddTransition(transition7);
			Transition transition8 = new Transition(lordToil_DefendTraderCaravan2, lordToil_ExitMapAndEscortCarriers, false, true);
			transition8.AddTrigger(new Trigger_TicksPassed((!DebugSettings.instantVisitorsGift) ? Rand.Range(27000, 45000) : 0));
			transition8.AddPreAction(new TransitionAction_CheckGiveGift());
			transition8.AddPreAction(new TransitionAction_Message("MessageTraderCaravanLeaving".Translate(new object[]
			{
				this.faction.Name
			}), null, 1f));
			transition8.AddPostAction(new TransitionAction_WakeAll());
			stateGraph.AddTransition(transition8);
			Transition transition9 = new Transition(lordToil_ExitMapAndEscortCarriers, lordToil_ExitMapAndEscortCarriers, true, true);
			transition9.canMoveToSameState = true;
			transition9.AddTrigger(new Trigger_PawnLost());
			transition9.AddTrigger(new Trigger_TickCondition(() => LordToil_ExitMapAndEscortCarriers.IsAnyDefendingPosition(this.lord.ownedPawns) && !GenHostility.AnyHostileActiveThreatTo(base.Map, this.faction), 60));
			stateGraph.AddTransition(transition9);
			Transition transition10 = new Transition(lordToil_ExitMapAndEscortCarriers, lordToil_ExitMap, false, true);
			transition10.AddTrigger(new Trigger_TicksPassed(60000));
			transition10.AddPostAction(new TransitionAction_WakeAll());
			stateGraph.AddTransition(transition10);
			Transition transition11 = new Transition(lordToil_DefendTraderCaravan2, lordToil_ExitMapAndEscortCarriers, false, true);
			transition11.AddSources(new LordToil[]
			{
				lordToil_Travel,
				lordToil_DefendTraderCaravan
			});
			transition11.AddTrigger(new Trigger_ImportantTraderCaravanPeopleLost());
			transition11.AddTrigger(new Trigger_BecamePlayerEnemy());
			transition11.AddPostAction(new TransitionAction_WakeAll());
			transition11.AddPostAction(new TransitionAction_EndAllJobs());
			stateGraph.AddTransition(transition11);
			return stateGraph;
		}

		// Token: 0x060007C0 RID: 1984 RVA: 0x0004BFA0 File Offset: 0x0004A3A0
		public override void ExposeData()
		{
			Scribe_References.Look<Faction>(ref this.faction, "faction", false);
			Scribe_Values.Look<IntVec3>(ref this.chillSpot, "chillSpot", default(IntVec3), false);
		}

		// Token: 0x04000361 RID: 865
		private Faction faction;

		// Token: 0x04000362 RID: 866
		private IntVec3 chillSpot;
	}
}
