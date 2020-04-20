using System.Collections;
using UnityEngine;
using Packages.Common.Base;
using Packages.Dynamic.Terrains;

namespace Packages.Dynamic
{
	
	/// <summary>
	/// 地形管理器
	/// </summary>
	[AddComponentMenu("Packages/Dynamic/DynamicLoader")]
	public class DynamicLoader : MonoBehaviour {
		
		private void Awake()
		{
//			TerrainsConf.GetInstance().ImportFromJsonFile();
		}

		void Start()
		{
			// 异步家在
			StartCoroutine(LoadTerrainAysnc());
		}

		/// <summary>
		/// 异步家在地形
		/// </summary>
		protected IEnumerator LoadTerrainAysnc()
		{
//			var confData = TerrainsConf.GetInstance().data;
//			if (null == confData) yield break;
//			var chunks = confData.TerrainChunks;
//			if (null == chunks) yield break;
//			foreach (var chunk in chunks)
//			{
//				var path = string.Format("{0}/{1}/chunk_{2}", confData.DataDir, confData.SceneName, chunk.Index);
//				if (path.StartsWith("Assets/Resources")) path = path.Replace("Assets/Resources/", "");
//				var terrainData = Resources.Load<TerrainData>(path);
//				var go = new GameObject(string.Format(TerrainsConst.CHUNK_NM_FORMAT, chunk.Index));
//				go.transform.parent = transform;
//				go.transform.localPosition = new Vector3(chunk.Bounds.center.x, 0.0f, chunk.Bounds.center.z);
//				var terrain = go.AddComponent<UnityEngine.Terrain>();
//				go.AddComponent<TerrainCollider>();
//				terrain.terrainData = terrainData;
//			}
			
			yield return null;
		}

	}
}


