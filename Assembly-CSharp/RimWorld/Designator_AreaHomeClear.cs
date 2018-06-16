﻿using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	// Token: 0x020007C4 RID: 1988
	public class Designator_AreaHomeClear : Designator_AreaHome
	{
		// Token: 0x06002BFB RID: 11259 RVA: 0x00174380 File Offset: 0x00172780
		public Designator_AreaHomeClear() : base(DesignateMode.Remove)
		{
			this.defaultLabel = "DesignatorAreaHomeClear".Translate();
			this.defaultDesc = "DesignatorAreaHomeClearDesc".Translate();
			this.icon = ContentFinder<Texture2D>.Get("UI/Designators/HomeAreaOff", true);
			this.soundDragSustain = SoundDefOf.Designate_DragAreaDelete;
			this.soundDragChanged = null;
			this.soundSucceeded = SoundDefOf.Designate_AreaDelete;
		}
	}
}
