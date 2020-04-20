using UnityEngine;

namespace Packages.Dynamic.Detector
{
    /// <summary>
    /// 检测器接口，用于检测和场景物件的触发
    /// </summary>
    public interface IDetector
    {
        /// <summary>
        /// 是否检测成功
        /// </summary>
        /// <param name="iBounds">包围盒</param>
        /// <returns>检测物体是否被包围</returns>
        bool IsDetected(Bounds iBounds);
 
        /// <summary>
        /// 触发器位置
        /// </summary>
        Vector3 Position { get; }
    }

}