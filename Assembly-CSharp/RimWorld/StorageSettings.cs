﻿using System;
using Verse;

namespace RimWorld
{
	public class StorageSettings : IExposable
	{
		public IStoreSettingsParent owner = null;

		public ThingFilter filter;

		[LoadAlias("priority")]
		private StoragePriority priorityInt = StoragePriority.Normal;

		public StorageSettings()
		{
			this.filter = new ThingFilter(new Action(this.TryNotifyChanged));
		}

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

		private IHaulDestination HaulDestinationOwner
		{
			get
			{
				return this.owner as IHaulDestination;
			}
		}

		private ISlotGroupParent SlotGroupParentOwner
		{
			get
			{
				return this.owner as ISlotGroupParent;
			}
		}

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

		public void ExposeData()
		{
			Scribe_Values.Look<StoragePriority>(ref this.priorityInt, "priority", StoragePriority.Unstored, false);
			Scribe_Deep.Look<ThingFilter>(ref this.filter, "filter", new object[0]);
		}

		public void SetFromPreset(StorageSettingsPreset preset)
		{
			this.filter.SetFromPreset(preset);
			this.TryNotifyChanged();
		}

		public void CopyFrom(StorageSettings other)
		{
			this.Priority = other.Priority;
			this.filter.CopyAllowancesFrom(other.filter);
			this.TryNotifyChanged();
		}

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

		private void TryNotifyChanged()
		{
			if (this.owner != null && this.SlotGroupParentOwner != null && this.SlotGroupParentOwner.GetSlotGroup() != null && this.SlotGroupParentOwner.Map != null)
			{
				this.SlotGroupParentOwner.Map.listerHaulables.Notify_SlotGroupChanged(this.SlotGroupParentOwner.GetSlotGroup());
			}
		}
	}
}
