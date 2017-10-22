using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Verse
{
	public static class GenMath
	{
		public struct BezierCubicControls
		{
			public Vector3 w0;

			public Vector3 w1;

			public Vector3 w2;

			public Vector3 w3;
		}

		public const float BigEpsilon = 1E-07f;

		public const float Sqrt2 = 1.41421354f;

		private static List<float> tmpScores = new List<float>();

		private static List<float> tmpCalcList = new List<float>();

		public static float RoundedHundredth(float f)
		{
			return (float)(Mathf.Round((float)(f * 100.0)) / 100.0);
		}

		public static float ChanceEitherHappens(float chanceA, float chanceB)
		{
			return (float)(chanceA + (1.0 - chanceA) * chanceB);
		}

		public static float SmootherStep(float edge0, float edge1, float x)
		{
			x = Mathf.Clamp01((x - edge0) / (edge1 - edge0));
			return (float)(x * x * x * (x * (x * 6.0 - 15.0) + 10.0));
		}

		public static int RoundRandom(float f)
		{
			return (int)f + ((Rand.Value < f % 1.0) ? 1 : 0);
		}

		public static float WeightedAverage(float A, float weightA, float B, float weightB)
		{
			return (A * weightA + B * weightB) / (weightA + weightB);
		}

		public static float Sqrt(float f)
		{
			return (float)Math.Sqrt((double)f);
		}

		public static float LerpDouble(float inFrom, float inTo, float outFrom, float outTo, float x)
		{
			float num = (x - inFrom) / (inTo - inFrom);
			return outFrom + (outTo - outFrom) * num;
		}

		public static float Reflection(float value, float mirror)
		{
			return mirror - (value - mirror);
		}

		public static Quaternion ToQuat(this float ang)
		{
			return Quaternion.AngleAxis(ang, Vector3.up);
		}

		public static float GetFactorInInterval(float min, float mid, float max, float power, float x)
		{
			if (min > max)
			{
				return 0f;
			}
			if (!(x <= min) && !(x >= max))
			{
				if (x == mid)
				{
					return 1f;
				}
				float num = 0f;
				num = (float)((!(x < mid)) ? (1.0 - (x - mid) / (max - mid)) : (1.0 - (mid - x) / (mid - min)));
				return Mathf.Pow(num, power);
			}
			return 0f;
		}

		public static float FlatHill(float min, float lower, float upper, float max, float x)
		{
			if (x < min)
			{
				return 0f;
			}
			if (x < lower)
			{
				return Mathf.InverseLerp(min, lower, x);
			}
			if (x < upper)
			{
				return 1f;
			}
			if (x < max)
			{
				return Mathf.InverseLerp(max, upper, x);
			}
			return 0f;
		}

		public static float FlatHill(float minY, float min, float lower, float upper, float max, float maxY, float x)
		{
			if (x < min)
			{
				return minY;
			}
			if (x < lower)
			{
				return GenMath.LerpDouble(min, lower, minY, 1f, x);
			}
			if (x < upper)
			{
				return 1f;
			}
			if (x < max)
			{
				return GenMath.LerpDouble(upper, max, 1f, maxY, x);
			}
			return maxY;
		}

		public static int OctileDistance(int dx, int dz, int cardinal, int diagonal)
		{
			return cardinal * (dx + dz) + (diagonal - 2 * cardinal) * Mathf.Min(dx, dz);
		}

		public static float UnboundedValueToFactor(float val)
		{
			if (val > 0.0)
			{
				return (float)(1.0 + val);
			}
			return (float)(1.0 / (1.0 - val));
		}

		public static void LogTestMathPerf()
		{
			IntVec3 intVec = new IntVec3(72, 0, 65);
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("Math perf tests (" + 1E+07f + " tests each)");
			float num = 0f;
			Stopwatch stopwatch = Stopwatch.StartNew();
			int num2 = 0;
			while ((float)num2 < 10000000.0)
			{
				num += (float)Math.Sqrt(101.20999908447266);
				num2++;
			}
			stringBuilder.AppendLine("(float)System.Math.Sqrt(" + 101.21f + "): " + stopwatch.ElapsedTicks);
			Stopwatch stopwatch2 = Stopwatch.StartNew();
			int num3 = 0;
			while ((float)num3 < 10000000.0)
			{
				num += Mathf.Sqrt(101.21f);
				num3++;
			}
			stringBuilder.AppendLine("UnityEngine.Mathf.Sqrt(" + 101.21f + "): " + stopwatch2.ElapsedTicks);
			Stopwatch stopwatch3 = Stopwatch.StartNew();
			int num4 = 0;
			while ((float)num4 < 10000000.0)
			{
				num += GenMath.Sqrt(101.21f);
				num4++;
			}
			stringBuilder.AppendLine("Verse.GenMath.Sqrt(" + 101.21f + "): " + stopwatch3.ElapsedTicks);
			Stopwatch stopwatch4 = Stopwatch.StartNew();
			int num5 = 0;
			while ((float)num5 < 10000000.0)
			{
				num += (float)intVec.LengthManhattan;
				num5++;
			}
			stringBuilder.AppendLine("Verse.IntVec3.LengthManhattan: " + stopwatch4.ElapsedTicks);
			Stopwatch stopwatch5 = Stopwatch.StartNew();
			int num6 = 0;
			while ((float)num6 < 10000000.0)
			{
				num += intVec.LengthHorizontal;
				num6++;
			}
			stringBuilder.AppendLine("Verse.IntVec3.LengthHorizontal: " + stopwatch5.ElapsedTicks);
			Stopwatch stopwatch6 = Stopwatch.StartNew();
			int num7 = 0;
			while ((float)num7 < 10000000.0)
			{
				num += (float)intVec.LengthHorizontalSquared;
				num7++;
			}
			stringBuilder.AppendLine("Verse.IntVec3.LengthHorizontalSquared: " + stopwatch6.ElapsedTicks);
			stringBuilder.AppendLine("total: " + num);
			Log.Message(stringBuilder.ToString());
		}

		public static float Min(float a, float b, float c)
		{
			if (a < b)
			{
				if (a < c)
				{
					return a;
				}
				return c;
			}
			if (b < c)
			{
				return b;
			}
			return c;
		}

		public static int Max(int a, int b, int c)
		{
			if (a > b)
			{
				if (a > c)
				{
					return a;
				}
				return c;
			}
			if (b > c)
			{
				return b;
			}
			return c;
		}

		public static float SphericalDistance(Vector3 normalizedA, Vector3 normalizedB)
		{
			if (normalizedA == normalizedB)
			{
				return 0f;
			}
			return Mathf.Acos(Vector3.Dot(normalizedA, normalizedB));
		}

		public static void DHondtDistribution(List<int> candidates, Func<int, float> scoreGetter, int numToDistribute)
		{
			GenMath.tmpScores.Clear();
			GenMath.tmpCalcList.Clear();
			for (int i = 0; i < candidates.Count; i++)
			{
				float item = scoreGetter(i);
				candidates[i] = 0;
				GenMath.tmpScores.Add(item);
				GenMath.tmpCalcList.Add(item);
			}
			for (int num = 0; num < numToDistribute; num++)
			{
				int num2 = GenMath.tmpCalcList.IndexOf(GenMath.tmpCalcList.Max());
				List<int> list;
				List<int> obj = list = candidates;
				int index;
				int index2 = index = num2;
				index = list[index];
				obj[index2] = index + 1;
				GenMath.tmpCalcList[num2] = (float)(GenMath.tmpScores[num2] / ((float)candidates[num2] + 1.0));
			}
		}

		public static int PositiveMod(int x, int m)
		{
			return (x % m + m) % m;
		}

		public static Vector3 BezierCubicEvaluate(float t, BezierCubicControls bcc)
		{
			return GenMath.BezierCubicEvaluate(t, bcc.w0, bcc.w1, bcc.w2, bcc.w3);
		}

		public static Vector3 BezierCubicEvaluate(float t, Vector3 w0, Vector3 w1, Vector3 w2, Vector3 w3)
		{
			float d = t * t;
			float num = (float)(1.0 - t);
			float d2 = num * num;
			return w0 * d2 * num + 3f * w1 * d2 * t + 3f * w2 * num * d + w3 * d * t;
		}

		public static float CirclesOverlapArea(float x1, float y1, float r1, float x2, float y2, float r2)
		{
			float num = (x2 - x1) * (x2 - x1) + (y2 - y1) * (y2 - y1);
			float num2 = Mathf.Sqrt(num);
			float num3 = r1 * r1;
			float num4 = r2 * r2;
			float num5 = Mathf.Abs(r1 - r2);
			if (num2 >= r1 + r2)
			{
				return 0f;
			}
			if (num2 <= num5 && r1 >= r2)
			{
				return (float)(3.1415927410125732 * num4);
			}
			if (num2 <= num5 && r2 >= r1)
			{
				return (float)(3.1415927410125732 * num3);
			}
			float num6 = (float)(Mathf.Acos((float)((num3 - num4 + num) / (2.0 * r1 * num2))) * 2.0);
			float num7 = (float)(Mathf.Acos((float)((num4 - num3 + num) / (2.0 * r2 * num2))) * 2.0);
			float num8 = (float)((num7 * num4 - num4 * Mathf.Sin(num7)) * 0.5);
			float num9 = (float)((num6 * num3 - num3 * Mathf.Sin(num6)) * 0.5);
			return num8 + num9;
		}
	}
}
