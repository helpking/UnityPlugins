using System;
using System.IO;
using UnityEngine;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using UnityEditor;
#endif
using Packages.AssetBundles;
using Packages.Common.Base;
using Packages.Logs;

namespace Packages.Utils {
	
	/// <summary>
	/// Utility asset.
	/// </summary>
	public static class UtilsAsset {	

		/// <summary>
		/// 数据路径.
		/// </summary>
		private static readonly string DataPath = Application.dataPath;
		/// <summary>
		/// asset文件数据路径.
		/// </summary>
		private const string AssetDataDir = "Resources/Conf";

		/// <summary>
		/// 标记目标物体已改变.
		/// </summary>
		/// <param name="iTarget">已改变目标.</param>
		public static void SetAssetDirty(Object iTarget)
		{
#if UNITY_EDITOR
			if(iTarget != null) {
				EditorUtility.SetDirty (iTarget);
			}
#endif
		}

		/// <summary>
		/// 刷新.
		/// </summary>
		public static void AssetsRefresh() {

#if UNITY_EDITOR
			AssetDatabase.Refresh();	
			AssetDatabase.SaveAssets ();
#endif
		}

		/// <summary>
		/// 清空GC
		/// </summary>
		public static void ClearGC()
		{
			Resources.UnloadUnusedAssets();
			// 清空GC
			GC.Collect();
		}
		
		/// <summary>
		/// 释放Asset内存
		/// </summary>
		/// <param name="iTarget">目标对象</param>
		/// <param name="iIsAsset">Asset标志位(区别于GameObject)</param>
		/// <param name="iIsGCClear">GC清空标志位</param>
		public static void ReleaseMemory(
			Object iTarget, bool iIsAsset, bool iIsGCClear = true)
		{
			if (null != iTarget)
			{
				if (iIsAsset)
				{
					Resources.UnloadAsset(iTarget);
				}
				else
				{
					Object.DestroyImmediate(iTarget);
				}
			}
		}

		/// <summary>
		/// 开始打包Bundles.
		/// </summary>
		public static void StartBuildBundles() {
		}

		/// <summary>
		/// 结束打包Bundles.
		/// </summary>
		public static void EndBuildBundles() {

			// 刷新Assets
			AssetsRefresh ();
		}

		/// <summary>	
		/// 创建Asset.	
		/// </summary>	
		/// <param name="iDirPath">创建目录.</param>
		public static T CreateAsset<T> (string iDirPath = null) where T : ScriptableObject {	

			T objRet = default(T);

#if UNITY_EDITOR

			string assetFullPath = null;
			try {

				assetFullPath = GetAssetFilePath<T> (iDirPath);
				if(assetFullPath.StartsWith("Resources/")) {
					assetFullPath = $"{DataPath}/{assetFullPath}";
				}
				if(assetFullPath.EndsWith(".asset") == false) {
					assetFullPath = $"{assetFullPath}.asset";
				}
				if(File.Exists(assetFullPath)) {
					File.Delete(assetFullPath);
				}
				assetFullPath = assetFullPath.Replace(DataPath, "Assets");
				assetFullPath = AssetDatabase.GenerateUniqueAssetPath (assetFullPath);

				T asset = ScriptableObject.CreateInstance<T> (); 	
				AssetDatabase.CreateAsset (asset, assetFullPath); 	
				AssetsRefresh();	
				EditorUtility.FocusProjectWindow ();	
				Selection.activeObject = asset;	

				if (File.Exists (assetFullPath)) {
					Loger.Info ($"UtilityAsset::CreateAsset Succeeded!!!(File:{assetFullPath})");
					// 读取并返回创建对象实例
					objRet = AssetDatabase.LoadAssetAtPath<T> (assetFullPath);
				} else {
					Loger.Error ($"UtilityAsset::CreateAsset Failed!!!(File:{assetFullPath})");
				}
			}
			catch(UnityException exp) {
				Loger.Fatal ($"UtilityAsset::CreateAsset()::Failed!!! DetailInfo " +
				             $"ClassName:{typeof(T)} \n AssetFile:{(assetFullPath ?? "null")} \n Error:{exp.Message}");
			}

#endif

			return objRet != null ? objRet : default(T);
		}

		/// <summary>
		/// 取得Asset文件数据保存路径.
		/// </summary>
		/// <returns>Asset文件数据保存路径.</returns>
		private static string GetAssetDataDir() {
			return AssetDataDir;
		}

		/// <summary>
		/// 取得Asset文件目录路径.
		/// </summary>
		/// <returns>取得Asset文件目录路径.</returns>
		/// <param name="iDirPath">Asset存放目录文件（不指定：当前选定对象所在目录）.</param>
		public static string GetAssetFileDir (string iDirPath = null) {
			var dirTmp = iDirPath;
			if (string.IsNullOrEmpty(dirTmp) != true) return dirTmp;
			dirTmp = GetAssetDataDir ();
			dirTmp = $"{DataPath}/{dirTmp}";
			return dirTmp;
		}

		/// <summary>
		/// 取得Asset文件路径.
		/// </summary>
		/// <returns>Asset文件路径.</returns>
		/// <param name="iDirPath">Asset存放目录文件（若不指定：当前选定对象所在目录）.</param>
		/// <param name="iFileName">文件名（若不指定：指定读取Asset文件绑定数据类型名）.</param>
		/// <typeparam name="T">指定读取Asset文件绑定数据类.</typeparam>
		public static string GetAssetFilePath<T> (
			string iDirPath = null, string iFileName = null)  where T : Object {
		
			// 目录
			var dirTmp = GetAssetFileDir(iDirPath);
				
			// 文件名指定
			var strTmp = string.IsNullOrEmpty(iFileName) ? typeof(T).ToString() : iFileName;
			// 因为有可能存在空间名（如：common.classA），所以截取最后一个(.)号开始的字符创
			var className = strTmp.Substring (strTmp.LastIndexOf (".", StringComparison.Ordinal) + 1);
			var assetFullPath = $"{dirTmp}/{className}.asset";

			const string key = "/Resources/";
			if (!assetFullPath.Contains(key)) return assetFullPath;
			var startIndex = assetFullPath.IndexOf(key, StringComparison.Ordinal);
			var pathTmp = assetFullPath.Substring (startIndex + 1);

			var endIndex = pathTmp.LastIndexOf (".", StringComparison.Ordinal);
			pathTmp = pathTmp.Substring (0, endIndex);
			assetFullPath = pathTmp;
			return assetFullPath;
		}

		/// <summary>
		/// 取得导入/导出Asset文件为Json文件用的路径.
		/// </summary>
		/// <returns>Json文件路径.</returns>
		/// <param name="iDirPath">Asset存放目录文件（若指定：当前选定对象所在目录）.</param>
		/// <param name="iFileName">文件名（若不指定：指定读取Asset文件绑定数据类型名）.</param>
		/// <typeparam name="T">指定读取Asset文件绑定类.</typeparam>
		public static string GetJsonFilePath<T> (
			string iDirPath = null, string iFileName = null) {
			var path = iDirPath;
			if(string.IsNullOrEmpty(path)) {
				path = GetAssetFileDir(path);
				path = $"{path}/Json";
			}

			// 文件名指定
			var strTmp = string.IsNullOrEmpty(iFileName) ? typeof(T).ToString() : iFileName;
			// 因为有可能存在空间名（如：common.classA），所以截取最后一个(.)号开始的字符创
			var className = strTmp.Substring (strTmp.LastIndexOf (".", StringComparison.Ordinal) + 1);

			return $"{path}/{className}.json";
		}

		/// <summary>
		/// 读取打包资源配置信息.
		/// </summary>
		/// <returns>打包资源配置信息.</returns>
		/// <param name="iDirPath">Asset存放目录文件（不指定：当前选定对象所在目录）.</param>
		public static T ReadSetting<T>(string iDirPath = null) where T : ScriptableObject {

			var objRet = default(T);
			string path = null;

			try {

				path = GetAssetFilePath<T>(iDirPath);
				if(string.IsNullOrEmpty(path)) {
					Loger.Error ($"UtilityAsset::GetAssetFilePath():Failed!!!(Dir:{(string.IsNullOrEmpty(iDirPath) ? "null" : iDirPath)})"); 
					return null;
				}
				Loger.Info ($"UtilityAsset::ReadSetting:{path}");
				objRet = LoadAssetFile<T>(path);

				SetAssetDirty (objRet);

			}
			catch(DirectoryNotFoundException exp)
			{
				Loger.Fatal ($"UtilityAsset::ReadSetting()::Failed!!! DetailInfo ClassName:{typeof(T)} \n " +
				             $"AssetFile:{(path ?? "null")} \n Error:{exp.Message}");
			}

			if (default(T) != objRet) return objRet;
			objRet = CreateAsset<T> (iDirPath);
			AssetsRefresh();
			SetAssetDirty (objRet);

			return objRet;
		}

		/// <summary>
		/// 删除文件.
		/// </summary>
		/// <param name="iDirPath">Asset存放目录文件（不指定：当前选定对象所在目录）.</param>
		/// <typeparam name="T">指定读取Asset文件绑定类.</typeparam>
		public static void DeleteFile<T> (string iDirPath = null) where T : JsonDataBase {

			var jsonFilePath = GetJsonFilePath<T> (iDirPath);
			if (File.Exists (jsonFilePath)) {
				File.Delete (jsonFilePath);
			}
			AssetsRefresh ();
		}

		/// <summary>
		/// 加载(*.asset)文件.
		/// * 加载优先顺序
		/// 1）编辑器模式加载
		/// 2）Resource下的资源
		/// </summary>
		/// <returns>加载文件对象.</returns>
		/// <param name="iPath">路径.</param>
		/// <typeparam name="T">加载对象类型.</typeparam>
		public static T LoadAssetFile<T>(string iPath) where T : Object {
			
			// 1）编辑器模式加载
#if UNITY_EDITOR
			var objRet = AssetDatabase.LoadAssetAtPath<T>(iPath);
			if(objRet != default(T)) {
				return objRet;
			}
			Loger.Warning($"UtilityAsset::LoadAssetFile() Failed!!! AssetPath : {iPath}");
#endif
			// 2）Resource下的资源
			// 若上述找不到，则在从资源文件夹中加载
			const string key = "Resources/";
			if (iPath.Contains(key) != true) return default(T);
			var startIndex = iPath.IndexOf(key, StringComparison.Ordinal);
			startIndex += key.Length;
			var pathTmp = iPath.Substring (startIndex);

			var endIndex = pathTmp.LastIndexOf (".", StringComparison.Ordinal);
			if (-1 != endIndex) {
				pathTmp = pathTmp.Substring (0, endIndex);
			}
			return Resources.Load (pathTmp) as T;

		}

		/// <summary>
		/// 从JSON文件，导入打包配置数据.
		/// </summary>
		/// <param name="iIsFileExist">文件是否存在标志位.</param>
		/// <param name="iJsonFileDir">导出Json文件目录.</param>
		/// <param name="iFileName">文件名（若不指定：指定读取Asset文件绑定数据类型名）.</param>
		/// <typeparam name="T">指定读取Asset文件数据类.</typeparam>
		public static T ImportDataByDir<T>(
			out bool iIsFileExist, string iJsonFileDir, string iFileName = null) 
			where T : JsonDataBase, new() {

			iIsFileExist = false;
			var jsonFilePath = GetJsonFilePath<T>(iJsonFileDir, iFileName);
			if (false == string.IsNullOrEmpty (jsonFilePath)) {
				bool isFileExist;
				return ImportDataByPath<T> (out isFileExist, jsonFilePath);
			}
			AssetsRefresh ();
			return default(T);
		}

		/// <summary>
		/// 从JSON文件，导入打包配置数据(文件必须存在).
		/// </summary>
		/// <param name="iIsFileExist">文件存在标识位。</param>
		/// <param name="iJsonFilePath">导入文件路径（若不存在该文件，则自动创建该文件）</param>
		/// <typeparam name="T">指定读取Asset文件数据类.</typeparam>
		public static T ImportDataByPath<T>(out bool iIsFileExist, string iJsonFilePath) 
			where T : JsonDataBase, new() {

			var objRet = default(T);
			iIsFileExist = false;
			try {
				// 优先加载下载资源包中的信息
				var _data = DataLoader.Load<TextAsset>(iJsonFilePath);
				string jsonString;
				if(null != _data) {
					// 读取文件
					jsonString = _data.text;
				} else {
					// 若已经有文件不存在
					if(File.Exists(iJsonFilePath) == false) {
						Loger.Warning($"UtilityAsset::ImportDataByPath():File not exist!!![File:{iJsonFilePath}]");
						return default(T);
					}
					iIsFileExist = true;
					jsonString = File.ReadAllText(iJsonFilePath);
				}
				if(false == string.IsNullOrEmpty(jsonString)) {
					objRet = UtilsJson<T>.ConvertFromJsonString(jsonString); 
					Loger.Info ($"UtilityAsset::ImportDataByPath(). <- AssetPath:{iJsonFilePath}");
					Loger.Info ($"UtilityAsset::ImportDataByPath. <- Data:{jsonString}");
				}
			}
			catch (Exception exp) {
				Loger.Fatal ($"UtilityAsset::ImportDataByPath()::Failed!!! \n " +
				             $"ClassName:{typeof(T)} \n AssetFile:{(iJsonFilePath ?? "null")} \n " +
				             $"Exception:{exp.Message} \n StackTrace:{exp.StackTrace}");
				objRet = default(T);
			}

			AssetsRefresh ();
			return objRet;
		}

		/// <summary>
		/// 导出成JSON文件.
		/// </summary>
		/// <returns>导出路径.</returns>
		/// <param name="iInstance">欲导出实例对象.</param>
		/// <param name="iJsonFileDir">导出Json文件目录.</param>
		/// <param name="iFileName">文件名（若不指定：指定读取Asset文件绑定数据类型名）.</param>
		/// <typeparam name="T">指定读取Asset文件绑定类.</typeparam>
		public static string ExportData<T>(T iInstance, string iJsonFileDir = null, string iFileName = null) 
			where T : JsonDataBase, new() {

			string jsonFilePath = null;
			try {

				// 检测导出文件夹
				if(false == UtilsTools.CheckAndCreateDirByFullDir(iJsonFileDir)) {
					return iJsonFileDir;
				}

				jsonFilePath = GetJsonFilePath<T>(iJsonFileDir, iFileName);
				// 若已经有文件存在，则强制删除
				if (File.Exists (jsonFilePath)) {
					File.Delete (jsonFilePath);
				}

				// 导出JSON文件
				var jsonString = UtilsJson<T>.ConvertToJsonString(iInstance);
				File.WriteAllText (jsonFilePath, jsonString);

				Loger.Info ($"UtilityAsset::ExportData(). -> AssetPath:{jsonFilePath}");
				Loger.Info ($"UtilityAsset::ExportData(). -> Data:{jsonString}");
			}
			catch (Exception exp) {
				Loger.Fatal ($"UtilityAsset::ExportData()::Failed!!! \n " +
				             $"ClassName:{typeof(T)} \n AssetFile:{jsonFilePath ?? "null"} \n " +
				             $"Exception:{exp.Message} \n StackTrace:{exp.StackTrace}");
			}

			AssetsRefresh();

			if (File.Exists (jsonFilePath)) {
				Loger.Info ($"UtilityAsset::ExportData(). -> {jsonFilePath}");
			}
			return jsonFilePath;
		}
	}
}
