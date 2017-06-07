using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace Common {

	/// <summary>
	/// UI下载脚本基类.
	/// </summary>
	public abstract class UIProgressProtocol : MonoBehaviour {

		/// <summary>
		/// 变化时间文本框.
		/// </summary>
		public Text _deltaTime = null;

		/// <summary>
		/// 预先通知确认事件委托.
		/// </summary>
		protected ConfirmHandle _preConfirm = null;

		/// <summary>
		/// 取消通知确认事件委托.
		/// </summary>
		protected ConfirmHandle _cancelConfirm = null;

		#region abstract

		/// <summary>
		/// 事先通知事件委托（如：下载前提示用户的信息）.
		/// </summary>
		/// <param name="iTotalCount">下载总数.</param>
		/// <param name="iTotalDataSize">下载数据总大小.</param>
		public abstract void PreConfirmNotification(
			int iTotalCount, long iTotalDataSize);

		/// <summary>
		/// 取消通知确认事件委托.
		/// </summary>
		public abstract void CancelNotification();

		/// <summary>
		/// 错误通知确认事件委托.
		/// </summary>
		/// <param name="iErrors">错误列表.</param>
		public abstract void ErrorNotification(
			List<ErrorDetail> iErrors);

		/// <summary>
		/// 完成确认事件委托.
		/// </summary>
		public abstract void CompletedNotification();

		#endregion

		#region 下载确认消息

		/// <summary>
		/// 事先通知事件委托（如：下载前提示用户的信息）.
		/// </summary>
		/// <param name="iTotalCount">下载总数.</param>
		/// <param name="iTotalDataSize">下载数据总大小.</param>
		/// <param name="iPreConfirm">事先确认委托.</param>
		public virtual void OnPreConfirmNotification(
			int iTotalCount, long iTotalDataSize, ConfirmHandle iPreConfirm) {
			this._preConfirm = iPreConfirm;
			this.PreConfirmNotification (iTotalCount, iTotalDataSize);
		}

		/// <summary>
		/// 取消通知确认事件委托.
		/// </summary>
		/// <param name="iCancelConfirm">取消通知确认事件委托.</param>
		public virtual void OnCancelNotification(
			ConfirmHandle iCancelConfirm) {
			this._cancelConfirm = iCancelConfirm;
			this.CancelNotification ();
		}
		
		#endregion
	}
}
