using UnityEngine;
using System;
using System.IO;

namespace BuildSystem
{
	using BuildLoggerImpl;
	
	public static class BuildLogger
	{
		#region Interface

		private static FileStream ostrm = null;
		private static StreamWriter writer = null;
		private static TextWriter oldOut = Console.Out;

		/// <summary>
		/// 文件打开标志位.
		/// </summary>
		private static bool isFileOpen = false;

		/// <summary>
		/// 打开控制台输出流(重定向输出流).
		/// </summary>
		/// <param name="iBuildLogFile">编译打包日志文件路径.</param>
		/// <param name="iBuildTime">打包时间(YYYYMMDDHHMMSS).</param>
		private static void OpenConsoleStream(string iBuildLogFile) {

#if UNITY_EDITOR
			if (string.IsNullOrEmpty (iBuildLogFile) == true) {
				Debug.LogError ("The path of build log file is null");
				return;
			}

			// 已经打开
			if(isFileOpen == true) {
				return;
			}

			try
			{
				ostrm = new FileStream (iBuildLogFile, FileMode.Append, FileAccess.Write);
				writer = new StreamWriter (ostrm);

				writer.AutoFlush = true;
				// 重定向标准输出流
				Console.SetOut(writer);

				// 重定向标准错误流
				Console.SetError(writer);

				isFileOpen = true;
			}
			catch (Exception e)
			{
				Console.WriteLine (string.Format("Cannot open {0} for writing", iBuildLogFile));
				Console.WriteLine (e.Message);
				return;
			}
			Console.SetOut (writer);
#endif
		}

		/// <summary>
		/// 关闭控制台输出流.
		/// </summary>
		private static void CloseConsoleStream()
		{
#if UNITY_EDITOR
			if (oldOut != null) {
				Console.SetOut (oldOut);
			}
			if (ostrm != null) {
				ostrm.Close ();
			}
			if (writer != null) {
				writer.Close ();
			}
			isFileOpen = false;
#endif
		}

		public static void OpenBlock(string blockName)
		{
#if UNITY_EDITOR

			if (isFileOpen == false) {
				OpenConsoleStream (BuildParameters.BuildLogFile);
			}
			impl.OpenBlock(blockName);
			if (ostrm != null) {
				ostrm.Flush ();
			}
			if (writer != null) {
				writer.Flush ();
			}

#endif
		}
		
		public static void CloseBlock(string blockName)
		{

#if UNITY_EDITOR

			if (isFileOpen == true) {
				impl.CloseBlock(blockName);
				if (ostrm != null) {
					ostrm.Flush ();
				}
				if (writer != null) {
					writer.Flush ();
				}
				CloseConsoleStream ();
			}

#endif
		}
		
		public static void LogMessage(string format, params string[] args)
		{
#if UNITY_EDITOR
			if (isFileOpen == false) {
				OpenConsoleStream (BuildParameters.BuildLogFile);
			}

			var message = string.Format(format, args);
			impl.LogMessage(message);
#endif
		}
		
		public static void LogWarning(string format, params string[] args)
		{
#if UNITY_EDITOR
			if (isFileOpen == false) {
				OpenConsoleStream (BuildParameters.BuildLogFile);
			}

			var message = string.Format(format, args);
			impl.LogWarning(message);

#endif
		}
		
		public static void LogError(string format, params string[] args)
		{
#if UNITY_EDITOR
			if (isFileOpen == false) {
				OpenConsoleStream (BuildParameters.BuildLogFile);
			}

			var message = string.Format(format, args);
			impl.LogError(message);

#endif
		}

		public static void LogException(string format, params string[] args)
		{
#if UNITY_EDITOR
			if (isFileOpen == false) {
				OpenConsoleStream (BuildParameters.BuildLogFile);
			}

			var message = string.Format(format, args);
			impl.LogException(message);
#endif
		}
		
		#endregion
		#region Implementation
		
		static BuildLogger()
		{
//			bool isUnityInBatchMode = UnityEditorInternal.InternalEditorUtility.inBatchMode;
			bool isBuildInCi = BuildParameters.IsBuildInCI;
			if(isBuildInCi)
				impl = new TeamCityBuildLogger();
			else
				impl = new ConsoleBuildLogger();
			
			// TODO: declare (but not define) private partial static void Awake()
			//Awake();
		}
		
		private static IBuildLogger impl { set; get; }
		
		#endregion
	}
} //namespace UnityBuildSystem
