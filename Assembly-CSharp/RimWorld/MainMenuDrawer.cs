﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using Verse;
using Verse.Profile;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	public static class MainMenuDrawer
	{
		private static bool anyMapFiles;

		private const float PlayRectWidth = 170f;

		private const float WebRectWidth = 145f;

		private const float RightEdgeMargin = 50f;

		private static readonly Vector2 PaneSize = new Vector2(450f, 450f);

		private static readonly Vector2 TitleSize = new Vector2(1032f, 146f);

		private static readonly Texture2D TexTitle = ContentFinder<Texture2D>.Get("UI/HeroArt/GameTitle", true);

		private const float TitleShift = 50f;

		private static readonly Vector2 LudeonLogoSize = new Vector2(200f, 58f);

		private static readonly Texture2D TexLudeonLogo = ContentFinder<Texture2D>.Get("UI/HeroArt/LudeonLogoSmall", true);

		[CompilerGenerated]
		private static Action <>f__am$cache0;

		[CompilerGenerated]
		private static Action <>f__am$cache1;

		[CompilerGenerated]
		private static Action <>f__am$cache2;

		[CompilerGenerated]
		private static Action <>f__am$cache3;

		[CompilerGenerated]
		private static Action <>f__am$cache4;

		[CompilerGenerated]
		private static Action <>f__am$cache5;

		[CompilerGenerated]
		private static Action <>f__am$cache6;

		[CompilerGenerated]
		private static Action <>f__am$cache7;

		[CompilerGenerated]
		private static Action <>f__am$cache8;

		[CompilerGenerated]
		private static Action <>f__am$cache9;

		[CompilerGenerated]
		private static Action <>f__am$cacheA;

		[CompilerGenerated]
		private static Action <>f__am$cacheB;

		[CompilerGenerated]
		private static Action <>f__am$cacheC;

		[CompilerGenerated]
		private static Action <>f__am$cacheD;

		[CompilerGenerated]
		private static Action <>f__am$cacheE;

		[CompilerGenerated]
		private static Action <>f__am$cacheF;

		[CompilerGenerated]
		private static Action <>f__am$cache10;

		[CompilerGenerated]
		private static Action <>f__am$cache11;

		[CompilerGenerated]
		private static Action <>f__am$cache12;

		public static void Init()
		{
			PlayerKnowledgeDatabase.Save();
			ShipCountdown.CancelCountdown();
			MainMenuDrawer.anyMapFiles = GenFilePaths.AllSavedGameFiles.Any<FileInfo>();
		}

		public static void MainMenuOnGUI()
		{
			VersionControl.DrawInfoInCorner();
			Rect rect = new Rect((float)(UI.screenWidth / 2) - MainMenuDrawer.PaneSize.x / 2f, (float)(UI.screenHeight / 2) - MainMenuDrawer.PaneSize.y / 2f + 50f, MainMenuDrawer.PaneSize.x, MainMenuDrawer.PaneSize.y);
			rect.x = (float)UI.screenWidth - rect.width - 30f;
			Rect rect2 = new Rect(0f, rect.y - 30f, (float)UI.screenWidth - 85f, 30f);
			Text.Font = GameFont.Medium;
			Text.Anchor = TextAnchor.UpperRight;
			string text = "MainPageCredit".Translate();
			if (UI.screenWidth < 990)
			{
				Rect position = rect2;
				position.xMin = position.xMax - Text.CalcSize(text).x;
				position.xMin -= 4f;
				position.xMax += 4f;
				GUI.color = new Color(0.2f, 0.2f, 0.2f, 0.5f);
				GUI.DrawTexture(position, BaseContent.WhiteTex);
				GUI.color = Color.white;
			}
			Widgets.Label(rect2, text);
			Text.Anchor = TextAnchor.UpperLeft;
			Text.Font = GameFont.Small;
			Vector2 a = MainMenuDrawer.TitleSize;
			if (a.x > (float)UI.screenWidth)
			{
				a *= (float)UI.screenWidth / a.x;
			}
			a *= 0.7f;
			Rect position2 = new Rect((float)UI.screenWidth - a.x - 50f, rect2.y - a.y, a.x, a.y);
			GUI.DrawTexture(position2, MainMenuDrawer.TexTitle, ScaleMode.StretchToFill, true);
			GUI.color = new Color(1f, 1f, 1f, 0.5f);
			Rect position3 = new Rect((float)(UI.screenWidth - 8) - MainMenuDrawer.LudeonLogoSize.x, 8f, MainMenuDrawer.LudeonLogoSize.x, MainMenuDrawer.LudeonLogoSize.y);
			GUI.DrawTexture(position3, MainMenuDrawer.TexLudeonLogo, ScaleMode.StretchToFill, true);
			GUI.color = Color.white;
			rect.yMin += 17f;
			MainMenuDrawer.DoMainMenuControls(rect, MainMenuDrawer.anyMapFiles);
		}

		public static void DoMainMenuControls(Rect rect, bool anyMapFiles)
		{
			GUI.BeginGroup(rect);
			Rect rect2 = new Rect(0f, 0f, 170f, rect.height);
			Rect rect3 = new Rect(rect2.xMax + 17f, 0f, 145f, rect.height);
			Text.Font = GameFont.Small;
			List<ListableOption> list = new List<ListableOption>();
			if (Current.ProgramState == ProgramState.Entry)
			{
				string label;
				if (!"Tutorial".CanTranslate())
				{
					label = "LearnToPlay".Translate();
				}
				else
				{
					label = "Tutorial".Translate();
				}
				list.Add(new ListableOption(label, delegate()
				{
					MainMenuDrawer.InitLearnToPlay();
				}, null));
				list.Add(new ListableOption("NewColony".Translate(), delegate()
				{
					Find.WindowStack.Add(new Page_SelectScenario());
				}, null));
			}
			if (Current.ProgramState == ProgramState.Playing)
			{
				if (!Current.Game.Info.permadeathMode)
				{
					list.Add(new ListableOption("Save".Translate(), delegate()
					{
						MainMenuDrawer.CloseMainTab();
						Find.WindowStack.Add(new Dialog_SaveFileList_Save());
					}, null));
				}
			}
			ListableOption item;
			if (anyMapFiles && (Current.ProgramState != ProgramState.Playing || !Current.Game.Info.permadeathMode))
			{
				item = new ListableOption("LoadGame".Translate(), delegate()
				{
					MainMenuDrawer.CloseMainTab();
					Find.WindowStack.Add(new Dialog_SaveFileList_Load());
				}, null);
				list.Add(item);
			}
			if (Current.ProgramState == ProgramState.Playing)
			{
				list.Add(new ListableOption("ReviewScenario".Translate(), delegate()
				{
					WindowStack windowStack = Find.WindowStack;
					string fullInformationText = Find.Scenario.GetFullInformationText();
					string name = Find.Scenario.name;
					windowStack.Add(new Dialog_MessageBox(fullInformationText, null, null, null, null, name, false, null, null));
				}, null));
			}
			item = new ListableOption("Options".Translate(), delegate()
			{
				MainMenuDrawer.CloseMainTab();
				Find.WindowStack.Add(new Dialog_Options());
			}, "MenuButton-Options");
			list.Add(item);
			if (Current.ProgramState == ProgramState.Entry)
			{
				item = new ListableOption("Mods".Translate(), delegate()
				{
					Find.WindowStack.Add(new Page_ModsConfig());
				}, null);
				list.Add(item);
				if (Prefs.DevMode && LanguageDatabase.activeLanguage != LanguageDatabase.defaultLanguage)
				{
					item = new ListableOption("SaveTranslationReport".Translate(), delegate()
					{
						LanguageReportGenerator.SaveTranslationReport();
					}, null);
					list.Add(item);
				}
				item = new ListableOption("Credits".Translate(), delegate()
				{
					Find.WindowStack.Add(new Screen_Credits());
				}, null);
				list.Add(item);
			}
			if (Current.ProgramState == ProgramState.Playing)
			{
				if (Current.Game.Info.permadeathMode)
				{
					item = new ListableOption("SaveAndQuitToMainMenu".Translate(), delegate()
					{
						LongEventHandler.QueueLongEvent(delegate()
						{
							GameDataSaveLoader.SaveGame(Current.Game.Info.permadeathModeUniqueName);
							MemoryUtility.ClearAllMapsAndWorld();
						}, "Entry", "SavingLongEvent", false, null);
					}, null);
					list.Add(item);
					item = new ListableOption("SaveAndQuitToOS".Translate(), delegate()
					{
						LongEventHandler.QueueLongEvent(delegate()
						{
							GameDataSaveLoader.SaveGame(Current.Game.Info.permadeathModeUniqueName);
							LongEventHandler.ExecuteWhenFinished(delegate
							{
								Root.Shutdown();
							});
						}, "SavingLongEvent", false, null);
					}, null);
					list.Add(item);
				}
				else
				{
					Action action = delegate()
					{
						if (GameDataSaveLoader.CurrentGameStateIsValuable)
						{
							Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("ConfirmQuit".Translate(), delegate
							{
								GenScene.GoToMainMenu();
							}, true, null));
						}
						else
						{
							GenScene.GoToMainMenu();
						}
					};
					item = new ListableOption("QuitToMainMenu".Translate(), action, null);
					list.Add(item);
					Action action2 = delegate()
					{
						if (GameDataSaveLoader.CurrentGameStateIsValuable)
						{
							Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("ConfirmQuit".Translate(), delegate
							{
								Root.Shutdown();
							}, true, null));
						}
						else
						{
							Root.Shutdown();
						}
					};
					item = new ListableOption("QuitToOS".Translate(), action2, null);
					list.Add(item);
				}
			}
			else
			{
				item = new ListableOption("QuitToOS".Translate(), delegate()
				{
					Root.Shutdown();
				}, null);
				list.Add(item);
			}
			OptionListingUtility.DrawOptionListing(rect2, list);
			Text.Font = GameFont.Small;
			List<ListableOption> list2 = new List<ListableOption>();
			ListableOption item2 = new ListableOption_WebLink("FictionPrimer".Translate(), "http://rimworldgame.com/backstory", TexButton.IconBlog);
			list2.Add(item2);
			item2 = new ListableOption_WebLink("LudeonBlog".Translate(), "http://ludeon.com/blog", TexButton.IconBlog);
			list2.Add(item2);
			item2 = new ListableOption_WebLink("Forums".Translate(), "http://ludeon.com/forums", TexButton.IconForums);
			list2.Add(item2);
			item2 = new ListableOption_WebLink("OfficialWiki".Translate(), "http://rimworldwiki.com", TexButton.IconBlog);
			list2.Add(item2);
			item2 = new ListableOption_WebLink("TynansTwitter".Translate(), "https://twitter.com/TynanSylvester", TexButton.IconTwitter);
			list2.Add(item2);
			item2 = new ListableOption_WebLink("TynansDesignBook".Translate(), "http://tynansylvester.com/book", TexButton.IconBook);
			list2.Add(item2);
			item2 = new ListableOption_WebLink("HelpTranslate".Translate(), "http://ludeon.com/forums/index.php?topic=2933.0", TexButton.IconForums);
			list2.Add(item2);
			item2 = new ListableOption_WebLink("BuySoundtrack".Translate(), "http://www.lasgameaudio.co.uk/#!store/t04fw", TexButton.IconSoundtrack);
			list2.Add(item2);
			float num = OptionListingUtility.DrawOptionListing(rect3, list2);
			GUI.BeginGroup(rect3);
			if (Current.ProgramState == ProgramState.Entry)
			{
				if (Widgets.ButtonImage(new Rect(0f, num + 10f, 64f, 32f), LanguageDatabase.activeLanguage.icon))
				{
					List<FloatMenuOption> list3 = new List<FloatMenuOption>();
					foreach (LoadedLanguage localLang2 in LanguageDatabase.AllLoadedLanguages)
					{
						LoadedLanguage localLang = localLang2;
						list3.Add(new FloatMenuOption(localLang.FriendlyNameNative, delegate()
						{
							LanguageDatabase.SelectLanguage(localLang);
							Prefs.Save();
						}, MenuOptionPriority.Default, null, null, 0f, null, null));
					}
					Find.WindowStack.Add(new FloatMenu(list3));
				}
			}
			GUI.EndGroup();
			GUI.EndGroup();
		}

		private static void InitLearnToPlay()
		{
			Current.Game = new Game();
			Current.Game.InitData = new GameInitData();
			Current.Game.Scenario = ScenarioDefOf.Crashlanded.scenario;
			Find.Scenario.PreConfigure();
			Current.Game.storyteller = new Storyteller(StorytellerDefOf.Tutor, DifficultyDefOf.Easy);
			Page firstConfigPage = Current.Game.Scenario.GetFirstConfigPage();
			Page next = firstConfigPage.next;
			next.prev = null;
			Find.WindowStack.Add(next);
		}

		private static void CloseMainTab()
		{
			if (Current.ProgramState == ProgramState.Playing)
			{
				Find.MainTabsRoot.EscapeCurrentTab(false);
			}
		}

		// Note: this type is marked as 'beforefieldinit'.
		static MainMenuDrawer()
		{
		}

		[CompilerGenerated]
		private static void <DoMainMenuControls>m__0()
		{
			MainMenuDrawer.InitLearnToPlay();
		}

		[CompilerGenerated]
		private static void <DoMainMenuControls>m__1()
		{
			Find.WindowStack.Add(new Page_SelectScenario());
		}

		[CompilerGenerated]
		private static void <DoMainMenuControls>m__2()
		{
			MainMenuDrawer.CloseMainTab();
			Find.WindowStack.Add(new Dialog_SaveFileList_Save());
		}

		[CompilerGenerated]
		private static void <DoMainMenuControls>m__3()
		{
			MainMenuDrawer.CloseMainTab();
			Find.WindowStack.Add(new Dialog_SaveFileList_Load());
		}

		[CompilerGenerated]
		private static void <DoMainMenuControls>m__4()
		{
			WindowStack windowStack = Find.WindowStack;
			string fullInformationText = Find.Scenario.GetFullInformationText();
			string name = Find.Scenario.name;
			windowStack.Add(new Dialog_MessageBox(fullInformationText, null, null, null, null, name, false, null, null));
		}

		[CompilerGenerated]
		private static void <DoMainMenuControls>m__5()
		{
			MainMenuDrawer.CloseMainTab();
			Find.WindowStack.Add(new Dialog_Options());
		}

		[CompilerGenerated]
		private static void <DoMainMenuControls>m__6()
		{
			Find.WindowStack.Add(new Page_ModsConfig());
		}

		[CompilerGenerated]
		private static void <DoMainMenuControls>m__7()
		{
			LanguageReportGenerator.SaveTranslationReport();
		}

		[CompilerGenerated]
		private static void <DoMainMenuControls>m__8()
		{
			Find.WindowStack.Add(new Screen_Credits());
		}

		[CompilerGenerated]
		private static void <DoMainMenuControls>m__9()
		{
			LongEventHandler.QueueLongEvent(delegate()
			{
				GameDataSaveLoader.SaveGame(Current.Game.Info.permadeathModeUniqueName);
				MemoryUtility.ClearAllMapsAndWorld();
			}, "Entry", "SavingLongEvent", false, null);
		}

		[CompilerGenerated]
		private static void <DoMainMenuControls>m__A()
		{
			LongEventHandler.QueueLongEvent(delegate()
			{
				GameDataSaveLoader.SaveGame(Current.Game.Info.permadeathModeUniqueName);
				LongEventHandler.ExecuteWhenFinished(delegate
				{
					Root.Shutdown();
				});
			}, "SavingLongEvent", false, null);
		}

		[CompilerGenerated]
		private static void <DoMainMenuControls>m__B()
		{
			if (GameDataSaveLoader.CurrentGameStateIsValuable)
			{
				Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("ConfirmQuit".Translate(), delegate
				{
					GenScene.GoToMainMenu();
				}, true, null));
			}
			else
			{
				GenScene.GoToMainMenu();
			}
		}

		[CompilerGenerated]
		private static void <DoMainMenuControls>m__C()
		{
			if (GameDataSaveLoader.CurrentGameStateIsValuable)
			{
				Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("ConfirmQuit".Translate(), delegate
				{
					Root.Shutdown();
				}, true, null));
			}
			else
			{
				Root.Shutdown();
			}
		}

		[CompilerGenerated]
		private static void <DoMainMenuControls>m__D()
		{
			Root.Shutdown();
		}

		[CompilerGenerated]
		private static void <DoMainMenuControls>m__E()
		{
			GameDataSaveLoader.SaveGame(Current.Game.Info.permadeathModeUniqueName);
			MemoryUtility.ClearAllMapsAndWorld();
		}

		[CompilerGenerated]
		private static void <DoMainMenuControls>m__F()
		{
			GameDataSaveLoader.SaveGame(Current.Game.Info.permadeathModeUniqueName);
			LongEventHandler.ExecuteWhenFinished(delegate
			{
				Root.Shutdown();
			});
		}

		[CompilerGenerated]
		private static void <DoMainMenuControls>m__10()
		{
			GenScene.GoToMainMenu();
		}

		[CompilerGenerated]
		private static void <DoMainMenuControls>m__11()
		{
			Root.Shutdown();
		}

		[CompilerGenerated]
		private static void <DoMainMenuControls>m__12()
		{
			Root.Shutdown();
		}

		[CompilerGenerated]
		private sealed class <DoMainMenuControls>c__AnonStorey0
		{
			internal LoadedLanguage localLang;

			public <DoMainMenuControls>c__AnonStorey0()
			{
			}

			internal void <>m__0()
			{
				LanguageDatabase.SelectLanguage(this.localLang);
				Prefs.Save();
			}
		}
	}
}
