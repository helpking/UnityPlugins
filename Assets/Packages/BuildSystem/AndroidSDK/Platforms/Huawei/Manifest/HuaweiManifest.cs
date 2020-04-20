using System;
using System.IO;
using System.Xml;
using UnityEditor;
using Packages.BuildSystem.AndroidSDK.Manifest;
using Packages.Logs;

#if UNITY_ANDROID && UNITY_EDITOR

namespace Packages.BuildSystem.AndroidSDK.Platforms.Huawei.Manifest {

	/// <summary>
	/// Huawei manifest类.
	/// </summary>
	public class HuaweiManifest : ManifestBase {

		/// <summary>
		/// 实例.
		/// </summary>
		private static HuaweiManifest _instance = null;

		/// <summary>
		/// 取得实例.
		/// </summary>
		/// <returns>实例.</returns>
		/// <param name="iHuaweiDir">华为路径.</param>
		/// <param name="iGameName">游戏名.</param>
		public static HuaweiManifest GetInstance(string iHuaweiDir, string iGameName = null) {
			try {
				if(_instance == null) {
					_instance = new HuaweiManifest ();
				}
				if(false == _instance.InitByHuaweiDir(iHuaweiDir, iGameName)) {
					_instance = null;
				}
			} catch (Exception e) {
				Loger.Fatal($"[HuaweiManifest Create Failed] Exeption : {e.Message}");
				_instance = null;
			}
			return _instance;
		}

		/// <summary>
		/// 初始化.
		/// </summary>
		/// <returns><c>true</c>, OK, <c>false</c> NG.</returns>
		/// <param name="iHuaweiDir">华为路径.</param>
		/// <param name="iGameName">游戏名.</param>
		public bool InitByHuaweiDir(string iHuaweiDir, string iGameName) {
			Dir = iHuaweiDir;

			var manifestPath = $"{iHuaweiDir}/AndroidManifest.xml";
			return File.Exists (manifestPath) && Init (manifestPath, iGameName);
		}

		/// <summary>
		/// 初始化SDK版本信息.
		/// </summary>
		/// <returns>SDK版本信息节点.</returns>
		protected override XmlElement InitSdkVersions() {
			var _useSdkNode = base.InitSdkVersions ();
			if (null == _useSdkNode) {
				return null;
			}
			if (-1 >= HuaweiSdkSettings.GetInstance ().MinSdkVersion) {
				MinSdkVersion = GetNodeAttribute_i (_useSdkNode, "minSdkVersion");
			} else {
				MinSdkVersion = HuaweiSdkSettings.GetInstance ().MinSdkVersion;
				SetNodeAttribute (_useSdkNode, "minSdkVersion", MinSdkVersion.ToString());
			}
			if (-1 >= HuaweiSdkSettings.GetInstance ().MaxSdkVersion) {
				MaxSdkVersion = GetNodeAttribute_i (_useSdkNode, "android:maxSdkVersion");
			} else {
				MaxSdkVersion = HuaweiSdkSettings.GetInstance ().MaxSdkVersion;
				SetNodeAttribute (_useSdkNode, "maxSdkVersion", MaxSdkVersion.ToString());
			}
			if (-1 >= HuaweiSdkSettings.GetInstance ().TargetSdkVersion) {
				TargetSdkVersion = GetNodeAttribute_i (_useSdkNode, "android:targetSdkVersion");
			} else {
				TargetSdkVersion = HuaweiSdkSettings.GetInstance ().TargetSdkVersion;
				SetNodeAttribute (_useSdkNode, "targetSdkVersion", TargetSdkVersion.ToString());
			}
			return _useSdkNode;
		}
			
		/// <summary>
		/// 应用包名.
		/// </summary>
		/// <param name="iPackageName">游戏包名.</param>
		public override void ApplyPackageName(string iPackageName) {
			base.ApplyPackageName (iPackageName);

			// 游戏名

			// 注意：这个在targetSDK >= 24时，在游戏中必须申明，否则影响N版本下使用SDK安装华为游戏中心。
			// SDK安装华为游戏中心;如果targetSDK < 24, 则可以不配置Provider
			// 其中android:authorities里“游戏包名”必须要替换为游戏自己包名，否则会导致冲突，游戏无法安装！
			// 详细说明请参考SDK接口文档
			ApplyProviderNodeInfo (iPackageName);
		}

		public void ApplyProviderNodeInfo(string iPackageName) {
			if (24 > TargetSdkVersion) {
				return;
			}
			var applicationNode = GetApplicationNode ();
			if (null == applicationNode) {
				return;
			}
			var _old = applicationNode.SelectSingleNode ("provider");
			if (null != _old) {
				applicationNode.RemoveChild (_old);
			}
			var _new = CreateElement ("provider");
			applicationNode.AppendChild (_new);

			// android:name
			{
				const string name = "name";
				const string value = "android.support.v4.content.FileProvider";
				SetNodeAttribute (_new, name, value);
			}

			// android:authorities
			{
				const string name = "authorities";
				var value = $"{iPackageName}.installnewtype.provider";
				SetNodeAttribute (_new, name, value);
			}

			// android:exported
			{
				const string name = "exported";
				const string value = "false";
				SetNodeAttribute (_new, name, value);
			}

			// android:grantUriPermissions
			{
				const string name = "grantUriPermissions";
				const string value = "true";
				SetNodeAttribute (_new, name, value);
			}

			// meta-data
			var metaData = CreateNode(_new, "meta-data");
			if (null == metaData) return;
			{
				// android:name
				{
					const string name = "name";
					const string value = "android.support.FILE_PROVIDER_PATHS";
					SetNodeAttribute (metaData, name, value);
				}
				// android:resource
				{
					const string name = "resource";
					const string value = "@xml/buoy_provider_paths";
					SetNodeAttribute (metaData, name, value);
				}
			}
		}

		/// <summary>
		/// 取得res/values目录下的strings.xml的文件路径.
		/// </summary>
		/// <returns>strings.xml文件路径.</returns>
		protected override string GetStringsXmlPath() {
			return $"{Dir}/res/values/huawei_strings.xml";
		}

		/// <summary>
		/// 应用用户自定义数据.
		/// </summary>
		/// <param name="iGameName">游戏名.</param>
		protected override void ApplyUserData(string iGameName) {

			// 本地设定
			{
				const string name = "Huawei_Local";
				var value = HuaweiSdkSettings.GetInstance ().Local.ToString();
				if (false == string.IsNullOrEmpty (value)) {
					AddUserDefineNode (name, value, false);
				}
				if (HuaweiSdkSettings.GetInstance ().Local == false) {
					return;
				}
			}

			// 游戏名
			{
				const string name = "Huawei_GameName";
				var value = iGameName;
				if (false == string.IsNullOrEmpty (value)) {
					AddUserDefineNode (name, value);
				}
			}

			// 自动登录
			{
				const string name = "Huawei_AutoLogin";
				var value = HuaweiSdkSettings.GetInstance ().AutoLogin.ToString();
				if (false == string.IsNullOrEmpty (value)) {
					AddUserDefineNode (name, value, false);
				}
				if (HuaweiSdkSettings.GetInstance ().Local == false) {
					return;
				}
			}
				
			// AppID
			{
				const string name = "Huawei_AppID";
				var value = HuaweiSdkSettings.GetInstance().AppId;
				if (false == string.IsNullOrEmpty (value)) {
					AddUserDefineNode (name, value);
				}
			}
			// 浮标密钥
			{
				const string name = "Huawei_BuoySecret";
				var value = HuaweiSdkSettings.GetInstance().BuoySecret;
				if (false == string.IsNullOrEmpty (value)) {
					AddUserDefineNode (name, value);
				}
			}
			// 支付ID
			{
				const string name = "Huawei_PayID";
				var value = HuaweiSdkSettings.GetInstance().PayId;
				if (false == string.IsNullOrEmpty (value)) {
					AddUserDefineNode (name, value);
				}
			}
			// 支付私钥
			{
				const string name = "Huawei_PayPrivateRsa";
				var value = HuaweiSdkSettings.GetInstance().PayPrivateRsa;
				if (false == string.IsNullOrEmpty (value)) {
					AddUserDefineNode (name, value);
				}
			}
			// 支付公钥
			{
				const string name = "Huawei_PayPublicRsa";
				var value = HuaweiSdkSettings.GetInstance().PayPublicRsa;
				if (false == string.IsNullOrEmpty (value)) {
					AddUserDefineNode (name, value);
				}
			}
			// CPID
			{
				const string name = "Huawei_CPID";
				var value = HuaweiSdkSettings.GetInstance().Cpid;
				if (false == string.IsNullOrEmpty (value)) {
					AddUserDefineNode (name, value);
				}
			}
			// 登录签名公钥
			{
				const string name = "Huawei_LoginPublicRsa";
				var value = HuaweiSdkSettings.GetInstance().LoginPublicRsa;
				if (false == string.IsNullOrEmpty (value)) {
					AddUserDefineNode (name, value);
				}
			}
			// 屏幕方向
			{
				const string name = "Huawei_Orientation";
				var value = "1";
				var _orientation = HuaweiSdkSettings.GetInstance ().Orientation;
				if (UIOrientation.LandscapeLeft == _orientation ||
					UIOrientation.LandscapeRight == _orientation) {
					value = "2";
				}
				if (false == string.IsNullOrEmpty (value)) {
					AddUserDefineNode (name, value, false);
				}
			}

			// 保存strings.xml
			StringsXml?.Save ();
		}
	}
}
#endif
