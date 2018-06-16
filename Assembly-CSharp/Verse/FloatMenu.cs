﻿using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse.Sound;

namespace Verse
{
	// Token: 0x02000E56 RID: 3670
	public class FloatMenu : Window
	{
		// Token: 0x06005654 RID: 22100 RVA: 0x002C7B30 File Offset: 0x002C5F30
		public FloatMenu(List<FloatMenuOption> options)
		{
			if (options.NullOrEmpty<FloatMenuOption>())
			{
				Log.Error("Created FloatMenu with no options. Closing.", false);
				this.Close(true);
			}
			this.options = (from op in options
			orderby op.Priority descending
			select op).ToList<FloatMenuOption>();
			for (int i = 0; i < options.Count; i++)
			{
				options[i].SetSizeMode(this.SizeMode);
			}
			this.layer = WindowLayer.Super;
			this.closeOnClickedOutside = true;
			this.doWindowBackground = false;
			this.drawShadow = false;
			SoundDefOf.FloatMenu_Open.PlayOneShotOnCamera(null);
		}

		// Token: 0x06005655 RID: 22101 RVA: 0x002C7C11 File Offset: 0x002C6011
		public FloatMenu(List<FloatMenuOption> options, string title, bool needSelection = false) : this(options)
		{
			this.title = title;
			this.needSelection = needSelection;
		}

		// Token: 0x17000D85 RID: 3461
		// (get) Token: 0x06005656 RID: 22102 RVA: 0x002C7C2C File Offset: 0x002C602C
		protected override float Margin
		{
			get
			{
				return 0f;
			}
		}

		// Token: 0x17000D86 RID: 3462
		// (get) Token: 0x06005657 RID: 22103 RVA: 0x002C7C48 File Offset: 0x002C6048
		public override Vector2 InitialSize
		{
			get
			{
				return new Vector2(this.TotalWidth, this.TotalWindowHeight);
			}
		}

		// Token: 0x17000D87 RID: 3463
		// (get) Token: 0x06005658 RID: 22104 RVA: 0x002C7C70 File Offset: 0x002C6070
		private float MaxWindowHeight
		{
			get
			{
				return (float)UI.screenHeight * 0.9f;
			}
		}

		// Token: 0x17000D88 RID: 3464
		// (get) Token: 0x06005659 RID: 22105 RVA: 0x002C7C94 File Offset: 0x002C6094
		private float TotalWindowHeight
		{
			get
			{
				return Mathf.Min(this.TotalViewHeight, this.MaxWindowHeight) + 1f;
			}
		}

		// Token: 0x17000D89 RID: 3465
		// (get) Token: 0x0600565A RID: 22106 RVA: 0x002C7CC0 File Offset: 0x002C60C0
		private float MaxViewHeight
		{
			get
			{
				float result;
				if (this.UsingScrollbar)
				{
					float num = 0f;
					float num2 = 0f;
					for (int i = 0; i < this.options.Count; i++)
					{
						float requiredHeight = this.options[i].RequiredHeight;
						if (requiredHeight > num)
						{
							num = requiredHeight;
						}
						num2 += requiredHeight + -1f;
					}
					int columnCount = this.ColumnCount;
					num2 += (float)columnCount * num;
					result = num2 / (float)columnCount;
				}
				else
				{
					result = this.MaxWindowHeight;
				}
				return result;
			}
		}

		// Token: 0x17000D8A RID: 3466
		// (get) Token: 0x0600565B RID: 22107 RVA: 0x002C7D54 File Offset: 0x002C6154
		private float TotalViewHeight
		{
			get
			{
				float num = 0f;
				float num2 = 0f;
				float maxViewHeight = this.MaxViewHeight;
				for (int i = 0; i < this.options.Count; i++)
				{
					float requiredHeight = this.options[i].RequiredHeight;
					if (num2 + requiredHeight + -1f > maxViewHeight)
					{
						if (num2 > num)
						{
							num = num2;
						}
						num2 = requiredHeight;
					}
					else
					{
						num2 += requiredHeight + -1f;
					}
				}
				return Mathf.Max(num, num2);
			}
		}

		// Token: 0x17000D8B RID: 3467
		// (get) Token: 0x0600565C RID: 22108 RVA: 0x002C7DE4 File Offset: 0x002C61E4
		private float TotalWidth
		{
			get
			{
				float num = (float)this.ColumnCount * this.ColumnWidth;
				if (this.UsingScrollbar)
				{
					num += 16f;
				}
				return num;
			}
		}

		// Token: 0x17000D8C RID: 3468
		// (get) Token: 0x0600565D RID: 22109 RVA: 0x002C7E1C File Offset: 0x002C621C
		private float ColumnWidth
		{
			get
			{
				float num = 70f;
				for (int i = 0; i < this.options.Count; i++)
				{
					float requiredWidth = this.options[i].RequiredWidth;
					if (requiredWidth >= 300f)
					{
						return 300f;
					}
					if (requiredWidth > num)
					{
						num = requiredWidth;
					}
				}
				return Mathf.Round(num);
			}
		}

		// Token: 0x17000D8D RID: 3469
		// (get) Token: 0x0600565E RID: 22110 RVA: 0x002C7E8C File Offset: 0x002C628C
		private int MaxColumns
		{
			get
			{
				return Mathf.FloorToInt(((float)UI.screenWidth - 16f) / this.ColumnWidth);
			}
		}

		// Token: 0x17000D8E RID: 3470
		// (get) Token: 0x0600565F RID: 22111 RVA: 0x002C7EBC File Offset: 0x002C62BC
		private bool UsingScrollbar
		{
			get
			{
				return this.ColumnCountIfNoScrollbar > this.MaxColumns;
			}
		}

		// Token: 0x17000D8F RID: 3471
		// (get) Token: 0x06005660 RID: 22112 RVA: 0x002C7EE0 File Offset: 0x002C62E0
		private int ColumnCount
		{
			get
			{
				return Mathf.Min(this.ColumnCountIfNoScrollbar, this.MaxColumns);
			}
		}

		// Token: 0x17000D90 RID: 3472
		// (get) Token: 0x06005661 RID: 22113 RVA: 0x002C7F08 File Offset: 0x002C6308
		private int ColumnCountIfNoScrollbar
		{
			get
			{
				int result;
				if (this.options == null)
				{
					result = 1;
				}
				else
				{
					Text.Font = GameFont.Small;
					int num = 1;
					float num2 = 0f;
					float maxWindowHeight = this.MaxWindowHeight;
					for (int i = 0; i < this.options.Count; i++)
					{
						float requiredHeight = this.options[i].RequiredHeight;
						if (num2 + requiredHeight + -1f > maxWindowHeight)
						{
							num2 = requiredHeight;
							num++;
						}
						else
						{
							num2 += requiredHeight + -1f;
						}
					}
					result = num;
				}
				return result;
			}
		}

		// Token: 0x17000D91 RID: 3473
		// (get) Token: 0x06005662 RID: 22114 RVA: 0x002C7FA4 File Offset: 0x002C63A4
		public FloatMenuSizeMode SizeMode
		{
			get
			{
				FloatMenuSizeMode result;
				if (this.options.Count > 60)
				{
					result = FloatMenuSizeMode.Tiny;
				}
				else
				{
					result = FloatMenuSizeMode.Normal;
				}
				return result;
			}
		}

		// Token: 0x06005663 RID: 22115 RVA: 0x002C7FD4 File Offset: 0x002C63D4
		protected override void SetInitialSizeAndPosition()
		{
			Vector2 vector = UI.MousePositionOnUIInverted + FloatMenu.InitialPositionShift;
			if (vector.x + this.InitialSize.x > (float)UI.screenWidth)
			{
				vector.x = (float)UI.screenWidth - this.InitialSize.x;
			}
			if (vector.y + this.InitialSize.y > (float)UI.screenHeight)
			{
				vector.y = (float)UI.screenHeight - this.InitialSize.y;
			}
			this.windowRect = new Rect(vector.x, vector.y, this.InitialSize.x, this.InitialSize.y);
		}

		// Token: 0x06005664 RID: 22116 RVA: 0x002C80A4 File Offset: 0x002C64A4
		public override void ExtraOnGUI()
		{
			base.ExtraOnGUI();
			if (!this.title.NullOrEmpty())
			{
				Vector2 vector = new Vector2(this.windowRect.x, this.windowRect.y);
				Text.Font = GameFont.Small;
				float width = Mathf.Max(150f, 15f + Text.CalcSize(this.title).x);
				Rect titleRect = new Rect(vector.x + FloatMenu.TitleOffset.x, vector.y + FloatMenu.TitleOffset.y, width, 23f);
				Find.WindowStack.ImmediateWindow(6830963, titleRect, WindowLayer.Super, delegate
				{
					GUI.color = this.baseColor;
					Text.Font = GameFont.Small;
					Rect position = titleRect.AtZero();
					position.width = 150f;
					GUI.DrawTexture(position, TexUI.TextBGBlack);
					Rect rect = titleRect.AtZero();
					rect.x += 15f;
					Text.Anchor = TextAnchor.MiddleLeft;
					Widgets.Label(rect, this.title);
					Text.Anchor = TextAnchor.UpperLeft;
				}, false, false, 0f);
			}
		}

		// Token: 0x06005665 RID: 22117 RVA: 0x002C8184 File Offset: 0x002C6584
		public override void DoWindowContents(Rect rect)
		{
			if (this.needSelection && Find.Selector.SingleSelectedThing == null)
			{
				Find.WindowStack.TryRemove(this, true);
			}
			else
			{
				this.UpdateBaseColor();
				bool usingScrollbar = this.UsingScrollbar;
				GUI.color = this.baseColor;
				Text.Font = GameFont.Small;
				Vector2 zero = Vector2.zero;
				float maxViewHeight = this.MaxViewHeight;
				float columnWidth = this.ColumnWidth;
				if (usingScrollbar)
				{
					rect.width -= 10f;
					Widgets.BeginScrollView(rect, ref this.scrollPosition, new Rect(0f, 0f, this.TotalWidth - 16f, this.TotalViewHeight), true);
				}
				foreach (FloatMenuOption floatMenuOption in this.options)
				{
					float requiredHeight = floatMenuOption.RequiredHeight;
					if (zero.y + requiredHeight + -1f > maxViewHeight)
					{
						zero.y = 0f;
						zero.x += columnWidth + -1f;
					}
					Rect rect2 = new Rect(zero.x, zero.y, columnWidth, requiredHeight);
					zero.y += requiredHeight + -1f;
					bool flag = floatMenuOption.DoGUI(rect2, this.givesColonistOrders);
					if (flag)
					{
						Find.WindowStack.TryRemove(this, true);
						break;
					}
				}
				if (usingScrollbar)
				{
					Widgets.EndScrollView();
				}
				if (Event.current.type == EventType.MouseDown)
				{
					Event.current.Use();
					this.Close(true);
				}
				GUI.color = Color.white;
			}
		}

		// Token: 0x06005666 RID: 22118 RVA: 0x002C835C File Offset: 0x002C675C
		public override void PostClose()
		{
			base.PostClose();
			if (this.onCloseCallback != null)
			{
				this.onCloseCallback();
			}
		}

		// Token: 0x06005667 RID: 22119 RVA: 0x002C837B File Offset: 0x002C677B
		public void Cancel()
		{
			SoundDefOf.FloatMenu_Cancel.PlayOneShotOnCamera(null);
			Find.WindowStack.TryRemove(this, true);
		}

		// Token: 0x06005668 RID: 22120 RVA: 0x002C8398 File Offset: 0x002C6798
		private void UpdateBaseColor()
		{
			this.baseColor = Color.white;
			if (this.vanishIfMouseDistant)
			{
				Rect r = new Rect(0f, 0f, this.TotalWidth, this.TotalWindowHeight).ContractedBy(-5f);
				if (!r.Contains(Event.current.mousePosition))
				{
					float num = GenUI.DistFromRect(r, Event.current.mousePosition);
					this.baseColor = new Color(1f, 1f, 1f, 1f - num / 95f);
					if (num > 95f)
					{
						this.Close(false);
						this.Cancel();
					}
				}
			}
		}

		// Token: 0x0400391F RID: 14623
		public bool givesColonistOrders = false;

		// Token: 0x04003920 RID: 14624
		public bool vanishIfMouseDistant = true;

		// Token: 0x04003921 RID: 14625
		public Action onCloseCallback = null;

		// Token: 0x04003922 RID: 14626
		protected List<FloatMenuOption> options;

		// Token: 0x04003923 RID: 14627
		private string title = null;

		// Token: 0x04003924 RID: 14628
		private bool needSelection = false;

		// Token: 0x04003925 RID: 14629
		private Color baseColor = Color.white;

		// Token: 0x04003926 RID: 14630
		private Vector2 scrollPosition;

		// Token: 0x04003927 RID: 14631
		private static readonly Vector2 TitleOffset = new Vector2(30f, -25f);

		// Token: 0x04003928 RID: 14632
		private const float OptionSpacing = -1f;

		// Token: 0x04003929 RID: 14633
		private const float MaxScreenHeightPercent = 0.9f;

		// Token: 0x0400392A RID: 14634
		private const float MinimumColumnWidth = 70f;

		// Token: 0x0400392B RID: 14635
		private static readonly Vector2 InitialPositionShift = new Vector2(4f, 0f);

		// Token: 0x0400392C RID: 14636
		private const float FadeStartMouseDist = 5f;

		// Token: 0x0400392D RID: 14637
		private const float FadeFinishMouseDist = 100f;
	}
}
