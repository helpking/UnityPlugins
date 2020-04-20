using System;
using System.Collections.Generic;
using System.Linq;
using Packages.Common.Base;
using Packages.Dynamic.Terrains.Morton;
using UnityEngine;
using UnityEngine.SceneManagement;
using UINT32 = System.UInt32;

namespace Packages.Dynamic
{
	/// <summary>
	/// 地形拆分块数据
	/// </summary>
	[Serializable]
	public class TerrainChunkData : JsonDataBase<TerrainChunkData>
	{
		/// <summary>
		/// 莫顿码(标示线性四叉树用的Key)
		/// 3维莫顿码
		///  第1唯:横向x
		///  第2唯:纵向z
		///  第3唯:竖向y(等级/阶层)
		/// </summary>
		public UINT32 Morton;
		/// <summary>
		/// 地形才分后的Chunk名
		/// </summary>
		public string Name;
		/// <summary>
		/// 拆分块的包围盒
		/// </summary>
		public Bounds Bounds;
	}

	/// <summary>
	/// 地形数据
	/// </summary>
	[Serializable]
	public class TerrainsData : JsonDataBase<TerrainsData>
	{
		/// <summary>
		/// 数据目录
		/// </summary>
		public string DataDir;
		
		/// <summary>
		/// 场景地形数据
		/// </summary>
		public List<TerrainChunkData> Chunks = new List<TerrainChunkData>();

		/// <summary>
		/// 追加地形Chunk信息
		/// </summary>
		/// <param name="iMorton">莫顿码</param>
		/// <param name="iBoundCenter">包围盒中心点(世界坐标)</param>
		/// <param name="iBoundSize">包围盒大小</param>
		public void AddChunkInfo(UINT32 iMorton, Vector3 iBoundCenter, Vector3 iBoundSize)
		{
			var array = Chunks
				.Where(o => iMorton == o.Morton)
				.ToArray();
			TerrainChunkData chunkInfo;
			if (0 >= array.Length)
			{
				chunkInfo = new TerrainChunkData();
				Chunks.Add(chunkInfo);
				
				chunkInfo.Morton = iMorton;
			}
			else
			{
				chunkInfo = array[0];
			}
			chunkInfo.Bounds = new Bounds(iBoundCenter, iBoundSize);
		}

		/// <summary>
		/// 清空.
		/// </summary>
		public override void Clear()
		{
			DataDir = null;
			Chunks.Clear();
		}
	}

	/// <summary>
	/// 
	/// </summary>
	[Serializable]
	public class TerrainsSettings : AssetReadOnlyBase<TerrainsSettings, TerrainsData>
	{
		/// <summary>
		/// 地形导出数据路径
		/// </summary>
		public const string DataBaseDir = "Assets/Resources/TerrainData";

		/// <summary>
		/// 地形chunk名前缀
		/// </summary>
		public const string ChunkNamePrefix = "Chunk_";
		
		/// <summary>
		/// 场景名
		/// </summary>
		public string SceneName = null;

		/// <summary>
		/// 地形Chunk数据导出目录
		/// </summary>
		public string ChunkDataDir
		{
			get
			{
				var _sceneName = string.IsNullOrEmpty(SceneName) ? SceneManager.GetActiveScene().name : SceneName;
				if (string.IsNullOrEmpty(data.DataDir))
				{
					data.DataDir = DataBaseDir;
				}
				return string.Format("{0}/{1}/Chunks", data.DataDir, _sceneName);
			}
		}
		
		/// <summary>
		/// 预制体数据导出目录
		/// </summary>
		public string PrefabsDataDir
		{
			get
			{
				var _sceneName = string.IsNullOrEmpty(SceneName) ? SceneManager.GetActiveScene().name : SceneName;
				if (string.IsNullOrEmpty(data.DataDir))
				{
					data.DataDir = DataBaseDir;
				}
				return string.Format("{0}/{1}/Prefabs", data.DataDir, _sceneName);
			}
		}
		
		/// <summary>
		/// 导入场景配置信息.
		/// </summary>
		/// <returns><c>true</c>, 导入成功, <c>false</c> 导入失败.</returns>
		/// <param name="iSceneName">场景名.</param>
		/// <param name="iForceClear">强制清空.</param>
		public bool ImportFromSceneName(string iSceneName = null, bool iForceClear = true)
		{
			var sceneName = string.IsNullOrEmpty(iSceneName) ? SceneName : iSceneName;
			return !string.IsNullOrEmpty(SceneName) && ImportFromJsonFile(sceneName, iForceClear);
		}

		/// <summary>
		/// 导出成JSON文件.
		/// </summary>
		/// <returns>导出文件(Json格式).</returns>
		/// <param name="iSceneName">场景名.</param>
		public string ExportBySceneName(string iSceneName = null)
		{
			var sceneName = string.IsNullOrEmpty(iSceneName) ? SceneName : iSceneName;
			return ExportToJsonFile (GetExportPath(), sceneName);
		}

		/// <summary>
		/// 解析地形Tile数据
		/// </summary>
		/// <param name="iTileIndex">Tile横向索引(Z轴方向)</param>
		/// <param name="iChunkSize">地形Chunk尺寸</param>
		/// <param name="iMaxLayerLevel">地形最大层级</param>
		/// <param name="iLayerIndex">层级索引</param>
		public void ParserTerrainTileData(
			Vector2 iTileIndex, Vector3 iChunkSize,
			int iMaxLayerLevel, int iLayerIndex)
		{
			// 计算当前层级的拆分数量
			var slicingCount = CurSlicingCount(iMaxLayerLevel, iLayerIndex);
			
			// 计算当前层级Chunk尺寸计算
			var chunkWidth = (int) iChunkSize.x;
			var chunkLength = (int) iChunkSize.z;
			var curPosX = 0.0f;
			var curPosZ = 0.0f;
			for (var r = 0; r < slicingCount; ++r)
			{
				curPosZ += (0 == r) ? chunkLength / 2 : chunkLength;
				curPosZ += (iTileIndex.y * chunkLength);
				curPosX = 0.0f;
				for (var c = 0; c < slicingCount; ++c)
				{
					curPosX += (0 == c) ? chunkWidth / 2 : chunkWidth;
					curPosX += (iTileIndex.x * chunkWidth);
					var chunkCenter = new Vector3(curPosX, iChunkSize.y / 2, curPosZ);

					// 取得莫顿码
					var morton = Morton(
						iTileIndex, new Vector2(c, r), 
						slicingCount, iMaxLayerLevel - iLayerIndex);
					data.AddChunkInfo(morton, chunkCenter, iChunkSize);
				}	
			}
		}

		/// <summary>
		/// 取得地形尺寸大小
		/// </summary>
		/// <param name="iTerrain">地形对象</param>
		/// <returns>地形尺寸大小</returns>
		public static Vector3 GetTerrainSize(GameObject iTerrain)
		{
			if (null == iTerrain) return Vector3.zero;
			var terrain = iTerrain.GetComponent<TerrainCollider>();
			return null != terrain ? terrain.terrainData.size : Vector3.zero;
		}

		/// <summary>
		/// 取得当前拆分的chunk层级的尺寸
		/// </summary>
		/// <param name="iTerrainSize">地形尺寸</param>
		/// <param name="iMaxLayerLevel">最大层级</param>
		/// <param name="iLayerIndex">层级索引</param>
		/// <returns></returns>
		public static Vector3 GetTerrainChunkSize(
			Vector3 iTerrainSize, int iMaxLayerLevel, int iLayerIndex)
		{
			var chunkSize = iTerrainSize / (float)Math.Pow(2, iMaxLayerLevel - iLayerIndex);
			// 高度不变化
			chunkSize.y = iTerrainSize.y;
			return chunkSize;
		}

		/// <summary>
		/// 当前层级的拆分数
		///  备注：
		///   当前层级所需要的拆分数目。如当前为第2层，则拆分的为
		/// </summary>
		/// <param name="iMaxLayerLevel">最大层级</param>
		/// <param name="iLayerIndex">层级索引</param>
		/// <returns>当前层级拆分数</returns>
		public static int CurSlicingCount(int iMaxLayerLevel, int iLayerIndex)
		{
			// 计算当前层级的拆分数量
			var tmpValue = iMaxLayerLevel - iLayerIndex;
			tmpValue = (0 >= tmpValue) ? 0 : tmpValue;
			return (int)Math.Pow(2, tmpValue);
		}

		/// <summary>
		/// 莫顿码计算
		/// </summary>
		/// <param name="iTileIndex">地形的Tile索引</param>
		/// <param name="iChunkTileIndex">单个地形的拆分成chunk后的tile索引</param>
		/// <param name="iSlicingCount">单个地形的拆分个数。(如：2x2 则为2)</param>
		/// <param name="iLayerLevel">当前所属层级</param>
		/// <returns>莫顿码</returns>
		public static UINT32 Morton(Vector2 iTileIndex, Vector2 iChunkTileIndex, int iSlicingCount, int iLayerLevel)
		{
			var chunkIndexXG = (int) (iTileIndex.x * iSlicingCount + iChunkTileIndex.x);
			var chunkIndexZG = (int) (iTileIndex.y * iSlicingCount + iChunkTileIndex.y);
			return UtilsMorton.Morton3d(chunkIndexXG, iLayerLevel, chunkIndexZG);
		}

		/// <summary>
		/// 自动排序
		/// </summary>
		public void AutoSort()
		{
			if (null == data || null == data.Chunks || 0 >= data.Chunks.Count)
			{
				return;
			}
			
			// 排序：排序层级(大->小) > x轴方向(小->大) > z轴方向(小->大)
			// 既：从下往上，从左到右，从外到内
			data.Chunks.Sort((iX, iY) =>
			{
				if (null == iX || null == iY) return 0;
			
				// 发型解析莫顿码
				var x = Vector3.zero;
				UtilsMorton.InverseMorton3d(ref x.x, ref x.y, ref x.z, iX.Morton);
				var y = Vector3.zero;
				UtilsMorton.InverseMorton3d(ref y.x, ref y.y, ref y.z, iY.Morton);	
				
				if (x.y > y.y) return -1;
				if (x.x < y.x) return -1;
				if (x.z < y.z) return -1;
				return (x.y < y.y) && (x.x > y.x) && (x.z > y.z) ? 1 : 0;
			});
		}

		/// <summary>
		/// 取得地形chunk信息列表
		/// </summary>
		/// <param name="iLayerLevel">层级(-1：默认 全部取得)</param>
		/// <returns></returns>
		public List<TerrainChunkData> GetChunkData(int iLayerLevel = -1)
		{
			if (-1 == iLayerLevel) return data.Chunks;
			var list = data.Chunks
				.Where(o =>
				{
					var tilePos = Vector3.zero;
					UtilsMorton.InverseMorton3d(ref tilePos.x, ref tilePos.y, ref tilePos.z, o.Morton);
					return Math.Abs(iLayerLevel - tilePos.y) <= 0.0f;
				})
				.ToList();
			return list;
		}

		/// <summary>
		/// 用用数据.
		/// </summary>
		/// <param name="iData">数据.</param>
		/// <param name="iForceClear">强制清空标志位.</param>
		protected override void ApplyData(TerrainsData iData, bool iForceClear = true)
		{
			if (null == iData) {
				return;
			}

			// 清空
			if(iForceClear) {
				Clear ();
			}

			data.DataDir = iData.DataDir;
			data.Chunks.AddRange(iData.Chunks);

		}
	}
}

