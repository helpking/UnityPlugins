using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using LitJson;
using BuildSystem;
using Common;

namespace AssetBundles {

	/// <summary>
	/// Bundle状态.
	/// </summary>
	public enum TBundleState {
		/// <summary>
		/// 无.
		/// </summary>
		None = 0x00000000,
		/// <summary>
		/// IOS需要导出.
		/// </summary>
		IosNeed = 0x00000001,
		/// <summary>
		/// IOS导出完成.
		/// </summary>
		IosCompleted = 0x00000002,
		/// <summary>
		/// 安卓需要导出.
		/// </summary>
		AndroidNeed = 0x00000010,
		/// <summary>
		/// 安卓导出完成.
		/// </summary>
		AndroidCompleted = 0x00000020,
		/// <summary>
		/// 所有都需要导出.
		/// </summary>
		AllNeed = (IosNeed | AndroidNeed),
		/// <summary>
		/// 所有都需要导出.
		/// </summary>
		AllCompleted = (IosCompleted | AndroidCompleted)
	}

	/// <summary>
	/// 打包模式.
	/// </summary>
	public enum BundleMode
	{
		/// <summary>
		/// 单一目录.
		/// </summary>
		OneDir = 1,
		/// <summary>
		/// 文件一对一.
		/// </summary>
		FileOneToOne = 2,
		/// <summary>
		/// 顶层目录一对一.
		/// </summary>
		TopDirOneToOne = 3,
		/// <summary>
		/// 场景一对一.
		/// </summary>
		SceneOneToOne = 4,
	}

	/// <summary>
	/// 打包资源资源信息
	/// </summary>
	[System.Serializable]
	public class BundleResource
	{
		/// <summary>
		/// 路径.
		/// </summary>
		public string Path = "";
		/// <summary>
		/// 模式.
		/// </summary>
		public BundleMode Mode = BundleMode.OneDir;
		/// <summary>
		/// 忽略列表.
		/// </summary>
		public List<string> IgnoreList = new List<string>();

		/// <summary>
		/// 需要打包标志位.
		/// </summary>
		public int State = (int)TBundleState.None;
	}

	/// <summary>
	/// 非资源信息.
	/// </summary>
	[System.Serializable]
	public class BundleUnResource
	{
		/// <summary>
		/// 路径.
		/// </summary>
		public string Path = "";
	}

	/// <summary>
	/// 资源打包配置信息.
	/// </summary>
	[System.Serializable]
	public class BundlesConfig : AssetBase {

		/// <summary>
		/// 文件后缀名.
		/// </summary>
		[SerializeField]public string FileSuffix = null;

		/// <summary>
		/// 资源列表.
		/// </summary>
		[SerializeField]public List<BundleResource> Resources = new List<BundleResource>();
		/// <summary>
		/// 非资源列表.
		/// </summary>
		[SerializeField]public List<BundleUnResource> UnResources = new List<BundleUnResource>();

		/// <summary>
		/// 实例.
		/// </summary>
		private static BundlesConfig _instance = null;

		/// <summary>
		/// 取得实例.
		/// </summary>
		/// <returns>实例.</returns>
		public static BundlesConfig GetInstance() {

			if (_instance == null) {
				_instance = UtilityAsset.Read<BundlesConfig>();
				if (_instance == null) {
					Debug.LogError ("BundlesConfig GetInstance Failed!!!");
					return null;
				} 
				_instance.Init ();
			}
			return _instance;
		}

		#region File

		#endregion

		#region Resource

		/// <summary>
		/// 添加资源信息.
		/// </summary>
		/// <param name="iResourceInfo">资源信息.</param>
		public BundleResource AddResource(BundleResource iResourceInfo) {
			if (iResourceInfo == null) {
				return null;
			}
			return AddResource (iResourceInfo.Path, iResourceInfo.Mode,
				(TBundleState)iResourceInfo.State, iResourceInfo.IgnoreList);
		}

		/// <summary>
		/// 添加资源信息.
		/// </summary>
		/// <param name="iResourePath">资源路径.</param>
		/// <param name="iMode">打包模式.</param>
		/// <param name="iSate">打包状态.</param>
		/// <param name="iIgnoreList">忽略列表.</param>
		public BundleResource AddResource(string iResourcePath,
			BundleMode iMode, TBundleState iSate = TBundleState.AllNeed, 
			List<string> iIgnoreList = null) {
		
			BundleResource br = null;
			// 不存在存在
			if (this.isResoureExist (iResourcePath, out br) == false) {
				br = new BundleResource();
				this.Resources.Add (br);
			}
			if (br != null) {
				br.Path = iResourcePath;
				br.Mode = iMode;
				// 设置状态为
				this.SetBundleState(br, iSate);
				if (iIgnoreList != null) {
					foreach (string loop in iIgnoreList) {
						br.IgnoreList.Add (loop);
					}
				}

				UtilityAsset.SetAssetDirty (this);

				Debug.LogFormat ("[AddResource Successed](Path:{0} Mode:{1} State:{2} IgnoreList:{3})",
					iResourcePath, iMode.ToString(), iSate.ToString(), 
					(iIgnoreList==null) ? "null" : iIgnoreList.ToString());
				
			} else {
				Debug.LogErrorFormat ("[Error]AddResource Failed!!!(Path:{0} Mode:{1} State:{2} IgnoreList:{3})",
					iResourcePath, iMode.ToString(), iSate.ToString(), 
					(iIgnoreList==null) ? "null" : iIgnoreList.ToString());
			}
			return br;
		}

		/// <summary>
		/// 添加忽略列表.
		/// </summary>
		/// <returns>资源信息.</returns>
		/// <param name="iResourePath">资源路径.</param>
		/// <param name="iIgnoreTarget">忽略对象.</param>
		public BundleResource AddIgnoreList(
			string iResourcePath, 
			string iIgnoreTarget)
		{

			BundleResource br = null;
			// 不存在存在
			if (this.isResoureExist (iResourcePath, out br) == false) {
				Debug.LogErrorFormat ("[Error]AddIgnoreList Failed!!!The Resource is not Exist!!!(Path:{0} IgnoreTarget:{1})",
					iResourcePath, iIgnoreTarget);
				return br;
			}

			if (br != null) {
				// 忽略列表变更，有必要重新导出资源，所以状态位重置
				this.SetBundleState(
					br, (TBundleState.IosNeed | TBundleState.AndroidNeed));

				bool isAlreadyExist = false;
				if (br.IgnoreList == null) {
					br.IgnoreList = new List<string> ();
				} else {
					foreach (string loop in br.IgnoreList) {
						if (loop.Equals (iIgnoreTarget) == true) {
							isAlreadyExist = true;
							break;
						}
					}
				}

				if (isAlreadyExist == false) { 
					br.IgnoreList.Add (iIgnoreTarget);
				}

				UtilityAsset.SetAssetDirty (this);

			} else {
				Debug.LogErrorFormat ("[Error]AddIgnoreList Failed!!!The Resource is not Exist!!!(Path:{0} IgnoreTarget:{1})",
					iResourcePath, iIgnoreTarget);
			}

			Debug.LogFormat ("AddIgnoreList successed!!{0} -> {1}",
				iIgnoreTarget, iResourcePath);

			return br;
		}

		/// <summary>
		/// 移除资源信息.
		/// </summary>
		/// <returns><c>true</c>, 移除成功, <c>false</c> 移除失败.</returns>
		/// <param name="iResourcePath">资源路径.</param>
		public bool RemoveResource(string iResourcePath) {
		
			BundleResource br = null;
			// 不存在存在
			if (this.isResoureExist (iResourcePath, out br) == true) {            
				if (br != null) {
					this.Resources.Remove (br);
					UtilityAsset.SetAssetDirty (this);

					Debug.LogFormat ("Resource Remove Successed!!(file:{0})", iResourcePath);
				} else {
					Debug.LogFormat ("The info of resource is null!!(file:{0})", iResourcePath);
				}
				return true;
			} else {
				Debug.LogFormat ("There is no info of resource!!(file:{0})", iResourcePath);
			}
			return false;
		}

		/// <summary>
		/// 清空资源列表.
		/// </summary>
		public void ClearResources() {
			if (this.Resources == null) {
				return;
			}
			this.Resources.Clear ();

			UtilityAsset.SetAssetDirty (this);
		}

		/// <summary>
		/// 清空指定资源对象的忽略列表.
		/// </summary>
		/// <param name="iTarget">目标.</param>
		/// <param name="iIgnoreInfo">忽略信息.</param>
		public void RemoveIgnoreInfo(string iTarget, string iIgnoreInfo) {
			if (this.Resources == null) {
				return;
			}
			BundleResource br = null;
			// 不存在存在
			if (this.isResoureExist (iTarget, out br) == false) {
				Debug.LogWarningFormat ("The target source is not exist!!!(TargetSource:{0})",
					iTarget);
				return;
			}
			if ((br != null) && (br.IgnoreList != null)) {
				bool isExistFlg = false;
				foreach (string ignore in br.IgnoreList) {
					if (ignore.Equals (iIgnoreInfo) == false) {
						continue;
					}
					br.IgnoreList.Remove (ignore);

					// 忽略列表变更，有必要重新导出资源，所以状态位重置
					this.SetBundleState(
						br, (TBundleState.IosNeed | TBundleState.AndroidNeed));
					
					UtilityAsset.SetAssetDirty (this);
					isExistFlg = true;
					Debug.LogFormat ("RemoveIgnoreInfo Successed!!!(Target:{0} IgnoreInfo:{1})",
						iTarget, iIgnoreInfo);
					break;
				}
				if (isExistFlg == false) {
					Debug.LogWarningFormat ("There is no ignore info in this target!!!(Target:{0} IgnoreInfo:{1})",
						iTarget, iIgnoreInfo);
				}
			}
		}

		/// <summary>
		/// 清空指定资源对象的忽略列表.
		/// <param name="iTarget">目标.</param>
		/// </summary>
		public void ClearAllIgnoreInfo(string iTarget) {
			if (this.Resources == null) {
				return;
			}
			BundleResource br = null;
			// 不存在存在
			if (this.isResoureExist (iTarget, out br) == false) {
				Debug.LogWarningFormat ("The target source is not exist!!!(Target:{0})",
					iTarget);
				return;
			}
			if ((br != null) && (br.IgnoreList != null)) {
				br.IgnoreList.Clear();
				UtilityAsset.SetAssetDirty (this);
				Debug.LogFormat ("ClearIgnoreList Successed!!!(Target:{0})",
					iTarget);
			}
		}

		/// <summary>
		/// 判断当前文件是否为指定目标的忽略文件.
		/// </summary>
		/// <returns><c>true</c>, 忽略文件, <c>false</c> 非忽略文件.</returns>
		/// <param name="iTarget">目标.</param>
		/// <param name="iCheckFile">检测文件.</param>
		public bool isIgnoreFile(BundleResource iTarget, string iCheckFile) {
			if ((iTarget == null) || 
				(iTarget.IgnoreList == null) || 
				(string.IsNullOrEmpty(iCheckFile) == true) ) {
				return false;
			}
			string checkFile = iCheckFile;
			checkFile = checkFile.Replace (".meta", "");
			foreach (string file in iTarget.IgnoreList) {
				if (file.Equals (checkFile) == true) {
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// 判断资源信息是否存在.
		/// </summary>
		/// <returns><c>true</c>, 存在, <c>false</c> 不存在.</returns>
		/// <param name="iSourcePath">资源路径.</param>
		/// <param name="iBr">资源信息.</param>
		private bool isResoureExist(string iResourcePath, out BundleResource iBr) {

			bool bolRet = false;
			iBr = null;
			foreach(BundleResource br in this.Resources)
			{
				if(br.Path == iResourcePath)
				{
					iBr = br;
					bolRet = true;
					break;
				}
			}

			return bolRet;
		}

		/// <summary>
		/// 检测iOS版本资源是否需要导出.
		/// </summary>
		/// <returns><c>true</c>, 需要, <c>false</c> 不需要.</returns>
		/// <param name="iBundle">资源打包信息.</param>
		public bool isIosNeedToBuild(BundleResource iBundle) {
		
			if (iBundle == null) {
				return false;
			}

			// iOS导出已经完成，则返回（不需要在导出）
			if (((int)TBundleState.IosCompleted & iBundle.State) == (int)TBundleState.IosCompleted) {
				return false;
			}

			// iOS导出已经完成，则返回（不需要在导出）
			if (((int)TBundleState.IosNeed & iBundle.State) == (int)TBundleState.IosNeed) {
				return true;
			}

			return false;
		}

		/// <summary>
		/// 检测Android版本资源是否需要导出.
		/// </summary>
		/// <returns><c>true</c>, 需要, <c>false</c> 不需要.</returns>
		/// <param name="iBundle">资源打包信息.</param>
		public bool isAndroidNeedToBuild(BundleResource iBundle) {

			if (iBundle == null) {
				return false;
			}

			// Android导出已经完成，则返回（不需要在导出）
			if (((int)TBundleState.AndroidCompleted & iBundle.State) == (int)TBundleState.AndroidCompleted) {
				return false;
			}

			// Android导出已经完成，则返回（不需要在导出）
			if (((int)TBundleState.AndroidNeed & iBundle.State) == (int)TBundleState.AndroidNeed) {
				return true;
			}

			return false;
		}

		/// <summary>
		/// 设定打包资源信息.
		/// </summary>
		/// <param name="iBundle">打包资源信息.</param>
		/// <param name="iState">状态.</param>
		public void SetBundleState(BundleResource iBundle, TBundleState iState) {

			int stateValue = (int)iState;
			// iOS需要导出位
			if(((int)TBundleState.IosNeed & stateValue) == (int)TBundleState.IosNeed) {

				// 设置iOS需要导出位
				iBundle.State |= (int)TBundleState.IosNeed;

				// 强制重置iOS完成标志位
				int intValue = ~((int)TBundleState.IosCompleted);
				stateValue &= intValue;
				iBundle.State &= intValue;
			}

			// iOS导出完成位
			if(((int)TBundleState.IosCompleted & stateValue) == (int)TBundleState.IosCompleted) {

				// 强制重置iOS需要导出位
				int intValue = ~((int)TBundleState.IosNeed);
				stateValue &= intValue;
				iBundle.State &= intValue;

				// 设置iOS完成标志位
				iBundle.State |= (int)TBundleState.IosCompleted;
			}


			// Android需要导出位
			if(((int)TBundleState.AndroidNeed & stateValue) == (int)TBundleState.AndroidNeed) {

				// 设置Android需要导出位
				iBundle.State |= (int)TBundleState.AndroidNeed;
				// 强制重置Android完成标志位
				int intValue = ~((int)TBundleState.AndroidCompleted);
				stateValue &= intValue;
				iBundle.State &= intValue;
			}

			// Android导出完成位
			if(((int)TBundleState.AndroidCompleted & stateValue) == (int)TBundleState.AndroidCompleted) {

				// 强制重置Android需要导出位
				int intValue = ~((int)TBundleState.AndroidNeed);
				stateValue &= intValue;
				iBundle.State &= intValue;

				// 设置Android完成标志位
				iBundle.State |= (int)TBundleState.AndroidCompleted;
			}

		}

#if UNITY_EDITOR
		/// <summary>
		/// 打包完成，更新资源包状态.
		/// </summary>
		/// <param name="iBuildTarget">打包目标.</param>
		public void UpdateBundleStateWhenCompleted(BuildTarget iBuildTarget) {

			List<BundleMap> maps = BundlesMap.GetInstance ().Maps;
			BundleResource bundle = null;
			foreach (BundleMap map in maps) {
				if (isResoureExist (map.Path, out bundle) == false) {
					continue;
				}
				if (bundle == null) {
					continue;
				}
				switch (iBuildTarget) {
				case BuildTarget.iOS:
					this.SetBundleState (bundle, TBundleState.IosCompleted);
					break;
				case BuildTarget.Android:
					this.SetBundleState (bundle, TBundleState.AndroidCompleted);
					break;
				default:
					break;
				}
			}

		}
#endif

		#endregion

		#region UnResource

		/// <summary>
		/// 添加非资源信息.
		/// </summary>
		/// <param name="iUnResourePath">非资源路径.</param>
		public BundleUnResource AddUnResource(BundleUnResource iUnResourceInfo) {
			if (iUnResourceInfo == null) {
				return null;
			}
			return AddUnResource(iUnResourceInfo.Path);
		}

		/// <summary>
		/// 添加非资源信息.
		/// </summary>
		/// <param name="iUnResourePath">非资源路径.</param>
		public BundleUnResource AddUnResource(string iUnResourcePath) {
			BundleUnResource bur = null;

			// 不存在存在
			if (this.isUnResoureExist (iUnResourcePath, out bur) == false) {
				bur = new BundleUnResource();
				this.UnResources.Add (bur);
			}
			if (bur != null) {
				bur.Path = iUnResourcePath;
				UtilityAsset.SetAssetDirty (this);
			} else {
				Debug.LogErrorFormat ("[Error]AddUnResource Failed!!!(Path:{0})",
					iUnResourcePath);
			}
			return bur;
		}

		/// <summary>
		/// 移除非资源信息.
		/// </summary>
		/// <returns><c>true</c>, 移除成功, <c>false</c> 移除失败.</returns>
		/// <param name="iUnResourcePath">非资源信息.</param>
		public bool RemoveUnResource(string iUnResourcePath) {
			BundleUnResource bur = null;
			// 不存在存在
			if (this.isUnResoureExist (iUnResourcePath, out bur) == true) {            
				if (bur != null) {
					this.UnResources.Remove (bur);
					UtilityAsset.SetAssetDirty (this);
				}
				return true;
			}
			return false;
		}

		/// <summary>
		/// 清空非资源列表.
		/// </summary>
		public void ClearUnResources() {
			if (this.UnResources == null) {
				return;
			}
			this.UnResources.Clear ();
			UtilityAsset.SetAssetDirty (this);
		}

		/// <summary>
		/// 判断非资源信息是否存在.
		/// </summary>
		/// <returns><c>true</c>, 村贼, <c>false</c> 不存在.</returns>
		/// <param name="iUnResourcePath">非资源路径.</param>
		/// <param name="iBur">非资源信息.</param>
		private bool isUnResoureExist(string iUnResourcePath, out BundleUnResource iBur) {

			bool bolRet = false;
			iBur = null;
			foreach(BundleUnResource bur in this.UnResources)
			{
				if(bur.Path == iUnResourcePath)
				{
					iBur = bur;
					bolRet = true;
					break;
				}
			}

			return bolRet;
		}

		#endregion

		/// <summary>
		/// 重置.
		/// </summary>
		public void Reset () {

			UtilityAsset.Clear<BundlesConfig> ();

			foreach (BundleResource loop in this.Resources) {
				loop.State = (int)(TBundleState.IosNeed | TBundleState.AndroidNeed);
			}
		}

		#region Implement

		/// <summary>
		/// 初始化.
		/// </summary>
		public override void Init () { 
		}

		/// <summary>
		/// 应用数据.
		/// </summary>
		/// <param name="iData">数据.</param>
		protected override void ApplyData(AssetBase iData) {
			if (iData == null) {
				return;
			}

			BundlesConfig data = iData as BundlesConfig;
			if (data == null) {
				return;
			}

			// 清空
			this.Clear ();

			this.FileSuffix = data.FileSuffix;

			// 添加资源信息
			foreach(BundleResource br in data.Resources) {
				this.AddResource (br);
			}

			// 添加非资源信息
			foreach(BundleUnResource ubr in data.UnResources) {
				this.AddUnResource (ubr);
			}

			UtilityAsset.SetAssetDirty (this);
		}

		/// <summary>
		/// 清空.
		/// </summary>
		public override void Clear () {

			UtilityAsset.Clear<BundlesConfig> ();

			this.ClearResources ();
			this.ClearUnResources ();
		}

		/// <summary>
		/// 从JSON文件，导入打包配置信息.
		/// </summary>
		public override void ImportFromJsonFile() {
			BundlesConfig jsonData = UtilityAsset.ImportFromJsonFile<BundlesConfig> ();
			if (jsonData != null) {
				this.ApplyData (jsonData);
			}
		}

		/// <summary>
		/// 导出成JSON文件.
		/// </summary>
		/// <returns>导出路径.</returns>
		public override string ExportToJsonFile() {
			return UtilityAsset.ExportToJsonFile<BundlesConfig> (this);
		}

		#endregion
	}

}