using System.IO;
using Packages.BuildSystem.AndroidSDK.Options;
using Packages.Common.Base;
using Packages.Common.Editor;
using Packages.Logs;
using Packages.Utils;
using UnityEditor;
using UnityEngine;

#if UNITY_ANDROID

namespace Packages.BuildSystem.AndroidSDK.Platforms.Tiange.Editor {

	/// <summary>
	/// 天鸽SDK设定编辑器.
	/// </summary>
	[CustomEditor(typeof(TiangeSdkSettings))]  
	public class TiangeSdkSettingsEditor  
		: AssetOptionsEditorBase<TiangeSdkSettings, TiangeSdkData, TiangeSdkSettingsData, BuildSettingOptionsData> {
	
		/// <summary>
		/// 取得当前选中对象所在目录.
		/// </summary>
		/// <returns>当前选中对象所在目录.</returns>
		private new static string GetCurDir()
		{
			var obj = Selection.GetFiltered(typeof(Object), 
				SelectionMode.Assets);
			var path = AssetDatabase.GetAssetPath(obj[0]);

			if(path.Contains(".") == false)
			{
				path += "/";
			}

			return path;
		}

		/// <summary>
		/// 初始化主面板（选项）.
		/// </summary>
		/// <param name="iTarget">目标信息.</param>
		protected override void InitMainPanelOfOptions(TiangeSdkSettings iTarget) {
			if (null == iTarget) {
				return;
			}
			EditorGUILayout.LabelField ("Options");

			var optionNames = System.Enum.GetNames (typeof(SDKOptions));
			for (var idx = 0; idx < optionNames.Length; ++idx) {
				var optionName = optionNames [idx];
				if (string.IsNullOrEmpty (optionName)) {
					continue;
				}
				var option = (SDKOptions)System.Enum.Parse (typeof(SDKOptions), optionName);
				if (SDKOptions.None == option) {
					continue;
				}

				EditorGUILayout.BeginHorizontal ();
				EditorGUILayout.LabelField (" ", 
					GUILayout.Width(10.0f), GUILayout.Height(20.0f));
				EditorGUILayout.LabelField (optionName, 
					GUILayout.Width(100.0f), GUILayout.Height(20.0f));

				var isOn = EditorGUILayout.Toggle (iTarget.data.Options.IsOptionValid(option));

				iTarget.data.Options.SetOptionOnOrOff (option, isOn);
				EditorGUILayout.EndHorizontal ();
				if (isOn) {
					EditorGUILayout.PropertyField (
						serializedObject.FindProperty($"data.Options.{optionName}"),true);
				}
			}
		}

#region Creator

		/// <summary>
		/// 创建下载用Bar.
		/// </summary>
		[MenuItem("Assets/Create/AndroidSDK/TiangeSDKSettings")]	
		static TiangeSdkSettings Create () {
			const string funcBlock = "TiangeSDKSettingsEditor:Create()";
			Loger.BuildStart(funcBlock);

			var curDir = GetCurDir ();
			if (Directory.Exists (curDir) == false) {
				return null;
			}
			var settings = UtilsAsset.CreateAsset<TiangeSdkSettings> (curDir);
			if (settings != null) {
				// 初始化
				settings.Init();
			}
			Loger.BuildEnd();
			return settings;
		}

#endregion

#region MenuItem

		[MenuItem("Assets/AndroidSDK/Tiange/Clear", false, 600)]
		static void Clear() {

			const string funcBlock = "TiangeSDKSettingsEditor:Clear()";
			Loger.BuildStart (funcBlock);

			var settings = TiangeSdkSettings.GetInstance();
			if (settings != null) {
				settings.Clear ();
			}
			Loger.BuildEnd ();
		}

		[MenuItem("Assets/AndroidSDK/Tiange/Json/Import", false, 600)]
		static void Import() {

			const string funcBlock = "TiangeSDKSettingsEditor:Import()";
			Loger.BuildStart (funcBlock);

			var settings = TiangeSdkSettings.GetInstance();
			if (settings != null) {
				settings.ImportFromJsonFile ();
			}

			Loger.BuildEnd ();
		}

		[MenuItem("Assets/AndroidSDK/Tiange/Json/Export", false, 600)]
		static void Export() {

			const string funcBlock = "TiangeSDKSettingsEditor:Export()";
			Loger.BuildStart (funcBlock);

			var settings = TiangeSdkSettings.GetInstance();
			if (settings != null) {
				settings.ExportToJsonFile ();
			}

			Loger.BuildEnd ();
		}

#endregion

	}
}

#endif