using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml;

namespace Verse
{
	public static class DirectXmlToObject
	{
		public const string DictionaryKeyName = "key";

		public const string DictionaryValueName = "value";

		public const string LoadDataFromXmlCustomMethodName = "LoadDataFromXmlCustom";

		public const string PostLoadMethodName = "PostLoad";

		public const string ObjectFromXmlMethodName = "ObjectFromXml";

		public const string ListFromXmlMethodName = "ListFromXml";

		public const string DictionaryFromXmlMethodName = "DictionaryFromXml";

		public static T ObjectFromXml<T>(XmlNode xmlRoot, bool doPostLoad) where T : new()
		{
			MethodInfo methodInfo = DirectXmlToObject.CustomDataLoadMethodOf(typeof(T));
			if (methodInfo != null)
			{
				xmlRoot = XmlInheritance.GetResolvedNodeFor(xmlRoot);
				Type type = DirectXmlToObject.ClassTypeOf<T>(xmlRoot);
				T val = (T)Activator.CreateInstance(type);
				try
				{
					methodInfo.Invoke(val, new object[1]
					{
						xmlRoot
					});
				}
				catch (Exception ex)
				{
					Log.Error("Exception in custom XML loader for " + typeof(T) + ". Node is:\n " + xmlRoot.OuterXml + "\n\nException is:\n " + ex.ToString());
					val = default(T);
				}
				if (doPostLoad)
				{
					DirectXmlToObject.TryDoPostLoad(val);
				}
				return val;
			}
			if (xmlRoot.ChildNodes.Count == 1 && xmlRoot.FirstChild.NodeType == XmlNodeType.CDATA)
			{
				if (typeof(T) != typeof(string))
				{
					Log.Error("CDATA can only be used for strings. Bad xml: " + xmlRoot.OuterXml);
					return default(T);
				}
				return (T)(object)xmlRoot.FirstChild.Value;
			}
			if (xmlRoot.ChildNodes.Count == 1 && xmlRoot.FirstChild.NodeType == XmlNodeType.Text)
			{
				try
				{
					return (T)ParseHelper.FromString(xmlRoot.InnerText, typeof(T));
					IL_0167:;
				}
				catch (Exception ex2)
				{
					Log.Error("Exception parsing " + xmlRoot.OuterXml + " to type " + typeof(T) + ": " + ex2);
				}
				return default(T);
			}
			if (Attribute.IsDefined(typeof(T), typeof(FlagsAttribute)))
			{
				List<T> list = DirectXmlToObject.ListFromXml<T>(xmlRoot);
				int num = 0;
				List<T>.Enumerator enumerator = list.GetEnumerator();
				try
				{
					while (enumerator.MoveNext())
					{
						T current = enumerator.Current;
						int num2 = (int)(object)current;
						num |= num2;
					}
				}
				finally
				{
					((IDisposable)(object)enumerator).Dispose();
				}
				return (T)(object)num;
			}
			if (typeof(T).HasGenericDefinition(typeof(List<>)))
			{
				MethodInfo method = typeof(DirectXmlToObject).GetMethod("ListFromXml", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
				Type[] genericArguments = typeof(T).GetGenericArguments();
				MethodInfo methodInfo2 = method.MakeGenericMethod(genericArguments);
				object[] parameters = new object[1]
				{
					xmlRoot
				};
				object obj = methodInfo2.Invoke(null, parameters);
				return (T)obj;
			}
			if (typeof(T).HasGenericDefinition(typeof(Dictionary<, >)))
			{
				MethodInfo method2 = typeof(DirectXmlToObject).GetMethod("DictionaryFromXml", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
				Type[] genericArguments2 = typeof(T).GetGenericArguments();
				MethodInfo methodInfo3 = method2.MakeGenericMethod(genericArguments2);
				object[] parameters2 = new object[1]
				{
					xmlRoot
				};
				object obj2 = methodInfo3.Invoke(null, parameters2);
				return (T)obj2;
			}
			if (!xmlRoot.HasChildNodes)
			{
				if (typeof(T) == typeof(string))
				{
					return (T)(object)string.Empty;
				}
				XmlAttribute xmlAttribute = xmlRoot.Attributes["IsNull"];
				if (xmlAttribute != null && xmlAttribute.Value.ToUpperInvariant() == "TRUE")
				{
					return default(T);
				}
				if (typeof(T).IsGenericType)
				{
					Type genericTypeDefinition = typeof(T).GetGenericTypeDefinition();
					if (genericTypeDefinition != typeof(List<>) && genericTypeDefinition != typeof(HashSet<>) && genericTypeDefinition != typeof(Dictionary<, >))
					{
						goto IL_0411;
					}
					return new T();
				}
			}
			goto IL_0411;
			IL_0411:
			xmlRoot = XmlInheritance.GetResolvedNodeFor(xmlRoot);
			Type type2 = DirectXmlToObject.ClassTypeOf<T>(xmlRoot);
			T val2 = (T)Activator.CreateInstance(type2);
			List<string> list2 = null;
			if (xmlRoot.ChildNodes.Count > 1)
			{
				list2 = new List<string>();
			}
			for (int i = 0; i < xmlRoot.ChildNodes.Count; i++)
			{
				XmlNode xmlNode = xmlRoot.ChildNodes[i];
				if (!(xmlNode is XmlComment))
				{
					if (xmlRoot.ChildNodes.Count > 1)
					{
						if (list2.Contains(xmlNode.Name))
						{
							Log.Error("XML " + typeof(T) + " defines the same field twice: " + xmlNode.Name + ".\n\nField contents: " + xmlNode.InnerText + ".\n\nWhole XML:\n\n" + xmlRoot.OuterXml);
						}
						else
						{
							list2.Add(xmlNode.Name);
						}
					}
					FieldInfo fieldInfo = null;
					Type type3 = val2.GetType();
					while (true)
					{
						fieldInfo = type3.GetField(xmlNode.Name, BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
						if (fieldInfo != null)
							break;
						if (type3.BaseType == typeof(object))
							break;
						type3 = type3.BaseType;
					}
					if (fieldInfo == null)
					{
						FieldInfo[] fields = val2.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
						int num3 = 0;
						while (num3 < fields.Length)
						{
							FieldInfo fieldInfo2 = fields[num3];
							object[] customAttributes = fieldInfo2.GetCustomAttributes(typeof(LoadAliasAttribute), true);
							for (int j = 0; j < customAttributes.Length; j++)
							{
								object obj3 = customAttributes[j];
								string alias = ((LoadAliasAttribute)obj3).alias;
								if (alias.EqualsIgnoreCase(xmlNode.Name))
								{
									fieldInfo = fieldInfo2;
									break;
								}
							}
							if (fieldInfo == null)
							{
								num3++;
								continue;
							}
							break;
						}
					}
					if (fieldInfo == null)
					{
						bool flag = false;
						object[] customAttributes2 = val2.GetType().GetCustomAttributes(typeof(IgnoreSavedElementAttribute), true);
						for (int k = 0; k < customAttributes2.Length; k++)
						{
							object obj4 = customAttributes2[k];
							string elementToIgnore = ((IgnoreSavedElementAttribute)obj4).elementToIgnore;
							if (string.Equals(elementToIgnore, xmlNode.Name, StringComparison.OrdinalIgnoreCase))
							{
								flag = true;
								break;
							}
						}
						if (!flag)
						{
							Log.Error("XML error: " + xmlNode.OuterXml + " doesn't correspond to any field in type " + val2.GetType().Name + ".");
						}
					}
					else if (typeof(Def).IsAssignableFrom(fieldInfo.FieldType))
					{
						if (xmlNode.InnerText.NullOrEmpty())
						{
							fieldInfo.SetValue(val2, null);
						}
						else
						{
							DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(val2, fieldInfo, xmlNode.InnerText);
						}
					}
					else
					{
						object obj5 = null;
						try
						{
							MethodInfo method3 = typeof(DirectXmlToObject).GetMethod("ObjectFromXml", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
							MethodInfo methodInfo4 = method3.MakeGenericMethod(fieldInfo.FieldType);
							obj5 = methodInfo4.Invoke(null, new object[2]
							{
								xmlNode,
								doPostLoad
							});
						}
						catch (Exception ex3)
						{
							Log.Error("Exception loading from " + xmlNode.ToString() + ": " + ex3.ToString());
							continue;
							IL_07a8:;
						}
						if (!typeof(T).IsValueType)
						{
							fieldInfo.SetValue(val2, obj5);
						}
						else
						{
							object obj6 = val2;
							fieldInfo.SetValue(obj6, obj5);
							val2 = (T)obj6;
						}
					}
				}
			}
			if (doPostLoad)
			{
				DirectXmlToObject.TryDoPostLoad(val2);
			}
			return val2;
		}

		private static Type ClassTypeOf<T>(XmlNode xmlRoot)
		{
			XmlAttribute xmlAttribute = xmlRoot.Attributes["Class"];
			if (xmlAttribute != null)
			{
				Type typeInAnyAssembly = GenTypes.GetTypeInAnyAssembly(xmlAttribute.Value);
				if (typeInAnyAssembly == null)
				{
					Log.Error("Could not find type named " + xmlAttribute.Value + " from node " + xmlRoot.OuterXml);
					return typeof(T);
				}
				return typeInAnyAssembly;
			}
			return typeof(T);
		}

		private static void TryDoPostLoad(object obj)
		{
			try
			{
				MethodInfo method = obj.GetType().GetMethod("PostLoad");
				if (method != null)
				{
					method.Invoke(obj, null);
				}
			}
			catch (Exception ex)
			{
				Log.Error("Exception while executing PostLoad on " + obj.ToStringSafe() + ": " + ex);
			}
		}

		private static List<T> ListFromXml<T>(XmlNode listRootNode) where T : new()
		{
			List<T> list = new List<T>();
			try
			{
				bool flag = typeof(Def).IsAssignableFrom(typeof(T));
				foreach (XmlNode childNode in listRootNode.ChildNodes)
				{
					if (DirectXmlToObject.ValidateListNode(childNode, listRootNode, typeof(T)))
					{
						if (flag)
						{
							DirectXmlCrossRefLoader.RegisterListWantsCrossRef<T>(list, childNode.InnerText);
						}
						else
						{
							list.Add(DirectXmlToObject.ObjectFromXml<T>(childNode, true));
						}
					}
				}
				return list;
			}
			catch (Exception ex)
			{
				Log.Error("Exception loading list from XML: " + ex + "\nXML:\n" + listRootNode.OuterXml);
				return list;
			}
		}

		private static Dictionary<K, V> DictionaryFromXml<K, V>(XmlNode dictRootNode) where K : new() where V : new()
		{
			Dictionary<K, V> dictionary = new Dictionary<K, V>();
			try
			{
				bool flag = typeof(Def).IsAssignableFrom(typeof(K));
				bool flag2 = typeof(Def).IsAssignableFrom(typeof(V));
				if (!flag && !flag2)
				{
					{
						foreach (XmlNode childNode in dictRootNode.ChildNodes)
						{
							if (DirectXmlToObject.ValidateListNode(childNode, dictRootNode, typeof(KeyValuePair<K, V>)))
							{
								K key = DirectXmlToObject.ObjectFromXml<K>((XmlNode)childNode["key"], true);
								V value = DirectXmlToObject.ObjectFromXml<V>((XmlNode)childNode["value"], true);
								dictionary.Add(key, value);
							}
						}
						return dictionary;
					}
				}
				foreach (XmlNode childNode2 in dictRootNode.ChildNodes)
				{
					if (DirectXmlToObject.ValidateListNode(childNode2, dictRootNode, typeof(KeyValuePair<K, V>)))
					{
						DirectXmlCrossRefLoader.RegisterDictionaryWantsCrossRef<K, V>(dictionary, childNode2);
					}
				}
				return dictionary;
			}
			catch (Exception ex)
			{
				Log.Error("Malformed dictionary XML. Node: " + dictRootNode.OuterXml + ".\n\nException: " + ex);
				return dictionary;
			}
		}

		private static MethodInfo CustomDataLoadMethodOf(Type type)
		{
			return type.GetMethod("LoadDataFromXmlCustom", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
		}

		private static bool ValidateListNode(XmlNode listEntryNode, XmlNode listRootNode, Type listItemType)
		{
			if (listEntryNode is XmlComment)
			{
				return false;
			}
			if (listEntryNode is XmlText)
			{
				Log.Error("XML format error: Raw text found inside a list element. Did you mean to surround it with list item <li> tags? " + listRootNode.OuterXml);
				return false;
			}
			if (listEntryNode.Name != "li" && DirectXmlToObject.CustomDataLoadMethodOf(listItemType) == null)
			{
				Log.Error("XML format error: List item found with name that is not <li>, and which does not have a custom XML loader method, in " + listRootNode.OuterXml);
				return false;
			}
			return true;
		}
	}
}
