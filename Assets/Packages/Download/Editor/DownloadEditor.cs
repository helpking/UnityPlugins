using UnityEngine;
using UnityEditor; 
using System.Collections;
using Common;
using BuildSystem;
using AssetBundles;

namespace Download {

	/// <summary>
	/// 下载编辑器.
	/// </summary>
	public class DownloadEditor : Editor {

		#region Creator

		/// <summary>
		/// 创建下载列表.
		/// </summary>
		[MenuItem("Assets/Create/Download/List")]	
		static DownloadList CreateDownloadList ()	{	
			return UtilityAsset.CreateAsset<DownloadList> ();	
		}


		#endregion

		#region DownloadList

		/// <summary>
		/// 从JSON文件导入打包配置信息(DownloadList).
		/// </summary>
		[UnityEditor.MenuItem("Assets/Download/List/File/Json/Import", false, 600)]
		static void ImportDownloadListFromJsonFile() {

			const string funcBlock = "AssetBundleEditor.ImportDownloadListFromJsonFile()";
			BuildLogger.OpenBlock(funcBlock);

			DownloadList list = DownloadList.GetInstance ();
			if (list != null) {
				list.ImportFromJsonFile();
			}

			UtilityAsset.AssetsRefresh ();
			BuildLogger.CloseBlock(funcBlock);
		}

		/// <summary>
		/// 将服务器信息导出为JSON文件(DownloadList).
		/// </summary>
		[UnityEditor.MenuItem("Assets/Download/List/File/Json/Export", false, 600)]
		static void ExportDownloadListToJsonFile() {

			const string funcBlock = "AssetBundleEditor.ExportDownloadListToJsonFile()";
			BuildLogger.OpenBlock(funcBlock);

			DownloadList list = DownloadList.GetInstance ();
			if (list != null) {
				list.ExportToJsonFile();
			}

			UtilityAsset.AssetsRefresh ();
			BuildLogger.CloseBlock(funcBlock);
		}

		[UnityEditor.MenuItem("Assets/Download/List/Clear", false, 600)]
		static void ClearDownloadList() {
			const string funcBlock = "AssetBundleEditor.ClearDownloadList()";
			BuildLogger.OpenBlock(funcBlock);

			DownloadList list = DownloadList.GetInstance ();
			if (list != null) {
				list.Clear ();
			}

			UtilityAsset.AssetsRefresh ();
			BuildLogger.CloseBlock(funcBlock);
		}

		[UnityEditor.MenuItem("Assets/Download/List/Reset", false, 600)]
		static void ResetDownloadList() {
			const string funcBlock = "AssetBundleEditor.ResetDownloadList()";
			BuildLogger.OpenBlock(funcBlock);

			DownloadList list = DownloadList.GetInstance ();
			if (list != null) {
				list.Reset ();
			}

			UtilityAsset.AssetsRefresh ();
			BuildLogger.CloseBlock(funcBlock);
		}

		#endregion
	}
}
