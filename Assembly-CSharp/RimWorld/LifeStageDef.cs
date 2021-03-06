﻿using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class LifeStageDef : Def
	{
		[MustTranslate]
		private string adjective = null;

		public bool visible = true;

		public bool reproductive = false;

		public bool milkable = false;

		public bool shearable = false;

		public float voxPitch = 1f;

		public float voxVolume = 1f;

		[NoTranslate]
		public string icon;

		[Unsaved]
		public Texture2D iconTex;

		public List<StatModifier> statFactors = new List<StatModifier>();

		public float bodySizeFactor = 1f;

		public float healthScaleFactor = 1f;

		public float hungerRateFactor = 1f;

		public float marketValueFactor = 1f;

		public float foodMaxFactor = 1f;

		public float meleeDamageFactor = 1f;

		public LifeStageDef()
		{
		}

		public string Adjective
		{
			get
			{
				return this.adjective ?? this.label;
			}
		}

		public override void ResolveReferences()
		{
			base.ResolveReferences();
			if (!this.icon.NullOrEmpty())
			{
				LongEventHandler.ExecuteWhenFinished(delegate
				{
					this.iconTex = ContentFinder<Texture2D>.Get(this.icon, true);
				});
			}
		}

		[CompilerGenerated]
		private void <ResolveReferences>m__0()
		{
			this.iconTex = ContentFinder<Texture2D>.Get(this.icon, true);
		}
	}
}
