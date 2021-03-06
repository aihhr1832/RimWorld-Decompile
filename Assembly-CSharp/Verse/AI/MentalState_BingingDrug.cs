﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using RimWorld;

namespace Verse.AI
{
	public class MentalState_BingingDrug : MentalState_Binging
	{
		public ChemicalDef chemical;

		public DrugCategory drugCategory;

		private static List<ChemicalDef> addictions = new List<ChemicalDef>();

		public MentalState_BingingDrug()
		{
		}

		public override string InspectLine
		{
			get
			{
				return string.Format(base.InspectLine, this.chemical.label);
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Defs.Look<ChemicalDef>(ref this.chemical, "chemical");
			Scribe_Values.Look<DrugCategory>(ref this.drugCategory, "drugCategory", DrugCategory.None, false);
		}

		public override void PostStart(string reason)
		{
			base.PostStart(reason);
			this.ChooseRandomChemical();
			if (PawnUtility.ShouldSendNotificationAbout(this.pawn))
			{
				string label = "MentalBreakLetterLabel".Translate() + ": " + "LetterLabelDrugBinge".Translate(new object[]
				{
					this.chemical.label
				}).CapitalizeFirst();
				string text = "LetterDrugBinge".Translate(new object[]
				{
					this.pawn.Label,
					this.chemical.label
				}).CapitalizeFirst();
				if (reason != null)
				{
					text = text + "\n\n" + "FinalStraw".Translate(new object[]
					{
						reason.CapitalizeFirst()
					});
				}
				Find.LetterStack.ReceiveLetter(label, text, LetterDefOf.ThreatSmall, this.pawn, null, null);
			}
		}

		public override void PostEnd()
		{
			base.PostEnd();
			if (PawnUtility.ShouldSendNotificationAbout(this.pawn))
			{
				Messages.Message("MessageNoLongerBingingOnDrug".Translate(new object[]
				{
					this.pawn.LabelShort,
					this.chemical.label
				}), this.pawn, MessageTypeDefOf.SituationResolved, true);
			}
		}

		private void ChooseRandomChemical()
		{
			MentalState_BingingDrug.addictions.Clear();
			List<Hediff> hediffs = this.pawn.health.hediffSet.hediffs;
			for (int i = 0; i < hediffs.Count; i++)
			{
				Hediff_Addiction hediff_Addiction = hediffs[i] as Hediff_Addiction;
				if (hediff_Addiction != null && AddictionUtility.CanBingeOnNow(this.pawn, hediff_Addiction.Chemical, DrugCategory.Any))
				{
					MentalState_BingingDrug.addictions.Add(hediff_Addiction.Chemical);
				}
			}
			if (MentalState_BingingDrug.addictions.Count > 0)
			{
				this.chemical = MentalState_BingingDrug.addictions.RandomElement<ChemicalDef>();
				this.drugCategory = DrugCategory.Any;
				MentalState_BingingDrug.addictions.Clear();
			}
			else
			{
				this.chemical = (from x in DefDatabase<ChemicalDef>.AllDefsListForReading
				where AddictionUtility.CanBingeOnNow(this.pawn, x, this.def.drugCategory)
				select x).RandomElementWithFallback(null);
				if (this.chemical != null)
				{
					this.drugCategory = this.def.drugCategory;
				}
				else
				{
					this.chemical = (from x in DefDatabase<ChemicalDef>.AllDefsListForReading
					where AddictionUtility.CanBingeOnNow(this.pawn, x, DrugCategory.Any)
					select x).RandomElementWithFallback(null);
					if (this.chemical != null)
					{
						this.drugCategory = DrugCategory.Any;
					}
					else
					{
						this.chemical = DefDatabase<ChemicalDef>.AllDefsListForReading.RandomElement<ChemicalDef>();
						this.drugCategory = DrugCategory.Any;
					}
				}
			}
		}

		// Note: this type is marked as 'beforefieldinit'.
		static MentalState_BingingDrug()
		{
		}

		[CompilerGenerated]
		private bool <ChooseRandomChemical>m__0(ChemicalDef x)
		{
			return AddictionUtility.CanBingeOnNow(this.pawn, x, this.def.drugCategory);
		}

		[CompilerGenerated]
		private bool <ChooseRandomChemical>m__1(ChemicalDef x)
		{
			return AddictionUtility.CanBingeOnNow(this.pawn, x, DrugCategory.Any);
		}
	}
}
