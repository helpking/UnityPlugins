using Packages.Logs;
using UnityEditor;

namespace Packages.Common.Extend.Editor
{
    
    /// <summary>
    /// 编辑器窗口扩展
    /// </summary>
    public static class EditorWindowEx
    {
        /// <summary>
        /// 日志输出(一般 - 运行日志)
        /// </summary>
        /// <param name="iWindow">当前窗口</param>
        /// <param name="iFormat">日志格式</param>
        /// <param name="iArgs"></param>
        public static void Info(this EditorWindow iWindow, string iFormat, params object[] iArgs)
        {
            var className = iWindow.GetType().Name;
            var log = string.Format(iFormat, iArgs);
            Loger.Info($"[{className}] {log}");
        }
        
        /// <summary>
        /// 日志输出(一般 - 逻辑日志)
        /// </summary>
        /// <param name="iWindow">当前窗口</param>
        /// <param name="iFormat">日志格式</param>
        /// <param name="iArgs"></param>
        public static void LInfo(this EditorWindow iWindow, string iFormat, params object[] iArgs)
        {
            var className = iWindow.GetType().Name;
            var log = string.Format(iFormat, iArgs);
            Loger.LInfo($"[{className}] {log}");
        }
        
        /// <summary>
        /// 日志输出(警告日志)
        /// </summary>
        /// <param name="iWindow">当前窗口</param>
        /// <param name="iFormat">日志格式</param>
        /// <param name="iArgs"></param>
        public static void Warning(this EditorWindow iWindow, string iFormat, params object[] iArgs)
        {
            var className = iWindow.GetType().Name;
            var log = string.Format(iFormat, iArgs);
            Loger.Warning($"[{className}] {log}");
        }
        
        /// <summary>
        /// 日志输出(错误日志)
        /// </summary>
        /// <param name="iWindow">当前窗口</param>
        /// <param name="iFormat">日志格式</param>
        /// <param name="iArgs"></param>
        public static void Error(this EditorWindow iWindow, string iFormat, params object[] iArgs)
        {
            var className = iWindow.GetType().Name;
            var log = string.Format(iFormat, iArgs);
            Loger.Error($"[{className}] {log}");
        }
        
        /// <summary>
        /// 日志输出(致命日志)
        /// </summary>
        /// <param name="iWindow">当前窗口</param>
        /// <param name="iFormat">日志格式</param>
        /// <param name="iArgs"></param>
        public static void Fatal(this EditorWindow iWindow, string iFormat, params object[] iArgs)
        {
            var className = iWindow.GetType().Name;
            var log = string.Format(iFormat, iArgs);
            Loger.Fatal($"[{className}] {log}");
        }
    }
}

