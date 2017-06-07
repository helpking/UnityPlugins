using UnityEditor;
using UnityEditor.Callbacks;
using System;
using System.Text;
using System.Diagnostics;
using System.IO;

namespace BuildSystem
{
    // Sets app bundle internal version (build) to 'shortversion.date.time'
	public static class BundleVersionScript
	{
        [PostProcessBuildAttribute(10)]
		public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
		{
            if (target == BuildTarget.iOS)
            {
                StringBuilder build = new StringBuilder(PlayerSettings.bundleVersion.Replace(".", String.Empty));
				string [] buildTagComponents = AutoBuild.buildTag.Split('_', '-');
				if (buildTagComponents.Length == 3) {
					build.Append('.').Append(buildTagComponents[1]).Append('.').Append(buildTagComponents[2]);
					BuildLogger.LogMessage("Formed bundle version from build tag: " + build.ToString());
				} else {
					DateTime now = DateTime.Now;
					build.Append('.').Append(now.ToString("yyyyMMdd")).Append('.').Append(now.ToString("HHmmss"));
					BuildLogger.LogWarning("Failed to get bundle version from build tag, using current timestamp instead");
				}
                var process = new Process();
                process.StartInfo.FileName = "/usr/libexec/PlistBuddy";
                process.StartInfo.Arguments =
                    "-c \"Set :CFBundleVersion '" + build.ToString() +
                    "'\" \"" + Path.Combine(pathToBuiltProject, "info.plist") + "\"";
                process.Start();
                process.WaitForExit();
            }
        }
    }
}