﻿using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	// Token: 0x0200085F RID: 2143
	public interface IInspectPane
	{
		// Token: 0x170007BD RID: 1981
		// (get) Token: 0x06003077 RID: 12407
		// (set) Token: 0x06003078 RID: 12408
		float RecentHeight { get; set; }

		// Token: 0x170007BE RID: 1982
		// (get) Token: 0x06003079 RID: 12409
		// (set) Token: 0x0600307A RID: 12410
		Type OpenTabType { get; set; }

		// Token: 0x170007BF RID: 1983
		// (get) Token: 0x0600307B RID: 12411
		bool AnythingSelected { get; }

		// Token: 0x170007C0 RID: 1984
		// (get) Token: 0x0600307C RID: 12412
		IEnumerable<InspectTabBase> CurTabs { get; }

		// Token: 0x170007C1 RID: 1985
		// (get) Token: 0x0600307D RID: 12413
		bool ShouldShowSelectNextInCellButton { get; }

		// Token: 0x170007C2 RID: 1986
		// (get) Token: 0x0600307E RID: 12414
		bool ShouldShowPaneContents { get; }

		// Token: 0x170007C3 RID: 1987
		// (get) Token: 0x0600307F RID: 12415
		float PaneTopY { get; }

		// Token: 0x06003080 RID: 12416
		void DrawInspectGizmos();

		// Token: 0x06003081 RID: 12417
		string GetLabel(Rect rect);

		// Token: 0x06003082 RID: 12418
		void DoInspectPaneButtons(Rect rect, ref float lineEndWidth);

		// Token: 0x06003083 RID: 12419
		void SelectNextInCell();

		// Token: 0x06003084 RID: 12420
		void DoPaneContents(Rect rect);

		// Token: 0x06003085 RID: 12421
		void CloseOpenTab();

		// Token: 0x06003086 RID: 12422
		void Reset();
	}
}
