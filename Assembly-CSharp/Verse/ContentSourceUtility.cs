using System;
using UnityEngine;

namespace Verse
{
	[StaticConstructorOnStartup]
	public static class ContentSourceUtility
	{
		public const float IconSize = 24f;

		private static readonly Texture2D ContentSourceIcon_LocalFolder = ContentFinder<Texture2D>.Get("UI/Icons/ContentSources/LocalFolder", true);

		private static readonly Texture2D ContentSourceIcon_SteamWorkshop = ContentFinder<Texture2D>.Get("UI/Icons/ContentSources/SteamWorkshop", true);

		public static Texture2D GetIcon(this ContentSource s)
		{
			switch (s)
			{
			case ContentSource.Undefined:
			{
				return BaseContent.BadTex;
			}
			case ContentSource.LocalFolder:
			{
				return ContentSourceUtility.ContentSourceIcon_LocalFolder;
			}
			case ContentSource.SteamWorkshop:
			{
				return ContentSourceUtility.ContentSourceIcon_SteamWorkshop;
			}
			default:
			{
				throw new NotImplementedException();
			}
			}
		}

		public static void DrawContentSource(Rect r, ContentSource source, Action clickAction = null)
		{
			Rect rect = new Rect(r.x, (float)(r.y + r.height / 2.0 - 12.0), 24f, 24f);
			GUI.DrawTexture(rect, source.GetIcon());
			Widgets.DrawHighlightIfMouseover(rect);
			TooltipHandler.TipRegion(rect, (Func<string>)(() => "Source".Translate() + ": " + source.HumanLabel()), (int)(r.x + r.y * 56161.0));
			if ((object)clickAction != null && Widgets.ButtonInvisible(rect, false))
			{
				clickAction();
			}
		}

		public static string HumanLabel(this ContentSource s)
		{
			return ("ContentSource_" + ((Enum)(object)s).ToString()).Translate();
		}
	}
}
