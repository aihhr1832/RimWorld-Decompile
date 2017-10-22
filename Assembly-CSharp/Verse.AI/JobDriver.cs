using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Verse.AI
{
	public abstract class JobDriver : IJobEndable, IExposable
	{
		public Pawn pawn;

		private List<Toil> toils = new List<Toil>();

		public List<Func<JobCondition>> globalFailConditions = new List<Func<JobCondition>>();

		public List<Action> globalFinishActions = new List<Action>();

		public bool ended;

		private int curToilIndex = -1;

		private ToilCompleteMode curToilCompleteMode;

		public int ticksLeftThisToil = 99999;

		private bool wantBeginNextToil;

		protected int startTick = -1;

		public TargetIndex rotateToFace = TargetIndex.A;

		public LayingDownState layingDown;

		public bool asleep;

		public float uninstallWorkLeft;

		public int debugTicksSpentThisToil;

		protected Toil CurToil
		{
			get
			{
				if (this.curToilIndex < 0)
				{
					return null;
				}
				if (this.curToilIndex >= this.toils.Count)
				{
					Log.Error(this.pawn + " with job " + this.pawn.CurJob + " tried to get CurToil with curToilIndex=" + this.curToilIndex + " but only has " + this.toils.Count + " toils.");
					return null;
				}
				return this.toils[this.curToilIndex];
			}
		}

		protected bool HaveCurToil
		{
			get
			{
				return this.curToilIndex >= 0 && this.curToilIndex < this.toils.Count;
			}
		}

		private bool CanStartNextToilInBusyStance
		{
			get
			{
				int num = this.curToilIndex + 1;
				if (num >= this.toils.Count)
				{
					return false;
				}
				return this.toils[num].atomicWithPrevious;
			}
		}

		public virtual PawnPosture Posture
		{
			get
			{
				return (PawnPosture)((this.layingDown != 0) ? 2 : 0);
			}
		}

		public int CurToilIndex
		{
			get
			{
				return this.curToilIndex;
			}
		}

		public bool HandlingFacing
		{
			get
			{
				return this.CurToil != null && this.CurToil.handlingFacing;
			}
		}

		protected Job CurJob
		{
			get
			{
				return (this.pawn.jobs == null) ? null : this.pawn.jobs.curJob;
			}
		}

		protected LocalTargetInfo TargetA
		{
			get
			{
				return this.pawn.jobs.curJob.targetA;
			}
		}

		protected LocalTargetInfo TargetB
		{
			get
			{
				return this.pawn.jobs.curJob.targetB;
			}
		}

		protected LocalTargetInfo TargetC
		{
			get
			{
				return this.pawn.jobs.curJob.targetC;
			}
		}

		protected Thing TargetThingA
		{
			get
			{
				return this.pawn.jobs.curJob.targetA.Thing;
			}
			set
			{
				this.pawn.jobs.curJob.targetA = value;
			}
		}

		protected Thing TargetThingB
		{
			get
			{
				return this.pawn.jobs.curJob.targetB.Thing;
			}
			set
			{
				this.pawn.jobs.curJob.targetB = value;
			}
		}

		protected IntVec3 TargetLocA
		{
			get
			{
				return this.pawn.jobs.curJob.targetA.Cell;
			}
		}

		protected Map Map
		{
			get
			{
				return this.pawn.Map;
			}
		}

		public virtual string GetReport()
		{
			return this.ReportStringProcessed(this.CurJob.def.reportString);
		}

		protected string ReportStringProcessed(string str)
		{
			Job curJob = this.CurJob;
			str = ((!curJob.targetA.HasThing) ? str.Replace("TargetA", "AreaLower".Translate()) : str.Replace("TargetA", curJob.targetA.Thing.LabelShort));
			str = ((!curJob.targetB.HasThing) ? str.Replace("TargetB", "AreaLower".Translate()) : str.Replace("TargetB", curJob.targetB.Thing.LabelShort));
			str = ((!curJob.targetC.HasThing) ? str.Replace("TargetC", "AreaLower".Translate()) : str.Replace("TargetC", curJob.targetC.Thing.LabelShort));
			return str;
		}

		protected abstract IEnumerable<Toil> MakeNewToils();

		public virtual void ExposeData()
		{
			Scribe_Values.Look<bool>(ref this.ended, "ended", false, false);
			Scribe_Values.Look<int>(ref this.curToilIndex, "curToilIndex", 0, true);
			Scribe_Values.Look<int>(ref this.ticksLeftThisToil, "ticksLeftThisToil", 0, false);
			Scribe_Values.Look<bool>(ref this.wantBeginNextToil, "wantBeginNextToil", false, false);
			Scribe_Values.Look<ToilCompleteMode>(ref this.curToilCompleteMode, "curToilCompleteMode", ToilCompleteMode.Undefined, false);
			Scribe_Values.Look<int>(ref this.startTick, "startTick", 0, false);
			Scribe_Values.Look<TargetIndex>(ref this.rotateToFace, "rotateToFace", TargetIndex.A, false);
			Scribe_Values.Look<LayingDownState>(ref this.layingDown, "layingDown", LayingDownState.NotLaying, false);
			Scribe_Values.Look<bool>(ref this.asleep, "asleep", false, false);
			Scribe_Values.Look<float>(ref this.uninstallWorkLeft, "uninstallWorkLeft", 0f, false);
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				this.SetupToils();
			}
		}

		public void Cleanup(JobCondition condition)
		{
			for (int i = 0; i < this.globalFinishActions.Count; i++)
			{
				try
				{
					this.globalFinishActions[i]();
				}
				catch (Exception ex)
				{
					Log.Error("Pawn " + this.pawn + " threw exception while executing a global finish action (" + i + "), jobDriver=" + base.GetType() + ": " + ex);
				}
			}
			if (this.HaveCurToil)
			{
				this.CurToil.Cleanup();
			}
		}

		internal void SetupToils()
		{
			try
			{
				this.toils.Clear();
				foreach (Toil item in this.MakeNewToils())
				{
					if (item.defaultCompleteMode == ToilCompleteMode.Undefined)
					{
						Log.Error("Toil has undefined complete mode.");
						item.defaultCompleteMode = ToilCompleteMode.Instant;
					}
					item.actor = this.pawn;
					this.toils.Add(item);
				}
			}
			catch (Exception ex)
			{
				this.pawn.jobs.StartErrorRecoverJob("Exception in SetupToils (pawn=" + this.pawn + ", job=" + this.CurJob + "): " + ex);
			}
		}

		public void DriverTick()
		{
			try
			{
				this.ticksLeftThisToil--;
				this.debugTicksSpentThisToil++;
				if (this.CurToil == null)
				{
					if (!this.pawn.stances.FullBodyBusy || this.CanStartNextToilInBusyStance)
					{
						this.ReadyForNextToil();
					}
				}
				else if (!this.CheckCurrentToilEndOrFail())
				{
					if (this.curToilCompleteMode == ToilCompleteMode.Delay)
					{
						if (this.ticksLeftThisToil <= 0)
						{
							this.ReadyForNextToil();
							goto end_IL_0000;
						}
					}
					else if (this.curToilCompleteMode == ToilCompleteMode.FinishedBusy && !this.pawn.stances.FullBodyBusy)
					{
						this.ReadyForNextToil();
						goto end_IL_0000;
					}
					if (this.wantBeginNextToil)
					{
						this.TryActuallyStartNextToil();
					}
					else if (this.curToilCompleteMode == ToilCompleteMode.Instant && this.debugTicksSpentThisToil > 300)
					{
						Log.Error(this.pawn + " had to be broken from frozen state. He was doing job " + this.CurJob + ", toilindex=" + this.curToilIndex);
						this.ReadyForNextToil();
					}
					else
					{
						Job curJob = this.CurJob;
						if (this.CurToil.preTickActions != null)
						{
							Toil curToil = this.CurToil;
							int num = 0;
							while (num < curToil.preTickActions.Count)
							{
								curToil.preTickActions[num]();
								if (this.CurJob == curJob && this.CurToil == curToil && !this.wantBeginNextToil)
								{
									num++;
									continue;
								}
								return;
							}
						}
						if ((object)this.CurToil.tickAction != null)
						{
							this.CurToil.tickAction();
						}
					}
				}
				end_IL_0000:;
			}
			catch (Exception ex)
			{
				this.pawn.jobs.StartErrorRecoverJob("Exception in Tick (pawn=" + this.pawn + ", job=" + this.CurJob + ", CurToil=" + this.curToilIndex + "): " + ex);
			}
		}

		public void ReadyForNextToil()
		{
			this.wantBeginNextToil = true;
			this.TryActuallyStartNextToil();
		}

		private void TryActuallyStartNextToil()
		{
			if (this.pawn.Spawned && (!this.pawn.stances.FullBodyBusy || this.CanStartNextToilInBusyStance))
			{
				if (this.HaveCurToil)
				{
					this.CurToil.Cleanup();
				}
				this.curToilIndex++;
				this.wantBeginNextToil = false;
				if (!this.HaveCurToil)
				{
					if (this.pawn.stances != null && this.pawn.stances.curStance.StanceBusy)
					{
						Log.ErrorOnce(this.pawn + " ended job " + this.CurJob + " due to running out of toils during a busy stance.", 6453432);
					}
					this.EndJobWith(JobCondition.Succeeded);
				}
				else
				{
					this.debugTicksSpentThisToil = 0;
					this.ticksLeftThisToil = this.CurToil.defaultDuration;
					this.curToilCompleteMode = this.CurToil.defaultCompleteMode;
					if (!this.CheckCurrentToilEndOrFail())
					{
						int num = this.CurToilIndex;
						if (this.CurToil.preInitActions != null)
						{
							int num2 = 0;
							while (num2 < this.CurToil.preInitActions.Count)
							{
								this.CurToil.preInitActions[num2]();
								if (this.CurToilIndex == num)
								{
									num2++;
									continue;
								}
								break;
							}
						}
						if (this.CurToilIndex == num)
						{
							if ((object)this.CurToil.initAction != null)
							{
								try
								{
									this.CurToil.initAction();
								}
								catch (Exception ex)
								{
									this.pawn.jobs.StartErrorRecoverJob("JobDriver threw exception in initAction. Pawn=" + this.pawn + ", Job=" + this.CurJob + ", Exception: " + ex);
									return;
									IL_01e4:;
								}
							}
							if (this.CurToilIndex == num && !this.ended && this.curToilCompleteMode == ToilCompleteMode.Instant)
							{
								this.ReadyForNextToil();
							}
						}
					}
				}
			}
		}

		public void EndJobWith(JobCondition condition)
		{
			if (condition == JobCondition.Ongoing)
			{
				Log.Warning("Ending a job with Ongoing as the condition. This makes no sense.");
			}
			if (!this.pawn.Destroyed)
			{
				this.pawn.jobs.EndCurrentJob(condition, true);
			}
		}

		private bool CheckCurrentToilEndOrFail()
		{
			Toil curToil = this.CurToil;
			if (this.globalFailConditions != null)
			{
				for (int i = 0; i < this.globalFailConditions.Count; i++)
				{
					JobCondition jobCondition = this.globalFailConditions[i]();
					if (jobCondition != JobCondition.Ongoing)
					{
						this.EndJobWith(jobCondition);
						return true;
					}
				}
			}
			if (curToil != null && curToil.endConditions != null)
			{
				for (int j = 0; j < curToil.endConditions.Count; j++)
				{
					JobCondition jobCondition2 = curToil.endConditions[j]();
					if (jobCondition2 != JobCondition.Ongoing)
					{
						this.EndJobWith(jobCondition2);
						return true;
					}
				}
			}
			return false;
		}

		private void SetNextToil(Toil to)
		{
			this.curToilIndex = this.toils.IndexOf(to) - 1;
		}

		public void JumpToToil(Toil to)
		{
			this.SetNextToil(to);
			this.ReadyForNextToil();
		}

		public virtual void Notify_Starting()
		{
			this.startTick = Find.TickManager.TicksGame;
		}

		public virtual void Notify_LastPosture(PawnPosture posture, LayingDownState layingDown)
		{
		}

		public virtual void Notify_PatherArrived()
		{
			if (this.curToilCompleteMode == ToilCompleteMode.PatherArrival)
			{
				this.ReadyForNextToil();
			}
		}

		public virtual void Notify_PatherFailed()
		{
			this.EndJobWith(JobCondition.ErroredPather);
		}

		public virtual void Notify_StanceChanged()
		{
		}

		public Pawn GetActor()
		{
			return this.pawn;
		}

		public void AddEndCondition(Func<JobCondition> newEndCondition)
		{
			this.globalFailConditions.Add(newEndCondition);
		}

		public void AddFailCondition(Func<bool> newFailCondition)
		{
			this.globalFailConditions.Add((Func<JobCondition>)delegate()
			{
				if (newFailCondition())
				{
					return JobCondition.Incompletable;
				}
				return JobCondition.Ongoing;
			});
		}

		public void AddFinishAction(Action newAct)
		{
			this.globalFinishActions.Add(newAct);
		}

		public virtual bool ModifyCarriedThingDrawPos(ref Vector3 drawPos, ref bool behind, ref bool flip)
		{
			return false;
		}

		public virtual RandomSocialMode DesiredSocialMode()
		{
			if (this.CurToil != null)
			{
				return this.CurToil.socialMode;
			}
			return RandomSocialMode.Normal;
		}

		public virtual bool IsContinuation(Job j)
		{
			return true;
		}
	}
}
