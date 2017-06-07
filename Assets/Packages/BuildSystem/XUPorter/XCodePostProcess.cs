using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.XCodeEditor;
#endif
using System.IO;
using BuildSystem;

public static class XCodePostProcess
{

#if UNITY_EDITOR
	[PostProcessBuild(999)]
	public static void OnPostProcessBuild( BuildTarget target, string pathToBuiltProject )
	{
//		const string funcBlock = "XCodePostProcess.OnPostProcessBuild()";
//		BuildLogger.OpenBlock(funcBlock);
//		BuildLogger.LogMessage (string.Format("BuildTarget : {0} ", target));
//		BuildLogger.LogMessage (string.Format("BuildPath : {0} ", pathToBuiltProject));
//
//		if (target != BuildTarget.iOS) {
//			BuildLogger.CloseBlock(funcBlock);
//			return;
//		}
//
//		string xcodePath = Path.GetFullPath (pathToBuiltProject);
//		BuildLogger.LogMessage (string.Format("XCodePath : {0} ", xcodePath));
//
//		// Create a new project object from build target
//		XCProject project = new XCProject( pathToBuiltProject );
//
//		// Find and run through all projmods files to patch the project.
//		// Please pay attention that ALL projmods files in your project folder will be excuted!
//		string[] files = Directory.GetFiles( Application.dataPath, "*.projmods", SearchOption.AllDirectories );
//		foreach( string file in files ) {
//			BuildLogger.LogMessage (string.Format("ProjMod File : {0} ", file));
//			project.ApplyMod( file );
//		}
//
//		// Unity5后 老板的XUPorter代码在projmods文件的 "libs"中添加“libz.tbd”不起效果，需要
//		// 需要添加在"linker_flags"中添加"-lz"才行;不过新的XUPorter代码已经添加了对tbd文件的支持
//		// 以防万一，全部添加
//		BuildLogger.LogMessage ("AddOtherLinkerFlags : -lz ");
//		project.AddOtherLinkerFlags("-lz");
//
//		// 指定签名（Debug）
////		project.overwriteBuildSetting("CODE_SIGN_IDENTITY", "iPhone Developer: Niko Hurst (E9RKWTEZFL)", "Debug");
//		// 指定签名（Release）
////		project.overwriteBuildSetting("CODE_SIGN_IDENTITY", "iPhone Developer: Niko Hurst (E9RKWTEZFL)", "Release");
//
//		// 设置Development Team
//		string developmentTeam = BuildParameters.DevelopmentTeam;
//		if (developmentTeam != null) {
//			BuildLogger.LogMessage (string.Format("Setting develop team : Debug : {0} ", developmentTeam));
//			project.overwriteBuildSetting ("DEVELOPMENT_TEAM", developmentTeam, "Debug");
//			BuildLogger.LogMessage (string.Format("Setting develop team : Release : {0} ", developmentTeam));
//			project.overwriteBuildSetting ("DEVELOPMENT_TEAM", developmentTeam, "Release");
//		}
//		
//		// Finally save the xcode project
//		project.Save();
//
//		BuildLogger.CloseBlock(funcBlock);

	}
#endif

}
