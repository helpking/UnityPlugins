using Packages.Logs;

namespace Packages.Common.Extend.Editor
{
    /// <summary>
    /// Unity编辑器日志方法扩展
    /// </summary>
    public static class EditorEx 
    {
        
        /// <summary>
        /// 打包日志(开始Scope).
        /// </summary>
        /// <param name="iEditor">当前编辑器</param>
        /// <param name="iScope">打包范围.</param>
        /// <param name="iFormat">格式.</param>
        /// <param name="iArgs">参数.</param>
        public static void BuildStart(this UnityEditor.Editor iEditor, string iScope, string iFormat, params object[] iArgs)
        {
            var className = iEditor.GetType().Name;
            var log = string.Format(iFormat, iArgs);
            Loger.BuildStart(iScope, $"{className} {log}");
        }
        
        /// <summary>
        /// 打包日志.
        /// </summary>
        /// <param name="iEditor">当前编辑器</param>
        /// <param name="iFormat">格式.</param>
        /// <param name="iArgs">参数.</param>
        public static void BuildLog(this UnityEditor.Editor iEditor, string iFormat, params object[] iArgs)
        {
            var className = iEditor.GetType().Name;
            var log = string.Format(iFormat, iArgs);
            Loger.BuildLog($"{className} {log}");
        }

        /// <summary>
        /// 打包日志 - 错误.
        /// </summary>
        /// <param name="iEditor">当前编辑器</param>
        /// <param name="iFormat">格式.</param>
        /// <param name="iArgs">参数.</param>
        public static void BuildErrorLog(this UnityEditor.Editor iEditor, string iFormat, params object[] iArgs)
        {
            var className = iEditor.GetType().Name;
            var log = string.Format(iFormat, iArgs);
            Loger.BuildErrorLog($"{className} {log}");
        }

        /// <summary>
        /// 打包日志(结束Scope).
        /// </summary>
        /// <param name="iEditor">当前编辑器</param>
        public static void BuildEnd(this UnityEditor.Editor iEditor)
        {
            Loger.BuildEnd();
        }
        
        /// <summary>
        /// 日志输出(一般 - 运行日志)
        /// </summary>
        /// <param name="iEditor">当前编辑器</param>
        /// <param name="iFormat">日志格式</param>
        /// <param name="iArgs"></param>
        public static void Info(this UnityEditor.Editor iEditor, string iFormat, params object[] iArgs)
        {
            var className = iEditor.GetType().Name;
            var log = string.Format(iFormat, iArgs);
            Loger.Info($"[{className}] {log}");
        }
        
        /// <summary>
        /// 日志输出(一般 - 逻辑日志)
        /// </summary>
        /// <param name="iEditor">当前编辑器</param>
        /// <param name="iFormat">日志格式</param>
        /// <param name="iArgs"></param>
        public static void LInfo(this UnityEditor.Editor iEditor, string iFormat, params object[] iArgs)
        {
            var className = iEditor.GetType().Name;
            var log = string.Format(iFormat, iArgs);
            Loger.LInfo($"[{className}] {log}");
        }
        
        /// <summary>
        /// 日志输出(警告日志)
        /// </summary>
        /// <param name="iEditor">当前编辑器</param>
        /// <param name="iFormat">日志格式</param>
        /// <param name="iArgs"></param>
        public static void Warning(this UnityEditor.Editor iEditor, string iFormat, params object[] iArgs)
        {
            var className = iEditor.GetType().Name;
            var log = string.Format(iFormat, iArgs);
            Loger.Warning($"[{className}] {log}");
        }
        
        /// <summary>
        /// 日志输出(错误日志)
        /// </summary>
        /// <param name="iEditor">当前编辑器</param>
        /// <param name="iFormat">日志格式</param>
        /// <param name="iArgs"></param>
        public static void Error(this UnityEditor.Editor iEditor, string iFormat, params object[] iArgs)
        {
            var className = iEditor.GetType().Name;
            var log = string.Format(iFormat, iArgs);
            Loger.Error($"[{className}] {log}");
        }
        
        /// <summary>
        /// 日志输出(致命日志)
        /// </summary>
        /// <param name="iEditor">当前编辑器</param>
        /// <param name="iFormat">日志格式</param>
        /// <param name="iArgs"></param>
        public static void Fatal(this UnityEditor.Editor iEditor, string iFormat, params object[] iArgs)
        {
            var className = iEditor.GetType().Name;
            var log = string.Format(iFormat, iArgs);
            Loger.Fatal($"[{className}] {log}");
        }
    }
}

