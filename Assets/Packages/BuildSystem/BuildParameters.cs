using UnityEngine;
using UnityEditor;
using System.Collections;
using System;
using System.IO;
using Common;

namespace BuildSystem {
	/// <summary>
	/// Build Options.
	/// </summary>
	public class BuildParameters  {

		/// <summary>
		/// 第三方平台SDK类型（预留跨平台编译时使用）.
		/// </summary>
		public enum TPerformSDKType
		{
			kNode,
			kDefault = kNode,
			kMax
		}

		/// <summary>
		/// 输出目录(格式：-output dir).
		/// </summary>
		/// <value>输出目录(格式：-output dir).</value>
		public static string OutputDir {
			get { 

				string[] args = System.Environment.GetCommandLineArgs();
				if ((args == null) || (args.Length <= 0)) {
					return null;
				}
				for(int idx = 0; idx < args.Length; idx++)
				{
					if((args[idx] == "-output") && (args.Length > idx + 1))
					{
						return args[idx + 1];
					}
				}
				return null;
			}
		}

		/// <summary>
		/// 项目名称(格式：-projectName name).
		/// </summary>
		/// <value>项目名称(格式：-projectName name).</value>
		public static string ProjectName
		{
			get
			{ 
				string[] args = System.Environment.GetCommandLineArgs();
				if ((args == null) || (args.Length <= 0)) {
					return null;
				}
				for(int idx = 0; idx < args.Length; idx++)
				{
					if((args[idx] == "-projectName") && (args.Length > idx + 1))
					{
						return args[idx + 1];
					}
				}
				return null;
			}
		}

		/// <summary>
		///  第三方平台SDK类型(格式：-performSdk N).
		/// </summary>
		/// <value>第三方平台SDK类型(格式：-performSdk N).</value>
		public static TPerformSDKType PerformSDKType {
			get { 

				string[] args = System.Environment.GetCommandLineArgs();
				if ((args == null) || (args.Length <= 0)) {
					return TPerformSDKType.kDefault;
				}
				for(int idx = 0; idx < args.Length; idx++)
				{
					if((args[idx] == "-performSdk") && (args.Length > idx + 1))
					{
						string str = args[idx + 1];
						int value;
						if (int.TryParse (str, out value)) {
							return (TPerformSDKType)value;
						} else {
							return TPerformSDKType.kDefault;
						}
					}
				}
				return TPerformSDKType.kDefault;
			}
		}

		/// <summary>
		/// BuildNumber(在TeamCity上打包No.格式：-buildNo N).
		/// </summary>
		/// <value>BuildNumber(在TeamCity上打包No.格式：-buildNo N).</value>
		public static int BuildNumber {
			get { 

				string[] args = System.Environment.GetCommandLineArgs();
				if ((args == null) || (args.Length <= 0)) {
					return -1;
				}
				for(int idx = 0; idx < args.Length; idx++)
				{
					if((args[idx] == "-buildNo") && (args.Length > idx + 1))
					{
						string str = args[idx + 1];
						int value;
						if (int.TryParse (str, out value)) {
							return value;
						} else {
							return -1;
						}
					}
				}
				return -1;
			}
		}

		/// <summary>
		/// build Mode(格式：-debug/-release/-store).
		/// </summary>
		/// <value>(格式：-debug/-release/-store).</value>
		public static TBuildMode BuildMode {
			get { 
				string[] args = System.Environment.GetCommandLineArgs();
				if ((args == null) || (args.Length <= 0)) {
					return TBuildMode.Debug;
				}
				for(int idx = 0; idx < args.Length; idx++)
				{
					if ("-debug".Equals (args [idx]) == true) {
						return TBuildMode.Debug;
					} else if ("-release".Equals (args [idx]) == true) {
						return TBuildMode.Release;
					} else if ("-store".Equals (args [idx]) == true) {
						return TBuildMode.Store;
					}
				}
				return TBuildMode.Debug;
			}
		}

		/// <summary>
		/// 是否在CI打包编译(格式：-teamCityBuild).
		/// </summary>
		/// <value><c>true</c> CI打包编译; 本地打包编译, <c>false</c>.</value>
		public static bool IsBuildInCI {
			get { 
				string[] args = System.Environment.GetCommandLineArgs();
				if ((args == null) || (args.Length <= 0)) {
					return false;
				}
				for(int idx = 0; idx < args.Length; idx++)
				{
					if(args[idx] == "-teamCityBuild")
					{
						return true;
					}
				}
				return false;
			}
		}

		private static string _buildTime = null;
		/// <summary>
		/// 打包时间(格式：-buildTime YYYYMMDDHHMMSS).
		/// </summary>
		/// <value>打包时间(格式：-buildTime YYYYMMDDHHMMSS).</value>
		public static string BuildTime {
			get { 
				string[] args = System.Environment.GetCommandLineArgs();

				if ((args != null) && (args.Length > 0)) {
					for (int idx = 0; idx < args.Length; idx++) {
						if (args [idx] == "-buildTime") {
							_buildTime = args [idx + 1];
							break;
						}
					}
				}
				if ((_buildTime == null) || 
					(_buildTime.Equals ("") == true) ||
					(_buildTime.Length == 0)) {
					DateTime nowDateTime = DateTime.Now; 
					_buildTime = string.Format ("{0:yyyyMMddHHmmss}", nowDateTime);
				}
				return _buildTime;
			}
		}

		/// <summary>
		/// 打包日志文件.
		/// </summary>
		/// <value>打包日志文件.</value>
		private static string _buildLogFile = null;
		/// <summary>
		/// 编译打包日志文件(格式：-buildLogFile xxxx.log).
		/// </summary>
		/// <value>编译打包日志文件(格式：-buildLogFile xxxx.log).</value>
		public static string BuildLogFile {
			get {
				if (string.IsNullOrEmpty (_buildLogFile) == false) {
					return _buildLogFile;
				}
				string[] args = System.Environment.GetCommandLineArgs ();
				if ((args != null) && (args.Length > 0)) {
					for (int idx = 0; idx < args.Length; idx++) {
						if (args [idx] == "-buildLogFile") {
							_buildLogFile = args [idx + 1];
						}	
					}
				}
				if (string.IsNullOrEmpty (_buildLogFile) == true) {
					_buildLogFile = string.Format("{0}/Logs", 
						Application.persistentDataPath);
					if (Directory.Exists (_buildLogFile) == false) {
						Directory.CreateDirectory (_buildLogFile);
					}
					_buildLogFile = string.Format ("{0}/{1}.log", 
						_buildLogFile, BuildParameters.BuildTime);
					if (File.Exists (_buildLogFile) == true) {
						File.Delete (_buildLogFile);
					}
				}
				return _buildLogFile;
			}
		}
	}
}