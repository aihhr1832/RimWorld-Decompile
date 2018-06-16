﻿using System;
using Verse;

namespace RimWorld
{
	// Token: 0x02000698 RID: 1688
	public class StorageSettings : IExposable
	{
		// Token: 0x060023BB RID: 9147 RVA: 0x00132562 File Offset: 0x00130962
		public StorageSettings()
		{
			this.filter = new ThingFilter(new Action(this.TryNotifyChanged));
		}

		// Token: 0x060023BC RID: 9148 RVA: 0x00132590 File Offset: 0x00130990
		public StorageSettings(IStoreSettingsParent owner) : this()
		{
			this.owner = owner;
			if (owner != null)
			{
				StorageSettings parentStoreSettings = owner.GetParentStoreSettings();
				if (parentStoreSettings != null)
				{
					this.priorityInt = parentStoreSettings.priorityInt;
				}
			}
		}

		// Token: 0x17000555 RID: 1365
		// (get) Token: 0x060023BD RID: 9149 RVA: 0x001325CC File Offset: 0x001309CC
		private IHaulDestination HaulDestinationOwner
		{
			get
			{
				return this.owner as IHaulDestination;
			}
		}

		// Token: 0x17000556 RID: 1366
		// (get) Token: 0x060023BE RID: 9150 RVA: 0x001325EC File Offset: 0x001309EC
		private ISlotGroupParent SlotGroupParentOwner
		{
			get
			{
				return this.owner as ISlotGroupParent;
			}
		}

		// Token: 0x17000557 RID: 1367
		// (get) Token: 0x060023BF RID: 9151 RVA: 0x0013260C File Offset: 0x00130A0C
		// (set) Token: 0x060023C0 RID: 9152 RVA: 0x00132628 File Offset: 0x00130A28
		public StoragePriority Priority
		{
			get
			{
				return this.priorityInt;
			}
			set
			{
				this.priorityInt = value;
				if (Current.ProgramState == ProgramState.Playing && this.HaulDestinationOwner != null && this.HaulDestinationOwner.Map != null)
				{
					this.HaulDestinationOwner.Map.haulDestinationManager.Notify_HaulDestinationChangedPriority();
				}
				if (Current.ProgramState == ProgramState.Playing && this.SlotGroupParentOwner != null && this.SlotGroupParentOwner.Map != null)
				{
					this.SlotGroupParentOwner.Map.listerHaulables.RecalcAllInCells(this.SlotGroupParentOwner.AllSlotCells());
				}
			}
		}

		// Token: 0x060023C1 RID: 9153 RVA: 0x001326BE File Offset: 0x00130ABE
		public void ExposeData()
		{
			Scribe_Values.Look<StoragePriority>(ref this.priorityInt, "priority", StoragePriority.Unstored, false);
			Scribe_Deep.Look<ThingFilter>(ref this.filter, "filter", new object[0]);
		}

		// Token: 0x060023C2 RID: 9154 RVA: 0x001326E9 File Offset: 0x00130AE9
		public void SetFromPreset(StorageSettingsPreset preset)
		{
			this.filter.SetFromPreset(preset);
			this.TryNotifyChanged();
		}

		// Token: 0x060023C3 RID: 9155 RVA: 0x001326FE File Offset: 0x00130AFE
		public void CopyFrom(StorageSettings other)
		{
			this.Priority = other.Priority;
			this.filter.CopyAllowancesFrom(other.filter);
			this.TryNotifyChanged();
		}

		// Token: 0x060023C4 RID: 9156 RVA: 0x00132724 File Offset: 0x00130B24
		public bool AllowedToAccept(Thing t)
		{
			bool result;
			if (!this.filter.Allows(t))
			{
				result = false;
			}
			else
			{
				if (this.owner != null)
				{
					StorageSettings parentStoreSettings = this.owner.GetParentStoreSettings();
					if (parentStoreSettings != null && !parentStoreSettings.AllowedToAccept(t))
					{
						return false;
					}
				}
				result = true;
			}
			return result;
		}

		// Token: 0x060023C5 RID: 9157 RVA: 0x00132784 File Offset: 0x00130B84
		public bool AllowedToAccept(ThingDef t)
		{
			bool result;
			if (!this.filter.Allows(t))
			{
				result = false;
			}
			else
			{
				if (this.owner != null)
				{
					StorageSettings parentStoreSettings = this.owner.GetParentStoreSettings();
					if (parentStoreSettings != null && !parentStoreSettings.AllowedToAccept(t))
					{
						return false;
					}
				}
				result = true;
			}
			return result;
		}

		// Token: 0x060023C6 RID: 9158 RVA: 0x001327E4 File Offset: 0x00130BE4
		private void TryNotifyChanged()
		{
			if (this.owner != null && this.SlotGroupParentOwner != null && this.SlotGroupParentOwner.GetSlotGroup() != null && this.SlotGroupParentOwner.Map != null)
			{
				this.SlotGroupParentOwner.Map.listerHaulables.Notify_SlotGroupChanged(this.SlotGroupParentOwner.GetSlotGroup());
			}
		}

		// Token: 0x040013F7 RID: 5111
		public IStoreSettingsParent owner = null;

		// Token: 0x040013F8 RID: 5112
		public ThingFilter filter;

		// Token: 0x040013F9 RID: 5113
		[LoadAlias("priority")]
		private StoragePriority priorityInt = StoragePriority.Normal;
	}
}
