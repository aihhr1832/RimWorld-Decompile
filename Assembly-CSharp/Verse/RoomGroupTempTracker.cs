﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Verse
{
	public sealed class RoomGroupTempTracker
	{
		private RoomGroup roomGroup;

		private float temperatureInt;

		private List<IntVec3> equalizeCells = new List<IntVec3>();

		private float noRoofCoverage;

		private float thickRoofCoverage;

		private int cycleIndex = 0;

		private const float ThinRoofEqualizeRate = 5E-05f;

		private const float NoRoofEqualizeRate = 0.0007f;

		private const float DeepEqualizeFractionPerTick = 5E-05f;

		private static int debugGetFrame = -999;

		private static float debugWallEq;

		public RoomGroupTempTracker(RoomGroup roomGroup, Map map)
		{
			this.roomGroup = roomGroup;
			this.Temperature = map.mapTemperature.OutdoorTemp;
		}

		private Map Map
		{
			get
			{
				return this.roomGroup.Map;
			}
		}

		private float ThinRoofCoverage
		{
			get
			{
				return 1f - (this.thickRoofCoverage + this.noRoofCoverage);
			}
		}

		public float Temperature
		{
			get
			{
				return this.temperatureInt;
			}
			set
			{
				this.temperatureInt = Mathf.Clamp(value, -273.15f, 2000f);
			}
		}

		public void RoofChanged()
		{
			this.RegenerateEqualizationData();
		}

		public void RoomChanged()
		{
			if (this.Map != null)
			{
				this.Map.autoBuildRoofAreaSetter.ResolveQueuedGenerateRoofs();
			}
			this.RegenerateEqualizationData();
		}

		private void RegenerateEqualizationData()
		{
			this.thickRoofCoverage = 0f;
			this.noRoofCoverage = 0f;
			this.equalizeCells.Clear();
			if (this.roomGroup.RoomCount != 0)
			{
				Map map = this.Map;
				if (!this.roomGroup.UsesOutdoorTemperature)
				{
					int num = 0;
					foreach (IntVec3 c in this.roomGroup.Cells)
					{
						RoofDef roof = c.GetRoof(map);
						if (roof == null)
						{
							this.noRoofCoverage += 1f;
						}
						else if (roof.isThickRoof)
						{
							this.thickRoofCoverage += 1f;
						}
						num++;
					}
					this.thickRoofCoverage /= (float)num;
					this.noRoofCoverage /= (float)num;
					foreach (IntVec3 a in this.roomGroup.Cells)
					{
						int i = 0;
						while (i < 4)
						{
							IntVec3 intVec = a + GenAdj.CardinalDirections[i];
							IntVec3 intVec2 = a + GenAdj.CardinalDirections[i] * 2;
							if (intVec.InBounds(map))
							{
								Region region = intVec.GetRegion(map, RegionType.Set_Passable);
								if (region != null)
								{
									if (region.type != RegionType.Portal)
									{
										goto IL_2DD;
									}
									bool flag = false;
									for (int j = 0; j < region.links.Count; j++)
									{
										Region regionA = region.links[j].RegionA;
										Region regionB = region.links[j].RegionB;
										if (regionA.Room.Group != this.roomGroup && regionA.portal == null)
										{
											flag = true;
											break;
										}
										if (regionB.Room.Group != this.roomGroup && regionB.portal == null)
										{
											flag = true;
											break;
										}
									}
									if (flag)
									{
										goto IL_2DD;
									}
								}
								goto IL_24F;
							}
							goto IL_24F;
							IL_2DD:
							i++;
							continue;
							IL_24F:
							if (intVec2.InBounds(map))
							{
								RoomGroup roomGroup = intVec2.GetRoomGroup(map);
								if (roomGroup != this.roomGroup)
								{
									bool flag2 = false;
									for (int k = 0; k < 4; k++)
									{
										IntVec3 loc = intVec2 + GenAdj.CardinalDirections[k];
										if (loc.GetRoomGroup(map) == this.roomGroup)
										{
											flag2 = true;
											break;
										}
									}
									if (!flag2)
									{
										this.equalizeCells.Add(intVec2);
									}
								}
							}
							goto IL_2DD;
						}
					}
					this.equalizeCells.Shuffle<IntVec3>();
				}
			}
		}

		public void EqualizeTemperature()
		{
			if (this.roomGroup.UsesOutdoorTemperature)
			{
				this.Temperature = this.Map.mapTemperature.OutdoorTemp;
			}
			else if (this.roomGroup.RoomCount == 0 || this.roomGroup.Rooms[0].RegionType != RegionType.Portal)
			{
				float num = this.ThinRoofEqualizationTempChangePerInterval();
				float num2 = this.NoRoofEqualizationTempChangePerInterval();
				float num3 = this.WallEqualizationTempChangePerInterval();
				float num4 = this.DeepEqualizationTempChangePerInterval();
				this.Temperature += num + num2 + num3 + num4;
			}
		}

		private float WallEqualizationTempChangePerInterval()
		{
			float result;
			if (this.equalizeCells.Count == 0)
			{
				result = 0f;
			}
			else
			{
				float num = 0f;
				int num2 = Mathf.CeilToInt((float)this.equalizeCells.Count * 0.2f);
				for (int i = 0; i < num2; i++)
				{
					this.cycleIndex++;
					int index = this.cycleIndex % this.equalizeCells.Count;
					float num3;
					if (GenTemperature.TryGetDirectAirTemperatureForCell(this.equalizeCells[index], this.Map, out num3))
					{
						num += num3 - this.Temperature;
					}
					else
					{
						num += Mathf.Lerp(this.Temperature, this.Map.mapTemperature.OutdoorTemp, 0.5f) - this.Temperature;
					}
				}
				float num4 = num / (float)num2;
				float num5 = num4 * (float)this.equalizeCells.Count;
				result = num5 * 120f * 0.00017f / (float)this.roomGroup.CellCount;
			}
			return result;
		}

		private float TempDiffFromOutdoorsAdjusted()
		{
			float num = this.Map.mapTemperature.OutdoorTemp - this.temperatureInt;
			float result;
			if (Mathf.Abs(num) < 100f)
			{
				result = num;
			}
			else
			{
				result = Mathf.Sign(num) * 100f + 5f * (num - Mathf.Sign(num) * 100f);
			}
			return result;
		}

		private float ThinRoofEqualizationTempChangePerInterval()
		{
			float result;
			if (this.ThinRoofCoverage < 0.001f)
			{
				result = 0f;
			}
			else
			{
				float num = this.TempDiffFromOutdoorsAdjusted();
				float num2 = num * this.ThinRoofCoverage * 5E-05f;
				num2 *= 120f;
				result = num2;
			}
			return result;
		}

		private float NoRoofEqualizationTempChangePerInterval()
		{
			float result;
			if (this.noRoofCoverage < 0.001f)
			{
				result = 0f;
			}
			else
			{
				float num = this.TempDiffFromOutdoorsAdjusted();
				float num2 = num * this.noRoofCoverage * 0.0007f;
				num2 *= 120f;
				result = num2;
			}
			return result;
		}

		private float DeepEqualizationTempChangePerInterval()
		{
			float result;
			if (this.thickRoofCoverage < 0.001f)
			{
				result = 0f;
			}
			else
			{
				float num = 15f - this.temperatureInt;
				if (num > 0f)
				{
					result = 0f;
				}
				else
				{
					float num2 = num * this.thickRoofCoverage * 5E-05f;
					num2 *= 120f;
					result = num2;
				}
			}
			return result;
		}

		public void DebugDraw()
		{
			foreach (IntVec3 c in this.equalizeCells)
			{
				CellRenderer.RenderCell(c, 0.5f);
			}
		}

		internal string DebugString()
		{
			string result;
			if (this.roomGroup.UsesOutdoorTemperature)
			{
				result = "uses outdoor temperature";
			}
			else
			{
				if (Time.frameCount > RoomGroupTempTracker.debugGetFrame + 120)
				{
					RoomGroupTempTracker.debugWallEq = 0f;
					for (int i = 0; i < 40; i++)
					{
						RoomGroupTempTracker.debugWallEq += this.WallEqualizationTempChangePerInterval();
					}
					RoomGroupTempTracker.debugWallEq /= 40f;
					RoomGroupTempTracker.debugGetFrame = Time.frameCount;
				}
				result = string.Concat(new object[]
				{
					"  thick roof coverage: ",
					this.thickRoofCoverage.ToStringPercent("F0"),
					"\n  thin roof coverage: ",
					this.ThinRoofCoverage.ToStringPercent("F0"),
					"\n  no roof coverage: ",
					this.noRoofCoverage.ToStringPercent("F0"),
					"\n\n  wall equalization: ",
					RoomGroupTempTracker.debugWallEq.ToStringTemperatureOffset("F3"),
					"\n  thin roof equalization: ",
					this.ThinRoofEqualizationTempChangePerInterval().ToStringTemperatureOffset("F3"),
					"\n  no roof equalization: ",
					this.NoRoofEqualizationTempChangePerInterval().ToStringTemperatureOffset("F3"),
					"\n  deep equalization: ",
					this.DeepEqualizationTempChangePerInterval().ToStringTemperatureOffset("F3"),
					"\n\n  temp diff from outdoors, adjusted: ",
					this.TempDiffFromOutdoorsAdjusted().ToStringTemperatureOffset("F3"),
					"\n  tempChange e=20 targ= 200C: ",
					GenTemperature.ControlTemperatureTempChange(this.roomGroup.Cells.First<IntVec3>(), this.roomGroup.Map, 20f, 200f),
					"\n  tempChange e=20 targ=-200C: ",
					GenTemperature.ControlTemperatureTempChange(this.roomGroup.Cells.First<IntVec3>(), this.roomGroup.Map, 20f, -200f),
					"\n  equalize interval ticks: ",
					120,
					"\n  equalize cells count:",
					this.equalizeCells.Count
				});
			}
			return result;
		}

		// Note: this type is marked as 'beforefieldinit'.
		static RoomGroupTempTracker()
		{
		}
	}
}
