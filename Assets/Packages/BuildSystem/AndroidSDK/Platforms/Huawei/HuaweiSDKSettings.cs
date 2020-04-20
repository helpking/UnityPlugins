using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using Packages.Common;
using Packages.Common.Base;
using Packages.Common.Extend;
using Packages.Utils;
#if UNITY_ANDROID && UNITY_EDITOR
using Packages.BuildSystem.AndroidSDK.Manifest;
using Packages.BuildSystem.AndroidSDK.Platforms.Huawei.Manifest;

namespace Packages.BuildSystem.AndroidSDK.Platforms.Huawei {

	/// <summary>
	/// 安卓SDK设定信息数据.
	/// </summary>
	[Serializable]
	public class HuaweiSdkSettingsData : AndroidSdkSettingsData {

		/// <summary>
		/// App ID.
		/// </summary>
		[FormerlySerializedAs("AppID")] 
		public string appId;

		/// <summary>
		/// 支付ID.
		/// </summary>
		[FormerlySerializedAs("PayID")] 
		public string payId;

		/// <summary>
		/// 浮标密钥(CP必须存储在服务端，然后通过安全网络（如https）获取下来，存储到内存中，否则存在私钥泄露风险).
		/// </summary>
		[FormerlySerializedAs("BuoySecret")] 
		public string buoySecret;

		/// <summary>
		/// 支付私钥(CP必须存储在服务端，然后通过安全网络（如https）获取下来，存储到内存中，否则存在私钥泄露风险).
		/// </summary>
		[FormerlySerializedAs("PayPrivateRsa")] 
		public string payPrivateRsa;

		/// <summary>
		/// 支付公钥(CP必须存储在服务端，然后通过安全网络（如https）获取下来，存储到内存中，否则存在私钥泄露风险).
		/// </summary>
		[FormerlySerializedAs("PayPublicRsa")] 
		public string payPublicRsa;

		/// <summary>
		/// CPID(开发者对应的ID).
		/// </summary>
		[FormerlySerializedAs("CPID")] 
		public string cpid;

		/// <summary>
		/// 登录签名公钥.
		/// </summary>
		[FormerlySerializedAs("LoginPublicRsa")] 
		public string loginPublicRsa;

		/// <summary>
		/// 初始化.
		/// </summary>
		public override void Clear() {
			base.Clear ();

			PlatformType = PlatformType.Huawei;
			appId = null;
			payId = null;
			buoySecret = null;
			payPrivateRsa = null;
			payPublicRsa = null;
			cpid = null;
			loginPublicRsa = null;
		}
	}

	/// <summary>
	/// 华为SDK设定信息.
	/// </summary>
	[Serializable]
	public class HuaweiSdkSettings : AssetBase<HuaweiSdkSettings, HuaweiSdkSettingsData>, IAndroidSdkSettings {
		public const string AssetFileDir = "Assets/Packages/BuildSystem/AndroidSDK/Editor/Platforms/Huawei/Conf";

		/// <summary>
		/// 平台类型.
		/// </summary>
		public static PlatformType PlatformType => PlatformType.Huawei;

		/// <summary>
		/// 安卓SDK最小版本.
		/// </summary>
		public int MinSdkVersion {
			get { 
				if (null != data) {
					return data.MinSdkVersion;
				}
				return -1;
			}	
			set { 
				if (null != data) {
					data.MinSdkVersion = value;
				}
			}
		}

		/// <summary>
		/// 安卓SDK最大版本.
		/// </summary>
		public int MaxSdkVersion {
			get { 
				if (null != data) {
					return data.MaxSdkVersion;
				}
				return -1;
			}	
			set { 
				if (null != data) {
					data.MaxSdkVersion = value;
				}
			}	
		}

		/// <summary>
		/// 安卓SDK目标版本.
		/// </summary>
		public int TargetSdkVersion {
			get { 
				if (null != data) {
					return data.TargetSdkVersion;
				}
				return -1;
			}
			set { 
				if (null != data) {
					data.TargetSdkVersion = value;
				}
			}	
		}

		/// <summary>
		/// App ID.
		/// </summary>
		public string AppId {
			get
			{
				return data?.appId;
			}
			set { 
				if (null != data) {
					data.appId = value;
				}
			}	
		}

		/// <summary>
		/// 浮标密钥(CP必须存储在服务端，然后通过安全网络（如https）获取下来，存储到内存中，否则存在私钥泄露风险).
		/// </summary>
		public string BuoySecret {
			get
			{
				return data?.buoySecret;
			}	
			set { 
				if (null != data) {
					data.buoySecret = value;
				}
			}	
		}

		/// <summary>
		/// 支付ID.
		/// </summary>
		public string PayId {
			get
			{
				return data?.payId;
			}		
			set { 
				if (null != data) {
					data.payId = value;
				}
			}
		}

		/// <summary>
		/// 支付私钥(CP必须存储在服务端，然后通过安全网络（如https）获取下来，存储到内存中，否则存在私钥泄露风险).
		/// </summary>
		public string PayPrivateRsa {
			get
			{
				return data?.payPrivateRsa;
			}		
			set { 
				if (null != data) {
					data.payPrivateRsa = value;
				}
			}	
		}

		/// <summary>
		/// 支付公钥(CP必须存储在服务端，然后通过安全网络（如https）获取下来，存储到内存中，否则存在私钥泄露风险).
		/// </summary>
		public string PayPublicRsa {
			get
			{
				return data?.payPublicRsa;
			}
			set { 
				if (null != data) {
					data.payPublicRsa = value;
				}
			}	
		}


		/// <summary>
		/// CPID(开发者对应的ID).
		/// </summary>
		public string Cpid {
			get
			{
				return data?.cpid;
			}	
			set { 
				if (null != data) {
					data.cpid = value;
				}
			}
		}


		/// <summary>
		/// 登录签名公钥.
		/// </summary>
		public string LoginPublicRsa {
			get
			{
				return data?.loginPublicRsa;
			}	
			set { 
				if (null != data) {
					data.loginPublicRsa = value;
				}
			}
		}

		/// <summary>
		/// 屏幕方向.
		/// </summary>
		public UIOrientation Orientation {
			get
			{
				return data?.Orientation ?? PlayerSettings.defaultInterfaceOrientation;
			}	
			set { 
				if (null != data) {
					data.Orientation = value;
				}
			}
		}

		/// <summary>
		/// 本地存储标志位（false:向服务器拉取相关信息）.
		/// </summary>
		public bool Local {
			get
			{
				return null == data || data.Local;
			}	
			set { 
				if (null != data) {
					data.Local = value;
				}
			}
		}

		/// <summary>
		/// SDK自动初始化.
		/// </summary>
		public bool AutoSdkInit {
			get
			{
				return null == data || data.AutoSdkInit;
			}	
			set { 
				if (null != data) {
					data.AutoSdkInit = value;
				}
			}
		}

		/// <summary>
		/// 自动登录标志位.
		/// </summary>
		public bool AutoLogin {
			get
			{
				return null == data || data.AutoLogin;
			}	
			set { 
				if (null != data) {
					data.AutoLogin = value;
				}
			}
		}

#region abstract - AssetBase

		/// <summary>
		/// 取得导入路径.
		/// </summary>
		/// <returns>导入路径.</returns>
		public override string GetImportPath () {
			return $"{AssetFileDir}/Json";
		}

		/// <summary>
		/// 取得导出路径.
		/// </summary>
		/// <returns>导出路径.</returns>
		public override string GetExportPath () {
			return $"{AssetFileDir}/Json";
		}

		/// <summary>
		/// 初始化数据.
		/// </summary>
		/// <returns><c>true</c>, OK, <c>false</c> NG.</returns>
		protected override bool InitAsset () { 
			return InitSettings(); 
		}

		/// <summary>
		/// 应用数据.
		/// </summary>
		/// <param name="iData">数据.</param>
		/// <param name="iForceClear">强制清空.</param>
		protected override void ApplyData(HuaweiSdkSettingsData iData, bool iForceClear = true) {

			if (null == iData) {
				return;
			}

			// 清空
			if (iForceClear) {
				Clear ();
			}

			data.MinSdkVersion = iData.MinSdkVersion;
			data.MaxSdkVersion = iData.MaxSdkVersion;
			data.TargetSdkVersion = iData.TargetSdkVersion;
			data.appId = iData.appId;
			data.buoySecret = iData.buoySecret;
			data.payId = iData.payId;
			data.payPrivateRsa = iData.payPrivateRsa;
			data.payPublicRsa = iData.payPublicRsa;
			data.cpid = iData.cpid;
			data.loginPublicRsa = iData.loginPublicRsa;
			data.Orientation = iData.Orientation;
			data.Local = iData.Local;
			data.AutoSdkInit = iData.AutoSdkInit;
			data.AutoLogin = iData.AutoLogin;

			UtilsAsset.SetAssetDirty (this);

		}

#endregion

#region Interface - IAndroidSDKSettings

		/// <summary>
		/// 取得导出华为用的AndroidManifest.xml文件路径.
		/// </summary>
		/// <returns>导出华为用的AndroidManifest.xml文件路径.</returns>
		public string GetAndroidManifestXmlPath() {
			var manifestXmlPath = $"{Application.dataPath}/Plugins/Android/AndroidManifest.xml";
			return false == File.Exists (manifestXmlPath) ? null : manifestXmlPath;
		}

		/// <summary>
		/// 初始化设定信息.
		/// </summary>
		public bool InitSettings() {

			// 路径
			_path = AssetFileDir;

			return true; 
		}

		/// <summary>
		/// 取得拷贝源文件目录.
		/// </summary>
		/// <returns>取得拷贝源文件目录.</returns>
		public string GetAndroidCopyFromDir() {

			var dir = $"{Application.dataPath}/../AndroidPlatform";
			if (false == Directory.Exists (dir)) {
				this.Warning("GetAndroidCopyFromDir()::The directory is not exist!!(Dir:{0})", dir);
				Directory.CreateDirectory (dir);
			}

			dir = $"{dir}/{PlatformType.ToString()}";
			if (Directory.Exists(dir)) return dir;
			this.Warning("GetAndroidCopyFromDir()::The directory is not exist!!(Dir:{0})", dir);
			Directory.CreateDirectory (dir);

			return dir;
		}

		/// <summary>
		/// 取得拷贝目的文件目录.
		/// </summary>
		/// <returns>取得拷贝目的文件目录.</returns>
		public string GetAndroidCopyToDir() {

			var dir = $"{Application.dataPath}/Plugins/Android";
			if (Directory.Exists(dir)) return dir;
			this.Warning("GetAndroidCopyToDir()::The directory is not exist!!(Dir:{0})", dir);
			Directory.CreateDirectory (dir);

			return dir;
		}

		/// <summary>
		/// 取得AndroidManifest对象.
		/// </summary>
		/// <returns>AndroidManifest对象.</returns>
		/// <param name="iGameName">游戏名.</param>
		public ManifestBase GetAndroidManifest (string iGameName) {
			ManifestBase manifest = null;
			var huaweiDir = GetAndroidCopyFromDir ();
			if (Directory.Exists (huaweiDir)) {
				manifest = HuaweiManifest.GetInstance (huaweiDir, iGameName);
			}
			return manifest;
		}


		/// <summary>
		/// 应用设定信息到AndroidManifest.xml.
		/// </summary>
		/// <param name="iManifest">AndroidManifest对象.</param>
		/// <param name="iPackageName">游戏包名.</param>
		public bool AppSettingsToAndroidManifestFile (
			ManifestBase iManifest, string iPackageName) {

			if (null == iManifest) {
				return false;
			}

			if (false == string.IsNullOrEmpty (iPackageName)) {
				iManifest.ApplyPackageName (iPackageName);  
			}

			return true;
		}

		/// <summary>
		/// 打包Android（apk文件）之前，提前应用设定.
		/// </summary>
		/// <param name="iGameName">游戏名.</param>
		/// <param name="iPackageName">游戏包名.</param>
		public void PreApplyAndroidBuild (string iGameName, string iPackageName) {

			// 合并AndroidManifest.xml文件
			if(MergeManifestFile(iGameName, iPackageName)) {
				// 拷贝库资源文件
				CopyResources();
			}
		}

		/// <summary>
		/// 合并AndroidManifest.xml文件.
		/// </summary>
		/// <returns><c>true</c>, OK, <c>false</c> NG.</returns>
		/// <param name="iGameName">游戏名.</param>
		/// <param name="iPackageName">游戏包名.</param>
		public bool MergeManifestFile(string iGameName, string iPackageName) {

			var manifest = GetAndroidManifest(iGameName);
			if (null == manifest) {
				return false;
			}

			// 保存路径
			var copyToManifestFile = GetAndroidCopyToDir();
			var savePath = $"{copyToManifestFile}/AndroidManifest.xml";

			// 保存AndroidManifest.xml文件
			return SaveAndroidManifestFile (manifest, savePath, iPackageName);

		}

		/// <summary>
		/// 保存AndroidManifest.xml文件.
		/// </summary>
		/// <param name="iManifest">AndroidManifest对象.</param>
		/// <param name="iSavePath">保存路径.</param>
		/// <param name="iPackageName">游戏包名.</param>
		public bool SaveAndroidManifestFile (ManifestBase iManifest, string iSavePath, string iPackageName) {
			if (null == iManifest) {
				return false;
			}

			// 应用设定信息到AndroidManifest.xml
			var bolRet = AppSettingsToAndroidManifestFile (iManifest, iPackageName);
			if (bolRet) {
				// 保存
				iManifest.Save (iSavePath);
			}
			return bolRet;
		}

		/// <summary>
		/// 拷贝库资源文件.
		/// </summary>
		public void CopyResources () {
			var copyFromDir = GetAndroidCopyFromDir ();
			var copyToDir = GetAndroidCopyToDir ();

			// Libs
			var files = Directory.GetFiles (copyFromDir);
			foreach (var file in files) {
				if (file.EndsWith ("AndroidManifest.xml")) {
					continue;
				}
				if (file.EndsWith (".meta")) {
					continue;
				}

				var lastIndex = file.LastIndexOf ("/", StringComparison.Ordinal);
				var fileName = file.Substring (lastIndex + 1);
				if (string.IsNullOrEmpty (fileName)) {
					continue;
				}

				var copyToFile = $"{copyToDir}/{fileName}";
				if (File.Exists (copyToFile)) {
					File.Delete (copyToFile);
				}
				this.Info("CopyResources()::Copy Libs : {0} -> {1}",
					file, copyToFile);

				File.Copy (file, copyToFile);
			}

			// res
			var copyRes = $"{copyFromDir}/res";
			if (Directory.Exists (copyRes)) {
				UtilsTools.CopyDirectory (copyRes, copyToDir);
			}
		}

#endregion
	}

}

#endif
