﻿using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public struct EventPack
	{
		private string tagInt;

		private IntVec3 cellInt;

		private IEnumerable<IntVec3> cellsInt;

		public EventPack(string tag)
		{
			this.tagInt = tag;
			this.cellInt = IntVec3.Invalid;
			this.cellsInt = null;
		}

		public EventPack(string tag, IntVec3 cell)
		{
			this.tagInt = tag;
			this.cellInt = cell;
			this.cellsInt = null;
		}

		public EventPack(string tag, IEnumerable<IntVec3> cells)
		{
			this.tagInt = tag;
			this.cellInt = IntVec3.Invalid;
			this.cellsInt = cells;
		}

		public string Tag
		{
			get
			{
				return this.tagInt;
			}
		}

		public IntVec3 Cell
		{
			get
			{
				return this.cellInt;
			}
		}

		public IEnumerable<IntVec3> Cells
		{
			get
			{
				return this.cellsInt;
			}
		}

		public static implicit operator EventPack(string s)
		{
			return new EventPack(s);
		}

		public override string ToString()
		{
			string result;
			if (this.Cell.IsValid)
			{
				result = this.Tag + "-" + this.Cell;
			}
			else
			{
				result = this.Tag;
			}
			return result;
		}
	}
}
