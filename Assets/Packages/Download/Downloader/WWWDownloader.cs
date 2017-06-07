using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Threading;
using Upload;
using BuildSystem;
using Common;

namespace Download {

	/// <summary>
	/// 下载状态.
	/// </summary>
	public enum TDownloadState
	{
		Init = 0,
		Start,
		Failed,
		SuccessCompleted,
		Max
	}

	/// <summary>
	/// 下载器（WWW）.
	/// </summary>
	public sealed class WWWDownloader : DownloaderBase
	{

		#region Creator

		/// <summary>
		/// 创建Downloader（Http）.
		/// </summary>
		/// <param name="iDownloadUrl">下载Url.</param>
		/// <param name="iOnStart">开始事件委托.</param>
		/// <param name="iOnSuccessed">成功事件委托.</param>
		/// <param name="iOnFailed">失败事件委托.</param>
		/// <param name="iType">下载对象类型.</param>
		public static WWWDownloader Create(
			string iDownloadUrl, OnStart iOnStart,
			OnSuccessedByUrl iOnSuccessed, OnFailedByUrl iOnFailed, 
			TargetType iType = TargetType.Bundle) {
			WWWDownloader downloader = new WWWDownloader ();
			if (downloader != null) {
				// 初始化
				downloader.Init (iDownloadUrl, iOnStart, iOnSuccessed, iOnFailed, iType);
				return downloader;
			} else {
				Debug.LogError ("Downloader Create failed!!");
				return null;
			}
		}
			
		/// <summary>
		/// 创建Downloader.
		/// </summary>
		/// <param name="iTargetInfo">下载目标.</param>
		/// <param name="iOnStart">开始委托回调.</param>
		/// <param name="iOnSuccessed">成功委托回调.</param>
		/// <param name="iOnFailed">失败委托回调.</param>
		/// <param name="iRetries">重下载次数.</param>
		/// <param name="iTimeOut">超时时间（单位：秒）.</param>
		public static WWWDownloader Create(
			DownloadTargetInfo iTargetInfo, OnStart iOnStart,
			OnSuccessed iOnSuccessed, OnFailed iOnFailed) {
			WWWDownloader downloader = new WWWDownloader ();
			if (downloader != null) {
				// 初始化
				downloader.Init (iTargetInfo, iOnStart, iOnSuccessed, iOnFailed);
				return downloader;
			} else {
				Debug.LogError ("Downloader Create failed!!");
				return null;
			}
		}

		#endregion


		#region Implement

		/// <summary>
		/// 异步下载(Url)
		/// </summary>
		/// <param name="iIsFileClean">文件清空标志位.</param>
		public override IEnumerator AsynDownLoadByUrl (bool iIsFileClean = false) {

			if ((string.IsNullOrEmpty (this.DownloadBaseUrl) == false) && 
				(string.IsNullOrEmpty (this.FileName) == false)) {

				if (iIsFileClean == true) {
					string fileFullPath = string.Format ("{0}/{1}", this.BundlesDownloadDir, this.FileName);
					if (File.Exists (fileFullPath) == true) {
						File.Delete (fileFullPath);
					}
				}

				// 下载开始
				if (this.onStart != null) {
					this.onStart (this.FileName);
				}

				while (this.Retries >= 0) {
					
					// 下载被打断
					if (DownloadManager.isCanceled == true) {
						this._State = TRunState.Canceled;
						break;
					}

					yield return this.DownloadFileByWWW (this.DownloadBaseUrl, this.FileName);
					yield return new WaitForSeconds (0.1f);

					if (TRunState.OK == this._State) {
						if (this.onSuccessedByUrl != null) {
							this.onSuccessedByUrl(this, string.Format("{0}/{1}", this.DownloadBaseUrl, this.FileName));
						}
						break;
					} else {
						--this.Retries;
						yield return new WaitForEndOfFrame();
						continue;
					}

				}

				if ((TRunState.OK != this._State) &&
				   (TRunState.Canceled != this._State)) {
					if (this.onFailedByUrl != null) {
						this.onFailedByUrl (this, string.Format ("{0}/{1}", this.DownloadBaseUrl, this.FileName), this._errors);
					}
				}

			}
			yield return null;
		}

		/// <summary>
		/// 异步下载目标
		/// </summary>
		public override IEnumerator AsynDownLoadTarget () {

			if ((this._target != null) &&
				(string.IsNullOrEmpty (this.DownloadBaseUrl) == false) && 
				(string.IsNullOrEmpty (this.FileName) == false)) {

				if (this.onStart != null) {
					this.onStart (this.FileName);
				}

				while (this.Retries >= 0) {
					
					// 下载被打断
					if (DownloadManager.isCanceled == true) {
						this._State = TRunState.Canceled;
						break;
					}
						
					yield return this.DownloadFileByWWW (this.DownloadBaseUrl, this.FileName);
					yield return new WaitForSeconds (0.1f);

					if (TRunState.OK == this._State) {
						if (this.onSuccessed != null) {
							this.onSuccessed(this, this._target, false);
						}
						break;
					} else {
						--this.Retries;
						yield return new WaitForEndOfFrame();
						continue;
					}

				}

				if ((TRunState.OK != this._State) &&
					(TRunState.Canceled != this._State)) {
					if (this.onFailed != null) {
						this.onFailed (this, this._target, false, this._errors);
					}
				}
			}
			yield return null;
		}

		/// <summary>
		/// 异步下载目标的Manifest
		/// </summary>
		public override IEnumerator AsynDownLoadTargetManifest () {

			if ((DownloadManager.ManifestUpload == true) &&
				(this._target != null) &&
				(string.IsNullOrEmpty (this.DownloadBaseUrl) == false) && 
				(string.IsNullOrEmpty (this.ManifestFileName) == false)) {

				if (this.onStart != null) {
					this.onStart (this.ManifestFileName);
				}

				while (this.Retries >= 0) {
					// 下载被打断
					if (DownloadManager.isCanceled == true) {
						this._State = TRunState.Canceled;
						break;
					}
						
					yield return this.DownloadFileByWWW (this.DownloadBaseUrl, this.ManifestFileName, true);
					yield return new WaitForSeconds (0.1f);

					if (TRunState.OK == this._State) {
						if (this.onSuccessed != null) {
							this.onSuccessed(this, this._target, true);
						}
						break;
					} else {
						--this.Retries;
						yield return new WaitForEndOfFrame();
						continue;
					}
				}
					
				if ((TRunState.OK != this._State) &&
					(TRunState.Canceled != this._State)) {
					if (this.onFailed != null) {
						this.onFailed (this, this._target, true, this._errors);
					}
				}
			}
			yield return null;
		}

		/// <summary>
		/// 子线程下载(Url)
		/// </summary>
		public override void ThreadDownLoadByUrl() {
		}

		/// <summary>
		/// 子线程下载目标
		/// </summary>
		public override void ThreadDownLoadTarget() {
		}

		/// <summary>
		/// 子线程下载目标的Manifest
		/// </summary>
		public override void ThreadDownLoadTargetManifest() {
		}

		#endregion
	}
		
}