﻿using System;
using System.Globalization;
using UnityEngine;

namespace Verse
{
	// Token: 0x02000F35 RID: 3893
	public static class GenColor
	{
		// Token: 0x06005DB8 RID: 23992 RVA: 0x002F9C58 File Offset: 0x002F8058
		public static Color SaturationChanged(this Color col, float change)
		{
			float num = col.r;
			float num2 = col.g;
			float num3 = col.b;
			float num4 = Mathf.Sqrt(num * num * 0.299f + num2 * num2 * 0.587f + num3 * num3 * 0.114f);
			num = num4 + (num - num4) * change;
			num2 = num4 + (num2 - num4) * change;
			num3 = num4 + (num3 - num4) * change;
			return new Color(num, num2, num3);
		}

		// Token: 0x06005DB9 RID: 23993 RVA: 0x002F9CCC File Offset: 0x002F80CC
		public static bool IndistinguishableFrom(this Color colA, Color colB)
		{
			Color color = colA - colB;
			return Mathf.Abs(color.r) + Mathf.Abs(color.g) + Mathf.Abs(color.b) + Mathf.Abs(color.a) < 0.001f;
		}

		// Token: 0x06005DBA RID: 23994 RVA: 0x002F9D24 File Offset: 0x002F8124
		public static Color RandomColorOpaque()
		{
			return new Color(Rand.Value, Rand.Value, Rand.Value, 1f);
		}

		// Token: 0x06005DBB RID: 23995 RVA: 0x002F9D54 File Offset: 0x002F8154
		public static Color FromBytes(int r, int g, int b, int a = 255)
		{
			return new Color
			{
				r = (float)r / 255f,
				g = (float)g / 255f,
				b = (float)b / 255f,
				a = (float)a / 255f
			};
		}

		// Token: 0x06005DBC RID: 23996 RVA: 0x002F9DB0 File Offset: 0x002F81B0
		public static Color FromHex(string hex)
		{
			if (hex.StartsWith("#"))
			{
				hex = hex.Substring(1);
			}
			Color result;
			if (hex.Length != 6 && hex.Length != 8)
			{
				Log.Error(hex + " is not a valid hex color.", false);
				result = Color.white;
			}
			else
			{
				int r = int.Parse(hex.Substring(0, 2), NumberStyles.HexNumber);
				int g = int.Parse(hex.Substring(2, 2), NumberStyles.HexNumber);
				int b = int.Parse(hex.Substring(4, 2), NumberStyles.HexNumber);
				int a = 255;
				if (hex.Length == 8)
				{
					a = int.Parse(hex.Substring(6, 2), NumberStyles.HexNumber);
				}
				result = GenColor.FromBytes(r, g, b, a);
			}
			return result;
		}
	}
}
