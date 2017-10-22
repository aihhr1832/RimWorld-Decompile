using System;

namespace Verse
{
	public static class RoofCollapseUtility
	{
		public const float RoofMaxSupportDistance = 6.9f;

		public static readonly int RoofSupportRadialCellsCount = GenRadial.NumCellsInRadius(6.9f);

		public static bool WithinRangeOfRoofHolder(IntVec3 c, Map map)
		{
			CellIndices cellIndices = map.cellIndices;
			Building[] innerArray = map.edificeGrid.InnerArray;
			for (int i = 0; i < RoofCollapseUtility.RoofSupportRadialCellsCount; i++)
			{
				IntVec3 c2 = c + GenRadial.RadialPattern[i];
				if (c2.InBounds(map))
				{
					Building building = innerArray[cellIndices.CellToIndex(c2)];
					if (building != null && building.def.holdsRoof)
					{
						return true;
					}
				}
			}
			return false;
		}

		public static bool ConnectedToRoofHolder(IntVec3 c, Map map, bool assumeRoofAtRoot)
		{
			bool connected = false;
			map.floodFiller.FloodFill(c, (Predicate<IntVec3>)((IntVec3 x) => (x.Roofed(map) || (x == c && assumeRoofAtRoot)) && !connected), (Action<IntVec3>)delegate(IntVec3 x)
			{
				int num = 0;
				while (true)
				{
					if (num < 5)
					{
						IntVec3 c2 = x + GenAdj.CardinalDirectionsAndInside[num];
						if (c2.InBounds(map))
						{
							Building edifice = c2.GetEdifice(map);
							if (edifice != null && edifice.def.holdsRoof)
								break;
						}
						num++;
						continue;
					}
					return;
				}
				connected = true;
			}, false);
			return connected;
		}
	}
}
