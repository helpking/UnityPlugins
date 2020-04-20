using Packages.Common.Base;
using UnityEngine;

namespace Packages.Utils.Editor.Draw
{
	
	/// <summary>
	/// 绘制类型
	///   备注：可用于位判断
	/// </summary>
	internal enum DrawType
	{
		/// <summary>
		/// 圆柱体(目前暂时用无地面和顶面的长方体代替)
		/// </summary>
		Cylinder = 0x00000001,
		/// <summary>
		/// 球体
		/// </summary>
		Sphere = 0x00000002,
		/// <summary>
		/// 包围盒(Cylinder + Sphere)
		/// </summary>
		Bounds = 0x00000004,/// <summary>
		/// 地形Chunk包围盒圆柱体(目前暂时用无地面和顶面的长方体代替)
		/// </summary>
		TerrainChunkCylinder = 0x00000008,
		/// <summary>
		/// 地形Chunk包围盒顶点
		/// </summary>
		TerrainChunkVertex = 0x00000010,
		/// <summary>
		/// 地形Chunk包围盒(Cylinder + Sphere)
		/// </summary>
		TerrainChunkBounds = 0x00000020
	}
	
	/// <summary>
    /// 绘制对象 
    /// </summary>
    internal abstract class DrawObjBase : JsonDataBase
    {
	    /// <summary>
      	/// 绘制对象
      	/// </summary>
      	public GameObject Target;

	    /// <summary>
	    /// 层索引
	    /// </summary>
	    public int LayerIndex;

	    /// <summary>
	    /// 类型
	    /// </summary>
	    public DrawType Type;
        
        /// <summary>
        /// 父节点
        /// </summary>
        protected Transform parent;

        /// <summary>
        /// 颜色
        /// </summary>
        protected Color color;

        /// <summary>
        /// 缩放
        /// </summary>
        protected float scale = 1.0f;

        /// <summary>
        /// 索引
        /// </summary>
        protected int index;

        /// <summary>
      	/// 释放/摧毁绘制对象
      	/// </summary>
      	public void Destroy()
      	{
      		if(null == Target) return;
      		Object.DestroyImmediate(Target);
      	}

        /// <summary>
        /// 取得游戏对象名
        /// </summary>
        /// <returns></returns>
        public virtual string GetTargetNm()
        {
	        return string.Format("{0}_{1}",
		        ClassName, index);
        }

        /// <summary>
        /// 重置缩放
        ///   备注：x,y,z等比例缩放
        /// </summary>
        /// <param name="iScale">缩放</param>
        public virtual void ResetScale(float iScale) { }

        /// <summary>
        /// 绘制网格
        /// </summary>
        /// <param name="iTagIndex">标签索引</param>
        public abstract void DrawMesh(int iTagIndex = -1);
    }
}