﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using RimWorld;
using UnityEngine;
using UnityEngine.Profiling;

namespace Verse
{
	public class EditWindow_PackageEditor<TNewDef> : EditWindow where TNewDef : Def, new()
	{
		public ModContentPack curMod = LoadedModManager.RunningMods.First<ModContentPack>();

		private DefPackage curPackage = null;

		private Vector2 scrollPosition = default(Vector2);

		private float viewHeight;

		private string relFolder;

		private const float EditButSize = 24f;

		public EditWindow_PackageEditor(string relFolder)
		{
			this.relFolder = relFolder;
			this.onlyOneOfTypeAllowed = true;
			this.optionalTitle = "Package Editor: " + relFolder;
		}

		public override Vector2 InitialSize
		{
			get
			{
				return new Vector2(250f, 600f);
			}
		}

		public override bool IsDebug
		{
			get
			{
				return true;
			}
		}

		public override void DoWindowContents(Rect selectorInner)
		{
			Profiler.BeginSample("PackageEditorOnGUI");
			Text.Font = GameFont.Tiny;
			float width = (selectorInner.width - 4f) / 2f;
			Rect rect = new Rect(0f, 0f, width, 24f);
			string str = this.curMod.ToString();
			if (Widgets.ButtonText(rect, "Editing: " + str, true, false, true))
			{
				Messages.Message("Mod changing not implemented - it's always Core for now.", MessageTypeDefOf.RejectInput, false);
			}
			TooltipHandler.TipRegion(rect, "Change the mod being edited.");
			Rect rect2 = new Rect(rect.xMax + 4f, 0f, width, 24f);
			string label = "No package loaded";
			if (this.curPackage != null)
			{
				label = this.curPackage.fileName;
			}
			if (Widgets.ButtonText(rect2, label, true, false, true))
			{
				Find.WindowStack.Add(new Dialog_PackageSelector(delegate(DefPackage pack)
				{
					if (pack != this.curPackage)
					{
						this.curPackage = pack;
					}
				}, this.curMod, this.relFolder));
			}
			TooltipHandler.TipRegion(rect2, "Open a Def package for editing.");
			WidgetRow widgetRow = new WidgetRow(0f, 28f, UIDirection.RightThenUp, 99999f, 4f);
			if (widgetRow.ButtonIcon(TexButton.NewFile, "Create a new Def package.", null))
			{
				string name = DefPackage.UnusedPackageName(this.relFolder, this.curMod);
				DefPackage defPackage = new DefPackage(name, this.relFolder);
				this.curMod.AddDefPackage(defPackage);
				this.curPackage = defPackage;
			}
			if (this.curPackage != null)
			{
				if (widgetRow.ButtonIcon(TexButton.Save, "Save the current Def package.", null))
				{
					this.curPackage.SaveIn(this.curMod);
				}
				if (widgetRow.ButtonIcon(TexButton.RenameDev, "Rename the current Def package.", null))
				{
					Find.WindowStack.Add(new Dialog_RenamePackage(this.curPackage));
				}
			}
			float num = 56f;
			Rect rect3 = new Rect(0f, num, selectorInner.width, selectorInner.height - num);
			Rect rect4 = new Rect(0f, 0f, rect3.width - 16f, this.viewHeight);
			Widgets.DrawMenuSection(rect3);
			Widgets.BeginScrollView(rect3, ref this.scrollPosition, rect4, true);
			Rect rect5 = rect4.ContractedBy(4f);
			rect5.height = 9999f;
			Listing_Standard listing_Standard = new Listing_Standard();
			listing_Standard.Begin(rect5);
			Text.Font = GameFont.Tiny;
			if (this.curPackage == null)
			{
				listing_Standard.Label("(no package open)", -1f, null);
			}
			else
			{
				if (this.curPackage.defs.Count == 0)
				{
					listing_Standard.Label("(package is empty)", -1f, null);
				}
				else
				{
					Def deletingDef = null;
					using (List<Def>.Enumerator enumerator = this.curPackage.GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							Def def = enumerator.Current;
							if (listing_Standard.SelectableDef(def.defName, false, delegate
							{
								deletingDef = def;
							}))
							{
								bool flag = false;
								WindowStack windowStack = Find.WindowStack;
								for (int i = 0; i < windowStack.Count; i++)
								{
									EditWindow_DefEditor editWindow_DefEditor = windowStack[i] as EditWindow_DefEditor;
									if (editWindow_DefEditor != null && editWindow_DefEditor.def == def)
									{
										flag = true;
									}
								}
								if (!flag)
								{
									Find.WindowStack.Add(new EditWindow_DefEditor(def));
								}
							}
						}
					}
					if (deletingDef != null)
					{
						Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("Really delete Def " + deletingDef.defName + "?", delegate
						{
							this.curPackage.RemoveDef(deletingDef);
						}, true, null));
					}
				}
				if (listing_Standard.ButtonImage(TexButton.Add, 24f, 24f))
				{
					Def def2 = Activator.CreateInstance<TNewDef>();
					def2.defName = "New" + typeof(TNewDef).Name;
					this.curPackage.AddDef(def2);
				}
			}
			if (Event.current.type == EventType.Layout)
			{
				this.viewHeight = listing_Standard.CurHeight;
			}
			listing_Standard.End();
			Widgets.EndScrollView();
			Profiler.EndSample();
		}

		[CompilerGenerated]
		private void <DoWindowContents>m__0(DefPackage pack)
		{
			if (pack != this.curPackage)
			{
				this.curPackage = pack;
			}
		}

		[CompilerGenerated]
		private sealed class <DoWindowContents>c__AnonStorey1
		{
			internal Def deletingDef;

			internal EditWindow_PackageEditor<TNewDef> $this;

			public <DoWindowContents>c__AnonStorey1()
			{
			}

			internal void <>m__0()
			{
				this.$this.curPackage.RemoveDef(this.deletingDef);
			}
		}

		[CompilerGenerated]
		private sealed class <DoWindowContents>c__AnonStorey0
		{
			internal Def def;

			internal EditWindow_PackageEditor<TNewDef>.<DoWindowContents>c__AnonStorey1 <>f__ref$1;

			public <DoWindowContents>c__AnonStorey0()
			{
			}

			internal void <>m__0()
			{
				this.<>f__ref$1.deletingDef = this.def;
			}
		}
	}
}
