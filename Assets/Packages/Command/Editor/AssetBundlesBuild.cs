using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Packages.AssetBundles;
using Packages.BuildSystem.Editor;
using Packages.Common.Base;
using Packages.Settings;
using Packages.Utils;
using Packages.Logs;

namespace Packages.Command.Editor {

	/// <summary>
	/// 资源打包.
	/// </summary>
	public class AssetBundlesBuild : UnityEditor.Editor {

#region Build Bundles

		/// <summary>
		/// 生成配置文件
		/// </summary>
		[MenuItem("Tools/Config/Check", false, 100)]
		private static void BuildConfigAssets()
		{
			UtilsAsset.CreateAsset<BundlesConfig> ();
			UtilsAsset.CreateAsset<BundlesMap> ();
			UtilsAsset.CreateAsset<BundlesResult> ();
			AssetDatabase.Refresh();
		}

		/// <summary>
		/// IOS资源打包.
		/// </summary>
		[MenuItem("Tools/AssetBundles/Build/IOS", false, 101)]
		public static void BuildForIos() 
		{
			const string funcBlock = "AssetBundlesBuild:BuildForIos()";
			Loger.BuildStart(funcBlock);

			// 开始打包Bundles
			UtilsAsset.StartBuildBundles ();

			BuildAssetBundle(BuildTarget.iOS, true);

			// 生成刷新CND服务器用的Shell
			CreateShellOfRefreshCdn();

			// 生成上传用的Shell
			if (UploadWay.Ftp == BundlesResult.GetInstance().UploadWay)
			{
				CreateUploadShellByFtp();
			} 
			else if (UploadWay.Curl == BundlesResult.GetInstance().UploadWay)
			{
				CreateUploadShellByCurl();
			}


			// 开始打包Bundles
			UtilsAsset.EndBuildBundles ();

			Loger.BuildEnd();
		}

		/// <summary>
		/// 安卓资源打包.
		/// </summary>
		[MenuItem("Tools/AssetBundles/Build/Android", false, 102)]
		public static void BuildForAndroid()
		{
			const string funcBlock = "AssetBundlesBuild:BuildForAndroid()";
			Loger.BuildStart(funcBlock);

			// 开始打包Bundles
			UtilsAsset.StartBuildBundles ();

			BuildAssetBundle(BuildTarget.Android, true);

			// 生成刷新CND服务器用的Shell
			CreateShellOfRefreshCdn();

			// 生成上传用的Shell
			if (UploadWay.Ftp == BundlesResult.GetInstance().UploadWay)
			{
				CreateUploadShellByFtp();
			} 
			else if (UploadWay.Curl == BundlesResult.GetInstance().UploadWay)
			{
				CreateUploadShellByCurl();
			}
			
			// 导出打包结果文件
			var exportDir = BuildParameters.OutputDir;
			BundlesResult.GetInstance().ExportToJsonFile(exportDir);

			// 开始打包Bundles
			UtilsAsset.EndBuildBundles ();

			Loger.BuildEnd();
		}

		const string DevelopPath = "Assets/Resources/Develop";
		/// <summary>
		/// 清理开发资源文件夹.
		/// </summary>
		[MenuItem("Tools/AssetBundles/Clear/DevelopDir", false, 103)]
		private static void ClearDevelopDir()
		{
			AssetDatabase.DeleteAsset(DevelopPath);
			AssetDatabase.Refresh();
		}

		/// <summary>
		/// 打包资源文件
		/// </summary>
		/// <param name="iBuildTarget">打包目标.</param>
		/// <param name="iNeedCompress">压缩标识位.</param>
		private static void BuildAssetBundle(BuildTarget iBuildTarget, bool iNeedCompress = false)
		{
			const string funcBlock = "AssetBundlesBuild:BuildAssetBundle()";
			Loger.BuildStart(funcBlock);
			Loger.BuildLog($"BuildTarget:{iBuildTarget} NeedCompress:{iNeedCompress}");

			// 清空上传信息
			BundlesResult.GetInstance().Clear();
			// 打包名
			BundlesResult.GetInstance().BuildName = SysSettings.GetInstance().BuildName;
			// 打包名
			BundlesResult.GetInstance().AppVersion = SysSettings.GetInstance().BuildVersion;
			// 打包名
			BundlesResult.GetInstance().CheckMode = CheckMode.CustomMd5;
			// 设置上传的打包类型
			BundlesResult.GetInstance().BuildTarget = iBuildTarget.ToString();

			var bcConfig = BundlesConfig.GetInstance();
			if(null == bcConfig || 0 >= bcConfig.Resources.Count) {
				Loger.BuildErrorLog("BundlesConfig is invalid!!!");
				return;
			}

			// 清空依赖关系列表
			var bundlesMap = BundlesMap.GetInstance ();
			if(null == bundlesMap) {
				Loger.BuildErrorLog ("BundlesMap is invalid!!!");
				return;
			}
			bundlesMap.Clear ();

			var allConfig = bcConfig.Resources;

			// make bundle config
			foreach(var bc in allConfig)
			{
				switch (bc.mode)
				{
					// filter file
					case BundleMode.OneDir:
					{
						var bundleId = BundlesMap.GetBundleId(bc.path);
						var bm = BundlesMap.GetOrCreateBundlesMap(bundleId);

						bm.id = bundleId;
						bm.path = bc.path;

						// 取得当前目录的文件列表
						var files = GetAllFiles(bc.path);

						// 遍历文件列表
						foreach(var file in files)
						{
							// .DS_Store文件
							if(file.EndsWith(".DS_Store")) {
								continue;
							}
							// *.meta文件
							if(file.EndsWith(".meta")) {
								continue;
							}

							// 若为忽略文件，则跳过
							if (bcConfig.IsIgnoreFile (bc, file)) {
								bm.RemoveIgnorFile (file);
								continue;
							}
							bm.AddFile(file);
						}

						bundlesMap.Maps.Add(bm);
						break;
					}

					case BundleMode.SceneOneToOne:
					{
						// 取得当前目录的文件列表
						var files = GetAllFiles(bc.path);

						foreach(var file in files)
						{
							// .DS_Store文件
							if(file.EndsWith(".DS_Store")) {
								continue;
							}
							// *.meta文件
							if(file.EndsWith(".meta")) {
								continue;
							}
							// 若非场景文件，则跳过
							if (file.EndsWith (".unity") == false) {
								continue;
							}

							// 若为忽略文件，则跳过
							var bundleId = BundlesMap.GetBundleId(file);
							var bm = BundlesMap.GetOrCreateBundlesMap(bundleId);
							if (bcConfig.IsIgnoreFile (bc, file)) {
								bm.RemoveIgnorFile (file);
								continue;
							}

							bm.id = bundleId;
							bm.path = bc.path;
							bm.type = BundleType.Scene;
							bm.AddFile(file);

							bundlesMap.Maps.Add(bm);
						}

						break;
					}

					case BundleMode.FileOneToOne:
					{
						// 取得当前目录的文件列表
						var files = GetAllFiles(bc.path);

						foreach(var file in files)
						{
							// .DS_Store文件
							if(file.EndsWith(".DS_Store")) {
								continue;
							}
							// *.meta文件
							if(file.EndsWith(".meta")) {
								continue;
							}

							// 若为忽略文件，则跳过
							var bundleId = BundlesMap.GetBundleId(file);
							var bm = BundlesMap.GetOrCreateBundlesMap(bundleId);
							if (bcConfig.IsIgnoreFile (bc, file)) {
								bm.RemoveIgnorFile (file);
								continue;
							}

							bm.id = bundleId;
							bm.path = bc.path;
							bm.AddFile(file);

							bundlesMap.Maps.Add(bm);

						}

						break;
					}

					case BundleMode.TopDirOneToOne:
					{
						// 取得目录列表
						var directories = Directory.GetDirectories (bc.path);
						if (directories.Length <= 0) {
							Loger.BuildWarningLog($"The no subfolder in this path!!!(dir:{bc.path})");
							continue;
						}

						foreach(var dir in directories)
						{
							// 取得当前目录的文件列表
							var files = GetAllFiles(dir);

							var bundleId = BundlesMap.GetBundleId(dir);
							if (string.IsNullOrEmpty (bundleId)) {
								continue;
							}
							var bm = BundlesMap.GetOrCreateBundlesMap(bundleId);
							bm.id = bundleId;
							bm.path = bc.path;

							foreach(var file in files)
							{
								// .DS_Store文件
								if(file.EndsWith(".DS_Store")) {
									continue;
								}
								// *.meta文件
								if(file.EndsWith(".meta")) {
									continue;
								}

								// 若为忽略文件，则跳过
								if (bcConfig.IsIgnoreFile (bc, file)) {
									bm.RemoveIgnorFile (file);
									continue;
								}

								bm.AddFile(file);
							}

							bundlesMap.Maps.Add(bm);
						}

						break;
					}
				}
			}

			// 目录检测
			var checkDir = BundlesResult.GetInstance().BundlesOutputDir;
			if(Directory.Exists(checkDir) == false) {
				Directory.CreateDirectory (checkDir);
			}
			checkDir = BundlesResult.GetInstance().BundlesOutputDirOfNormal;
			if(Directory.Exists(checkDir) == false) {
				Directory.CreateDirectory (checkDir);
			}
			checkDir = BundlesResult.GetInstance().BundlesOutputDirOfScene;
			if(Directory.Exists(checkDir) == false) {
				Directory.CreateDirectory (checkDir);
			}

			var succeeded = false;
			AssetBundleManifest result = null;
			string[] allAssets = null;

			// 一般Bundles
			try {

				var targets = bundlesMap.GetAllNormalBundleTargets();
				const BuildAssetBundleOptions options = BuildAssetBundleOptions.None;
				result = BuildPipeline.BuildAssetBundles(
					BundlesResult.GetInstance().BundlesOutputDirOfNormal, 
					targets, 
					options, 
					iBuildTarget);
				Loger.BuildLog(" -> BuildPipeline.BuildAssetBundles");
				if(result != null) {
					allAssets = result.GetAllAssetBundles();
					if(allAssets != null && targets.Length == allAssets.Length) {
						succeeded =  true;
					}
				}
			} catch (Exception exp) {
				Loger.BuildErrorLog($"BuildAssetBundles Detail : {exp.Message}");
				succeeded = false;
			}

			// 更新导出标志位
			if (succeeded) {
				Loger.BuildLog(" -> BundlesConfig.UpdateBundleStateWhenCompleted");

				var hashCodes = new Dictionary<string, string>();
				foreach(var asset in allAssets) {
					var hashCode = result.GetAssetBundleHash(asset);
					if(string.IsNullOrEmpty(hashCode.ToString())) {
						continue;
					}
					var fileSuffix = BundlesResult.GetInstance ().FileSuffix;
					var key = asset;
					if(string.IsNullOrEmpty(fileSuffix) == false) {
						fileSuffix = fileSuffix.ToLower();
						fileSuffix = $".{fileSuffix}";
						key = key.Replace(fileSuffix, "");
					}
					hashCodes[key] = hashCode.ToString();
				}
				// 初始化检测信息（Hash Code）
				bundlesMap.PushBundleResult(BundleType.Normal, hashCodes);
				Loger.BuildLog(" -> BundlesMap.UpdateUploadList Normal");
			}

			// Scene Bundles
			var targetScenes = bundlesMap.GetAllSceneBundleTargets();
			if (targetScenes != null && targetScenes.Count > 0) {
				foreach (var scene in targetScenes) {
					if (scene?.GetAllTargets () == null || scene.GetAllTargets ().Length <= 0) {
						continue;
					}
					try {

						var options = BuildOptions.BuildAdditionalStreamedScenes;
						if (BuildMode.Debug == SysSettings.GetInstance ().BuildMode) {
							options |= BuildOptions.Development;
						}
						var sceneState = BuildPipeline.BuildPlayer (
							scene.GetAllTargets (),
							BundlesResult.GetLocalSceneBundleFilePath (scene.BundleId),
							iBuildTarget,
							options);
						Loger.BuildLog ($" -> BuildPipeline.BuildStreamedSceneAssetBundle(State:{sceneState})");
					} catch (Exception exp) {
						Loger.BuildErrorLog($"BuildStreamedSceneAssetBundle Detail:{exp.Message}");
						succeeded = false;
					}
				}
			}

			// 更新导出标志位
			if (succeeded) {
				Loger.BuildLog(" -> BundlesConfig.UpdateBundleStateWhenCompleted");

				// 初始化检测信息（Hash Code）
				bundlesMap.PushBundleResult(BundleType.Scene);
				Loger.BuildLog(" -> BundlesMap.UpdateUploadList Scene");
			}

			SysSettings.GetInstance().ExportToJsonFile();
			Loger.BuildLog(" -> BuildInfo.ExportToJsonFile");
			Loger.BuildEnd();
		}


		/// <summary>
		/// 取得指定目录文件列表（包含子目录）.
		/// </summary>
		/// <returns>文件列表.</returns>
		/// <param name="iDirection">文件目录.</param>
		private static IEnumerable<string> GetAllFiles(string iDirection)
		{   
			var filesList = new List<string>();

			try   
			{  
				// 文件夹存在标识位
				var isDir = false == string.IsNullOrEmpty(iDirection) && 
				             Directory.Exists(iDirection);
				if(isDir)
				{
					var files = Directory.GetFiles(iDirection, "*.*", SearchOption.AllDirectories);

					filesList.AddRange(
						files.Where(iStrVal => !string.IsNullOrEmpty(iStrVal))
										.Where(iStrVal => !iStrVal.EndsWith(".ds_store")));
				} else {
					filesList.Add(iDirection);
				}

			}  
			catch (DirectoryNotFoundException exp)   
			{  
				Loger.BuildErrorLog($"The Directory is not exist!!!(dir:{iDirection} detail:{exp.Message})");
			} 

			return filesList;
		}

#endregion
		
#region Create Upload Shell By Ftp (旧)  

		[Obsolete]
		private static void CreateUploadShellByFtp() {
			const string funcBlock = "AssetBundlesBuild:CreateUploadShellByFtp()";
			Loger.BuildStart(funcBlock);

			var filePath = $"{Application.dataPath}/../Shell/UploadABByFtp.sh";
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
			sw.WriteLine("# 上传根目录");
			sw.WriteLine("ROOT_DIR=bundles");
			sw.Flush ();
			sw.WriteLine("# 本地上传路径");
			sw.WriteLine("UPLOAD_FROM_ROOT_DIR={0}/StreamingAssets", Application.dataPath);
			sw.WriteLine("");
			sw.Flush ();
			sw.WriteLine("# 上传目标平台");
			sw.WriteLine("BUILD_TARGET={0}", BundlesResult.GetInstance().BuildTarget);
			sw.Flush ();
			sw.WriteLine("# App Version");
			sw.WriteLine("APP_VERSION={0}", BundlesResult.GetInstance().AppVersion);
			sw.Flush ();

			var resourceServer = SysSettings.GetInstance().data.network.resourceServer;
			sw.WriteLine("# 检测上传目录");
			sw.WriteLine("# $1 上传目录");
			sw.WriteLine("checkUploadDir()");
			sw.WriteLine("{");
			sw.WriteLine("ftp -n<<!");
			sw.WriteLine("open {0} {1}", resourceServer.ipAddresss, resourceServer.portNo);
			sw.WriteLine("user {0} {1}", resourceServer.accountId, resourceServer.pwd);
			sw.WriteLine("binary");
			sw.WriteLine("pwd");
			sw.WriteLine("mkdir $1");
			sw.WriteLine("prompt");
//			sw.WriteLine("ls -l");
			sw.WriteLine("close");
			sw.WriteLine("bye");
			sw.WriteLine("!");
			sw.WriteLine("}");
			sw.WriteLine("");
			sw.Flush ();

			sw.WriteLine("# 文件上传函数(单个)");
			sw.WriteLine("# $1 本地上传目录");
			sw.WriteLine("# $2 上传目标目录");
			sw.WriteLine("# $3 上传目标文件");
			sw.WriteLine("uploadFile()");
			sw.WriteLine("{");
			sw.WriteLine("ftp -n<<!");
			sw.WriteLine("open {0} {1}", resourceServer.ipAddresss, resourceServer.portNo);
			sw.WriteLine("user {0} {1}", resourceServer.accountId, resourceServer.pwd);
			sw.WriteLine("binary");
			// 开启FTP被动模式上传
			sw.WriteLine("quote pasv");
			sw.WriteLine("passive");
			// 设置远程目录
			sw.WriteLine("cd $2");
			sw.WriteLine("pwd");
			// 设置本地目录
			sw.WriteLine("lcd $1");
			sw.WriteLine("prompt off");
			// 上传文件(单个)
			sw.WriteLine("put $3");
			sw.WriteLine("close");
			sw.WriteLine("bye");
			sw.WriteLine("!");
			sw.WriteLine("}");
			sw.WriteLine("");
			sw.Flush ();

			sw.WriteLine("# 文件夹上传函数");
			sw.WriteLine("# $1 本地上传目录");
			sw.WriteLine("# $2 上传目标目录");
			sw.WriteLine("uploadDir()");
			sw.WriteLine("{");
			sw.WriteLine("ftp -n<<!");
			sw.WriteLine("open {0} {1}", resourceServer.ipAddresss, resourceServer.portNo);
			sw.WriteLine("user {0} {1}", resourceServer.accountId, resourceServer.pwd);
			sw.WriteLine("binary");
			// 开启FTP被动模式上传
			sw.WriteLine("quote pasv");
			sw.WriteLine("passive");
			// 设置远程目录
			sw.WriteLine("cd $2");
			sw.WriteLine("pwd");
			// 设置本地目录
			sw.WriteLine("lcd $1");
			sw.WriteLine("prompt off");
			// 上传文件(单个)
			sw.WriteLine("mput *");
			sw.WriteLine("close");
			sw.WriteLine("bye");
			sw.WriteLine("!");
			sw.WriteLine("}");
			sw.WriteLine("");
			sw.Flush ();


			sw.WriteLine("# 检测目录");
			sw.WriteLine("checkUploadDir $ROOT_DIR");
			sw.WriteLine("checkUploadDir $ROOT_DIR/$BUILD_TARGET");
			sw.WriteLine("checkUploadDir $ROOT_DIR/$BUILD_TARGET/$APP_VERSION");
			sw.WriteLine("");
			sw.Flush ();

			sw.WriteLine("# 上传资源文件");
			var targets = BundlesResult.GetInstance ().Targets;
			var normals = targets
				.Where(iO => BundleType.Normal == iO.bundleType)
				.OrderBy(iO => iO.no)
				.ToArray ();
			if (0 < normals.Length) {
				sw.WriteLine ("# 检测一般文件目录");
				sw.WriteLine ("checkUploadDir $ROOT_DIR/$BUILD_TARGET/$APP_VERSION/{0}", BundleType.Normal);

				sw.WriteLine ("# 一般文件");
				foreach (var loop in normals) {
					var fileName = BundlesResult.GetLocalBundleFileName (loop.id, loop.fileType);
					sw.WriteLine ("uploadFile $UPLOAD_FROM_ROOT_DIR/{0} $ROOT_DIR/$BUILD_TARGET/$APP_VERSION/{0} {1}", 
						BundleType.Normal, fileName);
				}
				sw.WriteLine ("");
				sw.Flush ();
			}

			var scenes = targets
				.Where(iO => BundleType.Scene == iO.bundleType)
				.OrderBy(iO => iO.no)
				.ToArray ();
			if (0 < scenes.Length) {
				sw.WriteLine ("# 检测场景文件目录");
				sw.WriteLine ("checkUploadDir $ROOT_DIR/$BUILD_TARGET/$APP_VERSION/{0}", BundleType.Scene);
				sw.WriteLine ("# 场景文件");
				foreach (var loop in scenes) {
					var fileName = BundlesResult.GetLocalBundleFileName (loop.id, loop.fileType);
					sw.WriteLine ("uploadFile $UPLOAD_FROM_ROOT_DIR/{0} $ROOT_DIR/$BUILD_TARGET/$APP_VERSION/{0} {1}", 
						BundleType.Scene, fileName);
				}
				sw.WriteLine ("");
				sw.Flush ();
			}
			
			if (0 < targets.Count) {
				sw.WriteLine ("# 清空上传文件");
				sw.WriteLine ("rm -rfv $UPLOAD_FROM_ROOT_DIR");
				sw.WriteLine ("");
				sw.Flush ();
			}			

			sw.Close ();
			sw.Dispose ();

			fs.Close ();
			fs.Dispose ();

			Loger.BuildEnd();
		}

#endregion

#region Create Upload Shell By Curl (新)

		private static void CreateUploadShellByCurl() {
			const string funcBlock = "AssetBundlesBuild:CreateUploadShellByCurl()";
			Loger.BuildStart(funcBlock);

			var filePath = $"{Application.dataPath}/../Shell/UploadABByCurl.sh";
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
			sw.WriteLine("# 上传根目录");
			sw.WriteLine("ROOT_DIR=bundles");
			sw.Flush ();
			sw.WriteLine("# 本地上传路径");
			sw.WriteLine("UPLOAD_FROM_ROOT_DIR={0}/StreamingAssets", Application.dataPath);
			sw.WriteLine("");
			sw.Flush ();
			sw.WriteLine("# 上传目标平台");
			sw.WriteLine("BUILD_TARGET={0}", BundlesResult.GetInstance().BuildTarget);
			sw.Flush ();
			sw.WriteLine("# App Version");
			sw.WriteLine("APP_VERSION={0}", BundlesResult.GetInstance().AppVersion);
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
			sw.WriteLine("checkUploadDir $ROOT_DIR/$BUILD_TARGET");
			sw.WriteLine("checkUploadDir $ROOT_DIR/$BUILD_TARGET/$APP_VERSION");
			sw.WriteLine("");
			sw.Flush ();

			sw.WriteLine("# 上传资源文件");
			var targets = BundlesResult.GetInstance ().Targets;
			var normals = targets
				.Where(iO => BundleType.Normal == iO.bundleType)
				.OrderBy(iO => iO.no)
				.ToArray ();
			if (0 < normals.Length) {
				sw.WriteLine ("# 检测一般文件目录");
				sw.WriteLine ("checkUploadDir $ROOT_DIR/$BUILD_TARGET/$APP_VERSION/{0}", BundleType.Normal);

				sw.WriteLine ("# 一般文件");
				foreach (var loop in normals) {
					var fileName = BundlesResult.GetLocalBundleFileName (loop.id, loop.fileType);
					sw.WriteLine ("uploadFile $ROOT_DIR/$BUILD_TARGET/$APP_VERSION/{0}/ $UPLOAD_FROM_ROOT_DIR/{0}/{1}",
						BundleType.Normal, fileName);
				}
				sw.WriteLine ("");
				sw.Flush ();
			}

			var scenes = targets
				.Where(iO => BundleType.Scene == iO.bundleType)
				.OrderBy(iO => iO.no)
				.ToArray ();
			if (0 < scenes.Length) {
				sw.WriteLine ("# 检测场景文件目录");
				sw.WriteLine ("checkUploadDir $ROOT_DIR/$BUILD_TARGET/$APP_VERSION/{0}", BundleType.Scene);
				sw.WriteLine ("# 场景文件");
				foreach (var loop in scenes) {
					var fileName = BundlesResult.GetLocalBundleFileName (loop.id, loop.fileType);
					sw.WriteLine ("uploadFile $ROOT_DIR/$BUILD_TARGET/$APP_VERSION/{0}/ $UPLOAD_FROM_ROOT_DIR/{0}/{1}",
						BundleType.Scene, fileName);
				}
				sw.WriteLine ("");
				sw.Flush ();
			}
			
			if (0 < targets.Count) {
				sw.WriteLine ("# 清空上传文件");
				sw.WriteLine ("rm -rfv $UPLOAD_FROM_ROOT_DIR");
				sw.WriteLine ("");
				sw.Flush ();
			}

			sw.Close ();
			sw.Dispose ();

			fs.Close ();
			fs.Dispose ();

			Loger.BuildEnd();
		}

#endregion

#region Create Shell of Refresh CDN

		public static void CreateShellOfRefreshCdn() {
			const string funcBlock = "AssetBundlesBuild:CreateShellOfRefreshCdn()";
			Loger.BuildStart(funcBlock);

			var cdn = SysSettings.GetInstance().data.network.cdnServer;
			if(null == cdn) {
				Loger.BuildErrorLog("CreateShellOfRefreshCDN()::CDN Server info is invalid!!!");
				Loger.BuildEnd();
				return;
			}

			var filePath = $"{Application.dataPath}/../Shell/RefreshCDN.sh";
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
			sw.WriteLine("# 用户名");
			if(string.IsNullOrEmpty(cdn.userName)) {
				Loger.BuildErrorLog("CreateShellOfRefreshCDN()::The username of CDN Server info is invalid or empty!!!");
			}
			sw.WriteLine("username=\"{0}\"", cdn.userName);
			sw.WriteLine("# APIKey");
			if(string.IsNullOrEmpty(cdn.apiKey)) {
				Loger.BuildErrorLog("CreateShellOfRefreshCDN()::The apikey of CDN Server info is invalid or empty!!!");
			}
			sw.WriteLine("apiKey=\"{0}\"", cdn.apiKey);
			sw.Flush ();
			sw.WriteLine("# 刷新时间");
			sw.WriteLine("date=`env LANG=\"en_US.UTF-8\" date -u \"+%a, %d %b %Y %H:%M:%S GMT\"`");
			sw.WriteLine("# 密码");
			sw.WriteLine("password=`echo -en \"$date\" | openssl dgst -sha1 -hmac $apiKey -binary | openssl enc -base64`");
			sw.Flush ();
			sw.WriteLine("# 开始刷新");
			if(string.IsNullOrEmpty(cdn.cdnUrl)) {
				Loger.BuildErrorLog("CreateShellOfRefreshCDN()::The url of CDN Server info is invalid or empty!!!");
			}
			sw.WriteLine("curl -i --url \"{0}\" \\", cdn.cdnUrl);
			sw.WriteLine("-X \"POST\" \\");		
			sw.WriteLine("-u \"$username:$password\" \\");		
			sw.WriteLine("-H \"Date:$date\" \\");		
			sw.WriteLine("-H \"Content-Type: application/json\" \\");		
			sw.WriteLine("-d '{");				
			sw.WriteLine("     \"urlAction\":\"delete\",");	
			sw.WriteLine("     \"dirs\": [");
			for(var idx = 0; idx < cdn.refreshUrls.Count; ++idx)
			{
				sw.WriteLine(0 == idx
					? $"         \"{cdn.refreshUrls[idx]}\""
					: $"       , \"{cdn.refreshUrls[idx]}\"");
			}				
			sw.WriteLine("     ]");				
			sw.WriteLine("   }'");			
			sw.Flush ();

			sw.Close ();
			sw.Dispose ();

			fs.Close ();
			fs.Dispose ();

			Loger.BuildEnd ();
		}

#endregion

	}
}