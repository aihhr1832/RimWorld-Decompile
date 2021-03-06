﻿using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class Instruction_PlaceStockpile : Lesson_Instruction
	{
		private CellRect stockpileRect;

		private List<IntVec3> cachedCells;

		public Instruction_PlaceStockpile()
		{
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look<CellRect>(ref this.stockpileRect, "stockpileRect", default(CellRect), false);
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				this.RecacheCells();
			}
		}

		private void RecacheCells()
		{
			this.cachedCells = this.stockpileRect.Cells.ToList<IntVec3>();
		}

		public override void OnActivated()
		{
			base.OnActivated();
			this.stockpileRect = TutorUtility.FindUsableRect(6, 6, base.Map, 0f, false);
			this.RecacheCells();
		}

		public override void LessonOnGUI()
		{
			TutorUtility.DrawCellRectOnGUI(this.stockpileRect, this.def.onMapInstruction);
			base.LessonOnGUI();
		}

		public override void LessonUpdate()
		{
			GenDraw.DrawFieldEdges(this.cachedCells);
			GenDraw.DrawArrowPointingAt(this.stockpileRect.CenterVector3, false);
		}

		public override AcceptanceReport AllowAction(EventPack ep)
		{
			AcceptanceReport result;
			if (ep.Tag == "Designate-ZoneAddStockpile_Resources")
			{
				result = TutorUtility.EventCellsMatchExactly(ep, this.cachedCells);
			}
			else
			{
				result = base.AllowAction(ep);
			}
			return result;
		}
	}
}
