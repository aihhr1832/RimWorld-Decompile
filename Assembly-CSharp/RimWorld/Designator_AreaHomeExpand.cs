﻿using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	// Token: 0x020007C3 RID: 1987
	public class Designator_AreaHomeExpand : Designator_AreaHome
	{
		// Token: 0x06002BFA RID: 11258 RVA: 0x00174310 File Offset: 0x00172710
		public Designator_AreaHomeExpand() : base(DesignateMode.Add)
		{
			this.defaultLabel = "DesignatorAreaHomeExpand".Translate();
			this.defaultDesc = "DesignatorAreaHomeExpandDesc".Translate();
			this.icon = ContentFinder<Texture2D>.Get("UI/Designators/HomeAreaOn", true);
			this.soundDragSustain = SoundDefOf.Designate_DragAreaAdd;
			this.soundDragChanged = null;
			this.soundSucceeded = SoundDefOf.Designate_AreaAdd;
			this.tutorTag = "AreaHomeExpand";
		}
	}
}
