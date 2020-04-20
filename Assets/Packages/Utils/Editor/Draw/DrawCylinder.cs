using System;
using UnityEngine;

namespace Packages.Utils.Editor.Draw
{
	
	/// <summary>
	/// 绘制对象：圆柱体（目前暂时以四棱柱代替）
	/// </summary>
	internal class DrawCylinder : DrawObjBase
	{
		/// <summary>
		/// 构造函数
		/// </summary>
		/// <param name="iType">绘制类型</param>
		/// <param name="iStart">开始顶点</param>
		/// <param name="iEnd">结束顶点</param>
		/// <param name="iTargetDir">目标方向</param>
		/// <param name="iParent">父节点</param>
		/// <param name="iScale">缩放</param>
		/// <param name="iColor">颜色</param>
		/// <param name="iRadius">半径(因为暂时化成四棱锥了，所以是边长)</param>
		/// <param name="iIndex">索引</param>
		/// <param name="iLayerIndex">层索引</param>
		public DrawCylinder(
			DrawType iType, Vector3 iStart, Vector3 iEnd, 
			Vector3 iTargetDir, Transform iParent,
			float iScale, Color iColor,
			float iRadius = 1.0f, int iIndex = -1, int iLayerIndex = -1)
		{
			Start = iStart;
			End = iEnd;
			TargetDir = iTargetDir;
			Type = iType;
			LayerIndex = iLayerIndex;
			
			parent = iParent;
			scale = iScale;
			wStart = iParent.TransformPoint(iStart);
			wEnd = iParent.TransformPoint(iEnd);

			color = iColor;
			index = iIndex;
			radius = iRadius;
		}

		/// <summary>
		/// 起点(本地坐标)
		/// </summary>
		public Vector3 Start;
		/// <summary>
		/// 起点(世界坐标)
		/// </summary>
		private Vector3 wStart;
		
		/// <summary>
		/// 终点(本地坐标)
		/// </summary>
		public Vector3 End;
		/// <summary>
		/// 终点(世界坐标)
		/// </summary>
		private Vector3 wEnd;

		/// <summary>
		/// 目标方向
		/// </summary>
		/// <returns></returns>
		protected Vector3 TargetDir;
		
		/// <summary>
		/// 半径(因为暂时化成四棱锥了，所以是边长)
		/// </summary>
		private float radius;

		/// <summary>
		/// 长度
		/// </summary>
		private float length;

		/// <summary>
		/// 比较函数
		/// </summary>
		/// <param name="iWStart">开始顶点(世界坐标)</param>
		/// <param name="iWEnd">结束顶点(世界坐标)</param>
		/// <returns>true:相等; false:不相等;</returns>
		public bool Equal(Vector3 iWStart, Vector3 iWEnd)
		{
			// 起始&终点反过来也认为是同一根线（无方向）
//			return (wStart == iWStart && wEnd == iWEnd) || (wStart == iWEnd && wEnd == iWStart);
			return (Vector3.Distance(wStart, iWStart) <= UtilsDraw.MinEqualDistance && Vector3.Distance(wEnd, iWEnd) <= UtilsDraw.MinEqualDistance) || 
			       (Vector3.Distance(wStart, iWEnd) <= UtilsDraw.MinEqualDistance && Vector3.Distance(wEnd, iWStart) <= UtilsDraw.MinEqualDistance);
		}
		
		/// <summary>
		/// 绘制网格
		/// </summary>
		/// <param name="iTagIndex">标签索引</param>
		public override void DrawMesh(int iTagIndex = -1)
		{
			// 亮点之间的距离
			length = Vector3.Distance(Start, End); 
			// 0距离
			if (0 >= length) return;
			
			// 3D线主体
			var name = GetTargetNm();
			Target = new GameObject(name);
			if (null == Target) return;
			var tagIndex = (-1 == iTagIndex) ? LayerIndex : iTagIndex;
			Target.tag = (-1 == tagIndex)
				? UtilsDraw.GameObjectTagDrawDebug
				: string.Format("{0}_{1}", UtilsDraw.GameObjectTagDrawDebug, tagIndex);
			if (null != parent) Target.transform.parent = parent.transform;

			var lineWidthTmp = radius;
			// 现新建一个以原点为基准，沿着Y轴竖直向上，长&宽各为1，
			// 高度为开始点与结束点之间距离的长方体作为3D线的主体
			// 从底部左下角开始沿着逆时针方向，
			// 从下到上的风别为1，2，3，4，5，6，7，8
			var point1 = new Vector3(-lineWidthTmp / 2, 0.0f, -lineWidthTmp/2);
			var point2 = new Vector3(lineWidthTmp / 2, 0.0f, -lineWidthTmp / 2);
			var point3 = new Vector3(lineWidthTmp / 2, 0.0f, lineWidthTmp / 2);
			var point4 = new Vector3(-lineWidthTmp / 2, 0.0f, lineWidthTmp / 2);
			var point5 = new Vector3(-lineWidthTmp / 2, length, -lineWidthTmp / 2);
			var point6 = new Vector3(lineWidthTmp / 2, length, -lineWidthTmp / 2);
			var point7 = new Vector3(lineWidthTmp / 2, length, lineWidthTmp / 2);
			var point8 = new Vector3(-lineWidthTmp / 2, length, lineWidthTmp / 2);

			UtilsDraw.CreateCuboid(
				Target.transform,
				point1, point2, point3, point4,
				point5, point6, point7, point8,
				color, index);

			// 中心位置对齐校准：几何中心 -> 模型中心
			UtilsDraw.AlignPivotToCenter(Target.transform);
			// 将世界坐标转换到本地坐标
			var localPos = (Start + End) / 2;
			Target.transform.localPosition = localPos;
			
			// 缩放
			Target.transform.localScale = new Vector3(scale, 1.0f, scale);
			
			// 旋转
			Target.transform.localRotation = Quaternion.FromToRotation(
				new Vector3(0.0f, 1.0f, 0.0f), TargetDir);
		}
		
		/// <summary>
		/// 重置缩放
		///   备注：x,y,z等比例缩放
		/// </summary>
		/// <param name="iScale">缩放</param>
		public override void ResetScale(float iScale)
		{
			if(null == Target) return;
			if(scale == iScale) return;
			scale = iScale;
			Target.transform.localScale = new Vector3(iScale, 1.0f, iScale);
		}
	}

	/// <summary>
	/// 绘制对象：地形Chunk圆柱体（目前暂时以四棱柱代替）
	/// </summary>
	internal class DrawTerrainChunkCylinder : DrawCylinder
	{
		/// <summary>
		/// 构造函数
		/// </summary>
		/// <param name="iStart">开始顶点</param>
		/// <param name="iEnd">结束顶点</param>
		/// <param name="iTargetDir">目标方向</param>
		/// <param name="iParent">父节点</param>
		/// <param name="iScale">缩放</param>
		/// <param name="iColor">颜色</param>
		/// <param name="iRadius">半径(因为暂时化成四棱锥了，所以是边长)</param>
		/// <param name="iIndex">索引</param>
		/// <param name="iLayerIndex">层索引</param>
		public DrawTerrainChunkCylinder(
			Vector3 iStart, Vector3 iEnd,
			Vector3 iTargetDir, Transform iParent,
			float iScale, Color iColor,
			float iRadius = 1.0f, int iIndex = -1, int iLayerIndex = -1) : base(
			DrawType.TerrainChunkCylinder, iStart, iEnd, iTargetDir, iParent,
			iScale, iColor, iRadius, iIndex, iLayerIndex) { }
		
		/// <summary>
		/// 取得游戏对象名
		/// </summary>
		/// <returns></returns>
		public override string GetTargetNm()
		{
			return string.Format("l_{0}", index);
		}
	}
}
