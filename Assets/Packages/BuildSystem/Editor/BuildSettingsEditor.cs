using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections;
using Common;
using BuildSystem;
using AssetBundles;

namespace BuildSystem {

	/// <summary>
	/// 工程设置器.
	/// </summary>
	[CustomEditor(typeof(BuildSettings))]  
	public class BuildSettingsEditor : Editor {

		/// <summary>
		/// 取得当前选中对象所在目录.
		/// </summary>
		/// <returns>当前选中对象所在目录.</returns>
		private static string GetCurDir()
		{
			UnityEngine.Object[] obj = Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.Assets) as UnityEngine.Object[];
			string path = AssetDatabase.GetAssetPath(obj[0]);

			if(path.Contains(".") == false)
			{
				path += "/";
			}

			return path;
		}

		#region Creator

		/// <summary>
		/// 创建工程设置器.
		/// </summary>
		[MenuItem("Assets/Create/BuildSettings")]	
		static BuildSettings CreateBuildSettings ()	{	

			const string funcBlock = "BuildSettingsEditor.CreateBuildSettings()";
			BuildLogger.OpenBlock(funcBlock);

			string curDir = GetCurDir ();
			if (Directory.Exists (curDir) == false) {
				return null;
			}
			BuildLogger.CloseBlock (funcBlock);
			return UtilityAsset.CreateAsset<BuildSettings> (curDir);	
		}
			
		#endregion

		#region XCode-MenuItem

		[UnityEditor.MenuItem("Assets/BuildSettings/XCode/Clear", false, 600)]
		static void XCodeClear() {

			const string funcBlock = "BuildSettingsEditor.XCodeClear()";
			BuildLogger.OpenBlock (funcBlock);

			BuildSettings settings = BuildSettings.GetInstance();
			if (settings != null) {
				settings.XCodeClear ();
			}   

			BuildLogger.CloseBlock (funcBlock);
		}

		[UnityEditor.MenuItem("Assets/BuildSettings/XCode/Reset", false, 600)]
		static void XCodeReset() {

			const string funcBlock = "BuildSettingsEditor.XCodeReset()";
			BuildLogger.OpenBlock (funcBlock);

			BuildSettings settings = BuildSettings.GetInstance();
			if (settings != null) {
				settings.XCodeReset ();
			}

			BuildLogger.CloseBlock (funcBlock);
		}

		[UnityEditor.MenuItem("Assets/BuildSettings/JSon/Import", false, 600)]
		static void Import() {

			const string funcBlock = "BuildSettingsEditor.Import()";
			BuildLogger.OpenBlock (funcBlock);

			BuildSettings settings = BuildSettings.GetInstance();
			if (settings != null) {
				settings.ImportFromJsonFile ();
			}

			BuildLogger.CloseBlock (funcBlock);
		}

		[UnityEditor.MenuItem("Assets/BuildSettings/JSon/Export", false, 600)]
		static void Export() {

			const string funcBlock = "BuildSettingsEditor.Export()";
			BuildLogger.OpenBlock (funcBlock);

			BuildSettings settings = BuildSettings.GetInstance();
			if (settings != null) {
				settings.ExportToJsonFile ();
			}

			BuildLogger.CloseBlock (funcBlock);
		}

		#endregion

		#region XCode-Inspector

		public override void OnInspectorGUI ()  {
			serializedObject.Update ();

			BuildSettings setting = target as BuildSettings; 
			if (setting != null) {

				EditorGUILayout.PropertyField (serializedObject.FindProperty("XCodeSeetings"),true);
			}

			// 保存变化后的值
			serializedObject.ApplyModifiedProperties();
		}
			
		#endregion
	}
}
