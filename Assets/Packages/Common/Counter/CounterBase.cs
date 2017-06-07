using UnityEngine;
using System.Collections;

namespace Common {

	/// <summary>
	/// 计数器状态.
	/// </summary>
	public enum TCounterState {
		/// <summary>
		/// 无.
		/// </summary>
		None,
		/// <summary>
		/// 初始化.
		/// </summary>
		Init,
		/// <summary>
		/// 开始计数.
		/// </summary>
		Start,
		/// <summary>
		/// 技术中.
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
	public enum TCounterType {
		/// <summary>
		/// 时间计数器.
		/// </summary>
		TimeCounter,
		/// <summary>
		/// 数字计数器.
		/// </summary>
		NumberCounter
	}

	/// <summary>
	/// 计数器模式.
	/// </summary>
	public enum TCounterMode {
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
	public abstract class CounterBase<T> {

		/// <summary>
		/// 类型.
		/// </summary>
		protected TCounterType Type = TCounterType.NumberCounter;

		/// <summary>
		/// 模式.
		/// </summary>
		protected TCounterMode Mode = TCounterMode.CountDown;

		/// <summary>
		/// 模式.
		/// </summary>
		protected TCounterState State = TCounterState.None;

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
		public abstract void InitCounter(
			T iMaxValue, TCounterType iType, TCounterMode iMode = TCounterMode.CountDown);
	
		/// <summary>
		/// 开始计数器.
		/// </summary>
		public abstract void StartCounter ();

		/// <summary>
		/// 更新计数器.
		/// </summary>
		/// <param name="iDeltaValue">变化值.</param>
		public abstract void UpdateCounter (T iDeltaValue);

		/// <summary>
		/// 结束计数器.
		/// </summary>
		public abstract void EndCounter ();

		/// <summary>
		/// 重置计数器.
		/// </summary>
		public abstract void ResetCounter ();

		/// <summary>
		/// 取得计数器信息（根据类型不同，格式也不同）.
		/// </summary>
		public abstract string GetCounterInfo ();

	}

	/// <summary>
	/// 计数器.
	/// </summary>
	public class Counter<T> : CounterBase<T> {

		/// <summary>
		/// 根据变化值更新计数器.
		/// </summary>
		/// <param name="iDeltaVaule">变化值.</param>
		protected virtual void UpdateCounterByDeltaValue(T iDeltaVaule) {
		}

		#region Implement

		/// <summary>
		/// 初始化计数器.
		/// </summary>
		/// <param name="iMaxValue">最大值.</param>
		/// <param name="iType">类型.</param>
		/// <param name="iMode">模式.</param>
		public override void InitCounter(
			T iMaxValue, TCounterType iType, TCounterMode iMode = TCounterMode.CountDown) {
			
			this.MaxValue = iMaxValue;
			this.Type = iType;
			this.Mode = iMode;

			this.State = TCounterState.Init;
		}

		/// <summary>
		/// 开始计数器.
		/// </summary>
		public override void StartCounter () {
			this.State = TCounterState.Start;
			switch (this.Mode) {
			case TCounterMode.CountDown:
				{
					this.Value = this.MaxValue;
				}
				break;
			case TCounterMode.CountUp:
				{
					this.Value = default(T);
				}
				break;
			default:
				break;
			}
		}

		/// <summary>
		/// 更新计数器.
		/// </summary>
		/// <param name="iDeltaValue">变化值.</param>
		public override void UpdateCounter (T iDeltaValue) {
			// 尚未开始
			if((TCounterState.Start != this.State) && 
				(TCounterState.Counting != this.State)) {
				return;
			}
			this.State = TCounterState.Counting;

			// 根据变化值更新计数器
			this.UpdateCounterByDeltaValue (iDeltaValue);

		}

		/// <summary>
		/// 结束计数器.
		/// </summary>
		public override void EndCounter () {
			this.State = TCounterState.End;
			switch (this.Mode) {
			case TCounterMode.CountDown:
				{
					this.Value = this.MaxValue;
				}
				break;
			case TCounterMode.CountUp:
				{
					this.Value = default(T);
				}
				break;
			default:
				break;
			}
		}

		/// <summary>
		/// 重置计数器.
		/// </summary>
		public override void ResetCounter () {

			this.State = TCounterState.Init;
			this.Value = default(T);
		}

		/// <summary>
		/// 取得计数器信息（根据类型不同，格式也不同）.
		/// </summary>
		public override string GetCounterInfo () {
			return null;
		}

		#endregion
	};
}
