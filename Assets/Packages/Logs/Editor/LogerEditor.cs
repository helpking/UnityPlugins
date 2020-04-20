using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using Packages.Utils;

namespace Packages.Logs.Editor
{

    /// <summary>
    /// 日志编辑器
    /// </summary>
    public class LogerEditor
    {

        private const string LOGER_SCRIPT_PATH = "Assets/Packages/Logs/Loger.cs";

        /// <summary>
        /// 日志编辑器配置信息内部类
        /// </summary>
        private class LogerConfig
        {
            /// <summary>
            /// 自定义日志脚本路径
            /// </summary>
            public string scriptPath;
            /// <summary>
            /// 脚本type
            /// </summary>
            public string typeName;
            public int instanceID = 0;

            /// <summary>
            /// 构造函数
            /// </summary>
            /// <param name="iScriptPath"></param>
            /// <param name="iLogType"></param>
            public LogerConfig(string iScriptPath, System.Type iLogType)
            {
                scriptPath = iScriptPath;
                typeName = iLogType.FullName;
            }
        }
            
        //配置的日志
        private static readonly LogerConfig[] Config = {
            new LogerConfig(LOGER_SCRIPT_PATH, typeof(Loger))
        };

        /// <summary>
        /// 处理从ConsoleWindow双击跳转
        /// </summary>
        /// <param name="iInstanceId">实例ID</param>
        /// <param name="iLine">行号</param>
        /// <returns></returns>
        [UnityEditor.Callbacks.OnOpenAssetAttribute(-1)]
        private static bool OnOpenAsset(int iInstanceId, int iLine)
        {
            for (var i = Config.Length - 1; i >= 0; --i)
            {
                var configTmp = Config[i];
                UpdateLogInstanceId(configTmp);
                if (iInstanceId != configTmp.instanceID) continue;
                var statckTrack = GetStackTrace();
                if (string.IsNullOrEmpty(statckTrack)) break;
                /**
                 * 举例说明：下面这段是一条ConsoleWindow的日志信息
                 * Awake
                 * UnityEngine.Debug:Log(Object)
                 * Loger:Log(String) (at Assets/Scripts/DDebug/Loger.cs:13)
                 * Test:Awake() (at Assets/Scripts/Test.cs:13)
                 * 
                 * 说明：
                 * 1、其中第一行的"Awake":是指调用自定义打印日志函数的函数名，本例是在Test脚本中的Awake函数里调用的
                 * 2、第二行的"UnityEngine.Debug:Log(Object)":是指该日志最底层是通过Debug.Log函数打印出来的
                 * 3、第三行的"Loger:Log(String) (at Assets/Scripts/DDebug/Loger.cs:13)":指第二行的函数调用在DDebug.cs的13行
                 * 4、第四行的"Test:Awake() (at Assets/Scripts/Test.cs:13)":指Test.cs脚本的Awake函数调用了第二行的DDebug.cs的Log函数，在第13行
                 **/
                    
                //通过以上信息，不难得出双击该日志应该打开Test.cs文件，并定位到第13行
                //以换行分割堆栈信息
                var fileNames = statckTrack.Split('\n');
                //定位到调用自定义日志函数的那一行："Test:Awake() (at Assets/Scripts/Test.cs:13)"
                var fileName = GetCurrentFullFileName(fileNames);
                //定位到上例的行数：13
                var fileLine = LogFileNameToFileLine(fileName);
                if(-1 >= fileLine) continue;
                //得到调用自定义日志函数的脚本："Assets/Scripts/Test.cs"
                fileName = GetRealFileName(fileName);
                    
                // 根据脚本名和行数，打开脚本
                // 脚本："Assets/Scripts/Test.cs"
                // 行号：13
                AssetDatabase.OpenAsset(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(fileName), fileLine);
                return true;
            }

            return false;  
        }
        
        /// <summary>
        /// 更新日志实例ID
        /// </summary>
        /// <param name="iConfig">配置信息</param>
        private static void UpdateLogInstanceId(LogerConfig iConfig)
        {
            if (iConfig.instanceID > 0)
            {
                return;
            }

            var assetLoadTmp = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(iConfig.scriptPath);
            if (null == assetLoadTmp)
            {
                throw new Exception("not find asset by path=" + iConfig.scriptPath);
            }
            iConfig.instanceID = assetLoadTmp.GetInstanceID();
        }
        
        /// <summary>
        /// 反射出日志堆栈
        /// </summary>
        /// <returns></returns>
        private static string GetStackTrace()
        {
            var consoleWindowType = typeof(EditorWindow).Assembly.GetType("UnityEditor.ConsoleWindow");
            var fieldInfo = consoleWindowType.GetField(
                "ms_ConsoleWindow", 
                BindingFlags.Static | BindingFlags.NonPublic);
            if (fieldInfo == null) return null;
            var consoleWindowInstance = fieldInfo.GetValue(null);

            if (null == consoleWindowInstance) return null;
            if (EditorWindow.focusedWindow != (EditorWindow) consoleWindowInstance) return null;
            fieldInfo = consoleWindowType.GetField(
                "m_ActiveText", 
                BindingFlags.Instance | BindingFlags.NonPublic);
            if (fieldInfo == null) return null;
            var activeText = fieldInfo.GetValue(consoleWindowInstance).ToString();
            return activeText;
        }
        
        /// <summary>
        /// 取得当前文件全字（包含全路径）
        /// </summary>
        /// <param name="iFileNames">文件名列表</param>
        /// <returns>当前文件全字（包含全路径）</returns>
        private static string GetCurrentFullFileName(IReadOnlyList<string> iFileNames)
        {
            var retValue = "";
            var findIndex = -1;

            for (var i = iFileNames.Count - 1; i >= 0; --i)
            {
                var isCustomLog = false;
                var fileName = iFileNames[i];
                
                if(string.IsNullOrEmpty(fileName)) continue;
                // 跳过Unity自身的库文件
                if(fileName.StartsWith("UnityEngine.")) continue;
                if(fileName.StartsWith("UnityEditor.")) continue;
                if(fileName.StartsWith("System.")) continue;
                
                for (var j = Config.Length - 1; j >= 0; --j)
                {
                    if (!fileName.Contains(Config[j].typeName)) continue;
                    isCustomLog = true;
                    break;
                }

                if (!isCustomLog) continue;
                findIndex = i;
                break;
            }

            var searchIndex = findIndex + 1 >= iFileNames.Count ? findIndex - 1 : findIndex + 1;
            while (searchIndex < iFileNames.Count)
            {
                if (!string.IsNullOrEmpty(iFileNames[searchIndex]))
                {
                    break;
                }
                ++searchIndex;
            }

            if (searchIndex >= 0 && searchIndex < iFileNames.Count - 1)
            {
                retValue = iFileNames[searchIndex];
            }

            return retValue;
        }
        
        /// <summary>
        /// 取得日志文件行号
        /// </summary>
        /// <param name="iFileName">文件名</param>
        /// <returns>日志文件行号</returns>
        private static int LogFileNameToFileLine(string iFileName)
        {
            if (string.IsNullOrEmpty(iFileName))
            {
                return -1;
            }
            var findIndex = ParseFileLineStartIndex(iFileName);
            var stringParseLine = "";
            for (var i = findIndex; i < iFileName.Length; ++i)
            {
                var charCheck = iFileName[i];
                if (!UtilsTools.IsNumber(charCheck))
                {
                    break;
                }
                stringParseLine += charCheck;
            }

            return int.Parse(stringParseLine);
        }
        
        /// <summary>
        /// 取得文件真实名字
        /// </summary>
        /// <param name="iFileName">文件名</param>
        /// <returns>文件真实名字</returns>
        private static string GetRealFileName(string iFileName)
        {
            var indexStart = iFileName.IndexOf("(at ", StringComparison.Ordinal) + "(at ".Length;
            var indexEnd = ParseFileLineStartIndex(iFileName) - 1;

            iFileName = iFileName.Substring(indexStart, indexEnd - indexStart);
            return iFileName;
        }
        
        /// <summary>
        /// 取得文件名中字符开始索引
        /// </summary>
        /// <param name="iFileName">文件名</param>
        /// <returns>字符开始索引</returns>
        private static int ParseFileLineStartIndex(string iFileName)
        {
            var retValue = -1;
            for (var i = iFileName.Length - 1; i >= 0; --i)
            {
                var charCheck = iFileName[i];
                var isNumber = UtilsTools.IsNumber(charCheck);
                if (isNumber)
                {
                    retValue = i;
                }
                else
                {
                    if (retValue != -1)
                    {
                        break;
                    }
                }
            }
            return retValue;
        }

    }

}

