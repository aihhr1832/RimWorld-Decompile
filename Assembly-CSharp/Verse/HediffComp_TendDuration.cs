﻿using System;
using System.Text;
using RimWorld;
using UnityEngine;

namespace Verse
{
	[StaticConstructorOnStartup]
	public class HediffComp_TendDuration : HediffComp_SeverityPerDay
	{
		public int tendTick = -999999;

		public float tendQuality = 0f;

		private float totalTendQuality = 0f;

		public const float TendQualityRandomVariance = 0.25f;

		private static readonly Color UntendedColor;

		private static readonly Texture2D TendedIcon_Need_General;

		private static readonly Texture2D TendedIcon_Well_General;

		private static readonly Texture2D TendedIcon_Well_Injury;

		public HediffComp_TendDuration()
		{
		}

		public HediffCompProperties_TendDuration TProps
		{
			get
			{
				return (HediffCompProperties_TendDuration)this.props;
			}
		}

		private int FullTendDurationTicks
		{
			get
			{
				return Mathf.RoundToInt((this.TProps.baseTendDurationHours + this.TProps.tendOverlapHours) * 2500f);
			}
		}

		private int BaseTendDurationTicks
		{
			get
			{
				return Mathf.RoundToInt(this.TProps.baseTendDurationHours * 2500f);
			}
		}

		public override bool CompShouldRemove
		{
			get
			{
				return base.CompShouldRemove || (this.TProps.disappearsAtTotalTendQuality >= 0 && this.totalTendQuality >= (float)this.TProps.disappearsAtTotalTendQuality);
			}
		}

		public bool IsTended
		{
			get
			{
				bool result;
				if (Current.ProgramState != ProgramState.Playing)
				{
					result = false;
				}
				else if (this.TProps.baseTendDurationHours > 0f)
				{
					result = (Find.TickManager.TicksGame <= this.tendTick + this.FullTendDurationTicks);
				}
				else
				{
					result = (this.tendTick > 0);
				}
				return result;
			}
		}

		public bool AllowTend
		{
			get
			{
				bool result;
				if (this.TProps.baseTendDurationHours > 0f)
				{
					result = (Find.TickManager.TicksGame > this.tendTick + this.BaseTendDurationTicks);
				}
				else
				{
					result = !this.IsTended;
				}
				return result;
			}
		}

		public override string CompTipStringExtra
		{
			get
			{
				string result;
				if (this.parent.IsPermanent())
				{
					result = null;
				}
				else
				{
					StringBuilder stringBuilder = new StringBuilder();
					if (!this.IsTended)
					{
						if (!base.Pawn.Dead && this.parent.TendableNow(false))
						{
							stringBuilder.AppendLine("NeedsTendingNow".Translate());
						}
					}
					else
					{
						if (this.TProps.showTendQuality)
						{
							string text;
							if (this.parent.Part != null && this.parent.Part.def.IsSolid(this.parent.Part, base.Pawn.health.hediffSet.hediffs))
							{
								text = this.TProps.labelSolidTendedWell;
							}
							else if (this.parent.Part != null && this.parent.Part.depth == BodyPartDepth.Inside)
							{
								text = this.TProps.labelTendedWellInner;
							}
							else
							{
								text = this.TProps.labelTendedWell;
							}
							if (text != null)
							{
								stringBuilder.AppendLine(string.Concat(new string[]
								{
									text.CapitalizeFirst(),
									" (",
									"Quality".Translate().ToLower(),
									" ",
									this.tendQuality.ToStringPercent("F0"),
									")"
								}));
							}
							else
							{
								stringBuilder.AppendLine(string.Format("{0}: {1}", "TendQuality".Translate(), this.tendQuality.ToStringPercent()));
							}
						}
						if (!base.Pawn.Dead && this.TProps.baseTendDurationHours > 0f && this.parent.TendableNow(true))
						{
							int num = this.tendTick + this.BaseTendDurationTicks - Find.TickManager.TicksGame;
							int numTicks = this.tendTick + this.FullTendDurationTicks - Find.TickManager.TicksGame;
							if (num < 0)
							{
								stringBuilder.AppendLine("CanTendNow".Translate());
							}
							else if ("NextTendIn".CanTranslate())
							{
								stringBuilder.AppendLine("NextTendIn".Translate(new object[]
								{
									num.ToStringTicksToPeriod()
								}));
							}
							else
							{
								stringBuilder.AppendLine("NextTreatmentIn".Translate(new object[]
								{
									num.ToStringTicksToPeriod()
								}));
							}
							stringBuilder.AppendLine("TreatmentExpiresIn".Translate(new object[]
							{
								numTicks.ToStringTicksToPeriod()
							}));
						}
					}
					result = stringBuilder.ToString().TrimEndNewlines();
				}
				return result;
			}
		}

		public override TextureAndColor CompStateIcon
		{
			get
			{
				if (this.parent is Hediff_Injury)
				{
					if (this.IsTended && !this.parent.IsPermanent())
					{
						Color color = Color.Lerp(HediffComp_TendDuration.UntendedColor, Color.white, Mathf.Clamp01(this.tendQuality));
						return new TextureAndColor(HediffComp_TendDuration.TendedIcon_Well_Injury, color);
					}
				}
				else if (!(this.parent is Hediff_MissingPart))
				{
					if (!this.parent.FullyImmune())
					{
						if (this.IsTended)
						{
							Color color2 = Color.Lerp(HediffComp_TendDuration.UntendedColor, Color.white, Mathf.Clamp01(this.tendQuality));
							return new TextureAndColor(HediffComp_TendDuration.TendedIcon_Well_General, color2);
						}
						return HediffComp_TendDuration.TendedIcon_Need_General;
					}
				}
				return TextureAndColor.None;
			}
		}

		public override void CompExposeData()
		{
			Scribe_Values.Look<int>(ref this.tendTick, "tendTick", -999999, false);
			Scribe_Values.Look<float>(ref this.tendQuality, "tendQuality", 0f, false);
			Scribe_Values.Look<float>(ref this.totalTendQuality, "totalTendQuality", 0f, false);
		}

		protected override float SeverityChangePerDay()
		{
			float result;
			if (this.IsTended)
			{
				result = this.TProps.severityPerDayTended * this.tendQuality;
			}
			else
			{
				result = 0f;
			}
			return result;
		}

		public override void CompTended(float quality, int batchPosition = 0)
		{
			this.tendQuality = Mathf.Clamp01(quality + Rand.Range(-0.25f, 0.25f));
			this.tendTick = Find.TickManager.TicksGame;
			this.totalTendQuality += this.tendQuality;
			if (batchPosition == 0 && base.Pawn.Spawned)
			{
				string text = string.Concat(new string[]
				{
					"TextMote_Tended".Translate(new object[]
					{
						this.parent.Label
					}).CapitalizeFirst(),
					"\n",
					"Quality".Translate(),
					" ",
					this.tendQuality.ToStringPercent()
				});
				MoteMaker.ThrowText(base.Pawn.DrawPos, base.Pawn.Map, text, Color.white, 3.65f);
			}
			base.Pawn.health.Notify_HediffChanged(this.parent);
		}

		public override string CompDebugString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			if (this.IsTended)
			{
				stringBuilder.AppendLine("tendQuality: " + this.tendQuality.ToStringPercent());
				if (this.TProps.baseTendDurationHours > 0f)
				{
					int num = Find.TickManager.TicksGame - this.tendTick;
					stringBuilder.AppendLine("ticks since tend: " + num);
					stringBuilder.AppendLine("full tend duration passed: " + ((float)num / (float)this.FullTendDurationTicks).ToStringPercent());
					stringBuilder.AppendLine("severity change per day: " + (this.TProps.severityPerDayTended * this.tendQuality).ToString());
				}
			}
			else
			{
				stringBuilder.AppendLine("untended");
			}
			if (this.TProps.disappearsAtTotalTendQuality >= 0)
			{
				stringBuilder.AppendLine(string.Concat(new object[]
				{
					"total tend quality: ",
					this.totalTendQuality.ToString("F2"),
					" / ",
					this.TProps.disappearsAtTotalTendQuality
				}));
			}
			return stringBuilder.ToString().Trim();
		}

		// Note: this type is marked as 'beforefieldinit'.
		static HediffComp_TendDuration()
		{
			ColorInt colorInt = new ColorInt(116, 101, 72);
			HediffComp_TendDuration.UntendedColor = colorInt.ToColor;
			HediffComp_TendDuration.TendedIcon_Need_General = ContentFinder<Texture2D>.Get("UI/Icons/Medical/TendedNeed", true);
			HediffComp_TendDuration.TendedIcon_Well_General = ContentFinder<Texture2D>.Get("UI/Icons/Medical/TendedWell", true);
			HediffComp_TendDuration.TendedIcon_Well_Injury = ContentFinder<Texture2D>.Get("UI/Icons/Medical/BandageWell", true);
		}
	}
}
