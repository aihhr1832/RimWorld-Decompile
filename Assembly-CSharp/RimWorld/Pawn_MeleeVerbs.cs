using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class Pawn_MeleeVerbs : IExposable
	{
		private const int BestMeleeVerbUpdateInterval = 60;

		private Pawn pawn;

		private Verb curMeleeVerb;

		private int curMeleeVerbUpdateTick;

		private static List<VerbEntry> meleeVerbs = new List<VerbEntry>();

		public Pawn_MeleeVerbs(Pawn pawn)
		{
			this.pawn = pawn;
		}

		public Verb TryGetMeleeVerb()
		{
			if (this.curMeleeVerb == null || Find.TickManager.TicksGame >= this.curMeleeVerbUpdateTick + 60 || !this.curMeleeVerb.IsStillUsableBy(this.pawn))
			{
				this.ChooseMeleeVerb();
			}
			return this.curMeleeVerb;
		}

		private void ChooseMeleeVerb()
		{
			List<VerbEntry> updatedAvailableVerbsList = this.GetUpdatedAvailableVerbsList();
			if (updatedAvailableVerbsList.Count == 0)
			{
				Log.ErrorOnce(string.Format("{0} has no available melee attack", this.pawn), 1664289);
				this.SetCurMeleeVerb(null);
			}
			else
			{
				VerbEntry verbEntry = updatedAvailableVerbsList.RandomElementByWeight((Func<VerbEntry, float>)((VerbEntry ve) => ve.SelectionWeight));
				this.SetCurMeleeVerb(verbEntry.verb);
			}
		}

		public bool TryMeleeAttack(Thing target, Verb verbToUse = null, bool surpriseAttack = false)
		{
			if (this.pawn.stances.FullBodyBusy)
			{
				return false;
			}
			if (verbToUse != null)
			{
				if (!verbToUse.IsStillUsableBy(this.pawn))
				{
					return false;
				}
				if (!(verbToUse is Verb_MeleeAttack))
				{
					Log.Warning("Pawn " + this.pawn + " tried to melee attack " + target + " with non melee-attack verb " + verbToUse + ".");
					return false;
				}
			}
			Verb verb = (verbToUse == null) ? this.TryGetMeleeVerb() : verbToUse;
			if (verb == null)
			{
				return false;
			}
			verb.TryStartCastOn(target, surpriseAttack, true);
			return true;
		}

		public List<VerbEntry> GetUpdatedAvailableVerbsList()
		{
			Pawn_MeleeVerbs.meleeVerbs.Clear();
			if (this.pawn.equipment != null && this.pawn.equipment.Primary != null)
			{
				Verb verb = this.pawn.equipment.PrimaryEq.AllVerbs.Find((Predicate<Verb>)((Verb x) => x is Verb_MeleeAttack));
				if (verb != null)
				{
					Pawn_MeleeVerbs.meleeVerbs.Add(new VerbEntry(verb, this.pawn, this.pawn.equipment.Primary));
					return Pawn_MeleeVerbs.meleeVerbs;
				}
			}
			List<Verb> allVerbs = this.pawn.verbTracker.AllVerbs;
			for (int i = 0; i < allVerbs.Count; i++)
			{
				if (allVerbs[i] is Verb_MeleeAttack && allVerbs[i].IsStillUsableBy(this.pawn))
				{
					Pawn_MeleeVerbs.meleeVerbs.Add(new VerbEntry(allVerbs[i], this.pawn, null));
				}
			}
			foreach (Verb hediffsVerb in this.pawn.health.hediffSet.GetHediffsVerbs())
			{
				if (hediffsVerb is Verb_MeleeAttack && hediffsVerb.IsStillUsableBy(this.pawn))
				{
					Pawn_MeleeVerbs.meleeVerbs.Add(new VerbEntry(hediffsVerb, this.pawn, null));
				}
			}
			return Pawn_MeleeVerbs.meleeVerbs;
		}

		public void Notify_PawnKilled()
		{
			this.SetCurMeleeVerb(null);
		}

		private void SetCurMeleeVerb(Verb v)
		{
			this.curMeleeVerb = v;
			if (Current.ProgramState != ProgramState.Playing)
			{
				this.curMeleeVerbUpdateTick = 0;
			}
			else
			{
				this.curMeleeVerbUpdateTick = Find.TickManager.TicksGame;
			}
		}

		public void ExposeData()
		{
			if (Scribe.mode == LoadSaveMode.Saving && this.curMeleeVerb != null && !this.curMeleeVerb.IsStillUsableBy(this.pawn))
			{
				this.curMeleeVerb = null;
			}
			Scribe_References.Look<Verb>(ref this.curMeleeVerb, "curMeleeVerb", false);
			Scribe_Values.Look<int>(ref this.curMeleeVerbUpdateTick, "curMeleeVerbUpdateTick", 0, false);
		}
	}
}
