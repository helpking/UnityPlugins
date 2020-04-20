using System;
using System.Collections.Generic;
using Packages.Common.Base;
using Packages.Common.Editor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Packages.Dynamic.Terrains.Editor
{
	/// <summary>
	/// 地形Chunk信息列表
	/// </summary>
	[Serializable]
	public class TerrainSplitInfo : JsonDataBase<TerrainSplitInfo>
	{
		/// <summary>
		/// 拆分快数量
		///   备注:
		///     因为采用等长等宽拆分，且因为高度图的关系，必须为2的N次幂
		///     且地形拆分成 SlicingCount x SlicingCount
		/// </summary>
		public int SlicingCount;
		
		/// <summary>
		/// 保存着拆分地形用的
		/// </summary>
		public Dictionary<uint, int> MortonInfos = new Dictionary<uint, int>();
		
		/// <summary>
		/// chunks设定信息
		/// </summary>
		public Dictionary<int, TerrainChunkData> Chunks = new Dictionary<int, TerrainChunkData>();
		
		/// <summary>
		/// 初始化
		///   备注:
		///     这里我们切分长宽按等比例切分.既长和宽方向切分同等数量
		///     且高度地图的分辨率只能是2的N次幂加1，所以切分个数也必须为2的N次幂
		/// </summary>
		/// <param name="iData">地形数据</param>
		/// <param name="iSlicingCount">切分尺寸(x&z方向均等分，且为2的N次幂)</param>
		/// <param name="iTerrainChunks">Chunk拆分数据</param>
		public void Init(
			UnityEngine.TerrainData iData, int iSlicingCount,
			TerrainChunkData[] iTerrainChunks)
		{
			if (null == iData) return;
			if (0 >= iSlicingCount) return;
			
			// 查分数量(SlicingCount x SlicingCount)
			SlicingCount = iSlicingCount;

			// 追加chunk信息
			AddChunks(iTerrainChunks);

		}

		/// <summary>
		/// 追加chunk信息
		/// </summary>
		/// <param name="iTerrainChunks">chunks信息列表</param>
		private void AddChunks(TerrainChunkData[] iTerrainChunks)
		{
//			if(null == iTerrainChunks || 0 >= iTerrainChunks.Length) return;
//			foreach (var loop in iTerrainChunks)
//			{
//				TerrainChunkData terrainChunk;
//				if (Chunks.TryGetValue(loop.Index, out terrainChunk)) 
//				{
//					if (null == terrainChunk)
//					{
//						Chunks[loop.Index] = loop;
//					}
//					else
//					{
//						terrainChunk.Index = loop.Index;
//						terrainChunk.LocalIntdex = loop.LocalIntdex;
//						terrainChunk.Bounds = loop.Bounds;
//					}
//				}
//				else
//				{
//					Chunks.Add(loop.Index, loop);
//				}
//			}
		}

		/// <summary>
		/// 清空.
		/// </summary>
		public override void Clear()
		{
			Chunks.Clear();
			SlicingCount = 0;
		}
	}

	/// <summary>
	/// 地形分割类
	/// </summary>
	public class TerrainsSplit : SingletonBase<TerrainsSplit>
	{
		
		/// <summary>
		/// 拆地形信息列表
		///   Key : 地形对象的InstanceID
		///   Value : 拆分信息
		/// </summary>
		public Dictionary<int, TerrainSplitInfo> ConfInfo = new Dictionary<int, TerrainSplitInfo>();

		/// <summary>
		/// Chunks个数
		/// </summary>
		public int ChunksCount
		{
			get
			{
				var count = 0;
				foreach (var it in ConfInfo)
				{
					count += it.Value.Chunks.Count;
				}
				return count;
			}
		}

		/// <summary>
		/// 根据地形对象初始化拆分信息
		/// </summary>
		/// <param name="iTerrainObj">地形对象</param>
		/// <param name="iSlicingCount">切分个数(x&z方向均等分，且为2的N次幂)</param>
		/// <param name="iStaticOnly">仅处理静态对象标志位</param>
		/// <param name="iTerrainChunks">Chunk拆分数据</param>
		public void InitByTerrain(
			GameObject iTerrainObj,int iSlicingCount,
			bool iStaticOnly, TerrainChunkData[] iTerrainChunks)
		{
			
			if(null == iTerrainChunks || 0 >= iTerrainChunks.Length) return;
			if(null == iTerrainObj) return;

			var terrain = iTerrainObj.GetComponent<UnityEngine.Terrain>();
			if(null == terrain) return;
			
			// 初始化&追加chunk信息
			AddChunkInfo(
				iTerrainObj.GetInstanceID(), terrain.terrainData,
				iSlicingCount, iStaticOnly, iTerrainChunks);
		}


#region Chunks

		/// <summary>
		/// 解析地形Tile数据
		/// </summary>
		/// <param name="iSaveDir">保存目录</param>
		/// <param name="iTileIndex">Tile横向索引(Z轴方向)</param>
		/// <param name="iTerrainObj">拆分用地形对象</param>
		/// <param name="iMaxLayerLevel">地形最大层级</param>
		/// <param name="iLayerIndex">层级索引</param>
		/// <param name="iUpdateProgress">进度更新委托</param>
		public void SplitTerrains(
			string iSaveDir, GameObject iTerrainObj, 
			Vector2 iTileIndex, int iMaxLayerLevel, int iLayerIndex,
			ProgressBarUpdate iUpdateProgress = null)
		{
			// 计算当前层级的拆分数量
			var slicingCount = TerrainsSettings.CurSlicingCount(iMaxLayerLevel, iLayerIndex);

			// 计算当前层级Chunk尺寸计算
			for (var r = 0; r < slicingCount; ++r)
			{
				for (var c = 0; c < slicingCount; ++c)
				{
					// 开始拆分
					SplitTerrains(
						iSaveDir, iMaxLayerLevel - iLayerIndex, iTileIndex, slicingCount, 
						iTerrainObj, new Vector2(r,c), iUpdateProgress);
				}	
			}
		}

		/// <summary>
		/// 拆分地形
		/// </summary>
		/// <param name="iSaveDir">保存路径</param>
		/// <param name="iLayerLevel">地形层级</param>
		/// <param name="iTileIndex">地形的Tile索引</param>
		/// <param name="iSlicingCount">单个地形的拆分个数。(如：2x2 则为2)</param>
		/// <param name="iTerrainObj">地形对象</param>
		/// <param name="iChunkTileIndex">单个地形的拆分成chunk后的tile索引</param>
		/// <param name="iUpdateProgress">进度更新委托</param>
		private void SplitTerrains(
			string iSaveDir,int iLayerLevel,
			Vector2 iTileIndex, int iSlicingCount, 
			GameObject iTerrainObj, Vector2 iChunkTileIndex,
			ProgressBarUpdate iUpdateProgress = null)
		{
			if(string.IsNullOrEmpty(iSaveDir)) return;
			if(0 >= iSlicingCount) return;
			if (null == iTerrainObj)
			{
				Error("SplitTerrains():The gameobject is null or invalid in hierarchy!!!");
				return;
			}

			var terrain = iTerrainObj.GetComponent<Terrain>();
			if (null == terrain)
			{
				Error("SplitTerrains():There is no component named terrain in the gameobject(name:{0})!!!",
					iTerrainObj.name);
				return;
			}

			// 地形数据
			var terrainData = terrain.terrainData;
			
			// 地形尺寸
			var terrainSize = terrainData.size;
			
			// 得到地图分辨率
			var newHeightmapResolution = (terrainData.heightmapResolution - 1) / iSlicingCount;
			var newAlphamapResolution = terrainData.alphamapResolution / iSlicingCount;
			var newbaseMapResolution = terrainData.baseMapResolution / iSlicingCount;
			// 溅斑原型列表
			var splatProtos = terrainData.splatPrototypes;

			var x = (int)iChunkTileIndex.x;
			var y = (int)iChunkTileIndex.y;

			//创建资源
			// 地形拆分chunk保存路径
			var morton = TerrainsSettings.Morton(iTileIndex, iChunkTileIndex, iSlicingCount, iLayerLevel);
			var chunkName = string.Format("{0}{1}.asset", TerrainsSettings.ChunkNamePrefix, morton);
			var savePath = string.Format("{0}/{1}", iSaveDir, chunkName);
			// 更新进度 - 拆分开始
			if (null != iUpdateProgress)
			{
				var statusTxt = string.Format("拆分开始:{0}...", chunkName);
				iUpdateProgress(statusTxt, false);
			}
            var newData = new TerrainData();
            if (null == newData)
            {
	            Error("SplitTerrains():Failed!!(SavePath:{0})",
		            savePath);
	            return;
            }
			AssetDatabase.CreateAsset(newData, savePath);

            //设置分辨率参数
            newData.heightmapResolution = newHeightmapResolution;
            newData.alphamapResolution = newAlphamapResolution;
            newData.baseMapResolution = newbaseMapResolution;

            //设置大小
            newData.size = new Vector3(terrainSize.x / iSlicingCount, terrainSize.y, terrainSize.z / iSlicingCount);

            //设置地形原型
            var newSplats = new SplatPrototype[splatProtos.Length];
            for (var i = 0; i < splatProtos.Length;  ++i)
            {
	            newSplats[i] = new SplatPrototype
	            {
		            texture = splatProtos[i].texture, tileSize = splatProtos[i].tileSize
	            };

	            var offsetX = (newData.size.x * x) % splatProtos[i].tileSize.x + splatProtos[i].tileOffset.x;
                var offsetY = (newData.size.z * y) % splatProtos[i].tileSize.y + splatProtos[i].tileOffset.y;
                newSplats[i].tileOffset = new Vector2(offsetX, offsetY);
            }
            newData.splatPrototypes = newSplats;

            //设置混合贴图
            var alphamap = new float[newAlphamapResolution, newAlphamapResolution, splatProtos.Length];
            alphamap = terrainData.GetAlphamaps(
	            x * newData.alphamapWidth, 
	            y * newData.alphamapHeight, 
	            newData.alphamapWidth, 
	            newData.alphamapHeight);
            newData.SetAlphamaps(0, 0, alphamap);

            //设置高度
            var xBase = (terrainData.heightmapWidth - 1) / iSlicingCount;
            var yBase = (terrainData.heightmapHeight - 1) / iSlicingCount;
            var height = terrainData.GetHeights(xBase * x, yBase * y, xBase + 1, yBase + 1);
            newData.SetHeights(0, 0, height);
            AssetDatabase.SaveAssets();

            // 更新进度 - 拆分成功
            if (null != iUpdateProgress)
            {
	            var statusTxt = string.Format("拆分成功:{0}", chunkName);
	            iUpdateProgress(statusTxt, true);
            }
		}

		/// <summary>
		/// 设置拆分块信息
		/// </summary>
		/// <param name="iKey">Key：地形所在Gameobject的InstanceID</param>
		/// <param name="iData">地形数据</param>
		/// <param name="iSlicingCount">切分尺寸(x&z方向均等分，且为2的N次幂)</param>
		/// <param name="iStaticOnly">仅处理静态物体</param>
		/// <param name="iTerrainChunks">Chunk拆分数据</param>
		public void AddChunkInfo(
			int iKey, UnityEngine.TerrainData iData, 
			int iSlicingCount, bool iStaticOnly, 
			TerrainChunkData[] iTerrainChunks)
		{
			TerrainSplitInfo terrainChunk;
			if (ConfInfo.TryGetValue(iKey, out terrainChunk)) return;
			terrainChunk = new TerrainSplitInfo();
			// 初始化
			terrainChunk.Init(
				iData, iSlicingCount, 
				iTerrainChunks);
			ConfInfo.Add(iKey, terrainChunk);
		}

		/// <summary>
		/// 清空拆分块Chunk信息列表
		/// </summary>
		public void ClearChunksInfo()
		{
			// 清空拆分块Chunk信息列表
			if(null == ConfInfo || 0 >= ConfInfo.Count) return;
			ConfInfo.Clear();
		}
		
#endregion

#region Prefabs

		/// <summary>
		/// 拆分用预制体跟节点
		/// </summary>
		protected GameObject PrefabsTemp = null;
		
		/// <summary>
		/// 需要处理的预制体列表
		/// </summary>
		protected List<GameObject> Prefabs = null;

		/// <summary>
		/// 初始化拆分用预制体信息
		/// </summary>
		/// <param name="iStaticOnly">仅处理静态物体</param>
		/// <param name="iDepthLimit">遍历深度限制
		///   备注:
		///     小于0 不限制
		///     等于0 表示只访问root本身而不访问其子级,
		///     大于0 正值表示最多访问的子级层数
		/// </param>
		/// <param name="iUpdateProgress">进度更新委托</param>
		public void InitPrefabsInfo(
			bool iStaticOnly, int iDepthLimit = -1,
			ProgressBarUpdate iUpdateProgress = null)
		{
			if (null == Prefabs)
			{
				Prefabs = new List<GameObject>();
			}
			Prefabs.Clear();
			
			var roots = SceneManager.GetActiveScene().GetRootGameObjects();
			if(null == roots || 0 >= roots.Length) return;
			// 遍历节点对象
			foreach (var root in roots)
			{
				// 遍历
				TraverseHierarchy(root, iStaticOnly,
					(iCurObj, iIsStaticOnly, iRetObj) =>
					{
						// 若是地形对象，则返回继续遍历
						if (iCurObj.GetComponent<Terrain>()) return null;
						
						// 如果有MeshRender，或处理静态物体时，
						// 则认为是一个要处理的叶子节点，不再处理其孩子节点了
						if ((iCurObj.GetComponent<MeshRenderer>() ||
						    iCurObj.GetComponent<Animator>() ||
						    iCurObj.GetComponent<LODGroup>()) &&
						    (false == iIsStaticOnly || iCurObj.isStatic))
						{
							Prefabs.Add(iCurObj);
							return iCurObj;
						}
						return null;
					}, iDepthLimit);

			}

			if (null != iUpdateProgress)
			{
				iUpdateProgress("预制体拆分初始化", false);
			}
			Info("InitPrefabsInfo():StaticOnly:{0} Depth:{1}(Prefabs Count:{2})",
				iStaticOnly, iDepthLimit,
					(null == Prefabs) ? -1 : Prefabs.Count);
		}

		/// <summary>
		/// 创建拆分用预制体跟节点
		/// </summary>
		private void CreatePrefabsTempInfo()
		{
			PrefabsTemp = new GameObject(TerrainsConst.PREFABS_TEMP_NODE);
			if (null == PrefabsTemp) return;
			PrefabsTemp.transform.localPosition = Vector3.zero;
			PrefabsTemp.transform.eulerAngles = Vector3.zero;
			PrefabsTemp.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
		}

		/// <summary>
		/// 移动拆分用预制体跟节点
		/// </summary>
		private void RemovePrefabsTempInfo()
		{
			if(null == PrefabsTemp) return;
			Object.DestroyImmediate(PrefabsTemp);
		}

		/// <summary>
        /// 遍历Hierarchy,寻找到跟节点下所有的对象
        /// </summary>
        /// <param name="iRoot">根节点</param>
        /// <param name="iStaticOnly">仅处理静态物体</param>
        /// <param name="iOperate">操作
        ///  备注：
        ///    遍历到每一个节点时将调用此方法;
        ///      参数1: 当前访问的对象;
        ///      参数2: 包括本层次在内的剩余深度限制;
        ///      返回值: null 表示继续遍历, 非空引用将终止遍历过程, 并且此返回值最终作为遍历方法的返回值 
        /// </param>
        /// <param name="iDepthLimit">遍历深度限制
        ///   备注:
        ///     小于0 不限制
        ///     等于0 表示只访问root本身而不访问其子级,
        ///     大于0 正值表示最多访问的子级层数
        /// </param>
        protected void TraverseHierarchy(
			GameObject iRoot, bool iStaticOnly, 
			Func<GameObject, bool, int, GameObject> iOperate,
         	int iDepthLimit = -1)
		{
			// 非激活状态，则返回继续遍历
			if (false == iRoot.activeInHierarchy || false == iRoot.activeSelf) return;
			// 无操作毁掉，则返回继续遍历
			if (iOperate == null) return;
			var obj = iOperate(iRoot, iStaticOnly, iDepthLimit);
			// 查到到目标对象或遍历到最大深度，则结束，并返回上层遍历
			if (null != obj || 0 == iDepthLimit) return;
			// 若没有找到目标节点，则继续遍历子节点
			for (var i = iRoot.transform.childCount - 1; i >= 0; i--)
			{
				var child = iRoot.transform.GetChild(i).gameObject;
				TraverseHierarchy(
					child, iStaticOnly, 
					iOperate, iDepthLimit - 1);
			}
		}

#endregion


//		/// <summary>
//		/// 对一个GameObject按照位置进行分类，放置到对应的根节点下面。
//		/// </summary>
//		/// <param name="obj"></param>
//		static void ClassifyGameObject(GameObject obj, float width, float height)
//		{
//			Vector3 pos = obj.transform.position;
//			// chunk的索引
//			int targetChunkX = (int)(pos.x / width) + 1;
//			int targetChunkZ = (int)(pos.z / height) + 1;
//			string chunkName = ChunkRootNamePrefix + string.Format("{0}_{1}", targetChunkX, targetChunkZ);
//			GameObject chunkRoot = GameObject.Find(chunkName) ;
//			if (chunkRoot == null)
//			{
//				chunkRoot = new GameObject(chunkName);
//			}
//
//			//复制层次关系到Chunk的节点下面
//			GameObject tempObj = obj;
//			List<GameObject> objs2Copy = new List<GameObject>();
//			while(tempObj.transform.parent)
//			{
//				objs2Copy.Add(tempObj.transform.parent.gameObject);
//				tempObj = tempObj.transform.parent.gameObject;
//			}
//			tempObj = chunkRoot;
//			for (int i = objs2Copy.Count - 1; i > -1; --i)
//			{
//				GameObject targetObj = objs2Copy[i];
//				// 对于符合Chunk命名规则的父节点不进行拷贝过程。
//				if (targetObj.name.StartsWith(ChunkRootNamePrefix))
//				{
//					continue;
//				}
//				Transform parent = tempObj.transform.FindChild(targetObj.name);
//				if (parent == null)
//				{
//					parent = new GameObject(targetObj.name).transform;
//					CopyComponents(targetObj, parent.gameObject);
//					parent.parent = tempObj.transform;
//					targetObj = parent.gameObject;
//				}
//				tempObj = parent.gameObject;
//			}
//			Transform tempParent = obj.transform.parent;
//			obj.transform.parent = tempObj.transform;
//			// 移动完毕之后发现父节点没有孩子节点的情况下，向上遍历将无用节点删除。
//			while (tempParent != null && tempParent.childCount == 0)
//			{
//				Transform temp = tempParent.parent;
//				EngineUtils.Destroy(tempParent.gameObject);
//				tempParent = temp;
//			}
//		}
	}
}

