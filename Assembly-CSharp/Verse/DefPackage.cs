using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;

namespace Verse
{
	public class DefPackage
	{
		public string fileName = "NamelessPackage";

		public string relFolder = string.Empty;

		public List<Def> defs = new List<Def>();

		public DefPackage(string name, string relFolder)
		{
			this.fileName = name;
			this.relFolder = relFolder;
		}

		public List<Def>.Enumerator GetEnumerator()
		{
			return this.defs.GetEnumerator();
		}

		public void AddDef(Def def)
		{
			this.defs.Add(def);
		}

		public void RemoveDef(Def def)
		{
			if (def == null)
			{
				throw new ArgumentNullException("def");
			}
			if (!this.defs.Contains(def))
			{
				throw new InvalidOperationException("Package " + this + " cannot remove " + def + " because it doesn't contain it.");
			}
			this.defs.Remove(def);
		}

		public void SaveIn(ModContentPack mod)
		{
			string fullFolderPath = this.GetFullFolderPath(mod);
			string str = Path.Combine(fullFolderPath, this.fileName);
			XDocument xDocument = new XDocument();
			XElement xElement = new XElement("DefPackage");
			xDocument.Add(xElement);
			try
			{
				List<Def>.Enumerator enumerator = this.defs.GetEnumerator();
				try
				{
					while (enumerator.MoveNext())
					{
						Def current = enumerator.Current;
						XElement content = DirectXmlSaver.XElementFromObject(current, current.GetType());
						xElement.Add(content);
					}
				}
				finally
				{
					((IDisposable)(object)enumerator).Dispose();
				}
				DirectXmlSaveFormatter.AddWhitespaceFromRoot(xElement);
				SaveOptions options = SaveOptions.DisableFormatting;
				xDocument.Save(str, options);
				Messages.Message("Saved in " + str, MessageSound.Benefit);
			}
			catch (Exception ex)
			{
				Messages.Message("Exception saving XML: " + ex.ToString(), MessageSound.Negative);
				throw;
				IL_00c6:;
			}
		}

		public override string ToString()
		{
			return this.relFolder + "/" + this.fileName;
		}

		public string GetFullFolderPath(ModContentPack mod)
		{
			return Path.GetFullPath(Path.Combine(Path.Combine(mod.RootDir, "Defs/"), this.relFolder));
		}

		public static string UnusedPackageName(string relFolder, ModContentPack mod)
		{
			string fullPath = Path.GetFullPath(Path.Combine(Path.Combine(mod.RootDir, "Defs/"), relFolder));
			int num = 1;
			string text;
			while (true)
			{
				text = "NewPackage" + num.ToString() + ".xml";
				num++;
				if (!File.Exists(Path.Combine(fullPath, text)))
					break;
			}
			return text;
		}
	}
}
