using RimWorld;
using System.IO;
using UnityEngine;

namespace Verse
{
	public struct SaveFileInfo
	{
		private FileInfo fileInfo;

		private string gameVersion;

		public static readonly Color UnimportantTextColor = new Color(1f, 1f, 1f, 0.5f);

		public bool Valid
		{
			get
			{
				return this.gameVersion != null;
			}
		}

		public FileInfo FileInfo
		{
			get
			{
				return this.fileInfo;
			}
		}

		public string GameVersion
		{
			get
			{
				if (!this.Valid)
				{
					return "???";
				}
				return this.gameVersion;
			}
		}

		public Color VersionColor
		{
			get
			{
				if (!this.Valid)
				{
					return Color.red;
				}
				if (VersionControl.MajorFromVersionString(this.gameVersion) == VersionControl.CurrentMajor && VersionControl.MinorFromVersionString(this.gameVersion) == VersionControl.CurrentMinor)
				{
					if (VersionControl.BuildFromVersionString(this.gameVersion) != VersionControl.CurrentBuild)
					{
						return Color.yellow;
					}
					return SaveFileInfo.UnimportantTextColor;
				}
				if (BackCompatibility.IsSaveCompatibleWith(this.gameVersion))
				{
					return Color.yellow;
				}
				return Color.red;
			}
		}

		public TipSignal CompatibilityTip
		{
			get
			{
				if (!this.Valid)
				{
					return "SaveIsUnknownFormat".Translate();
				}
				if (VersionControl.MajorFromVersionString(this.gameVersion) == VersionControl.CurrentMajor && VersionControl.MinorFromVersionString(this.gameVersion) == VersionControl.CurrentMinor)
				{
					if (VersionControl.BuildFromVersionString(this.gameVersion) != VersionControl.CurrentBuild)
					{
						return "SaveIsFromDifferentGameBuild".Translate(VersionControl.CurrentVersionString, this.gameVersion);
					}
					return "SaveIsFromThisGameBuild".Translate();
				}
				return "SaveIsFromDifferentGameVersion".Translate(VersionControl.CurrentVersionString, this.gameVersion);
			}
		}

		public SaveFileInfo(FileInfo fileInfo)
		{
			this.fileInfo = fileInfo;
			this.gameVersion = ScribeMetaHeaderUtility.GameVersionOf(fileInfo);
		}
	}
}
