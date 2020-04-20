using System;
using System.IO;
using System.Xml;
using Packages.BuildSystem.AndroidSDK.Res.Values;
using Packages.Common.Base;
using Packages.Settings;
using Packages.Logs;
using UnityEngine.Serialization;

namespace Packages.BuildSystem.AndroidSDK.Manifest {

	/// <summary>
	/// AndroidManifest.xml中 <meta-data/>节点数据定义.
	/// </summary>
	[Serializable]
	public class MetaDataInfo : JsonDataBase<MetaDataInfo> {

		/// <summary>
		/// 属性：android:name.
		/// </summary>
		[FormerlySerializedAs("Name")] 
		public string name;

		/// <summary>
		/// 属性：android:value.
		/// </summary>
		[FormerlySerializedAs("Value")] 
		public string value;
					
		/// <summary>
		/// 清空.
		/// </summary>
		public override void Clear() {
			base.Clear ();
			name = null;
			value = null;
		}
	}

	/// <summary>
	/// 安卓配置文件
	/// </summary>
	public abstract class ManifestBase : XmlDocument {

		/// <summary>
		/// 目录路径.
		/// </summary>
		protected string Dir {
			get;
			set;
		}

		protected int MinSdkVersion {
			get;
			set;
		}
		protected int MaxSdkVersion {
			get;
			set;
		}
		protected int TargetSdkVersion {
			get;
			set;
		}
		protected string AndroidNameSpace {
			get;
			set;
		}

		/// <summary>
		/// The strings xml.
		/// </summary>
		protected StringsXmlBase StringsXml {
			get;
			set;
		}

		/// <summary>
		/// 初始化.
		/// </summary>
		/// <param name="iPath">路径.</param>
		/// <param name="iGameName">游戏名.</param>
		public bool Init(string iPath, string iGameName){

			try {
				Load(iPath);

				// 创建strings.xml对象
				StringsXml = CreateStringsXml();

				// 初始化Appliction
				InitApplicationInfo();

				// 初始化SDK版本信息.
				if (null == InitSdkVersions()) {
					return false;
				}

				// 应用用户自定义数据
				ApplyUserData(iGameName);

			} catch (Exception e) {
				Loger.Fatal ($"ManifestBase()::Init():Failed!!! Exeption:{e.Message}");
				return false;
			}

			return true;
		}

		/// <summary>
		/// 初始化SDK版本信息.
		/// </summary>
		protected virtual XmlElement InitSdkVersions() {
			var manifestNode = GetManifestNode ();

			var useSdkNode = manifestNode?.SelectSingleNode ("uses-sdk") as XmlElement;
			return useSdkNode;
		}

		/// <summary>
		/// 设定节点属性.
		/// </summary>
		/// <param name="iNode">节点.</param>
		/// <param name="iAttributeName">节点属性名.</param>
		/// <param name="iValue">属性值.</param>
		/// <param name="iPrefix">前缀.</param>
		protected void SetNodeAttribute(XmlElement iNode, string iAttributeName, string iValue, string iPrefix = "android") {

			if (null == iNode) {
				return;
			}
			var name = iAttributeName;
			if (false == string.IsNullOrEmpty (iPrefix)) {
				name = $"{iPrefix}:{name}";
			}
			var attribute = iNode.GetAttributeNode (name) ?? 
			                (string.IsNullOrEmpty (iPrefix) ? 
				                CreateAttribute (iAttributeName) : 
				                CreateAttribute (iPrefix, iAttributeName, AndroidNameSpace));
			attribute.Value = iValue;
			iNode.Attributes.Append (attribute);
		}

		/// <summary>
		/// 取得Node指定的属性值.
		/// </summary>
		/// <returns>属性值.</returns>
		/// <param name="iNode">节点.</param>
		/// <param name="iName">属性名.</param>
		protected static int GetNodeAttribute_i(XmlElement iNode, string iName) {
			var value = iNode.GetAttribute (iName);
			if (string.IsNullOrEmpty (value)) {
				return -1;
			}
			return Convert.ToInt32 (value);
		}

		/// <summary>
		/// 取得Node指定的属性值.
		/// </summary>
		/// <returns>属性值.</returns>
		/// <param name="iNode">节点.</param>
		/// <param name="iName">属性名.</param>
		protected static string GetNodeAttribute_s(XmlElement iNode, string iName) {
			var value = iNode.GetAttribute (iName);
			return string.IsNullOrEmpty (value) ? null : value;
		}

		/// <summary>
		/// 添加用户数据.
		/// </summary>
		/// <param name="iParent">父节点名.</param>
		/// <param name="iTagName">标签名</param>
		protected XmlElement CreateNode(XmlNode iParent, string iTagName) {
			if (null == iParent) {
				return null;
			}
			var metaData = CreateElement (iTagName);
			iParent.AppendChild (metaData);
			return metaData;
		}

		/// <summary>
		/// 添加用户数据自定义节点.
		/// </summary>
		/// <returns>用户数据自定义节点.</returns>
		/// <param name="iAttributeName">节点属性名.</param>
		/// <param name="iValue">值.</param>
		/// <param name="iIsStrings">是否定义在strings.xml中.</param>
		protected XmlElement AddUserDefineNode(string iAttributeName, string iValue, bool iIsStrings = true) {
			var parent = GetApplicationNode ();
			if (null == parent) {
				return null;
			}

			var list = parent.SelectNodes ("meta-data");
			XmlElement child = null;
			if (list != null)
				foreach (XmlNode node in list)
				{
					var nodeTmp = node as XmlElement;
					if (null == nodeTmp)
					{
						continue;
					}

					var name = nodeTmp.GetAttribute("name", AndroidNameSpace);
					if (string.IsNullOrEmpty(name) || !name.Equals(iAttributeName)) continue;
					child = nodeTmp;
					break;
				}

			if (null == child) {
				child = CreateNode (parent, "meta-data");
			}

			if (null == child) return null;
			SetNodeAttribute (child, "name", iAttributeName);
			if (false == iIsStrings) {
				SetNodeAttribute (child, "value", iValue);
			} else {
				SetNodeAttribute (child, "value", string.Format("@string/{0}", iAttributeName));

				// 设定值到strings.xml
				StringsXml?.AddString (iAttributeName, iValue);
			}
			return child;
		}

#region virtual

		/// <summary>
		/// 初始化Appliction.
		/// </summary>
		protected void InitApplicationInfo() {

			var manifestNode = GetManifestNode ();
			var manifestNodeTmp = manifestNode as XmlElement;
			if (manifestNodeTmp == null) {
				return;
			}
			// Android NameSpace
			AndroidNameSpace = GetNodeAttribute_s(manifestNodeTmp, "xmlns:android");

			var appNode = GetApplicationNode ();
			var appNodeTmp = appNode as XmlElement;
			if (appNodeTmp == null) {
				return;
			}
			
			// Icon
			SetNodeAttribute (appNodeTmp, "icon", "@drawable/app_icon");

			// debug
			SetNodeAttribute(appNodeTmp, "debuggable",
				BuildMode.Debug == SysSettings.GetInstance().BuildMode ? "true" : "false");
		}

		/// <summary>
		/// 应用包名.
		/// </summary>
		/// <param name="iPackageName">游戏包名.</param>
		public virtual void ApplyPackageName(string iPackageName) {
			var manifestNode = GetManifestNode ();
			var manifestNodeTmp = manifestNode as XmlElement;
			if (manifestNodeTmp == null) {
				return;
			}
			SetNodeAttribute (manifestNodeTmp, "package", iPackageName, null);
		}

		/// <summary>
		/// 应用用户自定义数据.
		/// </summary>
		/// <param name="iGameName">游戏名.</param>
		protected virtual void ApplyUserData(string iGameName) {}

		/// <summary>
		/// 创建 strings xml.
		/// </summary>
		/// <returns>string的XML文件对象.</returns>
		protected virtual StringsXmlBase CreateStringsXml() { 

			var filePath = GetStringsXmlPath ();
			if (false == File.Exists (filePath)) {
				return null;
			}
			var stringsXmlTmp = StringsXmlBase.LoadXML(filePath);
			return stringsXmlTmp;
		}

#endregion

#region XML

		public XmlNode GetManifestNode() {
			return SelectSingleNode("/manifest");
		}

		public XmlNode GetApplicationNode() {
			var rootNode = GetManifestNode ();
			return rootNode?.SelectSingleNode("application");
		}

#endregion

#region abstract

		/// <summary>
		/// 取得res/values目录下的strings.xml的文件路径.
		/// </summary>
		/// <returns>strings.xml文件路径.</returns>
		protected abstract string GetStringsXmlPath();

#endregion

	}

}
