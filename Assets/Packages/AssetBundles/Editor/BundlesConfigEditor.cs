using System;
using Packages.Common.Editor;
using Packages.Utils;
using UnityEditor;

namespace Packages.AssetBundles.Editor {

	[CustomEditor(typeof(BundlesConfig))]  
	public class BundlesConfigEditor 
		: AssetEditorBase<BundlesConfig, BundlesConfigData> {

#region Create 

		/// <summary>
		/// 创建资源打包配置文件（用于设定当前打包对象）.
		/// </summary>
		[MenuItem("Assets/Create/Bundles/Config")]	
		static BundlesConfig CreateBundlesConfig ()	{	
			return UtilsAsset.CreateAsset<BundlesConfig> ();
		}

#endregion

#region File - Json

		[MenuItem("Assets/Bundles/AllClear", false, 600)]
		static void TotalClear() {
			var bcConfig = BundlesConfig.GetInstance();
			if (bcConfig != null) {
				bcConfig.Clear ();
			}

			var map = BundlesMap.GetInstance ();
			if (map != null) {
				map.Clear ();
			}

			var info = BundlesResult.GetInstance ();
			if (info != null) {
				info.Clear ();
			}

			UtilsAsset.AssetsRefresh ();
		}
			
		[MenuItem("Assets/Bundles/Config/Clear", false, 600)]
		static void Clear() {
			var bcConfig = BundlesConfig.GetInstance();
			if (bcConfig != null) {
				bcConfig.Clear ();
			}

			UtilsAsset.AssetsRefresh ();
		}
			
		/// <summary>
		/// 从JSON文件导入打包配置信息.
		/// </summary>
		[MenuItem("Assets/Bundles/Config/File/Json/Import", false, 600)]
		static void Import() {

			var bcConfig = BundlesConfig.GetInstance();
			if (bcConfig != null) {
				bcConfig.ImportFromJsonFile ();
			}

			UtilsAsset.AssetsRefresh ();
		}

		/// <summary>
		/// 将打包配置信息导出为JSON文件.
		/// </summary>
		[MenuItem("Assets/Bundles/Config/File/Json/Export", false, 600)]
		static void Export() {

			var bcConfig = BundlesConfig.GetInstance();
			if (bcConfig != null) {
				bcConfig.ExportToJsonFile ();
			}

			UtilsAsset.AssetsRefresh ();
		}

#endregion

#region AssetBundles - Resource

		[MenuItem("Assets/Bundles/Config/Resources/OneDir", false, 600)]
		static void BundleOneDir()
		{
			var path = GetCurDir();

			var bcConfig = BundlesConfig.GetInstance();
			if (bcConfig != null) {
				bcConfig.AddResource (BundleMode.OneDir, path);
			}

			UtilsAsset.AssetsRefresh ();
		}

		[MenuItem("Assets/Bundles/Config/Resources/FileOneToOne", false, 600)]
		static void BundleFileOneToOne()
		{
			var path = GetCurDir();

			var bcConfig = BundlesConfig.GetInstance();
			if (bcConfig != null) {
				bcConfig.AddResource (BundleMode.FileOneToOne, path);
			}
			UtilsAsset.AssetsRefresh ();
		}

		[MenuItem("Assets/Bundles/Config/Resources/TopDirOneToOne", false, 600)]
		static void BundleTopDirOneToOne()
		{
			var path = GetCurDir();

			var bcConfig = BundlesConfig.GetInstance();
			if (bcConfig != null) {
				bcConfig.AddResource (BundleMode.TopDirOneToOne, path);
			}
			UtilsAsset.AssetsRefresh ();
		}

		[MenuItem("Assets/Bundles/Config/Resources/SceneOneToOne", false, 600)]
		static void BundleSceneOneToOne()
		{
			var path = GetCurDir();

			var bcConfig = BundlesConfig.GetInstance();
			if (bcConfig != null) {
				bcConfig.AddResource (BundleMode.SceneOneToOne, path);
			}
			UtilsAsset.AssetsRefresh ();
		}

		/// <summary>
		/// 从打包配置信息中移除当前指定对象.
		/// </summary>
		[MenuItem("Assets/Bundles/Config/Resources/Remove", false, 600)]
		static void RemoveSource()
		{
			var curPath = GetCurDir();
			var bcConfig = BundlesConfig.GetInstance();
			if (bcConfig != null) {
				bcConfig.RemoveResource (curPath);
			}
			UtilsAsset.AssetsRefresh ();
		}

		/// <summary>
		/// 清空打包资源列表.
		/// </summary>
		[MenuItem("Assets/Bundles/Config/Resources/Clear", false, 600)]
		static void ClearAll() {
			var bcConfig = BundlesConfig.GetInstance();
			if (bcConfig != null) {
				bcConfig.ClearResources ();
			}
			UtilsAsset.AssetsRefresh ();
		}

		/// <summary>
		/// 将当前目录或者文件，添加到菜单.
		/// </summary>
		[MenuItem("Assets/Bundles/Config/Resources/Ignore/Ignore", false, 600)]
		static void IgnoreCurrentTarget()
		{
			var curPath = GetCurDir();
			var lastIndex = curPath.LastIndexOf ("/", StringComparison.Ordinal);
			lastIndex = lastIndex == curPath.Length - 1 ? curPath.Length : lastIndex + 1;
			var resourcePath = curPath.Substring (0, lastIndex);

			// 设定忽略列表
			var bcConfig = BundlesConfig.GetInstance();
			if (bcConfig != null) {
				bcConfig.AddIgnoreTarget (resourcePath, curPath);
			}
			UtilsAsset.AssetsRefresh ();
		}

		/// <summary>
		/// 将当前目录或者文件，添加到菜单.
		/// </summary>
		[MenuItem("Assets/Bundles/Config/Resources/Ignore/Remove", false, 600)]
		static void RemoveIgnoreCurrentTarget()
		{
			var curPath = GetCurDir();
			var lastIndex = curPath.LastIndexOf ("/", StringComparison.Ordinal);
			lastIndex = lastIndex == curPath.Length - 1 ? curPath.Length : lastIndex + 1;
			var resourcePath = curPath.Substring (0, lastIndex);

			// 设定忽略列表
			var bcConfig = BundlesConfig.GetInstance();
			if (bcConfig != null) {
				bcConfig.RemoveIgnoreInfo (resourcePath, curPath);
			}
			UtilsAsset.AssetsRefresh ();
		}

		/// <summary>
		/// 清空打包资源列表.
		/// </summary>
		[MenuItem("Assets/Bundles/Config/Resources/Ignore/Clear", false, 600)]
		static void ClearAllIgnoreInfo() {

			var curPath = GetCurDir();
			var lastIndex = curPath.LastIndexOf ("/", StringComparison.Ordinal);
			lastIndex = lastIndex == curPath.Length - 1 ? curPath.Length : lastIndex + 1;
			var resourcePath = curPath.Substring (0, lastIndex);

			var bcConfig = BundlesConfig.GetInstance();
			if (bcConfig != null) {
				bcConfig.ClearAllIgnoreInfo (resourcePath);
			}
			UtilsAsset.AssetsRefresh ();
		}

#endregion

#region AssetBundles - UnResource

		/// <summary>
		/// 设定当前对象为非资源对象，并添加到打包资源配置信息.
		/// </summary>
		[MenuItem("Assets/Bundles/Config/UnResources/Add", false, 600)]
		static void AddUnResource()
		{
			var path = GetCurDir();

			var bcConfig = BundlesConfig.GetInstance();
			if (bcConfig != null) {
				bcConfig.AddUnResource (path);
			}
			UtilsAsset.AssetsRefresh ();
		}

		/// <summary>
		/// 从当前打包资源配置信息的非资源列表中移除当前对象.
		/// </summary>
		[MenuItem("Assets/Bundles/Config/UnResources/Remove", false, 600)]
		static void RemoveUnResource()
		{
			var curPath = GetCurDir();
			var lastIndex = curPath.LastIndexOf ("/", StringComparison.Ordinal);
			lastIndex = (lastIndex == (curPath.Length - 1)) ? curPath.Length : (lastIndex + 1);
			var resourcePath = curPath.Substring (0, lastIndex);

			var bcConfig = BundlesConfig.GetInstance();
			if (bcConfig != null) {
				bcConfig.RemoveUnResource (resourcePath);
			}
			UtilsAsset.AssetsRefresh ();
		}

		/// <summary>
		/// 清空非资源列表.
		/// </summary>
		[MenuItem("Assets/Bundles/Config/UnResources/Clear", false, 600)]
		static void ClearAllUnResources() {
			var bcConfig = BundlesConfig.GetInstance();
			if (bcConfig != null) {
				bcConfig.ClearUnResources ();
			}
			UtilsAsset.AssetsRefresh ();
		}

#endregion

	}

}
