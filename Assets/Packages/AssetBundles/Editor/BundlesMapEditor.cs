using Packages.Common.Editor;
using Packages.Utils;
using UnityEditor;

namespace Packages.AssetBundles.Editor {

	[CustomEditor(typeof(BundlesMap))]  
	public class BundlesMapEditor 
		: AssetEditorReadOnlyBase<BundlesMap, BundlesMapData> {

#region Create 

		/// <summary>
		/// 创建资源打包地图（用于打包）.
		/// </summary>
		[MenuItem("Assets/Create/Bundles/Map")]	
		static BundlesMap CreateBundlesMap ()	{	
			return UtilsAsset.CreateAsset<BundlesMap> ();	
		}

#endregion

#region File - Json

		/// <summary>
		/// 从JSON文件导入打包配置信息(Map).
		/// </summary>
		[MenuItem("Assets/Bundles/Map/File/Json/Import", false, 600)]
		static void ImportFromMapJsonFile() {

			var map = BundlesMap.GetInstance ();
			if (map != null) {
				map.ImportFromJsonFile();
			}

			UtilsAsset.AssetsRefresh ();
		}

		/// <summary>
		/// 将打包配置信息导出为JSON文件(Map).
		/// </summary>
		[MenuItem("Assets/Bundles/Map/File/Json/Export", false, 600)]
		static void ExportToMapJsonFile() {

			var map = BundlesMap.GetInstance ();
			if (map != null) {
				map.ExportToJsonFile();
			}

			UtilsAsset.AssetsRefresh ();
		}

		/// <summary>
		/// 清空 bundles map.
		/// </summary>
		[MenuItem("Assets/Bundles/Map/Clear", false, 600)]
		static void ClearBundlesMap() {
			var map = BundlesMap.GetInstance ();
			if (map != null) {
				map.Clear ();
			}

			UtilsAsset.AssetsRefresh ();
		}

#endregion

	}
}
