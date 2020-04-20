using UnityEngine;

namespace Packages.Common.Extend
{
    
    /// <summary>
    /// 包围盒扩展类
    /// </summary>
    public static class BoundsEx
    {
        
#if UNITY_EDITOR
        
        /// <summary>
        /// 绘制包围盒
        /// </summary>
        /// <param name="iBounds">包围盒</param>
        /// <param name="iColor">颜色</param>
        public static void DrawBounds(this Bounds iBounds, Color iColor)
        {
            Gizmos.color = iColor;

            Gizmos.DrawWireCube(iBounds.center, iBounds.size);
        }
        
#endif

        /// <summary>
        /// 判断包围盒是否在摄像机范围内
        /// </summary>
        /// <param name="iBounds">包围盒</param>
        /// <param name="iCamera">摄像机</param>
        /// <returns>包围盒是否在摄像机范围内</returns>
        public static bool IsInCamera(this Bounds iBounds, Camera iCamera)
        {

            var matrix = iCamera.projectionMatrix*iCamera.worldToCameraMatrix;

            var code =
                MatrixEx.ComputeOutCode(new Vector4(iBounds.center.x + iBounds.size.x/2, iBounds.center.y + iBounds.size.y/2,
                    iBounds.center.z + iBounds.size.z/2, 1), matrix);


            code &=
                MatrixEx.ComputeOutCode(new Vector4(iBounds.center.x - iBounds.size.x/2, iBounds.center.y + iBounds.size.y/2,
                    iBounds.center.z + iBounds.size.z/2, 1), matrix);

            code &=
                MatrixEx.ComputeOutCode(new Vector4(iBounds.center.x + iBounds.size.x/2, iBounds.center.y - iBounds.size.y/2,
                    iBounds.center.z + iBounds.size.z/2, 1), matrix);

            code &=
                MatrixEx.ComputeOutCode(new Vector4(iBounds.center.x - iBounds.size.x/2, iBounds.center.y - iBounds.size.y/2,
                    iBounds.center.z + iBounds.size.z/2, 1), matrix);

            code &=
                MatrixEx.ComputeOutCode(new Vector4(iBounds.center.x + iBounds.size.x/2, iBounds.center.y + iBounds.size.y/2,
                    iBounds.center.z - iBounds.size.z/2, 1), matrix);

            code &=
                MatrixEx.ComputeOutCode(new Vector4(iBounds.center.x - iBounds.size.x/2, iBounds.center.y + iBounds.size.y/2,
                    iBounds.center.z - iBounds.size.z/2, 1), matrix);

            code &=
                MatrixEx.ComputeOutCode(new Vector4(iBounds.center.x + iBounds.size.x/2, iBounds.center.y - iBounds.size.y/2,
                    iBounds.center.z - iBounds.size.z/2, 1), matrix);

            code &=
                MatrixEx.ComputeOutCode(new Vector4(iBounds.center.x - iBounds.size.x/2, iBounds.center.y - iBounds.size.y/2,
                    iBounds.center.z - iBounds.size.z/2, 1), matrix);
            
            return code == 0;
        }

        /// <summary>
        /// 判断包围盒是否在摄像机范围内(扩展)
        /// </summary>
        /// <param name="iBounds">包围盒</param>
        /// <param name="iCamera">摄像机</param>
        /// <param name="iLeftX">左偏移(基于-1)</param>
        /// <param name="iRightX">右偏移(基于1)</param>
        /// <param name="iDownY">下偏移(基于-1)</param>
        /// <param name="iUpY">上偏移(基于1)</param>
        /// <returns>包围盒是否在摄像机范围内</returns>
        public static bool IsInCamera(
            this Bounds iBounds, Camera iCamera, 
            float iLeftX, float iRightX, float iDownY, float iUpY)
        {

            // 裁剪矩阵
            var matrix = iCamera.projectionMatrix*iCamera.worldToCameraMatrix;

            var code =
                MatrixEx.ComputeOutCodeEx(new Vector4(iBounds.center.x + iBounds.size.x/2, iBounds.center.y + iBounds.size.y/2,
                    iBounds.center.z + iBounds.size.z/2, 1), matrix, iLeftX, iRightX, iDownY, iUpY);


            code &=
                MatrixEx.ComputeOutCodeEx(new Vector4(iBounds.center.x - iBounds.size.x/2, iBounds.center.y + iBounds.size.y/2,
                    iBounds.center.z + iBounds.size.z/2, 1), matrix, iLeftX, iRightX, iDownY, iUpY);

            code &=
                MatrixEx.ComputeOutCodeEx(new Vector4(iBounds.center.x + iBounds.size.x/2, iBounds.center.y - iBounds.size.y/2,
                    iBounds.center.z + iBounds.size.z/2, 1), matrix, iLeftX, iRightX, iDownY, iUpY);

            code &=
                MatrixEx.ComputeOutCodeEx(new Vector4(iBounds.center.x - iBounds.size.x/2, iBounds.center.y - iBounds.size.y/2,
                    iBounds.center.z + iBounds.size.z/2, 1), matrix, iLeftX, iRightX, iDownY, iUpY);

            code &=
                MatrixEx.ComputeOutCodeEx(new Vector4(iBounds.center.x + iBounds.size.x/2, iBounds.center.y + iBounds.size.y/2,
                    iBounds.center.z - iBounds.size.z/2, 1), matrix, iLeftX, iRightX, iDownY, iUpY);

            code &=
                MatrixEx.ComputeOutCodeEx(new Vector4(iBounds.center.x - iBounds.size.x/2, iBounds.center.y + iBounds.size.y/2,
                    iBounds.center.z - iBounds.size.z/2, 1), matrix, iLeftX, iRightX, iDownY, iUpY);

            code &=
                MatrixEx.ComputeOutCodeEx(new Vector4(iBounds.center.x + iBounds.size.x/2, iBounds.center.y - iBounds.size.y/2,
                    iBounds.center.z - iBounds.size.z/2, 1), matrix, iLeftX, iRightX, iDownY, iUpY);

            code &=
                MatrixEx.ComputeOutCodeEx(new Vector4(iBounds.center.x - iBounds.size.x/2, iBounds.center.y - iBounds.size.y/2,
                    iBounds.center.z - iBounds.size.z/2, 1), matrix, iLeftX, iRightX, iDownY, iUpY);
            
            return code == 0;
        }

        /// <summary>
        /// 判断包围盒是否包含另一个包围盒
        /// </summary>
        /// <param name="iBounds">包围盒</param>
        /// <param name="iCompareTo"></param>
        /// <returns>包围盒是否包含另一个包围盒</returns>
        public static bool IsContains(this Bounds iBounds, Bounds iCompareTo)
        {
            if (
                !iBounds.Contains(iCompareTo.center +
                                 new Vector3(-iCompareTo.size.x/2, iCompareTo.size.y/2, -iCompareTo.size.z/2)))
                return false;
            if (
                !iBounds.Contains(iCompareTo.center + new Vector3(iCompareTo.size.x/2, iCompareTo.size.y/2, -iCompareTo.size.z/2)))
                return false;
            if (!iBounds.Contains(iCompareTo.center + new Vector3(iCompareTo.size.x/2, iCompareTo.size.y/2, iCompareTo.size.z/2)))
                return false;
            if (
                !iBounds.Contains(iCompareTo.center + new Vector3(-iCompareTo.size.x/2, iCompareTo.size.y/2, iCompareTo.size.z/2)))
                return false;
            if (
                !iBounds.Contains(iCompareTo.center +
                                 new Vector3(-iCompareTo.size.x/2, -iCompareTo.size.y/2, -iCompareTo.size.z/2)))
                return false;
            if (
                !iBounds.Contains(iCompareTo.center +
                                 new Vector3(iCompareTo.size.x/2, -iCompareTo.size.y/2, -iCompareTo.size.z/2)))
                return false;
            if (
                !iBounds.Contains(iCompareTo.center + new Vector3(iCompareTo.size.x/2, -iCompareTo.size.y/2, iCompareTo.size.z/2)))
                return false;
            if (
                !iBounds.Contains(iCompareTo.center +
                                 new Vector3(-iCompareTo.size.x/2, -iCompareTo.size.y/2, iCompareTo.size.z/2)))
                return false;
            return true;
        }
    }
}


