﻿using System;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public static class LatitudeSectionUtility
	{
		private const float LerpDistance = 5f;

		public static LatitudeSection GetReportedLatitudeSection(float latitude)
		{
			float num;
			float num2;
			float num3;
			LatitudeSectionUtility.GetLatitudeSection(latitude, out num, out num2, out num3);
			LatitudeSection result;
			if (num == 0f && num2 == 0f && num3 == 0f)
			{
				result = LatitudeSection.Undefined;
			}
			else if (num == 1f)
			{
				result = LatitudeSection.Equatorial;
			}
			else if (num3 == 1f)
			{
				result = LatitudeSection.Polar;
			}
			else
			{
				result = LatitudeSection.Seasonal;
			}
			return result;
		}

		public static LatitudeSection GetDominantLatitudeSection(float latitude)
		{
			float num;
			float num2;
			float num3;
			LatitudeSectionUtility.GetLatitudeSection(latitude, out num, out num2, out num3);
			LatitudeSection result;
			if (num == 0f && num2 == 0f && num3 == 0f)
			{
				result = LatitudeSection.Undefined;
			}
			else
			{
				result = GenMath.MaxBy<LatitudeSection>(LatitudeSection.Equatorial, num, LatitudeSection.Seasonal, num2, LatitudeSection.Polar, num3);
			}
			return result;
		}

		public static void GetLatitudeSection(float latitude, out float equatorial, out float seasonal, out float polar)
		{
			float num = Mathf.Abs(latitude);
			float maxLatitude = LatitudeSection.Equatorial.GetMaxLatitude();
			float maxLatitude2 = LatitudeSection.Seasonal.GetMaxLatitude();
			float maxLatitude3 = LatitudeSection.Polar.GetMaxLatitude();
			if (num <= maxLatitude)
			{
				equatorial = 1f;
				seasonal = 0f;
				polar = 0f;
			}
			else if (num <= maxLatitude2)
			{
				equatorial = Mathf.InverseLerp(maxLatitude + 5f, maxLatitude, num);
				float a = 1f - equatorial;
				polar = Mathf.InverseLerp(maxLatitude2 - 5f, maxLatitude2, num);
				float b = 1f - polar;
				seasonal = Mathf.Min(a, b);
				GenMath.NormalizeToSum1(ref equatorial, ref seasonal, ref polar);
			}
			else if (num <= maxLatitude3)
			{
				equatorial = 0f;
				seasonal = 0f;
				polar = 1f;
			}
			else
			{
				equatorial = 0f;
				seasonal = 0f;
				polar = 0f;
			}
		}

		public static float GetMaxLatitude(this LatitudeSection latitudeSection)
		{
			switch (Find.World.info.overallTemperature)
			{
			case OverallTemperature.VeryCold:
				if (latitudeSection == LatitudeSection.Equatorial)
				{
					return -999f;
				}
				if (latitudeSection == LatitudeSection.Seasonal)
				{
					return -999f;
				}
				if (latitudeSection == LatitudeSection.Polar)
				{
					return 999f;
				}
				break;
			case OverallTemperature.Cold:
				if (latitudeSection == LatitudeSection.Equatorial)
				{
					return -999f;
				}
				if (latitudeSection == LatitudeSection.Seasonal)
				{
					return 15f;
				}
				if (latitudeSection == LatitudeSection.Polar)
				{
					return 999f;
				}
				break;
			case OverallTemperature.LittleBitColder:
				if (latitudeSection == LatitudeSection.Equatorial)
				{
					return -999f;
				}
				if (latitudeSection == LatitudeSection.Seasonal)
				{
					return 40f;
				}
				if (latitudeSection == LatitudeSection.Polar)
				{
					return 999f;
				}
				break;
			case OverallTemperature.Normal:
				if (latitudeSection == LatitudeSection.Equatorial)
				{
					return 15f;
				}
				if (latitudeSection == LatitudeSection.Seasonal)
				{
					return 75f;
				}
				if (latitudeSection == LatitudeSection.Polar)
				{
					return 999f;
				}
				break;
			case OverallTemperature.LittleBitWarmer:
				if (latitudeSection == LatitudeSection.Equatorial)
				{
					return 35f;
				}
				if (latitudeSection == LatitudeSection.Seasonal)
				{
					return 999f;
				}
				if (latitudeSection == LatitudeSection.Polar)
				{
					return 999f;
				}
				break;
			case OverallTemperature.Hot:
				if (latitudeSection == LatitudeSection.Equatorial)
				{
					return 65f;
				}
				if (latitudeSection == LatitudeSection.Seasonal)
				{
					return 999f;
				}
				if (latitudeSection == LatitudeSection.Polar)
				{
					return 999f;
				}
				break;
			case OverallTemperature.VeryHot:
				if (latitudeSection == LatitudeSection.Equatorial)
				{
					return 999f;
				}
				if (latitudeSection == LatitudeSection.Seasonal)
				{
					return 999f;
				}
				if (latitudeSection == LatitudeSection.Polar)
				{
					return 999f;
				}
				break;
			}
			return -1f;
		}
	}
}
