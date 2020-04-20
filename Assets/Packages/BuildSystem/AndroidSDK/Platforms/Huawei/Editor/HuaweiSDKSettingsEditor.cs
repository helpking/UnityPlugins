using System.IO;
using Packages.Common.Editor;
using Packages.Logs;
using Packages.Utils;
using UnityEditor;

#if UNITY_ANDROID && UNITY_EDITOR

namespace Packages.BuildSystem.AndroidSDK.Platforms.Huawei.Editor {
	/// <summary>
	/// 华为SDK设定信息编辑器扩展.
	/// </summary>
	[CustomEditor(typeof(HuaweiSdkSettings))]  
	public class HuaweiSdkSettingsEditor 
		: AssetEditorBase<HuaweiSdkSettings, HuaweiSdkSettingsData> {
	
		/// <summary>
		/// 取得当前选中对象所在目录.
		/// </summary>
		/// <returns>当前选中对象所在目录.</returns>
		private static string GetCurDir()
		{
			var obj = Selection.GetFiltered(typeof(UnityEngine.Object), 
				SelectionMode.Assets);
			var path = AssetDatabase.GetAssetPath(obj[0]);

			if(path.Contains(".") == false)
			{
				path += "/";
			}

			return path;
		}

#region Creator

		/// <summary>
		/// 创建下载用Bar.
		/// </summary>
		[MenuItem("Assets/Create/AndroidSDK/HuaweiSDKSettings")]	
		static HuaweiSdkSettings Create () {
			const string funcBlock = "HuaweiSDKEditor:Create()";
			Loger.BuildStart(funcBlock);

			var curDir = GetCurDir ();
			if (Directory.Exists (curDir) == false) {
				return null;
			}
			var settings = UtilsAsset.CreateAsset<HuaweiSdkSettings> (curDir);
			if (settings != null) {
				// 初始化
				settings.Init();
			}
			Loger.BuildEnd();
			return settings;
		}

#endregion

#region MenuItem

		[MenuItem("Assets/AndroidSDK/Huawei/Clear", false, 600)]
		static void Clear() {

			const string funcBlock = "HuaweiSDKEditor:Clear()";
			Loger.BuildStart (funcBlock);

			var settings = HuaweiSdkSettings.GetInstance();
			if (settings != null) {
				settings.Clear ();
			}   

			Loger.BuildEnd ();
		}

		[MenuItem("Assets/AndroidSDK/Huawei/Json/Import", false, 600)]
		static void Import() {

			const string funcBlock = "HuaweiSDKEditor:Import()";
			Loger.BuildStart (funcBlock);

			var settings = HuaweiSdkSettings.GetInstance();
			if (settings != null) {
				settings.ImportFromJsonFile ();
			}

			Loger.BuildEnd ();
		}

		[MenuItem("Assets/AndroidSDK/Huawei/Json/Export", false, 600)]
		static void Export() {

			const string funcBlock = "HuaweiSDKEditor:Export()";
			Loger.BuildStart (funcBlock);

			var settings = HuaweiSdkSettings.GetInstance();
			if (settings != null) {
				settings.ExportToJsonFile ();
			}

			Loger.BuildEnd ();
		}

#endregion

	}
}

#endif
