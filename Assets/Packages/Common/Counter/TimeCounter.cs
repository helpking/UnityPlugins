using Packages.Utils;

namespace Packages.Common.Counter {

	/// <summary>
	/// 时间计数器.
	/// </summary>
	public class TimeCounter : Counter<float> {

		/// <summary>
		/// 创建时间计数器.
		/// </summary>
		/// <param name="iMaxValue">计数器最大值.</param>
		/// <param name="iOnCountOver">超过计时回调函数.</param>
		/// <param name="iMode">模式（默认为：倒计时）.</param>
		public static TimeCounter Create(
			float iMaxValue, 
			System.Action<float, float> iOnCountOver = null,
			CounterMode iMode = CounterMode.CountDown) {
		
			var objRet = new TimeCounter ();
			objRet.InitCounter (iMaxValue, CounterType.TimeCounter, iMode);
			if (iOnCountOver != null) {
				objRet.OnCounterUpdated += iOnCountOver;
			}
			return objRet;
		}

		/// <summary>
		/// 构造函数（禁用外部New）.
		/// </summary>
		protected TimeCounter() {
		}

#region Implement

		/// <summary>
		/// 是否已经超过计数.
		/// </summary>
		/// <returns><c>true</c>, 已经超过计数, <c>false</c> 尚未超过计数.</returns>
		public override bool isCountOver() {
			switch (Mode) {
			case CounterMode.CountDown:
				{
					if (Value <= 0.0f) {
						return true;
					}
				}
				break;
			case CounterMode.CountUp:
				{
					if (Value >= MaxValue) {
						return true;
					}
				}
				break;
			default:
				break;
			}
			return false;
		}

		/// <summary>
		/// 根据变化值更新计数器.
		/// </summary>
		/// <param name="iDeltaVaule">变化值.</param>
		protected override void UpdateCounterByDeltaValue(float iDeltaVaule) {
			
			switch (Mode) {
			case CounterMode.CountDown:
				{
					Value -= iDeltaVaule;
				}
				break;
			case CounterMode.CountUp:
				{
					Value += iDeltaVaule;
				}
				break;
			default:
				break;
			}
			Info ("UpdateCounterByDeltaValue():Type::{0} Mode::{1} State::{2} Value::{3}/{4}",
				Type, Mode, State, Value, MaxValue);
		}

#endregion
	}

}