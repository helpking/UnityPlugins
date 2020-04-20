using Packages.Common.Base;
using Packages.Settings;

namespace Packages.Common
{
    
    /// <summary>
    /// 平台类型类型.
    /// </summary>
    public enum PlatformType {
        /// <summary>
        /// 无.
        /// </summary>
        None = 0,
        /// <summary>
        /// iOS.
        /// </summary>
        iOS = 1,
        /// <summary>
        /// 安卓：纯净版.
        /// </summary>
        Android = 2,
        /// <summary>
        /// 安卓：华为.
        /// </summary>
        Huawei = 3,
        /// <summary>
        /// 安卓：天鸽.
        /// </summary>
        Tiange = 4
    }

    /// <summary>
    /// 静态常量定义（全局）.
    /// </summary>
    public class Const : SingletonBase<Const>
    {
        public const string DefaultPath = "Assets/Resources/Default";
        public const string DevelopPath = "Assets/Resources/Develop";

        /// <summary>
        /// 场景根节点
        /// </summary>
        public static readonly string AppRoot = "_Root";

        /// <summary>
        /// 管理器根节点（固定）
        /// </summary>
        public static readonly string ConstManagersRoot = "_Consts";

        /// <summary>
        /// 管理器根节点（动态）
        /// </summary>
        public static readonly string DynamicsManagersRoot = "_Dynamics";
        
        /// <summary>
        /// 切换路径 Develop -> Default
        /// </summary>
        /// <param name="iPath">路径</param>
        /// <returns>切换后的路径</returns>
        public static string SwitchPathToDefault(string iPath) {
            return iPath.Replace(
                DevelopPath, DefaultPath);
        }
        
        /// <summary>
        /// 切换路径 Default -> Develop
        /// </summary>
        /// <param name="iPath">路径</param>
        /// <returns>切换后的路径</returns>
        public static string SwitchPathToDevelop(string iPath) {
            return iPath.Replace(
                DefaultPath, DevelopPath);
        }
    }
}
