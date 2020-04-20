using System;
using System.Xml;
using Packages.Logs;
using UnityEngine;

namespace Packages.BuildSystem.AndroidSDK.Res.Values {

	/// <summary>
	/// Strings XML base.
	/// </summary>
	public class StringsXmlBase : XmlDocument {

		/// <summary>
		/// 保存地址.
		/// </summary>
		private string SavePath {
			get;
			set;
		}

		/// <summary>
		/// 加载res/values下的strings.XML对象.
		/// </summary>
		/// <returns>strings.XML对象.</returns>
		/// <param name="iPath">路径.</param>
		public static StringsXmlBase LoadXML(string iPath) {
			var xml = new StringsXmlBase ();
			return xml.Init (iPath) ? xml : null;
		}

		/// <summary>
		/// 构造函数：禁止外部New.
		/// </summary>
		protected StringsXmlBase () {
			
		}

		/// <summary>
		/// 初始化.
		/// </summary>
		/// <param name="iPath">I path.</param>
		protected virtual bool Init(string iPath){

			try {
				
				// 保存路径
				SavePath = iPath;

				// 加载
				Load(iPath);

			} catch (Exception e) {
				Loger.Fatal($"StringsXMLBase::Init()::Failed!!! Exeption:{e.Message}");
				return false;
			}

			return true;
		}

		/// <summary>
		/// 添加字符串.
		/// </summary>
		/// <param name="iName">名字.</param>
		/// <param name="iValue">值.</param>
		public void AddString(string iName, string iValue) {
			
			if(string.IsNullOrEmpty(iName)){
				return;
			}
		
			var rootNode = GetResourcesNode ();
			if (null == rootNode) {
				return;
			}

			var list = rootNode.SelectNodes ("string");
			XmlElement child = null;
			if (list != null)
				foreach (XmlNode node in list)
				{
					var nodeTmp = node as XmlElement;
					if (null == nodeTmp)
					{
						continue;
					}

					var name = nodeTmp.GetAttribute("name");
					if (string.IsNullOrEmpty(name) || !name.Equals(iName)) continue;
					child = nodeTmp;
					break;
				}

			if (null == child) {
				child = CreateElement ("string");
				rootNode.AppendChild (child);
				child.SetAttribute ("name", iName);
			}
			child.InnerText = iValue;
		}
			
#region XML

		public XmlNode GetResourcesNode() {
			return SelectSingleNode("/resources");
		}

		/// <summary>
		/// 保存文件.
		/// </summary>
		public void Save() {
			if (string.IsNullOrEmpty (SavePath)) {
				return;
			}
			Save (SavePath);
		}

#endregion

	}

}
