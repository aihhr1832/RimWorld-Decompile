﻿using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	// Token: 0x020007FF RID: 2047
	public class Dialog_DefineBinding : Window
	{
		// Token: 0x06002D89 RID: 11657 RVA: 0x0017F398 File Offset: 0x0017D798
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

		// Token: 0x1700073F RID: 1855
		// (get) Token: 0x06002D8A RID: 11658 RVA: 0x0017F3FC File Offset: 0x0017D7FC
		public override Vector2 InitialSize
		{
			get
			{
				return this.windowSize;
			}
		}

		// Token: 0x17000740 RID: 1856
		// (get) Token: 0x06002D8B RID: 11659 RVA: 0x0017F418 File Offset: 0x0017D818
		protected override float Margin
		{
			get
			{
				return 0f;
			}
		}

		// Token: 0x06002D8C RID: 11660 RVA: 0x0017F434 File Offset: 0x0017D834
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

		// Token: 0x04001813 RID: 6163
		protected Vector2 windowSize = new Vector2(400f, 200f);

		// Token: 0x04001814 RID: 6164
		protected KeyPrefsData keyPrefsData;

		// Token: 0x04001815 RID: 6165
		protected KeyBindingDef keyDef;

		// Token: 0x04001816 RID: 6166
		protected KeyPrefs.BindingSlot slot;
	}
}
