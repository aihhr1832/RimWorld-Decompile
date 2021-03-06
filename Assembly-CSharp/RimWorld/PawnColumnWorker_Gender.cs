﻿using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class PawnColumnWorker_Gender : PawnColumnWorker_Icon
	{
		public PawnColumnWorker_Gender()
		{
		}

		protected override Texture2D GetIconFor(Pawn pawn)
		{
			return pawn.gender.GetIcon();
		}

		protected override string GetIconTip(Pawn pawn)
		{
			return pawn.gender.GetLabel().CapitalizeFirst();
		}

		protected override Vector2 GetIconSize(Pawn pawn)
		{
			return new Vector2(24f, 24f);
		}
	}
}
