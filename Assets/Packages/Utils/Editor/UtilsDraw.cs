using System;
using System.Collections.Generic;
using Packages.Utils.Editor.Draw;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Packages.Utils.Editor
{
	/// <summary>
	/// 绘制工具类
	/// </summary>
	public static class UtilsDraw
	{
		/// <summary>
		/// 由于计算所造成的误差，导致前后计算的点有可能出现细微便宜
		/// 所以在比较前后两个圆柱体两端顶点是否一致时，
		/// 采用判断两点间距离是否在允许的最短距离内，
		/// 若是，贼判断为同一点
		/// </summary>
		internal const float MinEqualDistance = 1.0f;
		
		/// <summary>
		/// 游戏对象调试用标签
		/// </summary>
		public const string GameObjectTagDrawDebug = "DrawDebug";
		
		/// <summary>
		/// 绘制对象列表
		///   key : InstanceID
		///   Value : DrawObjBase
		/// </summary>
		private static Dictionary<int, DrawObjBase> drawList = new Dictionary<int, DrawObjBase>();
		
		/// <summary>
		/// 中心位置对齐校准：几何中心 -> 模型中心
		/// </summary>
		/// <param name="iTarget">目标对象</param>
		internal static void AlignPivotToCenter(Transform iTarget)
		{
			var target = iTarget;
			var postion = target.position;
			var rotation = target.rotation;
			var scale = target.localScale;
			target.position = Vector3.zero;
			target.rotation = Quaternion.Euler(Vector3.zero);
			target.localScale = Vector3.one;
 
 
			var center = Vector3.zero;
			var renders = target.GetComponentsInChildren<Renderer>();
			foreach (var child in renders){
				center += child.bounds.center;   
			}
			center /= target.GetComponentsInChildren<Transform>().Length; 
			var bounds = new Bounds(center,Vector3.zero);
			foreach (var child in renders){
				bounds.Encapsulate(child.bounds);   
			}
	
			target.position = postion;
			target.rotation = rotation;
			target.localScale = scale;
 
			foreach(Transform t in target){
				t.position -= bounds.center;
			}
			target.transform.position = bounds.center + target.position;
		}

		/// <summary>
		/// 中心位置对齐校准：几何中心 -> 模型中心
		/// </summary>
		/// <param name="iTarget">目标对象</param>
		internal static void AlignPivotToCenter(
			Vector3 iBoundCenter, Transform iTarget)
		{
			var target = iTarget;
			target.transform.position = iBoundCenter + target.position;
		}

		/// <summary>
		/// 判断绘制对象是不是已经被绘制了
		/// </summary>
		/// <param name="iWCenter">包围盒中心(世界坐标)</param>
		/// <param name="iSize">包围盒尺寸</param>
		/// <param name="iLayerIndex">层级索引</param>
		/// <returns>true:已绘制; false:未绘制;</returns>
		internal static bool IsDrawBoundsExist(
			Vector3 iWCenter, Vector3 iSize, int iLayerIndex)
		{
			if (Vector3.zero == iWCenter && Vector3.zero == iSize) return false;
			var isExist = false;
			foreach (var it in drawList)
			{
				if(iLayerIndex != it.Value.LayerIndex) continue;
				var objTmp = it.Value as DrawBounds;
				if(objTmp == null) continue;
				isExist = objTmp.Equal(iWCenter, iSize);
				if(isExist) break;
			}
			return isExist;
		}
		
		/// <summary>
		/// 判断绘制对象是不是已经被绘制了
		/// </summary>
		/// <param name="iWOrigin">圆点(世界坐标)</param>
		/// <param name="iRadius">半径</param>
		/// <param name="iLayerIndex">层级索引</param>
		/// <returns>true:已绘制; false:未绘制;</returns>
		internal static bool IsDrawSphereExist(
			Vector3 iWOrigin, float iRadius, int iLayerIndex)
		{
			if (Vector3.zero == iWOrigin && Math.Abs(iRadius) <= 0) return false;
			var isExist = false;
			var temp = drawList;
			foreach (var it in drawList)
			{
				if(iLayerIndex != it.Value.LayerIndex) continue;
				var objTmp = it.Value as DrawSphere;
				if(objTmp == null) continue;
				isExist = objTmp.Equal(iWOrigin, iRadius);
				if(isExist) 
					break;
			}
			return isExist;
		}
		
		/// <summary>
		/// 判断绘制对象是不是已经被绘制了
		/// </summary>
		/// <param name="iStart">开始顶点</param>
		/// <param name="iEnd">结束顶点</param>
		/// <param name="iLayerIndex">层索引</param>
		/// <returns>true:已绘制; false:未绘制;</returns>
		internal static bool IsDrawCylinerExist(
			Vector3 iStart, Vector3 iEnd, int iLayerIndex)
		{
			if (Vector3.zero == iStart && Vector3.zero == iEnd) return false;
			var isExist = false;
			foreach (var it in drawList)
			{
				if(iLayerIndex != it.Value.LayerIndex) continue;
				var objTmp = it.Value as DrawCylinder;
				if(objTmp == null) continue;
				isExist = objTmp.Equal(iStart, iEnd);
				if(isExist) break;
			}
			return isExist;
		}

		/// <summary>
		/// 保存绘制对象信息
		/// </summary>
		/// <param name="iObject">绘制对象</param>
		internal static void SaveDrawObject(DrawObjBase iObject)
		{
			if(null == iObject) return;
			if(null == iObject.Target) return;
			if(drawList.ContainsKey(iObject.Target.GetInstanceID())) return;
			drawList.Add(iObject.Target.GetInstanceID(), iObject);
		}

		/// <summary>
		/// 清空所有绘制列表对象
		/// </summary>
		/// <param name="iLayerIndex">层索引</param>
		public static void ClearAllDrawList(int iLayerIndex = -1)
		{
			var indexes = new List<int>();
			var tmp = drawList; 
			foreach (var it in drawList)
			{
				if(null == it.Value) continue;
				var tmpLI = it.Value.LayerIndex;
				var itTmp = it.Value;
				if (0 != tmpLI)
				{
					tmpLI = tmpLI;
				}

				if(-1 != iLayerIndex && iLayerIndex != it.Value.LayerIndex) continue;
				indexes.Add(it.Key);
				it.Value.Destroy();
			}

			// 移除相关信息
			if (0 >= indexes.Count) return;
			foreach (var idx in indexes)
			{
				drawList.Remove(idx);
			}
		}
		
		/// <summary>
		/// 重置所有包围盒预览缩放
		///   备注:只作坊包围盒预览边界&顶点, 包围盒自身大小不缩放。
		/// </summary>
		/// <param name="iScale">缩放</param>
		/// <param name="iTargetTypes">目标类型(允许位操作)</param>
		public static void ResetAllBoundsPreviewScale(
			float iScale, int iTargetTypes)
		{
			foreach (var it in drawList)
			{
				if(null == it.Value) continue;
				// 非指定类型，则跳过
				if(!UtilsBytes.CheckByte(iTargetTypes, (int)it.Value.Type)) continue;
				// 包围盒除外
				it.Value.ResetScale(iScale);
			}
		}

		/// <summary>
		/// 创建网格线2D
		///   备注：
		///   1) 左下为起点
		///   2) 绘制顺序为逆时针 
		/// </summary>
		/// <param name="iParent">父节点</param>
		/// <param name="iV1">顶点1</param>
		/// <param name="iV2">顶点2</param>
		/// <param name="iV3">顶点3</param>
		/// <param name="iV4">顶点4</param>
		/// <param name="iColor">颜色</param>
		/// <param name="iIndex">线索引</param>
		/// <returns>网格线对象</returns>
		internal static GameObject CreateMesh2D(
			Transform iParent,
			Vector3 iV1, Vector3 iV2, Vector3 iV3, Vector3 iV4,
			Color iColor, int iIndex)
		{
			var lineNodeName = string.Format("Mesh_{0}", iIndex);
			var line = new GameObject(lineNodeName);
			if (null != iParent)
			{
				line.transform.parent = iParent.transform;
			}
			line.transform.localPosition = new Vector3(0.0f, 0.0f, 0.0f);

			var meshRenderer = line.AddComponent<MeshRenderer>();
			if (null == meshRenderer)
			{
				Object.DestroyImmediate(line);
				return null;
			}
			// 设定材质
			var material = AssetDatabase.LoadAssetAtPath<Material>("Assets/Packages/Dynamic/Terrains/Editor/Material/DrawLine.mat");
			if (null != material)
			{
				meshRenderer.sharedMaterial = material;
				meshRenderer.sharedMaterial.SetColor("_DrawColor", iColor);;
			}

			var meshFilter = line.AddComponent<MeshFilter>();
			if (null == meshFilter)
			{
				Object.DestroyImmediate(line);
				return null;
			}
			var mesh = new Mesh();
			mesh.vertices = new Vector3[]{iV1,iV2,iV3,iV4};
			mesh.triangles = new int[]{2,1,0,0,3,2};
			meshFilter.mesh = mesh;
			return line;
		}
		
		/// <summary>
		/// 创建长方体有别于Cude可以控制大小的,
		/// 但是没有地面和顶面
		///   备注：
		///   左下为起点，以逆时针方向，从上到下顺序绘制
		/// </summary>
		/// <param name="iParent">父节点</param>
		/// <param name="iV1">顶点1</param>
		/// <param name="iV2">顶点2</param>
		/// <param name="iV3">顶点3</param>
		/// <param name="iV4">顶点4</param>
		/// <param name="iV5">顶点5</param>
		/// <param name="iV6">顶点6</param>
		/// <param name="iV7">顶点7</param>
		/// <param name="iV8">顶点8</param>
		/// <param name="iColor">颜色</param>
		/// <param name="iIndex">线索引</param>
		/// <returns>网格线对象</returns>
		internal static GameObject CreateCuboid(
			Transform iParent,
			Vector3 iV1, Vector3 iV2, Vector3 iV3, Vector3 iV4,
			Vector3 iV5, Vector3 iV6, Vector3 iV7, Vector3 iV8,
			Color iColor, int iIndex)
		{
			var lineNodeName = string.Format("Cuboid_{0}", iIndex);
			var line = new GameObject(lineNodeName);
			if (null != iParent)
			{
				line.transform.parent = iParent.transform;
			}
			line.transform.localPosition = new Vector3(0.0f, 0.0f, 0.0f);

			var meshRenderer = line.AddComponent<MeshRenderer>();
			if (null == meshRenderer)
			{
				Object.DestroyImmediate(line);
				return null;
			}
			
			// 设定材质
			var material = AssetDatabase.LoadAssetAtPath<Material>("Assets/Packages/Dynamic/Terrains/Editor/Material/DrawLine.mat");
			if (null != material)
			{
				meshRenderer.sharedMaterial = material;
				meshRenderer.sharedMaterial.SetColor("_DrawColor", iColor);;
			}

			var meshFilter = line.AddComponent<MeshFilter>();
			if (null == meshFilter)
			{
				Object.DestroyImmediate(line);
				return null;
			}
			var mesh = new Mesh();
			mesh.vertices = new[]{ iV1, iV2, iV3, iV4, iV5, iV6, iV7, iV8 };
			mesh.triangles = new[] {
				0, 1, 5, 5, 4, 0,    // 外侧面
				1, 2, 6, 6, 5, 1,    // 右侧面
				2, 6, 7, 7, 3, 2,    // 里侧面
				0, 3, 7, 7, 4, 0     // 左侧面
			};
			meshFilter.mesh = mesh;
			return line;
		}

		/// <summary>
		/// 取得地形的包围盒
		/// </summary>
		/// <param name="iTarget">目标地形</param>
		/// <returns>包围盒</returns>
		internal static Bounds GetLocalTerrainBounds(GameObject iTarget)
		{
			var terrainCollider = iTarget.gameObject.GetComponent<UnityEngine.Terrain>();
			if(null == terrainCollider) return new Bounds(new Vector3(0.0f, 0.0f, 0.0f), new Vector3(0.0f, 0.0f, 0.0f));
			var terrainData = terrainCollider.terrainData;
			var terrainSize = new Vector3(terrainData.size.x, terrainData.size.y, terrainData.size.z);
			var center = new Vector3(terrainData.size.x / 2, terrainData.size.y / 2, terrainData.size.z / 2);
			var bounds = new Bounds(center, terrainSize);
			return bounds;
 
		}

		/// <summary>
		/// 取得地形Chunks的包围盒列表
		/// </summary>
		/// <param name="iTarget">目标地形</param>
		/// <param name="iChunkWidth">Chunk宽度</param>
		/// <param name="iChunkLength">Chunk长度</param>
		/// <returns>Chunks的包围盒列表</returns>
		internal static Bounds[] GetLocalChunkBounds(
			GameObject iTarget, float iChunkWidth, float iChunkLength)
		{
			var terrainCollider = iTarget.gameObject.GetComponent<TerrainCollider>();
			if(null == terrainCollider) return null;
			var terrainData = terrainCollider.terrainData;
			
			// 计算行列高数
			var rCount = (0 == iChunkWidth) ? 0 : (int) terrainData.size.x / (int) iChunkWidth;
			var cCount = (0 == iChunkLength) ? 0 : (int) terrainData.size.z / (int) iChunkLength;
			var count = rCount * cCount;
			if (0 == count) return null;
			var curPosX = 0.0f; 
			var curPosZ = 0.0f;
			var boundsArray = new Bounds[rCount * cCount];
			for (var r = 0; r < rCount; ++r)
			{
				curPosX += (0 == r) ? iChunkWidth / 2 : iChunkWidth;
				curPosZ = 0.0f;
				for (var c = 0; c < cCount; ++c)
				{
					curPosZ += (0 == c) ? iChunkLength / 2 : iChunkLength; 
					var center = new Vector3(curPosX, terrainData.size.y / 2, curPosZ);
					var chunkSize = new Vector3(iChunkWidth, terrainData.size.y, iChunkLength);
					var idx = r * cCount + c;
					boundsArray[idx] = new Bounds(center, chunkSize);
				}
			}
			return boundsArray;
		}
		
		/// <summary>
		/// 绘制地形包围盒
		/// </summary>
		/// <param name="iTerrain">地形对象</param>
		/// <param name="iParent">父节点</param>
		/// <param name="iEdgeColor">颜色:边</param>
		/// <param name="iVertexColor">颜色:顶点</param>
		/// <param name="iScale">缩放</param>
		/// <param name="iIndex">索引</param>
		/// <param name="iLayerIndex">层级索引</param>
		public static void DrawTerrainBounds(
			GameObject iTerrain, Transform iParent, 
			Color iEdgeColor,Color iVertexColor,
			float iScale = 1.0f, int iIndex = -1, int iLayerIndex = -1)
		{
			var bounds = GetLocalTerrainBounds(iTerrain);
			var wCenter = null == iParent ? bounds.center : iParent.TransformPoint(bounds.center);
			if (IsDrawBoundsExist(wCenter, bounds.size, iLayerIndex)) return;
			// 开始绘制
			var drawBound = new DrawBounds(
				DrawType.Bounds, bounds, iParent, 
				iEdgeColor, iVertexColor, 
				iScale, iIndex, iLayerIndex);
			// 绘制网格
			drawBound.DrawMesh(iLayerIndex);
			// 保存已绘制的对象
			SaveDrawObject(drawBound);
		}

		/// <summary>
		/// 绘制包围盒
		/// </summary>
		/// <param name="iBounds">包围盒</param>
		/// <param name="iParent">父亲节点</param>
		/// <param name="iEdgeColor">颜色:边</param>
		/// <param name="iVertexColor">颜色:顶点</param>
		/// <param name="iIndex">包围盒索引</param>
		/// <param name="iBorder">包围盒线粗细度</param>
		/// <param name="iLayerIndex">层索引</param>
		/// <returns>已绘制对象</returns>
		public static void DrawBounds(
			Bounds iBounds, Transform iParent, 
			Color iEdgeColor,Color iVertexColor,
			float iBorder = 1.0f, int iIndex = -1, int iLayerIndex = -1)
		{
			var wCenter = null == iParent ? iBounds.center : iParent.TransformPoint(iBounds.center);
			if (IsDrawBoundsExist(wCenter, iBounds.size, iLayerIndex)) return;
			// 开始绘制
			var drawBound = new DrawBounds(
				DrawType.Bounds, iBounds, iParent, 
				iEdgeColor, iVertexColor,
				iBorder, iIndex, iLayerIndex);
			// 绘制网格
			drawBound.DrawMesh(iLayerIndex);
			// 保存已绘制的对象
			SaveDrawObject(drawBound);
		}
		
		/// <summary>
		/// 绘制包围盒
		/// </summary>
		/// <param name="iBoundsList">包围盒列表</param>
		/// <param name="iIndexes">包围盒索引列表</param>
		/// <param name="iParent">父亲节点</param>
		/// <param name="iEdgeColor">颜色:边</param>
		/// <param name="iVertexColor">颜色:顶点</param>
		/// <param name="iScale">缩放</param>
		/// <param name="iLayerIndex">层索引</param>
		/// <returns>已绘制对象</returns>
		public static void DrawChunkBounds(
			Bounds[] iBoundsList, int[] iIndexes, 
			Transform iParent, Color iEdgeColor,Color iVertexColor, 
			float iScale = 1.0f, int iLayerIndex = -1)
		{
			if(null == iBoundsList || 0 >= iBoundsList.Length) return;
			if(null == iIndexes || 0 >= iIndexes.Length) return;
			if(iBoundsList.Length != iIndexes.Length) return;
			for(var i = 0; i < iBoundsList.Length; ++i)
			{
				var bounds = iBoundsList[i];
				var index = iIndexes[i];
				var wCenter = null == iParent ? bounds.center : iParent.TransformPoint(bounds.center);
				if (IsDrawBoundsExist(wCenter, bounds.size, iLayerIndex)) return;
				// 开始绘制
				var drawBound = new DrawTerrainChunkBounds(
					bounds, iParent, 
					iEdgeColor, iVertexColor,
					iScale, index, iLayerIndex);
				// 绘制网格
				drawBound.DrawMesh(iLayerIndex);
				// 保存已绘制的对象
				SaveDrawObject(drawBound);
			}
		}

		/// <summary>
		/// 重置已绘制边的颜色
		/// </summary>
		/// <param name="iColor">颜色</param>
		/// <param name="iTargetTypes">目标类型(允许位操作)</param>
		public static void ResetAllCylinderColor(Color iColor, int iTargetTypes)
		{
			foreach (var it in drawList)
			{
				if(null == it.Value) continue;
				if(!UtilsBytes.CheckByte(iTargetTypes, (int)it.Value.Type)) continue;
				var meshRenderers = it.Value.Target.GetComponentsInChildren<MeshRenderer>();
				if (null == meshRenderers || 0 >= meshRenderers.Length) continue;
				foreach (var renderer in meshRenderers)
				{
					if (null == renderer) continue;
					if(null == renderer.sharedMaterial) continue;
					renderer.sharedMaterial.SetColor("_DrawColor", iColor);
					break;
				}
			}
		}

		/// <summary>
		/// 重置已绘顶点的颜色
		/// </summary>
		/// <param name="iColor">颜色</param>
		/// <param name="iTargetTypes">目标类型(允许位操作)</param>
		public static void ResetAllVertexColor(Color iColor, int iTargetTypes)
		{
			foreach (var it in drawList)
			{
				if(null == it.Value) continue;
				if(!UtilsBytes.CheckByte(iTargetTypes, (int)it.Value.Type)) continue;
				var meshRenderers = it.Value.Target.GetComponentsInChildren<MeshRenderer>();
				if (null == meshRenderers || 0 >= meshRenderers.Length) continue;
				foreach (var renderer in meshRenderers)
				{
					if (null == renderer) continue;
					if(null == renderer.sharedMaterial) continue;
					renderer.sharedMaterial.SetColor("_DrawColor", iColor);
					break;
				}
			}
		}
	}
}


