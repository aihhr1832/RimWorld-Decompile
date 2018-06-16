﻿using System;
using Verse;

namespace RimWorld
{
	// Token: 0x02000518 RID: 1304
	public class Pawn_NativeVerbs : IExposable
	{
		// Token: 0x06001796 RID: 6038 RVA: 0x000CE633 File Offset: 0x000CCA33
		public Pawn_NativeVerbs(Pawn pawn)
		{
			this.pawn = pawn;
		}

		// Token: 0x17000340 RID: 832
		// (get) Token: 0x06001797 RID: 6039 RVA: 0x000CE644 File Offset: 0x000CCA44
		public Verb_BeatFire BeatFireVerb
		{
			get
			{
				if (this.beatFireVerb == null)
				{
					this.CreateVerbs();
				}
				return this.beatFireVerb;
			}
		}

		// Token: 0x17000341 RID: 833
		// (get) Token: 0x06001798 RID: 6040 RVA: 0x000CE670 File Offset: 0x000CCA70
		public Verb_Ignite IgniteVerb
		{
			get
			{
				if (this.igniteVerb == null)
				{
					this.CreateVerbs();
				}
				return this.igniteVerb;
			}
		}

		// Token: 0x06001799 RID: 6041 RVA: 0x000CE69C File Offset: 0x000CCA9C
		public void NativeVerbsTick()
		{
			if (this.BeatFireVerb != null)
			{
				this.BeatFireVerb.VerbTick();
			}
			if (this.IgniteVerb != null)
			{
				this.IgniteVerb.VerbTick();
			}
		}

		// Token: 0x0600179A RID: 6042 RVA: 0x000CE6CC File Offset: 0x000CCACC
		public bool TryStartIgnite(Thing target)
		{
			bool result;
			if (this.IgniteVerb == null)
			{
				Log.ErrorOnce(string.Concat(new object[]
				{
					this.pawn,
					" tried to ignite ",
					target,
					" but has no ignite verb."
				}), 76453432, false);
				result = false;
			}
			else
			{
				result = (!this.pawn.stances.FullBodyBusy && this.IgniteVerb.TryStartCastOn(target, false, true));
			}
			return result;
		}

		// Token: 0x0600179B RID: 6043 RVA: 0x000CE758 File Offset: 0x000CCB58
		public bool TryBeatFire(Fire targetFire)
		{
			bool result;
			if (this.BeatFireVerb == null)
			{
				Log.ErrorOnce(string.Concat(new object[]
				{
					this.pawn,
					" tried to beat fire ",
					targetFire,
					" but has no beat fire verb."
				}), 935137531, false);
				result = false;
			}
			else
			{
				result = (!this.pawn.stances.FullBodyBusy && this.BeatFireVerb.TryStartCastOn(targetFire, false, true));
			}
			return result;
		}

		// Token: 0x0600179C RID: 6044 RVA: 0x000CE7E2 File Offset: 0x000CCBE2
		public void ExposeData()
		{
			Scribe_Deep.Look<Verb_BeatFire>(ref this.beatFireVerb, "beatFireVerb", new object[0]);
			Scribe_Deep.Look<Verb_Ignite>(ref this.igniteVerb, "igniteVerb", new object[0]);
			if (Scribe.mode == LoadSaveMode.LoadingVars)
			{
				this.UpdateVerbsLinksAndProps();
			}
		}

		// Token: 0x0600179D RID: 6045 RVA: 0x000CE824 File Offset: 0x000CCC24
		private void CreateVerbs()
		{
			if (this.pawn.RaceProps.intelligence >= Intelligence.ToolUser)
			{
				UniqueIDsManager uniqueIDsManager = Find.UniqueIDsManager;
				this.beatFireVerb = new Verb_BeatFire();
				if (!this.pawn.RaceProps.IsMechanoid)
				{
					this.igniteVerb = new Verb_Ignite();
				}
				this.UpdateVerbsLinksAndProps();
			}
		}

		// Token: 0x0600179E RID: 6046 RVA: 0x000CE884 File Offset: 0x000CCC84
		private void UpdateVerbsLinksAndProps()
		{
			if (this.beatFireVerb != null)
			{
				this.beatFireVerb.caster = this.pawn;
				this.beatFireVerb.verbProps = NativeVerbPropertiesDatabase.VerbWithCategory(VerbCategory.BeatFire);
				this.beatFireVerb.loadID = VerbUtility.GenerateBeatFireLoadId(this.pawn);
			}
			if (this.igniteVerb != null)
			{
				this.igniteVerb.caster = this.pawn;
				this.igniteVerb.verbProps = NativeVerbPropertiesDatabase.VerbWithCategory(VerbCategory.Ignite);
				this.igniteVerb.loadID = VerbUtility.GenerateIgniteLoadId(this.pawn);
			}
		}

		// Token: 0x04000DED RID: 3565
		private Pawn pawn;

		// Token: 0x04000DEE RID: 3566
		private Verb_BeatFire beatFireVerb;

		// Token: 0x04000DEF RID: 3567
		private Verb_Ignite igniteVerb;
	}
}
