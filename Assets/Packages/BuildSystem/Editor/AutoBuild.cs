using UnityEditor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace BuildSystem
{
	public interface IBuildConfig
	{
		void Build(List<string> scenesToBuild, BuildTarget buildTarget, BuildOptions buildOptions);
	}

	public class DefaultIosBuildConfig : IBuildConfig
	{
		public virtual void Build(System.Collections.Generic.List<string> scenesToBuild,
			UnityEditor.BuildTarget buildTarget, UnityEditor.BuildOptions buildOptions)
		{
			AutoBuild.UnityBuildPlayer(scenesToBuild, buildTarget, buildOptions);
		}
	}

	public class AutoBuild
	{
		private static List<string> GetScenesToBuild()
		{
			List<string> scenePaths = new List<string>();
			foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
			{
				if (scene.enabled && File.Exists(scene.path))
					scenePaths.Add(scene.path);
			}

			return scenePaths;
		}

		public static string currentOutputDirectory { get { return "dist/" + currentConfigTypeName; } }
		public static string buildTag { get { return _buildTag; } }

		static List<string> configTypeNames = new List<string>();
		static string currentConfigTypeName = "";
		static string _buildTag = "LOCALCONF_" + DateTime.Now.ToString("yyyyMdd-HHmmss");

#if CSHARPCOMPILEROPTIONS
		public static uint SuspendPackages(string[] packages)
		{
			uint maskedFolders = 0;
			foreach (string package in packages)
			{
				string pathname = "Assets/Packages/" + package;
				if (Directory.Exists(pathname) && File.Exists(pathname + ".meta")) {
					Directory.Move(pathname, pathname + "~");
					File.Move(pathname + ".meta", pathname + "~.meta");
					++maskedFolders;
				}
			}
			if (maskedFolders > 0) {
				CSharpCompilerOptions.ReimportScripts();
				CSharpCompilerOptions.WriteDefines();
				AssetDatabase.Refresh();
			}
			return maskedFolders;
		}

		public static void RestorePackages(string[] packages)
		{
			bool restoredAtLeastOne = false;
			foreach (string package in packages)
			{
				string pathname = "Assets/Packages/" + package;
				if (Directory.Exists(pathname + "~")  && File.Exists(pathname + "~.meta")) {
					Directory.Move(pathname + "~", pathname);
					File.Move(pathname + "~.meta", pathname + ".meta");
					restoredAtLeastOne = true;
				}
			}
			if (restoredAtLeastOne) {
				CSharpCompilerOptions.ReimportScripts();
				CSharpCompilerOptions.WriteDefines();
				AssetDatabase.Refresh();
			}
		}
#endif

		public static void Build()
		{
			const string autoBuildBlock = "AutoBuild.Build()";
			BuildLogger.OpenBlock(autoBuildBlock);

			BuildLogger.LogMessage("Parsing additional params");
			var args = Environment.GetCommandLineArgs();

			for(int i = 0; i < args.Length; i++)
			{
				if(args[i] == "+configTypeName" && args.Length > i + 1)
				{
					do {
						string config = args[i + 1];
						configTypeNames.Add(config);
						BuildLogger.LogMessage("configTypeName: " + config);
						if(!config.Equals(config.Trim())) // to detect 'CR' at the end
						{
							BuildLogger.LogWarning("Build configurator type contains whitespaces (+configTypeName length = " + config.Length + ")!");
						}
						i++;
					} while( i + 1 < args.Length && !args[i + 1].StartsWith("-") && !args[i + 1].StartsWith("+") );

				}
				else if(args[i] == "+buildTag" && args.Length > i + 1)
				{
					_buildTag = args[i + 1];
					BuildLogger.LogMessage("buildTag: " + buildTag);
					i++;
				}
			}

			// produce Assets/Resources/build_tag.txt
			if(buildTag != null)
			{
				if (!Directory.Exists("Assets/Resources"))
				{
					BuildLogger.LogMessage("Creating Assets/Resources");
					Directory.CreateDirectory("Assets/Resources");
				}

				BuildLogger.LogMessage("Writing Assets/Resources/build_tag.txt: " + buildTag);
				File.WriteAllText("Assets/Resources/build_tag.txt", buildTag);

				AssetDatabase.Refresh();
			}

			// prepare default build params
			BuildLogger.LogMessage("Preparing default build params");
			List<string> scenesToBuild = GetScenesToBuild();
			BuildTarget buildTarget = BuildTarget.iOS;
			BuildOptions buildOptions = BuildOptions.None;
			BuildLogger.LogMessage("Default scenes to build:");
			foreach (string sceneName in scenesToBuild)
				BuildLogger.LogMessage('\t' + sceneName);
			BuildLogger.LogMessage("Default buildTarget=" + buildTarget.ToString());
			BuildLogger.LogMessage("Default buildOptions=" + buildOptions.ToString());

			// run custom builder (or fall back to default)
			int index = 0;
			foreach (string configTypeName in configTypeNames)
			{
				Type configType = Type.GetType(configTypeName);
				currentConfigTypeName = configTypeName;
				index++;

				IBuildConfig buildConfig = null;
				if(configType != null)
				{
					if(!configType.IsInterface && typeof(IBuildConfig).IsAssignableFrom(configType))
					{
						buildConfig = Activator.CreateInstance(configType) as IBuildConfig;
						if(buildConfig != null)
						{
							BuildLogger.LogMessage("Using build configurator \"" + buildConfig.ToString() + "\"");
						}
						else
						{
							BuildLogger.LogWarning("Failed to construct an instance of build configurator type (+configTypeName \"" + configTypeName + "\")");
						}
					}
					else
					{
						BuildLogger.LogWarning("Build configurator type (+configTypeName \"" + configTypeName + "\") does NOT implement IBuildConfig");
					}
				}
				else
				{
					BuildLogger.LogWarning("Build configurator type NOT found (+configTypeName \"" + configTypeName + "\")");
				}
				if(buildConfig != null)
				{
					string block = string.Format("{0}.Build()", buildConfig.GetType().Name);
					BuildLogger.OpenBlock(block);
					if (Directory.Exists(currentOutputDirectory))
						Directory.Delete(currentOutputDirectory, true);
					Directory.CreateDirectory(currentOutputDirectory);
					buildConfig.Build(scenesToBuild, buildTarget, buildOptions);
					BuildLogger.CloseBlock(block);
				}
				else
				{
					BuildLogger.LogError("Unable to configure build for " + configTypeName);
					throw new ApplicationException();
				}
			}

			BuildLogger.CloseBlock(autoBuildBlock);
		}

		public static void UnityBuildPlayer(List<string> scenesToBuild, BuildTarget buildTarget, BuildOptions buildOptions, string configuration = "Release")
		{
			const string funcBlock = "AutoBuild.UnityBuildPlayer()";
			BuildLogger.OpenBlock(funcBlock);
			BuildLogger.LogMessage ("Try update bundle version");
			string bundleVersion = Environment.GetEnvironmentVariable ("MarketingVersion");
			if (!string.IsNullOrEmpty (bundleVersion)) {
				PlayerSettings.bundleVersion = bundleVersion;
			}

			BuildLogger.LogMessage("Scenes to build:");
			foreach (string sceneName in scenesToBuild)
				BuildLogger.LogMessage('\t' + sceneName);
			BuildLogger.LogMessage("BuildTarget=" + buildTarget.ToString());
			BuildLogger.LogMessage("BuildOptions=" + buildOptions.ToString());

			string buildPlayerBlock = string.Format("BuildPipeline.BuildPlayer (\"{0}\")", Path.GetFileName(currentOutputDirectory));
			BuildLogger.OpenBlock(buildPlayerBlock);

			string outputPath = buildTarget == BuildTarget.Android ?
				currentOutputDirectory + "/" + PlayerSettings.bundleIdentifier + ".apk" :
				"GeneratedProject_" + currentConfigTypeName;
			string errorMessage = BuildPipeline.BuildPlayer(scenesToBuild.ToArray(),
				outputPath,
				buildTarget,
				buildOptions);

			BuildLogger.CloseBlock(buildPlayerBlock);

			if(errorMessage != null && !errorMessage.Equals("") && !(errorMessage.Length == 0))
			{
				BuildLogger.LogError(errorMessage);
				throw new ApplicationException();
			}

			BuildLogger.CloseBlock(funcBlock);
			buildTargetUsed = buildTarget;

			if (buildTargetUsed == BuildTarget.iOS) {
				string fullPathToOutputDirectory = Directory.GetCurrentDirectory() + "/dist/" + currentConfigTypeName + "/";
				BuildLogger.OpenBlock("BuildXcode");
				if (Directory.Exists("custom_provisions")) {
					var temporaryCredentials = AddCustomSigningCredentials("custom_provisions", buildTag);
					try {
						BuildXcode(outputPath, currentConfigTypeName, fullPathToOutputDirectory, configuration);
					} finally {
						RemoveCustomSigningCredentials(temporaryCredentials, buildTag);
					}
				} else {
					BuildXcode(outputPath, currentConfigTypeName, fullPathToOutputDirectory, configuration);
				}
				BuildLogger.CloseBlock("BuildXcode");
			}
		}

		static void BuildXcode(string projectPath, string buildConfigName, string outputPath, string buildConfiguration)
		{
			BuildLogger.LogMessage("Now building with Xcode");
			var provisionProfileUUIDs = new List<string>();
			var provisionProfileNames = new List<string>();
			var signingIdentityNames = new List<string>();
			var archiveNames = new List<string>();
			var provisionProfilePathNames = new List<string>();

			bool archiveReady = false;

			//string appName = "";
			Regex commonNameMask = new Regex("/CN=(.*?)/");
			string homeFolder = Environment.GetEnvironmentVariable("HOME");
			string tag = Environment.GetEnvironmentVariable("ARG_TAG");
			//var app = Directory.GetDirectories("build.xcarchive/Products/Applications", "*.app");
			// if (app.Length > 0)
			// 	appName = Path.GetFileNameWithoutExtension(app[0]);

			string pathToMobileprovisions = Path.Combine(homeFolder, "Library/MobileDevice/Provisioning Profiles");
			var provisionProfiles = Directory.GetFiles(pathToMobileprovisions, "*?.mobileprovision");

			// collect mobileprovision data
			foreach (string file in provisionProfiles)
			{
				// Read plist from mobileprovision
				var cmd = new ProcessStartInfo() {
					FileName = "security",
					Arguments = "cms -D -i \"" + file + "\"",
					UseShellExecute = false,
					RedirectStandardOutput = true
				};
				var proc = Process.Start(cmd);
				string output = proc.StandardOutput.ReadToEnd();
				proc.WaitForExit();

				// Filter out expired profiles
				var plist = new PListDictionary(output);
				string expirationDate = plist["ExpirationDate"] as string;
				var expiration = DateTime.ParseExact(expirationDate, "yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture);
				if (DateTime.Now >= expiration) {
					BuildLogger.LogWarning("Mobile provision " + Path.GetFileName(file) + " has expired");
					continue;
				}

				// Filter out profiles for other apps
				var entitlements = plist["Entitlements"] as PListDictionary;
				string bundleIdentifierMask = entitlements["application-identifier"] as string;
				string appIdentifier = bundleIdentifierMask.Remove(0, bundleIdentifierMask.IndexOf('.') + 1).Replace("*", "");
				if (!PlayerSettings.bundleIdentifier.Contains(appIdentifier)) {
					BuildLogger.LogWarning("Mobile provision " + Path.GetFileName(file) + " is for the bundles " + appIdentifier +
						" which do not match this app bundle id " + PlayerSettings.bundleIdentifier);
					continue;
				}

				// Find matching certificate and decode it
				var PEM = plist["DeveloperCertificates"] as List<object>;
				var firstCertificate = PEM[0] as string;
				if (!firstCertificate.Contains("\n")) {
					int length = firstCertificate.Length;
					var buffer = new StringBuilder(length + length / 61);
					for (int pos = 0; pos < length; pos += 61) {
						buffer.Append(firstCertificate.Substring(pos, Math.Min(61, length - pos))).Append('\n');
					}
					firstCertificate = buffer.ToString();
				}
				firstCertificate = "\n-----BEGIN CERTIFICATE-----\n" + firstCertificate + "-----END CERTIFICATE-----\n";

				var openssl = new ProcessStartInfo() {
					FileName = "openssl",
					Arguments = "x509 -inform pem -subject -noout",
					UseShellExecute = false,
					RedirectStandardInput = true,
					RedirectStandardOutput = true
				};
				proc = Process.Start(openssl);
				proc.StandardInput.WriteLine(firstCertificate);
				proc.StandardInput.Close();
				string result = proc.StandardOutput.ReadToEnd();
				proc.WaitForExit();

				string commonName = commonNameMask.Match(result).Groups[1].ToString();
				string provisionProfileName = plist["Name"] as string;
				string ipaName = "__" + buildConfigName + "__" + provisionProfileName.Replace("_", "").Replace(" ", "") + ".ipa";

				// Keep all data gathered
				provisionProfilePathNames.Add(Path.Combine(pathToMobileprovisions, file));
				provisionProfileUUIDs.Add(plist["UUID"] as string);
				signingIdentityNames.Add(commonName);
				archiveNames.Add(ipaName);
				provisionProfileNames.Add(provisionProfileName);
			}

			Directory.CreateDirectory(outputPath);
			if (Directory.Exists("build")) {
				Directory.Delete("build", true);
			}
			Directory.CreateDirectory("build");
			string buildFolder = Path.Combine( Directory.GetCurrentDirectory(), "build" );

			string appBundle = null;
			for (int index = 0; index < provisionProfileUUIDs.Count; index++)
			{
				string action = archiveReady ? "\" -scheme Unity-iPhone" : "\" clean build -scheme Unity-iPhone";
				string buildArguments = "-project \"" + projectPath + "/Unity-iPhone.xcodeproj" +
					"\" -configuration \"" + buildConfiguration + action +
					" SYMROOT=\"" + buildFolder +
					"\" PROVISIONING_PROFILE=\"" + provisionProfileUUIDs[index] +
					"\" CODE_SIGN_IDENTITY=\"" + signingIdentityNames[index] + "\"";
				BuildLogger.LogMessage("Running xcodebuild with parameters:\n" + buildArguments);
				var build = Process.Start("xcodebuild", buildArguments);
				build.WaitForExit();

				if (build.ExitCode == 0) {
					if (!archiveReady) {
						var bundles = Directory.GetDirectories("build", "*?.app", SearchOption.AllDirectories);
						if (bundles == null || bundles.Length == 0) {
							BuildLogger.LogError("Couldn't find created app bundle.");
							throw new ApplicationException();
						}
						appBundle = bundles[0];
						if (String.IsNullOrEmpty(tag))
							tag = Path.GetFileNameWithoutExtension(appBundle);
						archiveReady = true;
					}
					string exportArguments = "-sdk iphoneos PackageApplication -v \"" + appBundle +
						"\" -o \"" + outputPath + tag + archiveNames[index] + "\"";
					build = Process.Start("xcrun", exportArguments);
					build.WaitForExit();
					if (build.ExitCode != 0) {
						BuildLogger.LogError("Couldn't export IPA.");
						throw new ApplicationException();
					}
				} else {
					BuildLogger.LogError("Xcode build encountered error(s).");
					throw new ApplicationException();
				}
			}
			// Copy dSYM
			var dSYM = appBundle + ".dSYM";
			Process.Start("/usr/bin/ditto", "\"" + dSYM + "\" \"" + outputPath + Path.GetFileName(dSYM) + "\"").WaitForExit();
		}

		static List<string> AddCustomSigningCredentials(string path, string keychainName)
		{
			bool keychainCreated = false;
			keychainName += ".keychain";
			string homeFolder = Environment.GetEnvironmentVariable("HOME");
			string copyDestination = Path.Combine(homeFolder, "Library/MobileDevice/Provisioning Profiles");
			var copiedProvisions = new List<string>();
			var files = Directory.GetFiles(path);
			foreach (string file in files) {
				string extension = Path.GetExtension(file);
				if (extension == ".p12" || extension == ".cer") {
					if (!keychainCreated) {
						Process.Start("security", "-v create-keychain -p \"0000\" " + keychainName).WaitForExit();
						Process.Start("security", "-v unlock-keychain -p \"0000\" " + keychainName).WaitForExit();
						Process.Start("security", "-v set-keychain-settings -u " + keychainName).WaitForExit();
						Process.Start("security", "list-keychains -d user -s login.keychain " + keychainName)
							.WaitForExit();
						keychainCreated = true;
					}
					if (extension == ".p12") {
						string filenameWithoutExtension = Path.GetFileNameWithoutExtension(file);
						Process.Start("security", "-v import " + file + " -k " + keychainName +
						 " -P " + filenameWithoutExtension + " -A").WaitForExit();
					} else {	// ".cer"
						Process.Start("security", "-v add-certificates -k " + keychainName + " " + file)
							.WaitForExit();
					}
				} else if (extension == ".mobileprovision") {
					var cmd = new ProcessStartInfo() {
						FileName = "security",
						Arguments = "cms -D -i \"" + file + "\"",
						UseShellExecute = false,
						RedirectStandardOutput = true
					};
					var proc = Process.Start(cmd);
					StreamReader sr = proc.StandardOutput;
					string output = sr.ReadToEnd();
					proc.WaitForExit();

					var plist = new PListDictionary(output);
					string expirationDate = plist["ExpirationDate"] as string;
					var expiration = DateTime.ParseExact(expirationDate, "yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture);

					if (DateTime.Now >= expiration) {
						BuildLogger.LogWarning("Mobile provision " + Path.GetFileName(file) + " has expired");
						continue;
					}
					string provisionProfileUUID = plist["UUID"] as string;
					string copiedProfileName = Path.Combine(copyDestination, provisionProfileUUID + ".mobileprovision");
					BuildLogger.LogMessage("Copying custom " + file + " to " + copiedProfileName);
					File.Copy(file, copiedProfileName, true);
					copiedProvisions.Add(copiedProfileName);
				}
			}
			return copiedProvisions;
		}

		static void RemoveCustomSigningCredentials(List<string> provisionProfiles, string keychainName)
		{
			keychainName += ".keychain";
			Process.Start("security", "-v delete-keychain " + keychainName).WaitForExit();
			foreach (var file in provisionProfiles)
			{
				BuildLogger.LogMessage("Deleting " + file);
				File.Delete(file);
			}
		}

		static BuildTarget? buildTargetUsed = null;
	}
} //namespace UnityBuildSystem