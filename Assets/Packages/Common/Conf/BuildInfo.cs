using UnityEngine;
using UnityEditor;
using System.Collections;
using BuildSystem;
using AssetBundles;

namespace Common {

	/// <summary>
	/// BuildMode.
	/// </summary>
	public enum TBuildMode {
		/// <summary>
		/// Debug模式.
		/// </summary>
		Debug,

		/// <summary>
		/// Release模式.
		/// </summary>
		Release,

		/// <summary>
		/// Store模式.
		/// </summary>
		Store = Release,
		Max
	}

	/// <summary>
	/// 设备情报.
	/// </summary>
	[System.Serializable]
	public class DeviceInfo {
		/// <summary>
		/// 名称.
		/// </summary>
		public string Name = null;
	}

	/// <summary>
	/// 打包信息类.
	/// </summary>
	[System.Serializable]
	public class BuildInfo : AssetBase {

		/// <summary>
		/// BuildMode.
		/// </summary>
		[SerializeField]public TBuildMode BuildMode = TBuildMode.Debug;

		/// <summary>
		/// 打包名.
		/// </summary>
		[SerializeField]public string BuildName = null;

		/// <summary>
		/// ID.
		/// </summary>
		[SerializeField]public string BuildID = null;

		/// <summary>
		/// 版本号.
		/// </summary>
		[SerializeField]public string BuildVersion = null;

		/// <summary>
		/// 版本号(Short).
		/// </summary>
		[SerializeField]public string BuildShortVersion = null;

		/// <summary>
		/// 版本Code
		/// </summary>
		[SerializeField]public string BuildVersionCode  = null;

		/// <summary>
		/// 市场版本.
		/// </summary>
		[SerializeField]public string MarketVersion = null;

		/// <summary>
		/// 设备信息.
		/// </summary>
		[SerializeField]public DeviceInfo DeviceInfo = new DeviceInfo();

		/// <summary>
		/// 实例.
		/// </summary>
		private static BuildInfo _instance = null;

		/// <summary>
		/// 取得实例.
		/// </summary>
		/// <returns>实例.</returns>
		public static BuildInfo GetInstance() {
			if (_instance == null) {
				_instance = UtilityAsset.Read<BuildInfo>();
				if (_instance == null) {
					Debug.LogError ("BundlesCheck GetInstance Failed!!!");
					return null;
				} 
				_instance.Init ();
			}
			return _instance;
		}

		#region Implement

		/// <summary>
		/// 初始化.
		/// </summary>
		public override void Init () {
			// 打包ID
			if(string.IsNullOrEmpty(this.BuildID) == false) {
#if UNITY_EDITOR
				PlayerSettings.bundleIdentifier = this.BuildID;
#endif
			}

			// 版本号
			if(string.IsNullOrEmpty(this.BuildVersion) == false) {
#if UNITY_EDITOR && UNITY_EDITOR
				PlayerSettings.bundleVersion = this.BuildVersion;
#endif
			}

			// 版本号
			if(string.IsNullOrEmpty(this.BuildVersionCode) == false) {

#if UNITY_IOS && UNITY_EDITOR
				PlayerSettings.iOS.buildNumber = this.BuildVersionCode;
#endif
#if UNITY_ANDROID && UNITY_EDITOR
				int VersionCode = -1;
				bool result = int.TryParse(this.BuildVersionCode, out VersionCode);
				if(result == false) {
					Debug.LogErrorFormat("The BuildVersionCode is not a number (BuildVersionCode:{0})!!!",
						this.BuildVersionCode);
				} 
				PlayerSettings.Android.bundleVersionCode = VersionCode;
#endif

			}

			Debug.LogFormat ("[BuildInfo] BuildName : {0}", (this.BuildName == null) ? "null" : this.BuildName);
			Debug.LogFormat ("[BuildInfo] BuildID : {0}", (this.BuildID == null) ? "null" : this.BuildID);
			Debug.LogFormat ("[BuildInfo] BuildVersion : {0}", (this.BuildVersion == null) ? "null" : this.BuildVersion);
			Debug.LogFormat ("[BuildInfo] BuildShortVersion : {0}", (this.BuildShortVersion == null) ? "null" : this.BuildShortVersion);
			Debug.LogFormat ("[BuildInfo] BuildVersionCode : {0}", (this.BuildVersionCode == null) ? "null" : this.BuildVersionCode);
			Debug.LogFormat ("[BuildInfo] MarketVersion : {0}", (this.MarketVersion == null) ? "null" : this.MarketVersion);
		}

		/// <summary>
		/// 应用数据.
		/// </summary>
		/// <param name="iData">数据.</param>
		protected override void ApplyData(AssetBase iData) {

			if (iData == null) {
				return;
			}
				
			BuildInfo data = iData as BuildInfo;
			if (data == null) {
				return;
			}

			// 清空
			this.Clear ();

			this.BuildName = data.BuildName;
			this.BuildID = data.BuildID;
			this.BuildVersion = data.BuildVersion;
			this.BuildShortVersion = data.BuildShortVersion;
			this.BuildVersionCode = data.BuildVersionCode;
			this.MarketVersion = data.MarketVersion;

			UtilityAsset.SetAssetDirty (this);

		}

		/// <summary>
		/// 清空.
		/// </summary>
		public override void Clear() {

			UtilityAsset.Clear<BuildInfo> ();

			this.BuildName = null;
			this.BuildID = null;
			this.BuildVersion = null;
			this.BuildShortVersion = null;
			this.BuildVersionCode = null;
			this.MarketVersion = null;

			UtilityAsset.SetAssetDirty (this);

		}

		/// <summary>
		/// 从JSON文件，导入打包配置信息.
		/// </summary>
		public override void ImportFromJsonFile() {			
			BuildInfo jsonData = UtilityAsset.ImportFromJsonFile<BuildInfo> ();
			if (jsonData != null) {
				this.ApplyData (jsonData);
			}
		}

		/// <summary>
		/// 导出成JSON文件.
		/// </summary>
		/// <returns>导出路径.</returns>
		public override string ExportToJsonFile() {
			return UtilityAsset.ExportToJsonFile<BuildInfo> (this);
		}

		#endregion
	}
}
