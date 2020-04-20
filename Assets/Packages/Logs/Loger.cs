using System;
using System.IO;
using System.Collections.Generic;
using Unity.Jobs;
using UnityEngine;
using Packages.Utils;
using Unity.Collections;

namespace Packages.Logs
{
    /// <summary>
    /// Log日志等级定义（级别越高，日志输出越少。）.
    /// 输出的日志只输出大于等于当前日志级别的日志。
    /// </summary>
    public enum LogLevel {
        /// <summary>
        /// 全部输出(A).
        /// </summary>
        All = 0,
        /// <summary>
        /// Debug(T).
        /// </summary>
        Debug = 1,
        Test = Debug,
        /// <summary>
        /// 信息:运行(RI).
        /// </summary>
        RInfo = 2,
        /// <summary>
        /// 警告(W).
        /// </summary>
        Warning = 3,
        /// <summary>
        /// 信息:逻辑(LI).
        /// </summary>
        LInfo = 4,
        /// <summary>
        /// 错误(E).
        /// </summary>
        Error = 5,
        /// <summary>
        /// 致命日志(F).
        /// </summary>
        Fatal = 6,
        /// <summary>
        /// 全关闭.
        /// </summary>
        Off = 7
    }

    /// <summary>
    /// 日志Job
    /// </summary>
    public struct LogJob : IJob
    {
        /// <summary>
        /// 日志No
        /// </summary>
        public long LogNo;
        /// <summary>
        /// 上一次系统时间(单位:ms)
        /// </summary>
        public long LastSysDatetime;
        /// <summary>
        /// 类型颜色
        /// </summary>
        public NativeArray<byte> TypeColorData;
        /// <summary>
        /// 日志等级
        /// </summary>
        public LogLevel LogLevel;
        /// <summary>
        /// 内容
        /// </summary>
        public NativeArray<byte> ContentData;
        /// <summary>
        /// 日志是否输出为Log文件标志位
        /// </summary>
        public int Output;
        /// <summary>
        /// 日志文件输出路径
        /// </summary>
        public NativeArray<byte> OutputDirData;
        /// <summary>
        /// 日志文件上限(单位:byte)
        /// </summary>
        public float LogFileMaxSize;
        
        /// <summary>
        /// 取得系统日期及前后变化时间戳.
        /// </summary>
        /// <returns>The current datatime.</returns>
        private string GetCurDatetime()
        {
            var curDateTime = DateTime.Now.Ticks / 10000;
            var delTime = 0L;
            if (0L < LastSysDatetime)
            {
                delTime = curDateTime - LastSysDatetime;
            }
            return $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}(+{delTime} ms)";
        }

        /// <summary>
        /// Job执行方法
        /// </summary>
        public void Execute()
        {
            var logTime = GetCurDatetime();
            var logTag = "[RI]";
            switch (LogLevel)
            {
                case LogLevel.Debug:
                    logTag = "[T]"; 
                    break;
                case LogLevel.Warning:
                    logTag = "[W]"; 
                    break;
                case LogLevel.LInfo:
                    logTag = "[LI]"; 
                    break;
                case LogLevel.Error:
                    logTag = "[E]"; 
                    break;
                case LogLevel.Fatal:
                    logTag = "[F]"; 
                    break;
                default:
                    logTag = "[RI]";
                    break;
            }
#if UNITY_ANDROID
            if (Application.isMobilePlatform)
            {
                logTag = "";
            }
            var log = $"{logTime}[{LogNo}]{logTag} {System.Text.Encoding.UTF8.GetString(ContentData.ToArray())}";
#else 
            var log = $"{logTime}[{LogNo}]{logTag} {System.Text.Encoding.UTF8.GetString(ContentData.ToArray())}";
#endif
#if UNITY_EDITOR
            var info = $"<color=\"{System.Text.Encoding.UTF8.GetString(TypeColorData.ToArray())}\">{log}</color>";
#else
            var info = $"{log}";
#endif
            if (LogLevel.Warning == LogLevel)
            {
                Debug.LogWarning(info);
            } 
            else if (LogLevel.Error == LogLevel || LogLevel.Fatal == LogLevel)
            {
                Debug.LogError(info);
            }
            else
            {
                Debug.Log(info);
            }
            
            
            // 日志文件输出
            if(1 != Output) return;
            var outputDir = System.Text.Encoding.UTF8.GetString(OutputDirData.ToArray()); 
            var curDatetime = $"{DateTime.Now:yyyyMMddHH}";
            // 检测日志输出目录
            if (!Directory.Exists(outputDir))
            {
                UtilsTools.CheckAndCreateDirByFullDir(outputDir);
            }
            string outputLogFile;
            var index = 0;
            while (true)
            {
                outputLogFile = $"{outputDir}/Run_{curDatetime}_{index}.log";
                ++index;
                if (!File.Exists(outputLogFile)) break;
                // 若文件已存在则判断文件大小
                var fileInfo = new FileInfo(outputLogFile);
                // 尚未超过文件大小上限
                if (LogFileMaxSize <= fileInfo.Length) continue;
                break;
            }
            
            var fs = new FileStream (outputLogFile, FileMode.Append, FileAccess.Write);
            var sw = new StreamWriter(fs);
            
            // 写入日志
            sw.WriteLine(log);
            sw.Flush ();
            
            // 关闭
            sw.Close ();
            sw.Dispose ();
            fs.Close ();
            fs.Dispose ();
        }

        /// <summary>
        /// 释放
        /// </summary>
        public void Dispose()
        {
            TypeColorData.Dispose();
            ContentData.Dispose();
            OutputDirData.Dispose();
        }
    }

    /// <summary>
    /// 打包日志Job
    /// </summary>
    public struct BuildLogJob : IJob {
    /// <summary>
        /// 日志No
        /// </summary>
        public long LogNo;
        /// <summary>
        /// 上一次系统时间(单位:ms)
        /// </summary>
        public long LastSysDatetime;
        /// <summary>
        /// 类型颜色
        /// </summary>
        public NativeArray<byte> TypeColorData;
        /// <summary>
        /// 日志等级
        /// </summary>
        public LogLevel LogLevel;
        /// <summary>
        /// 日志范围
        /// </summary>
        public NativeArray<byte> ScopeData;
        /// <summary>
        /// 内容
        /// </summary>
        public NativeArray<byte> ContentData;
        /// <summary>
        /// 日志是否输出为Log文件标志位
        /// </summary>
        public int Output;
        /// <summary>
        /// 日志文件输出路径
        /// </summary>
        public NativeArray<byte> OutputDirData;
        /// <summary>
        /// 日志文件上限(单位:byte)
        /// </summary>
        public float LogFileMaxSize;
        
        /// <summary>
        /// 取得系统日期及前后变化时间戳.
        /// </summary>
        /// <returns>The current datatime.</returns>
        private string GetCurDatetime()
        {
            var curDateTime = UtilsDateTime.GetMilliseconL(DateTime.Now.Ticks);
            var delTime = 0L;
            if (0L < LastSysDatetime)
            {
                delTime = curDateTime - LastSysDatetime;
            }
            return $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}(+{delTime} ms)";
        }

        /// <summary>
        /// Job执行方法
        /// </summary>
        public void Execute()
        {
            var logTime = GetCurDatetime();
            var log = $"{logTime}[{LogNo}][B] {System.Text.Encoding.UTF8.GetString(ScopeData.ToArray())} {System.Text.Encoding.UTF8.GetString(ContentData.ToArray())}";
            var info = $"<color=\"{System.Text.Encoding.UTF8.GetString(TypeColorData.ToArray())}\">{log}</color>";
            if (LogLevel.Warning == LogLevel)
            {
                Debug.LogWarning(info);
            } 
            else if (LogLevel.Error == LogLevel || LogLevel.Fatal == LogLevel)
            {
                Debug.LogError(info);
            }
            else
            {
                Debug.Log(info);
            }
            
            
            // 日志文件输出
            if(1 != Output) return;
            var outputDir = System.Text.Encoding.UTF8.GetString(OutputDirData.ToArray()); 
            var curDatetime = $"{DateTime.Now:yyyyMMddHH}";
            // 检测日志输出目录
            if (!Directory.Exists(outputDir))
            {
                UtilsTools.CheckAndCreateDirByFullDir(outputDir);
            }
            string outputLogFile;
            var index = 0;
            while (true)
            {
                outputLogFile = $"{outputDir}/Build_{curDatetime}_{index}.log";
                ++index;
                if (!File.Exists(outputLogFile)) break;
                // 若文件已存在则判断文件大小
                var fileInfo = new FileInfo(outputLogFile);
                // 尚未超过文件大小上限
                if (LogFileMaxSize <= fileInfo.Length) continue;
                break;
            }
            
            var fs = new FileStream (outputLogFile, FileMode.Append, FileAccess.Write);
            var sw = new StreamWriter(fs);
            
            // 写入日志
            sw.WriteLine(log);
            sw.Flush ();
            
            // 关闭
            sw.Close ();
            sw.Dispose ();
            fs.Close ();
            fs.Dispose ();
        }

        /// <summary>
        /// 释放
        /// </summary>
        public void Dispose()
        {
            TypeColorData.Dispose();
            ScopeData.Dispose();
            ContentData.Dispose();
            OutputDirData.Dispose();
        }
    }
    
    /// <summary>
    /// 自定义日志器
    /// </summary>
    public class Loger
    {
        /// <summary>
        /// 打包日志前缀
        ///   文件命名规则 : Build_YYYYMMDDHH_N.log
        /// </summary>
        public const string BUILD_LOG_PREFIX = "Build";
        /// <summary>
        /// 运行日志前缀
        ///   文件命名规则 : Run_YYYYMMDDHH_N.log
        /// </summary>
        public const string RUN_LOG_PREFIX = "Run";
        
        /// <summary>
        /// 日志文件默认大小上限(单位:MB)
        /// </summary>
        private const int _DEFAULT_LOG_FILE_MAX_SIZE = 2;
        private const string _PLAYERPREFS_KEY_LOG_FILE_MAX_SIZE = "__Log_file_max_size__";
        private static float _logFileMaxSize = 0.0f;
        private static float _logFileCheckSize = 0.0f;

        /// <summary>
        /// 日志文件校验大小
        /// </summary>
        private static float LogFileCheckSize
        {
            get
            {
                if (0.0f >= _logFileCheckSize)
                {
                    _logFileCheckSize = LogFileMaxSize * 1024 * 1024;
                }
                return _logFileCheckSize;
            }
        }
        /// <summary>
        /// 日志文件大小上限(单位:MB).
        /// </summary>
        public static float LogFileMaxSize
        {
            get
            {
                if (0.0f >= _logFileMaxSize)
                {
                    _logFileMaxSize = PlayerPrefs.GetFloat(_PLAYERPREFS_KEY_LOG_FILE_MAX_SIZE, _DEFAULT_LOG_FILE_MAX_SIZE);
                }
                return _logFileMaxSize;
            }
            set
            {
                _logFileMaxSize = value;
                _logFileCheckSize = value * 1024 * 1024;
                PlayerPrefs.SetFloat(_PLAYERPREFS_KEY_LOG_FILE_MAX_SIZE, value);
            }
        }
        
        private const string _PLAYERPREFS_KEY_LOGLEVEL = "__Log_Level__";
        /// <summary>
        /// 日志等级.
        /// </summary>
        public static LogLevel LogLevel
        {
            get => (LogLevel)PlayerPrefs.GetInt(_PLAYERPREFS_KEY_LOGLEVEL, (int) LogLevel.All);
            set => PlayerPrefs.SetInt(_PLAYERPREFS_KEY_LOGLEVEL, (int)value);
        }
        
        private const string _PLAYERPREFS_KEY_LOG_OUTPUT_FLG = "__Log_OUTPUT_FLG__";
        /// <summary>
        /// 日志输出标识位
        /// </summary>
        public static bool LogOutput  
        {
            get => PlayerPrefs.GetInt(_PLAYERPREFS_KEY_LOG_OUTPUT_FLG, 0) == 1;
            set => PlayerPrefs.SetInt(_PLAYERPREFS_KEY_LOG_OUTPUT_FLG, value ? 1 : 0);
        }
        /// <summary>
        /// 日志输出目录
        /// </summary>
        public static string OutputDir = $"{Application.dataPath}/../Output/Logs/";

        private static string _buildOutputDir;

        /// <summary>
        /// 日志输出目录
        /// </summary>
        public static string BuildOutputDir
        {
            get
            {
                if (string.IsNullOrEmpty(_buildOutputDir)) _buildOutputDir = OutputDir;
                return _buildOutputDir;
            }
            set => _buildOutputDir = value;
        }
        /// <summary>
        /// 日志No.
        /// </summary>
        private static long _logNo = -1;
        /// <summary>
        /// 当前系统时刻（单位：毫秒)
        /// </summary>
        private static long _curDateTime = 0L;
        /// <summary>
        /// 打包日志颜色- 蓝紫
        /// </summary>
        private const string buildColor = "cyan";
        /// <summary>
        /// 错误日志颜色
        /// </summary>
        private const string errorColor = "red";
        /// <summary>
        /// 警告日志颜色
        /// </summary>
        private const string warningColor = "orange";
        /// <summary>
        /// 一般日志颜色
        /// </summary>
        private const string infoColor = "white";
#region Build - Only

#if UNITY_EDITOR
        /// <summary>
        /// 打包Scope栈
        /// </summary>
        private static Stack<string> buildScopeStack = new Stack<string>(); 
        
        /// <summary>
        /// 信息日志(打包日志开始 - B) .
        /// </summary>
        /// <param name="iScope">打包范围.</param>
        /// <param name="iLog">日志.</param>
        public static void BuildStart(string iScope, string iLog = null)
        {
            // 保存进栈
            buildScopeStack.Push(iScope);
            if (LogLevel > LogLevel.RInfo) {
                return;
            }
            ++_logNo;
            var scope = $" + {iScope}";
            var jobData = new BuildLogJob()
            {
                LogNo = _logNo, 
                LastSysDatetime = _curDateTime,
                ScopeData = new NativeArray<byte>(System.Text.Encoding.UTF8.GetBytes(scope), Allocator.TempJob),
                TypeColorData = new NativeArray<byte>(System.Text.Encoding.UTF8.GetBytes(buildColor), Allocator.TempJob),
                LogLevel = LogLevel.RInfo,
                Output = LogOutput ? 1 : 0,
                OutputDirData = new NativeArray<byte>(System.Text.Encoding.UTF8.GetBytes(OutputDir), Allocator.TempJob),
                LogFileMaxSize = LogFileCheckSize
            };
            if (!string.IsNullOrEmpty(iLog))
            {
                jobData.ContentData =
                    new NativeArray<byte>(System.Text.Encoding.UTF8.GetBytes(iLog), Allocator.TempJob);
            }
            else
            {
                jobData.ContentData =
                    new NativeArray<byte>(System.Text.Encoding.UTF8.GetBytes(""), Allocator.TempJob); 
            }
            // 刷新当前执行时间
            _curDateTime = DateTime.Now.Ticks / 10000;
            
            // 执行Job
            var handle = jobData.Schedule();
            handle.Complete();
            jobData.Dispose();
        }
        
        /// <summary>
        /// 信息日志(打包日志 - B).
        /// </summary>
        /// <param name="iLog">日志.</param>
        public static void BuildLog(string iLog)
        {
            if (LogLevel > LogLevel.RInfo) {
                return;
            }
            ++_logNo;
            // 保存进栈
            var scope = "   (nil)";
            if (0 < buildScopeStack.Count)
            {
                var scopeTmp = buildScopeStack.Peek();
                if (!string.IsNullOrEmpty(scope))
                {
                    scope = $"   {scopeTmp}";
                }
            }
            var jobData = new BuildLogJob()
            {
                LogNo = _logNo, 
                LastSysDatetime = _curDateTime,
                ScopeData = new NativeArray<byte>(System.Text.Encoding.UTF8.GetBytes(scope), Allocator.TempJob),
                TypeColorData = new NativeArray<byte>(System.Text.Encoding.UTF8.GetBytes(buildColor), Allocator.TempJob),
                LogLevel = LogLevel.RInfo,
                Output = LogOutput ? 1 : 0,
                OutputDirData = new NativeArray<byte>(System.Text.Encoding.UTF8.GetBytes(OutputDir), Allocator.TempJob),
                LogFileMaxSize = LogFileCheckSize
            };
            if (!string.IsNullOrEmpty(iLog))
            {
                jobData.ContentData =
                    new NativeArray<byte>(System.Text.Encoding.UTF8.GetBytes(iLog), Allocator.TempJob);
            }
            else
            {
                jobData.ContentData =
                    new NativeArray<byte>(System.Text.Encoding.UTF8.GetBytes(""), Allocator.TempJob); 
            }
            // 刷新当前执行时间
            _curDateTime = DateTime.Now.Ticks / 10000;
            
            // 执行Job
            var handle = jobData.Schedule();
            handle.Complete();
            jobData.Dispose();
        }
        
        /// <summary>
        /// 信息日志(打包警告日志 - BW).
        /// </summary>
        /// <param name="iLog">日志.</param>
        public static void BuildWarningLog(string iLog)
        {
            if (LogLevel > LogLevel.Warning) {
                return;
            }
            ++_logNo;
            // 保存进栈
            var scope = " * (nil)";
            if (0 < buildScopeStack.Count)
            {
                var scopeTmp = buildScopeStack.Peek();
                if (!string.IsNullOrEmpty(scope))
                {
                    scope = $" * {scopeTmp}";
                }
            }
            var jobData = new BuildLogJob()
            {
                LogNo = _logNo, 
                LastSysDatetime = _curDateTime,
                ScopeData = new NativeArray<byte>(System.Text.Encoding.UTF8.GetBytes(scope), Allocator.TempJob),
                TypeColorData = new NativeArray<byte>(System.Text.Encoding.UTF8.GetBytes(warningColor), Allocator.TempJob),
                LogLevel = LogLevel.Warning,
                Output = LogOutput ? 1 : 0,
                OutputDirData = new NativeArray<byte>(System.Text.Encoding.UTF8.GetBytes(OutputDir), Allocator.TempJob),
                LogFileMaxSize = LogFileCheckSize
            };
            if (!string.IsNullOrEmpty(iLog))
            {
                jobData.ContentData =
                    new NativeArray<byte>(System.Text.Encoding.UTF8.GetBytes(iLog), Allocator.TempJob);
            }
            else
            {
                jobData.ContentData =
                    new NativeArray<byte>(System.Text.Encoding.UTF8.GetBytes(""), Allocator.TempJob); 
            }
            // 刷新当前执行时间
            _curDateTime = DateTime.Now.Ticks / 10000;
            
            // 执行Job
            var handle = jobData.Schedule();
            handle.Complete();
            jobData.Dispose();
        }
        
        /// <summary>
        /// 信息日志(打包错误日志 - BE).
        /// </summary>
        /// <param name="iLog">日志.</param>
        public static void BuildErrorLog(string iLog)
        {
            if (LogLevel > LogLevel.Error) {
                return;
            }
            ++_logNo;
            // 保存进栈
            var scope = " @ (nil)";
            if (0 < buildScopeStack.Count)
            {
                var scopeTmp = buildScopeStack.Peek();
                if (!string.IsNullOrEmpty(scope))
                {
                    scope = $" @ {scopeTmp}";
                }
            }
            var jobData = new BuildLogJob()
            {
                LogNo = _logNo, 
                LastSysDatetime = _curDateTime,
                ScopeData = new NativeArray<byte>(System.Text.Encoding.UTF8.GetBytes(scope), Allocator.TempJob),
                TypeColorData = new NativeArray<byte>(System.Text.Encoding.UTF8.GetBytes(errorColor), Allocator.TempJob),
                LogLevel = LogLevel.Error,
                Output = LogOutput ? 1 : 0,
                OutputDirData = new NativeArray<byte>(System.Text.Encoding.UTF8.GetBytes(OutputDir), Allocator.TempJob),
                LogFileMaxSize = LogFileCheckSize
            };
            if (!string.IsNullOrEmpty(iLog))
            {
                jobData.ContentData =
                    new NativeArray<byte>(System.Text.Encoding.UTF8.GetBytes(iLog), Allocator.TempJob);
            }
            // 刷新当前执行时间
            _curDateTime = DateTime.Now.Ticks / 10000;
            
            // 执行Job
            var handle = jobData.Schedule();
            handle.Complete();
            jobData.Dispose();
        }
        
        /// <summary>
        /// 信息日志(打包日志).
        /// </summary>
        public static void BuildEnd()
        {
            // 保存进栈
            string scope = null;
            if (0 < buildScopeStack.Count)
            {
                scope = buildScopeStack.Pop();
            }
            if (LogLevel > LogLevel.RInfo) {
                return;
            }
            ++_logNo;

            if (string.IsNullOrEmpty(scope))
            {
                scope = " - (nil)";
            }
            else
            {
                scope = $" - {scope}";
            }
            var jobData = new BuildLogJob()
            {
                LogNo = _logNo, 
                LastSysDatetime = _curDateTime,
                ScopeData = new NativeArray<byte>(System.Text.Encoding.UTF8.GetBytes(scope), Allocator.TempJob),
                TypeColorData = new NativeArray<byte>(System.Text.Encoding.UTF8.GetBytes(buildColor), Allocator.TempJob),
                ContentData = new NativeArray<byte>(System.Text.Encoding.UTF8.GetBytes(""), Allocator.TempJob),
                LogLevel = LogLevel.RInfo,
                Output = LogOutput ? 1 : 0,
                OutputDirData = new NativeArray<byte>(System.Text.Encoding.UTF8.GetBytes(OutputDir), Allocator.TempJob),
                LogFileMaxSize = LogFileCheckSize
            };
            // 刷新当前执行时间
            _curDateTime = DateTime.Now.Ticks / 10000;
            
            // 执行Job
            var handle = jobData.Schedule();
            handle.Complete();
            jobData.Dispose();
        }
        
#endif
        
#endregion
        
        /// <summary>
        /// 信息日志(测试日志).
        /// </summary>
        /// <param name="iLog">日志.</param>
        public static void Test(string iLog)
        {
            if (LogLevel > LogLevel.Debug) {
                return;
            }
            ++_logNo;
            var jobData = new LogJob
            {
                LogNo = _logNo, 
                LastSysDatetime = _curDateTime,
                TypeColorData = new NativeArray<byte>(System.Text.Encoding.UTF8.GetBytes(infoColor), Allocator.TempJob),
                LogLevel = LogLevel.Test,
                ContentData = new NativeArray<byte>(System.Text.Encoding.UTF8.GetBytes(iLog), Allocator.TempJob),
                Output = LogOutput ? 1 : 0,
                OutputDirData = new NativeArray<byte>(System.Text.Encoding.UTF8.GetBytes(OutputDir), Allocator.TempJob),
                LogFileMaxSize = LogFileCheckSize
            };
            // 刷新当前执行时间
            _curDateTime = DateTime.Now.Ticks / 10000;
            
            // 执行Job
            var handle = jobData.Schedule();
            handle.Complete();
            jobData.Dispose(); 
        }
        
        /// <summary>
        /// 信息日志(默认：运行日志).
        /// </summary>
        /// <param name="iLog">日志.</param>
        public static void Info(string iLog)
        {
            if (LogLevel > LogLevel.RInfo) {
                return;
            }
            ++_logNo;
            var jobData = new LogJob
            {
                LogNo = _logNo, 
                LastSysDatetime = _curDateTime,
                TypeColorData = new NativeArray<byte>(System.Text.Encoding.UTF8.GetBytes(infoColor), Allocator.TempJob),
                LogLevel = LogLevel.RInfo,
                ContentData = new NativeArray<byte>(System.Text.Encoding.UTF8.GetBytes(iLog), Allocator.TempJob),
                Output = LogOutput ? 1 : 0,
                OutputDirData = new NativeArray<byte>(System.Text.Encoding.UTF8.GetBytes(OutputDir), Allocator.TempJob),
                LogFileMaxSize = LogFileCheckSize
            };
            // 刷新当前执行时间
            _curDateTime = DateTime.Now.Ticks / 10000;
            
            // 执行Job
            var handle = jobData.Schedule();
            handle.Complete();
            jobData.Dispose();
        }

        /// <summary>
        /// 警告日志.
        /// </summary>
        /// <param name="iLog">日志.</param>
        public static void Warning(string iLog)
        {
            if (LogLevel > LogLevel.Warning) {
                return;
            }
            ++_logNo;
            var jobData = new LogJob
            {
                LogNo = _logNo, 
                LastSysDatetime = _curDateTime,
                TypeColorData = new NativeArray<byte>(System.Text.Encoding.UTF8.GetBytes(warningColor), Allocator.TempJob),
                LogLevel = LogLevel.Warning,
                ContentData = new NativeArray<byte>(System.Text.Encoding.UTF8.GetBytes(iLog), Allocator.TempJob),
                Output = LogOutput ? 1 : 0,
                OutputDirData = new NativeArray<byte>(System.Text.Encoding.UTF8.GetBytes(OutputDir), Allocator.TempJob),
                LogFileMaxSize = LogFileCheckSize
            };
            // 刷新当前执行时间
            _curDateTime = DateTime.Now.Ticks / 10000;
            
            // 执行Job
            var handle = jobData.Schedule();
            handle.Complete();
            jobData.Dispose(); 
        }

        /// <summary>
        /// 信息:逻辑(LI).
        /// </summary>
        /// <param name="iLog">日志.</param>
        public static void LInfo(string iLog)
        {
            if (LogLevel > LogLevel.LInfo) {
                return;
            }
            ++_logNo;
            var jobData = new LogJob
            {
                LogNo = _logNo, 
                LastSysDatetime = _curDateTime,
                TypeColorData = new NativeArray<byte>(System.Text.Encoding.UTF8.GetBytes(infoColor), Allocator.TempJob),
                LogLevel = LogLevel.LInfo,
                ContentData = new NativeArray<byte>(System.Text.Encoding.UTF8.GetBytes(iLog), Allocator.TempJob),
                Output = LogOutput ? 1 : 0,
                OutputDirData = new NativeArray<byte>(System.Text.Encoding.UTF8.GetBytes(OutputDir), Allocator.TempJob),
                LogFileMaxSize = LogFileCheckSize
            };
            // 刷新当前执行时间
            _curDateTime = DateTime.Now.Ticks / 10000;
            
            // 执行Job
            var handle = jobData.Schedule();
            handle.Complete();
            jobData.Dispose();
        }

        /// <summary>
        /// 错误日志.
        /// </summary>
        /// <param name="iLog">日志.</param>
        public static void Error(string iLog)
        {
            if (LogLevel > LogLevel.Error) {
                return;
            }
            ++_logNo;
            var jobData = new LogJob
            {
                LogNo = _logNo, 
                LastSysDatetime = _curDateTime,
                TypeColorData = new NativeArray<byte>(System.Text.Encoding.UTF8.GetBytes(errorColor), Allocator.TempJob),
                LogLevel = LogLevel.Error,
                ContentData = new NativeArray<byte>(System.Text.Encoding.UTF8.GetBytes(iLog), Allocator.TempJob),
                Output = LogOutput ? 1 : 0,
                OutputDirData = new NativeArray<byte>(System.Text.Encoding.UTF8.GetBytes(OutputDir), Allocator.TempJob),
                LogFileMaxSize = LogFileCheckSize
            };
            // 刷新当前执行时间
            _curDateTime = DateTime.Now.Ticks / 10000;
            
            // 执行Job
            var handle = jobData.Schedule();
            handle.Complete();
            jobData.Dispose();
        }

        /// <summary>
        /// 致命日志.
        /// </summary>
        /// <param name="iLog">日志.</param>
        public static void Fatal(string iLog)
        {
            if (LogLevel > LogLevel.Fatal) {
                return;
            }
            ++_logNo;
            var jobData = new LogJob
            {
                LogNo = _logNo, 
                LastSysDatetime = _curDateTime,
                TypeColorData = new NativeArray<byte>(System.Text.Encoding.UTF8.GetBytes(errorColor), Allocator.TempJob),
                LogLevel = LogLevel.Fatal,
                ContentData = new NativeArray<byte>(System.Text.Encoding.UTF8.GetBytes(iLog), Allocator.TempJob),
                Output = LogOutput ? 1 : 0,
                OutputDirData = new NativeArray<byte>(System.Text.Encoding.UTF8.GetBytes(OutputDir), Allocator.TempJob),
                LogFileMaxSize = LogFileCheckSize
            };
            // 刷新当前执行时间
            _curDateTime = DateTime.Now.Ticks / 10000;
            
            // 执行Job
            var handle = jobData.Schedule();
            handle.Complete();
            jobData.Dispose();
        }
    } 

}

