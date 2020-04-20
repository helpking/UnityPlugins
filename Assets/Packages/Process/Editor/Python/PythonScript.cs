namespace Packages.Process.Editor.Python
{
	
	public static class PythonScript {
		/// <summary>
		/// 运行Path脚本
		/// 默认
		/// </summary>
		/// <param name="iPath">脚本路径</param>
		/// <param name="iWorkDir">工作目录（默认：Application.dataPath）</param>
		public static void Run(string iPath, string iWorkDir = null)
		{
			var fileName = "python";
#if UNITY_EDITOR_WIN
			fileName = "python.exe";
#endif
			
			// 运行Process
			ProcessEditor.Run(fileName, iPath, false, iWorkDir);

		}

	}
}
