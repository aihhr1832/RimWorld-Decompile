using System.Xml;

namespace Verse
{
	public class PatchOperationReplace : PatchOperationPathed
	{
		private XmlContainer value;

		protected override bool ApplyWorker(XmlDocument xml)
		{
			XmlNode node = this.value.node;
			bool result = false;
			foreach (object item in xml.SelectNodes(base.xpath))
			{
				result = true;
				XmlNode xmlNode = item as XmlNode;
				XmlNode parentNode = xmlNode.ParentNode;
				for (int i = 0; i < node.ChildNodes.Count; i++)
				{
					parentNode.InsertBefore(parentNode.OwnerDocument.ImportNode(node.ChildNodes[i], true), xmlNode);
				}
				parentNode.RemoveChild(xmlNode);
			}
			return result;
		}
	}
}
