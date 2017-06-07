using System.Collections;
using System.IO;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System;
using Common;
using AssetBundles;
using Download;

namespace BuildSystem {
	/// <summary>
	/// 工程打包类.
	/// </summary>
	class ProjectBuild : Editor {

		/// <summary>
		/// 输出用根目录(默认).
		/// </summary>
		private static string _defaultOutputRootDir = string.Format("{0}/../Output", Application.dataPath);

		#region Build

		/// <summary>
		/// 打包场景列表.
		/// </summary>
		/// <returns>打包场景列表.</returns>
		static string[] GetBuildScenes()
		{
			List<string> scenes = new List<string>();

			foreach(EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
			{
				if (scene == null) {
					continue;
				}
				if (scene.enabled) {
					scenes.Add (scene.path);
					BuildLogger.LogMessage ("[BuildScene]: -> {0}", scene.path);
				}
			}
			return scenes.ToArray();
		}

		/// <summary>
		/// 安卓打包.
		/// </summary>
		[UnityEditor.MenuItem("Tools/PerformBuild/AndroidBuild")]
		static void BuildForAndroid()
		{
			const string funcBlock = "ProjectBuild.BuildForAndroid()";
			BuildLogger.OpenBlock(funcBlock);

			// 初始化
			InitForBuild();
			BuildLogger.LogMessage (" --> InitForBuild()");

			// 输出格式（{ProjectName}_v{ProjectVersion}_{buildNumber}_YYYYMMDDHHMMSS.apk）
			string buildTime = BuildParameters.BuildTime;

			// 输出路径
			// 游戏输出目录（Android）:{OutputDir}/Android/{ProjectName}
			string outputDir = GetOutputDir (BuildTarget.Android, BuildParameters.OutputDir);
			if (outputDir == null) {
				BuildLogger.LogException("Create Dir Failed.!!(Dir:{0})", 
					BuildParameters.OutputDir);
				throw new ApplicationException();
			}

			// 打包选项
			BuildOptions buildOptionTmp = BuildOptions.None;
			if (TBuildMode.Debug == BuildParameters.BuildMode) {
				buildOptionTmp |= BuildOptions.Development;
				buildOptionTmp |= BuildOptions.AllowDebugging;
				buildOptionTmp |= BuildOptions.ConnectWithProfiler;
			} 
			BuildLogger.LogMessage ("BuildOption:{0}", ((int)buildOptionTmp).ToString());

			// 版本号
			string buildVersion = BuildInfo.GetInstance().BuildVersion;
			if (string.IsNullOrEmpty (buildVersion) == false) {
				PlayerSettings.bundleVersion = buildVersion;
			}

			// 工程名
			string projectName = BuildParameters.ProjectName;
			if (string.IsNullOrEmpty (projectName) == true) {
				projectName = BuildInfo.GetInstance ().BuildID;
				PlayerSettings.bundleIdentifier = projectName;

				int lastIndex = projectName.LastIndexOf (".");
				projectName = projectName.Substring (lastIndex + 1);
			}
		
			// Apk输出路径
			string apkPath = string.Format("{0}/{1}_{2}_v{3}_{4}_{5}.apk", 
				outputDir, 
				projectName, 
				BuildParameters.BuildMode.ToString(),
				buildVersion, 
				BuildParameters.BuildNumber, 
				buildTime);
			BuildLogger.LogMessage("Apk File Path:{0}", apkPath);
				
			// 输出打包信息
			OutputBuildInfo(buildVersion, projectName);

			// 开发者模式
			if (BuildOptions.Development == buildOptionTmp) {
				// 打包之前，移除非资源对象
				AssetBundles.Common.MoveUnResources();
			}

			string error = BuildPipeline.BuildPlayer(GetBuildScenes(), apkPath, BuildTarget.Android, buildOptionTmp);

			// 开发者模式
			if (BuildOptions.Development == buildOptionTmp) {
				// 打包之后，恢复非资源对象
				AssetBundles.Common.MoveBackUnResources();
			}

			if (error != null && !error.Equals ("") && !(error.Length == 0)) {
				BuildLogger.LogException("Android Build Failed!!!(error:{0})", error);
				BuildLogger.CloseBlock(funcBlock);
				throw new ApplicationException ();
			} else {
				BuildLogger.LogMessage("Android Build Successed.");
			}
			BuildLogger.CloseBlock(funcBlock);
		}

		/// <summary>
		/// 导出XCodeProject工程.
		/// </summary>
		[UnityEditor.MenuItem("Tools/PerformBuild/ExportXcodeProject")]
		static void ExportXcodeProject()
		{ 
			const string funcBlock = "ProjectBuild.ExportXcodeProject()";
			BuildLogger.OpenBlock(funcBlock);

			// 初始化
			InitForBuild();
			BuildLogger.LogMessage (" --> InitForBuild()");

			// 预定义宏
			//PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.iOS, "GAMELINK");

			// 游戏输出目录（iOS）:{OutputDir}/iOS/{ProjectName}
			string outputDir = GetOutputDir (BuildTarget.iOS, null);
			if (outputDir == null) {
				BuildLogger.LogException("Create Dir Failed.!!(Dir:{0})", 
					BuildParameters.OutputDir);
				throw new ApplicationException();
			}

			// 打包选项
			BuildOptions buildOptionTmp = BuildOptions.None;
			if (TBuildMode.Debug == BuildParameters.BuildMode) {
				buildOptionTmp |= BuildOptions.Development;
				buildOptionTmp |= BuildOptions.AllowDebugging;
				buildOptionTmp |= BuildOptions.ConnectWithProfiler;
			} 
			BuildLogger.LogMessage ("BuildOption:{0}", ((int)buildOptionTmp).ToString());


			// 版本号
			string buildVersion = BuildInfo.GetInstance().BuildVersion;
			if (string.IsNullOrEmpty (buildVersion) == false) {
				PlayerSettings.bundleVersion = buildVersion;
			}

			// 工程名
			string projectName = BuildParameters.ProjectName;
			if (string.IsNullOrEmpty (projectName) == true) {
				projectName = BuildInfo.GetInstance().BuildID;
				PlayerSettings.bundleIdentifier = projectName;

				int lastIndex = projectName.LastIndexOf (".");
				projectName = projectName.Substring (lastIndex + 1);
			}
			PlayerSettings.iOS.applicationDisplayName = projectName;

			// XCode工程目录
			string XcodeProject = string.Format("{0}/XcodeProject", outputDir);

			// 输出打包信息
			OutputBuildInfo(buildVersion, projectName);

			// 开发者模式
			if (BuildOptions.Development == buildOptionTmp) {
				// 打包之前，将非资源对象，临时移动到临时文件夹
				AssetBundles.Common.MoveUnResources();
			}

			// 打包成XCode工程目录
			#if UNITY_5
			string error = BuildPipeline.BuildPlayer(
				GetBuildScenes(), 
				XcodeProject, 
				BuildTarget.iOS, buildOptionTmp);
			#else
			string error = BuildPipeline.BuildPlayer(
				GetBuildScenes(), 
				XcodeProject,  
				BuildTarget.iPhone, buildOptionTmp);
			#endif

			// 开发者模式
			if (BuildOptions.Development == buildOptionTmp) {
				// 恢复非资源性文件
				AssetBundles.Common.MoveBackUnResources();
			}

			// 存在错误则，打包编译失败
			if (error != null && !error.Equals ("") && !(error.Length == 0)) {
				BuildLogger.LogException("iOS Build Failed!!!(error:{0})", error);
				BuildLogger.CloseBlock(funcBlock);
				throw new ApplicationException ();
			} else {
				BuildLogger.LogMessage("iOS Build Successed.");
			}
			BuildLogger.CloseBlock(funcBlock);
		}

		#endregion

		#region Build Info

		/// <summary>
		/// 从JSON文件导入打包配置信息(BuildInfo).
		/// </summary>
		[UnityEditor.MenuItem("Assets/BuildInfo/File/Json/Import", false, 600)]
		static void ImportFromBuildInfoJsonFile() {

			const string funcBlock = "ProjectBuild.ImportFromBuildInfoJsonFile()";
			BuildLogger.OpenBlock(funcBlock);

			BuildInfo info = BuildInfo.GetInstance ();
			if (info != null) {
				info.ImportFromJsonFile();
			}

			UtilityAsset.AssetsRefresh ();
			BuildLogger.CloseBlock(funcBlock);
		}



		/// <summary>
		/// 将打包配置信息导出为JSON文件(BuildInfo).
		/// </summary>
		[UnityEditor.MenuItem("Assets/BuildInfo/File/Json/Export", false, 600)]
		static void ExportToBuildInfoJsonFile() {

			const string funcBlock = "ProjectBuild.ExportToBuildInfoJsonFile()";
			BuildLogger.OpenBlock(funcBlock);

			BuildInfo info = BuildInfo.GetInstance ();
			if (info != null) {
				info.ExportToJsonFile();
			}

			UtilityAsset.AssetsRefresh ();
			BuildLogger.CloseBlock(funcBlock);
		}

		/// <summary>
		/// 清空 bundles map.
		/// </summary>
		[UnityEditor.MenuItem("Assets/BuildInfo/Clear", false, 600)]
		static void ClearBuildInfo() {
			const string funcBlock = "ProjectBuild.ClearBuildInfo()";
			BuildLogger.OpenBlock(funcBlock);

			BuildInfo info = BuildInfo.GetInstance ();
			if (info != null) {
				info.Clear ();
			}

			UtilityAsset.AssetsRefresh ();
			BuildLogger.CloseBlock(funcBlock);
		}

		#endregion

		#region 其他处理

		/// <summary>
		/// 取得输出目录.
		/// </summary>
		/// <returns>The output dir.</returns>
		/// <param name="iTarget">打包目标类型.</param>
		/// <param name="iOutputDir">输出目录（未指定：默认输出根目录）.</param>
		private static string GetOutputDir(BuildTarget iTarget, string iOutputDir = null) {
			string outputRootDir = iOutputDir;
			if (string.IsNullOrEmpty (outputRootDir) == true) {
				outputRootDir = _defaultOutputRootDir;
			}

			if (Directory.Exists (outputRootDir) == false) {
				BuildLogger.LogWarning ("The directory is not exist, so to create.(dir:{0})",
					outputRootDir);
				Directory.CreateDirectory (outputRootDir);
			}
			if (Directory.Exists (outputRootDir) == false) {
				BuildLogger.LogError ("[Directory Create Failed] -> Dir:{0}", outputRootDir);
				return null;
			} else {
				BuildLogger.LogMessage ("[Directory Create Successed] -> Dir:{0}", outputRootDir);
			}
			string outputDir = string.Format ("{0}/{1}", outputRootDir, iTarget.ToString());
			if (Directory.Exists (outputDir) == false) {
				BuildLogger.LogWarning ("The directory is not exist, so to create.(dir:{0})",
					outputDir);
				Directory.CreateDirectory (outputDir);
			}
			if (Directory.Exists (outputDir) == false) {
				BuildLogger.LogError ("[Directory Create Failed] -> Dir:{0}", outputDir);
				return null;
			} else {
				BuildLogger.LogMessage ("[Directory Create Successed] -> Dir:{0}", outputDir);
			}
			return outputDir;
		}

		/// <summary>
		/// 输出打包信息(导出的XCode工程 打包ipa文件时使用).
		/// </summary>
		/// <param name="iProjectName">工程名.</param>
		/// <param name="iProjectVersion">工程版本.</param>
		private static void OutputBuildInfo(
			string iProjectName, string iProjectVersion) {
			
			const string funcBlock = "ProjectBuild.OutputBuildInfo()";
			BuildLogger.OpenBlock(funcBlock);

			string filePath = string.Format("{0}/../Shell/BuildInfo", Application.dataPath);
			if (File.Exists (filePath) == true) {
				File.Delete (filePath);
			}
			FileStream fStrm = new FileStream (filePath, FileMode.OpenOrCreate, FileAccess.Write);

			string buildInfo = string.Format("{0}:{1}:{2}", 
				BuildParameters.BuildTime, iProjectName, iProjectVersion);

			BuildLogger.LogMessage ("BuildInfo:{0}", buildInfo);

			// 获得字节数组
			byte[] data = System.Text.Encoding.Default.GetBytes(buildInfo); 
			// 写入
			fStrm.Write (data, 0, data.Length);
			// 清空缓冲区、关闭流
			fStrm.Flush ();
			fStrm.Close ();

			BuildLogger.CloseBlock(funcBlock);
		}

		#endregion

		static void InitForBuild() {
			// 清空下载目录
			DownloadList _instance = DownloadList.GetInstance();
			if (_instance != null) {
				_instance.Clean ();
			}

			BuildSettings.GetInstance ();
		}
	}
}
