using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Packages.Common.Base
{
	/// <summary>
	/// 错误详细.
	/// </summary>
	public struct ErrorInfo {
		/// <summary>
		/// 失败类型.
		/// </summary>
		public int Code;
		/// <summary>
		/// 详细信息.
		/// </summary>
		public string Detail;
		/// <summary>
		/// 重试次数.
		/// </summary>
		public string Strace;
	}
}
