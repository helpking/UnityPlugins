using Packages.BuildSystem.AndroidSDK.Manifest;
using Packages.Common;
using Packages.Common.Base;
using UnityEditor;

namespace Packages.BuildSystem.AndroidSDK {

	/// <summary>
	/// 安卓SDK设定信息数据.
	/// </summary>
	public class AndroidSdkSettingsData : JsonDataBase<AndroidSdkSettingsData> {

		/// <summary>
		/// 平台类型.
		/// </summary>
		public PlatformType PlatformType;

		/// <summary>
		/// 安卓SDK最小版本.
		/// </summary>
		public int MinSdkVersion;

		/// <summary>
		/// 安卓SDK最大版本.
		/// </summary>
		public int MaxSdkVersion;

		/// <summary>
		/// 安卓SDK目标版本.
		/// </summary>
		public int TargetSdkVersion;

		/// <summary>
		/// 屏幕方向.
		/// </summary>
		public UIOrientation Orientation;

		/// <summary>
		/// 本地存储标志位（false:向服务器拉取相关信息）.
		/// </summary>
		public bool Local;

		/// <summary>
		/// SDK自动初始化.
		/// </summary>
		public bool AutoSdkInit;

		/// <summary>
		/// 自动登录标志位.
		/// </summary>
		public bool AutoLogin;

		/// <summary>
		/// 初始化.
		/// </summary>
		public override void Clear() {
			PlatformType = PlatformType.None;
			MinSdkVersion = -1;
			MaxSdkVersion = -1;
			TargetSdkVersion = -1;
			Orientation = PlayerSettings.defaultInterfaceOrientation;
			Local = false;
			AutoSdkInit = true;
			AutoLogin = true;
		}
	}

	/// <summary>
	/// 安卓设定接口.
	/// </summary>
	public interface IAndroidSdkSettings {
		
		/// <summary>
		/// 初始化设定信息.
		/// </summary>
		bool InitSettings();

		/// <summary>
		/// 取得导出华为用的AndroidManifest.xml文件路径.
		/// </summary>
		/// <returns>导出华为用的AndroidManifest.xml文件路径.</returns>
		string GetAndroidManifestXmlPath();

		/// <summary>
		/// 取得拷贝源文件目录.
		/// </summary>
		/// <returns>取得拷贝源文件目录.</returns>
		string GetAndroidCopyFromDir();

		/// <summary>
		/// 取得拷贝目的文件目录.
		/// </summary>
		/// <returns>取得拷贝目的文件目录.</returns>
		string GetAndroidCopyToDir();

		/// <summary>
		/// 取得AndroidManifest对象.
		/// </summary>
		/// <returns>AndroidManifest对象.</returns>
		/// <param name="iGameName">游戏名.</param>
		ManifestBase GetAndroidManifest (string iGameName);

		/// <summary>
		/// 应用设定信息到AndroidManifest.xml.
		/// </summary>
		/// <param name="iManifest">AndroidManifest对象.</param>
		/// <param name="iPackageName">游戏包名.</param>
		bool AppSettingsToAndroidManifestFile (ManifestBase iManifest, string iPackageName);

		/// <summary>
		/// 打包Android（apk文件）之前，提前应用设定.
		/// </summary>
		/// <param name="iGameName">游戏名.</param>
		/// <param name="iPackageName">游戏包名.</param>
		void PreApplyAndroidBuild (string iGameName, string iPackageName);

		/// <summary>
		/// 合并AndroidManifest.xml文件.
		/// </summary>
		/// <returns><c>true</c>, OK, <c>false</c> NG.</returns>
		/// <param name="iGameName">游戏名.</param>
		/// <param name="iPackageName">游戏包名.</param>
		bool MergeManifestFile(string iGameName, string iPackageName);

		/// <summary>
		/// 保存AndroidManifest.xml文件.
		/// </summary>
		/// <param name="iManifest">AndroidManifest对象.</param>
		/// <param name="iSavePath">保存路径.</param>
		/// <param name="iPackageName">游戏包名.</param>
		bool SaveAndroidManifestFile (ManifestBase iManifest, string iSavePath, string iPackageName);

		/// <summary>
		/// 拷贝库资源文件.
		/// </summary>
		void CopyResources ();
	}
}
