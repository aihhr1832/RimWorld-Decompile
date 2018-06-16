﻿using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;

namespace Verse
{
	// Token: 0x02000E16 RID: 3606
	[StaticConstructorOnStartup]
	public static class DesignatorUtility
	{
		// Token: 0x060051BE RID: 20926 RVA: 0x0029D840 File Offset: 0x0029BC40
		public static Designator FindAllowedDesignator<T>() where T : Designator
		{
			List<DesignationCategoryDef> allDefsListForReading = DefDatabase<DesignationCategoryDef>.AllDefsListForReading;
			GameRules rules = Current.Game.Rules;
			for (int i = 0; i < allDefsListForReading.Count; i++)
			{
				List<Designator> allResolvedDesignators = allDefsListForReading[i].AllResolvedDesignators;
				for (int j = 0; j < allResolvedDesignators.Count; j++)
				{
					if (rules.DesignatorAllowed(allResolvedDesignators[j]))
					{
						T t = allResolvedDesignators[j] as T;
						if (t != null)
						{
							return t;
						}
					}
				}
			}
			Designator designator = DesignatorUtility.StandaloneDesignators.TryGetValue(typeof(T), null);
			if (designator == null)
			{
				designator = (Activator.CreateInstance(typeof(T)) as Designator);
				DesignatorUtility.StandaloneDesignators[typeof(T)] = designator;
			}
			return designator;
		}

		// Token: 0x060051BF RID: 20927 RVA: 0x0029D940 File Offset: 0x0029BD40
		public static void RenderHighlightOverSelectableCells(Designator designator, List<IntVec3> dragCells)
		{
			for (int i = 0; i < dragCells.Count; i++)
			{
				Vector3 position = dragCells[i].ToVector3Shifted();
				position.y = AltitudeLayer.MetaOverlays.AltitudeFor();
				Graphics.DrawMesh(MeshPool.plane10, position, Quaternion.identity, DesignatorUtility.DragHighlightCellMat, 0);
			}
		}

		// Token: 0x060051C0 RID: 20928 RVA: 0x0029D99C File Offset: 0x0029BD9C
		public static void RenderHighlightOverSelectableThings(Designator designator, List<IntVec3> dragCells)
		{
			DesignatorUtility.selectedThings.Clear();
			for (int i = 0; i < dragCells.Count; i++)
			{
				List<Thing> thingList = dragCells[i].GetThingList(designator.Map);
				for (int j = 0; j < thingList.Count; j++)
				{
					if (designator.CanDesignateThing(thingList[j]).Accepted && !DesignatorUtility.selectedThings.Contains(thingList[j]))
					{
						DesignatorUtility.selectedThings.Add(thingList[j]);
						Vector3 drawPos = thingList[j].DrawPos;
						drawPos.y = AltitudeLayer.MetaOverlays.AltitudeFor();
						Graphics.DrawMesh(MeshPool.plane10, drawPos, Quaternion.identity, DesignatorUtility.DragHighlightThingMat, 0);
					}
				}
			}
			DesignatorUtility.selectedThings.Clear();
		}

		// Token: 0x04003586 RID: 13702
		public static readonly Material DragHighlightCellMat = MaterialPool.MatFrom("UI/Overlays/DragHighlightCell", ShaderDatabase.MetaOverlay);

		// Token: 0x04003587 RID: 13703
		public static readonly Material DragHighlightThingMat = MaterialPool.MatFrom("UI/Overlays/DragHighlightThing", ShaderDatabase.MetaOverlay);

		// Token: 0x04003588 RID: 13704
		private static Dictionary<Type, Designator> StandaloneDesignators = new Dictionary<Type, Designator>();

		// Token: 0x04003589 RID: 13705
		private static HashSet<Thing> selectedThings = new HashSet<Thing>();
	}
}
