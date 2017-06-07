using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using BuildSystem;
using Common;
using Upload;
using AssetBundles;

namespace Download {

	/// <summary>
	/// 下载目标信息.
	/// </summary>
	[System.Serializable]
	public class DownloadTargetInfo {
		
		/// <summary>
		/// bundle no.
		/// </summary>
		public int No = 0;

		/// <summary>
		/// Bundle ID.
		/// </summary>
		public string BundleId = null;

		/// <summary>
		/// bundle类型.
		/// </summary>
		public TBundleType BundleType = TBundleType.Normal;

		/// <summary>
		/// 上传文件类型.
		/// </summary>
		public TUploadFileType FileType = TUploadFileType.Bundle;

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
		/// 上传时间.
		/// </summary>
		public long UploadDateTime = 0;

		/// <summary>
		/// 本地保存标志位.
		/// 备注：如游戏关卡等临时加载信息，则无需再本地保存，采用时时加载
		/// </summary>
		public bool LocalSave = true;

		/// <summary>
		/// 下载完成标志位.
		/// </summary>
		public bool Downloaded = false;

		/// <summary>
		/// 下载完成标志位(Manifest).
		/// </summary>
		public bool ManifestDownloaded = false;

		/// <summary>
		/// 判断下载是否完成.
		/// </summary>
		/// <value><c>true</c> 完成; 尚未完成, <c>false</c>.</value>
		public bool DownloadCompleted {
			get { 
				if (UploadList.GetInstance ().ManifestUpload == false) {
					return Downloaded;
				}
				return (Downloaded && ManifestDownloaded);
			}
		}
	}

	/// <summary>
	/// 下载列表.
	/// </summary>
	public class DownloadList : AssetBase {

		/// <summary>
		/// 进度计数器.
		/// </summary>
		private ProgressCounter _progressCounter = new ProgressCounter();

		/// <summary>
		/// 下载列表.
		/// </summary>
		[SerializeField]public List<DownloadTargetInfo> Targets = new List<DownloadTargetInfo>();
		private static object _targetUpdateLock = new object();

		/// <summary>
		/// 实例.
		/// </summary>
		private static DownloadList _instance = null;

		/// <summary>
		/// 取得实例.
		/// </summary>
		/// <returns>实例.</returns>
		public static DownloadList GetInstance() {

			if (_instance == null) {
				_instance = UtilityAsset.Read<DownloadList>();
				if (_instance == null) {
					Debug.LogError ("DownloadConf GetInstance Failed!!!");
					return null;
				}
				_instance.Init ();
			}
			return _instance;
		}

		/// <summary>
		/// 添加对象.
		/// </summary>
		/// <param name="iTarget">目标.</param>
		public void AddTarget(UploadItem iTarget) {

			Debug.LogFormat ("DownloadList.AddTarget No : [Start]{0}", iTarget.No);

			DownloadTargetInfo target = null;
			if (isTargetExist (iTarget.No, out target) == false) {
				target = new DownloadTargetInfo ();
				this.Targets.Add (target);
				target.No = iTarget.No;
				target.BundleId = iTarget.ID;
				target.BundleType = iTarget.BundleType;
				target.FileType = iTarget.FileType;
				target.DataSize = iTarget.DataSize;
				target.ManifestDataSize = iTarget.ManifestDataSize;
				target.Md5 = iTarget.Md5;
				target.ManifestMd5 = iTarget.ManifestMd5;
				target.HashCode = iTarget.HashCode;
				target.UploadDateTime = iTarget.UploadDateTime;
				target.LocalSave = iTarget.isLocalSave;
				target.Downloaded = false;
				Debug.LogFormat ("DownloadList.AddTarget No : [Successed]{0}", iTarget.No);
			}
		}

		/// <summary>
		/// 取得下载对象列表
		/// </summary>
		/// <returns>下载对象列表.</returns>
		public DownloadTargetInfo[] GetDownloadTargets() {
			if ((this.Targets == null) ||
				(this.Targets.Count <= 0)) {
				return null;
			}
			DownloadTargetInfo[] targets = this.Targets
				.Where (o => (true == o.LocalSave))
				.OrderBy(o => o.No)
				.ToArray ();
			if ((targets != null) && (targets.Length > 0)) {
				return targets;
			} else {
				return null;
			}
		}

		/// <summary>
		/// 判断目标存不存在.
		/// </summary>
		/// <returns><c>true</c>, 存在, <c>false</c> 不存在.</returns>
		/// <param name="iBundleId">BundleID.</param>
		/// <param name="iUploadDateTime">上传时间.</param>
		/// <param name="iTarget">下载目标信息.</param>
		private bool isTargetExist(string iBundleId, long iUploadDateTime, out DownloadTargetInfo iTarget) {
			iTarget = null;
			if (string.IsNullOrEmpty (iBundleId) == true) {
				return false;
			}

			DownloadTargetInfo[] targets = this.Targets
				.Where (o => (
					(iBundleId.Equals (o.BundleId) == true) &&
					(iUploadDateTime == o.UploadDateTime)))
				.OrderByDescending (o => o.UploadDateTime)
				.ToArray ();
			if ((targets == null) || (targets.Length <= 0)) {
				return false;
			}
			iTarget = targets [0];
			return true;
		}

		/// <summary>
		/// 判断目标存不存在.
		/// </summary>
		/// <returns><c>true</c>, 存在, <c>false</c> 不存在.</returns>
		/// <param name="iBundleId">BundleID.</param>
		/// <param name="iTarget">下载目标信息.</param>
		public bool isTargetExist(string iBundleId, out DownloadTargetInfo iTarget) {
			iTarget = null;
			if (string.IsNullOrEmpty (iBundleId) == true) {
				return false;
			}
				
			DownloadTargetInfo[] targets = this.Targets
				.Where (o => (
			    	(iBundleId.Equals (o.BundleId) == true)))
				.OrderByDescending (o => o.UploadDateTime)
				.ToArray ();
			if ((targets == null) || (targets.Length <= 0)) {
				return false;
			}
			iTarget = targets [0];
			return true;
		}

		/// <summary>
		/// 判断目标存不存在.
		/// </summary>
		/// <returns><c>true</c>, 存在, <c>false</c> 不存在.</returns>
		/// <param name="iTargetNo">目标No.</param>
		/// <param name="iTarget">下载目标信息.</param>
		public bool isTargetExist(int iTargetNo, out DownloadTargetInfo iTarget) {
			iTarget = null;
			DownloadTargetInfo[] targets = this.Targets
				.Where (o => (
					(iTargetNo == o.No)))
				.OrderByDescending (o => o.UploadDateTime)
				.ToArray ();
			if ((targets == null) || (targets.Length <= 0)) {
				return false;
			}
			iTarget = targets [0];
			return true;
		}

		/// <summary>
		/// 取得Bundle全路径名.
		/// </summary>
		/// <returns>Bundle全路径名.</returns>
		/// <param name="iBundleId">BundleId.</param>
		/// <param name="iIsManifest">Manifest标志位.</param>
		public string GetBundleFullPath(string iBundleId, bool iIsManifest = false) {
			DownloadTargetInfo targetInfo = null;
			if (isTargetExist (iBundleId, out targetInfo) == false) {
				Debug.LogErrorFormat ("This bundles is not exist!!!({BundleId:{0}})", iBundleId);
				return null;
			}
			if (targetInfo == null) {
				return null;
			}
			string fileName = UploadList.GetLocalBundleFileName(iBundleId, targetInfo.FileType, iIsManifest);
			if (string.IsNullOrEmpty (fileName) == true) {
				return null;
			}

			string fileFullPath = null;
			switch (targetInfo.BundleType) {
			case TBundleType.Normal:
				{
					fileFullPath = string.Format ("{0}/{1}/{2}",
						ServersConf.GetInstance ().BundlesDirOfNormal,
						targetInfo.UploadDateTime, 
						fileName);
				}
				break;
			case TBundleType.Scene:
				{
					fileFullPath = string.Format ("{0}/{1}/{2}",
						ServersConf.GetInstance ().BundlesDirOfScene,
						targetInfo.UploadDateTime, 
						fileName);
				}
				break;
			default:
				{
					fileFullPath = string.Format ("{0}/{1}/{2}",
						ServersConf.GetInstance ().BundlesDir,
						targetInfo.UploadDateTime, 
						fileName);
				}
				break;
			}
			return fileFullPath;
		}

		/// <summary>
		/// 取得Bundle全路径名.
		/// </summary>
		/// <returns>Bundle全路径名.</returns>
		/// <param name="iBundleId">BundleId.</param>
		/// <param name="iIsManifest">Manifest标志位.</param>
		public string GetBundleFullPath(string iBundleId, long iUploadDateTime, bool iIsManifest = false) {
			DownloadTargetInfo targetInfo = null;
			if (isTargetExist (iBundleId, iUploadDateTime, out targetInfo) == false) {
				Debug.LogErrorFormat ("This bundles is not exist!!!({BundleId:{0} UploadDateTime:{1}})", 
					iBundleId, iUploadDateTime.ToString());
				return null;
			}
			if (targetInfo == null) {
				return null;
			}
			string fileName = UploadList.GetLocalBundleFileName(iBundleId, targetInfo.FileType, iIsManifest);
			if (string.IsNullOrEmpty (fileName) == true) {
				return null;
			}
				
			string fileFullPath = null;
			switch (targetInfo.BundleType) {
			case TBundleType.Normal:
				{
					fileFullPath = string.Format ("{0}/{1}/{2}",
						ServersConf.GetInstance ().BundlesDirOfNormal,
						targetInfo.UploadDateTime, 
						fileName);
				}
				break;
			case TBundleType.Scene:
				{
					fileFullPath = string.Format ("{0}/{1}/{2}",
						ServersConf.GetInstance ().BundlesDirOfScene,
						targetInfo.UploadDateTime, 
						fileName);
				}
				break;
			default:
				{
					fileFullPath = string.Format ("{0}/{1}/{2}",
						ServersConf.GetInstance ().BundlesDir,
						targetInfo.UploadDateTime, 
						fileName);
				}
				break;
			}
			return fileFullPath;
		}

		/// <summary>
		/// 重置.
		/// </summary>
		public void Reset() {

			// 初始化进度计数器
			this.InitProgressCounter ();

			foreach (DownloadTargetInfo loop in this.Targets) {
				loop.Downloaded = false;
			}

			// 清空列表
			UtilityAsset.SetAssetDirty (this);

		}

		/// <summary>
		/// 从JSON文件，导入打包配置信息.
		/// </summary>
		/// <param name="iImportDir">导入目录.</param>
		public void ImportFromJsonFile(string iImportDir) {			

			DownloadList jsonData = UtilityAsset.ImportFromJsonFile<DownloadList> (iImportDir);
			if (jsonData != null) {
				this.ApplyData (jsonData);
			}
		}

		/// <summary>
		/// 导出成JSON文件.
		/// </summary>
		/// <param name="iImportDir">导出目录.</param>
		public void ExportToJsonFile(string iExportDir) {
			UtilityAsset.ExportToJsonFile<DownloadList> (this, iExportDir);
		}

		/// <summary>
		/// 取得下载目标信息.
		/// </summary>
		/// <returns>下载目标信息.</returns>
		/// <param name="iBundleId">BundleId.</param>
		public DownloadTargetInfo GetDownloadTargetInfoById(string iBundleId) {

			if (string.IsNullOrEmpty (iBundleId) == true) {
				return null;
			}
		
			DownloadTargetInfo[] targets = this.Targets
				.Where (o => (o.BundleId.Equals (iBundleId) == true))
				.OrderByDescending(o => o.UploadDateTime)
				.ToArray ();
				
			if ((targets != null) && (targets.Length >= 1)) {
				return targets [0];
			} else {
				return null;
			}
		}

		/// <summary>
		/// 清空.
		/// </summary>
		public void Clean() {

			if (this._progressCounter != null) {
				this._progressCounter.Clear();
			}

			this.Targets.Clear ();

			// 清空列表
			UtilityAsset.SetAssetDirty (this);

		}

		#region Implement

		/// <summary>
		/// 初始化.
		/// </summary>
		public override void Init () {

			//			UtilityAsset.SetAssetDirty (this);
		}

		/// <summary>
		/// 应用数据.
		/// </summary>
		/// <param name="iData">数据.</param>
		protected override void ApplyData(AssetBase iData) {
			if (iData == null) {
				return;
			}

			DownloadList data = iData as DownloadList;
			if (data == null) {
				return;
			}
			// 清空
			this.Clear ();

			// 添加以后信息
			foreach (DownloadTargetInfo loop in data.Targets) {
				this.Targets.Add (loop);
			}

			UtilityAsset.SetAssetDirty (this);

		}

		/// <summary>
		/// 清空.
		/// </summary>
		public override void Clear() {

			UtilityAsset.Clear<DownloadList> (ServersConf.GetInstance().BundlesDir, true);

			if (this._progressCounter != null) {
				this._progressCounter.Clear();
			}

			this.Targets.Clear ();

			// 清空列表
			UtilityAsset.SetAssetDirty (this);

		}

		/// <summary>
		/// 从JSON文件，导入打包配置信息.
		/// </summary>
		public override void ImportFromJsonFile() {			

			DownloadList jsonData = UtilityAsset.ImportFromJsonFile<DownloadList> ();
			if (jsonData != null) {
				this.ApplyData (jsonData);
			}
		}

		/// <summary>
		/// 导出成JSON文件.
		/// </summary>
		/// <returns>导出路径.</returns>
		public override string ExportToJsonFile() {
			return UtilityAsset.ExportToJsonFile<DownloadList> (this);
		}

		#endregion

		#region ProgressCounter

		/// <summary>
		/// 是否需要下载.
		/// </summary>
		/// <value><c>true</c> 需要下载; 无需下载, <c>false</c>.</value>
		public bool isDownloadNecessary {
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
		/// 已下载文件总数.
		/// </summary>
		public int GetDownloadedCount() { 
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
			foreach (DownloadTargetInfo loop in this.Targets) {
				if (loop.Downloaded == false) {
					totalDataSize += ((string.IsNullOrEmpty(loop.DataSize) == true) ? 0 : Convert.ToInt64(loop.DataSize));
					++totalCount;
				}

				// Scene文件
				if(TBundleType.Scene == loop.BundleType) {
					continue;
				}

				if (DownloadManager.ManifestUpload == false) {
					continue;
				}

				if (loop.ManifestDownloaded == false) {
					totalDataSize += ((string.IsNullOrEmpty (loop.ManifestDataSize) == true) ? 0 : Convert.ToInt64 (loop.ManifestDataSize));
					++totalCount;
				}
			}
			this._progressCounter.Init (totalCount, totalDataSize);
		}

		/// <summary>
		/// 下载完成.
		/// </summary>
		/// <param name="iTargetNo">目标No.</param>
		/// <param name="iIsManifest">manifest标志位.</param>
		public void DownloadCompleted(int iTargetNo, bool iIsManifest) {

			lock (_targetUpdateLock) {
				DownloadTargetInfo downloadInfo = null;
				if (this.isTargetExist (iTargetNo, out downloadInfo) == true) {
					if (downloadInfo == null) {
						return;
					}
					if (iIsManifest == true) {
						downloadInfo.ManifestDownloaded = true;
					} else {
						downloadInfo.Downloaded = true;
					}

					if (this._progressCounter != null) {
						this._progressCounter.UpdateCompletedCount ();

						long dataSize = (string.IsNullOrEmpty(downloadInfo.DataSize) == true) ? 0 : Convert.ToInt64(downloadInfo.DataSize);
						if (iIsManifest == true) {
							dataSize = (string.IsNullOrEmpty(downloadInfo.ManifestDataSize) == true) ? 0 : Convert.ToInt64(downloadInfo.ManifestDataSize);
						}

						this._progressCounter.UpdateCompletedDataSize (dataSize);

						Debug.LogFormat ("Completed Info : Count({0}/{1}) DataSize({2}/{3})",
							this._progressCounter.DidCount, this._progressCounter.TotalCount, 
							this._progressCounter.DidDatasize, this._progressCounter.TotalDatasize);
					}
				}
			}
		}

		#endregion
	}
}