using System.Collections;
using Packages.Common.Extend;
using UnityEngine;
using Packages.Logs;

namespace Packages.Logs
{
    /// <summary>
    /// 日志测试脚本
    /// </summary>
    [AddComponentMenu("Packages/Logs/LogTest")]
    public class LogTest : MonoBehaviour
    {
        public int TestLogCount = 0;
        
        private void OnEnable()
        {
#if UNITY_EDITOR
            Loger.BuildStart("OnEnable");
            Loger.BuildLog("log");
            Loger.BuildWarningLog("Warning");
            Loger.BuildErrorLog("Error");
            Loger.BuildEnd();
#endif
            Loger.Test("OnEnable");
            this.Info("OnEnable():{0}", "Log args test(string - Info)");
            this.LInfo("OnEnable():{0}", "Log args test(string - LInfo)");
            this.Warning("OnEnable():{0}", "Log args test(string - Warning)");
            this.Error("OnEnable():{0}", "Log args test(string - Error)");
            this.Fatal("OnEnable():{0}", "Log args test(string - Fatal)");
        }

//         void Awake()
//         {
// #if UNITY_EDITOR
//             Loger.BuildStart("Awake()");
//             Loger.BuildLog("log");
//             Loger.BuildWarningLog("Warning");
//             Loger.BuildErrorLog("Error");
//             Loger.BuildEnd();
// #endif
//             TestLogCount = 0;
//             Loger.Test("Awake");
//             this.Info("Awake");
//             this.LInfo("Awake");
//             this.Warning("Awake");
//             this.Error("Awake");
//             this.Fatal("Awake");   
//         }
//
//         void Start () 
//         {
// #if UNITY_EDITOR
//             Loger.BuildStart("Start()");
//             Loger.BuildLog("log");
//             Loger.BuildWarningLog("Warning");
//             Loger.BuildErrorLog("Error");
//             Loger.BuildEnd();
// #endif
//             Loger.Test("Start");
//             this.Info("Start");
//             this.LInfo("Start");
//             this.Warning("Start");
//             this.Error("Start");
//             this.Fatal("Start");
//
//             StartCoroutine(TestLog());
//         }
//
//         private IEnumerator TestLog()
//         {
//             while (10000 >= TestLogCount)
//             {
//                 this.Info("Start:TestLog - {0}", TestLogCount);
//                 this.Warning("Start:TestLog - {0}", TestLogCount);
//                 this.Error("Start:TestLog - {0}", TestLogCount);
//                 this.Fatal("Start:TestLog - {0}", TestLogCount);
//                 ++TestLogCount; 
//                 yield return new WaitForEndOfFrame();
//
//             }
//             yield return new WaitForEndOfFrame();
//         }
    } 

}
