using System.Collections.Generic;
using Packages.Common.Base;

namespace Packages.UI.Handle
{
	/// <summary>
	/// 确认句柄定义
	/// </summary>
	/// <param name="iYesOrNo">Yes/No</param>
	public delegate void ConfirmHandle(bool iYesOrNo);

	/// <summary>
	/// 错误句柄定义
	/// </summary>
	/// <param name="iErrors">错误列表</param>
	public delegate void ErrorNotificationHandle(List<ErrorInfo> iErrors);
}
