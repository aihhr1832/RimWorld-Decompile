﻿using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse.AI;

namespace Verse
{
	// Token: 0x02000CB0 RID: 3248
	public sealed class TooltipGiverList
	{
		// Token: 0x06004781 RID: 18305 RVA: 0x0025A7C1 File Offset: 0x00258BC1
		public void Notify_ThingSpawned(Thing t)
		{
			if (t.def.hasTooltip || this.ShouldShowShotReport(t))
			{
				this.givers.Add(t);
			}
		}

		// Token: 0x06004782 RID: 18306 RVA: 0x0025A7EC File Offset: 0x00258BEC
		public void Notify_ThingDespawned(Thing t)
		{
			if (t.def.hasTooltip || this.ShouldShowShotReport(t))
			{
				this.givers.Remove(t);
			}
		}

		// Token: 0x06004783 RID: 18307 RVA: 0x0025A818 File Offset: 0x00258C18
		public void DispenseAllThingTooltips()
		{
			if (Event.current.type == EventType.Repaint)
			{
				if (Find.WindowStack.FloatMenu == null)
				{
					CellRect currentViewRect = Find.CameraDriver.CurrentViewRect;
					float cellSizePixels = Find.CameraDriver.CellSizePixels;
					Vector2 vector = new Vector2(cellSizePixels, cellSizePixels);
					Rect rect = new Rect(0f, 0f, vector.x, vector.y);
					int num = 0;
					for (int i = 0; i < this.givers.Count; i++)
					{
						Thing thing = this.givers[i];
						if (currentViewRect.Contains(thing.Position) && !thing.Position.Fogged(thing.Map))
						{
							Vector2 vector2 = thing.DrawPos.MapToUIPosition();
							rect.x = vector2.x - vector.x / 2f;
							rect.y = vector2.y - vector.y / 2f;
							if (rect.Contains(Event.current.mousePosition))
							{
								string text = (!this.ShouldShowShotReport(thing)) ? null : TooltipUtility.ShotCalculationTipString(thing);
								if (thing.def.hasTooltip || !text.NullOrEmpty())
								{
									TipSignal tooltip = thing.GetTooltip();
									if (!text.NullOrEmpty())
									{
										tooltip.text = tooltip.text + "\n\n" + text;
									}
									TooltipHandler.TipRegion(rect, tooltip);
								}
							}
							num++;
						}
					}
				}
			}
		}

		// Token: 0x06004784 RID: 18308 RVA: 0x0025A9C8 File Offset: 0x00258DC8
		private bool ShouldShowShotReport(Thing t)
		{
			return t.def.hasTooltip || t is Hive || t is IAttackTarget;
		}

		// Token: 0x04003086 RID: 12422
		private List<Thing> givers = new List<Thing>();
	}
}
