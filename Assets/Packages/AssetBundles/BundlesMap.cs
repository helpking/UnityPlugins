using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using Packages.Common.Base;
using Packages.Utils;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Packages.AssetBundles {

	/// <summary>
	/// T bundle type.
	/// </summary>
	public enum BundleType {
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
		[FormerlySerializedAs("ID")] [SerializeField]
		public string id;

		/// <summary>
		/// Bundle类型.
		/// </summary>
		[FormerlySerializedAs("Type")] [SerializeField]
		public BundleType type = BundleType.Normal;

		/// <summary>
		/// 路径.
		/// </summary>
		[FormerlySerializedAs("AssetPath")] [SerializeField]
		public string path;

		/// <summary>
		/// 对象列表
		/// </summary>
		[FormerlySerializedAs("Targets")] [SerializeField]
		public List<string> targets = new List<string>();

		/// <summary>
		/// 添加文件.
		/// </summary>
		/// <param name="iFilePath">文件路径.</param>
		public void AddFile(string iFilePath) {
			if (string.IsNullOrEmpty (iFilePath)) {
				return;
			}
			if (targets == null) {
				targets = new List<string>();
			}
			foreach (var loop in targets) {
				// 已经存在
				if (loop.Equals (iFilePath)) {
					return;
				}
			}
			targets.Add (iFilePath);
		}

		/// <summary>
		/// 移除忽略文件.
		/// </summary>
		/// <param name="iFilePath">文件路径.</param>
		public void RemoveIgnorFile(string iFilePath) {
			if (string.IsNullOrEmpty (iFilePath)) {
				return;
			}
			if (targets == null || targets.Count <= 0) {
				return;
			}
			foreach (var loop in targets) {
				if (!loop.Equals(iFilePath)) continue;
				targets.Remove (loop);
				break;
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
		public string BundleId;

		/// <summary>
		/// 目标文件一览
		/// </summary>
		private readonly List<string> _targets = new List<string> ();

		/// <summary>
		/// 初始化
		/// </summary>
		/// <param name="iBundleId">BundleID</param>
		public void Init(string iBundleId) {
			BundleId = iBundleId;
		}

		public void AddTarget(string iScenePath) {
			if (string.IsNullOrEmpty (iScenePath)) {
				return;
			}
			var isExist = false;
			foreach (var loop in _targets) {
				if (iScenePath.Equals (loop) == false) {
					continue;
				}
				isExist = true;
			}
			if (isExist == false) {
				_targets.Add (iScenePath);
			}
		}
		
		/// <summary>
		/// 取得所有目标
		/// </summary>
		/// <returns>所有目标</returns>
		public string[] GetAllTargets() {
			if (_targets == null || _targets.Count <= 0) {
				return null;
			}
			return _targets.ToArray ();
		}
	}

	/// <summary>
	/// Bundles map data.
	/// </summary>
	[System.Serializable]
	public class BundlesMapData : JsonDataBase<BundlesMapData> {

		/// <summary>
		/// 依赖关系列表.
		/// </summary>
		public List<BundleMap> maps = new List<BundleMap> ();

		/// <summary>
		/// 清空.
		/// </summary>
		public override void Clear ()
		{
			maps.Clear ();
		}
	}

	/// <summary>
	/// 资源包地图.
	/// </summary>
	[System.Serializable]
	public class BundlesMap : AssetReadOnlyBase<BundlesMap, BundlesMapData> {

		/// <summary>
		/// 依赖关系列表.
		/// </summary>
		public List<BundleMap> Maps {
			get
			{
				return data?.maps;
			}
			set { 
				if (null != data) {
					data.maps = value;
				}
			}
		}
		
		/// <summary>
		/// 取得或者创建一个BundleMap对象.
		/// </summary>
		/// <returns>BundleMap对象.</returns>
		/// <param name="iBundleId">BundleId.</param>
		public static BundleMap GetOrCreateBundlesMap(string iBundleId) {
		
			BundleMap objRet;
			if (GetInstance().IsTargetExist (iBundleId, out objRet)) {
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
		public static string GetBundleId(string iPath)
		{
			var strResult = iPath;
			strResult = strResult.Replace("/", "_");
			strResult = strResult.Replace(".", "_");
			strResult = strResult.Replace(" ", "_");
			strResult = strResult.ToLower();
			return strResult;
		}

		/// <summary>
		/// 判断目标存不存在.
		/// </summary>
		/// <returns><c>true</c>, 存在, <c>false</c> 不存在.</returns>
		/// <param name="iTargetId">目标ID.</param>
		/// <param name="iTarget">目标.</param>
		private bool IsTargetExist(string iTargetId, out BundleMap iTarget) {
			iTarget = null;
			foreach (var loop in Maps) {
				if (loop.id.Equals(iTargetId) != true) continue;
				iTarget = loop;
				return true;
			}
			return false;
		}

		/// <summary>
		/// 目标是否改变了.
		/// </summary>
		/// <returns><c>true</c>, 已改变, <c>false</c> 未改变.</returns>
		/// <param name="iNewTarget">新目标.</param>
		/// <param name="iOldTarget">旧目标</param>
		private bool IsTargetChanged(BundleMap iNewTarget, ref BundleMap iOldTarget) {
			iOldTarget = null;
			var isExist = IsTargetExist (iNewTarget.id, out iOldTarget);
			if (isExist){
				
				// Type
				if(iOldTarget.type != iNewTarget.type) {
					return true;
				}
 
				// AssetPath
				if(false == iOldTarget.path.Equals(iNewTarget.path)) {
					return true;
				}

				// Targets
				if(iOldTarget.targets.Count != iNewTarget.targets.Count) {
					return true;
				}
				foreach (var newPath in iNewTarget.targets) {
					var isExistTmp = false;
					foreach (var path in iOldTarget.targets) {
						if (!newPath.Equals(path)) continue;
						isExistTmp = true;
						break;
					}
					if (false == isExistTmp) {
						return true;
					}
				}
			} else {
				return true;
			}
			return false;
		}

		/// <summary>
		/// 合并目标.
		/// </summary>
		/// <param name="iNewTarget">I new ProjectName.</param>
		private void MergeTarget(BundleMap iNewTarget) {
			BundleMap oldTarget = null;
			var isChanged = IsTargetChanged (iNewTarget, ref oldTarget);
			if (true != isChanged) return;
			if (null == oldTarget) {
				Maps.Add (iNewTarget);
			} else {
				oldTarget.type = iNewTarget.type;
				oldTarget.path = iNewTarget.path;

				foreach (var newPath in iNewTarget.targets) {
					var isExist = false;
					foreach (var path in oldTarget.targets) {
						if (true != newPath.Equals(path)) continue;
						isExist = true;
						break;
					}
					if (false == isExist) {
						oldTarget.targets.Add(newPath);
					}
				}
			}
		}
			
#if UNITY_EDITOR

		/// <summary>
		/// 取得所有打包对象（一般的AssetBundle）.
		/// </summary>
		/// <returns>取得所有对象.</returns>
		public AssetBundleBuild[] GetAllNormalBundleTargets() {

			var targets = Maps
				.Where (iO => BundleType.Normal == iO.type)
				.OrderBy (iO => iO.id)
				.ToArray ();
			if (targets.Length <= 0) {
				return null;
			}
			var buildMap = new AssetBundleBuild[targets.Length];
			for(var _ = 0; _ < targets.Length; _++) {
				buildMap [_].assetBundleName = GetBundleFullName(targets[_].id);
				buildMap [_].assetNames = targets[_].targets.ToArray();
			}
			return buildMap;
		}

		/// <summary>
		/// 取得所有打包对象（scene的AssetBundle）.
		/// </summary>
		/// <returns>取得所有对象.</returns>
		public List<SceneBundleInfo> GetAllSceneBundleTargets() {

			var targets = Maps
				.Where (iO => BundleType.Scene == iO.type)
				.OrderBy (iO => iO.id)
				.ToArray ();
			if (targets.Length <= 0) {
				return null;
			}

			var scenesInfo = new List<SceneBundleInfo>();
			foreach (var loop in targets) {
				var sceneInfo = new SceneBundleInfo ();
				sceneInfo.Init (loop.id);
				foreach (var scene in loop.targets) {
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
			var strRet = iBundleId;
			var fileSuffix = BundlesResult.GetInstance ().FileSuffix;
			if (string.IsNullOrEmpty (fileSuffix) == false) {
				strRet = $"{strRet}.{fileSuffix}";
			}
			return strRet;
		}

		/// <summary>
		/// 取得Bundle manifest 全名（包含后缀）.
		/// </summary>
		/// <returns>Bundle manifest 全名（包含后缀）.</returns>
		/// <param name="iBundleId">Bundle ID.</param>
		public string GetBundleManifestFullName(string iBundleId) {
			var strRet = GetBundleFullName(iBundleId);
			return string.IsNullOrEmpty (strRet) == false ? $"{strRet}.manifest" : null;
		}

		/// <summary>
		/// 更新&生成上传列表信息.
		/// </summary>
		/// <param name="iBundleType">Bundle Type.</param>
		/// <param name="iHashCodes">HashCode列表（Unity3d打包生成）.</param>
		public void PushBundleResult(BundleType iBundleType, Dictionary<string, string> iHashCodes = null) {

			var list = BundlesResult.GetInstance ();
			if (list == null) {
				return;
			}

			list.AppVersion = BundlesResult.GetInstance ().AppVersion;

			// MainManifest
			if (BundleType.Normal == iBundleType) {
				list.AddMainManifestAssetsTarget ();
			}

			// 遍历Bundles
			foreach (var loop in Maps) {

				if (loop.type != iBundleType) {
					continue;
				}

				string hashCode = null;
				if (iHashCodes != null) {
					hashCode = GetHashCodeOfBundle (iHashCodes, loop.id);
				}

				// Bundle
				list.AddTarget (loop, BundleFileType.Bundle, hashCode);
				if (list.ManifestUpload && BundleType.Scene != loop.type) {
					// Manifest(Normal)
					list.AddTarget (loop, BundleFileType.NormalManifest);
				}

			}

		}

		/// <summary>
		/// 取得Bundle的HashCode.
		/// </summary>
		/// <returns>Bundle的HashCode.</returns>
		/// <param name="iHashCodes">HashCode列表.</param>
		/// <param name="iBundleId">BundleId.</param>
		private static string GetHashCodeOfBundle(Dictionary<string, string> iHashCodes, string iBundleId) {
			if (iHashCodes == null ||
			   iHashCodes.Count <= 0) {
				return null;
			}
			foreach (var it in iHashCodes) {
				if (it.Key.Equals (iBundleId) == false) {
					continue;
				}
				return it.Value;
			}
			return null;
		}

#region Implement

		/// <summary>
		/// 应用数据.
		/// </summary>
		/// <param name="iData">数据.</param>
		/// <param name="iForceClear">强制清空.</param>
		protected override void ApplyData(BundlesMapData iData, bool iForceClear = true) {
			if (null == iData) {
				return;
			}

			// 清空
			if(iForceClear) {
				Clear ();
			}
				
			// 添加资源信息
			foreach(BundleMap loop in iData.maps) {
				// 合并目标
				MergeTarget (loop);
			}

			UtilsAsset.SetAssetDirty (this);

		}
			
#endregion
	}
}
