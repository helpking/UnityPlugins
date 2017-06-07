using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using LitJson;
using Common;
using Upload;
using BuildSystem;
using AssetBundles;

namespace AssetBundles
{
	public class AssetBundleEditor : Editor
	{

		/// <summary>
		/// 取得当前选中Asset的路径.
		/// </summary>
		/// <returns>当前选中Asset的路径.</returns>
		static string GetCurPath()
		{
			UnityEngine.Object[] obj = Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.Assets) as UnityEngine.Object[];
			string path = AssetDatabase.GetAssetPath(obj[0]);

			if(path.Contains(".") == false)
			{
				path += "/";
			}

			return path;
		}
		#region Total
		[UnityEditor.MenuItem("Assets/Bundles/Clear", false, 600)]
		static void Clear() {
			const string funcBlock = "AssetBundleEditor.Clear()";
			BuildLogger.OpenBlock(funcBlock);

			BundlesConfig bcConfig = BundlesConfig.GetInstance();
			if (bcConfig != null) {
				bcConfig.Clear ();
			}

			BundlesMap map = BundlesMap.GetInstance ();
			if (map != null) {
				map.Clear ();
			}

			UploadList info = UploadList.GetInstance ();
			if (info != null) {
				info.Clear ();
			}

			UtilityAsset.AssetsRefresh ();

			BuildLogger.CloseBlock(funcBlock);
		}
		#endregion

		#region Config

		[UnityEditor.MenuItem("Assets/Bundles/Config/Clear", false, 600)]
		static void ClearConfig() {
			const string funcBlock = "AssetBundleEditor.ClearConfig()";
			BuildLogger.OpenBlock(funcBlock);

			BundlesConfig bcConfig = BundlesConfig.GetInstance();
			if (bcConfig != null) {
				bcConfig.Clear ();
			}

			UtilityAsset.AssetsRefresh ();

			BuildLogger.CloseBlock(funcBlock);
		}

		/// <summary>
		/// 清空打包资源列表.
		/// </summary>
		[UnityEditor.MenuItem("Assets/Bundles/Config/Reset", false, 600)]
		static void ResetConfig() {
			BundlesConfig bcConfig = BundlesConfig.GetInstance();
			if (bcConfig != null) {
				bcConfig.Reset ();
			}

			BundlesMap map = BundlesMap.GetInstance();
			if (map != null) {
				map.Clear ();
			}

			AssetBundles.Common.ClearStreamingAssets ();

			UtilityAsset.AssetsRefresh ();
		}

		#endregion
		#region Config Resource

		[UnityEditor.MenuItem("Assets/Bundles/Config/Resources/OneDir", false, 600)]
		static void BundleOneDir()
		{
			const string funcBlock = "AssetBundleEditor.BundleOneDir()";
			BuildLogger.OpenBlock(funcBlock);

			string path = GetCurPath();

			BundlesConfig bcConfig = BundlesConfig.GetInstance();
			if (bcConfig != null) {
				bcConfig.AddResource (path, BundleMode.OneDir);
			}

			UtilityAsset.AssetsRefresh ();

			BuildLogger.CloseBlock(funcBlock);
		}

		[UnityEditor.MenuItem("Assets/Bundles/Config/Resources/FileOneToOne", false, 600)]
		static void BundleFileOneToOne()
		{
			string path = GetCurPath();

			BundlesConfig bcConfig = BundlesConfig.GetInstance();
			if (bcConfig != null) {
				bcConfig.AddResource (path, BundleMode.FileOneToOne);
			}
			UtilityAsset.AssetsRefresh ();
		}

		[UnityEditor.MenuItem("Assets/Bundles/Config/Resources/TopDirOneToOne", false, 600)]
		static void BundleTopDirOneToOne()
		{
			string path = GetCurPath();

			BundlesConfig bcConfig = BundlesConfig.GetInstance();
			if (bcConfig != null) {
				bcConfig.AddResource (path, BundleMode.TopDirOneToOne);
			}
			UtilityAsset.AssetsRefresh ();
		}

		[UnityEditor.MenuItem("Assets/Bundles/Config/Resources/SceneOneToOne", false, 600)]
		static void BundleSceneOneToOne()
		{
			string path = GetCurPath();

			BundlesConfig bcConfig = BundlesConfig.GetInstance();
			if (bcConfig != null) {
				bcConfig.AddResource (path, BundleMode.SceneOneToOne);
			}
			UtilityAsset.AssetsRefresh ();
		}

		/// <summary>
		/// 从打包配置信息中移除当前指定对象.
		/// </summary>
		[UnityEditor.MenuItem("Assets/Bundles/Config/Resources/Remove", false, 600)]
		static void RemoveSource()
		{
			string curPath = GetCurPath();
			int lastIndex = curPath.LastIndexOf ("/");
			lastIndex = (lastIndex == (curPath.Length - 1)) ? curPath.Length : (lastIndex + 1);
			string resourcePath = curPath.Substring (0, lastIndex);

			BundlesConfig bcConfig = BundlesConfig.GetInstance();
			if (bcConfig != null) {
				bcConfig.RemoveResource (curPath);
			}
			UtilityAsset.AssetsRefresh ();
		}

		/// <summary>
		/// 清空打包资源列表.
		/// </summary>
		[UnityEditor.MenuItem("Assets/Bundles/Config/Resources/Clear", false, 600)]
		static void ClearAll() {
			BundlesConfig bcConfig = BundlesConfig.GetInstance();
			if (bcConfig != null) {
				bcConfig.ClearResources ();
			}
			UtilityAsset.AssetsRefresh ();
		}

		/// <summary>
		/// 将当前目录或者文件，添加到菜单.
		/// </summary>
		[UnityEditor.MenuItem("Assets/Bundles/Config/Resources/Ignore/Ignore", false, 600)]
		static void IgnoreCurrentTarget()
		{
			string curPath = GetCurPath();
			int lastIndex = curPath.LastIndexOf ("/");
			lastIndex = (lastIndex == (curPath.Length - 1)) ? curPath.Length : (lastIndex + 1);
			string resourcePath = curPath.Substring (0, lastIndex);

			// 设定忽略列表
			BundlesConfig bcConfig = BundlesConfig.GetInstance();
			if (bcConfig != null) {
				bcConfig.AddIgnoreList (resourcePath, curPath);
			}
			UtilityAsset.AssetsRefresh ();
		}

		/// <summary>
		/// 将当前目录或者文件，添加到菜单.
		/// </summary>
		[UnityEditor.MenuItem("Assets/Bundles/Config/Resources/Ignore/Remove", false, 600)]
		static void RemoveIgnoreCurrentTarget()
		{
			string curPath = GetCurPath();
			int lastIndex = curPath.LastIndexOf ("/");
			lastIndex = (lastIndex == (curPath.Length - 1)) ? curPath.Length : (lastIndex + 1);
			string resourcePath = curPath.Substring (0, lastIndex);

			// 设定忽略列表
			BundlesConfig bcConfig = BundlesConfig.GetInstance();
			if (bcConfig != null) {
				bcConfig.RemoveIgnoreInfo (resourcePath, curPath);
			}
			UtilityAsset.AssetsRefresh ();
		}

		/// <summary>
		/// 清空打包资源列表.
		/// </summary>
		[UnityEditor.MenuItem("Assets/Bundles/Config/Resources/Ignore/Clear", false, 600)]
		static void ClearAllIgnoreInfo() {

			string curPath = GetCurPath();
			int lastIndex = curPath.LastIndexOf ("/");
			lastIndex = (lastIndex == (curPath.Length - 1)) ? curPath.Length : (lastIndex + 1);
			string resourcePath = curPath.Substring (0, lastIndex);

			BundlesConfig bcConfig = BundlesConfig.GetInstance();
			if (bcConfig != null) {
				bcConfig.ClearAllIgnoreInfo (resourcePath);
			}
			UtilityAsset.AssetsRefresh ();
		}

		#endregion

		#region Config UnResource

		/// <summary>
		/// 设定当前对象为非资源对象，并添加到打包资源配置信息.
		/// </summary>
		[UnityEditor.MenuItem("Assets/Bundles/Config/UnResources/Add", false, 600)]
		static void AddUnResource()
		{
			string path = GetCurPath();

			BundlesConfig bcConfig = BundlesConfig.GetInstance();
			if (bcConfig != null) {
				bcConfig.AddUnResource (path);
			}
			UtilityAsset.AssetsRefresh ();
		}

		/// <summary>
		/// 从当前打包资源配置信息的非资源列表中移除当前对象.
		/// </summary>
		[UnityEditor.MenuItem("Assets/Bundles/Config/UnResources/Remove", false, 600)]
		static void RemoveUnResource()
		{
			string curPath = GetCurPath();
			int lastIndex = curPath.LastIndexOf ("/");
			lastIndex = (lastIndex == (curPath.Length - 1)) ? curPath.Length : (lastIndex + 1);
			string resourcePath = curPath.Substring (0, lastIndex);

			BundlesConfig bcConfig = BundlesConfig.GetInstance();
			if (bcConfig != null) {
				bcConfig.RemoveUnResource (resourcePath);
			}
			UtilityAsset.AssetsRefresh ();
		}

		/// <summary>
		/// 清空非资源列表.
		/// </summary>
		[UnityEditor.MenuItem("Assets/Bundles/Config/UnResources/Clear", false, 600)]
		static void ClearAllUnResources() {
			BundlesConfig bcConfig = BundlesConfig.GetInstance();
			if (bcConfig != null) {
				bcConfig.ClearUnResources ();
			}
			UtilityAsset.AssetsRefresh ();
		}
		#endregion

		#region Config File

		/// <summary>
		/// 从JSON文件导入打包配置信息.
		/// </summary>
		[UnityEditor.MenuItem("Assets/Bundles/Config/File/Json/Import", false, 600)]
		static void ImportFromConfigJsonFile() {
			
			const string funcBlock = "AssetBundleEditor.ImportFromJsonFile()";
			BuildLogger.OpenBlock(funcBlock);

			BundlesConfig bcConfig = BundlesConfig.GetInstance();
			if (bcConfig != null) {
				bcConfig.ImportFromJsonFile ();
			}

			UtilityAsset.AssetsRefresh ();
			BuildLogger.CloseBlock(funcBlock);
		}

		/// <summary>
		/// 将打包配置信息导出为JSON文件.
		/// </summary>
		[UnityEditor.MenuItem("Assets/Bundles/Config/File/Json/Export", false, 600)]
		static void ExportToConfigJsonFile() {

			const string funcBlock = "AssetBundleEditor.ExportToJsonFile()";
			BuildLogger.OpenBlock(funcBlock);

			BundlesConfig bcConfig = BundlesConfig.GetInstance();
			if (bcConfig != null) {
				bcConfig.ExportToJsonFile ();
			}

			UtilityAsset.AssetsRefresh ();
			BuildLogger.CloseBlock(funcBlock);
		}

		#endregion 

		#region Map File

		/// <summary>
		/// 从JSON文件导入打包配置信息(Map).
		/// </summary>
		[UnityEditor.MenuItem("Assets/Bundles/Map/File/Json/Import", false, 600)]
		static void ImportFromMapJsonFile() {
			
			const string funcBlock = "AssetBundleEditor.ImportFromMapJsonFile()";
			BuildLogger.OpenBlock(funcBlock);

			BundlesMap map = BundlesMap.GetInstance ();
			if (map != null) {
				map.ImportFromJsonFile();
			}

			UtilityAsset.AssetsRefresh ();
			BuildLogger.CloseBlock(funcBlock);
		}

		/// <summary>
		/// 将打包配置信息导出为JSON文件(Map).
		/// </summary>
		[UnityEditor.MenuItem("Assets/Bundles/Map/File/Json/Export", false, 600)]
		static void ExportToMapJsonFile() {

			const string funcBlock = "AssetBundleEditor.ExportToMapJsonFile()";
			BuildLogger.OpenBlock(funcBlock);

			BundlesMap map = BundlesMap.GetInstance ();
			if (map != null) {
				map.ExportToJsonFile();
			}

			UtilityAsset.AssetsRefresh ();
			BuildLogger.CloseBlock(funcBlock);
		}

		/// <summary>
		/// 清空 bundles map.
		/// </summary>
		[UnityEditor.MenuItem("Assets/Bundles/Map/Clear", false, 600)]
		static void ClearBundlesMap() {
			const string funcBlock = "AssetBundleEditor.ClearBundlesMap()";
			BuildLogger.OpenBlock(funcBlock);

			BundlesMap map = BundlesMap.GetInstance ();
			if (map != null) {
				map.Clear ();
			}

			UtilityAsset.AssetsRefresh ();
			BuildLogger.CloseBlock(funcBlock);
		}

		#endregion

		#region Build Bundles

		/// <summary>
		/// IOS资源打包.
		/// </summary>
		[UnityEditor.MenuItem("Tools/AssetBundles/BuildForIOS")]
		static void BuildForIOS() 
		{
			const string funcBlock = "AssetBundleEditor.BuildForIOS()";
			BuildLogger.OpenBlock(funcBlock);

			// 开始打包Bundles
			UtilityAsset.StartBuildBundles ();
				
			BuildAssetBundle(BuildTarget.iOS, true);

			// 开始打包Bundles
			UtilityAsset.EndBuildBundles ();

			BuildLogger.CloseBlock(funcBlock);
		}

		/// <summary>
		/// 安卓资源打包.
		/// </summary>
		[@MenuItem("Tools/AssetBundles/BuildForAndroid")]
		static void BuildForAndroid()
		{
			const string funcBlock = "AssetBundleEditor.BuildForAndroid()";
			BuildLogger.OpenBlock(funcBlock);

			// 开始打包Bundles
			UtilityAsset.StartBuildBundles ();

			BuildAssetBundle(BuildTarget.Android, true);

			// 开始打包Bundles
			UtilityAsset.EndBuildBundles ();

			BuildLogger.CloseBlock(funcBlock);
		}
			
		#endregion

		#region private 

		/// <summary>
		/// 更新AssetBundleName.
		/// </summary>
		/// <param name="iFilePath">文件路径.</param>
		private static void UpdateAssetBundleName(string iFilePath)  {
			if (IsIgnoreFileBySuffix (iFilePath) == true) {
				return;
			}

			AssetImporter importer = AssetImporter.GetAtPath(iFilePath); 
			if (importer != null) {
				if (Directory.Exists (iFilePath) == true) {
					importer.assetBundleName = ""; 
					return;
				}

				if (IsIgnoreFileByDir (iFilePath) == true) {
					importer.assetBundleName = ""; 
					return;
				}

				importer.assetBundleName = iFilePath;  
				importer.assetBundleVariant = "";  
				Debug.Log("UpdateAssetBundleName assetBundleName: " + importer.assetBundleName); 
			}
		}

		/// <summary>
		/// 忽略文件夹.
		/// </summary>
		private static string[] ignore_dirs = { "testBlackList" };  

		/// <summary>
		/// 根据路径，判断文件是否是忽略文件夹中的文件.
		/// </summary>
		/// <returns><c>true</c> 是; 否, <c>false</c>.</returns>
		/// <param name="iFilePath">文件路径.</param>
		private static bool IsIgnoreFileByDir(string iFilePath)  {
			List<string> ignoreList = new List<string>(ignore_dirs); 
			string[] dirNames = iFilePath.Split('/');  
			foreach (string ignore in ignoreList) {
				foreach (string loop in dirNames) {
					if (loop.Equals (ignore) == true) {
						return true;
					}
				}
			}
			return false;
		}

		/// <summary>
		/// 忽略文件后缀列表.
		/// </summary>
		private static string[] ignore_suffixs = { ".cs", ".meta" };  

		/// <summary>
		/// 根据文件后缀名，判断是否是忽略文件.
		/// </summary>
		/// <returns><c>true</c> 是忽略文件; 不是忽略文件, <c>false</c>.</returns>
		/// <param name="iFilePath">文件路径.</param>
		private static bool IsIgnoreFileBySuffix(string iFilePath)  
		{  
			List<string> ignoreList = new List<string>(ignore_suffixs);  
			string suffix = Path.GetExtension(iFilePath);  
			if (string.IsNullOrEmpty (suffix) == true) {
				return false;
			}
			suffix = suffix.ToLower ();
			foreach(string loop in ignoreList)  
			{  
				if (suffix.Equals (loop) == true) {
					return true; 
				}
			}  
			return false;  
		} 

		/// <summary>
		/// 打包资源文件
		/// </summary>
		/// <param name="buildTarget">Build target.</param>
		/// <param name="needCompress">If set to <c>true</c> need compress.</param>
		private static void BuildAssetBundle(BuildTarget buildTarget, bool needCompress = false)
		{
			const string funcBlock = "AssetBundle.BuildAssetBundle()";
			BuildLogger.OpenBlock(funcBlock);

			// 设置上传的打包类型
			UploadList.GetInstance().BuildTarget = buildTarget.ToString();

			BundlesConfig bcConfig = BundlesConfig.GetInstance();
			BundlesMap bundlesMap = BundlesMap.GetInstance ();
			List<BundleResource> allConfig = bcConfig.Resources;

			//make bundle config
			foreach(BundleResource bc in allConfig)
			{

				bool needBuild = false;
				if (BuildTarget.iOS == buildTarget) {
					needBuild = bcConfig.isIosNeedToBuild (bc);
				} 
				if (BuildTarget.Android == buildTarget) {
					needBuild = bcConfig.isAndroidNeedToBuild (bc);
				}
				// 不需要打包
				if(needBuild == false) {
					continue;
				}

				//filter file
				if(bc.Mode == BundleMode.OneDir)
				{
					string bundleId = BundlesMap.GetBundleID (bc.Path);
					BundleMap bm = bundlesMap.GetOrCreateBundlesMap(bundleId);

					bm.ID = bundleId;
					bm.Path = bc.Path;

					// 取得当前目录的文件列表
					List<string> files = GetAllFiles(bc.Path);

					// 遍历文件列表
					foreach(string file in files)
					{
						// .DS_Store文件
						if(file.EndsWith(".DS_Store") == true) {
							continue;
						}
						// *.meta文件
						if(file.EndsWith(".meta") == true) {
							continue;
						}

						// 若为忽略文件，则跳过
						if (bcConfig.isIgnoreFile (bc, file) == true) {
							bm.RemoveIgnorFile (file);
							continue;
						}
						bm.AddFile(file);
					}

					bundlesMap.AddTarget(bm);
				}
				else if(bc.Mode == BundleMode.SceneOneToOne)
				{
					// 取得当前目录的文件列表
					List<string> files = GetAllFiles(bc.Path);

					foreach(string file in files)
					{
						// .DS_Store文件
						if(file.EndsWith(".DS_Store") == true) {
							continue;
						}
						// *.meta文件
						if(file.EndsWith(".meta") == true) {
							continue;
						}
						// 若非场景文件，则跳过
						if (file.EndsWith (".unity") == false) {
							continue;
						}

						// 若为忽略文件，则跳过
						string bundleId = BundlesMap.GetBundleID(file);
						BundleMap bm = bundlesMap.GetOrCreateBundlesMap(bundleId);
						if (bcConfig.isIgnoreFile (bc, file) == true) {
							bm.RemoveIgnorFile (file);
							continue;
						}
							
						bm.ID = bundleId;
						bm.Path = bc.Path;
						bm.Type = TBundleType.Scene;
						bm.AddFile(file);

						bundlesMap.AddTarget(bm);
					}
				}
				else if(bc.Mode == BundleMode.FileOneToOne)
				{
					// 取得当前目录的文件列表
					List<string> files = GetAllFiles(bc.Path);

					foreach(string file in files)
					{
						// .DS_Store文件
						if(file.EndsWith(".DS_Store") == true) {
							continue;
						}
						// *.meta文件
						if(file.EndsWith(".meta") == true) {
							continue;
						}

						// 若为忽略文件，则跳过
						string bundleId = BundlesMap.GetBundleID(file);
						BundleMap bm = bundlesMap.GetOrCreateBundlesMap(bundleId);
						if (bcConfig.isIgnoreFile (bc, file) == true) {
							bm.RemoveIgnorFile (file);
							continue;
						}

						bm.ID = bundleId;
						bm.Path = bc.Path;
						bm.AddFile(file);

						bundlesMap.AddTarget(bm);

					}
				}
				else if(bc.Mode == BundleMode.TopDirOneToOne)
				{
					// 取得目录列表
					string[] directories = Directory.GetDirectories (bc.Path);
					if ((directories == null) || (directories.Length <= 0)) {
						BuildLogger.LogWarning ("The no subfolder in this path!!!(dir:{0})", 
							bc.Path);
						continue;
					}

					foreach(string dir in directories)
					{
						// 取得当前目录的文件列表
						List<string> files = GetAllFiles(dir);

						string bundleId = BundlesMap.GetBundleID(dir);
						bundleId = BundlesMap.GetBundleID(dir);
						if (string.IsNullOrEmpty (bundleId) == true) {
							continue;
						}
						BundleMap bm = bundlesMap.GetOrCreateBundlesMap(bundleId);
						bm.ID = bundleId;
						bm.Path = bc.Path;

						foreach(string file in files)
						{
							// .DS_Store文件
							if(file.EndsWith(".DS_Store") == true) {
								continue;
							}
							// *.meta文件
							if(file.EndsWith(".meta") == true) {
								continue;
							}

							// 若为忽略文件，则跳过
							if (bcConfig.isIgnoreFile (bc, file) == true) {
								bm.RemoveIgnorFile (file);
								continue;
							}

							bm.AddFile(file);
						}

						bundlesMap.AddTarget(bm);
					}
				}
			}

			// 目录检测
			string checkDir = UploadList.GetInstance().BundlesOutputDir;
			if(Directory.Exists(checkDir) == false) {
				Directory.CreateDirectory (checkDir);
			}
			checkDir = UploadList.GetInstance().BundlesOutputDirOfNormal;
			if(Directory.Exists(checkDir) == false) {
				Directory.CreateDirectory (checkDir);
			}
			checkDir = UploadList.GetInstance().AssetBundleDirNameOfScenes;
			if(Directory.Exists(checkDir) == false) {
				Directory.CreateDirectory (checkDir);
			}

			bool successed = false;
			AssetBundleManifest result = null;
			string[] allAssets = null;
			AssetBundleBuild[] targets = null;

			// 一般Bundles
			try {

				targets = bundlesMap.GetAllNormalBundleTargets();
				BuildAssetBundleOptions options = BuildAssetBundleOptions.UncompressedAssetBundle;
				result = BuildPipeline.BuildAssetBundles(
					UploadList.GetInstance().BundlesOutputDirOfNormal, 
					targets, 
					options, 
					buildTarget);
				BuildLogger.LogMessage(" -> BuildPipeline.BuildAssetBundles");
				if(result != null) {
					allAssets = result.GetAllAssetBundles();
					if((allAssets != null) && (targets.Length == allAssets.Length)) {
						successed =  true;
					}
				}
			} catch (Exception exp) {
				BuildLogger.LogException("[Exception BuildAssetBundles] Detail : {0}", exp.Message);
				successed = false;
			}

			// 更新导出标志位
			if (successed == true) {
				bcConfig.UpdateBundleStateWhenCompleted (buildTarget);
				BuildLogger.LogMessage (" -> BundlesConfig.UpdateBundleStateWhenCompleted");

				Dictionary<string, string> hashCodes = new Dictionary<string, string>();
				foreach(string asset in allAssets) {
					Hash128 hashCode = result.GetAssetBundleHash(asset);
					if(string.IsNullOrEmpty(hashCode.ToString()) == true) {
						continue;
					}
					string fileSuffix = BundlesConfig.GetInstance ().FileSuffix;
					fileSuffix = fileSuffix.ToLower();
					string key = asset;
					if(string.IsNullOrEmpty(fileSuffix) == false) {
						fileSuffix = string.Format(".{0}", fileSuffix);
						key = key.Replace(fileSuffix, "");
						hashCodes[key] = hashCode.ToString();
					}
				}
				// 初始化检测信息（Hash Code）
				bundlesMap.UpdateUploadList (TBundleType.Normal, hashCodes);
				BuildLogger.LogMessage(" -> BundlesMap.UpdateUploadList Normal");
			}

			// Scene Bundles
			List<SceneBundleInfo> targetScenes = bundlesMap.GetAllSceneBundleTargets();
			foreach (SceneBundleInfo scene in targetScenes) {
				
				try {

					BuildOptions options = BuildOptions.BuildAdditionalStreamedScenes;
					if(TBuildMode.Debug == BuildInfo.GetInstance().BuildMode) {
						options |= BuildOptions.Development;
					}
					string sceneState = BuildPipeline.BuildPlayer (
						scene.GetAllTargets(),
						UploadList.GetLocalSceneBundleFilePath(scene.BundleId),
						buildTarget,
						options);
					BuildLogger.LogMessage (" -> BuildPipeline.BuildStreamedSceneAssetBundle(State:)", sceneState);
				} catch (Exception exp) {
					BuildLogger.LogException ("[Exception BuildStreamedSceneAssetBundle] Detail : {0}", exp.Message);
					successed = false;
				}
			}

			// 更新导出标志位
			if (successed == true) {
				bcConfig.UpdateBundleStateWhenCompleted (buildTarget);
				BuildLogger.LogMessage (" -> BundlesConfig.UpdateBundleStateWhenCompleted");

				// 初始化检测信息（Hash Code）
				bundlesMap.UpdateUploadList (TBundleType.Scene);
				BuildLogger.LogMessage(" -> BundlesMap.UpdateUploadList Scene");
			}

			BuildInfo.GetInstance().ExportToJsonFile();
			BuildLogger.LogMessage(" -> BuildInfo.ExportToJsonFile");

			BuildLogger.CloseBlock(funcBlock);
		}

		/// <summary>
		/// 取得指定目录文件列表（包含子目录）.
		/// </summary>
		/// <returns>文件列表.</returns>
		/// <param name="iDirection">文件目录.</param>
		static List<string> GetAllFiles(string iDirection)
		{   
			List<string> filesList = new List<string>();

			try   
			{  
				string[] files = Directory.GetFiles(iDirection, "*.*", SearchOption.AllDirectories);

				foreach(string strVal in files)
				{
					if(string.IsNullOrEmpty(strVal)) {
						continue;
					}
					if(strVal.EndsWith(".ds_store") == true) {
						continue;
					}
					filesList.Add(strVal);
				}

			}  
			catch (System.IO.DirectoryNotFoundException exp)   
			{  
				BuildLogger.LogException ("The Directory is not exist!!!(dir:{0} detail:{1})", 
					iDirection, exp.Message);
			} 

			return filesList;
		}

		#endregion
	}
}
