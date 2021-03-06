﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public abstract class Designator_ZoneAdd : Designator_Zone
	{
		protected Type zoneTypeToPlace;

		public Designator_ZoneAdd()
		{
			this.soundDragSustain = SoundDefOf.Designate_DragAreaAdd;
			this.soundDragChanged = null;
			this.soundSucceeded = SoundDefOf.Designate_ZoneAdd;
			this.useMouseIcon = true;
		}

		private Zone SelectedZone
		{
			get
			{
				return Find.Selector.SelectedZone;
			}
			set
			{
				Find.Selector.ClearSelection();
				if (value != null)
				{
					Find.Selector.Select(value, false, false);
				}
			}
		}

		protected abstract string NewZoneLabel { get; }

		protected abstract Zone MakeNewZone();

		public override void SelectedUpdate()
		{
			base.SelectedUpdate();
			if (Find.Selector.SelectedZone != null && Find.Selector.SelectedZone.GetType() != this.zoneTypeToPlace)
			{
				Find.Selector.Deselect(Find.Selector.SelectedZone);
			}
		}

		public override void DrawMouseAttachments()
		{
			if (this.useMouseIcon)
			{
				string text = "";
				if (!Input.GetKey(KeyCode.Mouse0))
				{
					Zone selectedZone = Find.Selector.SelectedZone;
					if (selectedZone != null)
					{
						text = "ExpandOrCreateZone".Translate(new object[]
						{
							selectedZone.label,
							this.NewZoneLabel
						});
					}
					else
					{
						text = "CreateNewZone".Translate(new object[]
						{
							this.NewZoneLabel
						});
					}
				}
				GenUI.DrawMouseAttachment(this.icon, text, 0f, default(Vector2), null);
			}
		}

		public override AcceptanceReport CanDesignateCell(IntVec3 c)
		{
			AcceptanceReport result;
			if (!c.InBounds(base.Map))
			{
				result = false;
			}
			else if (c.Fogged(base.Map))
			{
				result = false;
			}
			else if (c.InNoZoneEdgeArea(base.Map))
			{
				result = "TooCloseToMapEdge".Translate();
			}
			else
			{
				Zone zone = base.Map.zoneManager.ZoneAt(c);
				if (zone != null && zone.GetType() != this.zoneTypeToPlace)
				{
					result = false;
				}
				else
				{
					foreach (Thing thing in base.Map.thingGrid.ThingsAt(c))
					{
						if (!thing.def.CanOverlapZones)
						{
							return false;
						}
					}
					result = true;
				}
			}
			return result;
		}

		public override void DesignateMultiCell(IEnumerable<IntVec3> cells)
		{
			List<IntVec3> list = cells.ToList<IntVec3>();
			if (list.Count == 1)
			{
				Zone zone = base.Map.zoneManager.ZoneAt(list[0]);
				if (zone != null)
				{
					if (zone.GetType() == this.zoneTypeToPlace)
					{
						this.SelectedZone = zone;
					}
					return;
				}
			}
			if (this.SelectedZone == null)
			{
				Zone zone2 = null;
				foreach (IntVec3 c3 in cells)
				{
					Zone zone3 = base.Map.zoneManager.ZoneAt(c3);
					if (zone3 != null && zone3.GetType() == this.zoneTypeToPlace)
					{
						if (zone2 == null)
						{
							zone2 = zone3;
						}
						else if (zone3 != zone2)
						{
							zone2 = null;
							break;
						}
					}
				}
				this.SelectedZone = zone2;
			}
			list.RemoveAll((IntVec3 c) => base.Map.zoneManager.ZoneAt(c) != null);
			if (list.Count != 0)
			{
				if (!TutorSystem.TutorialMode || TutorSystem.AllowAction(new EventPack(base.TutorTagDesignate, list)))
				{
					if (this.SelectedZone == null)
					{
						this.SelectedZone = this.MakeNewZone();
						base.Map.zoneManager.RegisterZone(this.SelectedZone);
						this.SelectedZone.AddCell(list[0]);
						list.RemoveAt(0);
					}
					bool somethingSucceeded;
					for (;;)
					{
						somethingSucceeded = true;
						int count = list.Count;
						for (int i = list.Count - 1; i >= 0; i--)
						{
							bool flag = false;
							for (int j = 0; j < 4; j++)
							{
								IntVec3 c2 = list[i] + GenAdj.CardinalDirections[j];
								if (c2.InBounds(base.Map))
								{
									if (base.Map.zoneManager.ZoneAt(c2) == this.SelectedZone)
									{
										flag = true;
										break;
									}
								}
							}
							if (flag)
							{
								this.SelectedZone.AddCell(list[i]);
								list.RemoveAt(i);
							}
						}
						if (list.Count == 0)
						{
							break;
						}
						if (list.Count == count)
						{
							this.SelectedZone = this.MakeNewZone();
							base.Map.zoneManager.RegisterZone(this.SelectedZone);
							this.SelectedZone.AddCell(list[0]);
							list.RemoveAt(0);
						}
					}
					this.SelectedZone.CheckContiguous();
					base.Finalize(somethingSucceeded);
					TutorSystem.Notify_Event(new EventPack(base.TutorTagDesignate, list));
				}
			}
		}

		[CompilerGenerated]
		private bool <DesignateMultiCell>m__0(IntVec3 c)
		{
			return base.Map.zoneManager.ZoneAt(c) != null;
		}
	}
}
