using System;
using Packages.Common.Base;

namespace Packages.Common.Counter {

	/// <summary>
	/// 计数器状态.
	/// </summary>
	public enum CounterState {
		/// <summary>
		/// 无.
		/// </summary>
		None,
		/// <summary>
		/// 闲置.
		/// </summary>
		Idle,
		/// <summary>
		/// 计数中.
		/// </summary>
		Counting,
		/// <summary>
		/// 结束计数.
		/// </summary>
		End
	}

	/// <summary>
	/// 计数器类型.
	/// </summary>
	public enum CounterType {
		/// <summary>
		/// 时间计数器.
		/// </summary>
		TimeCounter,
		/// <summary>
		/// 数字计数器.
		/// </summary>
		NumberCounter,
		/// <summary>
		/// 进度计数器
		/// </summary>
		ProgressCount
	}

	/// <summary>
	/// 计数器模式.
	/// </summary>
	public enum CounterMode {
		/// <summary>
		/// 倒计时.
		/// </summary>
		CountDown,
		/// <summary>
		/// 顺计时.
		/// </summary>
		CountUp
	}

	/// <summary>
	/// 计数器基类.
	/// </summary>
	public abstract class CounterBase<T> : ClassExtension {

		/// <summary>
		/// 类型.
		/// </summary>
		protected CounterType Type = CounterType.NumberCounter;

		/// <summary>
		/// 模式.
		/// </summary>
		protected CounterMode Mode = CounterMode.CountDown;

		/// <summary>
		/// 模式.
		/// </summary>
		protected CounterState State = CounterState.None;
		
		/// <summary>
		/// 更新时间间隔
		///  单位：100毫微秒
		/// </summary>
		protected long Interval { get; set; }

		/// <summary>
		/// 开始系统时钟数(单位:100毫微秒)
		/// </summary>
		protected long StartTicks = 0L;
		
		/// <summary>
		/// 上一次更新的时钟(单位:100毫微秒)
		/// </summary>
		protected long LastUpdateTime = 0L;

		/// <summary>
		/// 变化时间(单位:100毫微秒)
		/// </summary>
		protected long DeltaTime
		{
			get
			{
				var delTime = 0L;
				if (0L >= StartTicks) return delTime;
				delTime = DateTime.Now.Ticks - StartTicks;
				return 0L >= delTime ? 0L : delTime;
			}
		}

		/// <summary>
		/// 空闲标志位.
		/// </summary>
		public bool IsIdle => CounterState.Idle == State;

		/// <summary>
		/// 计数标志位
		/// </summary>
		public bool IsCounting => CounterState.Counting == State;

		/// <summary>
		/// 循环计数标志位.
		/// </summary>
		protected bool IsLoop = false;

		/// <summary>
		/// 最大值.
		/// </summary>
		protected T MaxValue = default(T);

		/// <summary>
		/// 值.
		/// </summary>
		protected T Value = default(T);

		/// <summary>
		/// 初始化计数器.
		/// </summary>
		/// <param name="iMaxValue">最大值.</param>
		/// <param name="iType">类型.</param>
		/// <param name="iMode">模式.</param>
		/// <param name="iInterval">更新时间间隔(单位:100毫微秒).</param>
		public abstract void InitCounter(
			T iMaxValue, CounterType iType, 
			CounterMode iMode = CounterMode.CountDown,
			long iInterval = 0L);
	
		/// <summary>
		/// 开始计数器.
		/// </summary>
		public abstract void StartCounter ();

		/// <summary>
		/// 更新计数器.
		/// </summary>
		/// <returns><c>true</c>, 已经溢出, <c>false</c> 尚未溢出.</returns>
		/// <param name="iDeltaValue">变化值.</param>
		public abstract bool UpdateCounter (T iDeltaValue);

		/// <summary>
		/// 结束计数器.
		/// </summary>
		public abstract void EndCounter ();

		/// <summary>
		/// 重置计数器.
		/// </summary>
		public abstract void ResetCounter ();

		/// <summary>
		/// 重启计数器.
		/// </summary>
		public abstract void RestartCounter ();

		/// <summary>
		/// 取得计数器值（根据类型不同，格式也不同）.
		/// </summary>
		public abstract string GetCounterValue ();

	}

	/// <summary>
	/// 计数器.
	/// </summary>
	public abstract class Counter<T> : CounterBase<T> {
		
		/// <summary>
		/// 自动清空
		/// </summary>
		public bool AutoClear { get; set; }

		protected Action<T, T> CounterUpdated = (iCurCount, iMaxCount) => {};
		/// <summary>
		/// 计数更新回调函数.
		/// </summary>
		protected event Action<T, T> OnCounterUpdated {
			add => CounterUpdated += value;
			remove => CounterUpdated = value;
		}
		
		protected System.Action<T, T> CounterEnd = (iCurCount, iMaxCount) => {};
		/// <summary>
		/// 计数计数回调函数.
		/// </summary>
		protected event System.Action<T, T> OnCounterEnd {
			add => CounterEnd += value;
			remove => CounterEnd = value;
		}

#region abstract

		/// <summary>
		/// 是否已经超过计数.
		/// </summary>
		/// <returns><c>true</c>, 已经超过计数, <c>false</c> 尚未超过计数.</returns>
		public abstract bool isCountOver();

		/// <summary>
		/// 根据变化值更新计数器.
		/// </summary>
		/// <param name="iDeltaVaule">变化值.</param>
		protected abstract void UpdateCounterByDeltaValue(T iDeltaVaule);

#endregion

#region Implement

		/// <summary>
		/// 初始化计数器.
		/// </summary>
		/// <param name="iMaxValue">最大值.</param>
		/// <param name="iType">类型.</param>
		/// <param name="iMode">模式.</param>
		/// <param name="iInterval">更新时间间隔(单位:100毫微秒).</param>
		public override void InitCounter(
			T iMaxValue, 
			CounterType iType, 
			CounterMode iMode = CounterMode.CountDown,
			long iInterval = 0L) {
			
			MaxValue = iMaxValue;
			Type = iType;
			Mode = iMode;

			// 默认更新时间间隔为0
			Interval = iInterval;

			State = CounterState.Idle;

			switch (Mode) {
			case CounterMode.CountDown:
				{
					Value = MaxValue;
				}
				break;
			case CounterMode.CountUp:
				{
					Value = default(T);
				}
				break;
			default:
				break;
			}
		}

		/// <summary>
		/// 开始计数器.
		/// </summary>
		public override void StartCounter () {
			State = CounterState.Counting;
			// 记录开始时钟数（单位:毫微秒）
			StartTicks = DateTime.Now.Ticks;
			Info ("StartCounter():Type::{0} Mode::{1} State::{2} Value::{3}/{4}",
				Type, Mode, State, Value, MaxValue);
		}

		/// <summary>
		/// 更新计数器.
		/// </summary>
		/// <param name="iDeltaValue">变化值.</param>
		/// <returns>true:计数结束; false:尚未结束;</returns>
		public override bool UpdateCounter (T iDeltaValue) {

			// 尚未开始计数
			if (CounterState.Counting != State) {
				return false;
			}
			// 更新当前状态为：计数中
			State = CounterState.Counting;
			// 根据变化值更新计数器
			UpdateCounterByDeltaValue (iDeltaValue);
			
			// 超过计数
			if (isCountOver())
			{
				// 回调更新
				CounterUpdated (Value, MaxValue);
				if(AutoClear) EndCounter ();
				return true;
			}

			// 临时保存更新时间
			var curUpdateTime = DateTime.Now.Ticks;
			if (0L == LastUpdateTime)
			{
				LastUpdateTime = DateTime.Now.Ticks;
			}
			// 计算前后更新变化时间，若超过更新时间间隔，则更新
			var deltaUpdateTime = curUpdateTime - LastUpdateTime;
			if (Interval >= deltaUpdateTime)
			{
				return false;
			}
			LastUpdateTime = curUpdateTime;

			// 回调更新
			CounterUpdated (Value, MaxValue);
			return false;
		}

		/// <summary>
		/// 结束计数器.
		/// </summary>
		public override void EndCounter () {
			if (CounterState.Counting == State) {
				Info ("EndCounter():Type::{0} Mode::{1} State::{2} Value::{3}/{4}",
					Type, Mode, State, Value, MaxValue);
			}
			State = CounterState.End;
			CounterEnd(Value, MaxValue);
		}

		/// <summary>
		/// 重置计数器.
		/// </summary>
		public override void ResetCounter () {
			InitCounter (MaxValue, Type, Mode);
		}
			
		/// <summary>
		/// 重启计数器.
		/// </summary>
		public override void RestartCounter () {
			// 重置计数器
			ResetCounter();
			// 开始计数
			StartCounter();
			Info ("RestartCounter():Type::{0} Mode::{1} State::{2} Value::{3}/{4}",
				Type, Mode, State, Value, MaxValue);
		}

		/// <summary>
		/// 取得计数器值（根据类型不同，格式也不同）.
		/// </summary>
		public override string GetCounterValue () {
			return Value.ToString();
		}

#endregion
	}
}
