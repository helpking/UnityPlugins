using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using LitJson;
using BuildSystem;
using Upload;
using Common;

namespace AssetBundles {

	/// <summary>
	/// T bundle type.
	/// </summary>
	public enum TBundleType {
		/// <summary>
		/// 一般.
		/// </summary>
		Normal,
		/// <summary>
		/// 场景.
		/// </summary>
		Scene
	}

	/// <summary>
	/// 资源包信息.
	/// </summary>
	[System.Serializable]
	public class BundleMap
	{
		/// <summary>
		/// ID.
		/// </summary>
		[SerializeField]public string ID = null;

		/// <summary>
		/// Bundle类型.
		/// </summary>
		[SerializeField]public TBundleType Type = TBundleType.Normal;

		/// <summary>
		/// 路径.
		/// </summary>
		[SerializeField]public string Path = null;

		/// <summary>
		/// 对象列表
		/// </summary>
		[SerializeField]public List<string> Targets = new List<string>();

		/// <summary>
		/// 添加文件.
		/// </summary>
		/// <param name="iFilePath">文件路径.</param>
		public void AddFile(string iFilePath) {
			if (string.IsNullOrEmpty (iFilePath) == true) {
				return;
			}
			if (Targets == null) {
				Targets = new List<string>();
			}
			foreach (string loop in Targets) {
				// 已经存在
				if (loop.Equals (iFilePath) == true) {
					return;
				}
			}
			this.Targets.Add (iFilePath);
		}

		/// <summary>
		/// 移除忽略文件.
		/// </summary>
		/// <param name="iFilePath">文件路径.</param>
		public void RemoveIgnorFile(string iFilePath) {
			if (string.IsNullOrEmpty (iFilePath) == true) {
				return;
			}
			if ((Targets == null) || (Targets.Count <= 0)) {
				return;
			}
			foreach (string loop in Targets) {
				if (loop.Equals (iFilePath) == true) {
					Targets.Remove (loop);
					break;
				}
			}
		}
	}

	/// <summary>
	/// 场景包信息.
	/// </summary>
	public class SceneBundleInfo {

		/// <summary>
		/// BundleId.
		/// </summary>
		public string BundleId = null;

		/// <summary>
		/// 目标文件一览
		/// </summary>
		public List<string> Targets = new List<string> ();

		public void init(string iBundleId) {
			this.BundleId = iBundleId;
		}

		public void AddTarget(string iScenePath) {
			if (string.IsNullOrEmpty (iScenePath) == true) {
				return;
			}
			bool isExist = false;
			foreach (string loop in this.Targets) {
				if (iScenePath.Equals (loop) == false) {
					continue;
				}
				isExist = true;
			}
			if (isExist == false) {
				this.Targets.Add (iScenePath);
			}
		}

		public string[] GetAllTargets() {
			if ((this.Targets == null) || (Targets.Count <= 0)) {
				return null;
			}
			return this.Targets.ToArray ();
		}


	}

	/// <summary>
	/// 资源包地图.
	/// </summary>
	[System.Serializable]
	public class BundlesMap : AssetBase {

		/// <summary>
		/// The bundles.
		/// </summary>
		[SerializeField]public List<BundleMap> Maps = new List<BundleMap> ();

		/// <summary>
		/// 实例.
		/// </summary>
		private static BundlesMap _instance = null;

		/// <summary>
		/// 取得实例.
		/// </summary>
		/// <returns>实例.</returns>
		public static BundlesMap GetInstance() {
			if (_instance == null) {
				_instance = UtilityAsset.Read<BundlesMap>();
				if (_instance == null) {
					Debug.LogError ("BundlesMap GetInstance Failed!!!");
					return null;
				} 
				_instance.Init ();
			}
			return _instance;
		}

		/// <summary>
		/// 取得或者创建一个BundleMap.
		/// </summary>
		/// <returns>BundleMap.</returns>
		/// <param name="iBundleId">BundleId.</param>
		public BundleMap GetOrCreateBundlesMap(string iBundleId) {
		
			BundleMap objRet = null;
			if (this.isTargetExist (iBundleId, out objRet) == true) {
				if (objRet == null) {
					objRet = new BundleMap ();
				}
			} else {
				objRet = new BundleMap ();
			}
			return objRet;
		}

		/// <summary>
		/// 取得bundle ID.
		/// </summary>
		/// <returns>bundle名.</returns>
		/// <param name="iPath">路径.</param>
		public static string GetBundleID(string iPath)
		{
			string strResult = iPath;
			strResult = strResult.Replace("/", "_");
			strResult = strResult.Replace(".", "_");
			strResult = strResult.Replace(" ", "_");
			strResult = strResult.ToLower();
			return strResult;
		}

		/// <summary>
		/// 追加Bundle对象.
		/// </summary>
		/// <param name="iTarget">Bundle对象.</param>
		public void AddTarget(BundleMap iTarget) {

			BundleMap target = null;
			// 存在
			if (isTargetExist (iTarget.ID, out target) == true) {
				if (target == null) {
					Debug.LogErrorFormat ("Target Add Failed!!!(target:{0})", iTarget.ID);
					return;
				}
				target.Path = iTarget.Path;
				target.Targets = iTarget.Targets;
			// 不存在
			} else {
				this.Maps.Add (iTarget);
			}
			UtilityAsset.SetAssetDirty (this);
		}

		/// <summary>
		/// 判断目标存不存在.
		/// </summary>
		/// <returns><c>true</c>, 存在, <c>false</c> 不存在.</returns>
		/// <param name="iTargetID">目标ID.</param>
		/// <param name="iTarget">目标.</param>
		private bool isTargetExist(string iTargetID, out BundleMap iTarget) {
			iTarget = null;
			foreach (BundleMap loop in this.Maps) {
				if (loop.ID.Equals (iTargetID) == true) {
					iTarget = loop;
					return true;
				}
			}
			return false;
		}

#if UNITY_EDITOR

		/// <summary>
		/// 取得所有打包对象（一般的AssetBundle）.
		/// </summary>
		/// <returns>取得所有对象.</returns>
		public AssetBundleBuild[] GetAllNormalBundleTargets() {

			BundleMap[] targets = this.Maps
				.Where (o => (TBundleType.Normal == o.Type))
				.OrderBy (o => o.ID)
				.ToArray ();
			if ((targets == null) || (targets.Length <= 0)) {
				return null;
			}
			AssetBundleBuild[] buildMap = new AssetBundleBuild[targets.Length];
			for(int i = 0; i < targets.Length; i++) {
				buildMap [i].assetBundleName = GetBundleFullName(targets[i].ID);
				buildMap [i].assetNames = targets[i].Targets.ToArray();
			}
			return buildMap;
		}

		/// <summary>
		/// 取得所有打包对象（scene的AssetBundle）.
		/// </summary>
		/// <returns>取得所有对象.</returns>
		public List<SceneBundleInfo> GetAllSceneBundleTargets() {

			BundleMap[] targets = this.Maps
				.Where (o => (TBundleType.Scene == o.Type))
				.OrderBy (o => o.ID)
				.ToArray ();
			if ((targets == null) || (targets.Length <= 0)) {
				return null;
			}

			List<SceneBundleInfo> scenesInfo = new List<SceneBundleInfo>();
			foreach (BundleMap loop in targets) {
				SceneBundleInfo sceneInfo = new SceneBundleInfo ();
				sceneInfo.init (loop.ID);
				foreach (string scene in loop.Targets) {
					sceneInfo.AddTarget (scene);
				}
				scenesInfo.Add (sceneInfo);
			}
			return scenesInfo;
		}

#endif
		/// <summary>
		/// 取得Bundle全名（包含后缀）.
		/// </summary>
		/// <returns>Bundle全名.</returns>
		/// <param name="iBundleId">Bundle ID.</param>
		public string GetBundleFullName(string iBundleId) {
			string strRet = iBundleId;
			string fileSuffix = BundlesConfig.GetInstance ().FileSuffix;
			if (string.IsNullOrEmpty (fileSuffix) == false) {
				strRet = string.Format ("{0}.{1}", 
					strRet, fileSuffix);
			}
			return strRet;
		}

		/// <summary>
		/// 取得Bundle manifest 全名（包含后缀）.
		/// </summary>
		/// <returns>Bundle manifest 全名（包含后缀）.</returns>
		/// <param name="iBundleId">Bundle ID.</param>
		public string GetBundleManifestFullName(string iBundleId) {
			string strRet = this.GetBundleFullName(iBundleId);
			if (string.IsNullOrEmpty (strRet) == false) {
				return string.Format ("{0}.manifest", 
					strRet);
			} else {
				return null;
			}
		}

		/// <summary>
		/// 更新&生成上传列表信息.
		/// </summary>
		/// <param name="iBundleType">Bundle Type.</param>
		/// <param name="iHashCodes">HashCode列表（Unity3d打包生成）.</param>
		public void UpdateUploadList(TBundleType iBundleType, Dictionary<string, string> iHashCodes = null) {

			UploadList list = UploadList.GetInstance ();
			if (list == null) {
				return;
			}

			// MainManifest
			if (TBundleType.Normal == iBundleType) {
				list.AddMainManifestAssetsTarget (TBuildMode.Debug);
				list.AddMainManifestAssetsTarget (TBuildMode.Release);
				list.AddMainManifestAssetsTarget (TBuildMode.Store);
			}

			// 遍历Bundles
			foreach (BundleMap loop in this.Maps) {

				if (loop.Type != iBundleType) {
					continue;
				}

				string hashCode = null;
				if (iHashCodes != null) {
					hashCode = this.GetHashCodeOfBundle (iHashCodes, loop.ID);
				}

				// Debug
				list.AddTarget (loop, TBuildMode.Debug, hashCode);
				list.AddTarget (loop, TBuildMode.Release, hashCode);
				list.AddTarget (loop, TBuildMode.Store, hashCode);

			}

		}

		/// <summary>
		/// 取得Bundle的HashCode.
		/// </summary>
		/// <returns>Bundle的HashCode.</returns>
		/// <param name="iHashCodes">HashCode列表.</param>
		/// <param name="iBundleId">BundleId.</param>
		private string GetHashCodeOfBundle(Dictionary<string, string> iHashCodes, string iBundleId) {
			if ((iHashCodes == null) ||
			   (iHashCodes.Count <= 0)) {
				return null;
			}
			foreach (KeyValuePair<string,string> it in iHashCodes) {
				if (it.Key.Equals (iBundleId) == false) {
					continue;
				}
				return it.Value;
			}
			return null;
		}

		/// <summary>
		/// 从JSON文件，导入打包配置信息.
		/// </summary>
		/// <param name="iImportDir">导入目录.</param>
		public void ImportFromJsonFile(string iImportDir) {	
			
			// 清空寄存信息
			this.Clear ();

			// 导入文件
			BundlesMap jsonData = UtilityAsset.ImportFromJsonFile<BundlesMap> (iImportDir);
			if (jsonData != null) {
				this.ApplyData (jsonData);
			}
		}

		#region Implement

		/// <summary>
		/// 初始化.
		/// </summary>
		public override void Init () {

//			string assetFullPath = UtilityAsset.GetAssetFilePath<BundlesMap> ();
//			// 将自身作为资源对象之一，加入列表
//			BundleMap bm = new BundleMap();
//			bm.ID = BundlesMap.GetBundleID(assetFullPath);
//			bm.Targets.Add(assetFullPath);
//			this.AddTarget (bm);
//
//			UtilityAsset.SetAssetDirty (this);
		}

		/// <summary>
		/// 应用数据.
		/// </summary>
		/// <param name="iData">数据.</param>
		protected override void ApplyData(AssetBase iData) {
			if (iData == null) {
				return;
			}

			BundlesMap data = iData as BundlesMap;
			if (data == null) {
				return;
			}

			// 清空
			this.Clear ();
				
			// 添加资源信息
			foreach(BundleMap loop in data.Maps) {
				this.AddTarget (loop);
			}

			UtilityAsset.SetAssetDirty (this);

		}

		/// <summary>
		/// 清空.
		/// </summary>
		public override void Clear() {

			UtilityAsset.Clear<BundlesMap> ();

			// 清空列表
			this.Maps.Clear ();
			UtilityAsset.SetAssetDirty (this);

		}

		/// <summary>
		/// 从JSON文件，导入打包配置信息.
		/// </summary>
		public override void ImportFromJsonFile() {			
			BundlesMap jsonData = UtilityAsset.ImportFromJsonFile<BundlesMap> ();
			if (jsonData != null) {
				this.ApplyData (jsonData);
			}
		}

		/// <summary>
		/// 导出成JSON文件.
		/// </summary>
		/// <returns>导出路径.</returns>
		public override string ExportToJsonFile() {
			return UtilityAsset.ExportToJsonFile<BundlesMap> (this);
		}
			
		#endregion
	}
}
