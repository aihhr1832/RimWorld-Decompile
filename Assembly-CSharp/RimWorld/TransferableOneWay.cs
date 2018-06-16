﻿using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	// Token: 0x02000827 RID: 2087
	public class TransferableOneWay : Transferable
	{
		// Token: 0x17000774 RID: 1908
		// (get) Token: 0x06002ED5 RID: 11989 RVA: 0x0018F218 File Offset: 0x0018D618
		public override Thing AnyThing
		{
			get
			{
				return (!this.HasAnyThing) ? null : this.things[0];
			}
		}

		// Token: 0x17000775 RID: 1909
		// (get) Token: 0x06002ED6 RID: 11990 RVA: 0x0018F24C File Offset: 0x0018D64C
		public override ThingDef ThingDef
		{
			get
			{
				return (!this.HasAnyThing) ? null : this.AnyThing.def;
			}
		}

		// Token: 0x17000776 RID: 1910
		// (get) Token: 0x06002ED7 RID: 11991 RVA: 0x0018F280 File Offset: 0x0018D680
		public override bool HasAnyThing
		{
			get
			{
				return this.things.Count != 0;
			}
		}

		// Token: 0x17000777 RID: 1911
		// (get) Token: 0x06002ED8 RID: 11992 RVA: 0x0018F2A8 File Offset: 0x0018D6A8
		public override string Label
		{
			get
			{
				return this.AnyThing.LabelNoCount;
			}
		}

		// Token: 0x17000778 RID: 1912
		// (get) Token: 0x06002ED9 RID: 11993 RVA: 0x0018F2C8 File Offset: 0x0018D6C8
		public override bool Interactive
		{
			get
			{
				return true;
			}
		}

		// Token: 0x17000779 RID: 1913
		// (get) Token: 0x06002EDA RID: 11994 RVA: 0x0018F2E0 File Offset: 0x0018D6E0
		public override TransferablePositiveCountDirection PositiveCountDirection
		{
			get
			{
				return TransferablePositiveCountDirection.Destination;
			}
		}

		// Token: 0x1700077A RID: 1914
		// (get) Token: 0x06002EDB RID: 11995 RVA: 0x0018F2F8 File Offset: 0x0018D6F8
		public override string TipDescription
		{
			get
			{
				return (!this.HasAnyThing) ? "" : this.AnyThing.DescriptionDetailed;
			}
		}

		// Token: 0x1700077B RID: 1915
		// (get) Token: 0x06002EDC RID: 11996 RVA: 0x0018F330 File Offset: 0x0018D730
		// (set) Token: 0x06002EDD RID: 11997 RVA: 0x0018F34B File Offset: 0x0018D74B
		public override int CountToTransfer
		{
			get
			{
				return this.countToTransfer;
			}
			protected set
			{
				this.countToTransfer = value;
				base.EditBuffer = value.ToStringCached();
			}
		}

		// Token: 0x1700077C RID: 1916
		// (get) Token: 0x06002EDE RID: 11998 RVA: 0x0018F364 File Offset: 0x0018D764
		public int MaxCount
		{
			get
			{
				int num = 0;
				for (int i = 0; i < this.things.Count; i++)
				{
					num += this.things[i].stackCount;
				}
				return num;
			}
		}

		// Token: 0x06002EDF RID: 11999 RVA: 0x0018F3B0 File Offset: 0x0018D7B0
		public override int GetMinimumToTransfer()
		{
			return 0;
		}

		// Token: 0x06002EE0 RID: 12000 RVA: 0x0018F3C8 File Offset: 0x0018D7C8
		public override int GetMaximumToTransfer()
		{
			return this.MaxCount;
		}

		// Token: 0x06002EE1 RID: 12001 RVA: 0x0018F3E4 File Offset: 0x0018D7E4
		public override AcceptanceReport OverflowReport()
		{
			return new AcceptanceReport("ColonyHasNoMore".Translate());
		}

		// Token: 0x06002EE2 RID: 12002 RVA: 0x0018F408 File Offset: 0x0018D808
		public override void ExposeData()
		{
			base.ExposeData();
			if (Scribe.mode == LoadSaveMode.Saving)
			{
				this.things.RemoveAll((Thing x) => x.Destroyed);
			}
			Scribe_Values.Look<int>(ref this.countToTransfer, "countToTransfer", 0, false);
			Scribe_Collections.Look<Thing>(ref this.things, "things", LookMode.Reference, new object[0]);
			if (Scribe.mode == LoadSaveMode.LoadingVars)
			{
				base.EditBuffer = this.countToTransfer.ToStringCached();
			}
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				if (this.things.RemoveAll((Thing x) => x == null) != 0)
				{
					Log.Warning("Some of the things were null after loading.", false);
				}
			}
		}

		// Token: 0x04001909 RID: 6409
		public List<Thing> things = new List<Thing>();

		// Token: 0x0400190A RID: 6410
		private int countToTransfer;
	}
}
