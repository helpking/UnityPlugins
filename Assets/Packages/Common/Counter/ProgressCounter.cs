using System;
using System.Collections.Generic;
using System.Linq;
using Packages.Common.Base;
using Packages.Utils;

namespace Packages.Common.Counter
{
    /// <summary>
    /// 进度计数
    /// </summary>
    public class ProgressCountStep : JsonDataBase<ProgressCountStep>
    {
        /// <summary>
        /// Step
        /// </summary>
        public int Step;
        /// <summary>
        /// 描述
        /// </summary>
        public string Description;
        /// <summary>
        /// 当前计数
        /// </summary>
        public int CurCount;
        /// <summary>
        /// 最大计数
        /// </summary>
        public int MaxCount;
    } 
    
    /// <summary>
    /// 进度计数器
    /// </summary>
    public class ProgressCounter : Counter<int>
    {

        protected List<ProgressCountStep> Steps = new List<ProgressCountStep>();

        private Action<int, int, long, string> _progressUpdated = (iCurCount, iMaxCount, iDeltaTime, iDes) => {};
        /// <summary>
        /// 进度更新回调事件.
        /// </summary>
        public event Action<int, int, long, string> OnProgressUpdated
        {
            add => _progressUpdated += value;
            remove
            {
                if (null != _progressUpdated) _progressUpdated -= value;
            }
        }

        private Action<int, int, long> _progressEnd = (iCurCount, iMaxCount, iDeltaTime) => {};
        /// <summary>
        /// 进度更新回调事件.
        /// </summary>
        public event Action<int, int, long> OnProgressEnd
        {
            add => _progressEnd += value;
            remove
            {
                if (null != _progressEnd) _progressEnd -= value;
            }
        }

        /// <summary>
        /// 当前Step
        /// </summary>
        public int CurStep;
        
        /// <summary>
        /// 创建时间计数器.
        /// </summary>
        /// <param name="iSteps">步数列表.</param>
        /// <param name="iAutoClear">自动清空.</param>
        /// <param name="iOnProgressUpdated">进度更新回调函数.</param>
        /// <param name="iOnProgressEnd">进度结束回调函数.</param>
        public static ProgressCounter Create(
            IEnumerable<ProgressCountStep> iSteps, bool iAutoClear,
            Action<int, int, long, string> iOnProgressUpdated = null, 
            Action<int, int, long> iOnProgressEnd = null) {
            
            var objRet = new ProgressCounter ();
            objRet.InitCounter (iSteps, CounterType.ProgressCount);
            if (iOnProgressUpdated == null) return objRet;
            objRet.AutoClear = iAutoClear;
            objRet.OnProgressUpdated += iOnProgressUpdated;
            objRet.OnProgressEnd += iOnProgressEnd;
            return objRet;
        }
        
        /// <summary>
        /// 重置最大计数
        /// </summary>
        protected void ResetMaxValue()
        {
            Value = 0;
            MaxValue = 0;
            foreach (var step in Steps)
            {
                Value += step.CurCount;
                MaxValue += step.MaxCount;
            }
        }

        /// <summary>
        /// 追加计数Step
        /// </summary>
        /// <param name="iStep">Step</param>
        public void AddCountStep(ProgressCountStep iStep)
        {
            
        }

        /// <summary>
        /// 追加Step信息
        ///   备注：若累加标志位为true，则最大计数累加
        /// </summary>
        /// <param name="iStep">Step</param>
        /// <param name="iMaxCount">最大计数</param>
        /// <param name="iAccumulation">累加标志位(true:累加最大计数)</param>
        public void AddStepInfo(int iStep, int iMaxCount, bool iAccumulation = false)
        {
            if (!Steps.Exists(iO => iStep == iO.Step))
            {
                Steps.Add(new ProgressCountStep() { Step = iStep, MaxCount = iMaxCount });
            
                // 重置最大计数
                ResetMaxValue();
                return;
            }

            if (iAccumulation)
            {
                Steps.Where(iO => iStep == iO.Step).ToArray()[0].MaxCount += iMaxCount;
            }
            else
            {
                Steps.Where(iO => iStep == iO.Step).ToArray()[0].MaxCount = iMaxCount;
            }
            
            // 重置最大计数
            ResetMaxValue();
            
        }

        /// <summary>
        /// 更新Step的Count信息
        /// </summary>
        /// <param name="iStep">Step</param>
        /// <param name="iDeltaCount">变化计数</param>
        /// <param name="iResetMaxCount">最大计数重置标识位</param>
        public void UpdateStepInfo(int iStep, int iDeltaCount, bool iResetMaxCount = true)
        {
            if (!Steps.Exists(iO => iStep == iO.Step))
            {
                return;
            }
            var infos = Steps.Where(iO => iStep == iO.Step).ToArray();
            if(0 >= infos.Length) 
            {
                return;
            }
            infos[0].MaxCount += iDeltaCount;

            if (iResetMaxCount)
            {
                ResetMaxValue();
            }
        }

        /// <summary>
        /// 更新步数
        /// </summary>
        /// <param name="iStep">Step</param>
        /// <param name="iDescription">内容描述</param>
        /// <param name="iDeltaCount">变化计数</param>
        /// <returns>true:计数结束; false:尚未结束;</returns>
        public bool UpdateByStep(
            int iStep, 
            string iDescription = null,
            int iDeltaCount = 1)
        {
            CurStep = iStep;
            if (!Steps.Exists(iO => iStep == iO.Step))
            {
                EndCounter ();
                return true;
            }

            var infos = Steps.Where(iO => iStep == iO.Step).ToArray();
            if(0 >= infos.Length) 
            {
                EndCounter ();
                return true;
            }
            var info = infos[0];
            info.Description = iDescription;
            info.CurCount += iDeltaCount;

            // 更新Count
            return UpdateCounter(iDeltaCount);
        }

        /// <summary>
        /// 更新计数器.
        /// </summary>
        /// <param name="iDeltaValue">变化值.</param>
        /// <returns>true:计数结束; false:尚未结束;</returns>
        [Obsolete]
        public override bool UpdateCounter(int iDeltaValue)
        {
            return base.UpdateCounter(iDeltaValue);
        }

#region Implement

        /// <summary>
        /// 初始化计数器.
        /// </summary>
        /// <param name="iSteps">步数列表.</param>
        /// <param name="iType">类型.</param>
        /// <param name="iInterval">更新时间间隔(单位:100毫微秒).</param>
        public void InitCounter(
            IEnumerable<ProgressCountStep> iSteps, CounterType iType,
            long iInterval = 8000000L)
        {
            if (null != Steps)
            {
                Steps.Clear(); 
                Steps.AddRange(iSteps);
            }

            // 更新
            void Updated(int iCurCount, int iMaxCount)
            {
                var desc = $"状态:{iCurCount}/{iMaxCount}";
                // 取得当前Step描述
                var infos = Steps.Where(iO => CurStep == iO.Step).ToArray();
                var deltaTime = UtilsDateTime.GetDisplayDeltaTime(DeltaTime);
                if (0 < infos.Length) desc = $"(+{deltaTime}) {desc}:{infos[0].Description}";
                _progressUpdated(iCurCount, iMaxCount, DeltaTime, desc);
            }
            OnCounterUpdated += Updated; 
            
            // 结束
            void End(int iCurCount, int iMaxCount)
            {
                _progressEnd(iCurCount, iMaxCount, DeltaTime);
            }
            OnCounterEnd += End;
            
            // 重置最大计数
            ResetMaxValue();
            // 初始化
            base.InitCounter(MaxValue, iType, CounterMode.CountUp, iInterval);
        }
        
        /// <summary>
        /// 开始计数器.
        /// </summary>
        public void StartCounter (string iStatus) {
            base.StartCounter();
            
            var desc = $"{iStatus}";
            // 取得当前Step描述
            var infos = Steps.Where(iO => CurStep == iO.Step).ToArray();
            var deltaTime = UtilsDateTime.GetDisplayDeltaTime(DeltaTime);
            if (0 < infos.Length) desc = $"(+{deltaTime}) {desc}:{infos[0].Description}";
            _progressUpdated(Value, MaxValue, DeltaTime, desc);
            
        }
        
        /// <summary>
        /// 开始计数器.
        /// </summary>
        public override void StartCounter () {
            base.StartCounter();
            
            var desc = $"状态:{Value}/{MaxValue}";
            // 取得当前Step描述
            var infos = Steps.Where(iO => CurStep == iO.Step).ToArray();
            var deltaTime = UtilsDateTime.GetDisplayDeltaTime(DeltaTime);
            if (0 < infos.Length) desc = $"(+{deltaTime}) {desc}:{infos[0].Description}";
            _progressUpdated(Value, MaxValue, DeltaTime, desc);
            
        }

        /// <summary>
        /// 是否已经超过计数.
        /// </summary>
        /// <returns><c>true</c>, 已经超过计数, <c>false</c> 尚未超过计数.</returns>
        public override bool isCountOver()
        {
            return CounterState.End == State || Value >= MaxValue;
        }

        /// <summary>
        /// 根据变化值更新计数器.
        /// </summary>
        /// <param name="iDeltaVaule">变化值.</param>
        protected override void UpdateCounterByDeltaValue(int iDeltaVaule)
        {
            Value += iDeltaVaule;
        }
        
#endregion
    }
}


