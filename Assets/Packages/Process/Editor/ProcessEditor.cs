using System;
using System.Text;
using UnityEngine;
using Packages.Utils;
using Packages.Logs;

namespace Packages.Process.Editor
{

	/// <summary>
	/// Process编辑器
	/// </summary>	
	public static class ProcessEditor {

		/// <summary>
		/// 运行Path脚本
		/// 默认
		/// </summary>
		/// <param name="iFileName">文件名</param>
		/// <param name="iPath">脚本路径</param>
		/// <param name="iIsBatch">Batch标志位</param>
		/// <param name="iWorkDir">工作目录（默认：Application.dataPath）</param>
		/// <param name="iWaitSeconds">等待结束时间（单位：毫秒）</param>
		public static void Run(string iFileName, string iPath, bool iIsBatch, string iWorkDir = null, int iWaitSeconds = 0)
		{
			var fileFullPath = $"{(string.IsNullOrEmpty(iWorkDir) ? Application.dataPath : iWorkDir)}/{iPath}";
			var proc = new System.Diagnostics.Process
			{
				StartInfo =
				{
					FileName = iFileName,
					StandardOutputEncoding = Encoding.Default,
					StandardErrorEncoding = Encoding.Default,
					// 指定工作目录
					WorkingDirectory = UtilsTools.GetFileDirByFilePath(fileFullPath),
					RedirectStandardOutput = true,
					RedirectStandardError = true,
					CreateNoWindow = true,
					UseShellExecute = false,
					Arguments = iIsBatch ? $"/C {UtilsTools.GetFileName(fileFullPath)}" : UtilsTools.GetFileName(fileFullPath)
				}
			};
			
			Loger.Info($"ProcessEditor::Run():FileName:{iFileName} Arguments:{(iIsBatch ? $"/C {iPath}" : iPath)}");

			try
			{
				if (!proc.Start()) return;
				if (0.0f < iWaitSeconds)
				{
					proc.WaitForExit(iWaitSeconds);	
				}
				else
				{
					proc.WaitForExit();	
				}
				
				// 错误输出
				var sReader = proc.StandardError;
				var error = sReader.ReadToEnd();
				var isError = !string.IsNullOrEmpty(error);
				if (isError)
				{
					Output(error, true);
				}
				else
				{
					// 正常输出
					sReader = proc.StandardOutput;
					var info = sReader.ReadToEnd();
					info = info.Replace('\n', '\r');
					info = info.Replace("\r\n", "\r");
					var output = info.Split('\r');

					Loger.Info($"ProcessEditor::Run():Path:{iPath} \nWorkDir:{proc.StartInfo.WorkingDirectory}");
					foreach (var s in output)
						Output(s);

					Console.Read();
				}
			}
			catch (Exception ex)
			{
				Loger.Fatal($"ProcessEditor::Run():Exception:Message:{ex.Message}\n StackTrace:{ex.StackTrace}");
			}
			finally
			{
				proc.Close();
			}

		}

		/// <summary>
		/// 输出
		/// </summary>
		/// <param name="iMsg">消息</param>
		/// <param name="iIsError">错误标志位</param>
		private static void Output(string iMsg, bool iIsError = false)
		{
			if(string.IsNullOrEmpty(iMsg)) return;
			if (iIsError)
			{
				Loger.Error($"ProcessEditor::Output():{iMsg}");
			}
			else
			{
				Loger.Info($"ProcessEditor::Output():{iMsg}");
			}
		}
	}

}
