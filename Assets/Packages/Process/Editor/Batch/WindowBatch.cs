#if UNITY_EDITOR_WIN

namespace Packages.Process.Editor.Batch
{

	/// <summary>
	/// Window系Batch脚本执行
	/// </summary>
	public static class BatchEditor {

		/// <summary>
		/// 运行Path脚本
		/// 默认
		/// </summary>
		/// <param name="iPath">脚本路径</param>
		/// <param name="iWorkDir">工作目录（默认：Application.dataPath）</param>
		public static void Run(string iPath, string iWorkDir = null)
		{
			const string fileName = "cmd.exe";
			// 运行Process
			ProcessEditor.Run(fileName, iPath, true, iWorkDir);
		}
	}
}

#endif
