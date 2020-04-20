using Packages.Logs;
using UnityEngine;

namespace Packages.Common.Extend
{
    /// <summary>
    /// ScriptableObject脚本扩展
    /// </summary>
    public static class ScriptableObjectEx
    {
        /// <summary>
        /// 日志输出(一般 - 运行日志)
        /// </summary>
        /// <param name="iScript">当前脚本</param>
        /// <param name="iFormat">日志格式</param>
        /// <param name="iArgs"></param>
        public static void Info(this ScriptableObject iScript, string iFormat, params object[] iArgs)
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
        public static void LInfo(this ScriptableObject iScript, string iFormat, params object[] iArgs)
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
        public static void Warning(this ScriptableObject iScript, string iFormat, params object[] iArgs)
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
        public static void Error(this ScriptableObject iScript, string iFormat, params object[] iArgs)
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
        public static void Fatal(this ScriptableObject iScript, string iFormat, params object[] iArgs)
        {
            var className = iScript.GetType().Name;
            var log = string.Format(iFormat, iArgs);
            Loger.Fatal($"[{className}] {log}");
        }
    } 

}

