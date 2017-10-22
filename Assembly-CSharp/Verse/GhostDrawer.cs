using System.Collections.Generic;
using UnityEngine;

namespace Verse
{
	public static class GhostDrawer
	{
		private static Dictionary<int, Graphic> ghostGraphics = new Dictionary<int, Graphic>();

		public static void DrawGhostThing(IntVec3 center, Rot4 rot, ThingDef thingDef, Graphic baseGraphic, Color ghostCol, AltitudeLayer drawAltitude)
		{
			if (baseGraphic == null)
			{
				baseGraphic = thingDef.graphic;
			}
			Graphic graphic = GhostDrawer.GhostGraphicFor(baseGraphic, thingDef, ghostCol);
			Vector3 loc = Gen.TrueCenter(center, rot, thingDef.Size, Altitudes.AltitudeFor(drawAltitude));
			graphic.DrawFromDef(loc, rot, thingDef);
			if (thingDef.PlaceWorkers != null)
			{
				for (int i = 0; i < thingDef.PlaceWorkers.Count; i++)
				{
					thingDef.PlaceWorkers[i].DrawGhost(thingDef, center, rot);
				}
			}
		}

		private static Graphic GhostGraphicFor(Graphic baseGraphic, ThingDef thingDef, Color ghostCol)
		{
			int seed = 0;
			seed = Gen.HashCombine(seed, baseGraphic);
			seed = Gen.HashCombine(seed, thingDef);
			seed = Gen.HashCombineStruct(seed, ghostCol);
			Graphic graphic = default(Graphic);
			if (!GhostDrawer.ghostGraphics.TryGetValue(seed, out graphic))
			{
				if (thingDef.graphicData.Linked || thingDef.IsDoor)
				{
					graphic = GraphicDatabase.Get<Graphic_Single>(thingDef.uiIconPath, ShaderDatabase.Transparent, thingDef.graphicData.drawSize, ghostCol);
				}
				else
				{
					if (baseGraphic == null)
					{
						baseGraphic = thingDef.graphic;
					}
					GraphicData graphicData = null;
					if (baseGraphic.data != null && baseGraphic.ShadowGraphic != null)
					{
						graphicData = new GraphicData();
						graphicData.CopyFrom(baseGraphic.data);
						graphicData.shadowData = null;
					}
					graphic = GraphicDatabase.Get(baseGraphic.GetType(), baseGraphic.path, ShaderDatabase.Transparent, baseGraphic.drawSize, ghostCol, Color.white, graphicData);
				}
				GhostDrawer.ghostGraphics.Add(seed, graphic);
			}
			return graphic;
		}
	}
}
