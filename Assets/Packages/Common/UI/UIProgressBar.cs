using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Upload;

namespace Common {

	/// <summary>
	/// 取得进度条进度事件委托（0.0f~1.0f）.
	/// </summary>
	public delegate float GetProgressRate();

	/// <summary>
	/// 取得进度条状态事件委托.
	/// </summary>
	public delegate string GetProgressState();

	/// <summary>
	/// 取得总数事件委托.
	/// </summary>
	public delegate int GetTotalCount();

	/// <summary>
	/// 取得完成总数事件委托.
	/// </summary>
	public delegate int GetCompletedCount();

	/// <summary>
	/// 下载Bar.
	/// </summary>
	public class UIProgressBar : MonoBehaviour {

		#region UI
		/// <summary>
		/// 下载Bar进度条图片.
		/// </summary>
		public Image _barImage = null;

		/// <summary>
		/// 下载用的Tips.
		/// </summary>
		public Text _tipsTxt = null;
		/// <summary>
		/// 详细信息文本.
		/// </summary>
		public Text _detailTxt = null;
		/// <summary>
		/// 现在进度文本.
		/// </summary>
		public Text _progressTxt = null;
		#endregion

		#region Delegate

		/// <summary>
		/// 取得进度条进度事件委托（0.0f~1.0f）.
		/// </summary>
		private GetProgressRate _getProgressRate = null;

		/// <summary>
		/// 取得进度条状态事件委托.
		/// </summary>
		private GetProgressState _getProgressState = null;

		/// <summary>
		/// 取得总数事件委托.
		/// </summary>
		private GetTotalCount _getTotalCount = null;

		/// <summary>
		/// 取得完成总数事件委托.
		/// </summary>
		private GetCompletedCount _getCompletedCount = null;

		/// <summary>
		/// 进度条进度（0.0f~1.0f）.
		/// </summary>
		/// <value>进度条进度（0.0f~1.0f）.</value>
		private float Progress {
			get { 
				if (this._getProgressRate != null) {
					return this._getProgressRate ();
				}
				return 0.0f;
			}
		}

		/// <summary>
		/// 总数.
		/// </summary>
		/// <value>总数.</value>
		private int TotalCount {
			get { 
				if (this._getTotalCount != null) {
					return this._getTotalCount ();
				} else {
					return 0;
				}
			}
		}

		/// <summary>
		/// 完成数.
		/// </summary>
		/// <value>完成数.</value>
		private int CompletedCount {
			get { 
				if (this._getCompletedCount != null) {
					return this._getCompletedCount ();
				}
				return 0;
			}
		}

		#endregion

		/// <summary>
		/// 被取消标志位.
		/// </summary>
		/// <value><c>true</c> 被取消了; 没有被取消, <c>false</c>.</value>
		protected bool isCanceled { get; private set; }

		/// <summary>
		/// 变化时间.
		/// </summary>
		/// <value>变化时间.</value>
		public static float _deltaTime { get; private set; }
		/// <summary>
		/// 允许计算变化时间标志位.
		/// </summary>
		private bool _isDeltaTimeOK = false;

		/// <summary>
		/// 已完成百分比.
		/// </summary>
		private float _completedPercent = 0.0f;

		/// <summary>
		/// Awake this instance.
		/// </summary>
		void Awake() {
			
			// 初始化
			this.Init();

			if(this._detailTxt != null) {
				this._detailTxt.text = "";
#if DEVELOPMENT_BUILD
				this._detailTxt.gameObject.SetActive(true);
#else
				this._detailTxt.gameObject.SetActive(true);
#endif
			}
		}

		/// <summary>
		/// 初始化进度条.
		/// </summary>
		public void Init() {

			this._completedPercent = 0.0f;
			_deltaTime = 0.0f;

			// 更新进度条
			this.UpdateByProgressRate(this.Progress);

		}

		/// <summary>
		/// 设置委托.
		/// </summary>
		/// <param name="iGetProgressRate">取得进度条进度事件委托（0.0f~1.0f）.</param>
		/// <param name="iGetProgressState">取得进度条状态事件委托.</param>
		/// <param name="iGetTotalCount">取得总数事件委托.</param>
		/// <param name="iGetCompletedCount">取得完成总数事件委托.</param>
		public void SetDelegate(
			GetProgressRate iGetProgressRate, GetProgressState iGetProgressState,
			GetTotalCount iGetTotalCount, GetCompletedCount iGetCompletedCount) {
			this._getProgressRate = iGetProgressRate;
			this._getProgressState = iGetProgressState;
			this._getTotalCount = iGetTotalCount;
			this._getCompletedCount = iGetCompletedCount;
		}

		/// <summary>
		/// 取消.
		/// </summary>
		public void Cancel() {
			this.isCanceled = true;
		}

		/// <summary>
		/// 下载进度条重置.
		/// </summary>
		public void Reset() {

			// 初始化
			this.Init ();

		}

		/// <summary>
		/// 根据进度，更新详细信息.
		/// </summary>
		/// <param name="iPercent">下载百分比.</param>
		private void UpdateByProgressRate(float iPercent = 0.0f) {
			int totalCount = this.TotalCount;
			int completedCount = this.CompletedCount;

			// 进度条
			if (this._barImage != null) {
				Vector3 scale = this._barImage.transform.localScale;
				scale.x = iPercent;
				this._barImage.transform.localScale = scale;
			}
				
			if (this._progressTxt != null) {
				float completedPercent = iPercent * 100.0f;
				this._progressTxt.text = string.Format("{0}/{1}({2:N1}%)", 
					completedCount, totalCount, completedPercent);
			}

			string state = null;
			if (this._getProgressState != null) {
				state = this._getProgressState ();
			}
			// 更新详细信息
			this.UpdateState(state);

		}

		/// <summary>
		/// 更新详细信息.
		/// </summary>
		/// <param name="iDownloadUrl">下载Url.</param>
		private void UpdateState(string iState) {
			if (string.IsNullOrEmpty (iState) == true) {
				return;
			}
			if (this._detailTxt == null) {
				return;
			}
			if (this._completedPercent >= 1.0f) {
				this._detailTxt.text = "Completed!!!!!!";
			} else {
				this._detailTxt.text = string.Format ("[{0:N2} s]{1}",
					_deltaTime, iState);
			}
		}

		/// <summary>
		/// 更新函数.
		/// </summary>
		void Update () {

			// 被取消
			if (this.isCanceled == true) {
				this._isDeltaTimeOK = false;
			}

			// 进度超过1.0f(100%)
			if (this._completedPercent >= 1.0f) {
				this._isDeltaTimeOK = false;
			} else if (this._completedPercent > 0) {
				this._isDeltaTimeOK = true;
			}

			// 下载所花时间计数
			if (this._isDeltaTimeOK == true) {
				_deltaTime += Time.deltaTime;
			}

			// 下载完毕
			float progress = this.Progress;
			if (progress == this._completedPercent) {
				return;
			}
			// 更新进度信息
			this.UpdateByProgressRate(progress);

			this._completedPercent = progress;
		}
			
	}

	/// <summary>
	/// 运行状态.
	/// </summary>
	public enum TRunState {
		/// <summary>
		/// OK.
		/// </summary>
		OK,
		/// <summary>
		/// 没有上传目标.
		/// </summary>
		NoUploadTarget,
		/// <summary>
		/// 没有下载目标.
		/// </summary>
		NoDownloadTarget,
		/// <summary>
		/// 服务器信息取得失败.
		/// </summary>
		GetServerInfoFailed,
		/// <summary>
		/// 检测目录错误.
		/// </summary>
		CheckDirFailed,
		/// <summary>
		/// 从服务器上取得文件大小失败.
		/// </summary>
		GetFileSizeFromServerFailed,
		/// <summary>
		/// 被取消.
		/// </summary>
		Canceled,
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
	/// 错误类型.
	/// </summary>
	public enum TErrorType {
		/// <summary>
		/// 无.
		/// </summary>
		None,
		/// <summary>
		/// 系统错误
		/// </summary>
		SysError,
		/// <summary>
		/// 系统异常
		/// </summary>
		SysException,
		/// <summary>
		/// www错误
		/// </summary>
		WWWError,
		/// <summary>
		/// www异常
		/// </summary>
		WWWException,
		/// <summary>
		/// http错误
		/// </summary>
		HttpError,
		/// <summary>
		/// http异常
		/// </summary>
		HttpException,
		/// <summary>
		/// 上传失败.
		/// </summary>
		UploadFailed,
		/// <summary>
		/// 文件Check失败（Md5等）.
		/// </summary>
		FileCheckFailed
	}
		
	/// <summary>
	/// 错误详细.
	/// </summary>
	public struct ErrorDetail {
		/// <summary>
		/// 失败类型.
		/// </summary>
		public TErrorType Type;
		/// <summary>
		/// 运行状况.
		/// </summary>
		public TRunState State;
		/// <summary>
		/// 详细信息.
		/// </summary>
		public string Detail;
		/// <summary>
		/// 重试次数.
		/// </summary>
		public int Retries;
	}

	/// <summary>
	/// 下载确认事件委托.
	/// </summary>
	public delegate void ConfirmHandle(bool iYesOrNo);

	/// <summary>
	/// 进度时间触发器.
	/// </summary>
	[System.Serializable]
	public class ProgressEventTrigger {
		/// <summary>
		/// 实现确认通知事件委托（如：下载前提示用户的信息）.
		/// </summary>
		[System.Serializable] 
		public class PreConfirmNotificationEvent:UnityEvent<int, long, ConfirmHandle> { }

		/// <summary>
		/// 完成确认事件委托.
		/// </summary>
		[System.Serializable] 
		public class CompeletedNotificationEvent:UnityEvent { }

		/// <summary>
		/// 取消通知确认事件委托.
		/// </summary>
		[System.Serializable] 
		public class CancelNotificationEvent:UnityEvent<ConfirmHandle> { }

		/// <summary>
		/// 错误确认事件委托.
		/// </summary>
		[System.Serializable] 
		public class ErrorsNotificationEvent:UnityEvent<List<ErrorDetail>> { }

		/// <summary>
		/// 事先确认通知事件委托（如：下载前提示用户的信息）.
		/// </summary>
		public PreConfirmNotificationEvent OnPreConfirmNotification = null;

		/// <summary>
		/// 完成确认事件委托.
		/// </summary>
		public CompeletedNotificationEvent OnCompletedNotification = null;

		/// <summary>
		/// 取消通知确认事件委托.
		/// </summary>
		public CancelNotificationEvent OnCancelNotification = null;

		/// <summary>
		/// 错误通知确认事件委托.
		/// </summary>
		public ErrorsNotificationEvent OnErrorsNotification = null;
	}

	/// <summary>
	/// 进度计数器.
	/// </summary>
	public class ProgressCounter {

		/// <summary>
		/// 文件下载总数.
		/// </summary>
		public int TotalCount { get; private set; }

		/// <summary>
		/// 已完成总数.
		/// </summary>
		public int DidCount { get; private set; }

		/// <summary>
		/// 数据总大小（单位：byte）.
		/// </summary>
		public long TotalDatasize { get; private set; }

		/// <summary>
		/// 已完成数据总大小（单位：byte）.
		/// </summary>
		public long DidDatasize { get; private set; }

		/// <summary>
		/// 进度百分比.
		/// </summary>
		public float Progress {
			get {
				// 总大小为0 -》 无需下载。
				if (this.TotalDatasize <= 0) {
					return 0.0f;
				}
				//				Debug.LogFormat ("DownloadDetail:{0}/{1} DataSize {2}/{3}",
				//					this.DownloadedCount, this.TotalCount, this.DownloadedDatasize, this.TotalDatasize);
				return ((float)this.DidDatasize / (float)this.TotalDatasize);
			}
		}

		/// <summary>
		/// 清空.
		/// </summary>
		public void Clear() {
			this.TotalCount = 0;
			this.DidCount = 0;
			this.TotalDatasize = 0;
			this.DidDatasize = 0;
		}

		/// <summary>
		/// 初始化.
		/// </summary>
		/// <param name="iTotalCount">I total count.</param>
		/// <param name="iTotalDatasize">I total datasize.</param>
		public void Init(int iTotalCount, long iTotalDatasize) {
			this.TotalCount = iTotalCount;
			this.DidCount = 0;
			this.TotalDatasize = iTotalDatasize;
			this.DidDatasize = 0;

			Debug.LogFormat ("ProgressCounter.Init: {0}/{1} {2}/{3}",
				this.DidCount, this.TotalCount,
				this.DidDatasize, this.TotalDatasize);
		}

		/// <summary>
		/// 更新已经完成计数.
		/// </summary>
		public void UpdateCompletedCount() {
			++this.DidCount;
		}

		/// <summary>
		/// 更新已经完成数据大小.
		/// </summary>
		/// <param name="iDataSize">数据大小.</param>
		public void UpdateCompletedDataSize(long iDataSize) {
			this.DidDatasize += iDataSize;
		}
	}
}