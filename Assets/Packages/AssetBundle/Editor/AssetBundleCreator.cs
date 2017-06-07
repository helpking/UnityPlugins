using UnityEngine;
using UnityEditor; 
using Common;

namespace AssetBundles {
	
	/// <summary>
	/// Utility asset creator.
	/// </summary>
	public class AssetBundleCreator : Editor {	

//#if UNITY_EDITOR

		/// <summary>
		/// 创建打包信息配置文件.
		/// </summary>
		[MenuItem("Assets/Create/Bundles/BuildInfo")]	
		static BuildInfo CreateBuildInfo ()	{	
			return UtilityAsset.CreateAsset<BuildInfo> ();
		}

		/// <summary>
		/// 创建资源打包配置文件（用于设定当前打包对象）.
		/// </summary>
		[MenuItem("Assets/Create/Bundles/Config")]	
		static BundlesConfig CreateBundlesConfig ()	{	
			return UtilityAsset.CreateAsset<BundlesConfig> ();
		}

		/// <summary>
		/// 创建资源打包地图（用于打包）.
		/// </summary>
		[MenuItem("Assets/Create/Bundles/Map")]	
		static BundlesMap CreateBundlesMap ()	{	
			return UtilityAsset.CreateAsset<BundlesMap> ();	
		}

//#endif
	}

}