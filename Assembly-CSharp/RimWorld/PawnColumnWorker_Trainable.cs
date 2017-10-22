using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	public class PawnColumnWorker_Trainable : PawnColumnWorker
	{
		public override void DoCell(Rect rect, Pawn pawn, PawnTable table)
		{
			if (pawn.training != null)
			{
				bool flag = default(bool);
				AcceptanceReport canTrain = pawn.training.CanAssignToTrain(base.def.trainable, out flag);
				if (flag && canTrain.Accepted)
				{
					int num = (int)((rect.width - 24.0) / 2.0);
					int num2 = Mathf.Max(3, 0);
					Rect rect2 = new Rect(rect.x + (float)num, rect.y + (float)num2, 24f, 24f);
					TrainingCardUtility.DoTrainableCheckbox(rect2, pawn, base.def.trainable, canTrain, false, true);
				}
			}
		}

		public override int GetMinWidth(PawnTable table)
		{
			return Mathf.Max(base.GetMinWidth(table), 24);
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
			if (pawn.training == null)
			{
				return -2147483648;
			}
			if (pawn.training.IsCompleted(base.def.trainable))
			{
				return 4;
			}
			bool flag = default(bool);
			AcceptanceReport acceptanceReport = pawn.training.CanAssignToTrain(base.def.trainable, out flag);
			if (!flag)
			{
				return 0;
			}
			if (!acceptanceReport.Accepted)
			{
				return 1;
			}
			if (!pawn.training.GetWanted(base.def.trainable))
			{
				return 2;
			}
			return 3;
		}

		protected override void HeaderClicked(Rect headerRect, PawnTable table)
		{
			base.HeaderClicked(headerRect, table);
			if (Event.current.shift)
			{
				List<Pawn> pawnsListForReading = table.PawnsListForReading;
				for (int i = 0; i < pawnsListForReading.Count; i++)
				{
					if (pawnsListForReading[i].training != null && !pawnsListForReading[i].training.IsCompleted(base.def.trainable))
					{
						bool flag = default(bool);
						AcceptanceReport acceptanceReport = pawnsListForReading[i].training.CanAssignToTrain(base.def.trainable, out flag);
						if (flag && acceptanceReport.Accepted)
						{
							bool wanted = pawnsListForReading[i].training.GetWanted(base.def.trainable);
							if (Event.current.button == 0)
							{
								if (!wanted)
								{
									pawnsListForReading[i].training.SetWantedRecursive(base.def.trainable, true);
								}
							}
							else if (Event.current.button == 1 && wanted)
							{
								pawnsListForReading[i].training.SetWantedRecursive(base.def.trainable, false);
							}
						}
					}
				}
				if (Event.current.button == 0)
				{
					SoundDefOf.CheckboxTurnedOn.PlayOneShotOnCamera(null);
				}
				else if (Event.current.button == 1)
				{
					SoundDefOf.CheckboxTurnedOff.PlayOneShotOnCamera(null);
				}
			}
		}

		protected override string GetHeaderTip(PawnTable table)
		{
			return base.GetHeaderTip(table) + "\n" + "CheckboxShiftClickTip".Translate();
		}
	}
}
