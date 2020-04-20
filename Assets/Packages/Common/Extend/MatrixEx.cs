using UnityEngine;

namespace Packages.Common.Extend
{
    public static class MatrixEx
    {

        /// <summary>
        /// 计算越界Code
        /// </summary>
        /// <param name="iPos">坐标</param>
        /// <param name="iProjection">投影矩阵</param>
        /// <returns>位操作符(分别标示着x,y,z的大小越界)</returns>
        public static int ComputeOutCode(Vector4 iPos, Matrix4x4 iProjection)
        {
            iPos = iProjection * iPos;
            var code = 0;
            if (iPos.x < -iPos.w) code |= 0x01;
            if (iPos.x > iPos.w) code |= 0x02;
            if (iPos.y < -iPos.w) code |= 0x04;
            if (iPos.y > iPos.w) code |= 0x08;
            if (iPos.z < -iPos.w) code |= 0x10;
            if (iPos.z > iPos.w) code |= 0x20;
            return code;
        }

        /// <summary>
        /// 计算越界Code(扩展)
        /// </summary>
        /// <param name="iPos">坐标</param>
        /// <param name="iProjection">投影矩阵</param>
        /// <param name="iLeftX">左偏移(基于-1)</param>
        /// <param name="iRightX">右偏移(基于1)</param>
        /// <param name="iDownY">下偏移(基于-1)</param>
        /// <param name="iUpY">上偏移(基于1)</param>
        /// <returns>位操作符(分别标示着x,y,z的大小越界)</returns>
        public static int ComputeOutCodeEx(
            Vector4 iPos, Matrix4x4 iProjection, 
            float iLeftX, float iRightX, float iDownY, float iUpY)
        {
            iPos = iProjection * iPos;
            var code = 0;
            if (iPos.x < (-1 + iLeftX) * iPos.w) code |= 0x01;
            if (iPos.x > (1 + iRightX) * iPos.w) code |= 0x02;
            if (iPos.y < (-1 + iDownY) * iPos.w) code |= 0x04;
            if (iPos.y > (1 + iUpY) * iPos.w) code |= 0x08;
            if (iPos.z < -iPos.w) code |= 0x10;
            if (iPos.z > iPos.w) code |= 0x20;
            return code;
        }
    }
}

