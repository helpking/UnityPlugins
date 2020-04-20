using UnityEngine;
using Packages.Logs;

namespace Packages.Common.Extend
{
    /// <summary>
    /// 基础脚本扩展
    /// </summary>
    public static class MonoBehaviourEx
    {
        /// <summary>
        /// 日志输出(一般 - 运行日志)
        /// </summary>
        /// <param name="iScript">当前脚本</param>
        /// <param name="iFormat">日志格式</param>
        /// <param name="iArgs"></param>
        public static void Info(this MonoBehaviour iScript, string iFormat, params object[] iArgs)
        {
            var className = iScript.GetType().Name;
            var log = string.Format(iFormat, iArgs);
            Loger.Info($"[{className}] {log}");
        }
        
        /// <summary>
        /// 日志输出(一般 - 逻辑日志)
        /// </summary>
        /// <param name="iScript">当前脚本</param>
        /// <param name="iFormat">日志格式</param>
        /// <param name="iArgs"></param>
        public static void LInfo(this MonoBehaviour iScript, string iFormat, params object[] iArgs)
        {
            var className = iScript.GetType().Name;
            var log = string.Format(iFormat, iArgs);
            Loger.LInfo($"[{className}] {log}");
        }
        
        /// <summary>
        /// 日志输出(警告日志)
        /// </summary>
        /// <param name="iScript">当前脚本</param>
        /// <param name="iFormat">日志格式</param>
        /// <param name="iArgs"></param>
        public static void Warning(this MonoBehaviour iScript, string iFormat, params object[] iArgs)
        {
            var className = iScript.GetType().Name;
            var log = string.Format(iFormat, iArgs);
            Loger.Warning($"[{className}] {log}");
        }
        
        /// <summary>
        /// 日志输出(错误日志)
        /// </summary>
        /// <param name="iScript">当前脚本</param>
        /// <param name="iFormat">日志格式</param>
        /// <param name="iArgs"></param>
        public static void Error(this MonoBehaviour iScript, string iFormat, params object[] iArgs)
        {
            var className = iScript.GetType().Name;
            var log = string.Format(iFormat, iArgs);
            Loger.Error($"[{className}] {log}");
        }
        
        /// <summary>
        /// 日志输出(致命日志)
        /// </summary>
        /// <param name="iScript">当前脚本</param>
        /// <param name="iFormat">日志格式</param>
        /// <param name="iArgs"></param>
        public static void Fatal(this MonoBehaviour iScript, string iFormat, params object[] iArgs)
        {
            var className = iScript.GetType().Name;
            var log = string.Format(iFormat, iArgs);
            Loger.Fatal($"[{className}] {log}");
        }
    }

}

