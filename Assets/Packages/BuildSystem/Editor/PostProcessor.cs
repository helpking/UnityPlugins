using UnityEngine;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;
using Common;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.iOS;
using UnityEditor.iOS.Xcode;
using UnityEditor.Callbacks;
#endif

namespace BuildSystem
{
	public class PostProcessor
	{
		static string specialAttentionFolder = "Assets/Packages";
		static string specialFolder = "_PROJECT_ROOT_";
		static bool DebugOutput = true;
		static bool ExcessDebugOutput = false;

		static string packagesFolder = "Assets/Packages";

        [PostProcessBuildAttribute(999)]
		public static void OnPostprocessBuild(BuildTarget target, string iBuiltProjectPath)
		{
#if UNITY_EDITOR

			if (target != BuildTarget.iOS) {
				return;
			}

			const string funcBlock = "PostProcessor.OnPostprocessBuild()";
			BuildLogger.OpenBlock(funcBlock);

			const string TargetProjectName = "Unity-iPhone";
			// 取得设定情报列表
			XCSettingItem[] settings = BuildSettings.GetInstance().GetXCSettingInfo(TargetProjectName);
			if ((settings == null) || (settings.Length <= 0)) {
				BuildLogger.CloseBlock(funcBlock);
				return;
			}

			string pbxprojPath = PBXProject.GetPBXProjectPath(iBuiltProjectPath);
			PBXProject project = new PBXProject();
			project.ReadFromString(File.ReadAllText(pbxprojPath));
			string targetGUID = project.TargetGuidByName(TargetProjectName);

			// BuildMode(debug/release/store)
			string debugConfigGUID = project.BuildConfigByName (targetGUID, "Debug");
			string releaseConfigGUID = project.BuildConfigByName (targetGUID, "Release");

			foreach(XCSettingItem item in settings) {

				switch(item.Type) {
				case TXCSettingInfoType.ReplaceSource:
					{
						foreach (string value in item.Value.LValue) {
							ReplaceSource(iBuiltProjectPath, value);
							BuildLogger.LogMessage("[Replace Source:] -> {0}", iBuiltProjectPath);
						}
					}
					break;
				case TXCSettingInfoType.FrameWorks:
					{
						foreach(string frameWork in item.Value.LValue) {
							if (project.HasFramework (frameWork) == false) {
								project.AddFrameworkToProject (targetGUID, frameWork, false);
								BuildLogger.LogMessage("[Add FrameWork:] -> {0}", frameWork);
							}
						}
					}
					break;
				case TXCSettingInfoType.Libraries:
					{
						foreach(string library in item.Value.LValue) {
							string fileGuid = project.AddFile("usr/lib/" + library, "Frameworks/" + library, PBXSourceTree.Sdk);
							project.AddFileToBuild(targetGUID, fileGuid);
							BuildLogger.LogMessage("[Add Library:] -> {0}", library);
						}
					}
					break;
				case TXCSettingInfoType.IncludeFiles:
					{
						foreach(string file in item.Value.LValue) {
							string addFilePath = null;
							PreSetFileToProject (iBuiltProjectPath, file, ref addFilePath);

							if(string.IsNullOrEmpty(addFilePath) == false) {
								string fileGUID = project.AddFile (addFilePath, addFilePath, PBXSourceTree.Source);
								project.AddFileToBuild (targetGUID, fileGUID);

								BuildLogger.LogMessage("[Add File:] -> {0}", file);
							}
						}
					}
					break;
				case TXCSettingInfoType.IncludeFolders:
					{
						foreach(string folder in item.Value.LValue) {

							string copyTo = null;
							string addDirReference = null;

							PreSetFolderToProject (iBuiltProjectPath, folder, ref copyTo, ref addDirReference);
							if((string.IsNullOrEmpty(copyTo) == false) &&
								(string.IsNullOrEmpty(addDirReference) == false)) {
								project.AddFolderReference (copyTo, addDirReference, PBXSourceTree.Source);
								BuildLogger.LogMessage("[Add Folder:] -> {0}", folder);
							}
						}
					}
					break;
				case TXCSettingInfoType.Bool:
					{
						// Debug
						if(TXCBool.Yes == item.Debug.BValue) {
							project.SetBuildPropertyForConfig (debugConfigGUID, item.Key, "YES");
						} else {
							project.SetBuildPropertyForConfig (debugConfigGUID, item.Key, "NO");
						}

						BuildLogger.LogMessage("[Add Bool(Debug):] -> Key:{0} Value:{1}", item.Key, item.Debug.BValue.ToString());

						// Release
						if(TXCBool.Yes == item.Release.BValue) {
							project.SetBuildPropertyForConfig (releaseConfigGUID, item.Key, "YES");
						} else {
							project.SetBuildPropertyForConfig (releaseConfigGUID, item.Key, "NO");
						}

						BuildLogger.LogMessage("[Add Bool(Release):] -> Key:{0} Value:{1}", item.Key, item.Release.BValue.ToString());
					}
					break;

				case TXCSettingInfoType.String:
					{
						// Debug
						project.SetBuildPropertyForConfig (debugConfigGUID, item.Key, item.Debug.SValue);

						BuildLogger.LogMessage("[Add String(Debug):] -> Key:{0} Value:{1}", item.Key, item.Debug.SValue);

						// Release
						project.SetBuildPropertyForConfig (releaseConfigGUID, item.Key, item.Release.SValue);

						BuildLogger.LogMessage("[Add String(Release):] -> Key:{0} Value:{1}", item.Key, item.Release.SValue);
					}
					break;
				case TXCSettingInfoType.List:
					{
						// Debug
						foreach (string value in item.Debug.LValue) {
							project.AddBuildPropertyForConfig (debugConfigGUID, item.Key, value);

							BuildLogger.LogMessage("[Add List(Debug):] -> Key:{0} Item:{1}", item.Key, value);
						}
						// Release
						foreach (string value in item.Release.LValue) {
							project.AddBuildPropertyForConfig (releaseConfigGUID, item.Key, value);

							BuildLogger.LogMessage("[Add List(Release):] -> Key:{0} Item:{1}", item.Key, value);
						}
					}
					break;
				default:
					break;
				}

			}
			
			File.WriteAllText(pbxprojPath, project.WriteToString());
			BuildLogger.CloseBlock(funcBlock);
#endif
		}

		static void ReplaceSource(string iProjectDir, string iFromFilePath) {

			string fromFilePath = string.Format ("{0}/{1}", Application.dataPath, iFromFilePath);
			if (File.Exists (fromFilePath) == false) {
				return;
			}
			int lastIndex = iFromFilePath.LastIndexOf ("/");
			string fromFileName = iFromFilePath.Substring (lastIndex + 1);
			if (string.IsNullOrEmpty (fromFileName) == true) {
				return;
			}
			List<string> fileList = GetAllFiles (iProjectDir, fromFileName);

			bool isSuccessed = false;
			foreach (string toFilePath in fileList) {

				lastIndex = toFilePath.LastIndexOf ("/");
				string toFileName = toFilePath.Substring (lastIndex + 1);
				if (string.IsNullOrEmpty (toFileName) == true) {
					continue;
				}

				if (fromFileName.Equals (toFileName) == false) {
					continue;
				}

				if (File.Exists (toFilePath) == true) {
					File.Delete (toFilePath);
				}
				File.Copy (fromFilePath, toFilePath);

				isSuccessed = true;
				break;
			}

			if (isSuccessed == false) {
				BuildLogger.LogError ("[ReplaceSource Failed] File:{0}", iFromFilePath);
			}
		}
			
		static void PreSetFileToProject(string iProjectDir, string iFile, ref string iAddFilePath) {
			string addDirReference = "Classes/Plugins";
			string CopyTo = string.Format ("{0}/{1}", iProjectDir, addDirReference);
			if (Directory.Exists (CopyTo) == false) {
				Directory.CreateDirectory (CopyTo);
			}
			int lastIndex = iFile.LastIndexOf ("/");
			string strTmp = null;
			string fileName = iFile.Substring (lastIndex + 1);
			if (-1 != lastIndex) {
				strTmp = iFile.Substring (0, lastIndex);
				string ignorePath = "Assets/Packages/";
				int Index = iFile.LastIndexOf (ignorePath);
				if (-1 == Index) {
					ignorePath = "Assets/";
					Index = iFile.LastIndexOf (ignorePath);
				}
				Index += ignorePath.Length;
				strTmp = strTmp.Substring (Index);

				string[] dirs = strTmp.Split ('/');
				for (int i = 0; i < dirs.Length; ++i) {
					CopyTo = string.Format ("{0}/{1}", CopyTo, dirs[i]);
					addDirReference = string.Format ("{0}/{1}", addDirReference, dirs[i]);
					if (Directory.Exists (CopyTo) == false) {
						Directory.CreateDirectory (CopyTo);
					}
				}
			}
			CopyTo = string.Format ("{0}/{1}", CopyTo, fileName);
			File.Copy (iFile, CopyTo);

			iAddFilePath = string.Format ("{0}/{1}", addDirReference, fileName);
		}

		static void PreSetFolderToProject(string iProjectDir, string iFolder, ref string iCopyTo, ref string iAddDirReference) {
			iAddDirReference = "Classes/Plugins";
			iCopyTo = string.Format ("{0}/{1}", iProjectDir, iAddDirReference);
			if (Directory.Exists (iCopyTo) == false) {
				Directory.CreateDirectory (iCopyTo);
			}
			string strTmp = iFolder;
			string ignorePath = "Assets/Packages/";
			int Index = iFolder.LastIndexOf (ignorePath);
			if (-1 == Index) {
				ignorePath = "Assets/";
				Index = iFolder.LastIndexOf (ignorePath);
			}
			Index += ignorePath.Length;
			strTmp = strTmp.Substring (Index);

			string[] dirs = strTmp.Split ('/');
			for (int i = 0; i < dirs.Length; ++i) {
				iCopyTo = string.Format ("{0}/{1}", iCopyTo, dirs[i]);
				iAddDirReference = string.Format ("{0}/{1}", iAddDirReference, dirs[i]);
				if (Directory.Exists (iCopyTo) == false) {
					Directory.CreateDirectory (iCopyTo);
				}
			}
			List<string> files = GetAllFiles (iFolder);
			foreach (string loop in files) {
				if (loop.EndsWith (".meta") == true) {
					continue;
				}
				int lastIndex = loop.LastIndexOf ("/");
				string fileName = loop.Substring ( lastIndex + 1);
				fileName = string.Format ("{0}/{1}", iCopyTo, fileName);
				File.Copy (loop, fileName);
			}
		}

		/// <summary>
		/// 取得指定目录文件列表（包含子目录）.
		/// </summary>
		/// <returns>文件列表.</returns>
		/// <param name="iDirection">文件目录.</param>
		static List<string> GetAllFiles(string iDirection, string iFileName = null)
		{   
			List<string> filesList = new List<string>();

			try   
			{  
				string fileName = iFileName;
				if(string.IsNullOrEmpty(fileName) == true) {
					fileName = "*.*";
				}
				string[] files = Directory.GetFiles(iDirection, fileName, SearchOption.AllDirectories);

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
	}
} //namespace BuildSystem
