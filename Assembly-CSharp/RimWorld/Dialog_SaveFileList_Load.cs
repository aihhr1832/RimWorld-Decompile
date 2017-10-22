using System;
using Verse;
using Verse.Profile;

namespace RimWorld
{
	public class Dialog_SaveFileList_Load : Dialog_SaveFileList
	{
		public Dialog_SaveFileList_Load()
		{
			base.interactButLabel = "LoadGameButton".Translate();
		}

		protected override void DoFileInteraction(string saveFileName)
		{
			PreLoadUtility.CheckVersionAndLoad(GenFilePaths.FilePathForSavedGame(saveFileName), ScribeMetaHeaderUtility.ScribeHeaderMode.Map, (Action)delegate()
			{
				Action preLoadLevelAction = (Action)delegate()
				{
					MemoryUtility.ClearAllMapsAndWorld();
					Current.Game = new Game();
					Current.Game.InitData = new GameInitData();
					Current.Game.InitData.gameToLoad = saveFileName;
				};
				LongEventHandler.QueueLongEvent(preLoadLevelAction, "Play", "LoadingLongEvent", true, null);
			});
		}
	}
}
