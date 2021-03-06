﻿using System;
using System.Runtime.CompilerServices;
using RimWorld.Planet;
using Verse;

namespace RimWorld
{
	public class IncidentWorker_QuestJourneyOffer : IncidentWorker
	{
		private const int MinTraversalDistance = 180;

		private const int MaxTraversalDistance = 800;

		[CompilerGenerated]
		private static Predicate<int> <>f__am$cache0;

		public IncidentWorker_QuestJourneyOffer()
		{
		}

		protected override bool CanFireNowSub(IncidentParms parms)
		{
			int num;
			return this.TryFindRootTile(out num);
		}

		protected override bool TryExecuteWorker(IncidentParms parms)
		{
			int rootTile;
			bool result;
			int tile;
			if (!this.TryFindRootTile(out rootTile))
			{
				result = false;
			}
			else if (!this.TryFindDestinationTile(rootTile, out tile))
			{
				result = false;
			}
			else
			{
				WorldObject journeyDestination = WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.EscapeShip);
				journeyDestination.Tile = tile;
				Find.WorldObjects.Add(journeyDestination);
				DiaNode diaNode = new DiaNode("JourneyOffer".Translate());
				DiaOption diaOption = new DiaOption("JumpToLocation".Translate());
				diaOption.action = delegate()
				{
					CameraJumper.TryJumpAndSelect(journeyDestination);
				};
				diaOption.resolveTree = true;
				diaNode.options.Add(diaOption);
				DiaOption diaOption2 = new DiaOption("OK".Translate());
				diaOption2.resolveTree = true;
				diaNode.options.Add(diaOption2);
				Find.WindowStack.Add(new Dialog_NodeTree(diaNode, true, true, null));
				Find.Archive.Add(new ArchivedDialog(diaNode.text, null, null));
				result = true;
			}
			return result;
		}

		private bool TryFindRootTile(out int tile)
		{
			int unused;
			return TileFinder.TryFindRandomPlayerTile(out tile, false, (int x) => this.TryFindDestinationTileActual(x, 180, out unused));
		}

		private bool TryFindDestinationTile(int rootTile, out int tile)
		{
			int num = 800;
			int i = 0;
			while (i < 1000)
			{
				num = (int)((float)num * Rand.Range(0.5f, 0.75f));
				if (num <= 180)
				{
					num = 180;
				}
				bool result;
				if (this.TryFindDestinationTileActual(rootTile, num, out tile))
				{
					result = true;
				}
				else
				{
					if (num > 180)
					{
						i++;
						continue;
					}
					result = false;
				}
				return result;
			}
			tile = -1;
			return false;
		}

		private bool TryFindDestinationTileActual(int rootTile, int minDist, out int tile)
		{
			return TileFinder.TryFindPassableTileWithTraversalDistance(rootTile, minDist, 800, out tile, (int x) => !Find.WorldObjects.AnyWorldObjectAt(x) && Find.WorldGrid[x].biome.canBuildBase && Find.WorldGrid[x].biome.canAutoChoose, true, true);
		}

		[CompilerGenerated]
		private static bool <TryFindDestinationTileActual>m__0(int x)
		{
			return !Find.WorldObjects.AnyWorldObjectAt(x) && Find.WorldGrid[x].biome.canBuildBase && Find.WorldGrid[x].biome.canAutoChoose;
		}

		[CompilerGenerated]
		private sealed class <TryExecuteWorker>c__AnonStorey0
		{
			internal WorldObject journeyDestination;

			public <TryExecuteWorker>c__AnonStorey0()
			{
			}

			internal void <>m__0()
			{
				CameraJumper.TryJumpAndSelect(this.journeyDestination);
			}
		}

		[CompilerGenerated]
		private sealed class <TryFindRootTile>c__AnonStorey1
		{
			internal int unused;

			internal IncidentWorker_QuestJourneyOffer $this;

			public <TryFindRootTile>c__AnonStorey1()
			{
			}

			internal bool <>m__0(int x)
			{
				return this.$this.TryFindDestinationTileActual(x, 180, out this.unused);
			}
		}
	}
}
