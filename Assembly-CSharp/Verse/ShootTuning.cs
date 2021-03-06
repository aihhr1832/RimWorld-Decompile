﻿using System;

namespace Verse
{
	public static class ShootTuning
	{
		public const float MinAccuracyFactorFromShooterAndDistance = 0.0201f;

		public const float LayingDownHitChanceFactorMinDistance = 4.5f;

		public const float HitChanceFactorIfLayingDown = 0.2f;

		public const float NonPawnShooterHitFactorPerDistance = 0.96f;

		public const float ExecutionMaxDistance = 3.9f;

		public const float ExecutionAccuracyFactor = 7.5f;

		public const float TargetSizeFactorFromFillPercentFactor = 1.7f;

		public const float TargetSizeFactorMin = 0.5f;

		public const float TargetSizeFactorMax = 2f;

		public const float MinAimOnChance_StandardTarget = 0.0201f;

		public static readonly SimpleSurface MissDistanceFromAimOnChanceCurves = new SimpleSurface
		{
			new SurfaceColumn(0.02f, new SimpleCurve
			{
				{
					new CurvePoint(0f, 1f),
					true
				},
				{
					new CurvePoint(1f, 10f),
					true
				}
			}),
			new SurfaceColumn(0.04f, new SimpleCurve
			{
				{
					new CurvePoint(0f, 1f),
					true
				},
				{
					new CurvePoint(1f, 8f),
					true
				}
			}),
			new SurfaceColumn(0.07f, new SimpleCurve
			{
				{
					new CurvePoint(0f, 1f),
					true
				},
				{
					new CurvePoint(1f, 6f),
					true
				}
			}),
			new SurfaceColumn(0.11f, new SimpleCurve
			{
				{
					new CurvePoint(0f, 1f),
					true
				},
				{
					new CurvePoint(1f, 4f),
					true
				}
			}),
			new SurfaceColumn(0.22f, new SimpleCurve
			{
				{
					new CurvePoint(0f, 1f),
					true
				},
				{
					new CurvePoint(1f, 2f),
					true
				}
			}),
			new SurfaceColumn(1f, new SimpleCurve
			{
				{
					new CurvePoint(0f, 1f),
					true
				},
				{
					new CurvePoint(1f, 1f),
					true
				}
			})
		};

		public const float CanInterceptPawnsChanceOnWildOrForcedMissRadius = 0.5f;

		public const float Intercept_Pawn_HitChancePerBodySize = 0.4f;

		public const float Intercept_Pawn_HitChanceFactor_LayingDown = 0.1f;

		public const float Intercept_Pawn_HitChanceFactor_NonWildNonEnemy = 0.4f;

		public const float Intercept_Object_HitChancePerFillPercent = 0.15f;

		public const float Intercept_Object_AdjToTarget_HitChancePerFillPercent = 1f;

		public const float Intercept_OpenDoor_HitChance = 0.05f;

		public const float ImpactCell_Pawn_HitChancePerBodySize = 0.5f;

		public const float ImpactCell_Object_HitChancePerFillPercent = 1.5f;

		public const float BodySizeClampMin = 0.1f;

		public const float BodySizeClampMax = 2f;

		// Note: this type is marked as 'beforefieldinit'.
		static ShootTuning()
		{
		}
	}
}
