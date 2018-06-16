﻿using System;
using System.Collections.Generic;
using RimWorld;
using Verse.AI;

namespace Verse
{
	// Token: 0x02000AA5 RID: 2725
	public class PriorityWork : IExposable
	{
		// Token: 0x06003CB6 RID: 15542 RVA: 0x0020214F File Offset: 0x0020054F
		public PriorityWork()
		{
		}

		// Token: 0x06003CB7 RID: 15543 RVA: 0x0020217A File Offset: 0x0020057A
		public PriorityWork(Pawn pawn)
		{
			this.pawn = pawn;
		}

		// Token: 0x17000931 RID: 2353
		// (get) Token: 0x06003CB8 RID: 15544 RVA: 0x002021AC File Offset: 0x002005AC
		public bool IsPrioritized
		{
			get
			{
				if (this.prioritizedCell.IsValid)
				{
					if (Find.TickManager.TicksGame < this.prioritizeTick + 30000)
					{
						return true;
					}
					this.Clear();
				}
				return false;
			}
		}

		// Token: 0x17000932 RID: 2354
		// (get) Token: 0x06003CB9 RID: 15545 RVA: 0x002021FC File Offset: 0x002005FC
		public IntVec3 Cell
		{
			get
			{
				return this.prioritizedCell;
			}
		}

		// Token: 0x17000933 RID: 2355
		// (get) Token: 0x06003CBA RID: 15546 RVA: 0x00202218 File Offset: 0x00200618
		public WorkTypeDef WorkType
		{
			get
			{
				return this.prioritizedWorkType;
			}
		}

		// Token: 0x06003CBB RID: 15547 RVA: 0x00202234 File Offset: 0x00200634
		public void ExposeData()
		{
			Scribe_Values.Look<IntVec3>(ref this.prioritizedCell, "prioritizedCell", default(IntVec3), false);
			Scribe_Defs.Look<WorkTypeDef>(ref this.prioritizedWorkType, "prioritizedWorkType");
			Scribe_Values.Look<int>(ref this.prioritizeTick, "prioritizeTick", 0, false);
		}

		// Token: 0x06003CBC RID: 15548 RVA: 0x0020227E File Offset: 0x0020067E
		public void Set(IntVec3 prioritizedCell, WorkTypeDef prioritizedWorkType)
		{
			this.prioritizedCell = prioritizedCell;
			this.prioritizedWorkType = prioritizedWorkType;
			this.prioritizeTick = Find.TickManager.TicksGame;
		}

		// Token: 0x06003CBD RID: 15549 RVA: 0x0020229F File Offset: 0x0020069F
		public void Clear()
		{
			this.prioritizedCell = IntVec3.Invalid;
			this.prioritizedWorkType = null;
			this.prioritizeTick = 0;
		}

		// Token: 0x06003CBE RID: 15550 RVA: 0x002022BB File Offset: 0x002006BB
		public void ClearPrioritizedWorkAndJobQueue()
		{
			this.Clear();
			this.pawn.jobs.ClearQueuedJobs();
		}

		// Token: 0x06003CBF RID: 15551 RVA: 0x002022D4 File Offset: 0x002006D4
		public IEnumerable<Gizmo> GetGizmos()
		{
			if ((this.IsPrioritized || (this.pawn.CurJob != null && this.pawn.CurJob.playerForced) || this.pawn.jobs.jobQueue.AnyPlayerForced) && !this.pawn.Drafted)
			{
				yield return new Command_Action
				{
					defaultLabel = "CommandClearPrioritizedWork".Translate(),
					defaultDesc = "CommandClearPrioritizedWorkDesc".Translate(),
					icon = TexCommand.ClearPrioritizedWork,
					activateSound = SoundDefOf.Tick_Low,
					action = delegate()
					{
						this.ClearPrioritizedWorkAndJobQueue();
						if (this.pawn.CurJob.playerForced)
						{
							this.pawn.jobs.EndCurrentJob(JobCondition.InterruptForced, true);
						}
					},
					hotKey = KeyBindingDefOf.Designator_Cancel,
					groupKey = 6165612
				};
			}
			yield break;
		}

		// Token: 0x04002672 RID: 9842
		private Pawn pawn;

		// Token: 0x04002673 RID: 9843
		private IntVec3 prioritizedCell = IntVec3.Invalid;

		// Token: 0x04002674 RID: 9844
		private WorkTypeDef prioritizedWorkType = null;

		// Token: 0x04002675 RID: 9845
		private int prioritizeTick = Find.TickManager.TicksGame;

		// Token: 0x04002676 RID: 9846
		private const int Timeout = 30000;
	}
}
