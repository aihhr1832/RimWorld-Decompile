﻿using System;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class CompHibernatable : ThingComp
	{
		private HibernatableStateDef state = HibernatableStateDefOf.Hibernating;

		private int endStartupTick = 0;

		public CompHibernatable()
		{
		}

		public CompProperties_Hibernatable Props
		{
			get
			{
				return (CompProperties_Hibernatable)this.props;
			}
		}

		public HibernatableStateDef State
		{
			get
			{
				return this.state;
			}
			set
			{
				if (this.state != value)
				{
					this.state = value;
					this.parent.Map.info.parent.Notify_HibernatableChanged();
				}
			}
		}

		public bool Running
		{
			get
			{
				return this.State == HibernatableStateDefOf.Running;
			}
		}

		public override void PostSpawnSetup(bool respawningAfterLoad)
		{
			base.PostSpawnSetup(respawningAfterLoad);
			if (!respawningAfterLoad)
			{
				this.parent.Map.info.parent.Notify_HibernatableChanged();
			}
		}

		public override void PostDeSpawn(Map map)
		{
			base.PostDeSpawn(map);
			map.info.parent.Notify_HibernatableChanged();
		}

		public void Startup()
		{
			if (this.State != HibernatableStateDefOf.Hibernating)
			{
				Log.ErrorOnce("Attempted to start a non-hibernating object", 34361223, false);
			}
			else
			{
				this.State = HibernatableStateDefOf.Starting;
				this.endStartupTick = Mathf.RoundToInt((float)Find.TickManager.TicksGame + this.Props.startupDays * 60000f);
			}
		}

		public override string CompInspectStringExtra()
		{
			string result;
			if (this.State == HibernatableStateDefOf.Hibernating)
			{
				result = "HibernatableHibernating".Translate();
			}
			else if (this.State == HibernatableStateDefOf.Starting)
			{
				result = string.Format("{0}: {1}", "HibernatableStartingUp".Translate(), (this.endStartupTick - Find.TickManager.TicksGame).ToStringTicksToPeriod());
			}
			else
			{
				result = null;
			}
			return result;
		}

		public override void CompTick()
		{
			if (this.State == HibernatableStateDefOf.Starting && Find.TickManager.TicksGame > this.endStartupTick)
			{
				this.State = HibernatableStateDefOf.Running;
				this.endStartupTick = 0;
				string text;
				if (this.parent.Map.Parent.GetComponent<EscapeShipComp>() != null)
				{
					text = "LetterHibernateComplete".Translate();
				}
				else
				{
					text = "LetterHibernateCompleteStandalone".Translate();
				}
				Find.LetterStack.ReceiveLetter("LetterLabelHibernateComplete".Translate(), text, LetterDefOf.PositiveEvent, new GlobalTargetInfo(this.parent), null, null);
			}
		}

		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Defs.Look<HibernatableStateDef>(ref this.state, "hibernateState");
			Scribe_Values.Look<int>(ref this.endStartupTick, "hibernateendStartupTick", 0, false);
		}
	}
}
