using System;
using System.IO;
using System.Xml;
using Packages.BuildSystem.AndroidSDK.Manifest;
using Packages.Common.Base;
using Packages.Logs;

#if UNITY_ANDROID
using UnityEditor;

namespace Packages.BuildSystem.AndroidSDK.Platforms.Tiange.Manifest {

	/// <summary>
	/// 天鸽Manifest文件处理类.
	/// </summary>
	public class TiangeManifest : ManifestBase {

		/// <summary>
		/// 实例.
		/// </summary>
		private static TiangeManifest _instance;

		/// <summary>
		/// 取得实例.
		/// </summary>
		/// <returns>实例.</returns>
		/// <param name="iDir">路径.</param>
		/// <param name="iGameName">游戏名.</param>
		public static TiangeManifest GetInstance(string iDir, string iGameName = null) {
			try {
				if(_instance == null) {
					_instance = new TiangeManifest ();
				}
				if(false == _instance.InitByTiangeDir(iDir, iGameName)) {
					_instance = null;
				}
			} catch (Exception e) {
				Loger.Fatal($"[TiangeManifest Create Failed] Exeption : {e.Message}");
				_instance = null;
			}
			return _instance;
		}

		/// <summary>
		/// 初始化.
		/// </summary>
		/// <returns><c>true</c>, OK, <c>false</c> NG.</returns>
		/// <param name="iDir">路径.</param>
		/// <param name="iGameName">游戏名.</param>
		public bool InitByTiangeDir(string iDir, string iGameName) {
			Dir = iDir;

			var manifestPath = $"{iDir}/AndroidManifest.xml";
			return File.Exists (manifestPath) && Init (manifestPath, iGameName);
		}

		/// <summary>
		/// 初始化SDK版本信息.
		/// </summary>
		/// <returns>SDK版本信息节点.</returns>
		protected override XmlElement InitSdkVersions() {
			var useSdkNode = base.InitSdkVersions ();
			if (null == useSdkNode) {
				return null;
			}
			if (-1 >= TiangeSdkSettings.GetInstance ().MinSdkVersion) {
				MinSdkVersion = GetNodeAttribute_i (useSdkNode, "minSdkVersion");
			} else {
				MinSdkVersion = TiangeSdkSettings.GetInstance ().MinSdkVersion;
				SetNodeAttribute (useSdkNode, "minSdkVersion", MinSdkVersion.ToString());
			}
			if (-1 >= TiangeSdkSettings.GetInstance ().MaxSdkVersion) {
				MaxSdkVersion = GetNodeAttribute_i (useSdkNode, "android:maxSdkVersion");
			} else {
				MaxSdkVersion = TiangeSdkSettings.GetInstance ().MaxSdkVersion;
				SetNodeAttribute (useSdkNode, "maxSdkVersion", MaxSdkVersion.ToString());
			}
			if (-1 >= TiangeSdkSettings.GetInstance ().TargetSdkVersion) {
				TargetSdkVersion = GetNodeAttribute_i (useSdkNode, "android:targetSdkVersion");
			} else {
				TargetSdkVersion = TiangeSdkSettings.GetInstance ().TargetSdkVersion;
				SetNodeAttribute (useSdkNode, "targetSdkVersion", TargetSdkVersion.ToString());
			}
			return useSdkNode;
		}

		/// <summary>
		/// 取得res/values目录下的strings.xml的文件路径.
		/// </summary>
		/// <returns>strings.xml文件路径.</returns>
		protected override string GetStringsXmlPath() {
			return $"{Dir}/res/values/tiange_strings.xml";
		}
			
		/// <summary>
		/// 应用用户自定义数据.
		/// </summary>
		/// <param name="iGameName">游戏名.</param>
		protected override void ApplyUserData(string iGameName) {

			// 本地设定
			{
				const string name = "Local";
				var value = TiangeSdkSettings.GetInstance ().Local.ToString();
				if (false == string.IsNullOrEmpty (value)) {
					AddUserDefineNode (name, value, false);
				}
				if (TiangeSdkSettings.GetInstance ().Local == false) {
					return;
				}
			}

			// 游戏名
			{
				const string name = "GameName";
				var value = iGameName;
				if (false == string.IsNullOrEmpty (value)) {
					AddUserDefineNode (name, value);
				}
			}

			// SDK自动初始化
			{
				const string name = "AutoSdkInit";
				var value = TiangeSdkSettings.GetInstance ().AutoSdkInit.ToString();
				if (false == string.IsNullOrEmpty (value)) {
					AddUserDefineNode (name, value, false);
				}
				if (TiangeSdkSettings.GetInstance ().Local == false) {
					return;
				}
			}

			// 自动登录
			{
				const string name = "AutoLogin";
				var value = TiangeSdkSettings.GetInstance ().AutoLogin.ToString();
				if (false == string.IsNullOrEmpty (value)) {
					AddUserDefineNode (name, value, false);
				}
				if (TiangeSdkSettings.GetInstance ().Local == false) {
					return;
				}
			}
			// 屏幕方向
			{
				const string name = "Orientation";
				var value = "1";
				var orientation = TiangeSdkSettings.GetInstance ().Orientation;
				if (UIOrientation.LandscapeLeft == orientation ||
					UIOrientation.LandscapeRight == orientation) {
					value = "2";
				}
				if (false == string.IsNullOrEmpty (value)) {
					AddUserDefineNode (name, value, false);
				}
			}

			// 易接SDK 设定
			if (TiangeSdkSettings.GetInstance ().data.Options.IsOptionValid (SDKOptions.OneSDK)) {

				// 易接SDK Key
				{
					const string name = "zy_class_name";
					var value = TiangeSdkSettings.GetInstance ().data.Options.OneSDK.zyClassName;
					if (false == string.IsNullOrEmpty (value)) {
						AddUserDefineNode (name, value);
					}
				}

				// 易接SDK MetaDatas
				{

					var metaDatas = TiangeSdkSettings.GetInstance ().data.Options.OneSDK.metaDatas;
					foreach (var metaData in metaDatas) {
						if (null != metaData && 
							false == string.IsNullOrEmpty (metaData.name) && 
							false == string.IsNullOrEmpty (metaData.value)) {
							AddUserDefineNode (metaData.name, metaData.value, false);
						}
					}
				}
			}

			// 保存strings.xml
			StringsXml?.Save ();
		}
	}
}
#endif
