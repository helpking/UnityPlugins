using UnityEditor;

namespace Packages.Process.Editor.Python
{
	/// <summary>
	/// Python示例文件
	/// </summary>
	public static class PythonSample {
		
		/// <summary>
		/// 运行Python测试文件
		/// </summary>
		[MenuItem("Tools/Process/Python/Test")]
		public static void RunTest()
		{
//			var scriptPath = $"{Application.dataPath}/Packages/Process/Editor/Python/Test.py";
			const string scriptPath = "./Packages/Process/Editor/Python/Test.py";
			PythonScript.Run(scriptPath);
		}

	}
	

}
