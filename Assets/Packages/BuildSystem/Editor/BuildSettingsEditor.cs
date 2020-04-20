using Packages.BuildSystem.Settings;
using Packages.Common.Editor;
using UnityEditor;

namespace Packages.BuildSystem.Editor {

	/// <summary>
	/// 工程设置器.
	/// </summary>
	[CustomEditor(typeof(BuildSettings))]  
	public class BuildSettingsEditor 
		: AssetEditorBase<BuildSettings, BuildSettingsData> {


#region Creator

		/// <summary>
		/// 创建工程设置器.
		/// </summary>
		[MenuItem("Assets/Create/BuildSettings")]	
		static BuildSettings Create ()	{	
			return CreateAssetAtCurDir();	
		}
			
#endregion

#region XCode - MenuItem

		[MenuItem("Assets/BuildSettings/XCode/Clear", false, 600)]
		static void XCodeClear() {

			var settings = BuildSettings.GetInstance(BuildSettings.AssetFileDir);
			if (settings != null) {
				settings.XCodeClear ();
			}
		}

		[MenuItem("Assets/BuildSettings/XCode/Reset", false, 600)]
		static void XCodeReset() {

			var settings = BuildSettings.GetInstance(BuildSettings.AssetFileDir);
			if (settings != null) {
				settings.XCodeReset ();
			}
		}

		[MenuItem("Assets/BuildSettings/JSon/Import", false, 600)]
		static void Import() {

			var settings = BuildSettings.GetInstance(BuildSettings.AssetFileDir);
			if (settings != null) {
				settings.ImportFromJsonFile ();
			}
		}

		[MenuItem("Assets/BuildSettings/JSon/Export", false, 600)]
		static void Export() {

			var settings = BuildSettings.GetInstance(BuildSettings.AssetFileDir);
			if (settings != null) {
				settings.ExportToJsonFile ();
			}
		}

#endregion

	}
}
