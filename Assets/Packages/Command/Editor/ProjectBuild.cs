using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Packages.Common.Base;
using Packages.BuildSystem.Editor;
using Packages.BuildSystem.Settings;
using Packages.Common;
using Packages.Common.Base.Editor;
using Packages.Settings;
using Packages.Utils;
using Packages.Logs;

#if UNITY_ANDROID && UNITY_EDITOR
using Packages.BuildSystem.AndroidSDK;
using Packages.BuildSystem.AndroidSDK.Platforms.Huawei;
using Packages.BuildSystem.AndroidSDK.Platforms.Tiange;
#endif

namespace Packages.Command.Editor {
	/// <summary>
	/// 工程打包类.
	/// </summary>
	internal class ProjectBuild : EditorBase {

		/// <summary>
		/// 输出用根目录(默认).
		/// </summary>
		private static readonly string DefaultOutputRootDir = $"{Application.dataPath}/../Output";

		/// <summary>
		/// 打包场景列表.
		/// </summary>
		/// <returns>打包场景列表.</returns>
		private static string[] GetBuildScenes()
		{
			var scenes = new List<string>();

			foreach(var scene in EditorBuildSettings.scenes)
			{
				if (scene == null) {
					continue;
				}

				if (!scene.enabled) continue;
				scenes.Add (scene.path);
				Loger.BuildLog($"[BuildScene]: -> {scene.path}");
			}
			return scenes.ToArray();
		}

#region Build Android

		/// <summary>
		/// 安卓打包.
		/// </summary>
		[MenuItem("Tools/PerformBuild/AndroidBuild", false, 200)]
		[Obsolete]
		public static void BuildForAndroid()
		{
			// 输出路径
			// 游戏输出目录（Android）:{OutputDir}/Android/{ProjectName}
			var outputDir = GetOutputDir (
				BuildTarget.Android, BuildParameters.BuildNumber, BuildParameters.OutputDir);
			if (outputDir == null) {
				Loger.BuildErrorLog($"Create Dir Failed.!!(Dir:{BuildParameters.OutputDir})");
				throw new ApplicationException();
			}
			
			const string funcBlock = "ProjectBuild.BuildForAndroid()";
			Loger.BuildStart(funcBlock);

			// 设定打包信息
			SetBuildInfoFromParameters(true);

			var buildMode = SysSettings.GetInstance().BuildMode;
			Loger.BuildLog($"BuildMode:{buildMode}");

			// 输出格式（{ProjectName}_v{ProjectVersion}_{buildNumber}_YYYYMMDDHHMMSS.apk）
			var buildTime = BuildParameters.BuildTime;

			// 打包选项
			var buildOptionTmp = BuildOptions.None;
			if (BuildMode.Debug == buildMode) {
				buildOptionTmp |= BuildOptions.Development;
				buildOptionTmp |= BuildOptions.AllowDebugging;
				buildOptionTmp |= BuildOptions.ConnectWithProfiler;
			} else {
				var isCheatMode = BuildParameters.IsCheatMode;
				if (isCheatMode) {
					buildOptionTmp |= BuildOptions.Development;
				}
			}
			Loger.BuildLog($"BuildOption:{buildOptionTmp}");

			// 版本号
			var buildVersion = SysSettings.GetInstance().BuildVersion;
			if (string.IsNullOrEmpty (buildVersion) == false) {
				PlayerSettings.bundleVersion = buildVersion;
			}
			Loger.BuildLog ($"BuildVersion:{buildVersion}");

			// buildVersionCode
			var buildVersionCode = SysSettings.GetInstance().BuildVersionCode;
			PlayerSettings.Android.bundleVersionCode = buildVersionCode;
			Loger.BuildLog ($"BundleVersionCode:{buildVersionCode}");

			// 工程名
			var projectName = SysSettings.GetInstance().BuildName;
			Loger.BuildLog ($"ProjectName:{projectName}");

			// 游戏名字
			var gameName = BuildParameters.GameName;
			if (string.IsNullOrEmpty (gameName)) {
				gameName = projectName;
			}
			PlayerSettings.productName = gameName;
			Loger.BuildLog ($"GameName:{gameName}");

			// BuildID
			var buildId = SysSettings.GetInstance().BuildId;
			if (false == string.IsNullOrEmpty (buildId)) {
#if UNITY_5_5_OR_NEWER
                PlayerSettings.applicationIdentifier = buildId;
#else
				PlayerSettings.bundleIdentifier = buildId;
#endif
			}
			Loger.BuildLog ($"BuildID:{buildId}");

			// 初始化
			InitForAndroidBuild();
			Loger.BuildLog (" --> InitForAndroidBuild()");
		
			// Apk输出路径
			var buildNumber = SysSettings.GetInstance().BuildNumber;
			Loger.BuildLog ($"BuildNumber:{buildNumber}");

			// 前端服务器Host
			var webHost = SysSettings.GetInstance ().data.network.webServer.host;
			Loger.BuildLog ($"WebServer:Host:{webHost}");

			// 前端服务器端口号
			var webPortNo = SysSettings.GetInstance ().data.network.webServer.portNo;
			Loger.BuildLog ($"WebServer:PortNo:{webPortNo}");

			// 输出宏定义
			var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android);
			Loger.BuildLog($"Defines:{defines}");

			var apkPath = $"{outputDir}/{projectName}_{buildMode}_v{buildVersion}_-_{buildTime}_{buildId}.apk";
			if (0 < buildNumber) {
				apkPath =
					$"{outputDir}/{projectName}_{buildMode}_v{buildVersion}_{buildNumber}_{buildTime}_{buildId}.apk";
			}
			Loger.BuildLog($"Apk File Path:{apkPath}");
				
			// 输出打包信息
			OutputBuildInfo(buildVersion, projectName);

			// 开发者模式
			if (BuildOptions.Development == buildOptionTmp) {
				// 打包之前，移除非资源对象
//				AssetBundles.Common.MoveUnResources();
			}

			// Android下IL2CPP不支持，所以设置回Mono
#pragma warning disable 618
			PlayerSettings.SetPropertyInt("ScriptingBackend", (int)ScriptingImplementation.Mono2x, BuildTarget.Android);
#pragma warning restore 618

			var report = BuildPipeline.BuildPlayer(GetBuildScenes(), apkPath, BuildTarget.Android, buildOptionTmp);
			// 开发者模式
			if (BuildOptions.Development == buildOptionTmp) {
				// 打包之后，恢复非资源对象
//				AssetBundles.Common.MoveBackUnResources();
			}

#if UNITY_2018_3 || UNITY_2018_3_OR_NEWER
			if (0 > report.summary.totalErrors)
			{  
				Loger.BuildErrorLog($"Android Build Failed!!!(error:{report.summary})");
				Loger.BuildEnd();
				throw new ApplicationException ();
			}
#else
			if (report != null) {
				Loger.BuildErrorLog($"Android Build Failed!!!(error:{report})");
				Loger.BuildEnd();
				throw new ApplicationException ();
			}			
#endif
			
			Loger.BuildLog("Android Build Succeeded.");
			
			// 生成上传用的Shell脚本
			CreateUploadShell(buildNumber, BuildTarget.Android, apkPath);
			// 生成远程CDN服务器刷新脚本
			AssetBundlesBuild.CreateShellOfRefreshCdn();
			Loger.BuildEnd();
		}

		private static void InitForAndroidBuild() {

			// 初始化打包信息
			BuildSettings.GetInstance (BuildSettings.AssetFileDir);

#if UNITY_ANDROID

			// 设定选项
			// 天鸽的场合
			if(PlatformType.Tiange == SysSettings.GetInstance().PlatformType) {

				// 初始化SDK设定信息&导入最新
				TiangeSdkSettings.GetInstance (TiangeSdkSettings.AssetFileDir).ImportFromJsonFile();
				Loger.BuildLog("TiangeSdkSettings -> ImportFromJsonFile().");
				var oneSDk = TiangeSdkSettings.GetInstance().data.Options.IsOptionValid(SDKOptions.OneSDK);
				var metaDatasCount = TiangeSdkSettings.GetInstance().data.Options.OneSDK.metaDatas.Count;
				Loger.BuildLog($"TiangeSdkSettings::OneSDK:{oneSDk}(MetaData:{metaDatasCount}).");

				TiangeSdkSettings.GetInstance ().data.Options.data =
					SysSettings.GetInstance ().data.Options.data;
			}

			// 清空Plugins/Android目录
			ClearPluginsAndroid();

			// 重置Plugins/Android
			ResetPluginsAndroid();

			// 设置相关AndroidSDK相关设定
			var androidSdk = GetCurAndroidSdkSetting ();
			if (androidSdk != null) {
				var gameName = SysSettings.GetInstance ().BuildName;
				var packageName = SysSettings.GetInstance ().BuildId;
				// 打包Android（apk文件）之前，提前应用设定
				androidSdk.PreApplyAndroidBuild (gameName, packageName);
			} else {
				Loger.BuildWarningLog("Android SDK invalid!!");
			}

#endif

			// 刷新
			UtilsAsset.AssetsRefresh ();
		}
			
#if UNITY_ANDROID

		/// <summary>
		/// 清空Plugins/Android目录.
		/// </summary>
		private static void ClearPluginsAndroid() {
		
			var dir = $"{Application.dataPath}/Plugins/Android";
			if (false == UtilsTools.CheckAndCreateDir (dir)) {
				return;
			}

			// 清空目录
			UtilsTools.ClearDirectory(dir);
		}

		/// <summary>
		/// 重置Plugins/Android.
		/// </summary>
		private static void ResetPluginsAndroid() {
			var fromDir = $"{Application.dataPath}/../AndroidPlatform/Default";
			if (false == UtilsTools.CheckAndCreateDir (fromDir)) {
				return;
			}

			var toDir = $"{Application.dataPath}/Plugins/Android";
			if (false == UtilsTools.CheckAndCreateDir (toDir)) {
				return;
			}
				
			// 拷贝文件
			var dirInfo = new DirectoryInfo(fromDir);
			var allFiles = dirInfo.GetFiles();
			if (1 <= allFiles.Length) {
				foreach (var file in allFiles) {
					if (file.Name.EndsWith (".meta")) {
						continue;
					}

					// 拷贝文件
					var copyToFile = $"{toDir}/{file.Name}";
					Loger.BuildLog($"Copy File : {file.FullName} -> {copyToFile}");

					File.Copy (file.FullName, copyToFile, true);
				}
			}

			// 检索子文件夹
			var subDirs = dirInfo.GetDirectories();
			if (1 > subDirs.Length) return;
			foreach (var subDir in subDirs) {
				var subFromDir = $"{fromDir}/{subDir.Name}";

				// 拷贝
				UtilsTools.CopyDirectory (subFromDir, toDir);
			}
		}

		/// <summary>
		/// 取得当前AndroidSDK设定信息.
		/// </summary>
		/// <returns>当前AndroidSDK设定信息.</returns>
		private static IAndroidSdkSettings GetCurAndroidSdkSetting() {
			IAndroidSdkSettings settings = null;

			// 平台类型
			var platformType = SysSettings.GetInstance ().PlatformType;
			Loger.BuildLog($"PlatformType:{platformType}.");

			switch (platformType) {
				// 华为
				case PlatformType.Huawei:
					{
						settings = HuaweiSdkSettings.GetInstance (HuaweiSdkSettings.AssetFileDir);
					}
					break;

					// 天鸽
				case PlatformType.Tiange:
					{
						settings = TiangeSdkSettings.GetInstance (TiangeSdkSettings.AssetFileDir);
					}
					break;
				case PlatformType.Android:
					break;
				case PlatformType.None:
					break;
				case PlatformType.iOS:
					break;
				default:
				{
					// 清空/Plugins/Android下的文件
				}
				break;
			}

			return settings;
		}

#endif

#endregion

#region Export Xcode Project

		/// <summary>
		/// 导出XCodeProject工程.
		/// </summary>
		[MenuItem("Tools/PerformBuild/ExportXcodeProject", false, 201)]
		public static void ExportXcodeProject()
		{ 
			// 输出路径
			// 游戏输出目录（Android）:{OutputDir}/Android/{ProjectName}
			var outputDir = GetOutputDir (
				BuildTarget.iOS, BuildParameters.BuildNumber, BuildParameters.OutputDir);
			if (outputDir == null) {
				Loger.BuildErrorLog($"Create Dir Failed.!!(Dir:{BuildParameters.OutputDir})");
				throw new ApplicationException();
			}
			
			const string funcBlock = "ProjectBuild:ExportXcodeProject()";
			Loger.BuildStart(funcBlock);

			// 设定打包信息
			SetBuildInfoFromParameters(false);
			// 平台类型
			SysSettings.GetInstance().PlatformType = PlatformType.iOS;

			var buildMode = SysSettings.GetInstance ().BuildMode;
			Loger.BuildLog($"BuildMode:{buildMode}");

			// 初始化
			InitForExportXcodeProject();
			Loger.BuildLog (" --> InitForExportXcodeProject()");

			// 预定义宏
			//PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.iOS, "GAMELINK");

			// 打包选项
			var buildOptionTmp = BuildOptions.None;
			if (BuildMode.Debug == buildMode) {
				buildOptionTmp |= BuildOptions.Development;
				buildOptionTmp |= BuildOptions.AllowDebugging;
				buildOptionTmp |= BuildOptions.ConnectWithProfiler;
			} else {
				var isCheatMode = BuildParameters.IsCheatMode;
				if (isCheatMode) {
					buildOptionTmp |= BuildOptions.Development;
				}
			}
			Loger.BuildLog($"BuildOption:{buildOptionTmp}");

			// 工程名
			var projectName = SysSettings.GetInstance().BuildName;
			Loger.BuildLog ($"ProjectName:{projectName}");
				
			// 游戏名字
			var gameName = BuildParameters.GameName;
			if (string.IsNullOrEmpty (gameName)) {
				gameName = projectName;
			}
			PlayerSettings.iOS.applicationDisplayName = gameName;
			Loger.BuildLog ($"GameName:{gameName}");

			// BuildID
			var buildId = SysSettings.GetInstance().BuildId;
			if (false == string.IsNullOrEmpty (buildId)) {
#if UNITY_5_5_OR_NEWER
                PlayerSettings.applicationIdentifier = buildId;
#else
				PlayerSettings.bundleIdentifier = buildId;
#endif
			}
			Loger.BuildLog ($"BuildID:{buildId}");

			// 版本号
			var buildVersion = SysSettings.GetInstance ().BuildVersion;
			PlayerSettings.bundleVersion = buildVersion;
			Loger.BuildLog ($"BuildVersion:{buildVersion}");

			// 前端服务器Host
			var webHost = SysSettings.GetInstance ().data.network.webServer.host;
			Loger.BuildLog ($"WebServer:Host:{webHost}");

			// 前端服务器端口号
			var webPortNo = SysSettings.GetInstance ().data.network.webServer.portNo;
			Loger.BuildLog ($"WebServer:PortNo:{webPortNo}");

			// 输出宏定义
			var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.iOS);
			Loger.BuildLog($"Defines:{defines}");

			// XCode工程目录
			var xcodeProject = outputDir;

			// 输出打包信息
			OutputBuildInfo(buildVersion, projectName);

			// 开发者模式
			if (BuildOptions.Development == buildOptionTmp) {
				// 打包之前，将非资源对象，临时移动到临时文件夹
				//AssetBundles.Common.MoveUnResources();
			}

			// 打包成XCode工程目录
			var report = BuildPipeline.BuildPlayer(
				GetBuildScenes(), 
				xcodeProject,  
				BuildTarget.iOS, buildOptionTmp);

			// 开发者模式
			if (BuildOptions.Development == buildOptionTmp) {
				// 恢复非资源性文件
				//AssetBundles.Common.MoveBackUnResources();
			}
			
#if UNITY_2018_3 || UNITY_2018_3_OR_NEWER
			// 存在错误则，打包编译失败
			if (0 > report.summary.totalErrors)
			{  
				Loger.BuildErrorLog($"Android Build Failed!!!(error:{report.summary})");
				Loger.BuildEnd();
				throw new ApplicationException ();
			}
#else
			// 存在错误则，打包编译失败
			if (false == string.IsNullOrEmpty(report)) {
				Loger.BuildErrorLog($"iOS Build Failed!!!(error:{report})");
				Loger.BuildEnd();
				throw new ApplicationException ();
			}
#endif

			Loger.BuildLog("iOS Build Succeeded.");
			Loger.BuildEnd();
		}

#endregion

#region 上传Shell

		/// <summary>
		/// 生成上传用的Shell脚本
		/// </summary>
		/// <param name="iBuildNo">打包No</param>
		/// <param name="iTarget">打包目标类型(iOS/Android)</param>
		/// <param name="iPath">ipa/apk输出目录</param>
		private static void CreateUploadShell(int iBuildNo, BuildTarget iTarget, string iPath) {
			const string funcBlock = "AssetBundlesBuild:CreateUploadShell()";
			Loger.BuildStart(funcBlock);

			// ReSharper disable HeapView.BoxingAllocation
			var filePath = $"{Application.dataPath}/../Shell/Upload{iTarget}.sh";
			// ReSharper restore HeapView.BoxingAllocation
			if (File.Exists (filePath)) {
				File.Delete (filePath);
			}
			var fs = new FileStream (filePath, FileMode.OpenOrCreate, FileAccess.Write);
			var sw = new StreamWriter(fs);

			// 写入文件头
			sw.WriteLine("#!/bin/bash");
			sw.Flush ();

			sw.WriteLine("");
			sw.Flush ();
			
			// 设定变量
			sw.WriteLine("# 打包No");
			sw.WriteLine("BUILD_NO={0}", iBuildNo);
			sw.Flush ();
			sw.WriteLine("# 上传根目录");
			sw.WriteLine("ROOT_DIR=\"Download\"");
			sw.Flush ();
			sw.WriteLine("# 平台类型");
			sw.WriteLine("BUILD_TARGET={0}", iTarget);
			sw.Flush ();
			sw.WriteLine("# 本地上传路径");
			sw.WriteLine("UPLOAD_FROM_ROOT_DIR={0}/StreamingAssets", Application.dataPath);
			sw.WriteLine("");
			sw.Flush ();
			sw.WriteLine("# App Version");
			sw.WriteLine("APP_VERSION={0}", SysSettings.GetInstance().BuildVersion);
			sw.Flush ();
			
			var resourceServer = SysSettings.GetInstance ().data.network.resourceServer;
			sw.WriteLine("# 认证");
			sw.WriteLine("UPLOAD_SERVER_ROOT_URL=\"ftp://{0}:{1}\"", resourceServer.ipAddresss, resourceServer.portNo);
			sw.WriteLine("VERIFY_CODE={0}:{1}", resourceServer.accountId, resourceServer.pwd);
			sw.WriteLine("");
			sw.Flush ();

			sw.WriteLine("# 检测上传目录");
			sw.WriteLine("# $1 上传目录");
			sw.WriteLine("checkUploadDir()");
			sw.WriteLine("{");
			sw.WriteLine("    # 检测URL");
			sw.WriteLine("    CHK_URL=${UPLOAD_SERVER_ROOT_URL}");
			sw.WriteLine("    ");
			sw.WriteLine("    # 目标文件夹名");
			sw.WriteLine("    CHK_DIRS=$1");
			sw.WriteLine("    CHK_DIRS_ARRAY=(${CHK_DIRS//\\// })");
			sw.WriteLine("    CHK_DIRS_SIZE=${#CHK_DIRS_ARRAY[@]}");
			sw.WriteLine("    if [ $CHK_DIRS_SIZE -ge 1 ]; then");
			sw.WriteLine("    ");
			sw.WriteLine("        # 检测目录");
			sw.WriteLine("        CHK_DIR=${CHK_DIRS_ARRAY[CHK_DIRS_SIZE-1]}");
			sw.WriteLine("        PREV_DIR=${CHK_DIRS}");
			sw.WriteLine("        ");
			sw.WriteLine("        # 检测");
			sw.WriteLine("        if [ -n $CHK_DIR ]; then ");
			sw.WriteLine("            # 取得之前目录");
			sw.WriteLine("            if [ $CHK_DIRS_SIZE -gt 1 ]; then");
			sw.WriteLine("                PREV_DIR=${CHK_DIRS/\\/$CHK_DIR/}");
			sw.WriteLine("            else");
			sw.WriteLine("                PREV_DIR=${CHK_DIRS/$CHK_DIR/}");
			sw.WriteLine("            fi");
			sw.WriteLine("            ");
			sw.WriteLine("            # 检测路径");
			sw.WriteLine("            if [ -z $PREV_DIR ]; then");
			sw.WriteLine("                CHK_URL=\"${CHK_URL}/\"");
			sw.WriteLine("            else");
			sw.WriteLine("                CHK_URL=\"${CHK_URL}/${PREV_DIR}/\"");
			sw.WriteLine("            fi");
			sw.WriteLine("            ");
			sw.WriteLine("            # 检测文件夹");
			sw.WriteLine("            CHK_INFO=`curl ${CHK_URL} -u ${VERIFY_CODE}`");
			sw.WriteLine("            # 已存在目录");
			sw.WriteLine("            EXIST_DIRS=(${CHK_INFO// / })");
			sw.WriteLine("            # 存在标识位");
			sw.WriteLine("            EXIST_FLG=0");
			sw.WriteLine("            for DIR in ${EXIST_DIRS[@]}");
			sw.WriteLine("            do");
			sw.WriteLine("                if [ \"${DIR}\" == \"${CHK_DIR}\" ]; then");
			sw.WriteLine("                    EXIST_FLG=1");
			sw.WriteLine("                    break");
			sw.WriteLine("                fi");
			sw.WriteLine("                ");
			sw.WriteLine("            done");
			sw.WriteLine("            ");
			sw.WriteLine("            if [ $EXIST_FLG -le 0 ]; then");
			sw.WriteLine("                curl ${CHK_URL} -u ${VERIFY_CODE} -X \"MKD ${CHK_DIR}\"");
			sw.WriteLine("                echo \"Create Directory : ${CHK_URL}${CHK_DIR}\"");
			sw.WriteLine("            fi");
			sw.WriteLine("            ");
			sw.WriteLine("        fi");
			sw.WriteLine("    else");
			sw.WriteLine("        echo \"[Error] The dirs for check is not invalid!!(Check Dir=${CHK_DIRS})\"");
			sw.WriteLine("    fi");
			sw.WriteLine("}");
			sw.WriteLine("");
			sw.Flush ();

			sw.WriteLine("# # 文件上传函数");
			sw.WriteLine("# $1 远程上传目录");
			sw.WriteLine("# $2 本地上传文件路径");
			sw.WriteLine("uploadFile()");
			sw.WriteLine("{");
			sw.WriteLine("    # 远程上传目标目录");
			sw.WriteLine("    UPLOAD_DIR=$1");
			sw.WriteLine("    # 本地上传文件");
			sw.WriteLine("    LOCAL_FILE_PATH=$2");
			sw.WriteLine("    ");
			sw.WriteLine("    # 检测URL");
			sw.WriteLine("    UPLOAD_URL=${UPLOAD_SERVER_ROOT_URL}/${UPLOAD_DIR}");
			sw.WriteLine("    #curl ${UPLOAD_URL} -v -u ${VERIFY_CODE} -T \"${LOCAL_FILE_PATH}\"");
			sw.WriteLine("    echo \"${LOCAL_FILE_PATH}\"");
			sw.WriteLine("    curl ${UPLOAD_URL} -u ${VERIFY_CODE} -T \"${LOCAL_FILE_PATH}\"");
			sw.WriteLine("}");
			sw.WriteLine("");
			sw.Flush ();
			
			sw.WriteLine("# 检测目录");
			sw.WriteLine("checkUploadDir $ROOT_DIR");
			sw.WriteLine("checkUploadDir $ROOT_DIR/$BUILD_NO");
			sw.WriteLine("checkUploadDir $ROOT_DIR/$BUILD_NO/$BUILD_TARGET");
			sw.WriteLine("checkUploadDir $ROOT_DIR/$BUILD_NO/$BUILD_TARGET/$APP_VERSION");
		
			sw.WriteLine("# 上传文件");
			sw.WriteLine("uploadFile $ROOT_DIR/$BUILD_NO/$BUILD_TARGET/$APP_VERSION/ {0}", iPath);
			
			sw.Close ();
			sw.Dispose ();

			fs.Close ();
			fs.Dispose ();
			
			Loger.BuildEnd();
		}

#endregion
		
#region 其他处理

		/// <summary>
		/// 取得输出目录.
		/// </summary>
		/// <returns>The output dir.</returns>
		/// <param name="iTarget">打包目标类型.</param>
		/// <param name="iBuildNo">打包No.</param>
		/// <param name="iOutputDir">输出目录（未指定：默认输出根目录）.</param>
		private static string GetOutputDir(BuildTarget iTarget, int iBuildNo, string iOutputDir = null) {
			var outputDir = iOutputDir;
			if (string.IsNullOrEmpty (outputDir)) {
				outputDir = DefaultOutputRootDir;
			}
			// 校验目录
			outputDir = $"{outputDir}/{iBuildNo}/{iTarget.ToString()}";
			
			// 日志输出目录
			Loger.OutputDir = $"{outputDir}/Logs";
			Loger.BuildLog($"GetOutputDir() -> LogDir:{Loger.OutputDir}");
			
			// 工程输出目录
			if (BuildTarget.iOS == iTarget)
			{
				outputDir = $"{outputDir}/XcodeProject";
			}

			// 校验 - 日志输出目录
			if (UtilsTools.CheckAndCreateDirByFullDir (Loger.OutputDir) == false) {
				Loger.BuildErrorLog($"CheckAndCreateDirByFullDir Failed!!(dir:{Loger.OutputDir})");
			}
			// 校验 - 打包输出目录
			if (UtilsTools.CheckAndCreateDirByFullDir (outputDir) == false) {
				Loger.BuildErrorLog($"CheckAndCreateDirByFullDir Failed!!(dir:{outputDir})");
			}
			Loger.BuildLog($"GetOutputDir() -> Dir:{outputDir}");
			
			return outputDir;
		}

		/// <summary>
		/// 输出打包信息(导出的XCode工程 打包ipa文件时使用).
		/// </summary>
		/// <param name="iProjectName">工程名.</param>
		/// <param name="iProjectVersion">工程版本.</param>
		private static void OutputBuildInfo(
			string iProjectName, string iProjectVersion) {
			
			const string funcBlock = "ProjectBuild:OutputBuildInfo()";
			var buildId = SysSettings.GetInstance().BuildId;
			Loger.BuildStart(funcBlock);

			var filePath = $"{Application.dataPath}/../Shell/BuildInfo";
			if (File.Exists (filePath)) {
				File.Delete (filePath);
			}
			var fStream = new FileStream (filePath, FileMode.OpenOrCreate, FileAccess.Write);

			var buildInfo = $"{BuildParameters.BuildTime}:{iProjectName}:{iProjectVersion}:{buildId}";

			Loger.BuildLog($"BuildInfo:{buildInfo}");

			// 获得字节数组
			var data = System.Text.Encoding.Default.GetBytes(buildInfo); 
			// 写入
			fStream.Write (data, 0, data.Length);
			// 清空缓冲区、关闭流
			fStream.Flush ();
			fStream.Close ();
			fStream.Dispose ();

			Loger.BuildEnd();
		}

#endregion

		private static void InitForExportXcodeProject() {
			BuildSettings.GetInstance (BuildSettings.AssetFileDir); 
		}

		/// <summary>
		/// 设定打包信息
		/// </summary>
		/// <param name="iIsAndroid">是否为安卓平台</param>
		private static void SetBuildInfoFromParameters(bool iIsAndroid) {

			// 平台类型
			var platformType = BuildParameters.PlatformType;
			if (PlatformType.None != platformType) {
				SysSettings.GetInstance ().PlatformType = platformType;
			}
			else
			{
				if (PlatformType.None == SysSettings.GetInstance().PlatformType)
				{
					SysSettings.GetInstance().PlatformType = iIsAndroid ? PlatformType.Android : PlatformType.iOS;
				}
			}
			Loger.BuildLog($"PlatformType:{SysSettings.GetInstance ().PlatformType}");

			// 工程名
			var projectName = BuildParameters.ProjectName;
			if (false == string.IsNullOrEmpty (projectName)) {
				SysSettings.GetInstance().BuildName = projectName;
			}
			Loger.BuildLog($"ProjectName:{SysSettings.GetInstance ().BuildName}");

			// 打包ID
			var buildId = BuildParameters.BuildId;
			if (false == string.IsNullOrEmpty (buildId)) {
				SysSettings.GetInstance().BuildId = buildId;
			}
			Loger.BuildLog($"BuildId:{SysSettings.GetInstance ().BuildId}");

			// 打包模式
			var buildMode = BuildParameters.BuildMode;
			SysSettings.GetInstance ().BuildMode = buildMode; 
			Loger.BuildLog($"BuildMode:{SysSettings.GetInstance ().BuildMode}");

			// 版本号
			var buildVersion = BuildParameters.BuildVersion;
			if(false == string.IsNullOrEmpty(buildVersion)) {
				SysSettings.GetInstance().BuildVersion = buildVersion;
			}
			Loger.BuildLog($"BuildVersion:{SysSettings.GetInstance ().BuildVersion}");

			// VersionCode
			var buildVersionCode = BuildParameters.BuildVersionCode;
			if (-1 != buildVersionCode) {
				SysSettings.GetInstance ().BuildVersionCode = buildVersionCode;
			}
			Loger.BuildLog($"BuildVersionCode:{SysSettings.GetInstance ().BuildVersionCode}");

			// 日志等级
			var logLevel = BuildParameters.LogLevel;
			SysSettings.GetInstance ().LogLevel = logLevel;
			Loger.BuildLog($"LogLevel:{SysSettings.GetInstance ().LogLevel}");

			// 打包号
			var buildNumber = BuildParameters.BuildNumber;
			if (-1 < buildNumber) {
				SysSettings.GetInstance ().BuildNumber = buildNumber;
			}
			Loger.BuildLog($"BuildNumber:{SysSettings.GetInstance ().BuildNumber}");

			var fps = BuildParameters.Fps;
			SysSettings.GetInstance ().Fps = fps;

			// 是否跳过下载
			var isSkipDownload = BuildParameters.IsSkipDownload;
			SysSettings.GetInstance ().data.network.SkipDownload = isSkipDownload;
			Loger.BuildLog($"SkipDownload:{SysSettings.GetInstance ().data.network.SkipDownload}");

			// 前端 Host
			var webHost = BuildParameters.WebHost;
			if(false == string.IsNullOrEmpty(webHost)) {
				SysSettings.GetInstance ().data.network.WebHost = webHost;
			}

			// 前端端口号
			var webPortNo = BuildParameters.WebPortNo;
			if (-1 < webPortNo) {
				SysSettings.GetInstance ().data.network.WebPortNo = webPortNo;
			}
			Loger.BuildLog($"Web(Host:{SysSettings.GetInstance ().data.network.WebHost} PortNo:{SysSettings.GetInstance ().data.network.WebPortNo})");
		}
	}
}
