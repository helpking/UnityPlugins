#if UNITY_EDITOR_WIN

using UnityEditor;
using UnityEngine;

namespace Packages.Process.Editor.Batch {
	
	public static class WindowBatchSample {

		/// <summary>
		/// 运行Python测试文件
		/// </summary>
		[MenuItem("Tools/Process/Window/Test")]
		public static void RunTest()
		{
//			var scriptPath = $"{Application.dataPath}/Packages/Process/Editor/Batch/Test.bat";
			const string scriptPath = "/Packages/Process/Editor/Batch/Test.bat";
			BatchEditor.Run(scriptPath);
		}
		
	}

}

#endif