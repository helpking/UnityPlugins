using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using BuildSystem;
using Common;
using AssetBundles;
using Download;

namespace Upload {

	/// <summary>
	/// 服务器ID.
	/// </summary>
	public enum TServerID {
		Invalid = -1,
		/// <summary>
		/// 默认ID.
		/// </summary>
		Default,
		Max
	}

	/// <summary>
	/// 下载方式.
	/// </summary>
	public enum TDownloadWay {
		/// <summary>
		/// 无.
		/// </summary>
		None,
		/// <summary>
		/// WWW.
		/// </summary>
		WWW,
		/// <summary>
		/// Http.
		/// </summary>
		Http
	}

	/// <summary>
	/// ProgressTips.
	/// </summary>
	[System.Serializable]
	public class ProgressTips {
		
		/// <summary>
		/// 语言包标志位.
		/// </summary>
		public bool LanguagePakage = false;

		/// <summary>
		/// 更新间隔时间(单位：秒).
		/// </summary>
		public int Interval = 3;

		/// <summary>
		/// 下载中提示信息包.
		/// </summary>
		public List<string> Tips = new List<string> ();

		public void Clear() {
			LanguagePakage = false;
			Interval = 3;
			if (Tips != null) {
				Tips.Clear ();
			}
		}
	}

	/// <summary>
	/// 上传服务器信息.
	/// </summary>
	[System.Serializable]
	public class UploadServerInfo
	{

		/// <summary>
		/// 服务器ID.
		/// </summary>
		public TServerID ID = TServerID.Default;

		/// <summary>
		/// 服务器登陆用账户ID/用户名.
		/// </summary>
		public string AccountId = null;

		/// <summary>
		/// 服务器登陆用密码.
		/// </summary>
		public string Pwd = null;
		
		/// <summary>
		/// 服务器地址（Ip地址）.
		/// </summary>
		public string IpAddresss = null;

		/// <summary>
		/// 端口号.
		/// </summary>
		public string PortNo = null;

		/// <summary>
		/// 服务器禁用标志位.
		/// </summary>
		public bool Disable = false;

	}

	/// <summary>
	/// 下载服务器信息.
	/// </summary>
	[System.Serializable]
	public class ServerDirInfo {
		/// <summary>
		/// 服务器ID.
		/// </summary>
		public TServerID ID = TServerID.Default;

		/// <summary>
		/// 文件夹列表.
		/// </summary>
		public List<string> Dirs = new List<string> ();
	}

	/// <summary>
	/// 下载服务器信息.
	/// </summary>
	[System.Serializable]
	public class DownloadServerInfo
	{
		/// <summary>
		/// 服务器ID.
		/// </summary>
		public TServerID ID = TServerID.Default;

		/// <summary>
		/// 下载地址.
		/// </summary>
		public string Url = null;

		/// <summary>
		/// 服务器禁用标志位.
		/// </summary>
		public bool Disable = false;

	}

	/// <summary>
	/// 服务器情报.
	/// </summary>
	public class ServersConf : AssetBase {

		/// <summary>
		/// 子线程最大数.
		/// </summary>
		[SerializeField]public int ThreadMaxCount = 3;

		/// <summary>
		/// 因为网络失败或者错误等。重试次数.
		/// </summary>
		[SerializeField]public int NetRetries = 3;

		/// <summary>
		/// 网络超时时间（单位：秒）
		/// </summary>
		[SerializeField]public int NetTimeOut = 30;

		/// <summary>
		/// 下载方式.
		/// </summary>
		[SerializeField]public TDownloadWay DownloadWay = TDownloadWay.Http;

		/// <summary>
		/// 跳过下载标志位.
		/// </summary>
		[SerializeField]public bool SkipDownload = false;

		/// <summary>
		/// 上传服务器.
		/// </summary>
		[SerializeField]public UploadServerInfo UploadServer = new UploadServerInfo ();

		/// <summary>
		/// 下载服务器.
		/// </summary>
		[SerializeField]public DownloadServerInfo DownloadServer = new DownloadServerInfo ();

		/// <summary>
		/// ProgressTips.
		/// </summary>
		[SerializeField]public ProgressTips ProgressTips = new ProgressTips();

		/// <summary>
		/// 服务器文件列表信息.
		/// </summary>
		[SerializeField]public List<ServerDirInfo> ServersDirs = new List<ServerDirInfo>();
		private static object _serversDirsLock = new object();

		/// <summary>
		/// 下载跟目录.
		/// </summary>
		public string DownloadRootDir {
			get; private set;
		}

		/// <summary>
		/// 下载目录.
		/// </summary>
		public string DownloadDir {
			get; private set;
		}

		/// <summary>
		/// 下载Bundles目录.
		/// </summary>
		public string BundlesDir {
			get; private set;
		}

		/// <summary>
		/// 下载Bundles目录.
		/// </summary>
		public string BundlesDirOfNormal {
			get; private set;
		}

		/// <summary>
		/// 下载Bundles目录.
		/// </summary>
		public string BundlesDirOfScene {
			get; private set;
		}

		/// <summary>
		/// 压缩/解压缩目录.
		/// </summary>
		public string CompressDir {
			get; private set;
		}

		/// <summary>
		/// 实例.
		/// </summary>
		private static ServersConf _instance = null;

		/// <summary>
		/// 取得实例.
		/// </summary>
		/// <returns>实例.</returns>
		public static ServersConf GetInstance() {
			if (_instance == null) {
				_instance = UtilityAsset.Read<ServersConf>();
				if (_instance == null) {
					Debug.LogError ("BundlesMap GetInstance Failed!!!");
					return null;
				} 
				_instance.Init ();
			}
			return _instance;
		}

		/// <summary>
		/// 判断上传服务器信息是否有效.
		/// </summary>
		/// <returns><c>true</c>, 有效, <c>false</c> 无效.</returns>
		/// <param name="iServer">上传服务器信息.</param>
		private bool isUploadServerValid(UploadServerInfo iServer) {
		
			// 禁用该服务器
			if (iServer.Disable == true) {
				return false;
			}

			// 服务器ID无效
			if ((TServerID.Invalid >= iServer.ID) ||
			   (TServerID.Max <= iServer.ID)) {
				return false;
			}

			return true;
		}

		/// <summary>
		/// 判断下载服务器信息是否有效.
		/// </summary>
		/// <returns><c>true</c>, 有效, <c>false</c> 无效.</returns>
		/// <param name="iServer">下载服务器信息.</param>
		private bool isDownloadServerValid(DownloadServerInfo iServer) {

			// 禁用该服务器
			if (iServer.Disable == true) {
				return false;
			}

			// 服务器ID无效
			if ((TServerID.Invalid >= iServer.ID) ||
				(TServerID.Max <= iServer.ID)) {
				return false;
			}

			return true;
		}

		/// <summary>
		/// 取得下载服务器信息.
		/// </summary>
		/// <returns>下载服务器信息.</returns>
		public UploadServerInfo GetUploadServerInfo() {
			if (isUploadServerValid (this.UploadServer) == false) {
				return null;
			}
			return this.UploadServer;
		}

		/// <summary>
		/// 取得下载服务器信息.
		/// </summary>
		/// <returns>下载服务器信息.</returns>
		public DownloadServerInfo GetDownloadServerInfo() {
			if (isDownloadServerValid (this.DownloadServer) == false) {
				return null;
			}
			return this.DownloadServer;
		}

		/// <summary>
		/// 取得上传服务器基础地址.
		/// 基础地址:<服务器地址>:<端口号>
		/// </summary>
		/// <returns>上传服务器基础地址.</returns>
		/// <param name="iServerInfo">上传服务器信息.</param>
		public static string GetUploadServerPostBaseURL (
			UploadServerInfo iServerInfo) {
			if (string.IsNullOrEmpty (iServerInfo.PortNo) == true) {
				return iServerInfo.IpAddresss;
			} else {
				return string.Format ("{0}:{1}",
					iServerInfo.IpAddresss, iServerInfo.PortNo);
			}
		}

		/// <summary>
		/// 取得下载服务器基础地址.
		/// 基础地址:<服务器地址>:<端口号>
		/// </summary>
		/// <returns>下载服务器基础地址.</returns>
		/// <param name="iServerInfo">下载服务器信息.</param>
		public static string GetDwonloadServerPostBaseURL (
			DownloadServerInfo iServerInfo) {
			if (string.IsNullOrEmpty (iServerInfo.Url) == true) {
				return null;
			} else {
				return iServerInfo.Url;
			}
		}

		/// <summary>
		/// 取得上传服务器传输用URL.
		/// <服务器地址>:<端口号>/<工程名（例：NFF）>
		/// </summary>
		/// <returns>服务器传输地址.</returns>
		/// <param name="iServerInfo">上传服务器信息.</param>
		private static string GetUploadServerPostURL(
			UploadServerInfo iServerInfo) {

			string serverPostUrl = GetUploadServerPostBaseURL (iServerInfo);
			serverPostUrl = string.Format ("{0}/{1}",
				serverPostUrl, BuildInfo.GetInstance ().BuildName);

			// Debug.LogFormat ("ServerPostURL:{0}", serverPostUrl);

			return serverPostUrl;
		}

		/// <summary>
		/// 取得下载服务器传输用URL.
		/// <服务器地址Url>/<工程名（例：NFF）>
		/// </summary>
		/// <returns>下载服务器传输地址.</returns>
		/// <param name="iServerInfo">下载服务器信息.</param>
		private static string GetDownloadServerPostURL(
			DownloadServerInfo iServerInfo) {

			string serverPostUrl = GetDwonloadServerPostBaseURL (iServerInfo);
			if (string.IsNullOrEmpty (serverPostUrl) == true) {
				return null;
			} else {
				return string.Format ("{0}/{1}",
					serverPostUrl, BuildInfo.GetInstance ().BuildName);
			}
		}

		#region Upload Info

		/// <summary>
		/// 取得上传列表文件Base URL.
		/// </summary>
		/// <returns>上传列表文件Base URL.</returns>
		/// <param name="iServerInfo">上传服务器信息.</param>
		public static string GetUploadListBaseUrl(UploadServerInfo iServerInfo) {
			string serverPostUrl = GetUploadServerPostURL (iServerInfo);
			string uploadBaseUrl = string.Format ("ftp://{0}", serverPostUrl);
			return uploadBaseUrl;
		}

		/// <summary>
		/// 取得上传资源用URL(Ftp格式).
		/// <服务器地址>:<端口号></c>/<工程名（例：NFF）>/<上传时间>
		/// </summary>
		/// <returns>上传资源用URL(Ftp格式).</returns>
		/// <param name="iServerInfo">上传服务器信息.</param>
		public static string GetUploadBaseURL(UploadServerInfo iServerInfo) {
			string serverPostUrl = GetUploadServerPostURL (iServerInfo);
			string uploadBaseUrl = string.Format ("ftp://{0}", serverPostUrl);
			// Debug.LogFormat ("UploadBaseUrl:{0}", uploadBaseUrl);
			return uploadBaseUrl;
		}

		/// <summary>
		/// 取得Bundle的上传地址.
		/// 上传地址：<上传资源用URL>/<BuildMode>/<UploadDateTime>
		/// </summary>
		/// <returns>Bundle的上传地址.</returns>
		/// <param name="iServerInfo">上传服务器信息.</param>
		/// <param name="iUploadInfo">上传信息.</param>
		public static string GetBundleUploadBaseURL(
			UploadServerInfo iServerInfo, UploadItem iUploadInfo) {

			string uploadBaseUrl = GetUploadBaseURL (iServerInfo);

			string bundleUploadUrl = string.Format ("{0}/{1}", 
				uploadBaseUrl, 
				UploadList.GetBundleRelativePath(iUploadInfo.BuildMode, iUploadInfo.UploadDateTime,
					(TBundleType.Scene == iUploadInfo.BundleType)));
			
			// Debug.LogFormat ("BundleUploadUrl:{0}", bundleUploadUrl);
			return bundleUploadUrl;
		}

		#endregion

		#region Download Info 

		/// <summary>
		/// 取得下载资源用URL(Ftp格式).
		/// </summary>
		/// <returns>下载资源用URL(Ftp格式).</returns>
		/// <param name="iServerInfo">下载服务器信息.</param>
		public static string GetDownloadBaseURL(
			DownloadServerInfo iServerInfo) {
			string serverPostUrl = GetDownloadServerPostURL (iServerInfo);
			string downloadBaseUrl = string.Format ("http://{0}", serverPostUrl);
			// Debug.LogFormat ("DownloadBaseUrl:{0}", downloadBaseUrl);
			return downloadBaseUrl;
		}

		/// <summary>
		/// 取得Bundle的下载地址.
		/// 下载地址：<下载资源用URL>/<BundleID>/<BundleVersion>/<BundleFullName>
		/// </summary>
		/// <returns>Bundle的下载地址.</returns>
		/// <param name="iDownloadInfo">下载信息.</param>
		public static string GetBundleDownloadBaseURL(
			DownloadTargetInfo iDownloadInfo) {

			DownloadServerInfo dlServerInfo = ServersConf.GetInstance ().GetDownloadServerInfo ();
			if (dlServerInfo == null) {
				return null;
			}
			string downloadBaseUrl = GetDownloadBaseURL (dlServerInfo);

			string bundleDownloadUrl = string.Format ("{0}/{1}", 
				downloadBaseUrl, 
				UploadList.GetBundleRelativePath(
					BuildInfo.GetInstance().BuildMode, iDownloadInfo.UploadDateTime, (TBundleType.Scene == iDownloadInfo.BundleType)));

			// Debug.LogFormat ("BundleDownloadUrl:{0}", bundleDownloadUrl);
			return bundleDownloadUrl;
		}

		/// <summary>
		/// 取得Bundle包依赖文件下载URL.
		/// </summary>
		/// <returns>Bundle包依赖文件下载URL.</returns>
		public string GetDownloadUrlOfBundlesMap() {

			DownloadServerInfo serverInfo = ServersConf.GetInstance ().GetDownloadServerInfo ();
			string downloadBaseUrl = ServersConf.GetDownloadBaseURL (serverInfo);

			string JsonFileFullPath = UtilityAsset.GetJsonFilePath<BundlesMap>();
			int lastIndex = JsonFileFullPath.LastIndexOf ("/");
			string JsonFileName = JsonFileFullPath.Substring (lastIndex+1);

			return string.Format ("{0}/{1}", downloadBaseUrl, JsonFileName);
		}

		/// <summary>
		/// 取得上传列表文件的下载URL.
		/// </summary>
		/// <returns>BundlesInfo的下载URL.</returns>
		public string GetDownloadUrlOfUploadList() {

			DownloadServerInfo serverInfo = ServersConf.GetInstance ().GetDownloadServerInfo ();
			string downloadBaseUrl = ServersConf.GetDownloadBaseURL (serverInfo);

			string JsonFileFullPath = UtilityAsset.GetJsonFilePath<UploadList>();
			int lastIndex = JsonFileFullPath.LastIndexOf ("/");
			string JsonFileName = JsonFileFullPath.Substring (lastIndex+1);

			return string.Format ("{0}/{1}", downloadBaseUrl, JsonFileName);
		}

		#endregion

		#region Dir of Server

		/// <summary>
		/// 文件夹是否已经在服务器上创建了
		/// </summary>
		/// <returns><c>true</c>, 已经创建, <c>false</c> 尚未创建.</returns>
		/// <param name="iServerId">服务器ID.</param>
		/// <param name="iDir">目录.</param>
		public bool isDirCreatedOnServer(TServerID iServerId, string iDir) {

			ServerDirInfo[] servers = this.ServersDirs
				.Where (o => (iServerId == o.ID))
				.ToArray ();
			if ((servers == null) || (servers.Length != 1)) {
				return false;
			}

			string[] dirs = (servers [0]).Dirs.Where (o => (o.Equals (iDir) == true)).ToArray ();
			return ((dirs != null) && (dirs.Length > 0));
		}

		/// <summary>
		/// 添加已经创建目录.
		/// </summary>
		/// <param name="iServerId">服务器ID.</param>
		/// <param name="iDir">I dir.</param>
		public void AddCreatedDir(TServerID iServerId, string iDir) {

			// 线程安全锁
			lock (_serversDirsLock) {

				ServerDirInfo targetServer = null;
				ServerDirInfo[] servers = this.ServersDirs
					.Where (o => (iServerId == o.ID))
					.OrderBy(o => o.ID)
					.ToArray ();
				if ((servers == null) || (servers.Length <= 0)) {
					targetServer = new ServerDirInfo ();
					targetServer.ID = iServerId;
					this.ServersDirs.Add (targetServer);
				} else {
					if (servers.Length > 1) {
						Debug.LogErrorFormat ("There are multiple id exist!!![ID:{0}]", iServerId);
					}
					targetServer = servers [0];
				}

				string[] dirs = targetServer.Dirs.Where (o => (o.Equals (iDir) == true)).ToArray ();
				if ((dirs == null) || (dirs.Length <= 0)) {
					targetServer.Dirs.Add (iDir);
				}
			}

		}

		#endregion

		#region ProgressTips

		/// <summary>
		/// 随机取得一个ProgressTip.
		/// </summary>
		/// <returns>ProgressTip.</returns>
		public string GetProgressTipByRandom() {

			if ((this.ProgressTips == null) ||
				(this.ProgressTips.Tips == null) ||
				(this.ProgressTips.Tips.Count <= 0)) {
				return null;
			}

			int minValue = 0;
			int maxValue = this.ProgressTips.Tips.Count - 1;
			int value = Random.Range (minValue, maxValue);
			return this.ProgressTips.Tips[value];
		}

		#endregion

		public void ClearCreatedDir() {
			if (this.ServersDirs != null) {
				this.ServersDirs.Clear ();
			}
		}

		#region Implement

		/// <summary>
		/// 初始化.
		/// </summary>
		public override void Init () {
			
			this.DownloadRootDir = Application.temporaryCachePath;
			Debug.LogFormat ("DownloadRootDir:{0}", this.DownloadRootDir);

			// 下载目录
			this.DownloadDir = string.Format ("{0}/Downloads", this.DownloadRootDir);
			Debug.LogFormat ("DownloadDir:{0}", this.DownloadDir);

			// Bundles目录
			this.BundlesDir = string.Format ("{0}/Bundles", this.DownloadRootDir);
			Debug.LogFormat ("BundlesDir:{0}", this.BundlesDir);

			// Normal
			this.BundlesDirOfNormal = string.Format ("{0}/{1}", this.BundlesDir, 
				UploadList.GetInstance().AssetBundleDirNameOfNormal);
			Debug.LogFormat ("BundlesDirOfNormal:{0}", this.BundlesDirOfNormal);

			// Scene
			this.BundlesDirOfScene = string.Format ("{0}/{1}", this.BundlesDir, 
				UploadList.GetInstance().AssetBundleDirNameOfScenes);
			Debug.LogFormat ("BundlesDirOfScene:{0}", this.BundlesDirOfScene);

			// 压缩/解压缩
			this.CompressDir = string.Format ("{0}/Compress", this.DownloadRootDir);
			Debug.LogFormat ("CompressDir:{0}", this.CompressDir);

			UtilityAsset.SetAssetDirty (this);
		}

		/// <summary>
		/// 应用数据.
		/// </summary>
		/// <param name="iData">数据.</param>
		protected override void ApplyData(AssetBase iData) {
			if (iData == null) {
				return;
			}

			ServersConf data = iData as ServersConf;
			if (data == null) {
				return;
			}

			// 清空
			this.Clear ();
				
			this.ThreadMaxCount = data.ThreadMaxCount;
			this.NetRetries = data.NetRetries;
			this.NetTimeOut = data.NetTimeOut;
			this.UploadServer = data.UploadServer;
			this.DownloadServer = data.DownloadServer;
			this.ProgressTips = data.ProgressTips;
			this.ServersDirs = data.ServersDirs;

			UtilityAsset.SetAssetDirty (this);

		}

		/// <summary>
		/// 清空.
		/// </summary>
		public override void Clear() {

			UtilityAsset.Clear<ServersConf> ();

			this.ThreadMaxCount = 3;
			this.NetRetries = 3;
			this.NetTimeOut = 30;
			this.ServersDirs.Clear ();
			if (ProgressTips != null) {
				ProgressTips.Clear ();
			}

			UtilityAsset.SetAssetDirty (this);

		}

		/// <summary>
		/// 从JSON文件，导入打包配置信息.
		/// </summary>
		public override void ImportFromJsonFile() {			

			ServersConf jsonData = UtilityAsset.ImportFromJsonFile<ServersConf> ();
			if (jsonData != null) {
				this.ApplyData (jsonData);
			}
		}

		/// <summary>
		/// 导出成JSON文件.
		/// </summary>
		/// <returns>导出路径.</returns>
		public override string ExportToJsonFile() {
			return UtilityAsset.ExportToJsonFile<ServersConf> (this);
		}

		#endregion
	}
}