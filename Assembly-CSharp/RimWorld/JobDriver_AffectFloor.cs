﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public abstract class JobDriver_AffectFloor : JobDriver
	{
		private float workLeft = -1000f;

		protected bool clearSnow = false;

		protected JobDriver_AffectFloor()
		{
		}

		protected abstract int BaseWorkAmount { get; }

		protected abstract DesignationDef DesDef { get; }

		protected virtual StatDef SpeedStat
		{
			get
			{
				return null;
			}
		}

		public override bool TryMakePreToilReservations()
		{
			Pawn pawn = this.pawn;
			LocalTargetInfo targetA = this.job.targetA;
			Job job = this.job;
			ReservationLayerDef floor = ReservationLayerDefOf.Floor;
			return pawn.Reserve(targetA, job, 1, -1, floor);
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOn(() => !this.job.ignoreDesignations && this.Map.designationManager.DesignationAt(this.TargetLocA, this.DesDef) == null);
			yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.Touch);
			Toil doWork = new Toil();
			doWork.initAction = delegate()
			{
				this.workLeft = (float)this.BaseWorkAmount;
			};
			doWork.tickAction = delegate()
			{
				float num = (this.SpeedStat == null) ? 1f : doWork.actor.GetStatValue(this.SpeedStat, true);
				this.workLeft -= num;
				if (doWork.actor.skills != null)
				{
					doWork.actor.skills.Learn(SkillDefOf.Construction, 0.11f, false);
				}
				if (this.clearSnow)
				{
					this.Map.snowGrid.SetDepth(this.TargetLocA, 0f);
				}
				if (this.workLeft <= 0f)
				{
					this.DoEffect(this.TargetLocA);
					Designation designation = this.Map.designationManager.DesignationAt(this.TargetLocA, this.DesDef);
					if (designation != null)
					{
						designation.Delete();
					}
					this.ReadyForNextToil();
				}
			};
			doWork.FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
			doWork.WithProgressBar(TargetIndex.A, () => 1f - this.workLeft / (float)this.BaseWorkAmount, false, -0.5f);
			doWork.defaultCompleteMode = ToilCompleteMode.Never;
			doWork.activeSkill = (() => SkillDefOf.Construction);
			yield return doWork;
			yield break;
		}

		protected abstract void DoEffect(IntVec3 c);

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look<float>(ref this.workLeft, "workLeft", 0f, false);
		}

		[CompilerGenerated]
		private sealed class <MakeNewToils>c__Iterator0 : IEnumerable, IEnumerable<Toil>, IEnumerator, IDisposable, IEnumerator<Toil>
		{
			internal JobDriver_AffectFloor $this;

			internal Toil $current;

			internal bool $disposing;

			internal int $PC;

			private JobDriver_AffectFloor.<MakeNewToils>c__Iterator0.<MakeNewToils>c__AnonStorey1 $locvar0;

			private static Func<SkillDef> <>f__am$cache0;

			[DebuggerHidden]
			public <MakeNewToils>c__Iterator0()
			{
			}

			public bool MoveNext()
			{
				uint num = (uint)this.$PC;
				this.$PC = -1;
				switch (num)
				{
				case 0u:
					this.FailOn(() => !this.job.ignoreDesignations && this.Map.designationManager.DesignationAt(this.TargetLocA, this.DesDef) == null);
					this.$current = Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.Touch);
					if (!this.$disposing)
					{
						this.$PC = 1;
					}
					return true;
				case 1u:
					<MakeNewToils>c__AnonStorey.doWork = new Toil();
					<MakeNewToils>c__AnonStorey.doWork.initAction = delegate()
					{
						<MakeNewToils>c__AnonStorey.<>f__ref$0.$this.workLeft = (float)<MakeNewToils>c__AnonStorey.<>f__ref$0.$this.BaseWorkAmount;
					};
					<MakeNewToils>c__AnonStorey.doWork.tickAction = delegate()
					{
						float num2 = (<MakeNewToils>c__AnonStorey.<>f__ref$0.$this.SpeedStat == null) ? 1f : <MakeNewToils>c__AnonStorey.doWork.actor.GetStatValue(<MakeNewToils>c__AnonStorey.<>f__ref$0.$this.SpeedStat, true);
						<MakeNewToils>c__AnonStorey.<>f__ref$0.$this.workLeft -= num2;
						if (<MakeNewToils>c__AnonStorey.doWork.actor.skills != null)
						{
							<MakeNewToils>c__AnonStorey.doWork.actor.skills.Learn(SkillDefOf.Construction, 0.11f, false);
						}
						if (<MakeNewToils>c__AnonStorey.<>f__ref$0.$this.clearSnow)
						{
							<MakeNewToils>c__AnonStorey.<>f__ref$0.$this.Map.snowGrid.SetDepth(<MakeNewToils>c__AnonStorey.<>f__ref$0.$this.TargetLocA, 0f);
						}
						if (<MakeNewToils>c__AnonStorey.<>f__ref$0.$this.workLeft <= 0f)
						{
							<MakeNewToils>c__AnonStorey.<>f__ref$0.$this.DoEffect(<MakeNewToils>c__AnonStorey.<>f__ref$0.$this.TargetLocA);
							Designation designation = <MakeNewToils>c__AnonStorey.<>f__ref$0.$this.Map.designationManager.DesignationAt(<MakeNewToils>c__AnonStorey.<>f__ref$0.$this.TargetLocA, <MakeNewToils>c__AnonStorey.<>f__ref$0.$this.DesDef);
							if (designation != null)
							{
								designation.Delete();
							}
							<MakeNewToils>c__AnonStorey.<>f__ref$0.$this.ReadyForNextToil();
						}
					};
					<MakeNewToils>c__AnonStorey.doWork.FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
					<MakeNewToils>c__AnonStorey.doWork.WithProgressBar(TargetIndex.A, () => 1f - <MakeNewToils>c__AnonStorey.<>f__ref$0.$this.workLeft / (float)<MakeNewToils>c__AnonStorey.<>f__ref$0.$this.BaseWorkAmount, false, -0.5f);
					<MakeNewToils>c__AnonStorey.doWork.defaultCompleteMode = ToilCompleteMode.Never;
					<MakeNewToils>c__AnonStorey.doWork.activeSkill = (() => SkillDefOf.Construction);
					this.$current = <MakeNewToils>c__AnonStorey.doWork;
					if (!this.$disposing)
					{
						this.$PC = 2;
					}
					return true;
				case 2u:
					this.$PC = -1;
					break;
				}
				return false;
			}

			Toil IEnumerator<Toil>.Current
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
				this.$disposing = true;
				this.$PC = -1;
			}

			[DebuggerHidden]
			public void Reset()
			{
				throw new NotSupportedException();
			}

			[DebuggerHidden]
			IEnumerator IEnumerable.GetEnumerator()
			{
				return this.System.Collections.Generic.IEnumerable<Verse.AI.Toil>.GetEnumerator();
			}

			[DebuggerHidden]
			IEnumerator<Toil> IEnumerable<Toil>.GetEnumerator()
			{
				if (Interlocked.CompareExchange(ref this.$PC, 0, -2) == -2)
				{
					return this;
				}
				JobDriver_AffectFloor.<MakeNewToils>c__Iterator0 <MakeNewToils>c__Iterator = new JobDriver_AffectFloor.<MakeNewToils>c__Iterator0();
				<MakeNewToils>c__Iterator.$this = this;
				return <MakeNewToils>c__Iterator;
			}

			private static SkillDef <>m__0()
			{
				return SkillDefOf.Construction;
			}

			private sealed class <MakeNewToils>c__AnonStorey1
			{
				internal Toil doWork;

				internal JobDriver_AffectFloor.<MakeNewToils>c__Iterator0 <>f__ref$0;

				public <MakeNewToils>c__AnonStorey1()
				{
				}

				internal bool <>m__0()
				{
					return !this.<>f__ref$0.$this.job.ignoreDesignations && this.<>f__ref$0.$this.Map.designationManager.DesignationAt(this.<>f__ref$0.$this.TargetLocA, this.<>f__ref$0.$this.DesDef) == null;
				}

				internal void <>m__1()
				{
					this.<>f__ref$0.$this.workLeft = (float)this.<>f__ref$0.$this.BaseWorkAmount;
				}

				internal void <>m__2()
				{
					float num = (this.<>f__ref$0.$this.SpeedStat == null) ? 1f : this.doWork.actor.GetStatValue(this.<>f__ref$0.$this.SpeedStat, true);
					this.<>f__ref$0.$this.workLeft -= num;
					if (this.doWork.actor.skills != null)
					{
						this.doWork.actor.skills.Learn(SkillDefOf.Construction, 0.11f, false);
					}
					if (this.<>f__ref$0.$this.clearSnow)
					{
						this.<>f__ref$0.$this.Map.snowGrid.SetDepth(this.<>f__ref$0.$this.TargetLocA, 0f);
					}
					if (this.<>f__ref$0.$this.workLeft <= 0f)
					{
						this.<>f__ref$0.$this.DoEffect(this.<>f__ref$0.$this.TargetLocA);
						Designation designation = this.<>f__ref$0.$this.Map.designationManager.DesignationAt(this.<>f__ref$0.$this.TargetLocA, this.<>f__ref$0.$this.DesDef);
						if (designation != null)
						{
							designation.Delete();
						}
						this.<>f__ref$0.$this.ReadyForNextToil();
					}
				}

				internal float <>m__3()
				{
					return 1f - this.<>f__ref$0.$this.workLeft / (float)this.<>f__ref$0.$this.BaseWorkAmount;
				}
			}
		}
	}
}
