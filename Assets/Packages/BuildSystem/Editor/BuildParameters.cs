using System;
using System.IO;
using UnityEngine;
using Packages.Common;
using Packages.Settings;
using Packages.Logs;

namespace Packages.BuildSystem.Editor {

	/// <summary>
	/// CI 模式
	/// </summary>
	public enum CiMode
	{
		/// <summary>
		/// 无
		/// </summary>
		None,
		
		/// <summary>
		/// Teamcity
		/// </summary>
		TeamCity,
		
		/// <summary>
		/// Jenkins
		/// </summary>
		Jenkins
	}

	/// <summary>
	/// Build Options.
	/// </summary>
	public static class BuildParameters  {

		/// <summary>
		/// 输出目录(格式：-output dir).
		/// </summary>
		/// <value>输出目录(格式：-output dir).</value>
		public static string OutputDir {
			get
			{
				string dir = null;
				var args = Environment.GetCommandLineArgs();
				if (args.Length <= 0) {
					return null;
				}
				for(var idx = 0; idx < args.Length; idx++)
				{
					if(args[idx] == "-output" && args.Length > idx + 1)
					{
						dir = args[idx + 1];
						break;
					}
				}

				if (false == string.IsNullOrEmpty(dir))
				{
					Loger.OutputDir = dir;
				}
				return dir;
			}
		}

		/// <summary>
		/// 项目名称(格式：-xcodeSchema name).
		/// </summary>
		/// <value>项目名称(格式：-xcodeSchema name).</value>
		public static string ProjectName
		{
			get
			{ 
				var args = Environment.GetCommandLineArgs();
				if (args.Length <= 0) {
					return null;
				}
				for(var idx = 0; idx < args.Length; idx++)
				{
					if(args[idx] == "-xcodeSchema" && args.Length > idx + 1)
					{
						return args[idx + 1];
					}
				}
				return null;
			}
		}

		/// <summary>
		/// 项目名称(格式：-gameName name).
		/// </summary>
		/// <value>项目名称(格式：-gameName name).</value>
		public static string GameName
		{
			get
			{ 
				var args = Environment.GetCommandLineArgs();
				if (args.Length <= 0) {
					return null;
				}
				for(var idx = 0; idx < args.Length; idx++)
				{
					if(args[idx] == "-gameName" && args.Length > idx + 1)
					{
						return args[idx + 1];
					}
				}
				return null;
			}
		}


		/// <summary>
		/// Build ID(格式：-buildId ID).
		/// </summary>
		/// <value>Build ID(格式：-buildId ID).</value>
		public static string BuildId {
			get
			{ 
				var args = Environment.GetCommandLineArgs();
				if (args.Length <= 0) {
					return null;
				}
				for(var idx = 0; idx < args.Length; idx++)
				{
					if(args[idx] == "-buildId" && args.Length > idx + 1)
					{
						return args[idx + 1];
					}
				}
				return null;
			}
		}

		/// <summary>
		/// Build Version(格式：-buildVersion version).
		/// </summary>
		/// <value>Build Version(格式：-buildVersion version).</value>
		public static string BuildVersion {
			get
			{ 
				var args = Environment.GetCommandLineArgs();
				if (args.Length <= 0) {
					return null;
				}
				for(var idx = 0; idx < args.Length; idx++)
				{
					if(args[idx] == "-buildVersion" && args.Length > idx + 1)
					{
						return args[idx + 1];
					}
				}
				return null;
			}
		}

		/// <summary>
		/// Build Version Code(格式：-buildVersionCode version).
		/// </summary>
		/// <value>Build Version Code(格式：-buildVersionCode version).</value>
		public static int BuildVersionCode {
			get
			{ 
				var args = Environment.GetCommandLineArgs();
				if (args.Length <= 0) {
					return 1;
				}
				for(var idx = 0; idx < args.Length; idx++)
				{
					if (args[idx] != "-buildVersionCode" || args.Length <= idx + 1) continue;
					var value = args [idx + 1];
					return string.IsNullOrEmpty(value) ? 1 : Convert.ToInt32(args[idx + 1]);
				}
				return 1;
			}
		}

		/// <summary>
		/// 日志等级(格式：-logLevel 数字(0~6)).
		/// </summary>
		/// <value>日志等级(格式：-logLevel 数字(0~6)).</value>
		public static LogLevel LogLevel {
			get
			{ 
				var args = Environment.GetCommandLineArgs();
				if (args.Length <= 0) {
					return LogLevel.All;
				}
				for(var idx = 0; idx < args.Length; idx++)
				{
					if (args[idx] != "-logLevel" || args.Length <= idx + 1) continue;
					var value = args [idx + 1];
					if(string.IsNullOrEmpty(value)) {
						return LogLevel.All;
					}
					var valueTmp = Convert.ToInt32(args[idx + 1]);
					return (LogLevel)valueTmp;
				}
				return LogLevel.All;
			}
		}

		/// <summary>
		/// 平台类型(格式：-Huawei/-Tiange).
		/// </summary>
		/// <value>平台类型(格式：-Huawei/-Tiange).</value>
		public static PlatformType PlatformType {
			get { 

				var args = Environment.GetCommandLineArgs();
				if (args.Length <= 0) {
					return PlatformType.None;
				}
				foreach (var t in args)
				{
					if (t == "-Huawei")
						return PlatformType.Huawei;
					if (t == "-Tiange") return PlatformType.Tiange;
				}
				return PlatformType.None;
			}
		}

		/// <summary>
		/// BuildNumber(在TeamCity上打包No.格式：-buildNo N).
		/// </summary>
		/// <value>BuildNumber(在TeamCity上打包No.格式：-buildNo N).</value>
		public static int BuildNumber {
			get { 

				var args = Environment.GetCommandLineArgs();
				if (args.Length <= 0) {
					return -1;
				}
				for(var idx = 0; idx < args.Length; idx++)
				{
					if (args[idx] != "-buildNo" || args.Length <= idx + 1) continue;
					var str = args[idx + 1];
					int value;
					if (int.TryParse (str, out value)) {
						return value;
					}

					return -1;
				}
				return -1;
			}
		}

		/// <summary>
		/// 每秒传输帧数(单位：帧／秒。默认60帧／秒)(在TeamCity上打包No.格式：-FPS N).
		/// </summary>
		/// <value>每秒传输帧数(单位：帧／秒。默认60帧／秒)(在TeamCity上打包No.格式：-FPS N).</value>
		public static int Fps {
			get { 

				var args = Environment.GetCommandLineArgs();
				if (args.Length <= 0) {
					return 60;
				}
				for(var idx = 0; idx < args.Length; idx++)
				{
					if (args[idx] != "-FPS" || args.Length <= idx + 1) continue;
					var str = args[idx + 1];
					int value;
					return int.TryParse (str, out value) ? value : 60;
				}
				return 60;
			}
		}

		/// <summary>
		/// build Mode(格式：-debug/-release/-production).
		/// </summary>
		/// <value>(格式：-debug/-release/-production).</value>
		public static BuildMode BuildMode {
			get { 
				var args = Environment.GetCommandLineArgs();
				if (args.Length <= 0) {
					return BuildMode.Debug;
				}
				foreach (var _t in args)
				{
					if ("-debug".Equals (_t)) {
						return BuildMode.Debug;
					}

					if ("-release".Equals (_t)) {
						return BuildMode.Release;
					}

					if ("-production".Equals (_t)) {
						return BuildMode.Production;
					}
				}
				return BuildMode.Debug;
			}
		}

		/// <summary>
		/// build Mode(格式：-define [agr2,arg2,arg3,...]).
		/// </summary>
		/// <value>(格式：-define [agr2,arg2,arg3,...]).</value>
		public static string[] Defines {
			get { 
				var args = Environment.GetCommandLineArgs();
				if (args.Length <= 2) {
					return null;
				}
				for(var idx = 0; idx < args.Length; idx++)
				{
					if ("-defines".Equals(args[idx]) != true || args.Length < idx + 2) continue;
					var temp = args [idx + 1];
					return temp.Split(',');
				}
				return null;
			}
		}

		/// <summary>
		/// 是否在CI打包编译(格式：-teamCity/-jenkins).
		/// </summary>
		/// <value>CI打包模式.</value>
		public static CiMode CiMode {
			get { 
				var args = Environment.GetCommandLineArgs();
				if (args.Length <= 0) {
					return CiMode.None;
				}
				foreach (var t in args)
				{
					if(t == "-teamCity")
					{
						return CiMode.TeamCity;
					}
					if(t == "-jenkins")
					{
						return CiMode.Jenkins;
					}
				}
				return CiMode.None;
			}
		}
			
		/// <summary>
		/// 是否为Cheat模式（可以输入命令行）(格式：-cheat).
		/// </summary>
		/// <value><c>true</c> 可以输入命令行, <c>false</c>不可以输入命令行.</value>
		public static bool IsCheatMode {
			get { 
				var args = Environment.GetCommandLineArgs();
				if (args.Length <= 0) {
					return false;
				}
				foreach (var t in args)
				{
					if(t == "-cheat")
					{
						return true;
					}
				}
				return false;
			}
		}

		/// <summary>
		/// 是否为跳过下载（可以输入命令行）(格式：-skipDownload).
		/// </summary>
		/// <value><c>true</c> 跳过下载, <c>false</c>不跳过下载.</value>
		public static bool IsSkipDownload {
			get { 
				var args = Environment.GetCommandLineArgs();
				if (args.Length <= 0) {
					return false;
				}
				foreach (var t in args)
				{
					if(t == "-skipDownload")
					{
						return true;
					}
				}
				return false;
			}
		}

		private static string _buildTime;
		/// <summary>
		/// 打包时间(格式：-buildTime YYYYMMDDHHMMSS).
		/// </summary>
		/// <value>打包时间(格式：-buildTime YYYYMMDDHHMMSS).</value>
		public static string BuildTime {
			get { 
				var args = Environment.GetCommandLineArgs();

				if (args.Length > 0) {
					for (var idx = 0; idx < args.Length; idx++) {
						if (args[idx] != "-buildTime") continue;
						_buildTime = args [idx + 1];
						break;
					}
				}

				if (_buildTime != null && !_buildTime.Equals("") && _buildTime.Length != 0) return _buildTime;
				var nowDateTime = DateTime.Now; 
				_buildTime = $"{nowDateTime:yyyyMMddHHmmss}";
				return _buildTime;
			}
		}

		/// <summary>
		/// 前端Host(格式：-webHost <Web Host Address>).
		/// </summary>
		/// <value>前端Host(格式：-webHost <Web Host Address>).</value>
		public static string WebHost {
			get
			{ 
				var args = Environment.GetCommandLineArgs();
				if (args.Length <= 0) {
					return null;
				}
				for(var idx = 0; idx < args.Length; idx++)
				{
					if(args[idx] == "-webHost" && args.Length > idx + 1)
					{
						return args[idx + 1];
					}
				}
				return null;
			}
		}

		/// <summary>
		/// 前端端口号(格式：-webPortNo <Web Port No>).
		/// </summary>
		/// <value>前端端口号(格式：-webPortNo <Web Port No>).</value>
		public static int WebPortNo {
			get { 
				var args = Environment.GetCommandLineArgs();
				if (args.Length <= 0) {
					return -1;
				}
				for(var idx = 0; idx < args.Length; idx++)
				{
					if (args[idx] != "-webPortNo" || args.Length <= idx + 1) continue;
					var str = args[idx + 1];
					int value;
					if (int.TryParse (str, out value)) {
						return value;
					}

					return -1;
				}
				return -1;
			}
		}

		/// <summary>
		/// 打包日志文件.
		/// </summary>
		/// <value>打包日志文件.</value>
		private static string _buildLogFile;
		/// <summary>
		/// 编译打包日志文件(格式：-buildLogFile xxxx.Logs).
		/// </summary>
		/// <value>编译打包日志文件(格式：-buildLogFile xxxx.Logs).</value>
		public static string BuildLogFile {
			get {
				if (string.IsNullOrEmpty (_buildLogFile) == false) {
					return _buildLogFile;
				}
				var args = Environment.GetCommandLineArgs ();
				if (args.Length > 0) {
					for (var idx = 0; idx < args.Length; idx++) {
						if (args [idx] == "-buildLogFile") {
							_buildLogFile = args [idx + 1];
						}	
					}
				}

				if (!string.IsNullOrEmpty(_buildLogFile)) return _buildLogFile;
				_buildLogFile =
					$"{(Application.isMobilePlatform ? Application.persistentDataPath : Application.dataPath)}/Logs";
				
				if (Directory.Exists (_buildLogFile) == false) {
					Directory.CreateDirectory (_buildLogFile);
				}
				_buildLogFile = $"{_buildLogFile}/{BuildTime}.Logs";
				if (File.Exists (_buildLogFile)) {
					File.Delete (_buildLogFile);
				}
				return _buildLogFile;
			}
		}
	}
}