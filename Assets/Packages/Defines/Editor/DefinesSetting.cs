using System;
using System.Linq;
using UnityEditor;
using Packages.BuildSystem.Editor;
using Packages.Command.Editor;
using Packages.Common.Base;
using Packages.Utils;
using Packages.Logs;

namespace Packages.Defines.Editor {
	
	/// <summary>
	/// 宏设定.
	/// </summary>
	public class DefinesSetting : ClassExtension {

		/// <summary>
		/// 追加宏(iOS).
		/// </summary>
		public static void AddIOSDefines() {

			const string funcBlock = "DefinesSetting::AddIOSDefines()";
			Loger.BuildStart(funcBlock);

			var defines = BuildParameters.Defines;
			SetDefines (defines, BuildTargetGroup.iOS);

			Loger.BuildEnd();
		}

		/// <summary>
		/// 追加宏(iOS).
		/// </summary>
		public static void AddAndroidDefines() {

			const string funcBlock = "DefinesSetting::AddAndroidDefines()";
			Loger.BuildStart(funcBlock);

			var defines = BuildParameters.Defines;
			SetDefines (defines, BuildTargetGroup.Android);

			Loger.BuildEnd();
		}
			
		/// <summary>
		/// 设定宏.
		/// </summary>
		/// <param name="iDefines">宏.</param>
		/// <param name="iTargetGroup">目标组.</param>
		private static void SetDefines(string[] iDefines, BuildTargetGroup iTargetGroup) {

			if (null == iDefines || 0 >= iDefines.Length) {
				return;
			}

			bool fileExistFlg;
			var definesData = UtilsAsset.ImportDataByDir<DefinesData> (
				out fileExistFlg, DefinesWindow.JsonFileDir);
			if (null == definesData) {
				return;
			}
			 
			// 追加
			definesData.AddDefines (iDefines, 
				(BuildTargetGroup.Android == iTargetGroup), 
				(BuildTargetGroup.iOS == iTargetGroup));

			// 重新导出
			UtilsAsset.ExportData(definesData, DefinesWindow.JsonFileDir);

			// 应用设定信息
			definesData.Apply();

		}
			
		/// <summary>
		/// 设定宏.
		/// </summary>
		/// <param name="iDefines">宏.</param>
		/// <param name="iTargetGroup">目标组.</param>
		public static void SetDefines(DefineInfo[] iDefines, BuildTargetGroup iTargetGroup) {

			if (null == iDefines || 0 >= iDefines.Length) {
				return;
			}

			var defines = iDefines
				.Where(iDefine => !string.IsNullOrEmpty(iDefine.name))
				.Aggregate<DefineInfo, string>(
					null, 
					(iCurrent, iDefine) => string.IsNullOrEmpty(iCurrent) ? iDefine.name : string.Format("{0};{1}",
						iCurrent, iDefine.name));

			if (string.IsNullOrEmpty(defines)) return;
			PlayerSettings.SetScriptingDefineSymbolsForGroup (iTargetGroup, defines); 
			Loger.BuildLog($"DefinesSetting()::Defines({iTargetGroup}):{defines}");
		}
	}
}
