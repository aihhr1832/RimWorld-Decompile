﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using UnityEngine;

namespace Verse.Sound
{
	public class SubSoundDef : Editable
	{
		[DefaultValue("UnnamedSubSoundDef")]
		[Description("A name to help you identify the sound.")]
		public string name = "UnnamedSubSoundDef";

		[DefaultValue(false)]
		[Description("Whether this sound plays on the camera or in the world.\n\nThis must match what the game expects from the sound Def with this name.")]
		public bool onCamera = false;

		[DefaultValue(false)]
		[Description("Whether to mute this subSound while the game is paused (either by the pausing in play or by opening a menu)")]
		public bool muteWhenPaused = false;

		[DefaultValue(false)]
		[Description("Whether this subSound's tempo should be affected by the current tick rate.")]
		public bool tempoAffectedByGameSpeed;

		[Description("The sound grains used for this sample. The game will choose one of these randomly when the sound plays. Sustainers choose one for each sample as it begins.")]
		public List<AudioGrain> grains = new List<AudioGrain>();

		[DefaultFloatRange(50f, 50f)]
		[Description("This sound will play at a random volume inside this range.\n\nSustainers will choose a different random volume for each sample.")]
		[EditSliderRange(0f, 100f)]
		public FloatRange volumeRange = new FloatRange(50f, 50f);

		[DefaultFloatRange(1f, 1f)]
		[Description("This sound will play at a random pitch inside this range.\n\nSustainers will choose a different random pitch for each sample.")]
		[EditSliderRange(0.05f, 2f)]
		public FloatRange pitchRange = FloatRange.One;

		[DefaultFloatRange(25f, 70f)]
		[Description("This sound will play max volume when it is under minDistance from the camera.\n\nIt will fade out linearly until the camera distance reaches its max.")]
		[EditSliderRange(0f, 200f)]
		public FloatRange distRange = new FloatRange(25f, 70f);

		[DefaultValue(RepeatSelectMode.NeverLastHalf)]
		[Description("When the sound chooses the next grain, you may use this setting to have it avoid repeating the last grain, or avoid repeating any of the grains in the last X played, X being half the total number of grains defined.")]
		public RepeatSelectMode repeatMode = RepeatSelectMode.NeverLastHalf;

		[DefaultEmptyList(typeof(SoundParameterMapping))]
		[Description("Mappings between game parameters (like fire size or wind speed) and properties of the sound.")]
		public List<SoundParameterMapping> paramMappings = new List<SoundParameterMapping>();

		[DefaultEmptyList(typeof(SoundFilter))]
		[Description("The filters to be applied to this sound.")]
		public List<SoundFilter> filters = new List<SoundFilter>();

		[DefaultFloatRange(0f, 0f)]
		[Description("A range of possible times between when this sound is triggered and when it will actually start playing.")]
		public FloatRange startDelayRange = FloatRange.Zero;

		[DefaultValue(true)]
		[Description("If true, each sample in the sustainer will be looped and ended only after sustainerLoopDurationRange. If not, the sounds will just play once and end after their own length.")]
		public bool sustainLoop = true;

		[DefaultFloatRange(9999f, 9999f)]
		[Description("The range of durations that individual looped samples in the sustainer will have. Each sample ends after a time randomly chosen in this range.\n\nOnly used if the sustainer is looped.")]
		[EditSliderRange(0f, 10f)]
		public FloatRange sustainLoopDurationRange = new FloatRange(9999f, 9999f);

		[DefaultFloatRange(0f, 0f)]
		[Description("The time between when one sample ends and the next starts.\n\nSet to negative if you wish samples to overlap.")]
		[EditSliderRange(-2f, 2f)]
		[LoadAlias("sustainInterval")]
		public FloatRange sustainIntervalRange = FloatRange.Zero;

		[DefaultValue(0f)]
		[Description("The fade-in time of each sample. The sample will start at 0 volume and fade in over this number of seconds.")]
		[EditSliderRange(0f, 2f)]
		public float sustainAttack = 0f;

		[DefaultValue(true)]
		[Description("Skip the attack on the first sustainer sample.")]
		public bool sustainSkipFirstAttack = true;

		[DefaultValue(0f)]
		[Description("The fade-out time of each sample. At this number of seconds before the sample ends, it will start fading out. Its volume will be zero at the moment it finishes fading out.")]
		[EditSliderRange(0f, 2f)]
		public float sustainRelease = 0f;

		[Unsaved]
		public SoundDef parentDef;

		[Unsaved]
		private List<ResolvedGrain> resolvedGrains = new List<ResolvedGrain>();

		[Unsaved]
		private ResolvedGrain lastPlayedResolvedGrain = null;

		[Unsaved]
		private int numToAvoid = 0;

		[Unsaved]
		private int distinctResolvedGrainsCount = 0;

		[Unsaved]
		private Queue<ResolvedGrain> recentlyPlayedResolvedGrains = new Queue<ResolvedGrain>();

		public SubSoundDef()
		{
		}

		public virtual void TryPlay(SoundInfo info)
		{
			if (this.resolvedGrains.Count == 0)
			{
				Log.Error(string.Concat(new object[]
				{
					"Cannot play ",
					this.parentDef,
					" (subSound ",
					this,
					"_: No resolved grains."
				}), false);
			}
			else if (Find.SoundRoot.oneShotManager.CanAddPlayingOneShot(this.parentDef, info))
			{
				ResolvedGrain resolvedGrain = this.RandomizedResolvedGrain();
				ResolvedGrain_Clip resolvedGrain_Clip = resolvedGrain as ResolvedGrain_Clip;
				if (resolvedGrain_Clip != null)
				{
					if (SampleOneShot.TryMakeAndPlay(this, resolvedGrain_Clip.clip, info) == null)
					{
						return;
					}
					SoundSlotManager.Notify_Played(this.parentDef.slot, resolvedGrain_Clip.clip.length);
				}
				if (this.distinctResolvedGrainsCount > 1)
				{
					if (this.repeatMode == RepeatSelectMode.NeverLastHalf)
					{
						while (this.recentlyPlayedResolvedGrains.Count >= this.numToAvoid)
						{
							this.recentlyPlayedResolvedGrains.Dequeue();
						}
						if (this.recentlyPlayedResolvedGrains.Count < this.numToAvoid)
						{
							this.recentlyPlayedResolvedGrains.Enqueue(resolvedGrain);
						}
					}
					else if (this.repeatMode == RepeatSelectMode.NeverTwice)
					{
						this.lastPlayedResolvedGrain = resolvedGrain;
					}
				}
			}
		}

		public ResolvedGrain RandomizedResolvedGrain()
		{
			ResolvedGrain chosenGrain = null;
			for (;;)
			{
				chosenGrain = this.resolvedGrains.RandomElement<ResolvedGrain>();
				if (this.distinctResolvedGrainsCount <= 1)
				{
					break;
				}
				if (this.repeatMode == RepeatSelectMode.NeverLastHalf)
				{
					if (!(from g in this.recentlyPlayedResolvedGrains
					where g.Equals(chosenGrain)
					select g).Any<ResolvedGrain>())
					{
						break;
					}
				}
				else
				{
					if (this.repeatMode != RepeatSelectMode.NeverTwice)
					{
						break;
					}
					if (!chosenGrain.Equals(this.lastPlayedResolvedGrain))
					{
						break;
					}
				}
			}
			return chosenGrain;
		}

		public float RandomizedVolume()
		{
			float randomInRange = this.volumeRange.RandomInRange;
			return randomInRange / 100f;
		}

		public override void ResolveReferences()
		{
			LongEventHandler.ExecuteWhenFinished(delegate
			{
				this.resolvedGrains.Clear();
				foreach (AudioGrain audioGrain in this.grains)
				{
					foreach (ResolvedGrain item in audioGrain.GetResolvedGrains())
					{
						this.resolvedGrains.Add(item);
					}
				}
				this.distinctResolvedGrainsCount = this.resolvedGrains.Distinct<ResolvedGrain>().Count<ResolvedGrain>();
				this.numToAvoid = Mathf.FloorToInt((float)this.distinctResolvedGrainsCount / 2f);
				if (this.distinctResolvedGrainsCount >= 6)
				{
					this.numToAvoid++;
				}
			});
		}

		public override IEnumerable<string> ConfigErrors()
		{
			if (this.resolvedGrains.Count == 0)
			{
				yield return "No grains resolved.";
			}
			if (this.sustainAttack + this.sustainRelease > this.sustainLoopDurationRange.TrueMin)
			{
				yield return "Attack + release < min loop duration. Sustain samples will cut off.";
			}
			if (this.distRange.min > this.distRange.max)
			{
				yield return "Dist range min/max are reversed.";
			}
			foreach (SoundParameterMapping mapping in this.paramMappings)
			{
				if (mapping.inParam == null || mapping.outParam == null)
				{
					yield return "At least one parameter mapping is missing an in or out parameter.";
					break;
				}
				if (mapping.outParam != null)
				{
					Type neededFilter = mapping.outParam.NeededFilterType;
					if (neededFilter != null)
					{
						if (!(from fil in this.filters
						where fil.GetType() == neededFilter
						select fil).Any<SoundFilter>())
						{
							yield return "A parameter wants to modify the " + neededFilter.ToString() + " filter, but this sound doesn't have it.";
						}
					}
				}
			}
			yield break;
		}

		public override string ToString()
		{
			return this.name;
		}

		[CompilerGenerated]
		private void <ResolveReferences>m__0()
		{
			this.resolvedGrains.Clear();
			foreach (AudioGrain audioGrain in this.grains)
			{
				foreach (ResolvedGrain item in audioGrain.GetResolvedGrains())
				{
					this.resolvedGrains.Add(item);
				}
			}
			this.distinctResolvedGrainsCount = this.resolvedGrains.Distinct<ResolvedGrain>().Count<ResolvedGrain>();
			this.numToAvoid = Mathf.FloorToInt((float)this.distinctResolvedGrainsCount / 2f);
			if (this.distinctResolvedGrainsCount >= 6)
			{
				this.numToAvoid++;
			}
		}

		[CompilerGenerated]
		private sealed class <RandomizedResolvedGrain>c__AnonStorey1
		{
			internal ResolvedGrain chosenGrain;

			public <RandomizedResolvedGrain>c__AnonStorey1()
			{
			}

			internal bool <>m__0(ResolvedGrain g)
			{
				return g.Equals(this.chosenGrain);
			}
		}

		[CompilerGenerated]
		private sealed class <ConfigErrors>c__Iterator0 : IEnumerable, IEnumerable<string>, IEnumerator, IDisposable, IEnumerator<string>
		{
			internal List<SoundParameterMapping>.Enumerator $locvar0;

			internal SoundParameterMapping <mapping>__1;

			internal SubSoundDef $this;

			internal string $current;

			internal bool $disposing;

			internal int $PC;

			private SubSoundDef.<ConfigErrors>c__Iterator0.<ConfigErrors>c__AnonStorey2 $locvar1;

			[DebuggerHidden]
			public <ConfigErrors>c__Iterator0()
			{
			}

			public bool MoveNext()
			{
				uint num = (uint)this.$PC;
				this.$PC = -1;
				bool flag = false;
				switch (num)
				{
				case 0u:
					if (this.resolvedGrains.Count == 0)
					{
						this.$current = "No grains resolved.";
						if (!this.$disposing)
						{
							this.$PC = 1;
						}
						return true;
					}
					break;
				case 1u:
					break;
				case 2u:
					goto IL_B3;
				case 3u:
					goto IL_F7;
				case 4u:
				case 5u:
					goto IL_111;
				default:
					return false;
				}
				if (this.sustainAttack + this.sustainRelease > this.sustainLoopDurationRange.TrueMin)
				{
					this.$current = "Attack + release < min loop duration. Sustain samples will cut off.";
					if (!this.$disposing)
					{
						this.$PC = 2;
					}
					return true;
				}
				IL_B3:
				if (this.distRange.min > this.distRange.max)
				{
					this.$current = "Dist range min/max are reversed.";
					if (!this.$disposing)
					{
						this.$PC = 3;
					}
					return true;
				}
				IL_F7:
				enumerator = this.paramMappings.GetEnumerator();
				num = 4294967293u;
				try
				{
					IL_111:
					switch (num)
					{
					case 4u:
						goto IL_24C;
					case 5u:
						IL_239:
						break;
					}
					IL_23A:
					IL_23B:
					if (enumerator.MoveNext())
					{
						mapping = enumerator.Current;
						if (mapping.inParam == null || mapping.outParam == null)
						{
							this.$current = "At least one parameter mapping is missing an in or out parameter.";
							if (!this.$disposing)
							{
								this.$PC = 4;
							}
							flag = true;
							return true;
						}
						if (mapping.outParam == null)
						{
							goto IL_23B;
						}
						Type neededFilter = mapping.outParam.NeededFilterType;
						if (neededFilter == null)
						{
							goto IL_23A;
						}
						if (!(from fil in this.filters
						where fil.GetType() == neededFilter
						select fil).Any<SoundFilter>())
						{
							this.$current = "A parameter wants to modify the " + neededFilter.ToString() + " filter, but this sound doesn't have it.";
							if (!this.$disposing)
							{
								this.$PC = 5;
							}
							flag = true;
							return true;
						}
						goto IL_239;
					}
					IL_24C:;
				}
				finally
				{
					if (!flag)
					{
						((IDisposable)enumerator).Dispose();
					}
				}
				this.$PC = -1;
				return false;
			}

			string IEnumerator<string>.Current
			{
				[DebuggerHidden]
				get
				{
					return this.$current;
				}
			}

			object IEnumerator.Current
			{
				[DebuggerHidden]
				get
				{
					return this.$current;
				}
			}

			[DebuggerHidden]
			public void Dispose()
			{
				uint num = (uint)this.$PC;
				this.$disposing = true;
				this.$PC = -1;
				switch (num)
				{
				case 4u:
				case 5u:
					try
					{
					}
					finally
					{
						((IDisposable)enumerator).Dispose();
					}
					break;
				}
			}

			[DebuggerHidden]
			public void Reset()
			{
				throw new NotSupportedException();
			}

			[DebuggerHidden]
			IEnumerator IEnumerable.GetEnumerator()
			{
				return this.System.Collections.Generic.IEnumerable<string>.GetEnumerator();
			}

			[DebuggerHidden]
			IEnumerator<string> IEnumerable<string>.GetEnumerator()
			{
				if (Interlocked.CompareExchange(ref this.$PC, 0, -2) == -2)
				{
					return this;
				}
				SubSoundDef.<ConfigErrors>c__Iterator0 <ConfigErrors>c__Iterator = new SubSoundDef.<ConfigErrors>c__Iterator0();
				<ConfigErrors>c__Iterator.$this = this;
				return <ConfigErrors>c__Iterator;
			}

			private sealed class <ConfigErrors>c__AnonStorey2
			{
				internal Type neededFilter;

				internal SubSoundDef.<ConfigErrors>c__Iterator0 <>f__ref$0;

				public <ConfigErrors>c__AnonStorey2()
				{
				}

				internal bool <>m__0(SoundFilter fil)
				{
					return fil.GetType() == this.neededFilter;
				}
			}
		}
	}
}
