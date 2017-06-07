using UnityEngine;
using System;
using System.IO;
using System.Net;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using AssetBundles;
using BuildSystem;
using Common;
using Download;

namespace Upload {

	/// <summary>
	/// 上传方式.
	/// </summary>
	public enum TUploadWay {
		Invalid,
		Ftp,
		Default = Ftp,
		Max
	}

	/// <summary>
	/// 上传管理者.
	/// </summary>
	public class UploadManager : MonoBehaviour {

		/// <summary>
		/// 上传进度条.
		/// </summary>
		[HideInInspector][SerializeField]
		public UIProgressBar _uploadBar = null;

		/// <summary>
		/// 上传事件.
		/// </summary>
		[SerializeField]
		private ProgressEventTrigger _uploadEvents = new ProgressEventTrigger();

		/// <summary>
		/// 错误列表.
		/// </summary>
		private List<ErrorDetail> _errors = new List<ErrorDetail> ();
		private static object _errorLock = new object ();

		/// <summary>
		/// 上传状态.
		/// </summary>
		private string _uploadState = null;

		/// <summary>
		/// 上传时间（格式：YYYYMMDDHHMM）.
		/// </summary>
		private long UploadDateTime = 0;

		/// <summary>
		/// 上传方式.
		/// </summary>
		private TUploadWay _uploadWay = TUploadWay.Ftp;

		/// <summary>
		/// 运行状态.
		/// </summary>
		private TRunState _State = TRunState.OK;

		/// <summary>
		/// 上传队列.
		/// </summary>
		private Queue<Uploader> UploadQueue = new Queue<Uploader> ();

		/// <summary>
		/// 最大上传器个数.
		/// </summary>
		private int UploaderMaxCount { get; set; }

		/// <summary>
		/// 传器个数.
		/// </summary>
		private int UploaderCount { get; set; }
		private static object _uploaderCountLock = new object ();

		/// <summary>
		/// 取消标志位.
		/// </summary>
		public static bool isCanceled = false;

		/// <summary>
		/// 完成标志位.
		/// </summary>
		private bool _isCompleted = false;

		/// <summary>
		/// 开始标志位.
		/// </summary>
		public bool isStarted { get; set; }

		/// <summary>
		/// Manifest文件上传标志位.
		/// </summary>
		public static bool ManifestUpload = false;

		void Awake() {

			ManifestUpload = UploadList.GetInstance ().ManifestUpload;

			// 设定下载进度条委托
			if (this._uploadBar != null) {
				this._uploadBar.SetDelegate (
					GetUploadedProgress,
					GetUploadedState,
					GetUploadTotalCount,
					GetUploadedCount);
			}
		}

		/// <summary>
		/// 更新函数.
		/// </summary>
		void Update () {

			// 尚未开始
			if (this.isStarted == false) {
				return;
			}

			// 已经完成
			if (this._isCompleted == true) {
				this.isStarted = false;
				return;
			}

			// 一直等待下载完成
			float progress = this.GetUploadedProgress ();
			if (progress >= 1.0f) {
				this.UploadCompleteNotification ();
				this._isCompleted = true;
				return;
			}
		}

		/// <summary>
		/// 初始化上传队列.
		/// </summary>
		/// <param name="iBuildMode">BuildMode.</param>
		private IEnumerator InitUploadQueue(TBuildMode iBuildMode) {

			this._isCompleted = false;

			// 初始化上传信息队列
			List<UploadItem> targets = UploadList.GetInstance().Targets;
			if (this.UploadQueue != null) {

				UploadItem[] uploadTargets = targets.
					Where (o => (
						(iBuildMode == o.BuildMode) && 
						(false == o.isAllUploaded)))
					.OrderBy (o => o.No)
					.ToArray ();
				if ((uploadTargets == null) || (uploadTargets.Length <= 0)) {
					this._State = TRunState.NoUploadTarget;
					Debug.LogWarning ("[Upload] There is no target to upload!!!");
				}
				yield return new WaitForEndOfFrame ();

				if (TRunState.OK == this._State) {
					this.UploaderCount = 0;
					int targetsCount = uploadTargets.Length;
					int maxCount = ServersConf.GetInstance ().ThreadMaxCount;
					this.UploaderMaxCount = (targetsCount > maxCount) ? maxCount : targetsCount;
				
					foreach (UploadItem loop in uploadTargets) {

						if (loop == null) {
							continue;
						}
						Uploader uploader = Uploader.Create (loop, 
							                   this.OnUploadStart,
							                   this.OnUploadFailed,
							                   this.OnUploadSuccessed);
						if (uploader != null) {
							this.UploadQueue.Enqueue (uploader);
						}
					}
					yield return new WaitForEndOfFrame ();
				}
			}
			yield return null;
		}

		#region UploadBar 

		private float GetUploadedProgress() {
			return UploadList.GetInstance ().GetCompletedProgress ();
		}
		private string GetUploadedState() {
			return this._uploadState;
		}
		private int GetUploadTotalCount() {
			return UploadList.GetInstance ().GetTotalCount ();
		}
		private int GetUploadedCount() {
			return UploadList.GetInstance ().GetUploadedCount ();
		}

		#endregion

		#region Upload

		/// <summary>
		/// 开始上传.
		/// </summary>
		/// <param name="iUploadWay">上传方式.</param>
		public void StartUpload(TUploadWay iUploadWay = TUploadWay.Default) {
		
			this._uploadWay = iUploadWay;
			this.isStarted = true;
			isCanceled = false;

			// 开始下载初始化状态：OK
			this._State = TRunState.OK;

			if (this._uploadBar != null) {
				this._uploadBar.Init ();
			}

			// 检测上传
			StartCoroutine(UploadCheck());
		}

		/// <summary>
		/// 取消上传.
		/// </summary>
		public void UploadCancel() {
			
			// 打断当前线程并停止所有协同进程
			this.StopAllCoroutines();

			// 确认取消下载
			this.UploadCancelNotification ();

		}

		/// <summary>
		/// 上传.
		/// </summary>
		private IEnumerator Upload() {

			// 检测目录
			yield return CheckDirsOnServer();
			yield return new WaitForSeconds (1.0f);
			if (TRunState.OK == this._State) {
				int max = (int)TBuildMode.Max;
				for (int idx = 0; idx < max; ++idx) {
					// 根据模式上传
					yield return UploadByMode ((TBuildMode)idx);
					yield return new WaitForSeconds (2.0f);
				}
			}

			yield return null;
		}

		/// <summary>
		/// 上传.
		/// </summary>
		/// <param name="iBuildMode">BuildMode.</param>
		private IEnumerator UploadByMode(TBuildMode iBuildMode) {

			// 初始化
			yield return this.InitUploadQueue(iBuildMode);
			yield return new WaitForSeconds (1.0f);

			if (TRunState.OK == this._State) {
				// 开始上传文件
				yield return UploadFiles (this._uploadWay);
				yield return new WaitForSeconds (1.0f);
			} else {
				this.isStarted = false;
				this._isCompleted = false;
				this._errors.Clear ();
			}
		}

		/// <summary>
		/// 上传.
		/// </summary>
		private IEnumerator UploadFiles(TUploadWay iUploadWay = TUploadWay.Default) {

			UploadServerInfo server = ServersConf.GetInstance ().GetUploadServerInfo ();
			if (server == null) {
				yield return null;
			}
			while (this.UploadQueue.Count > 0) {

				if (this.UploaderCount >= this.UploaderMaxCount) {
					yield return new WaitForSeconds (1.0f);
				}

				// 上传出错则停止
				if (TRunState.OK != this._State) {
					Debug.LogErrorFormat ("[Upload Failed] Error : {0}", this._State.ToString());
					// 取消现有上传线程
					isCanceled = true;
					break;
				}

				Uploader uploader = this.UploadQueue.Dequeue ();
				if (uploader == null) {
					continue;
				}
				yield return uploader.UploadBundleFile ();
				yield return new WaitForSeconds (0.5f);

				// 上传出错则停止
				if (TRunState.OK != this._State) {
					Debug.LogErrorFormat ("[Upload Failed] Error : {0}", this._State.ToString());
					// 取消现有上传线程
					isCanceled = true;
					break;
				}
					
				yield return uploader.UploadManifestFile ();
				yield return new WaitForSeconds (0.5f);

				// 上传出错则停止
				if (TRunState.OK != this._State) {
					Debug.LogErrorFormat ("[Upload Failed] Error : {0}", this._State.ToString());
					// 取消现有上传线程
					isCanceled = true;
					break;
				}

			}

			yield return null;
		}

		#endregion

		/// <summary>
		/// 重置上传时间.
		/// </summary>
		private void ResetUploadDateTime() {

			DateTime currentTime = System.DateTime.Now;
			string strTmp = string.Format ("{0:yyyyMMddHHmm}", currentTime);
			this.UploadDateTime = Convert.ToInt64 (strTmp);

			if (this.UploadDateTime > 0) {
				UploadList.GetInstance ().ResetUploadDateTime (this.UploadDateTime);
			}

		}

		#region BackUp


		/// <summary>
		/// 备份本地文件.
		/// </summary>
		private void BackUpBundleFiles() {

			foreach (UploadItem loop in UploadList.GetInstance().Targets) {

				// 尚未上传完毕
				if(loop.isAllUploaded == false) {
					continue;
				}

				// 已经备份完毕
				if(loop.isBackUped == true) {
					continue;
				}

				string subDir = UploadList.GetInstance ().AssetBundleDirNameOfNormal;
				if (TBundleType.Scene == loop.BundleType) {
					subDir = UploadList.GetInstance ().AssetBundleDirNameOfScenes;
				}

				string filePath = UploadList.GetLocalBundleFilePath(
					loop.ID, loop.FileType, false, (TBundleType.Scene == loop.BundleType));
				string manifestFilePath = UploadList.GetLocalBundleFilePath(
					loop.ID, loop.FileType, true, (TBundleType.Scene == loop.BundleType));

				// 废弃Bundle
				if (loop.Scraped == true) {
					if (File.Exists (filePath) == true) {
						File.Delete (filePath);
					}
					if (File.Exists (manifestFilePath) == true) {
						File.Delete (manifestFilePath);
					}
				} else {

					// Bundle 文件备份
					if (this.BackupFile (filePath, subDir) == false) {
						break;
					}

					// Bundle 文件备份
					if (this.BackupFile (manifestFilePath, subDir) == false) {
						break;
					}

					// 成功备份
					UploadItem[] targets = UploadList.GetInstance ().Targets
						.Where(o => (
							(o.ID.Equals(loop.ID) == true) &&
							(o.MarketVersion.Equals(loop.MarketVersion) == true) &&
							(o.BuildTarget.Equals(loop.BuildTarget) == true) &&
							(o.AppVersion.Equals(loop.AppVersion) == true) &&
							(o.UploadDateTime == this.UploadDateTime)))
						.ToArray();
					foreach(UploadItem backup in targets) {
						backup.SetStatus(TUploadStatus.Backuped);
					}
				}
			}
		}

		/// <summary>
		/// 备份文件.
		/// </summary>
		/// <param name="iBackupFilePath">备份文件路径.</param>
		/// <param name="iSubDir">备份文件子目录.</param>
		private bool BackupFile(string iBackupFilePath, string iSubDir = null) {

			// 上传文件列表
			if ((string.IsNullOrEmpty(iBackupFilePath) == false) && 
				(File.Exists (iBackupFilePath) == true)) {

				// 移到备份目录
				string backupDir = this.GetBundleBackDir (this.UploadDateTime, iSubDir);

				int lastIndex = iBackupFilePath.LastIndexOf ("/");
				string fileName = iBackupFilePath.Substring (lastIndex + 1);
				string backUpFileFullPath = string.Format ("{0}/{1}", backupDir, fileName);

				Debug.LogFormat ("[BackUp] {0} -> {1}", iBackupFilePath, backUpFileFullPath);
				File.Move (iBackupFilePath, backUpFileFullPath);
				if (File.Exists (backUpFileFullPath) == false) {
					Debug.LogFormat ("[BackUp Failed] {0} -> {1}", iBackupFilePath, backUpFileFullPath);
					return false;
				}
				this._uploadState = string.Format ("[BackUp] -> {0}", fileName);
			}
			return true;
		}

		/// <summary>
		/// 取得Bundle备份目录（用于SVN备份目录）.
		/// </summary>
		/// <returns>Bundle备份目录.</returns>
		/// <param name="iUploadDataTime">上传时间.</param>
		/// <param name="iSubDir">子文件夹.</param>
		private string GetBundleBackDir(long iUploadDataTime, string iSubDir = null) {
			string backUpDir = AssetBundlesManager.BackUpDir;
			if (Directory.Exists (backUpDir) == false) {
				Directory.CreateDirectory (backUpDir);
			}
			backUpDir = AssetBundlesManager.BackUpDirOfBundles;
			if (Directory.Exists (backUpDir) == false) {
				Directory.CreateDirectory (backUpDir);
			}
			backUpDir = string.Format ("{0}/{1}", backUpDir, UploadList.GetInstance().BuildTarget);
			if (Directory.Exists (backUpDir) == false) {
				Directory.CreateDirectory (backUpDir);
			}
			backUpDir = string.Format ("{0}/{1}", backUpDir, iUploadDataTime.ToString ());
			if (Directory.Exists (backUpDir) == false) {
				Directory.CreateDirectory (backUpDir);
			}
			if (string.IsNullOrEmpty (iSubDir) == false) {
				backUpDir = string.Format ("{0}/{1}", backUpDir, iSubDir);
				if (Directory.Exists (backUpDir) == false) {
					Directory.CreateDirectory (backUpDir);
				}
			}
			return backUpDir;
		}

		#endregion

		#region Ftp 处理

		/// <summary>
		/// 检测服务器上得各个路径.
		/// </summary>
		/// <returns><c>true</c>, OK, <c>false</c> NG.</returns>
		private IEnumerator CheckDirsOnServer() {

			string buildName = BuildInfo.GetInstance ().BuildName;

			// 上传服务器信息
			UploadServerInfo server = ServersConf.GetInstance().UploadServer;
			if (server == null) {
				this._State = TRunState.GetServerInfoFailed;
				yield return null;
			}

			// 取得上传URL
			string uploadBaseUrl = ServersConf.GetUploadBaseURL(server);

			int startIndex = uploadBaseUrl.IndexOf (buildName);
			string dirsInfo = uploadBaseUrl.Substring (startIndex);
			string[] dirNames = dirsInfo.Split ('/');

			string curUrl = uploadBaseUrl.Substring (0, startIndex - 1);
			string createdDir = null;
			for (int i = 0; i < dirNames.Length; ++i) {

				if (this.CheckDirOnServer (server, curUrl, dirNames [i], ref createdDir) == true) {
					curUrl = string.Format ("{0}/{1}", curUrl, dirNames [i]);
					yield return new WaitForSeconds (0.2f);
				} else {
					this._State = TRunState.CheckDirFailed;
					yield return null;
				}
			}

			// 检测目录
			// Url:<ParentUrl>/bundles
			if (this.CheckDirOnServer (server, curUrl, "bundles", ref createdDir) == true) {
				curUrl = string.Format ("{0}/{1}", curUrl, "bundles");
				yield return new WaitForSeconds (0.2f);
			} else {
				this._State = TRunState.CheckDirFailed;
				yield return null;
			}
				
			// Url:<ParentUrl>/<BuildTarget>
			string buildTarget = UploadList.GetInstance().BuildTarget;
			if (this.CheckDirOnServer (server, curUrl, buildTarget, ref createdDir) == true) {
				curUrl = string.Format ("{0}/{1}", curUrl, buildTarget);
				yield return new WaitForSeconds (0.2f);
			} else {
				this._State = TRunState.CheckDirFailed;
				yield return null;
			}

			// Url:<ParentUrl>/Debug
			string curUrlTmp = curUrl;
			string createdDirTmp = createdDir;
			if (this.CheckDirOnServer (server, curUrlTmp, TBuildMode.Debug.ToString(), ref createdDirTmp) == true) {
				curUrlTmp = string.Format ("{0}/{1}", curUrlTmp, TBuildMode.Debug.ToString());
				yield return new WaitForSeconds (0.2f);
			} else {
				this._State = TRunState.CheckDirFailed;
				yield return null;
			}
			// Url:<ParentUrl>/Normal
			string subUrlTmp = curUrlTmp;
			string subCreatedDirTmp = createdDirTmp;
			if (this.CheckDirOnServer (server, subUrlTmp, 
				UploadList.GetInstance().AssetBundleDirNameOfNormal, ref subCreatedDirTmp) == true) {
				subUrlTmp = string.Format ("{0}/{1}", subUrlTmp, UploadList.GetInstance().AssetBundleDirNameOfNormal);
				yield return new WaitForSeconds (0.2f);
			} else {
				this._State = TRunState.CheckDirFailed;
				yield return null;
			}

			// Url:<ParentUrl>/<UploadDatetime>
			if (this.CheckDirOnServer (server, subUrlTmp, 
				this.UploadDateTime.ToString(), ref subCreatedDirTmp) == true) {
				subUrlTmp = string.Format ("{0}/{1}", subUrlTmp, this.UploadDateTime.ToString());
				yield return new WaitForSeconds (0.2f);
			} else {
				this._State = TRunState.CheckDirFailed;
				yield return null;
			}

			// Url:<ParentUrl>/Scene
			subUrlTmp = curUrlTmp;
			subCreatedDirTmp = createdDirTmp;
			if (this.CheckDirOnServer (server, subUrlTmp, 
				UploadList.GetInstance().AssetBundleDirNameOfScenes, ref subCreatedDirTmp) == true) {
				subUrlTmp = string.Format ("{0}/{1}", subUrlTmp, UploadList.GetInstance().AssetBundleDirNameOfScenes);
				yield return new WaitForSeconds (0.2f);
			} else {
				this._State = TRunState.CheckDirFailed;
				yield return null;
			}

			// Url:<ParentUrl>/<UploadDatetime>
			if (this.CheckDirOnServer (server, subUrlTmp, 
				this.UploadDateTime.ToString(), ref subCreatedDirTmp) == true) {
				subUrlTmp = string.Format ("{0}/{1}", subUrlTmp, this.UploadDateTime.ToString());
				yield return new WaitForSeconds (0.2f);
			} else {
				this._State = TRunState.CheckDirFailed;
				yield return null;
			}

			// Url:<ParentUrl>/Release
			curUrlTmp = curUrl;
			createdDirTmp = createdDir;
			if (this.CheckDirOnServer (server, curUrlTmp, TBuildMode.Release.ToString(), ref createdDirTmp) == true) {
				curUrlTmp = string.Format ("{0}/{1}", curUrlTmp, TBuildMode.Release.ToString());
				yield return new WaitForSeconds (0.2f);
			} else {
				this._State = TRunState.CheckDirFailed;
				yield return null;
			}

			// Url:<ParentUrl>/Normal
			subUrlTmp = curUrlTmp;
			subCreatedDirTmp = createdDirTmp;
			if (this.CheckDirOnServer (server, subUrlTmp, 
				UploadList.GetInstance().AssetBundleDirNameOfNormal, ref subCreatedDirTmp) == true) {
				subUrlTmp = string.Format ("{0}/{1}", subUrlTmp, UploadList.GetInstance().AssetBundleDirNameOfNormal);
				yield return new WaitForSeconds (0.2f);
			} else {
				this._State = TRunState.CheckDirFailed;
				yield return null;
			}

			// Url:<ParentUrl>/<UploadDatetime>
			if (this.CheckDirOnServer (server, subUrlTmp, 
				this.UploadDateTime.ToString(), ref subCreatedDirTmp) == true) {
				subUrlTmp = string.Format ("{0}/{1}", subUrlTmp, this.UploadDateTime.ToString());
				yield return new WaitForSeconds (0.2f);
			} else {
				this._State = TRunState.CheckDirFailed;
				yield return null;
			}

			// Url:<ParentUrl>/Scene
			subUrlTmp = curUrlTmp;
			subCreatedDirTmp = createdDirTmp;
			if (this.CheckDirOnServer (server, subUrlTmp, 
				UploadList.GetInstance().AssetBundleDirNameOfScenes, ref subCreatedDirTmp) == true) {
				subUrlTmp = string.Format ("{0}/{1}", subUrlTmp, UploadList.GetInstance().AssetBundleDirNameOfScenes);
				yield return new WaitForSeconds (0.2f);
			} else {
				this._State = TRunState.CheckDirFailed;
				yield return null;
			}

			// Url:<ParentUrl>/<UploadDatetime>
			if (this.CheckDirOnServer (server, subUrlTmp, 
				this.UploadDateTime.ToString(), ref subCreatedDirTmp) == true) {
				subUrlTmp = string.Format ("{0}/{1}", subUrlTmp, this.UploadDateTime.ToString());
				yield return new WaitForSeconds (0.2f);
			} else {
				this._State = TRunState.CheckDirFailed;
				yield return null;
			}

			// Url:<ParentUrl>/Store
			curUrlTmp = curUrl;
			createdDirTmp = createdDir;
			if (this.CheckDirOnServer (server, curUrlTmp, TBuildMode.Store.ToString(), ref createdDirTmp) == true) {
				curUrlTmp = string.Format ("{0}/{1}", curUrlTmp, TBuildMode.Store.ToString());
				yield return new WaitForSeconds (0.2f);
			} else {
				this._State = TRunState.CheckDirFailed;
				yield return null;
			}

			// Url:<ParentUrl>/Normal
			subUrlTmp = curUrlTmp;
			subCreatedDirTmp = createdDirTmp;
			if (this.CheckDirOnServer (server, subUrlTmp, 
				UploadList.GetInstance().AssetBundleDirNameOfNormal, ref subCreatedDirTmp) == true) {
				subUrlTmp = string.Format ("{0}/{1}", subUrlTmp, UploadList.GetInstance().AssetBundleDirNameOfNormal);
				yield return new WaitForSeconds (0.2f);
			} else {
				this._State = TRunState.CheckDirFailed;
				yield return null;
			}

			// Url:<ParentUrl>/<UploadDatetime>
			if (this.CheckDirOnServer (server, subUrlTmp, 
				this.UploadDateTime.ToString(), ref subCreatedDirTmp) == true) {
				subUrlTmp = string.Format ("{0}/{1}", subUrlTmp, this.UploadDateTime.ToString());
				yield return new WaitForSeconds (0.2f);
			} else {
				this._State = TRunState.CheckDirFailed;
				yield return null;
			}

			// Url:<ParentUrl>/Scene
			subUrlTmp = curUrlTmp;
			subCreatedDirTmp = createdDirTmp;
			if (this.CheckDirOnServer (server, subUrlTmp, 
				UploadList.GetInstance().AssetBundleDirNameOfScenes, ref subCreatedDirTmp) == true) {
				subUrlTmp = string.Format ("{0}/{1}", subUrlTmp, UploadList.GetInstance().AssetBundleDirNameOfScenes);
				yield return new WaitForSeconds (0.2f);
			} else {
				this._State = TRunState.CheckDirFailed;
				yield return null;
			}

			// Url:<ParentUrl>/<UploadDatetime>
			if (this.CheckDirOnServer (server, subUrlTmp, 
				this.UploadDateTime.ToString(), ref subCreatedDirTmp) == true) {
				subUrlTmp = string.Format ("{0}/{1}", subUrlTmp, this.UploadDateTime.ToString());
				yield return new WaitForSeconds (0.2f);
			} else {
				this._State = TRunState.CheckDirFailed;
				yield return null;
			}

			yield return null;

		}

		/// <summary>
		/// 检测服务器上Dir信息.
		/// </summary>
		/// <returns><c>true</c>, 成功, <c>false</c> 失败.</returns>
		/// <param name="iServer">服务器信息.</param>
		/// <param name="iParentUrl">上一层Url.</param>
		/// <param name="iCheckDir">检测.</param>
		/// <param name="iCreatedDir">以创建的相对目录.</param>
		private bool CheckDirOnServer(
			UploadServerInfo iServer, string iParentUrl, 
			string iCheckDir, ref string iCreatedDir) {

			if (string.IsNullOrEmpty (iCreatedDir) == true) {
				iCreatedDir = iCheckDir;
			} else {
				iCreatedDir = string.Format ("{0}/{1}", iCreatedDir, iCheckDir);
			}

			// 已经在服务器上创建
			if (ServersConf.GetInstance ().isDirCreatedOnServer (iServer.ID, iCreatedDir) == true) {
				return true;
			}

			// 检测目录
			if (this.CheckDirOnServer (iServer, iParentUrl, iCheckDir) == true) {
				ServersConf.GetInstance ().AddCreatedDir (iServer.ID, iCreatedDir);
			} else {
				this._State = TRunState.CheckDirFailed;
				return false;
			}
			return true;
		}

		/// <summary>
		/// 检测服务器上Dir信息.
		/// </summary>
		/// <returns><c>true</c>, 成功, <c>false</c> 失败.</returns>
		/// <param name="iServer">服务器信息.</param>
		/// <param name="iParentUrl">上一层Url.</param>
		/// <param name="iCheckDir">检测.</param>
		private bool CheckDirOnServer(UploadServerInfo iServer, string iParentUrl, string iCheckDir) {
			if (string.IsNullOrEmpty (iParentUrl) == true) {
				return false;
			} 
			if (string.IsNullOrEmpty (iCheckDir) == true) {
				return false;
			}

			TDirState state = TDirState.None;

			// FTP
			if (TUploadWay.Ftp == this._uploadWay) {
				state = this.GetDirStateOnFtpServer (
					string.Format ("{0}/", iParentUrl), 
					iCheckDir, iServer.AccountId, iServer.Pwd);
			}
			switch (state) {
			case TDirState.Exist:
				{
					return true;
				}
				break;
			case TDirState.NotExist:
				{
					string createUrl = string.Format ("{0}/{1}", iParentUrl, iCheckDir);
					if (TUploadWay.Ftp == this._uploadWay) {
						state = this.CreateDirOnFtpServer (createUrl, iServer.AccountId, iServer.Pwd);
						if (TDirState.Created == state) {
							return true;
						}
					}
				}
				break;
			default:
				break;
			}
			return false;
		}

		/// <summary>
		/// 取得指定目录在Ftp服务器上得状态.
		/// </summary>
		/// <returns>目录在服务器上得状态.</returns>
		/// <param name="iParentUrl">上一层URL.</param>
		/// <param name="iTargetDir">目标目录.</param>
		/// <param name="iAccountId">账户.</param>
		/// <param name="iPwd">密码.</param>
		private TDirState GetDirStateOnFtpServer(
			string iParentUrl, string iTargetDir, 
			string iAccountId, string iPwd) {

			FtpWebRequest ftpRequest = null;
			FtpWebResponse response = null;
			StreamReader reader = null;
			TDirState state = TDirState.None;

			try
			{

				Uri targetURI = new Uri(iParentUrl);
				ftpRequest = (FtpWebRequest)FtpWebRequest.Create(targetURI);
				ftpRequest.Credentials = new NetworkCredential(iAccountId, iPwd);
				ftpRequest.KeepAlive = false;
				ftpRequest.Method = WebRequestMethods.Ftp.ListDirectory;
				ftpRequest.UseBinary = true;
				response = ftpRequest.GetResponse() as FtpWebResponse;

				reader = new StreamReader(
					response.GetResponseStream(),System.Text.Encoding.Default);
				string line = reader.ReadLine();
				state = TDirState.NotExist;
				// 循环遍历内容
				while (line != null) {
					if(line.Equals(iTargetDir) == true) {
						state = TDirState.Exist;
						break;
					}
					line = reader.ReadLine();
				}

				if(TDirState.NotExist == state) {
					Debug.LogFormat("[DirState] State:{0} ParentUrl:{1} TargetDir:{2}", 
						state.ToString(), iParentUrl, iTargetDir);
				}
			}
			catch (WebException exp) 
			{
				Debug.LogErrorFormat ("[DirState Exception] ParentUrl:{0} TargetDir:{1} \n WebException: \n {2}",
					iParentUrl, iTargetDir, exp.Message);
				state = TDirState.Exception;
			}
			catch(IOException exp) 
			{
				Debug.LogErrorFormat ("[DirState Exception] ParentUrl:{0} TargetDir:{1} \n IOException: \n {2}",
					iParentUrl, iTargetDir, exp.Message);
				state = TDirState.Exception;
			}
			catch (Exception exp) 
			{
				Debug.LogErrorFormat ("[DirState Exception] ParentUrl:{0} TargetDir:{1} \n Exception: \n {2}",
					iParentUrl, iTargetDir, exp.Message);
				state = TDirState.Exception;
			} 
			finally 
			{
				if (reader != null) {
					reader.Close();
				}
				if (response != null) {
					response.Close();
				}
			}

			return state;
		}

		/// <summary>
		/// 在Ftp服务器上创建文件夹.
		/// </summary>
		/// <param name="iUrl">URL.</param>
		/// <param name="iAccountId">账户.</param>
		/// <param name="iPwd">密码.</param>
		private TDirState CreateDirOnFtpServer(
			string iUrl, string iAccountId, string iPwd) {

			FtpWebRequest ftpRequest = null;
			FtpWebResponse response = null;
			TDirState state = TDirState.None;

			try
			{
				Uri targetURI = new Uri(iUrl);
				ftpRequest = (FtpWebRequest)FtpWebRequest.Create(targetURI);
				ftpRequest.Credentials = new NetworkCredential(iAccountId, iPwd);
				ftpRequest.KeepAlive = false;
				ftpRequest.Method = WebRequestMethods.Ftp.MakeDirectory;
				ftpRequest.UseBinary = true;
				response = ftpRequest.GetResponse() as FtpWebResponse;

				state = TDirState.Created;
				Debug.LogFormat("[CreateDir Successed] Url:{0}", iUrl);
			}
			catch (WebException exp) 
			{
				Debug.LogErrorFormat ("[CreateDir Failed] Url:{0} \n WebException: \n {1}",
					iUrl, exp.Message);
				state = TDirState.Exception;
			}
			catch(IOException exp) 
			{
				Debug.LogErrorFormat ("[CreateDir Failed] Url:{0} \n IOException: \n {1}",
					iUrl, exp.Message);
				state = TDirState.Exception;
			}
			catch (Exception exp) 
			{
				Debug.LogErrorFormat ("[CreateDir Failed] Url:{0} \n Exception: \n {1}",
					iUrl, exp.Message);
				state = TDirState.Exception;
			} 
			finally 
			{
				if (response != null) {
					response.Close();
				}
			}
			return state;
		}

		/// <summary>
		/// 上传Bundle包依赖文件.
		/// </summary>
		/// <param name="iServerInfo">服务器信息.</param>
		private string UploadBundlesMapFile(UploadServerInfo iServerInfo) {

			// 导出文件
			string inputFilePath = BundlesMap.GetInstance ().ExportToJsonFile ();
			Debug.LogFormat("[Export] Uploader:BundlesMap.ExportToJsonFile(Path:{0})", inputFilePath);

			// 上传URL
			string uploadUrl = ServersConf.GetUploadListBaseUrl(iServerInfo);

			if (File.Exists (inputFilePath) == true) {

				int lastIndex = inputFilePath.LastIndexOf ("/");
				string fileName = inputFilePath.Substring (lastIndex + 1);
				uploadUrl = string.Format ("{0}/{1}", uploadUrl, fileName);

				// 上传Bundles列表信息文件
				this._State = UpLoadFileToFtpServer (
					uploadUrl, inputFilePath, iServerInfo.AccountId, iServerInfo.Pwd);
				if (TRunState.OK != this._State) {
					Debug.LogErrorFormat("[Upload Failed] BundlesMap UpLoadFileToFtpServer({0} -> {1})", inputFilePath, uploadUrl);
					return null;
				} else {
					this._uploadState = string.Format ("[Upload] {0}", fileName);
					return inputFilePath;
				}
			} else {
				Debug.LogErrorFormat("[Upload Failed] Upload file is not exist!!!(Path:{0})", inputFilePath);
				return null;
			}
		}

		/// <summary>
		/// 上传上传列表列表信息.
		/// </summary>
		/// <param name="iServerInfo">服务器信息.</param>
		private string UploadUploadListFile(UploadServerInfo iServerInfo) {

			// 导出Json文件，保存至(Resources/Conf)
			string inputFilePath = UploadList.GetInstance().ExportToJsonFile();
			Debug.LogFormat("[Export] Uploader:UploadList.ExportToJsonFile(Path:{0})", inputFilePath);

			// 打包信息URL
			string uploadUrl = ServersConf.GetUploadListBaseUrl(iServerInfo);

			if (File.Exists (inputFilePath) == true) {
				int lastIndex = inputFilePath.LastIndexOf ("/");
				string fileName = inputFilePath.Substring (lastIndex + 1);
				uploadUrl = string.Format ("{0}/{1}", uploadUrl, fileName);

				// 上传Bundles列表信息文件
				this._State = UpLoadFileToFtpServer (
					uploadUrl, inputFilePath, iServerInfo.AccountId, iServerInfo.Pwd);

				if (TRunState.OK != this._State) {
					Debug.LogErrorFormat("[Upload Failed] UploadList UpLoadFileToFtpServer({0} -> {1})", inputFilePath, uploadUrl);
					return null;
				} else {
					this._uploadState = string.Format ("[Upload] {0}", fileName);
					return inputFilePath;
				}

			} else {
				Debug.LogErrorFormat("[Upload Failed] Upload file is not exist!!!(Path:{0})", inputFilePath);
				return null;
			}
		}

		/// <summary>
		/// 往Ftp服务器上传文件.
		/// </summary>
		/// <param name="iUrl">URL.</param>
		/// <param name="iAccountId">账户.</param>
		/// <param name="iPwd">密码.</param>
		private TRunState UpLoadFileToFtpServer(
			string iUploadUrl, string iInputPath,
			string iAccountId, string iPwd) {

			FtpWebRequest ftpRequest = null;
			FtpWebResponse response = null;
			FileStream fileStream = null;
			Stream ftpStream = null;
			TRunState state = TRunState.OK;

			try
			{
				Uri targetURI = new Uri(iUploadUrl);
				ftpRequest = (FtpWebRequest)FtpWebRequest.Create(targetURI);
				ftpRequest.Credentials = new NetworkCredential(iAccountId, iPwd);
				ftpRequest.KeepAlive = false;
				ftpRequest.Method = WebRequestMethods.Ftp.UploadFile;
				ftpRequest.UseBinary = true;

				// 读取文件
				fileStream = File.OpenRead(iInputPath);  
				byte[] buffer = new byte[fileStream.Length];  
				fileStream.Read(buffer, 0, buffer.Length);  

				// 写入请求
				ftpStream = ftpRequest.GetRequestStream();  
				ftpStream.Write(buffer, 0, buffer.Length);  

				// 发出请求
				response = ftpRequest.GetResponse() as FtpWebResponse;

				Debug.LogFormat("[UpLoad Successed] UploadUrl:{0} \n InputPath:{1}", 
					iUploadUrl, iInputPath);

			}
			catch (WebException exp) 
			{
				Debug.LogErrorFormat ("[UpLoad Failed] UploadUrl:{0} \n InputPath:{1} \n WebException: \n {2}",
					iUploadUrl, iInputPath, exp.Message);
				state = TRunState.Exception;
			}
			catch(IOException exp) 
			{
				Debug.LogErrorFormat ("[UpLoad Failed] UploadUrl:{0} \n InputPath:{1} \n IOException: \n {2}",
					iUploadUrl, iInputPath, exp.Message);
				state = TRunState.Exception;
			}
			catch (Exception exp) 
			{
				Debug.LogErrorFormat ("[UpLoad Failed] UploadUrl:{0} \n InputPath:{1} \n Exception: \n {2}",
					iUploadUrl, iInputPath, exp.Message);
				state = TRunState.Exception;
			} 
			finally 
			{
				if (fileStream != null) {
					fileStream.Close();  
				}

				if (ftpStream != null) {
					ftpStream.Close();  
				}

				if (response != null) {
					response.Close();
				}
			}
			return state;
		}
		#endregion

		#region Upload Delegate

		/// <summary>
		/// 下载前通知事件委托（下载前提示用户的信息）.
		/// </summary>
		private void UploadPreConfirmNotification(int iTotalCount, long iTotalDataSize) {
			if ((this._uploadEvents != null) && (this._uploadEvents.OnPreConfirmNotification != null)) {
				this._uploadEvents.OnPreConfirmNotification.Invoke (iTotalCount, iTotalDataSize, UploadPreConfirm);
			}
		}

		/// <summary>
		/// 上传事前确认.
		/// </summary>
		/// <param name="iYesOrNo">是否确认上传（true:上传;false:不上传;）.</param>
		private void UploadPreConfirm(bool iYesOrNo) {
			if (iYesOrNo == false) {
				this.isStarted = false;
				return;
			}
			// 继续下载
			StartCoroutine (this.Upload ());
		}

		/// <summary>
		/// 上传取消确认委托.
		/// </summary>
		private void UploadCancelNotification() { 

			if ((this._uploadEvents != null) && (this._uploadEvents.OnCancelNotification != null)) {
				this._uploadEvents.OnCancelNotification.Invoke (UploadCancelConfirm);
			}

		}

		/// <summary>
		/// 上传取消确认.
		/// </summary>
		/// <param name="iYesOrNo">是否取消下载（true:取消下载;false:不取消下载;）.</param>
		private void UploadCancelConfirm(bool iYesOrNo) {
			if (iYesOrNo == true) {
				isCanceled = true;
				this.isStarted = false; 
				this._isCompleted = false;

				// 清空错误列表
				if(this._errors != null) {
					this._errors.Clear ();
				}
					
				// 导出已上传完成信息
				// 导出最新的下载列表信息
				UploadList.GetInstance().ExportToJsonFile();
			
				if (this._uploadBar != null) {
					this._uploadBar.Cancel ();
				}

				// 取消上传 重置状态：OK
				this._State = TRunState.OK;

				return;
			}

			// 继续上传
			StartCoroutine (UploadContinue ());
		}

		/// <summary>
		/// 上传完成确认.
		/// </summary>
		private void UploadCompleteNotification() {

			// 向服务器更新最新状态
			StartCoroutine(UpdateInfoToServer());

		}

		/// <summary>
		/// 向服务器更新最新状态.
		/// </summary>
		private IEnumerator UpdateInfoToServer() {

			UploadServerInfo server = ServersConf.GetInstance ().UploadServer;

			// 备份本地文件
			this.BackUpBundleFiles();
			yield return new WaitForSeconds (1.0f);

			// 开始上传Bunlde包依赖信息文件(Json文件)
			string bundlesMapFilePath = this.UploadBundlesMapFile(server);
			yield return new WaitForSeconds (1.0f);
			// 备份文件
			this.BackupFile (bundlesMapFilePath);
			yield return new WaitForSeconds (1.0f);

			// 开始上传Bundles列表信息(Json文件)
			string uploadListFilePath = this.UploadUploadListFile(server);
			yield return new WaitForSeconds (1.0f);
			// 备份文件
			this.BackupFile (uploadListFilePath);
			yield return new WaitForSeconds (1.0f);

			this.UploadDateTime = 0;
			if ((this._uploadEvents != null) && (this._uploadEvents.OnCompletedNotification != null)) {
				this._uploadEvents.OnCompletedNotification.Invoke ();
			}
			yield return null;
		}

		/// <summary>
		/// 上传检测
		/// </summary>
		private IEnumerator UploadCheck() {

			// 重置上传时间
			this.ResetUploadDateTime();
			yield return new WaitForEndOfFrame();

			// 保存硬件信息
			BuildInfo.GetInstance().DeviceInfo.Name = UnityEngine.SystemInfo.deviceName;
			yield return new WaitForEndOfFrame ();

			// 初始化上传进度计数器
			UploadList.GetInstance().InitProgressCounter();
			yield return new WaitForEndOfFrame();

			// 需要上传
			if (UploadList.GetInstance ().isUploadNecessary == true) {
				this.UploadPreConfirmNotification (
					UploadList.GetInstance ().GetTotalCount (),
					UploadList.GetInstance ().GetTotalDatasize ());
			} else {
				this.isStarted = false;
				this._isCompleted = true;

				// 无效上传 重置状态为：OK
				this._State = TRunState.OK;
			}
		}

		/// <summary>
		/// 继续上传.
		/// </summary>
		/// <returns>The check.</returns>
		private IEnumerator UploadContinue() {

			// 继续下载 重置状态：OK
			this._State = TRunState.OK;

			if (this._uploadBar != null) {
				this._uploadBar.Reset ();
			}
			yield return new WaitForEndOfFrame();

			// 开始上传
			yield return Upload();
			yield return new WaitForEndOfFrame();

			yield return null;
		}

		/// <summary>
		/// 开始下载.
		/// </summary>
		/// <param name="iState">上传状态.</param>
		public void OnUploadStart(string iState) {
			if (this._uploadBar == null) {
				return;
			}
			lock (_uploaderCountLock) {
				// 上传信息
				this._uploadState = iState;

				++this.UploaderCount;
			}
		}
			
		/// <summary>
		/// 上传失败委托.
		/// </summary>
		/// <param name="iUploader">上传器.</param>
		/// <param name="iTargetInfo">上传目标信息.</param>
		/// <param name="iIsManifest">Manifest文件标志位.</param>
		/// <param name="iError">错误信息.</param>
		public void OnUploadFailed(Uploader iUploader, UploadItem iTargetInfo, bool iIsManifest, List<ErrorDetail> iErrors) {

			lock (_errorLock) {
				
				Debug.LogErrorFormat ("[UploadFailed] No:{0} ID:{1} Mode:{2} Manifest:{3} UploadStatus(Bundle:{4} Manifest:{5} Retries:{6})",
					iTargetInfo.No, iTargetInfo.ID, iTargetInfo.BuildMode.ToString(), 
					iIsManifest.ToString(), iTargetInfo.isBundleUploaded.ToString(),
					iTargetInfo.isManifestUploaded.ToString());
				
				this._errors.AddRange(iErrors);
				this._State = TRunState.Error;

				if (iUploader != null) {
					iUploader.Dispose ();
					GC.Collect ();
				}
			}


		}

		/// <summary>
		/// 上传成功委托.
		/// </summary>
		/// <param name="iUploader">上传器.</param>
		/// <param name="iTargetInfo">下载目标信息.</param>
		/// <param name="iIsManifest">manifest标志位.</param>
		/// <param name="iRetries">剩余重试次数.</param>
		public void OnUploadSuccessed(Uploader iUploader, UploadItem iTargetInfo, bool iIsManifest, int iRetries) {

			Debug.LogFormat ("[UploadSuccessed] No:{0} ID:{1} Mode:{2} Manifest:{3} UploadStatus(Bundle:{4} Manifest:{5} Retries:{6})",
				iTargetInfo.No, iTargetInfo.ID, iTargetInfo.BuildMode.ToString(), 
				iIsManifest.ToString(), iTargetInfo.isBundleUploaded.ToString(),
				iTargetInfo.isManifestUploaded.ToString(), iRetries.ToString());

			UploadList.GetInstance ().UploadCompleted (iTargetInfo.No, iIsManifest);

			if (iTargetInfo.isAllUploaded == true) {

				if (iUploader != null) {
					iUploader.Dispose ();
					GC.Collect ();
				}

				lock (_uploaderCountLock) {
					--this.UploaderCount;
				}
			}
		}

		#endregion
	}
}
