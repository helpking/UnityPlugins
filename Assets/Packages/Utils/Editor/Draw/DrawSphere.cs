using System;
using UnityEngine;
using UnityEditor;

namespace Packages.Utils.Editor.Draw
{
	
   /// <summary>
   	/// 绘制对象：圆球
   	/// </summary>
   	internal class DrawSphere : DrawObjBase
   	{

	    /// <summary>
	    /// 构造函数
	    /// </summary>
	    /// <param name="iType">绘制类型</param>
	    /// <param name="iOrigin">圆点</param>
	    /// <param name="iRadius">半径</param>
	    /// <param name="iColor">颜色</param>
	    /// <param name="iParent">父节点</param>
	    /// <param name="iScale">缩放</param>
	    /// <param name="iIndex">索引</param>
	    /// <param name="iLayerIndex">层索引</param>
	    public DrawSphere(
		    DrawType iType, Vector3 iOrigin, float iRadius, Color iColor,
		    Transform iParent, float iScale, 
		    int iIndex = -1, int iLayerIndex = -1)
	    {
		    Origin = iOrigin;
		    Radius = iRadius;
		    Type = iType;
		    LayerIndex = iLayerIndex;
		    
		    parent = iParent;
		    scale = iScale * 2.0f;
		    wOrigin = parent.TransformPoint(iOrigin);
		    color = iColor;
		    index = iIndex;
	    }

	    /// <summary>
   		/// 圆点
   		/// </summary>
   		public Vector3 Origin;
	    /// <summary>
	    /// 圆点(世界坐标)
	    /// </summary>
	    private Vector3 wOrigin; 
   
   		/// <summary>
   		/// 半径
   		/// </summary>
   		public float Radius;
        
        /// <summary>
        /// 比较函数
        /// </summary>
        /// <param name="iWOrigin">圆点(世界坐标)</param>
        /// <param name="iRadius">半径</param>
        /// <returns>true:相等; false:不相等;</returns>
        public bool Equal(Vector3 iWOrigin, float iRadius)
        {
//	        return wOrigin == iWOrigin && Math.Abs(Radius - iRadius) <= 0;
			return Vector3.Distance(wOrigin, iWOrigin) <= UtilsDraw.MinEqualDistance && Math.Abs(Radius - iRadius) <= 0;
        }

        /// <summary>
        /// 绘制网格
        /// </summary>
        /// <param name="iTagIndex">标签索引</param>
        public override void DrawMesh(int iTagIndex = -1)
        {
	        Target = GameObject.CreatePrimitive(PrimitiveType.Sphere);
	        Target.name = GetTargetNm();
	        var tagIndex = (-1 == iTagIndex) ? LayerIndex : iTagIndex;
	        Target.tag = (-1 == tagIndex)
		        ? UtilsDraw.GameObjectTagDrawDebug
		        : string.Format("{0}_{1}", UtilsDraw.GameObjectTagDrawDebug, tagIndex);
	        if(null != parent) Target.transform.parent = parent.transform;

	        Target.transform.localPosition = Origin;
	        Target.transform.localScale = new Vector3(scale, scale, scale);
	        
	        var meshRenderer = Target.GetComponent<MeshRenderer>();
	        if(null == meshRenderer) return;
	        var material = AssetDatabase.LoadAssetAtPath<Material>("Assets/Packages/Dynamic/Terrains/Editor/Material/DrawVertex.mat");
	        if (null == material) return;
	        meshRenderer.sharedMaterial = material;
	        meshRenderer.sharedMaterial.SetColor("_DrawColor", color);

        }

        /// <summary>
        /// 重置缩放
        ///   备注：x,y,z等比例缩放
        /// </summary>
        /// <param name="iScale">缩放</param>
        public override void ResetScale(float iScale)
        {
	        if(null == Target) return;
	        var scaleTmp = iScale * 2.0f;
	        if(scale == scaleTmp) return;
	        scale = scaleTmp;
	        Target.transform.localScale = 
		        new Vector3(scale, scale, scale);
        }
    }

   /// <summary>
   /// 绘制对象：地形Chunk包围盒顶点
   /// </summary>
   internal class DrawTerrainChunkVertex : DrawSphere
   {
	   /// <summary>
	   /// 构造函数
	   /// </summary>
	   /// <param name="iOrigin">圆点</param>
	   /// <param name="iRadius">半径</param>
	   /// <param name="iColor">颜色</param>
	   /// <param name="iParent">父节点</param>
	   /// <param name="iScale">缩放</param>
	   /// <param name="iIndex">索引</param>
	   /// <param name="iLayerIndex">层索引</param>
	   public DrawTerrainChunkVertex(
		   Vector3 iOrigin, float iRadius, Color iColor,
		   Transform iParent, float iScale, int iIndex = -1, int iLayerIndex = -1) : base(
		   DrawType.TerrainChunkVertex, iOrigin, iRadius, iColor,
		   iParent, iScale,iIndex, iLayerIndex) { }
	   
	   /// <summary>
	   /// 取得游戏对象名
	   /// </summary>
	   /// <returns></returns>
	   public override string GetTargetNm()
	   {
		   return string.Format("v_{0}", index);
	   }
   }
}
