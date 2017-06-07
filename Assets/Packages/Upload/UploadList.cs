using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using LitJson;
using BuildSystem;
using Common;
using AssetBundles;
using Download;

namespace Upload {

	/// <summary>
	/// 压缩格式.
	/// </summary>
	public enum TCompressFormat {
		None,
		Zip,
		LZ4
	}

	/// <summary>
	/// 检测模式.
	/// </summary>
	public enum TCheckMode {
		/// <summary>
		/// Unity自带模式(hash128).
		/// </summary>
		Unity3d_Hash128,
		/// <summary>
		/// 自定义模式(Md5).
		/// </summary>
		Custom_Md5,
		/// <summary>
		/// 默认.
		/// </summary>
		Defult = Custom_Md5
	}

	/// <summary>
	/// 上传文件类型.
	/// </summary>
	public enum TUploadFileType {
		/// <summary>
		/// Main Manifest(StreamingAssets).
		/// </summary>
		MainManifest,
		/// <summary>
		/// Bundle.
		/// </summary>
		Bundle
	}

	/// <summary>
	/// 上传状态.
	/// </summary>
	public enum TUploadStatus {
		/// <summary>
		/// 无.
		/// </summary>
		None = 0x00000000,
		/// <summary>
		/// 本地保存.
		/// </summary>
		LocalSave = 0x00000001,
		/// <summary>
		/// Bundle上传完成.
		/// </summary>
		BundleUploaded = 0x00000002,
		/// <summary>
		/// Manifest上传完成.
		/// </summary>
		ManifestUploaded = 0x00000004,
		/// <summary>
		/// 全部上传完成.
		/// </summary>
		AllUploaded = (BundleUploaded | ManifestUploaded),
		/// <summary>
		/// 备份完成.
		/// </summary>
		Backuped = 0x00000008
	}

	/// <summary>
	/// 上传信息.
	/// </summary>
	[System.Serializable]
	public class UploadItem {

		/// <summary>
		/// bundle no.
		/// </summary>
		public int No = 0;

		/// <summary>
		/// ID.
		/// </summary>
		public string ID = null;

		/// <summary>
		/// bundle类型.
		/// </summary>
		public TBundleType BundleType = TBundleType.Normal;

		/// <summary>
		/// 上传文件类型.
		/// </summary>
		public TUploadFileType FileType = TUploadFileType.Bundle;

		/// <summary>
		/// 打包类型.
		/// </summary>
		public string BuildTarget = null;

		/// <summary>
		/// BuildMode.
		/// </summary>
		public TBuildMode BuildMode = TBuildMode.Debug;

		/// <summary>
		/// The market version.
		/// </summary>
		public string MarketVersion = null;

		/// <summary>
		/// App版本号.
		/// </summary>
		public string AppVersion = null;

		/// <summary>
		/// 数据大小.
		/// </summary>
		public string DataSize = null;

		/// <summary>
		/// 数据大小(manifest).
		/// </summary>
		public string ManifestDataSize = null;

		/// <summary>
		/// md5（只有在打包完成后才添加验证码）.
		/// </summary>
		public string Md5 = null;
		/// <summary>
		/// md5 of mainfest（只有在打包完成后才添加验证码）.
		/// </summary>
		public string ManifestMd5 = null;

		/// <summary>
		/// Unity3d系统打包后的Hash128检测码.
		/// </summary>
		public string HashCode = null;

		/// <summary>
		/// 上传时间（YYYYMMDDHHMM）.
		/// </summary>
		public long UploadDateTime = 0;

		/// <summary>
		/// 上传状态.
		/// </summary>
		public int Status = (int)TUploadStatus.None;

		/// <summary>
		/// 废弃标志位.
		/// </summary>
		/// <value><c>true</c> 废弃; 不废弃, <c>false</c>.</value>
		public bool Scraped = false;

		/// <summary>
		/// Bundle上传完成.
		/// </summary>
		/// <value><c>true</c> 完成; 未完成, <c>false</c>.</value>
		public bool isBundleUploaded {
			get { 
				return ((this.Status & (int)TUploadStatus.BundleUploaded) == (int)TUploadStatus.BundleUploaded);
			}
		}

		/// <summary>
		/// Manifest上传完成.
		/// </summary>
		/// <value><c>true</c> 完成; 未完成, <c>false</c>.</value>
		public bool isManifestUploaded {
			get { 
				if (UploadManager.ManifestUpload == false) {
					return false;
				} else {
					return ((this.Status & (int)TUploadStatus.ManifestUploaded) == (int)TUploadStatus.ManifestUploaded);
				}
			}
		}

		/// <summary>
		/// 全部上传完成.
		/// </summary>
		/// <value><c>true</c> 完成; 未完成, <c>false</c>.</value>
		public bool isAllUploaded {
			get { 
				// Scene文件（没有Manifest文件）
				if (TBundleType.Scene == this.BundleType) {
					return ((this.Status & (int)TUploadStatus.BundleUploaded) == (int)TUploadStatus.BundleUploaded);
				} else {
					if (UploadManager.ManifestUpload == false) {
						return ((this.Status & (int)TUploadStatus.BundleUploaded) == (int)TUploadStatus.BundleUploaded);
					} else {
						return ((this.Status & (int)TUploadStatus.AllUploaded) == (int)TUploadStatus.AllUploaded);
					}
				}
			}
		}

		/// <summary>
		/// 本地保存标志位.
		/// 备注：如游戏关卡等临时加载信息，则无需再本地保存，采用时时加载
		/// </summary>
		public bool isLocalSave {
			get { 
				return ((this.Status & (int)TUploadStatus.LocalSave) == (int)TUploadStatus.LocalSave);
			}
		}

		/// <summary>
		/// 备份标志位.
		/// </summary>
		public bool isBackUped {
			get { 
				return ((this.Status & (int)TUploadStatus.Backuped) == (int)TUploadStatus.Backuped);
			}
		}

		/// <summary>
		/// 重置状态.
		/// </summary>
		public void ResetSatus() {
			this.Status = (int)TUploadStatus.LocalSave;
		}

		/// <summary>
		/// 设定状态.
		/// </summary>
		/// <param name="iStatus">状态.</param>
		public void SetStatus(TUploadStatus iStatus) {
			this.Status |= ((int)iStatus);
		}

	}

	/// <summary>
	/// 上传列表.
	/// </summary>
	public class UploadList : AssetBase {

		/// <summary>
		/// 检测模式.
		/// </summary>
		[SerializeField]public TCheckMode CheckMode = TCheckMode.Defult;

		/// <summary>
		/// 平台类型.
		/// </summary>
		/// <value>平台类型.</value>
		[SerializeField]public string BuildTarget = null;

		/// <summary>
		/// 压缩格式.
		/// </summary>
		[SerializeField]public TCompressFormat CompressFormat = TCompressFormat.None;

		/// <summary>
		/// Manifest上传标志位.
		/// </summary>
		[SerializeField]public bool ManifestUpload = true;

		/// <summary>
		/// AssetBundle打包输出目录.
		/// </summary>
		[SerializeField]public string BundlesOutputDir = null;

		/// <summary>
		/// 目标列表.
		/// </summary>
		[SerializeField]public List<UploadItem> Targets = new List<UploadItem> ();
		/// <summary>
		/// 线程锁.
		/// </summary>
		private static object _targetsThreadLock = new object ();

		/// <summary>
		/// 进度计数器.
		/// </summary>
		private ProgressCounter _progressCounter = new ProgressCounter();

		/// <summary>
		/// 一般的Assetbundle的Main Manifest bundle ID(StreamingAssets).
		/// </summary>
		public string AssetBundleDirNameOfNormal {
			get { 
				return "Normal";
			}	
		}

		/// <summary>
		/// Scene打包成AssetBundle，存放的文件夹名.
		/// </summary>
		public string AssetBundleDirNameOfScenes {
			get { 
				return "Scenes";
			}	
		}

		/// <summary>
		/// 一般的AssetBundle打包输出路径.
		/// </summary>
		public string BundlesOutputDirOfNormal {
			get { 
				string dir = string.Format ("{0}/{1}", this.BundlesOutputDir, this.AssetBundleDirNameOfNormal);
				if (Directory.Exists (dir) == false) {
					Directory.CreateDirectory (dir);
				}
				return dir;
			}
		}

		/// <summary>
		/// Scenes的AssetBundle打包输出路径
		/// </summary>
		public string BundlesOutputDirOfScene {
			get { 
				string dir = string.Format ("{0}/{1}", this.BundlesOutputDir, this.AssetBundleDirNameOfScenes);
				if (Directory.Exists (dir) == false) {
					Directory.CreateDirectory (dir);
				}
				return dir;
			}
		}

		/// <summary>
		/// 实例.
		/// </summary>
		private static UploadList _instance = null;

		/// <summary>
		/// Md5对象.
		/// </summary>
		private static MD5CryptoServiceProvider _md5 = null;

		/// <summary>
		/// 取得实例.
		/// </summary>
		/// <returns>实例.</returns>
		public static UploadList GetInstance() {
			if (_instance == null) {
				_instance = UtilityAsset.Read<UploadList>();
				if (_instance == null) {
					Debug.LogError ("BundlesCheck GetInstance Failed!!!");
					return null;
				} 
				_instance.Init ();
			}
			return _instance;
		}

		/// <summary>
		/// 创建Bundle.
		/// </summary>
		/// <returns>Bundle.</returns>
		private UploadItem CreateBundle() {
			UploadItem bundle = new UploadItem ();
			this.Targets.Add (bundle);
			return bundle;
		}

		/// <summary>
		/// 重置上传时间.
		/// </summary>
		/// <param name="iUploadDateTime">上传时间.</param>
		public void ResetUploadDateTime(long iUploadDateTime) {
			foreach (UploadItem loop in this.Targets) {
				// 已经上传
				if (loop.isAllUploaded == true) {
					continue;
				}
				// 已经废弃
				if (loop.Scraped == true) {
					continue;
				}
				loop.UploadDateTime = iUploadDateTime;
			}
		}

		/// <summary>
		/// 添加对象.
		/// </summary>
		/// <param name="iTarget">I target.</param>
		public void AddTarget(UploadItem iTarget) {
			if (iTarget == null) {
				return;
			}
			this.Targets.Add (iTarget);
		}

		/// <summary>
		/// 取得BundleNo.
		/// </summary>
		/// <returns>BundleNo.</returns>
		private int GetBundleNo() {
			if ((this.Targets == null) || (this.Targets.Count <= 0)) {
				return 1;
			}
			return (this.Targets.Count + 1);
		}

		/// <summary>
		/// 取得要添加的Bundle信息.
		/// </summary>
		/// <returns>上传信息.</returns>
		/// <param name="iTargetId">目标ID.</param>
		/// <param name="iBuildMode">BuildMode.</param>
		private UploadItem GetAddBundleInfo(string iTargetId, TBuildMode iBuildMode) {
			UploadItem objRet = null;
			UploadItem lastItem = this.GetLastUploadBundleInfo (iTargetId, iBuildMode);
			if (lastItem == null) {
				objRet = new UploadItem (); 
				objRet.No = this.GetBundleNo ();
				objRet.ID = iTargetId;
				objRet.BuildMode = iBuildMode;
				this.Targets.Add (objRet);
			} else {
				// 最近一次，已经上传完成
				if (lastItem.isAllUploaded == true) {
					objRet = new UploadItem (); 
					objRet.No = this.GetBundleNo ();
					objRet.ID = iTargetId;
					objRet.BuildMode = iBuildMode;
					this.Targets.Add (objRet);

					// 尚未上传完成
				} else {
					objRet = lastItem;
					objRet.ID = iTargetId;
					objRet.BuildMode = iBuildMode;
				}
			}
			if (objRet != null) {
				objRet.BuildTarget = UploadList.GetInstance().BuildTarget.ToString();
				objRet.MarketVersion = BuildInfo.GetInstance ().MarketVersion;
				objRet.AppVersion = BuildInfo.GetInstance ().BuildVersion;
			}
			return objRet;
		}

		/// <summary>
		/// 添加MainManifest对象.
		/// </summary>
		/// <param name="iBuildMode">BuildMode.</param>
		public void AddMainManifestAssetsTarget(TBuildMode iBuildMode) {

			string manifestBundleId = this.AssetBundleDirNameOfNormal;
			if (string.IsNullOrEmpty (manifestBundleId) == true) {
				return;
			}

			string path = GetLocalBundleFilePath(
				manifestBundleId, TUploadFileType.MainManifest, false, false);
			if (File.Exists (path) == false) {
				return;
			}

			BundleMap bm = new BundleMap ();
			bm.ID = manifestBundleId;
			bm.Type = TBundleType.Normal;
			int index = path.IndexOf (manifestBundleId);
			bm.Path = path.Substring (0, index);

			// 添加对象
			this.AddTarget (bm, iBuildMode, null, TUploadFileType.MainManifest);

		}

		/// <summary>
		/// 添加对象.
		/// </summary>
		/// <param name="iTarget">对象.</param>
		/// <param name="iBuildMode">BuildMode.</param>
		/// <param name="iHashCode">HashCode(Unity3d打包生成).</param>
		/// <param name="iFileType">上传文件类型.</param>
		/// <param name="iIsLocalSave">下载保存到本地标志位.</param>
		public void AddTarget(
			BundleMap iTarget, TBuildMode iBuildMode, 
			string iHashCode = null,
			TUploadFileType iFileType = TUploadFileType.Bundle,
			bool iIsLocalSave = true) {
			if (iTarget == null) {
				return;
			}

			// 检测Md5码&HashCode128码
			// 若无变化，则无需上传
			// 取得当前最新文件的Md5码&HashCode128码信息
			string md5 = null;
			long dataSize = 0;
			string manifestMd5 = null;
			long manifestDataSize = 0;
			string filePath = GetLocalBundleFilePath (
				iTarget.ID, iFileType, false, (TBundleType.Scene == iTarget.Type));
			
			if (File.Exists (filePath) == true) {
				md5 = GetFileMD5 (filePath);
				FileInfo fileInfo = new FileInfo (filePath);
				dataSize = fileInfo.Length;

			} else {
				Debug.LogWarningFormat ("Target File is not exist!!!(target:{0})", filePath);
			}
			string mainfestPath = GetLocalBundleFilePath (
				iTarget.ID, iFileType, true, (TBundleType.Scene == iTarget.Type));
			if (File.Exists (mainfestPath) == true) {
				manifestMd5 = GetFileMD5(mainfestPath);
				FileInfo fileInfo = new FileInfo (mainfestPath);
				manifestDataSize = fileInfo.Length;
			} else {
				Debug.LogWarningFormat ("Target mainfest File is not exist!!!(target:{0})", mainfestPath);
			}

			bool isChanged = false;
			UploadItem lastBundleInfo = this.GetLastUploadBundleInfo (iTarget.ID, iBuildMode);
			if (lastBundleInfo == null) {
				isChanged = true;
			} else {
				if (TUploadFileType.MainManifest == lastBundleInfo.FileType) {
					isChanged = true;
				} else { 
					// 最近一次, 尚未上传
					if (lastBundleInfo.isAllUploaded == false) {
						isChanged = true;
					} else {
						if (((string.IsNullOrEmpty (md5) == false) && (md5.Equals (lastBundleInfo.Md5) == false)) ||
						   ((string.IsNullOrEmpty (manifestMd5) == false) && (manifestMd5.Equals (lastBundleInfo.ManifestMd5) == false)) ||
						   ((string.IsNullOrEmpty (iHashCode) == false) && (iHashCode.Equals (lastBundleInfo.HashCode) == false))) {
							isChanged = true;
						}
					}
				}
			}
			if (isChanged == false) {
				Debug.LogFormat ("Target Bundle has no changed!![Path:{0}]", filePath);
				if (File.Exists (filePath) == true) {
					File.Delete (filePath);
				}
				Debug.LogFormat ("Target Bundle has no changed!![Path:{0}]", mainfestPath);
				if (File.Exists (mainfestPath) == true) {
					File.Delete (mainfestPath);
				}
				return;
			}

			// 取得已存在最新版本的信息
			UploadItem bundle = this.GetAddBundleInfo(iTarget.ID, iBuildMode);
			bundle.BundleType = iTarget.Type;
			if (iIsLocalSave == true) {
				bundle.Status |= (int)TUploadStatus.LocalSave;
			}
			bundle.FileType = iFileType;
			bundle.Md5 = md5;
			bundle.DataSize = dataSize.ToString();
			bundle.ManifestMd5 = manifestMd5;
			bundle.ManifestDataSize = manifestDataSize.ToString();
			bundle.HashCode = iHashCode;

			UtilityAsset.SetAssetDirty (this);
		}

		/// <summary>
		/// 取得最近一次更新的Bundle信息.
		/// </summary>
		/// <returns>最近一次更新的Bundle信息.</returns>
		/// <param name="iBundleId">BundleId.</param>
		/// <param name="iBuildMode">BuildMode.</param>
		private UploadItem GetLastUploadBundleInfo(string iBundleId, TBuildMode iBuildMode) {

			UploadItem[] Targets = null;

			if ((this.Targets == null) && (this.Targets.Count <= 0)) {
				return null;
			}
			string buildTarget = UploadList.GetInstance().BuildTarget.ToString();

			Targets = this.Targets
				.Where (o => 
					(iBundleId.Equals(o.ID) == true) && 
					(buildTarget.Equals(o.BuildTarget) == true) && 
					(iBuildMode == o.BuildMode))
				.OrderByDescending (o => o.UploadDateTime)
				.ToArray ();
			if ((Targets == null) || (Targets.Length <= 0)) {
				return null;
			}
			return Targets[0];
		}

		/// <summary>
		/// 取得本地上传用的输入文件名.
		/// </summary>
		/// <returns>上传用的输入文件名.</returns>
		/// <param name="iBundleId">I bundle identifier.</param>
		/// <param name="iFileType">I file type.</param>
		/// <param name="iIsManifest">I file type.</param>
		public static string GetLocalBundleFileName(
			string iBundleId, TUploadFileType iFileType, bool iIsManifest) {

			string fileName = iBundleId;
			if (TUploadFileType.MainManifest != iFileType) {
				string fileSuffix = BundlesConfig.GetInstance ().FileSuffix;
				if (string.IsNullOrEmpty (fileSuffix) == false) {
					fileName = string.Format ("{0}.{1}", fileName, fileSuffix);
				}
			}
			if (iIsManifest == true) {
				fileName = string.Format ("{0}.manifest", fileName);
			}
			return fileName;
		}

		/// <summary>
		/// Gets the local scene bundle file path.
		/// </summary>
		/// <returns>The local scene bundle file path.</returns>
		/// <param name="iBundleId">I bundle identifier.</param>
		public static string GetLocalSceneBundleFilePath(string iBundleId) {
			string fileName = GetLocalBundleFileName(iBundleId, TUploadFileType.Bundle, false);
			return string.Format ("{0}/{1}", 
				UploadList.GetInstance ().BundlesOutputDirOfScene, fileName);
		}

		/// <summary>
		/// 取得本地上传用的输入文件地址.
		/// </summary>
		/// <returns>上传用的输入文件地址.</returns>
		/// <param name="iBundleId">I bundle identifier.</param>
		/// <param name="iFileType">I file type.</param>
		/// <param name="iIsManifest">I file type.</param>
		/// <param name="iIsScene">场景标志位.</param>
		public static string GetLocalBundleFilePath(
			string iBundleId, TUploadFileType iFileType, bool iIsManifest, bool iIsScene) {
		
			string fileName = GetLocalBundleFileName(iBundleId, iFileType, iIsManifest);
			if (iIsScene == true) {
				return string.Format ("{0}/{1}", 
					UploadList.GetInstance ().BundlesOutputDirOfScene, fileName);
			} else {
				return string.Format ("{0}/{1}", 
					UploadList.GetInstance ().BundlesOutputDirOfNormal, fileName);
			}

		}

		/// <summary>
		/// 取得Bundle的相对路径（下载/上传用）.
		/// 路径：bundles/<BuildTarget:(iOS/Android)><BuildMode:(Debug/Release/Store)><UploadDatetime>
		/// </summary>
		/// <returns>Bundle的相对路径.</returns>
		/// <param name="iBuildMode">BuildMode.</param>
		/// <param name="iUploadDatetime">上传时间.</param>
		/// <param name="iIsScene">场景标志位.</param>
		public static string GetBundleRelativePath(TBuildMode iBuildMode, long iUploadDatetime, bool iIsScene) {
			if (iIsScene == true) {
				return string.Format ("bundles/{0}/{1}/{2}/{3}", 
					UploadList.GetInstance ().BuildTarget, 
					iBuildMode.ToString (), 
					UploadList.GetInstance().AssetBundleDirNameOfScenes,
					iUploadDatetime.ToString ());
			} else {
				return string.Format ("bundles/{0}/{1}/{2}/{3}", 
					UploadList.GetInstance ().BuildTarget, 
					iBuildMode.ToString (), 
					UploadList.GetInstance().AssetBundleDirNameOfNormal,
					iUploadDatetime.ToString ());
			}
		}

		/// <summary>
		/// 取得文件的Md5码.
		/// </summary>
		/// <returns>文件的Md5码.</returns>
		/// <param name="iFilePath">文件路径.</param>
		public static string GetFileMD5(string iFilePath)
		{
			if(_md5 == null)
			{
				_md5 = new MD5CryptoServiceProvider();
			}

			if(File.Exists(iFilePath) == false)
			{
				Debug.LogError("FileMD5 file == null!");
				return "";
			}

			FileStream file = new FileStream(iFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
			byte[] hash = _md5.ComputeHash(file);
			string strMD5 = System.BitConverter.ToString(hash);
			file.Close();

			strMD5 = strMD5.ToLower();
			strMD5 = strMD5.Replace("-", "");

			return strMD5;
		}

		/// <summary>
		/// 重置.
		/// </summary>
		public void Reset() {

			UtilityAsset.Clear<UploadList> ();

			// 清空列表
			foreach (UploadItem loop in this.Targets) {
				loop.ResetSatus();
			}

			UtilityAsset.SetAssetDirty (this);

		}

		/// <summary>
		/// 更新上传信息.
		/// </summary>
		/// <param name="iDir">指定目录.</param>
		public void UpdateFromUploadlistFile() {

			// 取得导入目录
			string ImportDir = ServersConf.GetInstance().BundlesDir;

			// 导入文件
			this.ImportFromJsonFile(ImportDir);
		}

		/// <summary>
		/// 更新本地信息，为下一步下载做准备.
		/// </summary>
		public void UpdateLocalInfoForDownload() {

			// 取得导入目录
			string ImportDir = ServersConf.GetInstance().BundlesDir;

			// 导入已经有下载信息
			DownloadList.GetInstance().ImportFromJsonFile(ImportDir);

			// 导入Bundle包依赖关系
			BundlesMap.GetInstance().ImportFromJsonFile(ImportDir);

			string marketVersion = BuildInfo.GetInstance ().MarketVersion;
			string buildTarget = UploadList.GetInstance().BuildTarget.ToString();
			string appVersion = BuildInfo.GetInstance ().BuildVersion;
			TBuildMode buildMode = BuildInfo.GetInstance().BuildMode;

			// 抽出条件
			// 1）MarketVersion匹配
			// 2）BuildTarget匹配
			// 3）AppVersion匹配
			// 4）打包模式匹配
			// 5）上传完毕
			// 6）没有被废弃
			// 7）非时时下载
			UploadItem[] targets = this.Targets
				.Where (o => 
					(marketVersion.Equals(o.MarketVersion) == true) && 
					(buildTarget.Equals(o.BuildTarget) == true) && 
					(appVersion.Equals(o.AppVersion) == true) && 
					(buildMode == o.BuildMode) && 
					(true == o.isAllUploaded) && 
					(false == o.Scraped) && 
					(true == o.isLocalSave))
				.OrderBy (o => o.No)
				.ToArray ();

			if ((targets == null) || (targets.Length <= 0)) {
				Debug.Log ("There is no target to download！！！");
				return;
			} 
			Debug.LogFormat ("Download Targets Count : {0}", targets.Length.ToString());

			foreach (UploadItem Loop in targets) {
				DownloadList.GetInstance ().AddTarget (Loop);
			}
			// 初始化进度计数器
			DownloadList.GetInstance ().InitProgressCounter ();
		}

		/// <summary>
		/// 从JSON文件，导入打包配置信息.
		/// </summary>
		/// <param name="iImportDir">导入目录.</param>
		public void ImportFromJsonFile(string iImportDir) {
			UploadList jsonData = UtilityAsset.ImportFromJsonFile<UploadList> (iImportDir);
			if (jsonData != null) {
				this.ApplyData (jsonData);
			}
		}

		/// <summary>
		/// 导出成JSON文件.
		/// </summary>
		/// <param name="iImportDir">导出目录.</param>
		public void ExportToJsonFile(string iExportDir) {
			UtilityAsset.ExportToJsonFile<UploadList> (this, iExportDir);
		}

		#region Implement

		/// <summary>
		/// 初始化.
		/// </summary>
		public override void Init () { 
			if (string.IsNullOrEmpty (this.BundlesOutputDir) == true) {
				this.BundlesOutputDir = Application.streamingAssetsPath;
			}
		}

		/// <summary>
		/// 应用数据.
		/// </summary>
		/// <param name="iData">数据.</param>
		protected override void ApplyData(AssetBase iData) {
			if (iData == null) {
				return;
			}

			UploadList data = iData as UploadList;
			if (data == null) {
				return;
			}

			// 清空
			this.Clear ();

			this.CheckMode = data.CheckMode;
			this.BuildTarget = data.BuildTarget;
			this.ManifestUpload = data.ManifestUpload;
			this.CompressFormat = data.CompressFormat;

			// 添加资源信息
			foreach(UploadItem loop in data.Targets) {
				this.AddTarget (loop);
			}
			UtilityAsset.SetAssetDirty (this);

		}

		/// <summary>
		/// 清空.
		/// </summary>
		public override void Clear() {

			UtilityAsset.Clear<UploadList> ();

			this.CheckMode = TCheckMode.Defult;
			this.BuildTarget = null;
			this.ManifestUpload = false;
			this.CompressFormat = TCompressFormat.None;

			// 清空列表
			this.Targets.Clear ();

			if (this._progressCounter != null) {
				this._progressCounter.Clear ();
			}

			UtilityAsset.SetAssetDirty (this);

		}

		/// <summary>
		/// 从JSON文件，导入打包配置信息.
		/// </summary>
		public override void ImportFromJsonFile() {
			UploadList jsonData = UtilityAsset.ImportFromJsonFile<UploadList> ();
			if (jsonData != null) {
				this.ApplyData (jsonData);
			}
		}

		/// <summary>
		/// 导出成JSON文件.
		/// </summary>
		public override string ExportToJsonFile() {
			return UtilityAsset.ExportToJsonFile<UploadList> (this);
		}

		#endregion


		/// <summary>
		/// 判断目标是否存在.
		/// </summary>
		/// <returns><c>true</c>,存在, <c>false</c> 不存在.</returns>
		/// <param name="iTargetNo">目标No.</param>
		/// <param name="iTarget">目标信息.</param>
		private bool isTargetExist(int iTargetNo, out UploadItem iTarget) {

			iTarget = null;

			UploadItem[] targets = this.Targets
				.Where (o => (o.No == iTargetNo))
				.OrderBy (o => o.UploadDateTime)
				.ToArray ();
			if ((targets == null) || (targets.Length <= 0)) {
				return false;
			}
			iTarget = targets [0];
			return true;
		}


		#region ProgressCounter

		/// <summary>
		/// 是否需要上传.
		/// </summary>
		/// <value><c>true</c> 需要下载; 无需下载, <c>false</c>.</value>
		public bool isUploadNecessary {
			get { 
				if (this._progressCounter == null) {
					return false;
				}
				return (this._progressCounter.TotalDatasize > 0); 
			}
		}

		/// <summary>
		/// 取得完成进度（0.0f~1.0f）.
		/// </summary>
		/// <returns>完成进度（0.0f~1.0f）.</returns>
		public float GetCompletedProgress() {
			if (this._progressCounter == null) {
				return 1.0f;
			}
			return this._progressCounter.Progress;
		}

		/// <summary>
		/// 文件下载总数.
		/// </summary>
		/// <returns>文件下载总数.</returns>
		public int GetTotalCount() { 
			return (_progressCounter == null) ? 0 : _progressCounter.TotalCount; 
		}

		/// <summary>
		/// 数据总大小（单位：byte）.
		/// </summary>
		/// <returns>数据总大小（单位：byte）.</returns>
		public long GetTotalDatasize() { 
			return (_progressCounter == null) ? 0 : _progressCounter.TotalDatasize;
		}

		/// <summary>
		/// 已上传文件总数.
		/// </summary>
		public int GetUploadedCount() { 
			return (_progressCounter == null) ? 0 : _progressCounter.DidCount;
		}



		/// <summary>
		/// 初始化进度计数器.
		/// </summary>
		public void InitProgressCounter() {
			if (this._progressCounter == null) {
				return;
			}
			int totalCount = 0;
			long totalDataSize = 0;
			foreach (UploadItem loop in this.Targets) {
				// 已经废弃
				if(loop.Scraped) {
					continue;	
				}
				// 已经上传完成
				if (loop.isAllUploaded == true) {
					continue;
				}
					
				totalDataSize += ((string.IsNullOrEmpty(loop.DataSize) == true) ? 0 : Convert.ToInt64(loop.DataSize));
				++totalCount;

				// Scene文件（没有Manifest文件）
				if(TBundleType.Scene == loop.BundleType) {
					continue;
				}

				// Manifest
				if (UploadManager.ManifestUpload == true) {
					totalDataSize += ((string.IsNullOrEmpty (loop.ManifestDataSize) == true) ? 0 : Convert.ToInt64 (loop.ManifestDataSize));
					++totalCount;
				}
			}
			this._progressCounter.Init (totalCount, totalDataSize);
		}

		/// <summary>
		/// 上传完成.
		/// </summary>
		/// <param name="iTargetNo">目标No.</param>
		/// <param name="iIsManifest">manifest标志位.</param>
		public void UploadCompleted(int iTargetNo, bool iIsManifest) {

			lock (_targetsThreadLock) {
				UploadItem uploadItem = null;
				if (this.isTargetExist (iTargetNo, out uploadItem) == true) {
					if (uploadItem == null) {
						return;
					}

					if (this._progressCounter != null) {
						this._progressCounter.UpdateCompletedCount ();

						long dataSize = (string.IsNullOrEmpty(uploadItem.DataSize) == true) ? 0 : Convert.ToInt64(uploadItem.DataSize);
						if (iIsManifest == true) {
							dataSize = (string.IsNullOrEmpty(uploadItem.ManifestDataSize) == true) ? 0 : Convert.ToInt64(uploadItem.ManifestDataSize);
						}

						this._progressCounter.UpdateCompletedDataSize (dataSize);
					}
				}
			}
		}

		#endregion
		
	}
}
