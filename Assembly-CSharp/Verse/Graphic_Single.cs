﻿using System;
using UnityEngine;

namespace Verse
{
	public class Graphic_Single : Graphic
	{
		protected Material mat = null;

		public static readonly string MaskSuffix = "_m";

		public Graphic_Single()
		{
		}

		public override Material MatSingle
		{
			get
			{
				return this.mat;
			}
		}

		public override Material MatWest
		{
			get
			{
				return this.mat;
			}
		}

		public override Material MatSouth
		{
			get
			{
				return this.mat;
			}
		}

		public override Material MatEast
		{
			get
			{
				return this.mat;
			}
		}

		public override Material MatNorth
		{
			get
			{
				return this.mat;
			}
		}

		public override bool ShouldDrawRotated
		{
			get
			{
				return this.data == null || this.data.drawRotated;
			}
		}

		public override void Init(GraphicRequest req)
		{
			this.data = req.graphicData;
			this.path = req.path;
			this.color = req.color;
			this.colorTwo = req.colorTwo;
			this.drawSize = req.drawSize;
			MaterialRequest req2 = default(MaterialRequest);
			req2.mainTex = ContentFinder<Texture2D>.Get(req.path, true);
			req2.shader = req.shader;
			req2.color = this.color;
			req2.colorTwo = this.colorTwo;
			req2.renderQueue = req.renderQueue;
			req2.shaderParameters = req.shaderParameters;
			if (req.shader.SupportsMaskTex())
			{
				req2.maskTex = ContentFinder<Texture2D>.Get(req.path + Graphic_Single.MaskSuffix, false);
			}
			this.mat = MaterialPool.MatFrom(req2);
		}

		public override Graphic GetColoredVersion(Shader newShader, Color newColor, Color newColorTwo)
		{
			return GraphicDatabase.Get<Graphic_Single>(this.path, newShader, this.drawSize, newColor, newColorTwo, this.data);
		}

		public override Material MatAt(Rot4 rot, Thing thing = null)
		{
			return this.mat;
		}

		public override string ToString()
		{
			return string.Concat(new object[]
			{
				"Single(path=",
				this.path,
				", color=",
				this.color,
				", colorTwo=",
				this.colorTwo,
				")"
			});
		}

		// Note: this type is marked as 'beforefieldinit'.
		static Graphic_Single()
		{
		}
	}
}
