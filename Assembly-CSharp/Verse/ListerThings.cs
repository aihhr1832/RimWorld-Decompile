﻿using System;
using System.Collections.Generic;

namespace Verse
{
	public sealed class ListerThings
	{
		private Dictionary<ThingDef, List<Thing>> listsByDef = new Dictionary<ThingDef, List<Thing>>(ThingDefComparer.Instance);

		private List<Thing>[] listsByGroup;

		public ListerThingsUse use = ListerThingsUse.Undefined;

		private static readonly List<Thing> EmptyList = new List<Thing>();

		public ListerThings(ListerThingsUse use)
		{
			this.use = use;
			this.listsByGroup = new List<Thing>[ThingListGroupHelper.AllGroups.Length];
			this.listsByGroup[2] = new List<Thing>();
		}

		public List<Thing> AllThings
		{
			get
			{
				return this.listsByGroup[2];
			}
		}

		public List<Thing> ThingsInGroup(ThingRequestGroup group)
		{
			return this.ThingsMatching(ThingRequest.ForGroup(group));
		}

		public List<Thing> ThingsOfDef(ThingDef def)
		{
			return this.ThingsMatching(ThingRequest.ForDef(def));
		}

		public List<Thing> ThingsMatching(ThingRequest req)
		{
			List<Thing> result;
			if (req.singleDef != null)
			{
				List<Thing> list;
				if (!this.listsByDef.TryGetValue(req.singleDef, out list))
				{
					result = ListerThings.EmptyList;
				}
				else
				{
					result = list;
				}
			}
			else
			{
				if (req.group == ThingRequestGroup.Undefined)
				{
					throw new InvalidOperationException("Invalid ThingRequest " + req);
				}
				if (this.use == ListerThingsUse.Region && !req.group.StoreInRegion())
				{
					Log.ErrorOnce("Tried to get things in group " + req.group + " in a region, but this group is never stored in regions. Most likely a global query should have been used.", 1968735132, false);
					result = ListerThings.EmptyList;
				}
				else
				{
					List<Thing> list2 = this.listsByGroup[(int)req.group];
					result = (list2 ?? ListerThings.EmptyList);
				}
			}
			return result;
		}

		public bool Contains(Thing t)
		{
			return this.AllThings.Contains(t);
		}

		public void Add(Thing t)
		{
			if (ListerThings.EverListable(t.def, this.use))
			{
				List<Thing> list;
				if (!this.listsByDef.TryGetValue(t.def, out list))
				{
					list = new List<Thing>();
					this.listsByDef.Add(t.def, list);
				}
				list.Add(t);
				foreach (ThingRequestGroup thingRequestGroup in ThingListGroupHelper.AllGroups)
				{
					if (this.use != ListerThingsUse.Region || thingRequestGroup.StoreInRegion())
					{
						if (thingRequestGroup.Includes(t.def))
						{
							List<Thing> list2 = this.listsByGroup[(int)thingRequestGroup];
							if (list2 == null)
							{
								list2 = new List<Thing>();
								this.listsByGroup[(int)thingRequestGroup] = list2;
							}
							list2.Add(t);
						}
					}
				}
			}
		}

		public void Remove(Thing t)
		{
			if (ListerThings.EverListable(t.def, this.use))
			{
				this.listsByDef[t.def].Remove(t);
				ThingRequestGroup[] allGroups = ThingListGroupHelper.AllGroups;
				for (int i = 0; i < allGroups.Length; i++)
				{
					ThingRequestGroup group = allGroups[i];
					if (this.use != ListerThingsUse.Region || group.StoreInRegion())
					{
						if (group.Includes(t.def))
						{
							this.listsByGroup[i].Remove(t);
						}
					}
				}
			}
		}

		public static bool EverListable(ThingDef def, ListerThingsUse use)
		{
			return (def.category != ThingCategory.Mote || (def.drawGUIOverlay && use != ListerThingsUse.Region)) && (def.category != ThingCategory.Projectile || use != ListerThingsUse.Region) && def.category != ThingCategory.Gas;
		}

		public void Clear()
		{
			this.listsByDef.Clear();
			for (int i = 0; i < this.listsByGroup.Length; i++)
			{
				if (this.listsByGroup[i] != null)
				{
					this.listsByGroup[i].Clear();
				}
			}
		}

		// Note: this type is marked as 'beforefieldinit'.
		static ListerThings()
		{
		}
	}
}
