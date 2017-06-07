using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using Download;
using AssetBundles;
using Common;

public class UISceneDownload : UIProgressProtocol {

	#region ButtonClick

	/// <summary>
	/// 点击下载按钮(Http).
	/// </summary>
	public void OnHttpDownloadBtnClick() {

		DownloadManager manager = this.GetComponent<DownloadManager> ();
		if (manager == null) {
			return;
		}

		// 下载已经开始
		if(manager.isStarted == true) {
			return;
		}

		manager.DownloadByHttp ();
	}

	/// <summary>
	/// 点击下载按钮(WWW).
	/// </summary>
	public void OnWWWDownloadBtnClick() {

		DownloadManager manager = this.GetComponent<DownloadManager> ();
		if (manager == null) {
			return;
		}

		// 下载已经开始
		if(manager.isStarted == true) {
			return;
		}

		manager.DownloadByWWW ();
	}

	/// <summary>
	/// 点击下载取消按钮.
	/// </summary>
	public void OnCancelBtnClick() {
		DownloadManager manager = this.GetComponent<DownloadManager> ();
		if (manager == null) {
			return;
		}
		manager.DownloadCancel ();
	}

	/// <summary>
	/// 点击下载重置按钮.
	/// </summary>
	public void OnResetBtnClick() {

		DownloadManager manager = this.GetComponent<DownloadManager> ();
		if (manager == null) {
			return;
		}
		manager.DownloadReset ();
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
		Debug.Log ("CompletedNotification");

		// 加载场景
//		SceneManager.LoadScene ("SceneAssetBundleLoad");
		AssetBundlesManager.GetInstance().LoadScene("SceneAssetBundleLoad");
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
