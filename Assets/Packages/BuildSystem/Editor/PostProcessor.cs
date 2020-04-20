using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using UnityEngine;
using Packages.BuildSystem.iOS;
using Packages.BuildSystem.Settings;
using Packages.Common.Base;
using Packages.Logs;

namespace Packages.BuildSystem.Editor
{
	public class PostProcessor : ClassExtension
	{
//		private static string _specialAttentionFolder = "Assets/Packages";
//		private static string _specialFolder = "_PROJECT_ROOT_";
//		private static bool _debugOutput = true;
//		private static bool _excessDebugOutput = false;
//
//		private static string _packagesFolder = "Assets/Packages";

        [PostProcessBuild(999)]
		public static void OnPostprocessiOSBuild(BuildTarget iTarget, string iBuiltProjectPath)
		{
			Loger.BuildStart("PostProcessor::OnPostprocessiOSBuild()");
			Loger.Info ($"Target:{iTarget} ProPath:{iBuiltProjectPath}");

			// iOS
			if (iTarget != BuildTarget.iOS) {
				Loger.BuildEnd();
				return;
			}

			const string targetProjectName = "Unity-iPhone";
			var buildSetting = BuildSettings.GetInstance(BuildSettings.AssetFileDir);
			if(null == buildSetting) {
				return;
			}
			// 取得设定情报列表
			var settings = buildSetting.GetXcSettingInfo(targetProjectName);
			if (settings == null || settings.Length <= 0) {
				Loger.BuildEnd();
				return;
			}

			var pbxprojPath = PBXProject.GetPBXProjectPath(iBuiltProjectPath);
			var project = new PBXProject();
			project.ReadFromString(File.ReadAllText(pbxprojPath));
			var targetGuid = project.TargetGuidByName(targetProjectName);

			// BuildMode(debug/release/store)
			var debugGuid = project.BuildConfigByName (targetGuid, "Debug");
			var releaseGuid = project.BuildConfigByName (targetGuid, "Release");
			var releaseForProfilingGuid = project.BuildConfigByName (targetGuid, "ReleaseForProfiling");
			var releaseForRunningGuid = project.BuildConfigByName (targetGuid, "ReleaseForRunning");

			foreach(var item in settings) {

				switch(item.type) {
				case TxcSettingInfoType.ReplaceSource:
				{
					var list = item.Value.Values;
					if (list != null) {
						foreach (var value in list)
						{
							ReplaceSource(iBuiltProjectPath, Convert.ToString(value));
							Loger.BuildLog($"Replace Source {value} -> {iBuiltProjectPath}");
						}
					}
				}
					break;
				case TxcSettingInfoType.FrameWorks:
				{
					var list = item.Value.Values;
					if (list != null) { 
						foreach (var frameWork in list)
						{
							var strTmp = Convert.ToString(frameWork);
#if UNITY_2017_1_OR_NEWER
							if (project.ContainsFramework(targetGuid, strTmp) == false)
							{
#else
							if (project.HasFramework (strTmp) == false) {
#endif
								project.AddFrameworkToProject(targetGuid, strTmp, false);
								Loger.BuildLog($"Add FrameWork -> {strTmp}");
							}
						}
					}
				}
					break;
				case TxcSettingInfoType.Libraries:
				{
					var list = item.Value.Values;
					if (list != null)
					{
						foreach (var library in list)
						{
							var strTmp = Convert.ToString(library);
							var fileGuid = project.AddFile("usr/lib/" + strTmp, "Frameworks/" + strTmp,
								PBXSourceTree.Sdk);
							project.AddFileToBuild(targetGuid, fileGuid);
							Loger.BuildLog($"Add Library -> {strTmp}");
						}
					}
				}
					break;
				case TxcSettingInfoType.IncludeFiles:
				{
					var list = item.Value.Values;
					if (list != null)
					{
						foreach (var file in list)
						{
							var strTmp = Convert.ToString(file);
							string addFilePath = null;
							PreSetFileToProject(iBuiltProjectPath, strTmp, ref addFilePath);

							if (string.IsNullOrEmpty(addFilePath)) continue;
							var fileGuid = project.AddFile(addFilePath, addFilePath);
							project.AddFileToBuild(targetGuid, fileGuid);

							Loger.BuildLog($"Add File -> {strTmp}");
						}
					}
				}
					break;
				case TxcSettingInfoType.IncludeFolders:
				{
					var list = item.Value.Values;
					if (list != null)
					{
						foreach (var folder in list)
						{
							var strTmp = Convert.ToString(folder);
							string copyTo = null;
							string addDirReference = null;

							PreSetFolderToProject(iBuiltProjectPath, strTmp, ref copyTo, ref addDirReference);
							if (string.IsNullOrEmpty(copyTo) || string.IsNullOrEmpty(addDirReference))
								continue;
							project.AddFolderReference(copyTo, addDirReference);
							Loger.BuildLog($"Add Folder -> {strTmp}");
						}
					}
				}
					break;
				case TxcSettingInfoType.Bool:
				{
					var debugValue = (TxcBool)item.Debug.Value;
					// Debug
					project.SetBuildPropertyForConfig(debugGuid, item.key,
						TxcBool.Yes == debugValue ? "YES" : "NO");

					Loger.BuildLog($"Add Bool(Debug) -> Key:{item.key} Value:{(TxcBool.Yes == debugValue ? "YES" : "NO")}");

					// Release
					var releaseValue = (TxcBool)item.Release.Value;
					project.SetBuildPropertyForConfig(releaseGuid, item.key,
						TxcBool.Yes == releaseValue ? "YES" : "NO");

					Loger.BuildLog($"Add Bool(Release) -> Key:{item.key} Value:{(TxcBool.Yes == releaseValue ? "YES" : "NO")}");

					// ReleaseForProfiling
					var releaseForProfilingValue = (TxcBool)item.ReleaseForProfiling.Value;
					project.SetBuildPropertyForConfig(releaseForProfilingGuid, item.key,
						TxcBool.Yes == releaseForProfilingValue ? "YES" : "NO");

					Loger.BuildLog($"Add Bool(ReleaseForProfiling) -> Key:{item.key} Value:{(TxcBool.Yes == releaseValue ? "YES" : "NO")}");

					// ReleaseForRunning
					var releaseForRunningValue = (TxcBool)item.ReleaseForRunning.Value;
					project.SetBuildPropertyForConfig(releaseForRunningGuid, item.key,
						TxcBool.Yes == releaseForRunningValue ? "YES" : "NO");

					Loger.BuildLog($"Add Bool(ReleaseForRunning) -> Key:{item.key} Value:{(TxcBool.Yes == releaseValue ? "YES" : "NO")}");
				}
					break;
				case TxcSettingInfoType.Enum:
				{
					// Debug
					var debugValue = $"{item.Debug.Value}";
					if (false == string.IsNullOrEmpty(debugValue))
					{
						project.SetBuildPropertyForConfig(debugGuid, item.key, debugValue);

						Loger.BuildLog($"Add String(Debug) -> Key:{item.key} Value:{debugValue}");

					}
					
					// Release
					var releaseValue = $"{item.Release.Value}";
					if (false == string.IsNullOrEmpty(releaseValue))
					{
						project.SetBuildPropertyForConfig (releaseGuid, item.key, releaseValue);

						Loger.BuildLog($"Add String(Release) -> Key:{item.key} Value:{releaseValue}");
					}
					
					// ReleaseForProfiling
					var releaseForProfilingValue = $"{item.ReleaseForProfiling.Value}";
					if (false == string.IsNullOrEmpty(releaseForProfilingValue))
					{
						project.SetBuildPropertyForConfig (releaseForProfilingGuid, item.key, releaseForProfilingValue);

						Loger.BuildLog($"Add String(ReleaseForProfiling) -> Key:{item.key} Value:{releaseForProfilingValue}");
					}
					
					// ReleaseForRunning
					var releaseForRunningValue = $"{item.ReleaseForRunning.Value}";
					if (false == string.IsNullOrEmpty(releaseForRunningValue))
					{
						project.SetBuildPropertyForConfig (releaseForRunningGuid, item.key, releaseForRunningValue);

						Loger.BuildLog($"Add String(ReleaseForRunning) -> Key:{item.key} Value:{releaseForRunningValue}");
					}
				}
					break;

				case TxcSettingInfoType.String:
				{
					// Debug
					var debugValue = item.Debug.Value as string;
					if (false == string.IsNullOrEmpty(debugValue))
					{
						project.SetBuildPropertyForConfig(debugGuid, item.key, debugValue);

						Loger.BuildLog($"Add String(Debug) -> Key:{item.key} Value:{debugValue}");

					}
					
					// Release
					var releaseValue = item.Release.Value as string;
					if (false == string.IsNullOrEmpty(releaseValue))
					{
						project.SetBuildPropertyForConfig (releaseGuid, item.key, releaseValue);

						Loger.BuildLog($"Add String(Release) -> Key:{item.key} Value:{releaseValue}");
					}
					
					// ReleaseForProfiling
					var releaseForProfilingValue = item.ReleaseForProfiling.Value as string;
					if (false == string.IsNullOrEmpty(releaseForProfilingValue))
					{
						project.SetBuildPropertyForConfig (releaseForProfilingGuid, item.key, releaseForProfilingValue);

						Loger.BuildLog($"Add String(ReleaseForProfiling) -> Key:{item.key} Value:{releaseForProfilingValue}");
					}
					
					// ReleaseForRunning
					var releaseForRunningValue = item.ReleaseForRunning.Value as string;
					if (false == string.IsNullOrEmpty(releaseForRunningValue))
					{
						project.SetBuildPropertyForConfig (releaseForRunningGuid, item.key, releaseForRunningValue);

						Loger.BuildLog($"Add String(ReleaseForProfiling) -> Key:{item.key} Value:{releaseForRunningValue}");
					}
				}
					break;
				case TxcSettingInfoType.List:
					{
						// Debug
						var debugValue = item.Debug.Value as List<string>;
						if (null != debugValue)
						{
							foreach (var value in debugValue) {
								project.AddBuildPropertyForConfig (debugGuid, item.key, value);

								Loger.BuildLog($"Add List(Debug) -> Key:{item.key} Item:{value}");
							}
						}
						
						// Release
						var releaseValue = item.Release.Value as List<string>;
						if (null != releaseValue)
						{
							foreach (var value in releaseValue) {
								project.AddBuildPropertyForConfig (releaseGuid, item.key, value);

								Loger.BuildLog($"Add List(Release) -> Key:{item.key} Item:{value}");
							}
						}
						
						// ReleaseForProfiling
						var releaseForProfilingValue = item.ReleaseForProfiling.Value as List<string>;
						if (null != releaseForProfilingValue)
						{
							foreach (var value in releaseForProfilingValue) {
								project.AddBuildPropertyForConfig (releaseForProfilingGuid, item.key, value);

								Loger.BuildLog($"Add List(ReleaseForProfiling) -> Key:{item.key} Item:{value}");
							}
						}
						
						// ReleaseForRunning
						var releaseForRunningValue = item.ReleaseForRunning.Value as List<string>;
						if (null != releaseForRunningValue)
						{
							foreach (var value in releaseForRunningValue) {
								project.AddBuildPropertyForConfig (releaseForRunningGuid, item.key, value);

								Loger.BuildLog($"Add List(ReleaseForRunning) -> Key:{item.key} Item:{value}");
							}
						}
					}
					break;
				case TxcSettingInfoType.None:
					break;
				default:
					Loger.BuildEnd();
					throw new ArgumentOutOfRangeException();
				}

			}
			
			File.WriteAllText(pbxprojPath, project.WriteToString());
			Loger.BuildEnd();
		}

		private static void ReplaceSource(string iProjectDir, string iFromFilePath) {

			var fromFilePath = $"{Application.dataPath}/{iFromFilePath}";
			if (File.Exists (fromFilePath) == false) {
				return;
			}
			var lastIndex = iFromFilePath.LastIndexOf ("/", StringComparison.Ordinal);
			var fromFileName = iFromFilePath.Substring (lastIndex + 1);
			if (string.IsNullOrEmpty (fromFileName)) {
				return;
			}
			var fileList = GetAllFiles (iProjectDir, fromFileName);

			var isSucceeded = false;
			foreach (var toFilePath in fileList) {

				lastIndex = toFilePath.LastIndexOf ("/", StringComparison.Ordinal);
				var toFileName = toFilePath.Substring (lastIndex + 1);
				if (string.IsNullOrEmpty (toFileName)) {
					continue;
				}

				if (fromFileName.Equals (toFileName) == false) {
					continue;
				}

				if (File.Exists (toFilePath)) {
					File.Delete (toFilePath);
				}
				File.Copy (fromFilePath, toFilePath);

				isSucceeded = true;
				break;
			}

			if (isSucceeded == false) {
				Loger.BuildErrorLog($"[ReplaceSource Failed] File:{iFromFilePath}");
			}
		}

		private static void PreSetFileToProject(string iProjectDir, string iFile, ref string iAddFilePath) {
			var addDirReference = "Classes/Plugins";
			var copyTo = $"{iProjectDir}/{addDirReference}";
			if (Directory.Exists (copyTo) == false) {
				Directory.CreateDirectory (copyTo);
			}
			var lastIndex = iFile.LastIndexOf ("/", StringComparison.Ordinal);
			var fileName = iFile.Substring (lastIndex + 1);
			if (-1 != lastIndex) {
				var strTmp = iFile.Substring (0, lastIndex);
				var ignorePath = "Assets/Packages/";
				var index = iFile.LastIndexOf (ignorePath, StringComparison.Ordinal);
				if (-1 == index) {
					ignorePath = "Assets/";
					index = iFile.LastIndexOf (ignorePath, StringComparison.Ordinal);
				}
				index += ignorePath.Length;
				strTmp = strTmp.Substring (index);

				var dirs = strTmp.Split ('/');
				for (var _ = 0; _ < dirs.Length; ++_) {
					copyTo = $"{copyTo}/{dirs[_]}";
					addDirReference = $"{addDirReference}/{dirs[_]}";
					if (Directory.Exists (copyTo) == false) {
						Directory.CreateDirectory (copyTo);
					}
				}
			}
			copyTo = $"{copyTo}/{fileName}";
			File.Copy (iFile, copyTo);

			iAddFilePath = $"{addDirReference}/{fileName}";
		}

		private static void PreSetFolderToProject(string iProjectDir, string iFolder, ref string iCopyTo, ref string iAddDirReference) {
			iAddDirReference = "Classes/Plugins";
			iCopyTo = $"{iProjectDir}/{iAddDirReference}";
			if (Directory.Exists (iCopyTo) == false) {
				Directory.CreateDirectory (iCopyTo);
			}
			var strTmp = iFolder;
			var ignorePath = "Assets/Packages/";
			var index = iFolder.LastIndexOf (ignorePath, StringComparison.Ordinal);
			if (-1 == index) {
				ignorePath = "Assets/";
				index = iFolder.LastIndexOf (ignorePath, StringComparison.Ordinal);
			}
			index += ignorePath.Length;
			strTmp = strTmp.Substring (index);

			var dirs = strTmp.Split ('/');
			for (var _ = 0; _ < dirs.Length; ++_) {
				iCopyTo = $"{iCopyTo}/{dirs[_]}";
				iAddDirReference = $"{iAddDirReference}/{dirs[_]}";
				if (Directory.Exists (iCopyTo) == false) {
					Directory.CreateDirectory (iCopyTo);
				}
			}
			var files = GetAllFiles (iFolder);
			foreach (var loop in files) {
				if (loop.EndsWith (".meta") == true) {
					continue;
				}
				var lastIndex = loop.LastIndexOf ("/", StringComparison.Ordinal);
				var fileName = loop.Substring ( lastIndex + 1);
				fileName = $"{iCopyTo}/{fileName}";
				File.Copy (loop, fileName);
			}
		}

		/// <summary>
		/// 取得指定目录文件列表（包含子目录）.
		/// </summary>
		/// <returns>文件列表.</returns>
		/// <param name="iDirection">文件目录.</param>
		/// <param name="iFileName">文件名</param>
		private static IEnumerable<string> GetAllFiles(string iDirection, string iFileName = null)
		{   
			var filesList = new List<string>();

			try   
			{  
				var fileName = iFileName;
				if(string.IsNullOrEmpty(fileName)) {
					fileName = "*.*";
				}
				var files = Directory.GetFiles(iDirection, fileName, SearchOption.AllDirectories);

				foreach(var strVal in files)
				{
					if(string.IsNullOrEmpty(strVal)) {
						continue;
					}
					if(strVal.EndsWith(".ds_store")) {
						continue;
					}
					filesList.Add(strVal);
				}

			}  
			catch (DirectoryNotFoundException exp)   
			{  
				Loger.BuildErrorLog($"The Directory is not exist!!!(dir:{iDirection} detail:{exp.Message})");
			} 

			return filesList;
		}


		[PostProcessBuild(998)]
		public static void OnPostprocessAndroidBuild(BuildTarget iTarget, string iBuiltProjectPath)
		{

			if (iTarget != BuildTarget.Android) {
				return;
			}
			if(string.IsNullOrEmpty(iBuiltProjectPath)) {
			}
			
		}

	}
} //namespace BuildSystem
