using System.Collections.Generic;
using UnityEngine.UI;
using Packages.Common.Base;
using Packages.UI.Handle;
using UnityEngine;

namespace Packages.UI 
{

	/// <summary>
	/// UI下载脚本基类.
	/// </summary>
	public abstract class UiProgressProtocol : MonoBehaviour {

		/// <summary>
		/// 变化时间文本框.
		/// </summary>
		public Text _deltaTime;

		/// <summary>
		/// 预先通知确认事件委托.
		/// </summary>
		protected ConfirmHandle _preConfirm;

		/// <summary>
		/// 取消通知确认事件委托.
		/// </summary>
		protected ConfirmHandle _cancelConfirm;

		/// <summary>
		/// 错误提示句柄
		/// </summary>
		protected ErrorNotificationHandle _errorNotification;

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
			List<ErrorInfo> iErrors);

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
			_preConfirm = iPreConfirm;
			PreConfirmNotification (iTotalCount, iTotalDataSize);
		}

		/// <summary>
		/// 取消通知确认事件委托.
		/// </summary>
		/// <param name="iCancelConfirm">取消通知确认事件委托.</param>
		public virtual void OnCancelNotification(
			ConfirmHandle iCancelConfirm) {
			_cancelConfirm = iCancelConfirm;
			CancelNotification ();
		}
		
#endregion
	}
}
