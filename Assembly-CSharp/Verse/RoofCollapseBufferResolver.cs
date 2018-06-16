﻿using System;
using System.Collections.Generic;
using System.Text;
using RimWorld;

namespace Verse
{
	// Token: 0x02000C9F RID: 3231
	public class RoofCollapseBufferResolver
	{
		// Token: 0x06004717 RID: 18199 RVA: 0x0025718B File Offset: 0x0025558B
		public RoofCollapseBufferResolver(Map map)
		{
			this.map = map;
		}

		// Token: 0x06004718 RID: 18200 RVA: 0x002571B4 File Offset: 0x002555B4
		public void CollapseRoofsMarkedToCollapse()
		{
			RoofCollapseBuffer roofCollapseBuffer = this.map.roofCollapseBuffer;
			if (roofCollapseBuffer.CellsMarkedToCollapse.Any<IntVec3>())
			{
				this.tmpCrushedThings.Clear();
				RoofCollapserImmediate.DropRoofInCells(roofCollapseBuffer.CellsMarkedToCollapse, this.map, this.tmpCrushedThings);
				if (this.tmpCrushedThings.Any<Thing>())
				{
					StringBuilder stringBuilder = new StringBuilder();
					stringBuilder.AppendLine("RoofCollapsed".Translate());
					stringBuilder.AppendLine();
					stringBuilder.AppendLine("TheseThingsCrushed".Translate());
					this.tmpCrushedNames.Clear();
					for (int i = 0; i < this.tmpCrushedThings.Count; i++)
					{
						Thing thing = this.tmpCrushedThings[i];
						string item = thing.LabelShort.CapitalizeFirst();
						if (thing.def.category == ThingCategory.Pawn)
						{
							item = thing.LabelCap;
						}
						if (!this.tmpCrushedNames.Contains(item))
						{
							this.tmpCrushedNames.Add(item);
						}
					}
					foreach (string str in this.tmpCrushedNames)
					{
						stringBuilder.AppendLine("    -" + str);
					}
					Find.LetterStack.ReceiveLetter("LetterLabelRoofCollapsed".Translate(), stringBuilder.ToString().TrimEndNewlines(), LetterDefOf.NegativeEvent, new TargetInfo(roofCollapseBuffer.CellsMarkedToCollapse[0], this.map, false), null, null);
				}
				else
				{
					string text = "RoofCollapsed".Translate();
					Messages.Message(text, new TargetInfo(roofCollapseBuffer.CellsMarkedToCollapse[0], this.map, false), MessageTypeDefOf.SilentInput, true);
				}
				this.tmpCrushedThings.Clear();
				roofCollapseBuffer.Clear();
			}
		}

		// Token: 0x04003050 RID: 12368
		private Map map;

		// Token: 0x04003051 RID: 12369
		private List<Thing> tmpCrushedThings = new List<Thing>();

		// Token: 0x04003052 RID: 12370
		private HashSet<string> tmpCrushedNames = new HashSet<string>();
	}
}
