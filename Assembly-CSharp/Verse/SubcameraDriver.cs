﻿using System;
using UnityEngine;

namespace Verse
{
	// Token: 0x02000AF0 RID: 2800
	public class SubcameraDriver : MonoBehaviour
	{
		// Token: 0x06003DF6 RID: 15862 RVA: 0x0020AC08 File Offset: 0x00209008
		public void Init()
		{
			if (this.subcameras == null)
			{
				if (PlayDataLoader.Loaded)
				{
					Camera camera = Find.Camera;
					this.subcameras = new Camera[DefDatabase<SubcameraDef>.DefCount];
					foreach (SubcameraDef subcameraDef in DefDatabase<SubcameraDef>.AllDefsListForReading)
					{
						Camera camera2 = new GameObject
						{
							name = subcameraDef.defName,
							transform = 
							{
								parent = base.transform,
								localPosition = Vector3.zero,
								localScale = Vector3.one,
								localRotation = Quaternion.identity
							}
						}.AddComponent<Camera>();
						camera2.orthographic = camera.orthographic;
						camera2.orthographicSize = camera.orthographicSize;
						if (subcameraDef.layer.NullOrEmpty())
						{
							camera2.cullingMask = 0;
						}
						else
						{
							camera2.cullingMask = LayerMask.GetMask(new string[]
							{
								subcameraDef.layer
							});
						}
						camera2.nearClipPlane = camera.nearClipPlane;
						camera2.farClipPlane = camera.farClipPlane;
						camera2.useOcclusionCulling = camera.useOcclusionCulling;
						camera2.allowHDR = camera.allowHDR;
						camera2.renderingPath = camera.renderingPath;
						camera2.clearFlags = CameraClearFlags.Color;
						camera2.backgroundColor = new Color(0f, 0f, 0f, 0f);
						camera2.depth = (float)subcameraDef.depth;
						this.subcameras[(int)subcameraDef.index] = camera2;
					}
				}
			}
		}

		// Token: 0x06003DF7 RID: 15863 RVA: 0x0020ADD4 File Offset: 0x002091D4
		public void UpdatePositions(Camera camera)
		{
			if (this.subcameras != null)
			{
				for (int i = 0; i < this.subcameras.Length; i++)
				{
					this.subcameras[i].orthographicSize = camera.orthographicSize;
					RenderTexture renderTexture = this.subcameras[i].targetTexture;
					if (renderTexture != null && (renderTexture.width != Screen.width || renderTexture.height != Screen.height))
					{
						UnityEngine.Object.Destroy(renderTexture);
						renderTexture = null;
					}
					if (renderTexture == null)
					{
						renderTexture = new RenderTexture(Screen.width, Screen.height, 0, DefDatabase<SubcameraDef>.AllDefsListForReading[i].BestFormat);
					}
					if (!renderTexture.IsCreated())
					{
						renderTexture.Create();
					}
					this.subcameras[i].targetTexture = renderTexture;
				}
			}
		}

		// Token: 0x06003DF8 RID: 15864 RVA: 0x0020AEB4 File Offset: 0x002092B4
		public Camera GetSubcamera(SubcameraDef def)
		{
			Camera result;
			if (this.subcameras == null || def == null || this.subcameras.Length <= (int)def.index)
			{
				result = null;
			}
			else
			{
				result = this.subcameras[(int)def.index];
			}
			return result;
		}

		// Token: 0x04002736 RID: 10038
		private Camera[] subcameras;
	}
}
