﻿using System;
using System.Collections.Generic;
using RimWorld;

namespace Verse
{
	// Token: 0x02000C13 RID: 3091
	public sealed class FogGrid : IExposable
	{
		// Token: 0x06004381 RID: 17281 RVA: 0x00239E83 File Offset: 0x00238283
		public FogGrid(Map map)
		{
			this.map = map;
		}

		// Token: 0x06004382 RID: 17282 RVA: 0x00239E93 File Offset: 0x00238293
		public void ExposeData()
		{
			DataExposeUtility.BoolArray(ref this.fogGrid, this.map.Area, "fogGrid");
		}

		// Token: 0x06004383 RID: 17283 RVA: 0x00239EB4 File Offset: 0x002382B4
		public void Unfog(IntVec3 c)
		{
			this.UnfogWorker(c);
			List<Thing> thingList = c.GetThingList(this.map);
			for (int i = 0; i < thingList.Count; i++)
			{
				Thing thing = thingList[i];
				if (thing.def.Fillage == FillCategory.Full)
				{
					foreach (IntVec3 c2 in thing.OccupiedRect().Cells)
					{
						this.UnfogWorker(c2);
					}
				}
			}
		}

		// Token: 0x06004384 RID: 17284 RVA: 0x00239F68 File Offset: 0x00238368
		private void UnfogWorker(IntVec3 c)
		{
			int num = this.map.cellIndices.CellToIndex(c);
			if (this.fogGrid[num])
			{
				this.fogGrid[num] = false;
				if (Current.ProgramState == ProgramState.Playing)
				{
					this.map.mapDrawer.MapMeshDirty(c, MapMeshFlag.FogOfWar);
				}
				Designation designation = this.map.designationManager.DesignationAt(c, DesignationDefOf.Mine);
				if (designation != null && c.GetFirstMineable(this.map) == null)
				{
					designation.Delete();
				}
				if (Current.ProgramState == ProgramState.Playing)
				{
					this.map.roofGrid.Drawer.SetDirty();
				}
			}
		}

		// Token: 0x06004385 RID: 17285 RVA: 0x0023A014 File Offset: 0x00238414
		public bool IsFogged(IntVec3 c)
		{
			return c.InBounds(this.map) && this.fogGrid != null && this.fogGrid[this.map.cellIndices.CellToIndex(c)];
		}

		// Token: 0x06004386 RID: 17286 RVA: 0x0023A064 File Offset: 0x00238464
		public bool IsFogged(int index)
		{
			return this.fogGrid[index];
		}

		// Token: 0x06004387 RID: 17287 RVA: 0x0023A084 File Offset: 0x00238484
		public void ClearAllFog()
		{
			for (int i = 0; i < this.map.Size.x; i++)
			{
				for (int j = 0; j < this.map.Size.z; j++)
				{
					this.Unfog(new IntVec3(i, 0, j));
				}
			}
		}

		// Token: 0x06004388 RID: 17288 RVA: 0x0023A0EC File Offset: 0x002384EC
		public void Notify_FogBlockerRemoved(IntVec3 c)
		{
			if (Current.ProgramState == ProgramState.Playing)
			{
				bool flag = false;
				for (int i = 0; i < 8; i++)
				{
					IntVec3 c2 = c + GenAdj.AdjacentCells[i];
					if (c2.InBounds(this.map) && !this.IsFogged(c2))
					{
						flag = true;
						break;
					}
				}
				if (flag)
				{
					this.FloodUnfogAdjacent(c);
				}
			}
		}

		// Token: 0x06004389 RID: 17289 RVA: 0x0023A16E File Offset: 0x0023856E
		public void Notify_PawnEnteringDoor(Building_Door door, Pawn pawn)
		{
			if (pawn.Faction == Faction.OfPlayer || pawn.HostFaction == Faction.OfPlayer)
			{
				this.FloodUnfogAdjacent(door.Position);
			}
		}

		// Token: 0x0600438A RID: 17290 RVA: 0x0023A1A4 File Offset: 0x002385A4
		internal void SetAllFogged()
		{
			CellIndices cellIndices = this.map.cellIndices;
			if (this.fogGrid == null)
			{
				this.fogGrid = new bool[cellIndices.NumGridCells];
			}
			foreach (IntVec3 c in this.map.AllCells)
			{
				this.fogGrid[cellIndices.CellToIndex(c)] = true;
			}
			if (Current.ProgramState == ProgramState.Playing)
			{
				this.map.roofGrid.Drawer.SetDirty();
			}
		}

		// Token: 0x0600438B RID: 17291 RVA: 0x0023A258 File Offset: 0x00238658
		private void FloodUnfogAdjacent(IntVec3 c)
		{
			this.Unfog(c);
			bool flag = false;
			FloodUnfogResult floodUnfogResult = default(FloodUnfogResult);
			for (int i = 0; i < 4; i++)
			{
				IntVec3 intVec = c + GenAdj.CardinalDirections[i];
				if (intVec.InBounds(this.map))
				{
					if (intVec.Fogged(this.map))
					{
						Building edifice = intVec.GetEdifice(this.map);
						if (edifice == null || !edifice.def.MakeFog)
						{
							flag = true;
							floodUnfogResult = FloodFillerFog.FloodUnfog(intVec, this.map);
						}
						else
						{
							this.Unfog(intVec);
						}
					}
				}
			}
			for (int j = 0; j < 8; j++)
			{
				IntVec3 c2 = c + GenAdj.AdjacentCells[j];
				if (c2.InBounds(this.map))
				{
					Building edifice2 = c2.GetEdifice(this.map);
					if (edifice2 != null && edifice2.def.MakeFog)
					{
						this.Unfog(c2);
					}
				}
			}
			if (flag)
			{
				if (floodUnfogResult.mechanoidFound)
				{
					Find.LetterStack.ReceiveLetter("LetterLabelAreaRevealed".Translate(), "AreaRevealedWithMechanoids".Translate(), LetterDefOf.ThreatBig, new TargetInfo(c, this.map, false), null, null);
				}
				else if (!floodUnfogResult.allOnScreen || floodUnfogResult.cellsUnfogged >= 600)
				{
					Find.LetterStack.ReceiveLetter("LetterLabelAreaRevealed".Translate(), "AreaRevealed".Translate(), LetterDefOf.NeutralEvent, new TargetInfo(c, this.map, false), null, null);
				}
			}
		}

		// Token: 0x04002E1E RID: 11806
		private Map map;

		// Token: 0x04002E1F RID: 11807
		public bool[] fogGrid;

		// Token: 0x04002E20 RID: 11808
		private const int AlwaysSendLetterIfUnfoggedMoreCellsThan = 600;
	}
}
