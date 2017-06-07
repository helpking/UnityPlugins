using UnityEngine;
using UnityEditor;
using System.IO; 
using LitJson;
using BuildSystem;
using Common;

namespace AssetBundles {
	
	/// <summary>
	/// Utility asset.
	/// </summary>
	public class UtilityAsset {	

		/// <summary>
		/// asset文件数据路径.
		/// </summary>
		private const string _assetDataDir = "Resources/Conf";

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
		//	创建Asset.	
		/// </summary>	
		/// <param name="iDirPath">创建目录.</param>
		public static T CreateAsset<T> (string iDirPath = null) where T : AssetBase {	

			T objRet = default(T);

#if UNITY_EDITOR

			string assetFullPath = null;
			try {

				assetFullPath = GetAssetFilePath<T> (iDirPath);
				if(assetFullPath.StartsWith("Resources/") == true) {
					assetFullPath = string.Format("{0}/{1}", Application.dataPath, assetFullPath);
				}
				if(assetFullPath.EndsWith(".asset") == false) {
					assetFullPath = string.Format("{0}.asset", assetFullPath);
				}
//				assetFullPath = AssetDatabase.GenerateUniqueAssetPath (assetFullPath);
				if(File.Exists(assetFullPath) == true) {
					File.Delete(assetFullPath);
				}

				T asset = ScriptableObject.CreateInstance<T> (); 	
				AssetDatabase.CreateAsset (asset, assetFullPath); 	
				AssetsRefresh();	
				EditorUtility.FocusProjectWindow ();	
				Selection.activeObject = asset;	

				if (File.Exists (assetFullPath) == true) {
					BuildLogger.LogMessage ("Asset File Create Successed!!!(File:{0})",
						assetFullPath);
					// 读取并返回创建对象实例
					objRet = AssetDatabase.LoadAssetAtPath<T> (assetFullPath);
				} else {
					BuildLogger.LogError ("Asset File Create Failed!!!(File:{0})",
						assetFullPath);
				}
			}
			catch(UnityException exp) {
				BuildLogger.LogException ("[CreateAsset Failed] DetailInfo ClassName:{0} \n AssetFile:{1} \n Error:{2}",
					typeof(T).ToString (), 
					(assetFullPath == null) ? "null" : assetFullPath, 
					exp.Message);
			}

#endif

			if (objRet != null) {
				return objRet;
			} else {
				return default(T);
			}
		}

		/// <summary>
		/// 取得Asset文件数据保存路径.
		/// </summary>
		/// <returns>Asset文件数据保存路径.</returns>
		private static string GetAssetDataDir() {
			return _assetDataDir;
		}

		/// <summary>
		/// 取得Asset文件路径.
		/// </summary>
		/// <returns>Asset文件路径.</returns>
		/// <param name="iDirPath">Asset存放目录文件（不指定：当前选定对象所在目录）.</param>
		/// <typeparam name="T">指定读取Asset文件绑定类.</typeparam>
		public static string GetAssetFilePath<T> (string iDirPath = null) where T : AssetBase {
		
			string dirTmp = iDirPath;
			if(string.IsNullOrEmpty(dirTmp) == true) {
				dirTmp = GetAssetDataDir ();
				dirTmp = string.Format ("{0}/{1}", Application.dataPath, dirTmp);
			}
				
			// 文件名指定
			string strTmp = typeof(T).ToString();
			// 因为有可能存在空间名（如：common.classA），所以截取最后一个(.)号开始的字符创
			string className = strTmp.Substring (strTmp.LastIndexOf (".") + 1);
			string assetFullPath = string.Format("{0}/{1}.asset", dirTmp, className);

			const string _key = "/Resources/";
			if (assetFullPath.Contains (_key) == true) {

				int startIndex = assetFullPath.IndexOf(_key);
				string pathTmp = assetFullPath.Substring (startIndex + 1);

				int endIndex = pathTmp.LastIndexOf (".");
				pathTmp = pathTmp.Substring (0, endIndex);
				assetFullPath = pathTmp;
			}
			return assetFullPath;
		}

		/// <summary>
		/// 取得导入/导出Asset文件为Json文件用的路径.
		/// </summary>
		/// <returns>Json文件路径.</returns>
		/// <param name="iDirPath">Asset存放目录文件（不指定：当前选定对象所在目录）.</param>
		/// <typeparam name="T">指定读取Asset文件绑定类.</typeparam>
		public static string GetJsonFilePath<T> (string iDirPath = null) where T : AssetBase {

			string path = iDirPath;
			if(string.IsNullOrEmpty(path) == true) {
				path = GetAssetDataDir ();	
				path = string.Format ("{0}/{1}/Json", Application.dataPath, path);
			}

			// 文件名指定
			string strTmp = typeof(T).ToString();
			// 因为有可能存在空间名（如：common.classA），所以截取最后一个(.)号开始的字符创
			string className = strTmp.Substring (strTmp.LastIndexOf (".") + 1);
//			string jsonFullPath = AssetDatabase.GenerateUniqueAssetPath (
//				string.Format("{0}/{1}.json",
//					path, className));

			return string.Format("{0}/{1}.json", path, className);
		}

		/// <summary>
		/// 读取打包资源配置信息.
		/// </summary>
		/// <returns>打包资源配置信息.</returns>
		/// <param name="iDirPath">Asset存放目录文件（不指定：当前选定对象所在目录）.</param>
		public static T Read<T>(string iDirPath = null) where T : AssetBase {

			T objRet = null;
			string path = null;

			try {

				path = GetAssetFilePath<T>(iDirPath);

				BuildLogger.LogMessage ("DownloadConf Load:{0}", path);
				objRet = UtilityAsset.LoadAssetFile<T>(path);

				SetAssetDirty (objRet);

			}
			catch(System.IO.DirectoryNotFoundException exp)
			{
				BuildLogger.LogException ("[Read Failed] DetailInfo ClassName:{0} \n AssetFile:{1} \n Error:{2}",
					typeof(T).ToString (), 
					(path == null) ? "null" : path, 
					exp.Message);
			}




			if(objRet == null)
			{
				objRet = UtilityAsset.CreateAsset<T> (iDirPath);
				objRet.ImportFromJsonFile ();
				SetAssetDirty (objRet);
			}
			AssetsRefresh();

			return objRet;
		}

		/// <summary>
		/// 清空.
		/// </summary>
		/// <param name="iDirPath">Asset存放目录文件（不指定：当前选定对象所在目录）.</param>
		/// <param name="iClearFile">文件清空标志位.</param>
		/// <typeparam name="T">指定读取Asset文件绑定类.</typeparam>
		public static void Clear<T> (string iDirPath = null, bool iClearFile = false) where T : AssetBase {

			string jsonFilePath = GetJsonFilePath<T> (iDirPath);
			if ( (iClearFile == true) && (File.Exists (jsonFilePath) == true)) {
				File.Delete (jsonFilePath);
			}
		}

		/// <summary>
		/// 加载(*.asset)文件.
		/// </summary>
		/// <returns>The asset.</returns>
		/// <param name="iPath">I path.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public static T LoadAssetFile<T>(string iPath) where T : Object {

			T objRet = default(T);
			TextAsset textAsset = null;
#if UNITY_EDITOR
			objRet = AssetDatabase.LoadAssetAtPath<T>(iPath);
			if(objRet != null) {
				return objRet;
			} else {
				BuildLogger.LogWarning("[LoadAssetFile Failed] Path : {0}", iPath);
			}

#endif

			// 若上述找不到，则在从资源文件夹中加载
			const string _key = "Resources/";
			if ((textAsset == null) && (iPath.Contains (_key) == true)) {
				
				int startIndex = iPath.IndexOf(_key);
				startIndex += _key.Length;
				string pathTmp = iPath.Substring (startIndex);

				int endIndex = pathTmp.LastIndexOf (".");
				if (-1 != endIndex) {
					pathTmp = pathTmp.Substring (0, endIndex);
				}
				Object objTemp = Resources.Load (pathTmp);
				objRet = objTemp as T;
				return objRet;
			}

			return default(T);
		}

		/// <summary>
		/// 从JSON文件，导入打包配置信息.
		/// </summary>
		/// <param name="iJsonFileDir">导出Json文件目录.</param>
		/// <typeparam name="T">指定读取Asset文件绑定类.</typeparam>
		public static T ImportFromJsonFile<T>(string iJsonFileDir = null) where T : AssetBase {

			T objRet = default(T);
			string jsonFilePath = null;

			try {

				jsonFilePath = GetJsonFilePath<T>(iJsonFileDir);
				// 若已经有文件不存在
				if(File.Exists(jsonFilePath) == false) {
					BuildLogger.LogWarning("The json file is not exist!!![File:{0}]",
						jsonFilePath);
					return default(T);
				}
					
				string jsonString = File.ReadAllText(jsonFilePath);
				if(string.IsNullOrEmpty(jsonString) == false) {
					objRet = JsonMapper.ToObject<T>(jsonString);
					BuildLogger.LogMessage ("[Import Success] <- {0}", jsonFilePath);
				}
			}
			catch (System.Exception exp) {
				BuildLogger.LogException ("[Import Failed] ClassName:{0}   \n AssetFile:{1}   \n Exception:{2}",
					typeof(T).ToString (), 
					(jsonFilePath == null) ? "null" : jsonFilePath, 
					exp.Message);
				objRet = default(T);
			}

			AssetsRefresh();

			return objRet;
		}

		/// <summary>
		/// 导出成JSON文件.
		/// </summary>
		/// <returns>导出路径.</returns>
		/// <param name="iInstance">欲导出实例对象.</param>
		/// <param name="iJsonFileDir">导出Json文件目录.</param>
		/// <typeparam name="T">指定读取Asset文件绑定类.</typeparam>
		public static string ExportToJsonFile<T>(T iInstance, string iJsonFileDir = null) where T : AssetBase {

			string jsonFilePath = null;
			try {

				jsonFilePath = GetJsonFilePath<T>(iJsonFileDir);
				// 若已经有文件存在，则强制删除
				if (File.Exists (jsonFilePath) == true) {
					File.Delete (jsonFilePath);
				}

				// 导出JSON文件
				string jsonString = JsonMapper.ToJson (iInstance);
				File.WriteAllText (jsonFilePath, jsonString);
			}
			catch (System.Exception exp) {
				BuildLogger.LogException ("[Export Failed] ClassName:{0}   \n AssetFile:{1}   \n Exception:{2}",
					typeof(T).ToString (), 
					(jsonFilePath == null) ? "null" : jsonFilePath, 
					exp.Message);
			}

			AssetsRefresh();

			BuildLogger.LogMessage ("[Export Success] -> {0}", jsonFilePath);
			return jsonFilePath;
		}
	}
}
