using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using Packages.Common.Base;
using Packages.Common.Editor;
using Packages.Common.Extend;
using Packages.Common.Extend.Editor;
using Packages.Utils;
using Packages.Utils.Editor;
using Packages.Utils.Editor.Draw;

namespace Packages.Dynamic.Terrains.Editor
{

	/// <summary>
	/// 地形编辑配置数据
	/// </summary>
	[System.Serializable]
	public class TerrainEditorData : JsonDataBase<TerrainEditorData>
	{
		
		/// <summary>
		/// 预览信息
		/// </summary>
		[Serializable]
		public class PreviewData : JsonDataBase<PreviewData>
		{
			/// <summary>
			/// 边色
			/// </summary>
			public Color edgeColor = Color.white;

			/// <summary>
			/// 顶点色
			/// </summary>
			public Color vertexColor = Color.white;

			/// <summary>
			/// 预览缩放（基础尺寸为1.0f）
			/// </summary>
			public float Scale = 1.0f;

			public override void Clear()
			{
				edgeColor = Color.white;
				vertexColor = Color.white;
				Scale = 1.0f;
			}
		}
		
		[Serializable]
		public class TerrainTileData
		{
			/// <summary>
			/// 地形尺寸
			///  备注:x:宽 y:高 z:长
			/// </summary>
			public Vector3 TerrainSize = Vector3.zero;
			/// <summary>
			/// 基础地形
			/// </summary>
			public TerrainData Data;
			
			/// <summary>
			/// 预览信息(分层级设定)
			/// </summary>
			public PreviewData Preview = new PreviewData();
		}

		/// <summary>
		/// 拆分块信息
		/// </summary>
		[System.Serializable]
		public class ChunkSizeData
		{
			/// <summary>
			/// Chunk切分个数
			///   备注:
			///     这里我们切分长宽按等比例切分.既长和宽方向切分同等数量
			///     且高度地图的分辨率只能是2的N次幂加1，所以切分个数也必须为2的N次幂
			/// </summary>
			public float SlicingCount;
			
			/// <summary>
			/// 仅处理静态物体
			/// </summary>
			public bool StaticOnly;

			/// <summary>
			/// 预制体导出深度
			/// </summary>
			public int PrefabsExportDepthLimit = 10;
			
			/// <summary>
			/// 预览信息
			/// </summary>
			public PreviewData Preview = new PreviewData();

		}
		
		/// <summary>
		/// 地形编辑模式
		/// </summary>
		public enum TerrainEditorMode
		{
			/// <summary>
			/// 新建
			/// </summary>
			New,
			/// <summary>
			/// 编辑
			/// </summary>
			Edit,
			/// <summary>
			/// 拆分
			/// </summary>
			Split,
			/// <summary>
			/// 导出
			/// </summary>
			Export,
			/// <summary>
			/// 其他
			/// </summary>
			Else,
			Max
		}
		
		/// <summary>
		/// 已选中编辑模式索引
		/// </summary>
		public TerrainEditorMode SelectedMode = TerrainEditorMode.New;

		/// <summary>
		/// 地形尺寸
		/// </summary>
		public TerrainTileData tileData = new TerrainTileData();
		
		/// <summary>
		/// Chunk Tile尺寸
		/// </summary>
		public Vector2 TileSize = Vector2.zero;

		/// <summary>
		/// 地形尺寸
		/// </summary>
		public ChunkSizeData chunkData = new ChunkSizeData();
		
		/// <summary>
		/// 清空.
		/// </summary>
		public override void Clear()
		{
			SelectedMode = TerrainEditorMode.New;
			// 地形尺寸至少是1x1
			if (null != tileData)
			{
				tileData.TerrainSize = Vector3.zero;
				tileData.Data = null;
				
				tileData.Preview.Clear();
			}
			TileSize = Vector2.zero;

			// Chunk设定
			if (null != chunkData)
			{
				chunkData.SlicingCount = 2;
				chunkData.StaticOnly = false;
			}
		}
		
	}

	/// <summary>
	/// 地形编辑器
	/// </summary>
	[Serializable]
	public class TerrainEditor : WindowInspectorBase<TerrainEditor, TerrainEditorData> {
		
		/// <summary>
		/// Json文件导出路径
		/// </summary>
		private const string JsonFileDir = "Assets/Packages/Dynamic/Terrains/Editor/Json";

		/// <summary>
		/// 地形数据到处目录
		/// </summary>
		private string terrainDataOutputDir = null;
		
		/// <summary>
		/// 地形根节点
		/// </summary>
		private GameObject _terrainsRoot = null;
		private const string TerrainsRootName = "Terrains";
		
		/// <summary>
		/// 地形包围盒预览显示标识位
		/// </summary>
		private bool _terrainBoundPreviewVisiable = false;
		
		/// <summary>
		/// Chunk包围盒预览显示标识位
		/// </summary>
		private bool _chunkBoundPreviewVisiable = false;

		private string _curSceneName = null;
		
		/// <summary>
		/// 层级预览选择
		/// </summary>
		private List<string> _layerPreviewTitles = new List<string>();
		private List<bool> _layerPreviewSelected = new List<bool>();
		
		/// <summary>
		/// 导出层级
		/// </summary>
		private int _exportLayerLevel = 0;
		
		/// <summary>
		/// 当前场景名
		/// </summary>
		public string CurSceneName
		{
			get
			{
				if (!string.IsNullOrEmpty(_curSceneName)) return _curSceneName;
				var curScene = SceneManager.GetActiveScene();
				_curSceneName = curScene.name;
				return _curSceneName;
			}
		}

		private TerrainsSettings _settings = null;
		/// <summary>
		/// 设定信息
		/// </summary>
		public TerrainsSettings Settings
		{
			get
			{
				if (null == _settings) _settings = TerrainsSettings.GetInstance();
				return _settings;
			}
		}

		/// <summary>
		/// 窗口类不要写构造函数，初始化写在Awake里
		/// </summary>
		void Awake()
		{
			if (Init(JsonFileDir)) return;
			this.Error("Awake():TerrainEditor Failed Or not conf file!!!\n(file={0})",
				JsonFileDir);
			return;
		}

#region Implement

		/// <summary>
		/// 初始化地形
		/// </summary>
		/// <param name="iLevel">层级(影响到左边缩进的余白)</param>
		private void InitTerrains(int iLevel)
		{
			var level = iLevel;
			var selected = (int) ConfData.SelectedMode;
			DrawToolBars(level, new [] {"新建", "编辑&预览", "拆分", "导出", "其他"}, ref selected );
			ConfData.SelectedMode = (TerrainEditorData.TerrainEditorMode) selected;
			
			switch (ConfData.SelectedMode)
			{
				case TerrainEditorData.TerrainEditorMode.New:
				{
					// 新建地形
					NewTerrains(level);
				}
					break;
				case TerrainEditorData.TerrainEditorMode.Edit:
				{
					// 编辑地形
					EditorTerrains(level);
				}
					break;
				case TerrainEditorData.TerrainEditorMode.Split:
				{
					// 拆分地形
					SplitTerrains(level);
				}
					break;
				case TerrainEditorData.TerrainEditorMode.Export:
				{
					// 导出地形Chunks
					ExportChunks(level);
				}
					break;
			}
		}
		
		/// <summary>
		/// 清空Debug调试节点
		/// </summary>
		/// <param name="iTagIndex">标签索引</param>
		protected override void ClearDrawObjects(int iTagIndex = -1)
		{
			// 清空所有绘制对象
			UtilsDraw.ClearAllDrawList(iTagIndex);
			
			// 清空所有残留对象
			var tag = (-1 == iTagIndex)
				? UtilsDraw.GameObjectTagDrawDebug
				: string.Format("{0}_{1}", UtilsDraw.GameObjectTagDrawDebug, iTagIndex);
			var tagObjects = GameObject.FindGameObjectsWithTag(tag);
			if (null != tagObjects && 0 < tagObjects.Length)
			{
				foreach (var tagObj in tagObjects)
				{
					if(null == tagObj) continue;
					DestroyImmediate(tagObj);
				} 
			}

			// 清空拆分块Chunk信息列表
			TerrainsSplit.Instance.ClearChunksInfo();
		}
		
		/// <summary>
		/// 新建地形
		/// </summary>
		/// <param name="iLevel">层级(影响到左边缩进的余白)</param>
		private void NewTerrains(int iLevel)
		{
			var level = iLevel;
			++level;
			DrawLongLabel(level, "地形尺寸设定：");

			++level;
			{
				DrawMiddleSlider(
					level, "宽(X方向)：",
					ref ConfData.tileData.TerrainSize.x,
					128.0f, 1024.0f);
				if (!UtilsTools.IsPowerOf2((int)ConfData.tileData.TerrainSize.x))
				{
					var error = string.Format("{0} 不是2的N次幂(2~16)！",
						(int) ConfData.tileData.TerrainSize.x);
					DrawLabel(level+1, error, Color.red);
				}
			}
			{
				DrawMiddleSlider(
					level, "长(Z方向)：",
					ref ConfData.tileData.TerrainSize.z,
					128.0f, 1024.0f);
				if (!UtilsTools.IsPowerOf2((int)ConfData.tileData.TerrainSize.z))
				{
					var error = string.Format("{0} 不是2的N次幂(2~16)！",
						(int) ConfData.tileData.TerrainSize.z);
					DrawLabel(level+1, error, Color.red);
				}
			}
			{
				DrawMiddleSlider(
					level, "高(Y方向)：",
					ref ConfData.tileData.TerrainSize.y,
					128.0f, 1024.0f);
				if (!UtilsTools.IsPowerOf2((int)ConfData.tileData.TerrainSize.y))
				{
					var error = string.Format("{0} 不是2的N次幂(2~16)！",
						(int) ConfData.tileData.TerrainSize.y);
					DrawLabel(level+1, error, Color.red);
				}
				
			}

			// 分界线
			DrawLongLabel(0, "---------------");
			
			--level;
			// 地形数量
			DrawLongLabel(level, "地形数量(布局)：");
			
			++level;
			DrawMiddleSlider(
				level, "数量(x)：",
				ref ConfData.TileSize.x,
				1.0f, 16.0f);
			DrawMiddleSlider(
				level, "数量(z)：",
				ref ConfData.TileSize.y,
				1.0f, 16.0f);
			
			// 基础地形
			DrawMiddleTerrain(level, "基础地形：", ref ConfData.tileData.Data);
			--level;
			
			DrawSingleButton(0, "初始化地形",
				delegate()
				{
					// 自动生成Chunk
					AutoCreateTerrains(ConfData.tileData);
				}, Color.yellow);
		}

		/// <summary>
		/// 编辑地形
		/// </summary>
		/// <param name="iLevel">层级(影响到左边缩进的余白)</param>
		private void EditorTerrains(int iLevel)
		{
			var level = iLevel;
			++level;
			DrawMiddleGameObject(
				level, "地形根节点：", ref _terrainsRoot);

			DrawSingleMiddleToggle(level, "显示边界：", _terrainBoundPreviewVisiable,
				delegate(bool iValue) { _terrainBoundPreviewVisiable = iValue; });
			if (_terrainBoundPreviewVisiable)
			{

				// 取得预览信息设定
				var preview = ConfData.tileData.Preview;
				DrawLongLabel(level, "预览缩放：");
				DrawLongLabel(level+1, "颜色设定：");
				++level;
				DrawMiddleColor(level+2, "边色：", preview.edgeColor,
					delegate(Color iColor)
					{
						preview.edgeColor = iColor;
						// 重置所有边的颜色
						UtilsDraw.ResetAllCylinderColor(
							iColor, (int)DrawType.Cylinder);
					});
				
				DrawMiddleColor(level+2, "顶点色：", preview.vertexColor,
					delegate(Color iColor)
					{
						preview.vertexColor = iColor;
						// 重置所有边的颜色
						UtilsDraw.ResetAllVertexColor(iColor, 
							(int)DrawType.Sphere);
					});
				DrawMiddleSlider(level+2, "缩放：", 
					preview.Scale, delegate(float iValue)
					{
						preview.Scale = iValue;
						UtilsDraw.ResetAllBoundsPreviewScale(
							iValue, (int)DrawType.Cylinder | (int)DrawType.Sphere);
					}, 
					1.0f, 50.0f);
				
				// 遍历根节点并取得包围盒
				if (null == _terrainsRoot) return;
				for (var i = _terrainsRoot.transform.childCount - 1; i >= 0; i--)
				{
					var child = _terrainsRoot.transform.GetChild(i);
					if(null == child) continue;
					if(false == child.gameObject.activeInHierarchy) continue;
					// 绘制包围盒
					UtilsDraw.DrawTerrainBounds(
						child.gameObject, child.transform, 
						preview.edgeColor, preview.vertexColor, preview.Scale, i, 
						9999);
				}
			}
			else
			{
				ClearDebugNode(9999);
			}
		}

		/// <summary>
		/// 拆分地形
		/// </summary>
		/// <param name="iLevel">层级(影响到左边缩进的余白)</param>
		private void SplitTerrains(int iLevel)
		{

			var level = iLevel;
			DrawMiddleGameObject(
				level, "地形根节点：", ref _terrainsRoot);
			var childCount = 0;
			if (!_terrainsRoot) return;
			childCount = _terrainsRoot.transform.childCount;
			// 若长宽地形个数不想等
			if (false == UtilsTools.IsSquare(childCount))
			{
				DrawLabel(level+1, "地形部署长宽不相同(不是 N x N 布局)!", Color.red);
			}
			else
			{
				// 保存地形Tile尺寸
				if (Math.Abs(ConfData.TileSize.x * ConfData.TileSize.y - childCount) > 0.0f)
				{
					ConfData.TileSize.x = (float)Math.Sqrt(childCount);
					ConfData.TileSize.y = ConfData.TileSize.x;
				}
					
				DrawMiddleLabel(level, "chunk拆分数：");
				++level;

				DrawLabel(level, "因为与到处高度图相关，拆分个数必须为2的N次幂(1~64)", Color.yellow);
				var lastSlicingCount = ConfData.chunkData.SlicingCount; 
				DrawMiddleSlider(
					level, "个数( N x N )：", ref lastSlicingCount,
					1.0f,64);
				if (!UtilsTools.IsPowerOf2((int)lastSlicingCount))
				{
					var error = string.Format("{0} 不是2的N次幂(1~64)！",
						(int) ConfData.chunkData.SlicingCount);
					DrawLabel(level+1, error, Color.red);
				}
				else
				{
					if (Math.Abs(lastSlicingCount - ConfData.chunkData.SlicingCount) > 0.0f)
					{
						// 有变化贼，晴空已有数据
						Settings.Clear();
						ConfData.chunkData.SlicingCount = lastSlicingCount;
						// 清空Debug用绘制节点
						ClearDebugNode();
						
						// 层级预览信息清空
						_layerPreviewTitles.Clear();
						_layerPreviewSelected.Clear();
					}
						
					// 分界线
					DrawLongLabel(0, "---------------");
					DrawSingleLongToggle(level, "是否预览拆分边界：", _chunkBoundPreviewVisiable,
						delegate(bool iValue) { _chunkBoundPreviewVisiable = iValue; });
					// 选择层级
					// 取得最高层级
					var maxLayerLevel = GetMaxLayerLevel((int)ConfData.chunkData.SlicingCount);
					// 层级菜单标题
					if (0 >= _layerPreviewSelected.Count)
					{
						for (var l = 0; l <= maxLayerLevel; ++l)
						{
							_layerPreviewTitles.Add(string.Format("层级{0}", l));
							_layerPreviewSelected.Add(false);
						}
					}
					if (_chunkBoundPreviewVisiable)
					{
						// 各层级显示
						DrawLongLabel(level+1, string.Format("当前拆分数可分 {0} 层：", maxLayerLevel+1));
						DrawMiddleMultiplyToggle(level+2, _layerPreviewTitles.ToArray(), _layerPreviewSelected.ToArray(),
							delegate(int l, bool iSelected) {
								_layerPreviewSelected[l] = iSelected;
								if (!iSelected) ClearDebugNode(l);
							});
						
						// 显示颜色设定
						var flg = false;
						for (var l = 0; l <= maxLayerLevel; ++l)
						{
							if(!_layerPreviewSelected[l]) continue;
							flg = true;
						}

						if (flg)
						{
							var layerPreview = ConfData.chunkData.Preview;
							DrawLongLabel(level+3, "颜色设定：");

							DrawMiddleColor(level+4, "边色", layerPreview.edgeColor,
								delegate(Color iColor)
								{
									layerPreview.edgeColor = iColor;
									// 重置所有边的颜色
									UtilsDraw.ResetAllCylinderColor(iColor, 
										(int)DrawType.TerrainChunkCylinder);
								});
							
							DrawMiddleColor(level+4, "顶点色", layerPreview.vertexColor,
								delegate(Color iColor)
								{
									layerPreview.vertexColor = iColor;
									// 重置所有边的颜色
									UtilsDraw.ResetAllVertexColor(iColor, 
										(int)DrawType.TerrainChunkVertex);
								});
							DrawMiddleSlider(level+4, "缩放", 
								layerPreview.Scale, delegate(float iValue)
								{
									layerPreview.Scale = iValue;
									UtilsDraw.ResetAllBoundsPreviewScale(
										iValue, (int)DrawType.TerrainChunkCylinder | (int)DrawType.TerrainChunkVertex);
								}, 
								1.0f, 50.0f);
							--level;
						}

						// 遍历根节点并取得包围盒
						if (null == _terrainsRoot) return;
						// 基准原点：子节点中最小的点为为基准原点(既：最左最前的为第一块地形Tile)
						// 其他的地形都以该地形位置准排布
						var baseOriginPos = GetOriginPosOfTerrainsFromTiles(_terrainsRoot.transform);
						
						var terrainTileSize = Vector3.zero;
						for (var i = 0; i < _terrainsRoot.transform.childCount; ++i)
						{
							var child = _terrainsRoot.transform.GetChild(i);
							if(null == child) continue;
							if(false == child.gameObject.activeInHierarchy) continue;
							
							// 地形Tile尺寸
							if (Vector3.zero.Equals(terrainTileSize))
							{
								terrainTileSize = TerrainsSettings.GetTerrainSize(child.gameObject);
							}
							
							// Tile Index
							var tileIndex = GetTerrainTileIndexByPos(
								baseOriginPos, child.transform.position,
								terrainTileSize);
							
							// 按照层级生成解析用的地形层级数据
							for (var layerIndex = 0; layerIndex <= maxLayerLevel; ++layerIndex)
							{
								// 当前层级的chunk尺寸
								var layerChunkSize = TerrainsSettings.GetTerrainChunkSize(
										terrainTileSize, maxLayerLevel, layerIndex);
								
								// 拆分当前层级的chunk数据
								Settings.ParserTerrainTileData(
									tileIndex, layerChunkSize, maxLayerLevel,
									layerIndex);
							}

							for (var l = 0; l <= maxLayerLevel; ++l)
							{
								if (!_layerPreviewSelected[l])
								{
									continue;
								}
								// 取得当前层级的chunk信息
								var chunks = Settings.GetChunkData(l);
								if(null == chunks || 0 >= chunks.Count) continue;
								var bounds = new List<Bounds>();
								// 以莫顿码为chunk索引
								var chunksIndex = new List<int>();
								foreach (var chunk in chunks)
								{
									bounds.Add(chunk.Bounds);
									chunksIndex.Add((int)chunk.Morton);
								}
								// 绘制Chunks包围盒
								var layerPreview = ConfData.chunkData.Preview;
								if (null == layerPreview) continue;
								UtilsDraw.DrawChunkBounds(
									bounds.ToArray(), chunksIndex.ToArray(),
									child.transform, 
									layerPreview.edgeColor, layerPreview.vertexColor, layerPreview.Scale, l);
							}
						}
						Settings.AutoSort();
					}
					else
					{
						ClearDebugNode();
					}

					DrawLongLabel(0, "---------------");
					
					DrawLongLabel(0, "拆分设定：");
					DrawLongLabel(1, "数据导入/导出目录：");
					DrawLabel(2, string.Format("Chunk : {0}", Settings.ChunkDataDir), Color.green);
					DrawLabel(2, string.Format("预制体 : {0}", Settings.PrefabsDataDir), Color.green);
					
					DrawSingleMiddleToggle(1, "仅处理静态物体:", ConfData.chunkData.StaticOnly,
						delegate(bool iValue) { ConfData.chunkData.StaticOnly = iValue; });

					// 地形导出层级
					DrawSelectList(1, "地形导出层级:", _layerPreviewTitles.ToArray(), ref _exportLayerLevel);
					
					// 预制体导出深度
					DrawMiddleSlider(1, "预制体导出深度:",
						ConfData.chunkData.PrefabsExportDepthLimit,
						delegate(float iValue) { ConfData.chunkData.PrefabsExportDepthLimit = (int)iValue; },
						1.0f, 15.0f);
					
					DrawSingleButton(0, "拆分场景",
						delegate()
						{
								
							// 显示处理进度条
							ProgressBarStart("拆分地形", "开始拆分...");
							
							// Chunk导出目录检测
							var dirInfo = new DirectoryInfo(Settings.ChunkDataDir);
							UtilsTools.CheckAndCreateDirByFullDir(dirInfo.FullName);

							// 清空保存目录，但不删除目录本身
							UtilsTools.ClearDirectory(TerrainsSettings.DataBaseDir, false);
							
							// 遍历根节点并取得包围盒
							if (null == _terrainsRoot) return;
							// 基准原点：子节点中最小的点为为基准原点(既：最左最前的为第一块地形Tile)
							// 其他的地形都以该地形位置准排布
							var baseOriginPos = GetOriginPosOfTerrainsFromTiles(_terrainsRoot.transform);
							
							var terrainTileSize = Vector3.zero;
							var maxCount = _terrainsRoot.transform.childCount * ConfData.chunkData.SlicingCount *
							               ConfData.chunkData.SlicingCount;
							var progressCount = 0;
							for (var i = 0; i < _terrainsRoot.transform.childCount; ++i)
							{
								
								var child = _terrainsRoot.transform.GetChild(i);
								if (null == child) continue;
								if (false == child.gameObject.activeInHierarchy) continue;
								
								// 地形Tile尺寸
								if (Vector3.zero.Equals(terrainTileSize))
								{
									terrainTileSize = TerrainsSettings.GetTerrainSize(child.gameObject);
								}
								
								// Tile Index
								var tileIndex = GetTerrainTileIndexByPos(
									baseOriginPos, child.transform.position,
									terrainTileSize);
								
								// 拆分地形
								TerrainsSplit.Instance.SplitTerrains(
									Settings.ChunkDataDir, child.gameObject, tileIndex, 
									maxLayerLevel,maxLayerLevel - _exportLayerLevel,
									delegate(string iStatusTxt, bool iStatusCount)
									{
										if (iStatusCount)
										{
											++progressCount;
										}
										// 计算进度
										var progress = progressCount / maxCount;
										ProgressBarUpdate(iStatusTxt, progress);
										this.Info("SplitTerrains:({0}) {1}", progress, iStatusTxt);
									});
							}
							
							// 初始化地形拆分用的预制体信息
							TerrainsSplit.Instance.InitPrefabsInfo(
								ConfData.chunkData.StaticOnly, ConfData.chunkData.PrefabsExportDepthLimit);

							ProgressBarUpdate(
								"地形拆分成功", 1.0f);
							// 到处此次地形拆分配置信息
							TerrainsSettings.GetInstance().ExportBySceneName(CurSceneName);
							ProgressBarClear(); 
							// 刷新
							AssetDatabase.Refresh();
						}, Color.yellow);
				}
			}

		}

		/// <summary>
		/// 从地形Tiles群中取得基准远点
		/// </summary>
		/// <param name="iParent">父对象</param>
		/// <returns>远点坐标</returns>
		private Vector3 GetOriginPosOfTerrainsFromTiles(Transform iParent)
		{
			// 基准原点：子节点中最小的点为为基准原点(既：最左最前的为第一块地形Tile)
			// 其他的地形都以该地形位置准排布
			var baseOriginPos = Vector3.zero;
			if (null == iParent || 0 >= iParent.childCount) return baseOriginPos;
			for (var i = 0; i < iParent.childCount; ++i)
			{
				var child = iParent.GetChild(i);
				if(null == child) continue;
				if(false == child.gameObject.activeInHierarchy) continue;
				if (Vector3.zero.Equals(baseOriginPos))
				{
					baseOriginPos = child.transform.position;
				}
				if (child.transform.position.x < baseOriginPos.x ||
				    child.transform.position.z < baseOriginPos.z)
				{
					baseOriginPos = child.transform.position;
				}
			}
			return baseOriginPos;
		}
		
		/// <summary>
		/// 取得地形最高层级
		///  备注:
		///   根据地形切割数量，计算地形最高层级
		/// </summary>
		/// <param name="iSlicingCount">地形切割数量</param>
		/// <returns>地形最高层级</returns>
		private int GetMaxLayerLevel(int iSlicingCount)
		{
			return (int)Math.Log(iSlicingCount, 2);
		}

		/// <summary>
		/// 取得地形Tile的索引
		/// </summary>
		/// <param name="iOriginBase">原点基准位置</param>
		/// <param name="iCurTilePos">当前地形的Tile位置</param>
		/// <param name="iTerrainTileSize">地形Tile的尺寸大小</param>
		/// <returns>地形Tile的索引(包含x和z轴方向的索引)</returns>
		private Vector2 GetTerrainTileIndexByPos(
			Vector3 iOriginBase, Vector3 iCurTilePos, 
			Vector3 iTerrainTileSize)
		{
			var delta = iCurTilePos - iOriginBase;
			return new Vector2(
				(int)(delta.x / iTerrainTileSize.x), 
				(int)(delta.z / iTerrainTileSize.z));
		}

		/// <summary>
		/// 导出地形（Chunk）
		/// </summary>
		/// <param name="iLevel">层级(影响到左边缩进的余白)</param>
		private void ExportChunks(int iLevel)
		{
			var level = iLevel;

			DrawSingleButton(0, "导出Prefabs",
				delegate()
				{
					// 自动生成Chunk
//					AutoCreateTerrains(ConfData.tileData);
				}, Color.yellow);
			
			DrawSingleButton(0, "导出拆分后的物体",
				delegate()
				{
					// 自动生成Chunk
//					AutoCreateTerrains(ConfData.tileData);
				}, Color.yellow);

		}

		/// <summary>
		/// 绘制WindowGUI.
		/// </summary>
		protected override void OnWindowGui()
		{
			base.OnWindowGui();
			var level = 0;

			// 初始化地形
			InitTerrains(level);
			
			DrawSingleButton(0, "清除地形辅助类脚本&空对象",
				delegate()
				{
					// 自动生成Chunk
//					AutoCreateTerrains(ConfData.tileData);
				}, Color.green);

		}
		
		/// <summary>
		/// 应用导入数据数据.
		/// </summary>
		/// <param name="iData">数据.</param>
		/// <param name="iForceClear">强制清空标志位.</param>
		protected override void ApplyImportData(TerrainEditorData iData, bool iForceClear)
		{
			if (null == iData) {
				return;
			}

			// 清空
			if (iForceClear) {
				Clear ();
			}
			
			// 设定导入信息
			ConfData.tileData.Preview.edgeColor = iData.tileData.Preview.edgeColor;
			ConfData.tileData.Preview.vertexColor = iData.tileData.Preview.vertexColor;
			ConfData.tileData.Preview.Scale = iData.tileData.Preview.Scale;
			
			// Chunk信息
			ConfData.chunkData.SlicingCount = iData.chunkData.SlicingCount;
			ConfData.chunkData.Preview.edgeColor = iData.chunkData.Preview.edgeColor;
			ConfData.chunkData.Preview.vertexColor = iData.chunkData.Preview.vertexColor;
			ConfData.chunkData.Preview.Scale = iData.chunkData.Preview.Scale;
			ConfData.chunkData.StaticOnly = iData.chunkData.StaticOnly;
			
			UtilsAsset.SetAssetDirty (this);
		}

#endregion

#region Create - Terrains

		/// <summary>
		/// 自动生成Chunk
		/// </summary>
		/// <param name="iData">Tile数据</param>
		private void AutoCreateTerrains(TerrainEditorData.TerrainTileData iData)
		{
			
			_terrainsRoot = new GameObject(TerrainsRootName);
			for (var x = 0; x < ConfData.TileSize.x; ++x)
			{
				for (var z = 0; z < ConfData.TileSize.y; ++z)
				{
					var terrainObj = new GameObject();
					terrainObj.name = string.Format("terrain_x{0}_z{1}", x, z);
					// 设定父节点
					terrainObj.transform.parent = _terrainsRoot.transform;
					var terrain = terrainObj.AddComponent<UnityEngine.Terrain>();
					var terrainCollider = terrainObj.AddComponent<TerrainCollider>();
					
					// 深拷贝地形数据
					var tmpData = UtilsTools.DeepCopy(iData.Data) as UnityEngine.TerrainData;
					
					// 设置地形
					if (terrain && tmpData)
					{
						// 设定尺寸(长x宽x高)
						var size = ConfData.tileData.TerrainSize;
						tmpData.size = size;
						
						terrain.terrainData = tmpData;
						
						// 设定位置
						// Z 方向
						var posX = size.x * x + size.x / 2;
						var posZ = size.z * z + size.z / 2;
						terrain.transform.localPosition = new Vector3(posX, 0.0f, posZ);
					}
					
					// 设置地形碰撞
					if (terrainCollider && tmpData)
					{
						terrainCollider.terrainData = tmpData;	
					}
				}
			}
		}

#endregion

		[MenuItem("Tools/Terrain/EditorWindow", false, 800)]
		static void ShowTerrainWindow()
		{
			ShowWindow("Terrain Editor");
		}
		
	}
}


