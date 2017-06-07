using UnityEngine;
using System;
using System.IO;
using System.Net;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using AssetBundles;
using BuildSystem;
using Common;

namespace Upload {

	/// <summary>
	/// 开始上传委托.
	/// </summary>
	public delegate void OnStart(string iPath);

	/// <summary>
	/// 上传失败委托.
	/// </summary>
	/// <param name="iUploader">上传器.</param>
	/// <param name="iTargetInfo">上传目标信息.</param>
	/// <param name="iIsManifest">Manifest文件标志位.</param>
	/// <param name="iError">错误信息.</param>
	public delegate void OnFailed(Uploader iUploader, UploadItem iTargetInfo, bool iIsManifest, List<ErrorDetail> iErrors);

	/// <summary>
	/// 上传成功委托.
	/// </summary>
	/// <param name="iUploader">上传器.</param>
	/// <param name="iTargetInfo">下载目标信息.</param>
	/// <param name="iIsManifest">manifest标志位.</param>
	/// <param name="iRetries">剩余重试次数.</param>
	public delegate void OnSuccessed(Uploader iUploader, UploadItem iTargetInfo, bool iIsManifest, int iRetries);
		
	/// <summary>
	/// 目录状态.
	/// </summary>
	public enum TDirState {
		/// <summary>
		/// 无.
		/// </summary>
		None,
		/// <summary>
		/// 存在.
		/// </summary>
		Exist,
		/// <summary>
		/// 不存在.
		/// </summary>
		NotExist,
		/// <summary>
		/// 已创建.
		/// </summary>
		Created,
		/// <summary>
		/// 错误.
		/// </summary>
		Error,
		/// <summary>
		/// 异常.
		/// </summary>
		Exception
	}

	/// <summary>
	/// 上传者.
	/// </summary>
	public sealed class Uploader : IDisposable {

		/// <summary>
		/// 服务器信息.
		/// </summary>
		private UploadServerInfo _server =  null;

		/// <summary>
		/// 上传方式.
		/// </summary>
		private TUploadWay _uploadWay = TUploadWay.Ftp;

		/// <summary>
		/// 运行状态.
		/// </summary>
		private TRunState _state = TRunState.OK;

		/// <summary>
		/// 上传目标.
		/// </summary>
		private UploadItem _target = null;

		/// <summary>
		/// 上传BaseUrl.
		/// </summary>
		private string UploadBaseUrl { get; set; }

		/// <summary>
		/// Bunlde文件名.
		/// </summary>
		private string BunldeFileName { get; set; }

		/// <summary>
		/// Manifest文件名.
		/// </summary>
		private string ManifestFileName { get; set; }

		/// <summary>
		/// 重上传次数.
		/// </summary>
		/// <value>The retries.</value>
		private int Retries { get; set;}

		/// <summary>
		/// 子线程(Bundle).
		/// </summary>
		private Thread _bundleThread = null;

		/// <summary>
		/// 子线程(manifest).
		/// </summary>
		private Thread _manifestThread = null;

		/// <summary>
		/// 错误一览.
		/// </summary>
		private List<ErrorDetail> _errors = new List<ErrorDetail> ();
		private static object _uploaderErrorLock = new object ();

		#region Delegate 

		/// <summary>
		/// 开始上传委托.
		/// </summary>
		private OnStart _onStart = null;
		/// <summary>
		/// 上传失败委托.
		/// </summary>
		private OnFailed _onFailed = null;
		/// <summary>
		/// 上传成功委托.
		/// </summary>
		private OnSuccessed _onSuccessed = null;

		#endregion

		/// <summary>
		/// 取消标志位.
		/// </summary>
		public bool isCanceled { 
			get { 
				if (UploadManager.isCanceled == true) {
					_state = TRunState.Canceled;
				}
				return (TRunState.Canceled == _state);
			}
		}

		/// <summary>
		/// 失败标志位.
		/// </summary>
		public bool isFailed {
			get { 
				return ((TRunState.GetServerInfoFailed == _state) || 
					(TRunState.CheckDirFailed == _state)|| 
					(TRunState.Error == _state)|| 
					(TRunState.Exception == _state));
			}
		}

		/// <summary>
		/// 创建上传者.
		/// </summary>
		/// <param name="iTarget">上传目标.</param>
		/// <param name="iOnStart">开始上传委托.</param>
		/// <param name="iOnFailed">上传失败委托.</param>
		/// <param name="iOnSuccessed">上传成功委托.</param>
		/// <param name="iUploadWay">上传方式.</param>
		public static Uploader Create(
			UploadItem iTarget,
			OnStart iOnStart,
			OnFailed iOnFailed,
			OnSuccessed iOnSuccessed,
			TUploadWay iUploadWay = TUploadWay.Ftp) {

			Uploader objRet = new Uploader ();
			objRet.Init (iTarget, iOnStart, iOnFailed, iOnSuccessed, iUploadWay);

			return objRet;
		}

		/// <summary>
		/// 释放函数.
		/// </summary>
		public void Dispose() {
			if (_bundleThread != null) {
				_bundleThread.Abort ();
			}
			if (this._manifestThread != null) {
				this._manifestThread.Abort ();
			}
		}

		/// <summary>
		/// 取消上传  .
		/// </summary>
		public void Cancel() {
			this._state = TRunState.Canceled;
		}

		#region Thread

		/// <summary>
		/// 开始上传Bundle文件.
		/// </summary>
		public IEnumerator UploadBundleFile() {

			this._state = TRunState.OK;

			if (_bundleThread != null) {
				// 终止线程
				_bundleThread.Abort ();
				_bundleThread = null;
			}
			yield return new WaitForEndOfFrame ();

			if (this._target != null) {
				
				//开启子线程下载,使用匿名方法
				_bundleThread = new Thread ( delegate() {

					while (this.Retries >0) {
						if(this.isCanceled == true) {
							break;
						}

						// Bundle文件
						if(this._target.isBundleUploaded == false) {
							if(this.UpLoadBundleFileToServer(false) == false) {
								--this.Retries;
								continue;
							} 
						}
					}
				});
				yield return new WaitForEndOfFrame ();

				//开启子线程
				_bundleThread.IsBackground = true;
				_bundleThread.Start ();
			}

			if (TRunState.OK != this._state) {
				if (this._onFailed != null) {
					this._onFailed (this, this._target, false, this._errors);
				}
			} else {
				lock (_uploaderErrorLock) {
					this._errors.Clear ();
				}
			}

			yield return null;

		}

		/// <summary>
		/// 开始上传Manifest文件.
		/// </summary>
		public IEnumerator UploadManifestFile() {

			this._state = TRunState.OK;

			if (_manifestThread != null) {
				// 终止线程
				_manifestThread.Abort ();
				_manifestThread = null;
			}
			yield return new WaitForEndOfFrame ();

			if (this._target != null) {

				//开启子线程下载,使用匿名方法
				_manifestThread = new Thread ( delegate() {

					while (this.Retries >0) {
						if(this.isCanceled == true) {
							break;
						}

						// Manifest文件不需要上传
						if(UploadManager.ManifestUpload == false) {
							break;
						}

						// Scene文件（没有Manifest文件）
						if(TBundleType.Scene ==  this._target.BundleType)
						{
							break;
						}

						// Manifest文件
						if(this._target.isManifestUploaded == false) {
							if(this.UpLoadBundleFileToServer(true) == false) {
								--this.Retries;
								continue;
							}
						}
					}
				});
				yield return new WaitForEndOfFrame ();

				//开启子线程
				_manifestThread.IsBackground = true;
				_manifestThread.Start ();
			}
				
			if (TRunState.OK != this._state) {
				if (this._onFailed != null) {
					this._onFailed (this, this._target, false, this._errors);
				}
			} else {
				lock (_uploaderErrorLock) {
					this._errors.Clear ();
				}
			}

			yield return null;
		}

		#endregion

		/// <summary>
		/// 初始化.
		/// </summary>
		/// <param name="iTarget">上传目标.</param>
		/// <param name="iOnStart">开始上传委托.</param>
		/// <param name="iOnFailed">上传失败委托.</param>
		/// <param name="iOnSuccessed">上传成功委托.</param>
		/// <param name="iUploadWay">上传方式.</param>
		private void Init(
			UploadItem iTarget,
			OnStart iOnStart,
			OnFailed iOnFailed,
			OnSuccessed iOnSuccessed,
			TUploadWay iUploadWay = TUploadWay.Ftp) {

			this._target = iTarget;
			this._onStart = iOnStart;
			this._onFailed = iOnFailed;
			this._onSuccessed = iOnSuccessed;
			this._uploadWay = iUploadWay;
			this.Retries = ServersConf.GetInstance().NetRetries;

			if (this._server == null) {
				this._server = ServersConf.GetInstance ().UploadServer;
			}
			this.UploadBaseUrl = ServersConf.GetBundleUploadBaseURL (this._server, this._target);
			this.BunldeFileName = UploadList.GetLocalBundleFileName (this._target.ID, this._target.FileType, false);
			this.ManifestFileName = UploadList.GetLocalBundleFileName (this._target.ID, this._target.FileType, true);
		}

		#region Ftp 处理

		/// <summary>
		/// 向服务器上传Bundle文件.
		/// </summary>
		/// <param name="iIsManifest">Manifest标志位.</param>
		private bool UpLoadBundleFileToServer(bool iIsManifest) {

			if (string.IsNullOrEmpty (this.UploadBaseUrl) == true) {
				return false;
			}
			if (this._target == null) {
				return false;
			}
			if (this._server == null) {
				return false;
			}

			// 上传Bundle文件
			string uploadBundleUrl = null;
			if (iIsManifest == true) {
				uploadBundleUrl = string.Format ("{0}/{1}", this.UploadBaseUrl, this.ManifestFileName);
			} else {
				uploadBundleUrl = string.Format ("{0}/{1}", this.UploadBaseUrl, this.BunldeFileName);
			}
			string inputFilePath = UploadList.GetLocalBundleFilePath(
				this._target.ID, this._target.FileType, iIsManifest, (TBundleType.Scene == this._target.BundleType));

			if (File.Exists (inputFilePath) == true) {

				if (this._onStart != null) {
					if (iIsManifest == true) {
						this._onStart (string.Format("[Upload][{0}]:{1}",
							this._target.BuildMode.ToString(), this.ManifestFileName));
					} else {
						this._onStart (string.Format("[Upload][{0}]:{1}",
							this._target.BuildMode.ToString(), this.BunldeFileName));
					}
				}

				if (TUploadWay.Ftp == this._uploadWay) {
					this._state = this.UpLoadFileToFtpServer (
						uploadBundleUrl, inputFilePath, this._server.AccountId, this._server.Pwd);
					if (TRunState.OK != this._state) {

						ErrorDetail error = new ErrorDetail ();
						error.Type = TErrorType.UploadFailed;
						error.State = this._state;
						error.Detail = string.Format ("[Upload Failed] {0} -> {1}",
							inputFilePath, uploadBundleUrl);
						error.Retries = this.Retries;
						this._errors.Add (error);
						return false;
					} else {
						if (iIsManifest) {
							this._target.SetStatus (TUploadStatus.ManifestUploaded);
						} else {
							this._target.SetStatus (TUploadStatus.BundleUploaded);
						}
						if (this._onSuccessed != null) {
							this._onSuccessed (this, this._target, iIsManifest, this.Retries);
						}
					}
				}
			}
			return true;
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

//				Debug.LogFormat("[UpLoad Successed] UploadUrl:{0} \n InputPath:{1}", 
//					iUploadUrl, iInputPath);

			}
			catch (WebException exp) 
			{
				Debug.LogErrorFormat ("[UpLoad Failed] Retries:{0} \n UploadUrl:{1} \n InputPath:{2} \n WebException: \n {3}",
					this.Retries, iUploadUrl, iInputPath, exp.Message);
				state = TRunState.Exception;
			}
			catch(IOException exp) 
			{
				Debug.LogErrorFormat ("[UpLoad Failed] Retries:{0} \n UploadUrl:{1} \n InputPath:{2} \n WebException: \n {3}",
					this.Retries, iUploadUrl, iInputPath, exp.Message);
				state = TRunState.Exception;
			}
			catch (Exception exp) 
			{
				Debug.LogErrorFormat ("[UpLoad Failed] Retries:{0} \n UploadUrl:{1} \n InputPath:{2} \n WebException: \n {3}",
					this.Retries, iUploadUrl, iInputPath, exp.Message);
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


	}
}
