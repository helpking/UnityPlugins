using UnityEditor;
using UnityEngine;
using Packages.Common.Base;
using Packages.Common.Editor;
using Packages.Utils;

namespace Packages.AssetBundles.Editor {

	/// <summary>
	/// Upload list editor.
	/// </summary>
	[CustomEditor(typeof(BundlesResult))]  
	public class UploadListEditor 
		: AssetEditorBase<BundlesResult, BundlesResultData> {

		/// <summary>
		/// 初始化主面板.
		/// </summary>
		/// <param name="iTarget">目标信息.</param>
		protected override void InitMainPanel(BundlesResult iTarget) {
			// BuildName
			EditorGUILayout.LabelField ("BuildName", iTarget.BuildName);
			// 文件后缀
			iTarget.FileSuffix = EditorGUILayout.TextField ("FileSuffix", iTarget.FileSuffix);
			// 检测模式
			iTarget.CheckMode = (CheckMode)EditorGUILayout.EnumPopup ("CheckMode", iTarget.CheckMode);
			// BuildTarget
			EditorGUILayout.LabelField ("BuildTarget", iTarget.BuildTarget);
			// AppVersion
			EditorGUILayout.LabelField ("AppVersion", iTarget.AppVersion);
			// 检测模式
			iTarget.CompressFormat = (CompressFormat)EditorGUILayout.EnumPopup ("CompressFormat", iTarget.CompressFormat);
			// 上传Manifest
			iTarget.ManifestUpload = EditorGUILayout.Toggle("上传Manifest", iTarget.ManifestUpload);

			// Targets
			EditorGUILayout.PropertyField (serializedObject.FindProperty("data.targets"),true);
		}

		/// <summary>
		/// 初始化顶部按钮列表.
		/// </summary>
		/// <param name="iTarget">目标信息.</param>
		protected override void InitTopButtons(BundlesResult iTarget) {

			base.InitTopButtons (iTarget);

			// 清空按钮
			if (!GUILayout.Button("ClearList")) return;
			iTarget.data.targets.Clear();
			iTarget.ExportToJsonFile ();
		}

		/// <summary>
		/// 初始化底部按钮列表.
		/// </summary>
		/// <param name="iTarget">目标信息.</param>
		protected override void InitBottomButtons(BundlesResult iTarget) {

			base.InitBottomButtons (iTarget);

			// 清空按钮
			if (!GUILayout.Button("ClearList")) return;
			iTarget.data.targets.Clear();
			iTarget.ExportToJsonFile ();
		}

#region Creator

		/// <summary>
		/// 创建资源打包地图（用于打包）.
		/// </summary>
		[MenuItem("Assets/Create/BundlesResult")]	
		static BundlesResult Create ()	{	
			return UtilsAsset.CreateAsset<BundlesResult> ();	
		}

#endregion

#region File - Json

		/// <summary>
		/// 从JSON文件导入打包配置信息(Info).
		/// </summary>
		[MenuItem("Assets/BundlesResult/File/Json/Import", false, 600)]
		static void Import() {

			var info = BundlesResult.GetInstance ();
			if (info != null) {
				info.ImportFromJsonFile();
			}

			UtilsAsset.AssetsRefresh ();
		}

		/// <summary>
		/// 将打包配置信息导出为JSON文件(Info).
		/// </summary>
		[MenuItem("Assets/BundlesResult/File/Json/Export", false, 600)]
		static void Export() {

			var info = BundlesResult.GetInstance ();
			if (info != null) {
				info.ExportToJsonFile();
			}

			UtilsAsset.AssetsRefresh ();
		}

		/// <summary>
		/// 清空 bundles信息.
		/// </summary>
		[MenuItem("Assets/BundlesResult/Clear", false, 600)]
		static void Clear() {
			var info = BundlesResult.GetInstance ();
			if (info != null) {
				info.Clear ();
			}

			UtilsAsset.AssetsRefresh ();
		}

#endregion
	}

}