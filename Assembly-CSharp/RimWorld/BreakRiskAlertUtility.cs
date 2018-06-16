﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace RimWorld
{
	// Token: 0x020007B0 RID: 1968
	public static class BreakRiskAlertUtility
	{
		// Token: 0x170006C1 RID: 1729
		// (get) Token: 0x06002B70 RID: 11120 RVA: 0x0016EEA0 File Offset: 0x0016D2A0
		public static IEnumerable<Pawn> PawnsAtRiskExtreme
		{
			get
			{
				foreach (Pawn p in PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonists_NoCryptosleep)
				{
					if (!p.Downed && p.mindState.mentalBreaker.BreakExtremeIsImminent)
					{
						yield return p;
					}
				}
				yield break;
			}
		}

		// Token: 0x170006C2 RID: 1730
		// (get) Token: 0x06002B71 RID: 11121 RVA: 0x0016EEC4 File Offset: 0x0016D2C4
		public static IEnumerable<Pawn> PawnsAtRiskMajor
		{
			get
			{
				foreach (Pawn p in PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonists_NoCryptosleep)
				{
					if (!p.Downed && p.mindState.mentalBreaker.BreakMajorIsImminent)
					{
						yield return p;
					}
				}
				yield break;
			}
		}

		// Token: 0x170006C3 RID: 1731
		// (get) Token: 0x06002B72 RID: 11122 RVA: 0x0016EEE8 File Offset: 0x0016D2E8
		public static IEnumerable<Pawn> PawnsAtRiskMinor
		{
			get
			{
				foreach (Pawn p in PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonists_NoCryptosleep)
				{
					if (!p.Downed && p.mindState.mentalBreaker.BreakMinorIsImminent)
					{
						yield return p;
					}
				}
				yield break;
			}
		}

		// Token: 0x170006C4 RID: 1732
		// (get) Token: 0x06002B73 RID: 11123 RVA: 0x0016EF0C File Offset: 0x0016D30C
		public static string AlertLabel
		{
			get
			{
				int num = BreakRiskAlertUtility.PawnsAtRiskExtreme.Count<Pawn>();
				string text;
				if (num > 0)
				{
					text = "BreakRiskExtreme".Translate();
				}
				else
				{
					num = BreakRiskAlertUtility.PawnsAtRiskMajor.Count<Pawn>();
					if (num > 0)
					{
						text = "BreakRiskMajor".Translate();
					}
					else
					{
						num = BreakRiskAlertUtility.PawnsAtRiskMinor.Count<Pawn>();
						text = "BreakRiskMinor".Translate();
					}
				}
				if (num > 1)
				{
					text = text + " x" + num.ToStringCached();
				}
				return text;
			}
		}

		// Token: 0x170006C5 RID: 1733
		// (get) Token: 0x06002B74 RID: 11124 RVA: 0x0016EF9C File Offset: 0x0016D39C
		public static string AlertExplanation
		{
			get
			{
				StringBuilder stringBuilder = new StringBuilder();
				if (BreakRiskAlertUtility.PawnsAtRiskExtreme.Any<Pawn>())
				{
					StringBuilder stringBuilder2 = new StringBuilder();
					foreach (Pawn pawn in BreakRiskAlertUtility.PawnsAtRiskExtreme)
					{
						stringBuilder2.AppendLine("    " + pawn.LabelShort);
					}
					stringBuilder.Append("BreakRiskExtremeDesc".Translate(new object[]
					{
						stringBuilder2
					}));
				}
				if (BreakRiskAlertUtility.PawnsAtRiskMajor.Any<Pawn>())
				{
					if (stringBuilder.Length != 0)
					{
						stringBuilder.AppendLine();
					}
					StringBuilder stringBuilder3 = new StringBuilder();
					foreach (Pawn pawn2 in BreakRiskAlertUtility.PawnsAtRiskMajor)
					{
						stringBuilder3.AppendLine("    " + pawn2.LabelShort);
					}
					stringBuilder.Append("BreakRiskMajorDesc".Translate(new object[]
					{
						stringBuilder3
					}));
				}
				if (BreakRiskAlertUtility.PawnsAtRiskMinor.Any<Pawn>())
				{
					if (stringBuilder.Length != 0)
					{
						stringBuilder.AppendLine();
					}
					StringBuilder stringBuilder4 = new StringBuilder();
					foreach (Pawn pawn3 in BreakRiskAlertUtility.PawnsAtRiskMinor)
					{
						stringBuilder4.AppendLine("    " + pawn3.LabelShort);
					}
					stringBuilder.Append("BreakRiskMinorDesc".Translate(new object[]
					{
						stringBuilder4
					}));
				}
				stringBuilder.AppendLine();
				stringBuilder.Append("BreakRiskDescEnding".Translate());
				return stringBuilder.ToString();
			}
		}
	}
}
