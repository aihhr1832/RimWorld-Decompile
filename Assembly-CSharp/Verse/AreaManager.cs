﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using RimWorld;

namespace Verse
{
	public class AreaManager : IExposable
	{
		public Map map;

		private List<Area> areas = new List<Area>();

		public const int MaxAllowedAreas = 10;

		[CompilerGenerated]
		private static Comparison<Area> <>f__am$cache0;

		[CompilerGenerated]
		private static Func<Area, bool> <>f__am$cache1;

		public AreaManager(Map map)
		{
			this.map = map;
		}

		public List<Area> AllAreas
		{
			get
			{
				return this.areas;
			}
		}

		public Area_Home Home
		{
			get
			{
				return this.Get<Area_Home>();
			}
		}

		public Area_BuildRoof BuildRoof
		{
			get
			{
				return this.Get<Area_BuildRoof>();
			}
		}

		public Area_NoRoof NoRoof
		{
			get
			{
				return this.Get<Area_NoRoof>();
			}
		}

		public Area_SnowClear SnowClear
		{
			get
			{
				return this.Get<Area_SnowClear>();
			}
		}

		public void AddStartingAreas()
		{
			this.areas.Add(new Area_Home(this));
			this.areas.Add(new Area_BuildRoof(this));
			this.areas.Add(new Area_NoRoof(this));
			this.areas.Add(new Area_SnowClear(this));
			Area_Allowed area_Allowed;
			this.TryMakeNewAllowed(out area_Allowed);
		}

		public void ExposeData()
		{
			Scribe_Collections.Look<Area>(ref this.areas, "areas", LookMode.Deep, new object[0]);
			if (Scribe.mode == LoadSaveMode.LoadingVars)
			{
				this.UpdateAllAreasLinks();
			}
		}

		public void AreaManagerUpdate()
		{
			for (int i = 0; i < this.areas.Count; i++)
			{
				this.areas[i].AreaUpdate();
			}
		}

		internal void Remove(Area area)
		{
			if (!area.Mutable)
			{
				Log.Error("Tried to delete non-Deletable area " + area, false);
			}
			else
			{
				this.areas.Remove(area);
				this.NotifyEveryoneAreaRemoved(area);
				if (Designator_AreaAllowed.SelectedArea == area)
				{
					Designator_AreaAllowed.ClearSelectedArea();
				}
			}
		}

		public Area GetLabeled(string s)
		{
			for (int i = 0; i < this.areas.Count; i++)
			{
				if (this.areas[i].Label == s)
				{
					return this.areas[i];
				}
			}
			return null;
		}

		public T Get<T>() where T : Area
		{
			for (int i = 0; i < this.areas.Count; i++)
			{
				T t = this.areas[i] as T;
				if (t != null)
				{
					return t;
				}
			}
			return (T)((object)null);
		}

		private void SortAreas()
		{
			this.areas.InsertionSort((Area a, Area b) => b.ListPriority.CompareTo(a.ListPriority));
		}

		private void UpdateAllAreasLinks()
		{
			for (int i = 0; i < this.areas.Count; i++)
			{
				this.areas[i].areaManager = this;
			}
		}

		private void NotifyEveryoneAreaRemoved(Area area)
		{
			foreach (Pawn pawn in PawnsFinder.All_AliveOrDead)
			{
				if (pawn.playerSettings != null)
				{
					pawn.playerSettings.Notify_AreaRemoved(area);
				}
			}
		}

		public void Notify_MapRemoved()
		{
			for (int i = 0; i < this.areas.Count; i++)
			{
				this.NotifyEveryoneAreaRemoved(this.areas[i]);
			}
		}

		public bool CanMakeNewAllowed()
		{
			return (from a in this.areas
			where a is Area_Allowed
			select a).Count<Area>() < 10;
		}

		public bool TryMakeNewAllowed(out Area_Allowed area)
		{
			bool result;
			if (!this.CanMakeNewAllowed())
			{
				area = null;
				result = false;
			}
			else
			{
				area = new Area_Allowed(this, null);
				this.areas.Add(area);
				this.SortAreas();
				result = true;
			}
			return result;
		}

		[CompilerGenerated]
		private static int <SortAreas>m__0(Area a, Area b)
		{
			return b.ListPriority.CompareTo(a.ListPriority);
		}

		[CompilerGenerated]
		private static bool <CanMakeNewAllowed>m__1(Area a)
		{
			return a is Area_Allowed;
		}
	}
}
