﻿using System;
using System.Collections.Generic;
using System.Text;
using Verse;

namespace RimWorld
{
	// Token: 0x02000677 RID: 1655
	public class Blueprint_Build : Blueprint
	{
		// Token: 0x17000515 RID: 1301
		// (get) Token: 0x060022BB RID: 8891 RVA: 0x0012AFF0 File Offset: 0x001293F0
		public override string Label
		{
			get
			{
				string label = base.Label;
				string result;
				if (this.stuffToUse != null)
				{
					result = "ThingMadeOfStuffLabel".Translate(new object[]
					{
						this.stuffToUse.LabelAsStuff,
						label
					});
				}
				else
				{
					result = label;
				}
				return result;
			}
		}

		// Token: 0x17000516 RID: 1302
		// (get) Token: 0x060022BC RID: 8892 RVA: 0x0012B040 File Offset: 0x00129440
		protected override float WorkTotal
		{
			get
			{
				return this.def.entityDefToBuild.GetStatValueAbstract(StatDefOf.WorkToBuild, this.stuffToUse);
			}
		}

		// Token: 0x060022BD RID: 8893 RVA: 0x0012B070 File Offset: 0x00129470
		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Defs.Look<ThingDef>(ref this.stuffToUse, "stuffToUse");
		}

		// Token: 0x060022BE RID: 8894 RVA: 0x0012B08C File Offset: 0x0012948C
		public override ThingDef UIStuff()
		{
			return this.stuffToUse;
		}

		// Token: 0x060022BF RID: 8895 RVA: 0x0012B0A8 File Offset: 0x001294A8
		public override List<ThingDefCountClass> MaterialsNeeded()
		{
			return this.def.entityDefToBuild.CostListAdjusted(this.stuffToUse, true);
		}

		// Token: 0x060022C0 RID: 8896 RVA: 0x0012B0D4 File Offset: 0x001294D4
		protected override Thing MakeSolidThing()
		{
			return ThingMaker.MakeThing(this.def.entityDefToBuild.frameDef, this.stuffToUse);
		}

		// Token: 0x060022C1 RID: 8897 RVA: 0x0012B104 File Offset: 0x00129504
		public override IEnumerable<Gizmo> GetGizmos()
		{
			foreach (Gizmo c in this.<GetGizmos>__BaseCallProxy0())
			{
				yield return c;
			}
			Command buildCopy = BuildCopyCommandUtility.BuildCopyCommand(this.def.entityDefToBuild, this.stuffToUse);
			if (buildCopy != null)
			{
				yield return buildCopy;
			}
			if (base.Faction == Faction.OfPlayer)
			{
				foreach (Command facility in BuildFacilityCommandUtility.BuildFacilityCommands(this.def.entityDefToBuild))
				{
					yield return facility;
				}
			}
			yield break;
		}

		// Token: 0x060022C2 RID: 8898 RVA: 0x0012B130 File Offset: 0x00129530
		public override string GetInspectString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(base.GetInspectString());
			if (stringBuilder.Length > 0)
			{
				stringBuilder.AppendLine();
			}
			stringBuilder.AppendLine("ContainedResources".Translate() + ":");
			bool flag = true;
			foreach (ThingDefCountClass thingDefCountClass in this.MaterialsNeeded())
			{
				if (!flag)
				{
					stringBuilder.AppendLine();
				}
				stringBuilder.Append(thingDefCountClass.thingDef.LabelCap + ": 0 / " + thingDefCountClass.count);
				flag = false;
			}
			return stringBuilder.ToString().Trim();
		}

		// Token: 0x04001392 RID: 5010
		public ThingDef stuffToUse = null;
	}
}
