using UnityEngine;
using UnityEditor;
using Packages.Common.Base;
using Packages.Common.Editor;
using Packages.Common.Extend;
using Packages.Common.Extend.Editor;
using Packages.Utils;
using Packages.Logs;

namespace Packages.Settings.Editor
{
	/// <summary>
	/// 系统设定信息编辑器
	/// </summary>
	[CustomEditor(typeof(SysSettings))]  
	public class SysSettingsEditor : 
		AssetOptionsEditorBase<SysSettings, SysSettingsData, SysGeneralData, SysOptionsData>{

		/// <summary>
		/// 初始化标题信息.
		/// </summary>
		/// <param name="iTarget">目标信息.</param>
		protected override void InitTitleInfo(SysSettings iTarget) {
			base.InitTitleInfo(iTarget);
			
			// 日志等级
			Loger.LogLevel = (LogLevel)DrawSelectList(0, "LogLevel", Loger.LogLevel);
			// 日志是否输出
			DrawSingleToggle(0, "日志输出", Loger.LogOutput,
				delegate(bool iValue) { Loger.LogOutput = iValue; });
			// 日志文件输出大小
			var logFileMaxSize = Loger.LogFileMaxSize;
			DrawSlider(0, "日志文件大小(单位:MB)", ref logFileMaxSize, 1.0f, 16.0f);
			Loger.LogFileMaxSize = logFileMaxSize;
			
			// 日志输出目录
			var outputDir = Loger.OutputDir.Replace(Application.dataPath, "");
			outputDir = outputDir.Substring(1);
			EditorGUILayout.LabelField ("日志输出", outputDir);
		}
		
		/// <summary>
		/// 初始化主面板（选项）.
		/// </summary>
		/// <param name="iTarget">目标信息.</param>
		protected override void InitMainPanelOfOptions(SysSettings iTarget)
		{
			if (null == iTarget)
			{
				return;
			}
			
			EditorGUILayout.BeginHorizontal ();
			if (GUILayout.Button ("CheckAllSE", 
				GUILayout.Width(150.0f), 
				GUILayout.Height(20.0f))) {
				foreach(var loop in iTarget.data.se) {
					loop.Check();
				}
			}
			if (GUILayout.Button ("ResetAllSE", 
				GUILayout.Width(150.0f), 
				GUILayout.Height(20.0f))) {
				foreach(var loop in iTarget.data.se) {
					loop.Reset();
				}
			}
			EditorGUILayout.EndHorizontal ();
			
			// 音效
			DrawSerializedObject(0, "data.se");
			
			// 网络
			DrawSerializedObject(0, "data.network");
			
			// Tips
			DrawSerializedObject(0, "data.tips");

			var optionNames = System.Enum.GetNames (typeof(SDKOptions));
			// 去掉TSDKOptions.None
			var intOpsMaxCnt = optionNames.Length - 1;
			intOpsMaxCnt = intOpsMaxCnt >= 1 ? intOpsMaxCnt : 0;

			const int intMaxCCnt = 3;
			var intMaxRCnt = (intOpsMaxCnt - intOpsMaxCnt % intMaxCCnt) / intMaxCCnt;
			intMaxRCnt = intOpsMaxCnt % intMaxCCnt == 0 ? intMaxRCnt : intMaxRCnt + 1;
			for (var idxR = 0; idxR < intMaxRCnt; ++idxR) {
				EditorGUILayout.BeginHorizontal ();
				var isBreak = false;
				for (var idxC = 0; idxC < intMaxCCnt; ++idxC) {
					var intOpsIdx = idxR * intMaxCCnt + idxC;
					if (intOpsIdx >= intOpsMaxCnt) {
						isBreak = true;
						break;
					}
					// 不包含TSDKOptions.None
					var optionName = optionNames [idxR * intMaxCCnt + idxC + 1];
					if (string.IsNullOrEmpty (optionName)) {
						continue;
					}
					EditorGUILayout.LabelField (" ", 
						GUILayout.Width(15.0f), GUILayout.Height(20.0f));

					var option = (SDKOptions)System.Enum.Parse (typeof(SDKOptions), optionName);
					var isOn = EditorGUILayout.Toggle (iTarget.data.Options.IsOptionValid(option), 
						GUILayout.Width(10.0f), GUILayout.Height(20.0f));
					iTarget.data.Options.SetOptionOnOrOff (option, isOn);

					EditorGUILayout.LabelField (optionName, 
						GUILayout.Width(60.0f), GUILayout.Height(20.0f));
				}

				if (isBreak) {
					EditorGUILayout.EndHorizontal ();
					break;
				}
				EditorGUILayout.EndHorizontal ();
			}
		}

#region Create
		
		/// <summary>
		/// 创建打包信息配置文件.
		/// </summary>
		[MenuItem("Assets/Create/SysSettings")]
		static SysSettings Create () {	
			return CreateAsset();  
		}
		
#endregion

#region File - Json

		/// <summary>
		/// 从JSON文件导入打包配置信息(BuildInfo).
		/// </summary>
		[MenuItem("Assets/SysSettings/File/Json/Import", false, 600)]
		static void Import() {

			var info = SysSettings.GetInstance ();
			if (null != info) {
				info.ImportFromJsonFile();
			}

			UtilsAsset.AssetsRefresh ();
		}



		/// <summary>
		/// 将打包配置信息导出为JSON文件(BuildInfo).
		/// </summary>
		[MenuItem("Assets/SysSettings/File/Json/Export", false, 600)]
		static void Export() {

			var info = SysSettings.GetInstance ();
			if (null != info) {
				info.ExportToJsonFile();
			}

			UtilsAsset.AssetsRefresh ();
		}

		/// <summary>
		/// 清空 bundles map.
		/// </summary>
		[MenuItem("Assets/SysSettings/Clear", false, 600)]
		static void Clear() {
			var info = SysSettings.GetInstance ();
			if (null != info) {
				info.Clear ();
			}
			UtilsAsset.AssetsRefresh ();
		}

#endregion
		
	}
}
