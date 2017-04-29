using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public abstract class PawnColumnWorker_Checkbox : PawnColumnWorker
	{
		private const int HorizontalPadding = 2;

		public override void DoCell(Rect rect, Pawn pawn, PawnTable table)
		{
			if (!this.HasCheckbox(pawn))
			{
				return;
			}
			int num = (int)((rect.width - 24f) / 2f);
			int num2 = Mathf.Max(3, 0);
			Vector2 topLeft = new Vector2(rect.x + (float)num, rect.y + (float)num2);
			Rect rect2 = new Rect(topLeft.x, topLeft.y, 24f, 24f);
			bool value = this.GetValue(pawn);
			bool flag = value;
			Widgets.Checkbox(topLeft, ref value, 24f, false);
			if (Mouse.IsOver(rect2))
			{
				string tip = this.GetTip(pawn);
				if (!tip.NullOrEmpty())
				{
					TooltipHandler.TipRegion(rect2, tip);
				}
			}
			if (value != flag)
			{
				this.SetValue(pawn, value);
			}
		}

		public override int GetMinWidth(PawnTable table)
		{
			return Mathf.Max(base.GetMinWidth(table), 28);
		}

		public override int GetMaxWidth(PawnTable table)
		{
			return Mathf.Min(base.GetMaxWidth(table), this.GetMinWidth(table));
		}

		public override int GetMinCellHeight(Pawn pawn)
		{
			return Mathf.Max(base.GetMinCellHeight(pawn), 24);
		}

		public override int Compare(Pawn a, Pawn b)
		{
			return this.GetValueToCompare(a).CompareTo(this.GetValueToCompare(b));
		}

		private int GetValueToCompare(Pawn pawn)
		{
			if (!this.HasCheckbox(pawn))
			{
				return 0;
			}
			if (!this.GetValue(pawn))
			{
				return 1;
			}
			return 2;
		}

		protected virtual string GetTip(Pawn pawn)
		{
			return null;
		}

		protected virtual bool HasCheckbox(Pawn pawn)
		{
			return true;
		}

		protected abstract bool GetValue(Pawn pawn);

		protected abstract void SetValue(Pawn pawn, bool value);
	}
}