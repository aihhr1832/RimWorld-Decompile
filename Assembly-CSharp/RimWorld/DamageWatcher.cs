﻿using System;
using Verse;

namespace RimWorld
{
	// Token: 0x02000380 RID: 896
	public class DamageWatcher : IExposable
	{
		// Token: 0x17000239 RID: 569
		// (get) Token: 0x06000F8A RID: 3978 RVA: 0x00083508 File Offset: 0x00081908
		public float DamageTakenEver
		{
			get
			{
				return this.everDamage;
			}
		}

		// Token: 0x1700023A RID: 570
		// (get) Token: 0x06000F8B RID: 3979 RVA: 0x00083524 File Offset: 0x00081924
		public float DamageTakenRecently
		{
			get
			{
				return this.recentDamage;
			}
		}

		// Token: 0x1700023B RID: 571
		// (get) Token: 0x06000F8C RID: 3980 RVA: 0x00083540 File Offset: 0x00081940
		public float DaysSinceSeriousDamage
		{
			get
			{
				return Find.TickManager.TicksGame.TicksToDays() - this.lastSeriousDamageTick.TicksToDays();
			}
		}

		// Token: 0x06000F8D RID: 3981 RVA: 0x00083570 File Offset: 0x00081970
		public void Notify_DamageTaken(Thing damagee, float amount)
		{
			if (damagee.Faction == Faction.OfPlayer)
			{
				if (damagee.def.category == ThingCategory.Pawn && damagee.def.race.Humanlike)
				{
					amount *= 5f;
				}
				this.recentDamage += amount;
				this.everDamage += amount;
				if (this.recentDamage > 8000f)
				{
					this.recentDamage = 8000f;
				}
				if (this.recentDamage >= 7500f)
				{
					this.lastSeriousDamageTick = Find.TickManager.TicksGame;
				}
			}
		}

		// Token: 0x06000F8E RID: 3982 RVA: 0x0008361C File Offset: 0x00081A1C
		public void DamageWatcherTick()
		{
			if (Find.TickManager.TicksGame % 2000 == 0)
			{
				this.recentDamage -= 70f;
				if (this.recentDamage < 0f)
				{
					this.recentDamage = 0f;
				}
			}
		}

		// Token: 0x06000F8F RID: 3983 RVA: 0x00083670 File Offset: 0x00081A70
		public void ExposeData()
		{
			Scribe_Values.Look<float>(ref this.everDamage, "everDamage", 0f, false);
			Scribe_Values.Look<float>(ref this.recentDamage, "recentDamage", 0f, false);
			Scribe_Values.Look<int>(ref this.lastSeriousDamageTick, "lastSeriousDamageTick", 0, false);
		}

		// Token: 0x0400097B RID: 2427
		private float everDamage = 0f;

		// Token: 0x0400097C RID: 2428
		private float recentDamage = 0f;

		// Token: 0x0400097D RID: 2429
		private int lastSeriousDamageTick = 0;

		// Token: 0x0400097E RID: 2430
		private const int UpdateInterval = 2000;

		// Token: 0x0400097F RID: 2431
		private const float ColonistDamageFactor = 5f;

		// Token: 0x04000980 RID: 2432
		private const float DamageFallPerInterval = 70f;

		// Token: 0x04000981 RID: 2433
		public const float MaxRecentDamage = 8000f;

		// Token: 0x04000982 RID: 2434
		private const float SeriousDamageThreshold = 7500f;
	}
}
