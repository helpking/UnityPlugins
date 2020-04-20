using UnityEngine;

namespace Packages.Common.Extend.Editor
{
    /// <summary>
    /// Gizmos扩展
    /// </summary>
    public static class GizmosEx 
    {
        /// <summary>
        /// 绘制视锥体
        /// </summary>
        /// <param name="iCamera">摄像机</param>
        /// <param name="iColor">颜色</param>
        public static void DrawViewFrustum(Camera iCamera, Color iColor)
        {
            DrawProjection(iCamera, 
                new Vector3(-1, -1, -1), 
                new Vector3(1, -1, -1), 
                new Vector3(1, -1, 1),
                new Vector3(-1, -1, 1),
                new Vector3(-1, 1, -1), 
                new Vector3(1, 1, -1), 
                new Vector3(1, 1, 1), 
                new Vector3(-1, 1, 1), iColor);
        }

        /// <summary>
        /// 绘制视锥体(扩展)
        /// </summary>
        /// <param name="iCamera">摄像机</param>
        /// <param name="iLeftX">左偏移(基于-1)</param>
        /// <param name="iRightX">右偏移(基于1)</param>
        /// <param name="iDownY">下偏移(基于-1)</param>
        /// <param name="iUpY">上偏移(基于1)</param>
        /// <param name="iColor">颜色</param>
        public static void DrawViewFrustumEx(
            Camera iCamera, float iLeftX, float iRightX, float iDownY, float iUpY, Color iColor)
        {
            DrawProjection(
                iCamera, 
                new Vector3(-1 + iLeftX, -1 + iDownY, -1), 
                new Vector3(1 + iRightX, -1 + iDownY, -1), 
                new Vector3(1 + iRightX, -1 + iDownY, 1),
                new Vector3(-1 + iLeftX, -1 + iDownY, 1),
                new Vector3(-1 + iLeftX, 1 + iUpY, -1), 
                new Vector3(1 + iRightX, 1 + iUpY, -1), 
                new Vector3(1 + iRightX, 1 + iUpY, 1), 
                new Vector3(-1 + iLeftX, 1 + iUpY, 1), iColor);
        }

        /// <summary>
        /// 绘制视锥体
        /// </summary>
        /// <param name="iCamera">摄像机</param>
        /// <param name="iPj1">裁剪空间顶点1</param>
        /// <param name="iPj2">裁剪空间顶点2</param>
        /// <param name="iPj3">裁剪空间顶点3</param>
        /// <param name="iPj4">裁剪空间顶点4</param>
        /// <param name="iPj5">裁剪空间顶点5</param>
        /// <param name="iPj6">裁剪空间顶点6</param>
        /// <param name="iPj7">裁剪空间顶点7</param>
        /// <param name="iPj8">裁剪空间顶点8</param>
        /// <param name="iColor">颜色</param>
        private static void DrawProjection(
            Camera iCamera, 
            Vector3 iPj1, Vector3 iPj2, Vector3 iPj3, Vector3 iPj4, 
            Vector3 iPj5, Vector3 iPj6, Vector3 iPj7, Vector3 iPj8, 
            Color iColor)
        {
            // 反算裁剪空间顶点坐标到摄像机空间
            var p1 = iCamera.projectionMatrix.inverse.MultiplyPoint(iPj1);
            var p2 = iCamera.projectionMatrix.inverse.MultiplyPoint(iPj2);
            var p3 = iCamera.projectionMatrix.inverse.MultiplyPoint(iPj3);
            var p4 = iCamera.projectionMatrix.inverse.MultiplyPoint(iPj4);
            var p5 = iCamera.projectionMatrix.inverse.MultiplyPoint(iPj5);
            var p6 = iCamera.projectionMatrix.inverse.MultiplyPoint(iPj6);
            var p7 = iCamera.projectionMatrix.inverse.MultiplyPoint(iPj7);
            var p8 = iCamera.projectionMatrix.inverse.MultiplyPoint(iPj8);

            // 摄像机空间坐标换算世界坐标
            p1 = iCamera.cameraToWorldMatrix.MultiplyPoint(p1);
            p2 = iCamera.cameraToWorldMatrix.MultiplyPoint(p2);
            p3 = iCamera.cameraToWorldMatrix.MultiplyPoint(p3);
            p4 = iCamera.cameraToWorldMatrix.MultiplyPoint(p4);
            p5 = iCamera.cameraToWorldMatrix.MultiplyPoint(p5);
            p6 = iCamera.cameraToWorldMatrix.MultiplyPoint(p6);
            p7 = iCamera.cameraToWorldMatrix.MultiplyPoint(p7);
            p8 = iCamera.cameraToWorldMatrix.MultiplyPoint(p8);

            Gizmos.color = iColor;

            // 绘制是锥体线
            Gizmos.DrawLine(p1, p2);
            Gizmos.DrawLine(p2, p3);
            Gizmos.DrawLine(p3, p4);
            Gizmos.DrawLine(p4, p1);

            Gizmos.DrawLine(p5, p6);
            Gizmos.DrawLine(p6, p7);
            Gizmos.DrawLine(p7, p8);
            Gizmos.DrawLine(p8, p5);

            Gizmos.DrawLine(p1, p5);
            Gizmos.DrawLine(p2, p6);
            Gizmos.DrawLine(p3, p7);
            Gizmos.DrawLine(p4, p8);
        }
    }
}
