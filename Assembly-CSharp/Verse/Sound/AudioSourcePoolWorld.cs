﻿using System;
using System.Collections.Generic;
using UnityEngine;

namespace Verse.Sound
{
	// Token: 0x02000DBF RID: 3519
	public class AudioSourcePoolWorld
	{
		// Token: 0x06004E7A RID: 20090 RVA: 0x0028F8C4 File Offset: 0x0028DCC4
		public AudioSourcePoolWorld()
		{
			GameObject gameObject = new GameObject("OneShotSourcesWorldContainer");
			gameObject.transform.position = Vector3.zero;
			for (int i = 0; i < 32; i++)
			{
				GameObject gameObject2 = new GameObject("OneShotSource_" + i.ToString());
				gameObject2.transform.parent = gameObject.transform;
				gameObject2.transform.localPosition = Vector3.zero;
				this.sourcesWorld.Add(AudioSourceMaker.NewAudioSourceOn(gameObject2));
			}
		}

		// Token: 0x06004E7B RID: 20091 RVA: 0x0028F964 File Offset: 0x0028DD64
		public AudioSource GetSourceWorld()
		{
			foreach (AudioSource audioSource in this.sourcesWorld)
			{
				if (!audioSource.isPlaying)
				{
					SoundFilterUtility.DisableAllFiltersOn(audioSource);
					return audioSource;
				}
			}
			return null;
		}

		// Token: 0x0400343F RID: 13375
		private List<AudioSource> sourcesWorld = new List<AudioSource>();

		// Token: 0x04003440 RID: 13376
		private const int NumSourcesWorld = 32;
	}
}