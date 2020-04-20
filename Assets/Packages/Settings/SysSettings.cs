using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using Packages.AssetBundles;
using Packages.Common;
using Packages.Common.Base;
using Packages.Common.Extend;
using Packages.SE;
using Packages.Utils;
using Packages.Logs;

namespace Packages.Settings
{
	
	/// <summary>
	/// BuildMode.
	/// </summary>
	public enum BuildMode {
		/// <summary>
		/// Debug模式.
		/// </summary>
		Debug,

		/// <summary>
		/// Release模式.
		/// </summary>
		Release,
		
		/// <summary>
		/// Release模式(Runing)
		///     注意：目前仅iOS下有该模式
		/// </summary>
		ReleaseForRunning,
		
		/// <summary>
		/// Release模式(Profiling)
		///     注意：目前仅iOS下有该模式
		/// </summary>
		ReleaseForProfiling,

		/// <summary>
		/// 生产模式.
		/// </summary>
		Production,
		
		/// <summary>
		/// 最大。(计数用)
		/// </summary>
		Max
	}

	/// <summary>
	/// 临时情报.
	///    备注：
	///        在这里存储的信息只是临时用。每次App启动都有可能被重置/初始化。
	///        可以保证的只有当且仅当当前启动时，值有效。
	/// </summary>
	[System.Serializable]
	public class TempInfo : JsonDataBase<TempInfo> {
		
		/// <summary>
		/// 打包No（Teamcity/Jenkins上的打包no）
		/// </summary>
		public int buildNumber = -1;

		/// <summary>
		/// 清空.
		/// </summary>
		public override void Clear() {
			buildNumber = -1;
		}
	}
	
	/// <summary>
	/// 设定信息 - 一般
	/// </summary>
	[System.Serializable]
	public partial class SysGeneralData : JsonDataBase<SysGeneralData>
	{
		
		/// <summary>
		/// BuildMode.
		/// </summary>
		public BuildMode buildMode;

		/// <summary>
		/// 打包名.
		/// </summary>
		public string buildName;

		/// <summary>
		/// ID.
		/// </summary>
		public string buildId;

		/// <summary>
		/// 版本号.
		/// </summary>
		public string buildVersion;

		/// <summary>
		/// 版本号(Short).
		/// </summary>
		public string buildShortVersion;

		/// <summary>
		/// 版本Code
		/// </summary>
		public int buildVersionCode;

		/// <summary>
		/// 资源号.
		/// </summary>
		public int resourceNo;

		/// <summary>
		/// Android SDK(只有打包Android的时候 才有效).
		/// </summary>
		public PlatformType platformType;
		
		/// <summary>
		/// 语言
		/// </summary>
		public SystemLanguage language = SystemLanguage.ChineseSimplified;

		/// <summary>
		/// 每秒传输帧数(单位：帧／秒。默认60帧／秒).
		/// </summary>
		public int fps;

		/// <summary>
		/// 睡眠超时(单位：秒。默认30秒).
		/// </summary>
		public int sleepTimeOut;

		/// <summary>
		/// 临时信息.
		/// </summary>
		public TempInfo tempInfo = new TempInfo();

		private ValueChangerHandler _onAppVersionInfoChanged = () => {};
		// App版本信息变动句柄
		public event ValueChangerHandler AppVersionInfoChanged {
			add { _onAppVersionInfoChanged += value; } remove { _onAppVersionInfoChanged -= value; }
		}

		/// <summary>
		/// 清空.
		/// </summary>
		public override void Clear() {
			base.Clear ();

			buildMode = BuildMode.Debug;
			buildName = null;
			buildId = null;
			buildVersion = null;
			buildShortVersion = null;
			buildVersionCode = -1;
			resourceNo = -1;
			platformType = PlatformType.None;
			language = SystemLanguage.ChineseSimplified;
			fps = 60;
			sleepTimeOut = SleepTimeout.NeverSleep;
			tempInfo.Clear ();
		}
	}

	/// <summary>
	/// 设定信息 - 选项
	/// </summary>
	[System.Serializable]
	public partial class SysOptionsData : OptionsBaseData
	{
		
	}

#if UNITY_EDITOR
	
	/// <summary>
	/// 资源服务器信息.
	/// </summary>
	[System.Serializable]
	public class ResourcesServerData : JsonDataBase<ResourcesServerData>
	{
		
		/// <summary>
		/// 服务器登陆用账户ID/用户名.
		/// </summary>
		public string accountId;

		/// <summary>
		/// 服务器登陆用密码.
		/// </summary>
		public string pwd;
		
		/// <summary>
		/// 服务器地址（Ip地址）.
		/// </summary>
		public string ipAddresss;

		/// <summary>
		/// 端口号.
		/// </summary>
		public int portNo;

		/// <summary>
		/// 上传根目录.
		/// </summary>
		public string rootDir;

		/// <summary>
		/// 服务器禁用标志位.
		/// </summary>
		public bool disable;

		/// <summary>
		/// 清空.
		/// </summary>
		public override void Clear() {
			accountId = null;
			pwd = null;
			ipAddresss = null;
			portNo = -1;
			rootDir = null;
			disable = false;
		}
	}
	
	/// <summary>
	/// CND服务器信息.
	/// </summary>
	[System.Serializable]
	public class CdnServerInfo : JsonDataBase<CdnServerInfo> {

		/// <summary>
		/// 用户名
		/// </summary>
		public string userName;

		/// <summary>
		/// APIKey
		/// </summary>
		public string apiKey;

		/// <summary>
		/// 刷新的CDN缓存地址
		/// </summary>
		public string cdnUrl;

		/// <summary>
		/// 刷新的关联地址
		/// </summary>
		public List<string> refreshUrls = new List<string>();

		/// <summary>
		/// 清空.
		/// </summary>
		public override void Clear() {
			base.Clear ();
			userName = null;
			apiKey = null;
			cdnUrl = null;
			refreshUrls.Clear();
		}
	}
	
#endif
	
	/// <summary>
	/// Web服务器信息
	/// </summary>
	[System.Serializable]
	public class WebServer : JsonDataBase<WebServer> {

		/// <summary>
		/// Host
		/// </summary>
		public string host;

		/// <summary>
		/// 端口号
		/// </summary>
		public int portNo = -1;

		/// <summary>
		/// Http / Htpps协议
		/// </summary>
		public bool https;

		/// <summary>
		/// Http Host 地址
		/// </summary>
		public string HostUrl =>
			https
				? $"https://{host}:{portNo}"
				: $"http://{host}:{portNo}";


		/// <summary>
		/// 清空.
		/// </summary>
		public override void Clear() {
			base.Clear();

			host = null;
			portNo = -1;
		}
	}
	
	/// <summary>
	/// 设定信息 - 网络
	/// </summary>
	[System.Serializable]
	public class NetworksData : JsonDataBase<NetworksData>
	{
		/// <summary>
		/// 线程最大并发数
		/// </summary>
		public int ThreadMaxCount;

		/// <summary>
		/// 通信失败后重发次数
		/// </summary>
		public int Retries;

		/// <summary>
		/// 超时时间（单位：秒）
		/// </summary>
		public int Timeout;

		/// <summary>
		/// 跳过下载标识位
		/// </summary>
		public bool SkipDownload;
		
		/// <summary>
		/// 下载根地址
		/// </summary>
		public string downloadRootUrl;

		/// <summary>
		/// http or https?
		/// </summary>
		public bool https;

		/// <summary>
		/// 下载地址
		/// </summary>
		public string DownloadUrl
		{
			get
			{
				return https
					? $"https://{downloadRootUrl}"
					: $"http://{downloadRootUrl}";
			}
		}
		
		/// <summary>
		/// Web Host
		/// </summary>
		public string WebHost
		{
			get
			{
				return webServer?.host;
			}
			set
			{
				if (null != webServer)
				{
					webServer.host = value; 
				}
			}
		}
		
		/// <summary>
		/// Web Host
		/// </summary>
		public int WebPortNo
		{
			get
			{
				if (null != webServer)
				{
					return webServer.portNo;
				}
				return -1;
			}
			set
			{
				if (null != webServer)
				{
					webServer.portNo = value; 
				}
			}
		}

		/// <summary>
		/// Web地址
		/// </summary>
		public string WebUrl
		{
			get { return webServer?.HostUrl; }
		}

#if UNITY_EDITOR

		/// <summary>
		/// 资源服务器
		/// </summary> 
		public ResourcesServerData resourceServer = new ResourcesServerData();
		
		/// <summary>
		/// CDN服务器
		/// </summary>
		public CdnServerInfo cdnServer = new CdnServerInfo();
#endif
		
		/// <summary>
		/// Web服务器信息
		/// </summary>
		public WebServer webServer = new WebServer();
		
		/// <summary>
		/// 清空.
		/// </summary>
		public override void Clear()
		{
			ThreadMaxCount = 10;
			Retries = 3;
			Timeout = 30;
			SkipDownload = false;
			
#if UNITY_EDITOR
			resourceServer.Clear();
			cdnServer.Clear();
#endif
			webServer.Clear();
		}
	}
	
	/// <summary>
	/// ProgressTips.
	/// </summary>
	[System.Serializable]
	public class ProgressTips : JsonDataBase<ProgressTips> {
		
		/// <summary>
		/// 语言包标志位.
		/// </summary>
		public bool languagePackage;

		/// <summary>
		/// 更新间隔时间(单位：秒).
		/// </summary>
		public int interval = 3;

		/// <summary>
		/// 下载中提示信息包.
		/// </summary>
		public List<string> tips = new List<string> ();

		/// <summary>
		/// 初始化.
		/// </summary>
		public override void Init() {
			languagePackage = false;
			interval = 3;
			tips?.Clear();
		}
	}

	/// <summary>
	/// 设定信息
	/// </summary>
	[System.Serializable]
	public partial class SysSettingsData : OptionsDataBase<SysGeneralData, SysOptionsData>
	{
		/// <summary>
		/// 设定信息 - 音效
		/// </summary>
		public List<SeData> se = new List<SeData>();
		
		/// <summary>
		/// 设定信息 - 网络
		/// </summary>
		public NetworksData network = new NetworksData();
		
		/// <summary>
		/// 进度信息用的Tips
		/// 备注：可用于进度条/下载条等刷新信息
		/// </summary>
		public ProgressTips tips = new ProgressTips();

		/// <summary>
		/// 清空
		/// </summary>
		public override void Clear()
		{
			se.Clear();
			network.Clear();
			tips.Clear();
		}
	}

	/// <summary>
	/// 系统设定信息
	/// </summary>
	[System.Serializable]
	public class SysSettings : AssetOptionsBase<SysSettings, SysSettingsData, SysGeneralData, SysOptionsData> {

		private const string _PLAYERPREFS_KEY_IS_SKIP_DOWNLOAD = "__is_skip_download__";
		
		/// <summary>
		/// 跳过下载标识位.
		/// </summary>
		public static bool IsSkipDownLoad
		{
			get => PlayerPrefs.GetInt(_PLAYERPREFS_KEY_IS_SKIP_DOWNLOAD, 0) == 1;
			set
			{
				PlayerPrefs.SetInt(_PLAYERPREFS_KEY_IS_SKIP_DOWNLOAD, value ? 1 : 0) ;
			}
		}
		
		/// <summary>
		/// BuildMode.
		/// </summary>
		public BuildMode BuildMode {
			get => data?.General.buildMode ?? BuildMode.Debug;
			set { 
				if (null != data) {
					data.General.buildMode = value;
				}
			}
		}

		/// <summary>
		/// 打包名.
		/// </summary>
		public string BuildName {
			get => data?.General.buildName;
			set { 
				if (null != data) {
					data.General.buildName = value;
				}
			}
		}

		/// <summary>
		/// ID.
		/// </summary>
		public string BuildId {
			get => data?.General.buildId;
			set {  
				if (null != data) {
					data.General.buildId = value;
				}
			}
		}

		/// <summary>
		/// 版本号.
		/// </summary>
		public string BuildVersion {
			get => data?.General.buildVersion;
			set { 
				if (null != data) {
					data.General.buildVersion = value;
				}
			}
		}

		/// <summary>
		/// 版本号(Short).
		/// </summary>
		public string BuildShortVersion
		{
			get { return data?.General.buildShortVersion; }
		}

		/// <summary>
		/// 版本Code
		/// </summary>
		public int BuildVersionCode {
			get => data?.General.buildVersionCode ?? -1;
			set { 
				if (null != data) {
					data.General.buildVersionCode = value;
				}
			}
		}

		/// <summary>
		/// 资源号.
		/// </summary>
		public int ResourceNo {
			get => data?.General.resourceNo ?? -1;
			set {
				if (null == data) return;
				data.General.resourceNo = value;
//				data.General.OnAppVersionInfoChanged();
			}
		}

		/// <summary>
		/// 日志等级.
		/// </summary>
		public LogLevel LogLevel {
			get => Loger.LogLevel;
			set => Loger.LogLevel = value;
		}

		/// <summary>
		/// 语言.
		/// </summary>
		public SystemLanguage Language {
			get => data?.General.language ?? SystemLanguage.English;
			set { 
				if (null != data) {
					data.General.language = value;
				}
			}
		}

		/// <summary>
		/// 平台类型.
		/// </summary>
		public PlatformType PlatformType {
			get => data?.General.platformType ?? PlatformType.None;
			set { 
				if (null != data) {
					data.General.platformType = value;
				}
			}
		}

		/// <summary>
		/// 睡眠时间.
		/// </summary>
		public int SleepTimeout
		{
			get
			{
				if (null == data)
				{
					return -1;
				}
				return data.General.sleepTimeOut;
			}
		}

		/// <summary>
		/// 每秒传输帧数(单位：帧／秒).
		/// </summary>
		public int Fps
		{
			get => data?.General.fps ?? 60;
			set => data.General.fps = value;
		}

		/// <summary>
		/// 打包号（Teamcity等CI上生成的打包号）.
		/// </summary>
		public int BuildNumber {
			get
			{
				if (data != null) return data.General.tempInfo.buildNumber;
				return -1;
			}
			set { 
				if (data?.General.tempInfo != null) {
					data.General.tempInfo.buildNumber = value;
				}

				if (null != data)
				{
					data.General.resourceNo = value;
				}
			}
		}
		/// <summary>
		/// 下载跟目录.
		/// </summary>
		public static string DownloadRootDir {
			get; private set;
		}

		/// <summary>
		/// 下载目录.
		/// </summary>
		public static string DownloadDir {
			get; private set;
		}

		/// <summary>
		/// 下载目录(Normal).
		/// </summary>
		public static string DownloadDirOfNormal {
			get; private set;
		}

		/// <summary>
		/// 下载目录(Scenes).
		/// </summary>
		public static string DownloadDirOfScenes {
			get; private set;
		}

		/// <summary>
		/// Bundles目录.
		/// </summary>
		public static string BundlesDir {
			get; private set;
		}

		/// <summary>
		/// Bundles目录(Normal).
		/// </summary>
		public static string BundlesDirOfNormal {
			get; private set;
		}

		/// <summary>
		/// Bundles目录(Scenes).
		/// </summary>
		public static string BundlesDirOfScenes {
			get; private set;
		}

		/// <summary>
		/// 解压缩目录.
		/// </summary>
		public static string DecompressedDir {
			get; private set;
		}

		/// <summary>
		/// 解压缩目录(Normal).
		/// </summary>
		public static string DecompressedDirOfNormal {
			get; private set;
		}

		/// <summary>
		/// 解压缩目录(Scenes).
		/// </summary>
		public static string DecompressedDirOfScenes {
			get; private set;
		}
		
#region ProgressTips

		/// <summary>
		/// 随机取得一个ProgressTip.
		/// </summary>
		/// <returns>ProgressTip.</returns>
		public string GetProgressTipByRandom() {

			if (data.tips?.tips == null || data.tips.tips.Count <= 0) {
				return null;
			}

			const int minValue = 0;
			var maxValue = data.tips.tips.Count - 1;
			var random = new System.Random();
			var value = random.Next(minValue, maxValue);
			return data.tips.tips[value];
		}

#endregion

#region Implement

		/// <summary>
		/// 初始化数据.
		/// </summary>
		/// <returns><c>true</c>, OK, <c>false</c> NG.</returns>
		protected override bool InitAsset()
		{

			// 打包ID
			if(string.IsNullOrEmpty(data.General.buildId) == false) {

#if UNITY_EDITOR
#if UNITY_5_5_OR_NEWER
				PlayerSettings.applicationIdentifier = data.General.buildId;
#else
				PlayerSettings.bundleIdentifier = data.General.buildId;
#endif
#endif
			}

			// 版本号
			if(string.IsNullOrEmpty(data.General.buildVersion) == false) {
#if UNITY_EDITOR
				PlayerSettings.bundleVersion = data.General.buildVersion;
#endif
			}

			// 版本号
			if (-1 != data.General.buildVersionCode)
			{

#if UNITY_IOS && UNITY_EDITOR
				PlayerSettings.iOS.buildNumber = BuildVersionCode.ToString();
#endif
#if UNITY_ANDROID && UNITY_EDITOR
				PlayerSettings.Android.bundleVersionCode = data.General.buildVersionCode;
#endif
			}
			
			DownloadRootDir = Application.temporaryCachePath;
			this.Info("InitAsset()::DownloadRootDir:{0}", DownloadRootDir);

			// 下载目录
			DownloadDir = $"{DownloadRootDir}/Downloads";
			this.Info("InitAsset()::DownloadDir:{0}", DownloadDir);

			// 下载目录(Normal)
			DownloadDirOfNormal = $"{DownloadDir}/{BundlesResult.AssetBundleDirNameOfNormal}";
			this.Info("InitAsset()::DownloadDirOfNormal:{0}", DownloadDirOfNormal);

			// 下载目录(Scenes)
			DownloadDirOfScenes = $"{DownloadDir}/{BundlesResult.AssetBundleDirNameOfScenes}";
			this.Info("InitAsset()::DownloadDirOfScenes:{0}", DownloadDirOfScenes);

			// Bundles目录
			BundlesDir = $"{Application.persistentDataPath}/Bundles";
			this.Info("InitAsset()::BundlesDir:{0}", BundlesDir);

			// Bundles目录(Normal)
			BundlesDirOfNormal = $"{BundlesDir}/{BundlesResult.AssetBundleDirNameOfNormal}";
			this.Info("InitAsset()::BundlesDirOfNormal:{0}", BundlesDirOfNormal);

			// Scene
			BundlesDirOfScenes = $"{BundlesDir}/{BundlesResult.AssetBundleDirNameOfScenes}";
			this.Info("InitAsset()::BundlesDirOfScenes:{0}", BundlesDirOfScenes);

			// 解压缩
			DecompressedDir = $"{DownloadRootDir}/Decompressed";
			this.Info("InitAsset()::DecompressedDir:{0}", DecompressedDir);

			// 解压缩(Normal)
			DecompressedDirOfNormal =
				$"{DecompressedDir}/{BundlesResult.AssetBundleDirNameOfNormal}";
			this.Info("InitAsset()::DecompressedDirOfNormal:{0}", DecompressedDirOfNormal);

			// 解压缩(Scenes)
			DecompressedDirOfScenes =
				$"{DecompressedDir}/{BundlesResult.AssetBundleDirNameOfScenes}";
			this.Info("InitAsset()::DecompressedDirOfScenes:{0}", DecompressedDirOfScenes);

			UtilsAsset.SetAssetDirty (this);

			this.Info ("Data : {0}", data.ToString());

			return base.InitAsset();
		}

		/// <summary>
		/// 用用数据.
		/// </summary>
		/// <param name="iData">数据.</param>
		/// <param name="iForceClear">强制清空标志位.</param>
		protected override void ApplyData (SysSettingsData iData, bool iForceClear = true) {
			
			if (null == iData) {
				return;
			}

			// 清空
			if (iForceClear) {
				Clear ();
			}

			// 默认数据
			data.General.buildName = iData.General.buildName;
			data.General.buildVersion = iData.General.buildName;
			data.General.buildId = iData.General.buildId;
			data.General.buildVersion = iData.General.buildVersion;
			data.General.buildShortVersion = iData.General.buildShortVersion;
			data.General.buildVersionCode = iData.General.buildVersionCode;
			data.General.platformType = iData.General.platformType;
			data.General.fps = iData.General.fps;
			data.General.sleepTimeOut = iData.General.sleepTimeOut;

			data.General.tempInfo = iData.General.tempInfo;

			// 选项数据
			data.Options.data = iData.Options.data;
			
			// 音效数据
			data.se = iData.se;
			// 网络设定数据
			data.network = iData.network;
			// Tips
			data.tips = iData.tips;
		
		}
		
#endregion
	}
}
