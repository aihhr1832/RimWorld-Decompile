﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Verse
{
	[HasDebugOutput]
	public struct ShootLine
	{
		private IntVec3 source;

		private IntVec3 dest;

		[CompilerGenerated]
		private static Func<int, string> <>f__am$cache0;

		[CompilerGenerated]
		private static Func<int, string> <>f__am$cache1;

		public ShootLine(IntVec3 source, IntVec3 dest)
		{
			this.source = source;
			this.dest = dest;
		}

		public IntVec3 Source
		{
			get
			{
				return this.source;
			}
		}

		public IntVec3 Dest
		{
			get
			{
				return this.dest;
			}
		}

		public void ChangeDestToMissWild(float aimOnChance)
		{
			float num = ShootTuning.MissDistanceFromAimOnChanceCurves.Evaluate(aimOnChance, Rand.Value);
			if (num < 0f)
			{
				Log.ErrorOnce("Attempted to wild-miss less than zero tiles away", 94302089, false);
			}
			IntVec3 a;
			do
			{
				Vector2 unitVector = Rand.UnitVector2;
				Vector3 b = new Vector3(unitVector.x * num, 0f, unitVector.y * num);
				a = (this.dest.ToVector3Shifted() + b).ToIntVec3();
			}
			while (Vector3.Dot((this.dest - this.source).ToVector3(), (a - this.source).ToVector3()) < 0f);
			this.dest = a;
		}

		public IEnumerable<IntVec3> Points()
		{
			return GenSight.PointsOnLineOfSight(this.source, this.dest);
		}

		public override string ToString()
		{
			return string.Concat(new object[]
			{
				"(",
				this.source,
				"->",
				this.dest,
				")"
			});
		}

		[DebugOutput]
		public static void WildMissResults()
		{
			IntVec3 intVec = new IntVec3(100, 0, 0);
			ShootLine shootLine = new ShootLine(IntVec3.Zero, intVec);
			IEnumerable<int> enumerable = Enumerable.Range(0, 101);
			IEnumerable<int> colValues = Enumerable.Range(0, 12);
			int[,] results = new int[enumerable.Count<int>(), colValues.Count<int>()];
			foreach (int num in enumerable)
			{
				for (int i = 0; i < 10000; i++)
				{
					ShootLine shootLine2 = shootLine;
					shootLine2.ChangeDestToMissWild((float)num / 100f);
					if (shootLine2.dest.z == 0 && shootLine2.dest.x > intVec.x)
					{
						results[num, shootLine2.dest.x - intVec.x]++;
					}
				}
			}
			DebugTables.MakeTablesDialog<int, int>(colValues, (int cells) => cells.ToString() + "-away\ncell\nhit%", enumerable, (int hitchance) => ((float)hitchance / 100f).ToStringPercent() + " aimon chance", delegate(int cells, int hitchance)
			{
				float num2 = (float)hitchance / 100f;
				string result;
				if (cells == 0)
				{
					result = num2.ToStringPercent();
				}
				else
				{
					result = ((float)results[hitchance, cells] / 10000f * (1f - num2)).ToStringPercent();
				}
				return result;
			}, "");
		}

		[CompilerGenerated]
		private static string <WildMissResults>m__0(int cells)
		{
			return cells.ToString() + "-away\ncell\nhit%";
		}

		[CompilerGenerated]
		private static string <WildMissResults>m__1(int hitchance)
		{
			return ((float)hitchance / 100f).ToStringPercent() + " aimon chance";
		}

		[CompilerGenerated]
		private sealed class <WildMissResults>c__AnonStorey0
		{
			internal int[,] results;

			public <WildMissResults>c__AnonStorey0()
			{
			}

			internal string <>m__0(int cells, int hitchance)
			{
				float num = (float)hitchance / 100f;
				string result;
				if (cells == 0)
				{
					result = num.ToStringPercent();
				}
				else
				{
					result = ((float)this.results[hitchance, cells] / 10000f * (1f - num)).ToStringPercent();
				}
				return result;
			}
		}
	}
}
