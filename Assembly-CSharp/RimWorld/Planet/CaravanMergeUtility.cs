﻿using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld.Planet
{
	[StaticConstructorOnStartup]
	public static class CaravanMergeUtility
	{
		private static readonly Texture2D MergeCommandTex = ContentFinder<Texture2D>.Get("UI/Commands/MergeCaravans", true);

		private static List<Caravan> tmpSelectedPlayerCaravans = new List<Caravan>();

		private static List<Caravan> tmpCaravansOnSameTile = new List<Caravan>();

		[CompilerGenerated]
		private static Action <>f__am$cache0;

		[CompilerGenerated]
		private static Func<Caravan, int> <>f__am$cache1;

		public static bool ShouldShowMergeCommand
		{
			get
			{
				return CaravanMergeUtility.CanMergeAnySelectedCaravans || CaravanMergeUtility.AnySelectedCaravanCloseToAnyOtherMergeableCaravan;
			}
		}

		public static bool CanMergeAnySelectedCaravans
		{
			get
			{
				List<WorldObject> selectedObjects = Find.WorldSelector.SelectedObjects;
				for (int i = 0; i < selectedObjects.Count; i++)
				{
					Caravan caravan = selectedObjects[i] as Caravan;
					if (caravan != null && caravan.IsPlayerControlled)
					{
						for (int j = i + 1; j < selectedObjects.Count; j++)
						{
							Caravan caravan2 = selectedObjects[j] as Caravan;
							if (caravan2 != null && caravan2.IsPlayerControlled && CaravanMergeUtility.CloseToEachOther(caravan, caravan2))
							{
								return true;
							}
						}
					}
				}
				return false;
			}
		}

		public static bool AnySelectedCaravanCloseToAnyOtherMergeableCaravan
		{
			get
			{
				List<WorldObject> selectedObjects = Find.WorldSelector.SelectedObjects;
				List<Caravan> caravans = Find.WorldObjects.Caravans;
				for (int i = 0; i < selectedObjects.Count; i++)
				{
					Caravan caravan = selectedObjects[i] as Caravan;
					if (caravan != null && caravan.IsPlayerControlled)
					{
						for (int j = 0; j < caravans.Count; j++)
						{
							Caravan caravan2 = caravans[j];
							if (caravan2 != caravan)
							{
								if (caravan2.IsPlayerControlled && CaravanMergeUtility.CloseToEachOther(caravan, caravan2))
								{
									return true;
								}
							}
						}
					}
				}
				return false;
			}
		}

		public static Command MergeCommand(Caravan caravan)
		{
			Command_Action command_Action = new Command_Action();
			command_Action.defaultLabel = "CommandMergeCaravans".Translate();
			command_Action.defaultDesc = "CommandMergeCaravansDesc".Translate();
			command_Action.icon = CaravanMergeUtility.MergeCommandTex;
			command_Action.action = delegate()
			{
				CaravanMergeUtility.TryMergeSelectedCaravans();
				SoundDefOf.Tick_High.PlayOneShotOnCamera(null);
			};
			if (!CaravanMergeUtility.CanMergeAnySelectedCaravans)
			{
				command_Action.Disable("CommandMergeCaravansFailCaravansNotSelected".Translate());
			}
			return command_Action;
		}

		public static void TryMergeSelectedCaravans()
		{
			CaravanMergeUtility.tmpSelectedPlayerCaravans.Clear();
			List<WorldObject> selectedObjects = Find.WorldSelector.SelectedObjects;
			for (int i = 0; i < selectedObjects.Count; i++)
			{
				Caravan caravan = selectedObjects[i] as Caravan;
				if (caravan != null && caravan.IsPlayerControlled)
				{
					CaravanMergeUtility.tmpSelectedPlayerCaravans.Add(caravan);
				}
			}
			while (CaravanMergeUtility.tmpSelectedPlayerCaravans.Any<Caravan>())
			{
				Caravan caravan2 = CaravanMergeUtility.tmpSelectedPlayerCaravans[0];
				CaravanMergeUtility.tmpSelectedPlayerCaravans.RemoveAt(0);
				CaravanMergeUtility.tmpCaravansOnSameTile.Clear();
				CaravanMergeUtility.tmpCaravansOnSameTile.Add(caravan2);
				for (int j = CaravanMergeUtility.tmpSelectedPlayerCaravans.Count - 1; j >= 0; j--)
				{
					if (CaravanMergeUtility.CloseToEachOther(CaravanMergeUtility.tmpSelectedPlayerCaravans[j], caravan2))
					{
						CaravanMergeUtility.tmpCaravansOnSameTile.Add(CaravanMergeUtility.tmpSelectedPlayerCaravans[j]);
						CaravanMergeUtility.tmpSelectedPlayerCaravans.RemoveAt(j);
					}
				}
				if (CaravanMergeUtility.tmpCaravansOnSameTile.Count >= 2)
				{
					CaravanMergeUtility.MergeCaravans(CaravanMergeUtility.tmpCaravansOnSameTile);
				}
			}
		}

		private static bool CloseToEachOther(Caravan c1, Caravan c2)
		{
			bool result;
			if (c1.Tile == c2.Tile)
			{
				result = true;
			}
			else
			{
				Vector3 drawPos = c1.DrawPos;
				Vector3 drawPos2 = c2.DrawPos;
				float num = Find.WorldGrid.averageTileSize * 0.5f;
				result = ((drawPos - drawPos2).sqrMagnitude < num * num);
			}
			return result;
		}

		private static void MergeCaravans(List<Caravan> caravans)
		{
			Caravan caravan = caravans.MaxBy((Caravan x) => x.PawnsListForReading.Count);
			for (int i = 0; i < caravans.Count; i++)
			{
				Caravan caravan2 = caravans[i];
				if (caravan2 != caravan)
				{
					caravan2.pawns.TryTransferAllToContainer(caravan.pawns, true);
					Find.WorldObjects.Remove(caravan2);
				}
			}
			caravan.Notify_Merged(caravans);
		}

		// Note: this type is marked as 'beforefieldinit'.
		static CaravanMergeUtility()
		{
		}

		[CompilerGenerated]
		private static void <MergeCommand>m__0()
		{
			CaravanMergeUtility.TryMergeSelectedCaravans();
			SoundDefOf.Tick_High.PlayOneShotOnCamera(null);
		}

		[CompilerGenerated]
		private static int <MergeCaravans>m__1(Caravan x)
		{
			return x.PawnsListForReading.Count;
		}
	}
}
