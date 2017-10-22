using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Verse
{
	public sealed class Region
	{
		public const int GridSize = 12;

		public RegionType type = RegionType.Normal;

		public int id = -1;

		public sbyte mapIndex = (sbyte)(-1);

		private Room roomInt;

		public List<RegionLink> links = new List<RegionLink>();

		public CellRect extentsClose;

		public CellRect extentsLimit;

		public Building_Door portal;

		private int precalculatedHashCode;

		public bool touchesMapEdge;

		private int cachedCellCount = -1;

		public bool valid = true;

		private ListerThings listerThings = new ListerThings(ListerThingsUse.Region);

		public uint[] closedIndex = new uint[RegionTraverser.NumWorkers];

		public uint reachedIndex;

		public int newRegionGroupIndex = -1;

		private Dictionary<Area, AreaOverlap> cachedAreaOverlaps;

		public int mark;

		private int debug_makeTick = -1000;

		private int debug_lastTraverseTick = -1000;

		private static int nextId = 1;

		public Map Map
		{
			get
			{
				return (this.mapIndex >= 0) ? Find.Maps[this.mapIndex] : null;
			}
		}

		public IEnumerable<IntVec3> Cells
		{
			get
			{
				RegionGrid regions = this.Map.regionGrid;
				for (int z = this.extentsClose.minZ; z <= this.extentsClose.maxZ; z++)
				{
					for (int x = this.extentsClose.minX; x <= this.extentsClose.maxX; x++)
					{
						IntVec3 c = new IntVec3(x, 0, z);
						if (regions.GetRegionAt_NoRebuild_InvalidAllowed(c) == this)
						{
							yield return c;
						}
					}
				}
			}
		}

		public int CellCount
		{
			get
			{
				if (this.cachedCellCount == -1)
				{
					this.cachedCellCount = this.Cells.Count();
				}
				return this.cachedCellCount;
			}
		}

		public IEnumerable<Region> Neighbors
		{
			get
			{
				for (int li = 0; li < this.links.Count; li++)
				{
					RegionLink link = this.links[li];
					for (int ri = 0; ri < 2; ri++)
					{
						if (link.regions[ri] != null && link.regions[ri] != this && link.regions[ri].valid)
						{
							yield return link.regions[ri];
						}
					}
				}
			}
		}

		public IEnumerable<Region> NeighborsOfSameType
		{
			get
			{
				for (int li = 0; li < this.links.Count; li++)
				{
					RegionLink link = this.links[li];
					for (int ri = 0; ri < 2; ri++)
					{
						if (link.regions[ri] != null && link.regions[ri] != this && link.regions[ri].type == this.type && link.regions[ri].valid)
						{
							yield return link.regions[ri];
						}
					}
				}
			}
		}

		public Room Room
		{
			get
			{
				return this.roomInt;
			}
			set
			{
				if (value != this.roomInt)
				{
					if (this.roomInt != null)
					{
						this.roomInt.RemoveRegion(this);
					}
					this.roomInt = value;
					if (this.roomInt != null)
					{
						this.roomInt.AddRegion(this);
					}
				}
			}
		}

		public IntVec3 RandomCell
		{
			get
			{
				Map map = this.Map;
				CellIndices cellIndices = map.cellIndices;
				Region[] directGrid = map.regionGrid.DirectGrid;
				for (int i = 0; i < 1000; i++)
				{
					IntVec3 randomCell = this.extentsClose.RandomCell;
					if (directGrid[cellIndices.CellToIndex(randomCell)] == this)
					{
						return randomCell;
					}
				}
				return this.AnyCell;
			}
		}

		public IntVec3 AnyCell
		{
			get
			{
				Map map = this.Map;
				CellIndices cellIndices = map.cellIndices;
				Region[] directGrid = map.regionGrid.DirectGrid;
				CellRect.CellRectIterator iterator = this.extentsClose.GetIterator();
				while (!iterator.Done())
				{
					IntVec3 current = iterator.Current;
					if (directGrid[cellIndices.CellToIndex(current)] == this)
					{
						return current;
					}
					iterator.MoveNext();
				}
				Log.Error("Couldn't find any cell in region " + this.ToString());
				return this.extentsClose.RandomCell;
			}
		}

		public string DebugString
		{
			get
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.AppendLine("id: " + this.id);
				stringBuilder.AppendLine("mapIndex: " + this.mapIndex);
				stringBuilder.AppendLine("links count: " + this.links.Count);
				List<RegionLink>.Enumerator enumerator = this.links.GetEnumerator();
				try
				{
					while (enumerator.MoveNext())
					{
						RegionLink current = enumerator.Current;
						stringBuilder.AppendLine("  --" + current.ToString());
					}
				}
				finally
				{
					((IDisposable)(object)enumerator).Dispose();
				}
				stringBuilder.AppendLine("valid: " + this.valid.ToString());
				stringBuilder.AppendLine("makeTick: " + this.debug_makeTick);
				stringBuilder.AppendLine("roomID: " + ((this.Room == null) ? "null room!" : this.Room.ID.ToString()));
				stringBuilder.AppendLine("extentsClose: " + this.extentsClose);
				stringBuilder.AppendLine("extentsLimit: " + this.extentsLimit);
				stringBuilder.AppendLine("ListerThings:");
				if (this.listerThings.AllThings != null)
				{
					for (int i = 0; i < this.listerThings.AllThings.Count; i++)
					{
						stringBuilder.AppendLine("  --" + this.listerThings.AllThings[i]);
					}
				}
				return stringBuilder.ToString();
			}
		}

		public bool DebugIsNew
		{
			get
			{
				return this.debug_makeTick > Find.TickManager.TicksGame - 60;
			}
		}

		public ListerThings ListerThings
		{
			get
			{
				return this.listerThings;
			}
		}

		private Region()
		{
		}

		public static Region MakeNewUnfilled(IntVec3 root, Map map)
		{
			Region region = new Region();
			region.debug_makeTick = Find.TickManager.TicksGame;
			region.id = Region.nextId;
			Region.nextId++;
			region.mapIndex = (sbyte)map.Index;
			region.precalculatedHashCode = Gen.HashCombineInt(region.id, 1295813358);
			region.extentsClose.minX = root.x;
			region.extentsClose.maxX = root.x;
			region.extentsClose.minZ = root.z;
			region.extentsClose.maxZ = root.z;
			region.extentsLimit.minX = root.x - root.x % 12;
			region.extentsLimit.maxX = root.x + 12 - (root.x + 12) % 12 - 1;
			region.extentsLimit.minZ = root.z - root.z % 12;
			region.extentsLimit.maxZ = root.z + 12 - (root.z + 12) % 12 - 1;
			region.extentsLimit.ClipInsideMap(map);
			return region;
		}

		public bool Allows(TraverseParms tp, bool isDestination)
		{
			if (tp.mode != TraverseMode.PassAllDestroyableThings && !this.type.Passable())
			{
				return false;
			}
			if ((int)tp.maxDanger < 3 && tp.pawn != null)
			{
				Danger danger = this.DangerFor(tp.pawn);
				if (isDestination || danger == Danger.Deadly)
				{
					Region region = tp.pawn.GetRegion(RegionType.Set_All);
					if ((region == null || (int)danger > (int)region.DangerFor(tp.pawn)) && (int)danger > (int)tp.maxDanger)
					{
						return false;
					}
				}
			}
			switch (tp.mode)
			{
			case TraverseMode.ByPawn:
			{
				if (this.portal != null)
				{
					ByteGrid avoidGrid = tp.pawn.GetAvoidGrid();
					if (avoidGrid != null && avoidGrid[this.portal.Position] == 255)
					{
						return false;
					}
					if (tp.pawn.HostileTo(this.portal))
					{
						return this.portal.CanPhysicallyPass(tp.pawn) || tp.canBash;
					}
					return this.portal.CanPhysicallyPass(tp.pawn) && !this.portal.IsForbiddenToPass(tp.pawn);
				}
				return true;
			}
			case TraverseMode.NoPassClosedDoors:
			{
				return this.portal == null || this.portal.FreePassage;
			}
			case TraverseMode.PassDoors:
			{
				return true;
			}
			case TraverseMode.PassAllDestroyableThings:
			{
				return true;
			}
			default:
			{
				throw new NotImplementedException();
			}
			}
		}

		public Danger DangerFor(Pawn p)
		{
			Room room = this.Room;
			float temperature = room.Temperature;
			FloatRange floatRange = p.SafeTemperatureRange();
			if (floatRange.Includes(temperature))
			{
				return Danger.None;
			}
			if (floatRange.ExpandedBy(80f).Includes(temperature))
			{
				return Danger.Some;
			}
			return Danger.Deadly;
		}

		public AreaOverlap OverlapWith(Area a)
		{
			if (a.TrueCount == 0)
			{
				return AreaOverlap.None;
			}
			if (this.Map != a.Map)
			{
				return AreaOverlap.None;
			}
			if (this.cachedAreaOverlaps == null)
			{
				this.cachedAreaOverlaps = new Dictionary<Area, AreaOverlap>();
			}
			AreaOverlap areaOverlap = default(AreaOverlap);
			if (!this.cachedAreaOverlaps.TryGetValue(a, out areaOverlap))
			{
				int num = 0;
				int num2 = 0;
				foreach (IntVec3 cell in this.Cells)
				{
					num2++;
					if (a[cell])
					{
						num++;
					}
				}
				areaOverlap = (AreaOverlap)((num != 0) ? ((num == num2) ? 1 : 2) : 0);
				this.cachedAreaOverlaps.Add(a, areaOverlap);
			}
			return areaOverlap;
		}

		public void Notify_AreaChanged(Area a)
		{
			if (this.cachedAreaOverlaps != null && this.cachedAreaOverlaps.ContainsKey(a))
			{
				this.cachedAreaOverlaps.Remove(a);
			}
		}

		public void DecrementMapIndex()
		{
			if (this.mapIndex <= 0)
			{
				Log.Warning("Tried to decrement map index for region " + this.id + ", but mapIndex=" + this.mapIndex);
			}
			else
			{
				this.mapIndex = (sbyte)(this.mapIndex - 1);
			}
		}

		public void Notify_MyMapRemoved()
		{
			this.mapIndex = (sbyte)(-1);
		}

		public override string ToString()
		{
			string str = (this.portal == null) ? "null" : this.portal.ToString();
			return "Region(id=" + this.id + ", mapIndex=" + this.mapIndex + ", center=" + this.extentsClose.CenterCell + ", links=" + this.links.Count + ", cells=" + this.CellCount + ((this.portal == null) ? null : (", portal=" + str)) + ")";
		}

		public void DebugDraw()
		{
			if (DebugViewSettings.drawRegionTraversal && Find.TickManager.TicksGame < this.debug_lastTraverseTick + 60)
			{
				float a = (float)(1.0 - (float)(Find.TickManager.TicksGame - this.debug_lastTraverseTick) / 60.0);
				GenDraw.DrawFieldEdges(this.Cells.ToList(), new Color(0f, 0f, 1f, a));
			}
		}

		public void DebugDrawMouseover()
		{
			int num = Mathf.RoundToInt((float)(Time.realtimeSinceStartup * 2.0)) % 2;
			if (DebugViewSettings.drawRegions)
			{
				Color color = this.valid ? ((!this.DebugIsNew) ? Color.green : Color.yellow) : Color.red;
				GenDraw.DrawFieldEdges(this.Cells.ToList(), color);
				foreach (Region neighbor in this.Neighbors)
				{
					GenDraw.DrawFieldEdges(neighbor.Cells.ToList(), Color.grey);
				}
			}
			if (DebugViewSettings.drawRegionLinks)
			{
				List<RegionLink>.Enumerator enumerator2 = this.links.GetEnumerator();
				try
				{
					while (enumerator2.MoveNext())
					{
						RegionLink current2 = enumerator2.Current;
						if (num == 1)
						{
							foreach (IntVec3 cell in current2.span.Cells)
							{
								CellRenderer.RenderCell(cell, DebugSolidColorMats.MaterialOf(Color.magenta));
							}
						}
					}
				}
				finally
				{
					((IDisposable)(object)enumerator2).Dispose();
				}
			}
			if (DebugViewSettings.drawRegionThings)
			{
				List<Thing>.Enumerator enumerator4 = this.listerThings.AllThings.GetEnumerator();
				try
				{
					while (enumerator4.MoveNext())
					{
						Thing current4 = enumerator4.Current;
						CellRenderer.RenderSpot(current4.TrueCenter(), (float)((float)(current4.thingIDNumber % 256) / 256.0));
					}
				}
				finally
				{
					((IDisposable)(object)enumerator4).Dispose();
				}
			}
		}

		public void Debug_Notify_Traversed()
		{
			this.debug_lastTraverseTick = Find.TickManager.TicksGame;
		}

		public override int GetHashCode()
		{
			return this.precalculatedHashCode;
		}

		public override bool Equals(object obj)
		{
			if (obj == null)
			{
				return false;
			}
			Region region = obj as Region;
			if (region == null)
			{
				return false;
			}
			return region.id == this.id;
		}
	}
}
