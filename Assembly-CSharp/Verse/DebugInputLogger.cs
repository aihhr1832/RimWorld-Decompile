﻿using System;
using UnityEngine;

namespace Verse
{
	// Token: 0x02000E97 RID: 3735
	internal static class DebugInputLogger
	{
		// Token: 0x06005804 RID: 22532 RVA: 0x002D1658 File Offset: 0x002CFA58
		public static void InputLogOnGUI()
		{
			if (DebugViewSettings.logInput)
			{
				if (Event.current.type == EventType.MouseDown || Event.current.type == EventType.MouseUp || Event.current.type == EventType.KeyDown || Event.current.type == EventType.KeyUp || Event.current.type == EventType.ScrollWheel)
				{
					Log.Message(string.Concat(new object[]
					{
						"Frame ",
						Time.frameCount,
						": ",
						Event.current.ToStringFull()
					}), false);
				}
			}
		}

		// Token: 0x06005805 RID: 22533 RVA: 0x002D1704 File Offset: 0x002CFB04
		public static string ToStringFull(this Event ev)
		{
			return string.Concat(new object[]
			{
				"(EVENT\ntype=",
				ev.type,
				"\nbutton=",
				ev.button,
				"\nkeyCode=",
				ev.keyCode,
				"\ndelta=",
				ev.delta,
				"\nalt=",
				ev.alt,
				"\ncapsLock=",
				ev.capsLock,
				"\ncharacter=",
				(ev.character == '\0') ? ' ' : ev.character,
				"\nclickCount=",
				ev.clickCount,
				"\ncommand=",
				ev.command,
				"\ncommandName=",
				ev.commandName,
				"\ncontrol=",
				ev.control,
				"\nfunctionKey=",
				ev.functionKey,
				"\nisKey=",
				ev.isKey,
				"\nisMouse=",
				ev.isMouse,
				"\nmodifiers=",
				ev.modifiers,
				"\nmousePosition=",
				ev.mousePosition,
				"\nnumeric=",
				ev.numeric,
				"\npressure=",
				ev.pressure,
				"\nrawType=",
				ev.rawType,
				"\nshift=",
				ev.shift,
				"\n)"
			});
		}
	}
}
