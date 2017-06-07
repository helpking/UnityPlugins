using UnityEngine;
using UnityEditor; 
using System.Collections;
using BuildSystem;
using Common;
using AssetBundles;

namespace Upload {

	/// <summary>
	/// 上传编辑器.
	/// </summary>
	public class UploadEditor : Editor {

		#region Creator
		/// <summary>
		/// 创建资源打包地图（用于打包）.
		/// </summary>
		[MenuItem("Assets/Create/Upload/List")]	
		static UploadList CreateBundlesInfo ()	{	
			return UtilityAsset.CreateAsset<UploadList> ();	
		}

		/// <summary>
		/// 创建服务器配置信息.
		/// </summary>
		[MenuItem("Assets/Create/Upload/ServerConf")]	
		static ServersConf CreateServersConf ()	{	
			return UtilityAsset.CreateAsset<ServersConf> ();	
		}

		#endregion

		#region UploadList

		/// <summary>
		/// 从JSON文件导入打包配置信息(Info).
		/// </summary>
		[UnityEditor.MenuItem("Assets/Upload/List/File/Json/Import", false, 600)]
		static void ImportCheckFromJsonFile() {

			const string funcBlock = "AssetBundleEditor.ImportCheckFromJsonFile()";
			BuildLogger.OpenBlock(funcBlock);

			UploadList info = UploadList.GetInstance ();
			if (info != null) {
				info.ImportFromJsonFile();
			}

			UtilityAsset.AssetsRefresh ();
			BuildLogger.CloseBlock(funcBlock);
		}



		/// <summary>
		/// 将打包配置信息导出为JSON文件(Info).
		/// </summary>
		[UnityEditor.MenuItem("Assets/Upload/List/File/Json/Export", false, 600)]
		static void ExportCheckToJsonFile() {

			const string funcBlock = "AssetBundleEditor.ExportCheckToJsonFile()";
			BuildLogger.OpenBlock(funcBlock);

			UploadList info = UploadList.GetInstance ();
			if (info != null) {
				info.ExportToJsonFile();
			}

			UtilityAsset.AssetsRefresh ();
			BuildLogger.CloseBlock(funcBlock);
		}

		/// <summary>
		/// 清空 bundles信息.
		/// </summary>
		[UnityEditor.MenuItem("Assets/Upload/List/Clear", false, 600)]
		static void ClearBundlesCheck() {
			const string funcBlock = "AssetBundleEditor.ClearBundlesCheck()";
			BuildLogger.OpenBlock(funcBlock);

			UploadList info = UploadList.GetInstance ();
			if (info != null) {
				info.Clear ();
			}

			UtilityAsset.AssetsRefresh ();
			BuildLogger.CloseBlock(funcBlock);
		}

		/// <summary>
		/// 清空 bundles信息.
		/// </summary>
		[UnityEditor.MenuItem("Assets/Upload/List/Reset", false, 600)]
		static void ResetBundlesCheck() {
			const string funcBlock = "AssetBundleEditor.ResetBundlesCheck()";
			BuildLogger.OpenBlock(funcBlock);

			UploadList info = UploadList.GetInstance ();
			if (info != null) {
				info.Reset ();
			}

			UtilityAsset.AssetsRefresh ();
			BuildLogger.CloseBlock(funcBlock);
		}

		#endregion

		#region ServerConf

		/// <summary>
		/// 从JSON文件导入打包配置信息(ServersConf).
		/// </summary>
		[UnityEditor.MenuItem("Assets/Upload/ServersConf/File/Json/Import", false, 600)]
		static void ImportServersConfFromJsonFile() {

			const string funcBlock = "AssetBundleEditor.ImportServersConfFromJsonFile()";
			BuildLogger.OpenBlock(funcBlock);

			ServersConf conf = ServersConf.GetInstance ();
			if (conf != null) {
				conf.ImportFromJsonFile();
			}

			UtilityAsset.AssetsRefresh ();
			BuildLogger.CloseBlock(funcBlock);
		}

		/// <summary>
		/// 将服务器信息导出为JSON文件(ServersConf).
		/// </summary>
		[UnityEditor.MenuItem("Assets/Upload/ServersConf/File/Json/Export", false, 600)]
		static void ExportServersConfToJsonFile() {

			const string funcBlock = "AssetBundleEditor.ExportServersConfToJsonFile()";
			BuildLogger.OpenBlock(funcBlock);

			ServersConf conf = ServersConf.GetInstance ();
			if (conf != null) {
				conf.ExportToJsonFile();
			}

			UtilityAsset.AssetsRefresh ();
			BuildLogger.CloseBlock(funcBlock);
		}

		[UnityEditor.MenuItem("Assets/Upload/ServersConf/Clear", false, 600)]
		static void ClearServersConf() {
			const string funcBlock = "AssetBundleEditor.ClearServersConf()";
			BuildLogger.OpenBlock(funcBlock);

			ServersConf conf = ServersConf.GetInstance ();
			if (conf != null) {
				conf.Clear ();
			}

			UtilityAsset.AssetsRefresh ();
			BuildLogger.CloseBlock(funcBlock);
		}

		[UnityEditor.MenuItem("Assets/Upload/ServersConf/ClearDirs", false, 600)]
		static void ClearCreatedDirsInfo() {
			const string funcBlock = "AssetBundleEditor.ClearCreatedDirsInfo()";
			BuildLogger.OpenBlock(funcBlock);

			ServersConf conf = ServersConf.GetInstance ();
			if (conf != null) {
				conf.ClearCreatedDir ();
			}

			UtilityAsset.AssetsRefresh ();
			BuildLogger.CloseBlock(funcBlock);
		}

		#endregion
	}

}
