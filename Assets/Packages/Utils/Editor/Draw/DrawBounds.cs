using Packages.Dynamic.Terrains;
using UnityEngine;

namespace Packages.Utils.Editor.Draw
{
	
	/// <summary>
	/// 绘制对象：包围盒
	/// </summary>
	internal class DrawBounds : DrawObjBase {

		/// <summary>
		/// 颜色:边
		/// </summary>
		protected Color edgeColor
		{
			get { return color; }
			set { color = value; }
		}
		
		/// <summary>
		/// 颜色:顶点
		/// </summary>
		protected Color vertexColor;

		/// <summary>
		/// 构造函数
		/// </summary>
		/// <param name="iType">绘制类型</param>
		/// <param name="iInfo">包围盒信息</param>
		/// <param name="iParent">父对象</param>
		/// <param name="iEdgeColor">颜色:边</param>
		/// <param name="iVertexColor">颜色:顶点</param>
		/// <param name="iScale">缩放</param>
		/// <param name="iIndex">索引</param>
		/// <param name="iLayerIndex">层索引</param>
		public DrawBounds(
			DrawType iType, Bounds iInfo, Transform iParent, 
			Color iEdgeColor, Color iVertexColor,
			float iScale = 1.0f, int iIndex = -1, int iLayerIndex = -1)
		{
			Type = iType;
			Info = iInfo;
			LayerIndex = iLayerIndex;

			parent = iParent;
			wCenter = parent.TransformPoint(iInfo.center);
			edgeColor = iEdgeColor;
			vertexColor = iVertexColor;
			
			index = iIndex;
			scale = iScale;
		}

		/// <summary>
		/// 包围盒信息
		/// </summary>
		public Bounds Info;

		/// <summary>
		/// 包围盒中心(世界坐标)
		/// </summary>
		private Vector3 wCenter;

		/// <summary>
		/// 比较函数
		/// </summary>
		/// <param name="iWCenter">包围盒中心(世界坐标)</param>
		/// <param name="iSize">包围盒尺寸</param>
		/// <returns>true:相等; false:不相等;</returns>
		public bool Equal(Vector3 iWCenter, Vector3 iSize)
		{
//			return wCenter == iWCenter && Info.size == iSize;
			return Vector3.Distance(wCenter, iWCenter) <= UtilsDraw.MinEqualDistance &&
			       Vector3.Distance(Info.size, iSize) <= UtilsDraw.MinEqualDistance;
		}

		/// <summary>
		/// 绘制网格
		/// </summary>
		/// <param name="iTagIndex">标签索引</param>
		public override void DrawMesh(int iTagIndex = -1)
		{
			Target = new GameObject(GetTargetNm());
			if (null == Target) return;
			
			// 中心位置对齐校准：几何中心 -> 模型中心
			UtilsDraw.AlignPivotToCenter(Target.transform);
			var tagIndex = (-1 == iTagIndex) ? LayerIndex : iTagIndex;
			Target.tag = (-1 == tagIndex)
				? UtilsDraw.GameObjectTagDrawDebug
				: string.Format("{0}_{1}", UtilsDraw.GameObjectTagDrawDebug, tagIndex);
			if (null != parent) Target.transform.parent = parent;
			var centerPos = Info.center;
			Target.transform.localPosition = new Vector3(0.0f, 0.0f, 0.0f);

			// 顶点绘制顺序以从下到上逆时针绘制
			var vertex1 = centerPos;
			vertex1.x -= Info.size.x / 2;
			vertex1.y -= Info.size.y / 2;
			vertex1.z -= Info.size.z / 2;
			
			var vertex2 = centerPos;
			vertex2.x += Info.size.x / 2;
			vertex2.y -= Info.size.y / 2;
			vertex2.z -= Info.size.z / 2;
			
			var vertex3 = centerPos;
			vertex3.x += Info.size.x / 2;
			vertex3.y -= Info.size.y / 2;
			vertex3.z += Info.size.z / 2;
			
			var vertex4 = centerPos;
			vertex4.x -= Info.size.x / 2;
			vertex4.y -= Info.size.y / 2;
			vertex4.z += Info.size.z / 2;
			
			var vertex5 = centerPos;
			vertex5.x -= Info.size.x / 2;
			vertex5.y += Info.size.y / 2;
			vertex5.z -= Info.size.z / 2;
			
			var vertex6 = centerPos;
			vertex6.x += Info.size.x / 2;
			vertex6.y += Info.size.y / 2;
			vertex6.z -= Info.size.z / 2;
			
			var vertex7 = centerPos;
			vertex7.x += Info.size.x / 2;
			vertex7.y += Info.size.y / 2;
			vertex7.z += Info.size.z / 2;
			
			var vertex8 = centerPos;
			vertex8.x -= Info.size.x / 2;
			vertex8.y += Info.size.y / 2;
			vertex8.z += Info.size.z / 2;
			
			// 绘制球体
			var layerIndex = (-1 == iTagIndex) ? LayerIndex : iTagIndex;
			
			// line1
			DrawEdge(vertex1, vertex2, Vector3.right, 1, layerIndex);
			
			// Line2
			DrawEdge(vertex2, vertex3, Vector3.forward, 2, layerIndex);
			
			// Line3
			DrawEdge(vertex3, vertex4, Vector3.left, 3, layerIndex);
			
			// Line4
			DrawEdge(vertex4, vertex1, Vector3.back, 4, layerIndex);
			
			// Line5
			DrawEdge(vertex1, vertex5, Vector3.up, 5, layerIndex);
			
			// Line6
			DrawEdge(vertex2, vertex6, Vector3.up, 6, layerIndex);
			
			// Line7
			DrawEdge(vertex3, vertex7, Vector3.up, 7, layerIndex);
			
			// Line8
			DrawEdge(vertex4, vertex8, Vector3.up, 8, layerIndex);
			
			// Line9
			DrawEdge(vertex5, vertex6, Vector3.right, 9, layerIndex);
			
			// Line10
			DrawEdge(vertex6, vertex7, Vector3.forward, 10, layerIndex);
			
			// Line11
			DrawEdge(vertex7, vertex8, Vector3.left, 11, layerIndex);
			
			// Line12
			DrawEdge(vertex8, vertex5, Vector3.back, 12, layerIndex);
			
			// sphere1
			DrawVertex(vertex1, 1, layerIndex);
			
			// sphere2
			DrawVertex(vertex2, 2, layerIndex);
			
			// sphere3
			DrawVertex(vertex3, 3, layerIndex);
			
			// sphere4
			DrawVertex(vertex4, 4, layerIndex);
			
			// sphere5
			DrawVertex(vertex5, 5, layerIndex);
			
			// sphere6
			DrawVertex(vertex6, 6, layerIndex);
			
			// sphere7
			DrawVertex(vertex7, 7, layerIndex);
			
			// sphere8
			DrawVertex(vertex8, 8, layerIndex);


		}

		/// <summary>
		/// 绘制包围盒顶点
		/// </summary>
		/// <param name="iOrigin">圆点</param>
		/// <param name="iIndex">索引</param>
		/// <param name="iLayerIndex">层级索引</param>
		private void DrawVertex(
			Vector3 iOrigin, int iIndex, int iLayerIndex)
		{
			var sphereOrigin = iOrigin;
			sphereOrigin.y = iIndex <= 4 ? 0.0f : Info.size.y;
			var wSphereOrigin = Target.transform.TransformPoint(sphereOrigin);
			if (UtilsDraw.IsDrawSphereExist(wSphereOrigin, 2.0f, iLayerIndex)) return;
			var sphere = CreateVertex(
				sphereOrigin, 2.0f, vertexColor, 
				Target.transform, scale, iIndex, iLayerIndex);
			// 绘制
			sphere.DrawMesh(iLayerIndex);
			// 保存已绘制的对象
			UtilsDraw.SaveDrawObject(sphere);
		}

		/// <summary>
		/// 创建包围盒顶点
		/// </summary>
		/// <param name="iOrigin">圆点</param>
		/// <param name="iRadius">半径</param>
		/// <param name="iColor">颜色</param>
		/// <param name="iParent">父节点</param>
		/// <param name="iScale">缩放</param>
		/// <param name="iIndex">索引</param>
		/// <param name="iLayerIndex">层索引</param>
		/// <returns>包围盒顶点</returns>
		protected virtual DrawSphere CreateVertex(
			Vector3 iOrigin, float iRadius, Color iColor,
			Transform iParent, float iScale, 
			int iIndex = -1, int iLayerIndex = -1)
		{
			return new DrawSphere(
				DrawType.Sphere, iOrigin, iRadius, iColor, 
				iParent, iScale, iIndex, iLayerIndex);
		}

		/// <summary>
		/// 绘制包围盒边
		/// </summary>
		/// <param name="iStart">开始坐标</param>
		/// <param name="iEnd">结束坐标</param>
		/// <param name="iRotationTargetDir">旋转目标方向</param>
		/// <param name="iIndex">索引</param>
		/// <param name="iLayerIndex">层级索引</param>
		private void DrawEdge(
			Vector3 iStart, Vector3 iEnd, 
			Vector3 iRotationTargetDir, int iIndex, int iLayerIndex)
		{
			if(null == Target) return;
			var wStart = Target.transform.TransformPoint(iStart); 
			var wEnd = Target.transform.TransformPoint(iEnd);
			// 判断是否已经绘制
			if (UtilsDraw.IsDrawCylinerExist(wStart, wEnd, iLayerIndex)) return;
			// 新建绘制对象
			var cylinder = CreateEdge(
				iStart, iEnd, iRotationTargetDir,
				Target.transform, scale, edgeColor, 1.0f, 
				iIndex, iLayerIndex);
			// 绘制
			cylinder.DrawMesh(iLayerIndex);
			// 保存已绘制的对象
			UtilsDraw.SaveDrawObject(cylinder);
		}

		/// <summary>
		/// 创建包围盒边
		/// </summary>
		/// <param name="iStart">开始顶点</param>
		/// <param name="iEnd">结束顶点</param>
		/// <param name="iRotationTargetDir">目标方向</param>
		/// <param name="iParent">父节点</param>
		/// <param name="iScale">缩放</param>
		/// <param name="iColor">颜色</param>
		/// <param name="iRadius">半径(因为暂时化成四棱锥了，所以是边长)</param>
		/// <param name="iIndex">索引</param>
		/// <param name="iLayerIndex">层索引</param>
		/// <returns>包围盒边对象</returns>
		protected virtual DrawCylinder CreateEdge(
			Vector3 iStart, Vector3 iEnd, 
			Vector3 iRotationTargetDir, Transform iParent,
			float iScale, Color iColor, 
			float iRadius = 1.0f, int iIndex = -1, int iLayerIndex = -1)
		{
			return new DrawCylinder(
				DrawType.Cylinder, iStart, iEnd, iRotationTargetDir,
				iParent, iScale, iColor, iRadius, iIndex, iLayerIndex);
		}
		
		/// <summary>
		/// 取得游戏对象名
		/// </summary>
		/// <returns></returns>
		public override string GetTargetNm()
		{
			return string.Format("bounds_{0}", index);
		}
	}
	
	/// <summary>
	/// 绘制对象：地形Chunk包围盒
	/// </summary>
	internal class DrawTerrainChunkBounds : DrawBounds {
		
		/// <summary>
		/// 构造函数
		/// </summary>
		/// <param name="iInfo">包围盒信息</param>
		/// <param name="iParent">父对象</param>
		/// <param name="iEdgeColor">颜色:边</param>
		/// <param name="iVertexColor">颜色:顶点</param>
		/// <param name="iIndex">索引</param>
		/// <param name="iScale">缩放</param>
		/// <param name="iLayerIndex">层索引</param>
		public DrawTerrainChunkBounds(Bounds iInfo, Transform iParent, 
			Color iEdgeColor, Color iVertexColor,
			float iScale = 1.0f, int iIndex = -1, int iLayerIndex = -1) : base (
			DrawType.TerrainChunkBounds, iInfo, iParent,
			iEdgeColor, iVertexColor, iScale, iIndex, iLayerIndex) {
		}

		/// <summary>
		/// 创建包围盒顶点
		/// </summary>
		/// <param name="iOrigin">圆点</param>
		/// <param name="iRadius">半径</param>
		/// <param name="iColor">颜色</param>
		/// <param name="iParent">父节点</param>
		/// <param name="iScale">缩放</param>
		/// <param name="iIndex">索引</param>
		/// <param name="iLayerIndex">层索引</param>
		/// <returns>包围盒顶点</returns>
		protected override DrawSphere CreateVertex(
			Vector3 iOrigin, float iRadius, Color iColor,
			Transform iParent, float iScale, int iIndex = -1, int iLayerIndex = -1)
		{
			return new DrawTerrainChunkVertex(
				iOrigin, iRadius, iColor, 
				iParent, iScale, iIndex, iLayerIndex);
		}

		/// <summary>
		/// 创建包围盒边
		/// </summary>
		/// <param name="iStart">开始顶点</param>
		/// <param name="iEnd">结束顶点</param>
		/// <param name="iRotationTargetDir">目标方向</param>
		/// <param name="iParent">父节点</param>
		/// <param name="iScale">缩放</param>
		/// <param name="iColor">颜色</param>
		/// <param name="iRadius">半径(因为暂时化成四棱锥了，所以是边长)</param>
		/// <param name="iIndex">索引</param>
		/// <param name="iLayerIndex">层索引</param>
		/// <returns>包围盒边对象</returns>
		protected override DrawCylinder CreateEdge(
			Vector3 iStart, Vector3 iEnd, 
			Vector3 iRotationTargetDir, Transform iParent,
			float iScale, Color iColor,
			float iRadius = 1.0f, int iIndex = -1, int iLayerIndex = -1)
		{
			return new DrawTerrainChunkCylinder(
				iStart, iEnd, iRotationTargetDir,
				Target.transform, scale, edgeColor, iRadius, 
				iIndex, iLayerIndex);
		}
		
		/// <summary>
		/// 取得游戏对象名
		/// </summary>
		/// <returns></returns>
		public override string GetTargetNm()
		{
			return string.Format(TerrainsConst.CHUNK_NM_FORMAT, index);
		}

	}
}

