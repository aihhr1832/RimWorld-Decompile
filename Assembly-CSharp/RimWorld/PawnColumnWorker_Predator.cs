﻿using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	public class PawnColumnWorker_Predator : PawnColumnWorker_Icon
	{
		private static readonly Texture2D Icon = ContentFinder<Texture2D>.Get("UI/Icons/Animal/Predator", true);

		public PawnColumnWorker_Predator()
		{
		}

		protected override Texture2D GetIconFor(Pawn pawn)
		{
			Texture2D result;
			if (pawn.RaceProps.predator)
			{
				result = PawnColumnWorker_Predator.Icon;
			}
			else
			{
				result = null;
			}
			return result;
		}

		protected override string GetIconTip(Pawn pawn)
		{
			return "IsPredator".Translate();
		}

		// Note: this type is marked as 'beforefieldinit'.
		static PawnColumnWorker_Predator()
		{
		}
	}
}
