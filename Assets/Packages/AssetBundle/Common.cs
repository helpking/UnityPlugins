using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using LitJson;
//using Glu.UnityBuildSystem;
using Common;
using Upload;
using AssetBundles;
using BuildSystem;

namespace AssetBundles
{

	public class Common
	{
		static public string configPath = "Assets/Packages/CommandBuildSystem/AssetBundle/AssetBundleConfig.txt";
		//static public string []resPath = {"Assets/Resources", "Assets/Level"};
		static public string unResourcesPathTempDir = "/../ResourceTempDir/";//Application.dataPath + "/../TempDir/";

		static public string sceneBundlesPath = AssetBundleConst.bundlesDir;

		static int ConvertDateTimeInt(System.DateTime time)
		{
			System.DateTime startTime = System.TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1));
			return (int)(time - startTime).TotalSeconds;
		}

		/// <summary>
		/// 打包前将非资源对象，移动到临时文件夹.
		/// </summary>
		public static void MoveUnResources()
		{
			const string funcBlock = "AssetBundle.MoveUnResources()";
			BuildLogger.OpenBlock(funcBlock);

//			AssetBundlesConfig bcConfig = AssetBundlesConfig.ReadConfig();
//
//			foreach(BundleUnResource urpi in bcConfig.UnResources)
//			{
//				bool isDir = false;
//				string dirName = "";
//				string allDirName = "";
//				if(urpi.Path.Contains(".") == false)
//				{
//					isDir = true;
//					int index = urpi.Path.LastIndexOf("/");
//					allDirName = urpi.Path.Substring(0, index);
//					index = allDirName.LastIndexOf("/");
//					dirName = allDirName.Substring(index + 1, allDirName.Length - index - 1);
//
//					string pathTempDir = Application.dataPath + unResourcesPathTempDir + dirName;
//					allDirName = allDirName.Replace("Assets/", "/");
//					allDirName = Application.dataPath + allDirName;
//
//					FileUtil.DeleteFileOrDirectory(allDirName);
//					FileUtil.DeleteFileOrDirectory(allDirName + ".meta");
//
//					//FileUtil.MoveFileOrDirectory(allDirName, pathTempDir);
//					//FileUtil.MoveFileOrDirectory(allDirName + ".meta", pathTempDir + ".meta");
//				}
//				else
//				{
//					Debug.LogError("just dir" + urpi.Path);
//				}
//
//			}
//
//			AssetDatabase.Refresh();

			BuildLogger.CloseBlock(funcBlock);
		}

		/// <summary>
		/// ipa/apk打包完毕后，将非资源项目移回原来目录.
		/// </summary>
		public static void MoveBackUnResources()
		{
			const string funcBlock = "AssetBundle.MoveBackUnResources()";
			BuildLogger.OpenBlock(funcBlock);

//			AssetBundlesConfig bcConfig = AssetBundlesConfig.ReadConfig();
//
//			foreach(BundleUnResource urpi in bcConfig.UnResources)
//			{
//				bool isDir = false;
//				string dirName = "";
//				string allDirName = "";
//				if(urpi.Path.Contains(".") == false)
//				{
//					isDir = true;
//					int index = urpi.Path.LastIndexOf("/");
//					allDirName = urpi.Path.Substring(0, index);
//					index = allDirName.LastIndexOf("/");
//					dirName = allDirName.Substring(index + 1, allDirName.Length - index - 1);
//
//					string pathTempDir = Application.dataPath + unResourcesPathTempDir + dirName;
//					allDirName = allDirName.Replace("Assets/", "/");
//					allDirName = Application.dataPath + allDirName;
//					
//					FileUtil.MoveFileOrDirectory(pathTempDir, allDirName);
//					FileUtil.MoveFileOrDirectory(pathTempDir + ".meta", allDirName + ".meta");
//				}
//				else
//				{
//					Debug.LogError("just dir" + urpi.Path);
//				}
//				//Debug.LogError("dirName " + dirName);
//				//Debug.LogError("allDirName " + allDirName);
//			}
//
//			AssetDatabase.Refresh();

			BuildLogger.CloseBlock(funcBlock);
		}

		/// <summary>
		/// 清空目录（Application.streamingAssetsPath）
		/// </summary>
		public static bool ClearStreamingAssets()
		{
			const string funcBlock = "AssetBundle.ClearStreamingAssets()";
			BuildLogger.OpenBlock(funcBlock);

			if (Directory.Exists (Application.streamingAssetsPath) == true) {
				string[] files = Directory.GetFiles (Application.streamingAssetsPath);
				foreach (string loop in files) {
					File.Delete (loop);
					BuildLogger.LogMessage ("[Delete File] -> {0}", loop);
					if (File.Exists (loop) == true) {
						BuildLogger.LogMessage ("[Delete File Failed] -> {0}", loop);
					}
				}

				string[] subDirs = Directory.GetDirectories (Application.streamingAssetsPath);
				foreach (string subDir in subDirs) {
					files = Directory.GetFiles (subDir);
					foreach (string loop in files) {
						BuildLogger.LogMessage ("[Delete File] -> {0}", loop);
						File.Delete (loop);
						if (File.Exists (loop) == true) {
							BuildLogger.LogMessage ("[Delete File Failed] -> {0}", loop);
						}
					}
					files = Directory.GetFiles (subDir);
					if ((files != null) && (files.Length <= 0)) {
						BuildLogger.LogMessage ("[Delete Directory] -> {0}", subDir);
						Directory.Delete (subDir);
						if (Directory.Exists (subDir) == true) {
							BuildLogger.LogMessage ("[Delete Directory Failed] -> {0}", subDir);
						}
					}
				}
			}
				
			BuildLogger.CloseBlock(funcBlock);
			return true;
		}
	}
}
