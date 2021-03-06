﻿using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Dialog_DefineBinding : Window
	{
		protected Vector2 windowSize = new Vector2(400f, 200f);

		protected KeyPrefsData keyPrefsData;

		protected KeyBindingDef keyDef;

		protected KeyPrefs.BindingSlot slot;

		[CompilerGenerated]
		private static Action<KeyBindingDef> <>f__am$cache0;

		public Dialog_DefineBinding(KeyPrefsData keyPrefsData, KeyBindingDef keyDef, KeyPrefs.BindingSlot slot)
		{
			this.keyDef = keyDef;
			this.slot = slot;
			this.keyPrefsData = keyPrefsData;
			this.closeOnAccept = false;
			this.closeOnCancel = false;
			this.forcePause = true;
			this.onlyOneOfTypeAllowed = true;
			this.absorbInputAroundWindow = true;
		}

		public override Vector2 InitialSize
		{
			get
			{
				return this.windowSize;
			}
		}

		protected override float Margin
		{
			get
			{
				return 0f;
			}
		}

		public override void DoWindowContents(Rect inRect)
		{
			Text.Anchor = TextAnchor.MiddleCenter;
			Widgets.Label(inRect, "PressAnyKeyOrEsc".Translate());
			Text.Anchor = TextAnchor.UpperLeft;
			if (Event.current.isKey && Event.current.type == EventType.KeyDown && Event.current.keyCode != KeyCode.None)
			{
				if (Event.current.keyCode != KeyCode.Escape)
				{
					this.keyPrefsData.EraseConflictingBindingsForKeyCode(this.keyDef, Event.current.keyCode, delegate(KeyBindingDef oldDef)
					{
						Messages.Message("KeyBindingOverwritten".Translate(new object[]
						{
							oldDef.LabelCap
						}), MessageTypeDefOf.TaskCompletion, false);
					});
					this.keyPrefsData.SetBinding(this.keyDef, this.slot, Event.current.keyCode);
				}
				this.Close(true);
				Event.current.Use();
			}
		}

		[CompilerGenerated]
		private static void <DoWindowContents>m__0(KeyBindingDef oldDef)
		{
			Messages.Message("KeyBindingOverwritten".Translate(new object[]
			{
				oldDef.LabelCap
			}), MessageTypeDefOf.TaskCompletion, false);
		}
	}
}
