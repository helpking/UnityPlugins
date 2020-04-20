using System;
using System.IO;
using UnityEngine;
using Packages.Common;
using Packages.Common.Base;
using Packages.Common.Extend;
using Packages.Utils;
#if UNITY_ANDROID && UNITY_EDITOR
using Packages.BuildSystem.AndroidSDK.Manifest;
using Packages.BuildSystem.AndroidSDK.Options;
using Packages.BuildSystem.AndroidSDK.Platforms.Tiange.Manifest;
using UnityEditor;

namespace Packages.BuildSystem.AndroidSDK.Platforms.Tiange {

	/// <summary>
	/// 天鸽SDK设定信息数据.
	/// </summary>
	[Serializable]
	public class TiangeSdkSettingsData : AndroidSdkSettingsData {

		/// <summary>
		/// 清空.
		/// </summary>
		public override void Clear() {
			base.Clear ();

			PlatformType = PlatformType.None;
			MinSdkVersion = 19;
			MaxSdkVersion = 26;
			TargetSdkVersion = 25;
			Local = true;
			AutoSdkInit = false;
			AutoLogin = false;
		}
	}

	/// <summary>
	/// 天鸽SDK设定数据.
	/// </summary>
	[Serializable]
	public class TiangeSdkData : OptionsDataBase<TiangeSdkSettingsData, BuildSettingOptionsData> {}

	/// <summary>
	/// 天鸽SDK设定.
	/// </summary>
	public class TiangeSdkSettings 
		: AssetOptionsBase<TiangeSdkSettings, TiangeSdkData, TiangeSdkSettingsData, BuildSettingOptionsData>, IAndroidSdkSettings {
		
		public const string AssetFileDir = "Assets/Packages/AndroidSDK/Editor/Platforms/Tiange/Conf";

		/// <summary>
		/// 平台类型.
		/// </summary>
		public PlatformType PlatformType {
			get {
				if (null == data) return PlatformType.Tiange;
				if (PlatformType.Tiange != data.General.PlatformType) {
					return data.General.PlatformType = PlatformType.Tiange;
				}
				return PlatformType.Tiange;
			}	
		}

		/// <summary>
		/// 安卓SDK最小版本.
		/// </summary>
		public int MinSdkVersion {
			get { 
				if (null != data) {
					return data.General.MinSdkVersion;
				}
				return -1;
			}	
			set { 
				if (null != data) {
					data.General.MinSdkVersion = value;
				}
			}
		}

		/// <summary>
		/// 安卓SDK最大版本.
		/// </summary>
		public int MaxSdkVersion {
			get { 
				if (null != data) {
					return data.General.MaxSdkVersion;
				}
				return -1;
			}	
			set { 
				if (null != data) {
					data.General.MaxSdkVersion = value;
				}
			}	
		}

		/// <summary>
		/// 安卓SDK目标版本.
		/// </summary>
		public int TargetSdkVersion {
			get { 
				if (null != data) {
					return data.General.TargetSdkVersion;
				}
				return -1;
			}
			set { 
				if (null != data) {
					data.General.TargetSdkVersion = value;
				}
			}	
		}

		/// <summary>
		/// 屏幕方向.
		/// </summary>
		public UIOrientation Orientation {
			get
			{
				return data?.General.Orientation ?? PlayerSettings.defaultInterfaceOrientation;
			}	
			set { 
				if (null != data) {
					data.General.Orientation = value;
				}
			}
		}

		/// <summary>
		/// 本地存储标志位（false:向服务器拉取相关信息）.
		/// </summary>
		public bool Local {
			get
			{
				return null == data || data.General.Local;
			}	
			set { 
				if (null != data) {
					data.General.Local = value;
				}
			}
		}

		/// <summary>
		/// SDK自动初始化.
		/// </summary>
		public bool AutoSdkInit {
			get
			{
				return null == data || data.General.AutoSdkInit;
			}	
			set { 
				if (null != data) {
					data.General.AutoSdkInit = value;
				}
			}
		}

		/// <summary>
		/// 自动登录标志位.
		/// </summary>
		public bool AutoLogin {
			get
			{
				return null == data || data.General.AutoLogin;
			}	
			set { 
				if (null != data) {
					data.General.AutoLogin = value;
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
		protected override void ApplyData(TiangeSdkData iData, bool iForceClear = true) {

			if (null == iData) {
				return;
			}

			// 清空
			if (iForceClear) {
				Clear ();
			}

			data.General.MinSdkVersion = iData.General.MinSdkVersion;
			data.General.MaxSdkVersion = iData.General.MaxSdkVersion;
			data.General.TargetSdkVersion = iData.General.TargetSdkVersion;
			data.General.Orientation = iData.General.Orientation;
			data.General.Local = iData.General.Local;
			data.General.AutoSdkInit = iData.General.AutoSdkInit;
			data.General.AutoLogin = iData.General.AutoLogin;

			data.Options.data = iData.Options.data;
			data.Options.OneSDK.zyClassName = iData.Options.OneSDK.zyClassName;
			data.Options.OneSDK.metaDatas.AddRange(iData.Options.OneSDK.metaDatas);

			UtilsAsset.SetAssetDirty (this);

		}

#endregion


#region Interface - IAndroidSDKSettings

		/// <summary>
		/// 取得导出天鸽用的AndroidManifest.xml文件路径.
		/// </summary>
		/// <returns>导出天鸽用的AndroidManifest.xml文件路径.</returns>
		public string GetAndroidManifestXmlPath() {
			var manifestXmlPath = $"{Application.dataPath}/Plugins/Android/AndroidManifest.xml";
			return false == File.Exists (manifestXmlPath) ? null : manifestXmlPath;
		}

		/// <summary>
		/// 初始化设定信息.
		/// </summary>
		public bool InitSettings() {

			// 路径
			_assetPath = AssetFileDir;

			return true; 
		}

		/// <summary>
		/// 取得拷贝源文件目录.
		/// </summary>
		/// <returns>取得拷贝源文件目录.</returns>
		public string GetAndroidCopyFromDir() {

			var dir = $"{Application.dataPath}/../AndroidPlatform";
			if (false == Directory.Exists (dir))
			{
				this.Warning("GetAndroidCopyFromDir():The directory is not exist!!(Dir:{0})", dir);
				Directory.CreateDirectory (dir);
			}

			dir = $"{dir}/{PlatformType}";
			if (Directory.Exists(dir)) return dir;
			this.Warning("GetAndroidCopyFromDir():The directory is not exist!!(Dir:{0})", dir);
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
			this.Warning("GetAndroidCopyToDir():The directory is not exist!!(Dir:{0})", dir);
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
			var dir = GetAndroidCopyFromDir ();
			if (Directory.Exists (dir)) {
				manifest = TiangeManifest.GetInstance (dir, iGameName);
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

			// 拷贝资源文件包含子文件夹中的内容
			CopyAllFiles(copyFromDir, copyToDir);

		}

		/// <summary>
		/// 拷贝所有文件.
		/// </summary>
		/// <param name="iFromDir">拷贝源目录.</param>
		/// <param name="iToDir">拷贝目标目录.</param>
		private void CopyAllFiles(string iFromDir, string iToDir) {

			if (false == Directory.Exists (iToDir)) {
				Directory.CreateDirectory (iToDir);
			}

			// 源目录下的文件
			var files = Directory.GetFiles (iFromDir);
			foreach (var file in files) {

				if (file.EndsWith (".meta")) {
					continue;
				}
				if (file.EndsWith (".DS_Store")) {
					continue;
				}
				if (file.EndsWith ("AndroidManifest.xml")) {
					continue;
				}

				var lastIndex = file.LastIndexOf ("/", StringComparison.Ordinal);
				var fileName = file.Substring (lastIndex + 1);
				if (string.IsNullOrEmpty (fileName)) {
					continue;
				}

				var copyToFile = $"{iToDir}/{fileName}";
				if (File.Exists (copyToFile)) {
					File.Delete (copyToFile);
				}
				this.Info("CopyAllFiles()::Copy Libs : {0} -> {1}",
					file, copyToFile);

				File.Copy (file, copyToFile);
			}

			// 源目录下的子文件夹
			var dirs = Directory.GetDirectories(iFromDir);
			foreach (var dir in dirs) {
				var dirInfo = new DirectoryInfo (dir);
				CopyAllFiles (dirInfo.FullName,
					$"{iToDir}/{dirInfo.Name}");
			}
		}

#endregion

	}
}

#endif
