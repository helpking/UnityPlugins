using Packages.Common.Editor;
using UnityEditor;

namespace Packages.Utils.Editor {

#if UNITY_EDITOR

	/// <summary>
	/// Window工具类.
	/// </summary>
	public class UtilsWindow {

		/// <summary>
		/// 创建窗口.
		/// </summary>
		/// <param name="iConfInfo">配置信息.</param>
		public static T1 CreateWindow<T1,T2>(T2 iConfInfo) 
			where T1 : EditorWindow 
			where T2 : WindowConfInfoBase {

			// 创建窗口
			return (T1)EditorWindow.GetWindowWithRect (
				typeof (T1), iConfInfo.DisplayRect, 
				true, iConfInfo.WindowName);	
		}

	}

#endif

}
