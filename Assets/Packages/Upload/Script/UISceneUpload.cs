using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using Common;
using AssetBundles;

namespace Upload {

	/// <summary>
	/// 上传场景.
	/// </summary>
	public class UISceneUpload : UIProgressProtocol {

		#region ButtonClick

		/// <summary>
		/// 点击上传按钮.
		/// </summary>
		public void OnUploadBtnClick() {
			Debug.Log ("--> OnUploadBtnClick");
			UploadManager manager = this.GetComponent<UploadManager> ();
			if (manager == null) {
				return;
			}
			if (manager.isStarted == true) {
				return;
			}
			manager.StartUpload ();
		}

		/// <summary>
		/// 点击取消按钮.
		/// </summary>
		public void OnCancelBtnClick() {
			Debug.Log ("--> OnCancelBtnClick");

			UploadManager manager = this.GetComponent<UploadManager> ();
			if (manager == null) {
				return;
			}
			manager.UploadCancel ();

		}
		#endregion

		#region Implement
		/// <summary>
		/// 事先通知事件委托（如：下载前提示用户的信息）.
		/// </summary>
		/// <param name="iTotalCount">下载总数.</param>
		/// <param name="iTotalDataSize">下载数据总大小.</param>
		public override void PreConfirmNotification(int iTotalCount, long iTotalDataSize) {
			Debug.LogFormat ("PreConfirmNotification: TotalCount:{0} TotalDataSize:{1}", 
				iTotalCount, iTotalDataSize);
			if (this._preConfirm != null) {
				this._preConfirm (true);
			}
		}

		/// <summary>
		/// 完成事件委托.
		/// </summary>
		public override void CompletedNotification() {
			Debug.Log ("CompletedNotification -> SceneDownload");

			// 加载场景
			SceneManager.LoadScene ("SceneDownload");
		}

		/// <summary>
		/// 取消通知确认事件委托.
		/// </summary>
		public override void CancelNotification() {
			Debug.Log ("DownloadCancelNotification");
			if (this._cancelConfirm != null) {
				this._cancelConfirm (true);
			}
		}

		/// <summary>
		/// 错误通知确认事件委托.
		/// </summary>
		/// <param name="iError">错误.</param>
		public override void ErrorNotification(
			List<ErrorDetail> iErrors) {

			foreach (ErrorDetail Loop in iErrors) {
				Debug.LogErrorFormat ("[DownloadErrorNotification] Type:{0} Detail:{1} Retries:{2}",
					Loop.Type.ToString (), Loop.Detail, Loop.Retries);
			}

		}
		#endregion

		/// <summary>
		/// 更新函数.
		/// </summary>
		void Update () {
			if (this._deltaTime != null) {
				this._deltaTime.text = string.Format ("Time:{0:N3} s",
					UIProgressBar._deltaTime);
			}
		}
	}
}
