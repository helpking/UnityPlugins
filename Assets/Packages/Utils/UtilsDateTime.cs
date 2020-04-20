using System;
using System.Globalization;

namespace Packages.Utils {

	/// <summary>
	/// 时间.
	/// </summary>
	public class UtilsDateTime {

		/// <summary>
		/// 判断是否在开启日期之前.
		/// </summary>
		/// <returns><c>true</c>, 时, <c>false</c> 不是.</returns>
		/// <param name="iStartDate">开始日期（YYYY/MM/DD HH:MM）.</param>
		public static bool IsBeforeDate (string iStartDate) {

			var now = DateTime.Now;
			if (string.IsNullOrEmpty(iStartDate) || "-".Equals(iStartDate)) return false;
			var dtFormat = new DateTimeFormatInfo
			{
				ShortDatePattern = "yyyy/MM/dd HH:MM"
			};

			var start = Convert.ToDateTime (iStartDate, dtFormat);
			return now.CompareTo (start) < 0;
		}

		/// <summary>
		/// 检测活动有效日期.
		/// </summary>
		/// <returns><c>true</c>, 有效期内, <c>false</c> 有效期外.</returns>
		/// <param name="iStartDate">开始日期（YYYY/MM/DD HH:MM）.</param>
		/// <param name="iEndDate">结束日期（YYYY/MM/DD HH:MM）.</param>
		public static bool CheckDate (string iStartDate, string iEndDate) {
			var now = DateTime.Now;
			if (false == string.IsNullOrEmpty (iStartDate) &&
				false == "-".Equals (iStartDate)) {

				var dtFormat = new DateTimeFormatInfo {ShortDatePattern = "yyyy/MM/dd HH:MM"};

				var start = Convert.ToDateTime (iStartDate, dtFormat);
				if (now.CompareTo (start) < 0) {
					return false;
				}
			}

			if (string.IsNullOrEmpty(iEndDate) || "-".Equals(iEndDate)) return true;
			{
				var dtFormat = new DateTimeFormatInfo
				{
					ShortDatePattern = "yyyy/MM/dd HH:MM"
				};

				var end = Convert.ToDateTime (iEndDate, dtFormat);
				if (end.CompareTo (now) <= 0) {
					return false;
				}
			}

			return true;
		}

		/// <summary>
		/// 取得当前日期和时间的刻度数
		///   备注:此属性的值为自 0001 年 1 月 1 日午夜 12:00 以来所经过时间以 100 毫微秒为间隔表示时的数字
		/// </summary>
		/// <returns>当前日期和时间的刻度数</returns>
		public static long GetCurDateTimeTicks()
		{
			return DateTime.Now.Ticks;
		}

		/// <summary>
		/// 取得毫秒值
		/// </summary>
		/// <param name="iValue">值(单位：100 毫微秒)</param>
		/// <returns>毫秒值</returns>
		public static float GetMilliseconF(long iValue)
		{
			return iValue / 10000.0f;
		}
		
		/// <summary>
		/// 取得毫秒值
		/// </summary>
		/// <param name="iValue">值(单位：100 毫微秒)</param>
		/// <returns>毫秒值</returns>
		public static long GetMilliseconL(long iValue)
		{
			return iValue / 10000;
		}
		
		/// <summary>
		/// 取得两个时间刻度数之间的间隔
		///   备注:此属性的值单位为自 0001 年 1 月 1 日午夜 12:00 以来所经过时间以 100 毫微秒为间隔表示时的数字
		/// </summary>
		/// <param name="iStart">开始时间(单位：100毫微秒)</param>
		/// <param name="iCurrent">现在时间(单位：100毫微秒)</param>
		/// <returns>两个时间刻度数之间的间隔(单位:ms)</returns>
		public static long GetDeltaDateTimeTicks(long iStart, long iCurrent)
		{
			var deltaTime = GetMilliseconL(iCurrent - iStart);
			deltaTime = (1 >= deltaTime) ? 0 : deltaTime;
			return deltaTime;
		}

		/// <summary>
		/// 强行等待一段时间
		/// </summary>
		/// <param name="iWaitTime">等待时间(单位：毫秒)</param>
		public static void WaitForAWhile(long iWaitTime)
		{
			var startTime = DateTime.Now.Ticks;
			// 将等待时间转换到100 毫微秒
			var waitTime = iWaitTime * 10000;
			while (waitTime >= 0L)
			{
				waitTime -= DateTime.Now.Ticks - startTime;
				startTime = DateTime.Now.Ticks;
			}
		}

		/// <summary>
		/// 取得表示用变化时间
		///   会根据时间的大小自动变化时间单位
		/// </summary>
		/// <param name="iStart">开始时间(单位：毫微秒)</param>
		/// <param name="iCurrent">现在时间(单位：毫微秒)</param>
		/// <param name="iIsShort">剪短表示(true:仅表示当前最大单位时间戳)</param>
		/// <returns>两个时间刻度数之间的间隔(单位:ms/s/m/h)</returns>
		public static string GetDisplayDeltaTime(long iStart, long iCurrent, bool iIsShort = true)
		{
			return GetDisplayDeltaTime(iCurrent - iStart, iIsShort);
		}
		
		/// <summary>
		/// 取得表示用变化时间
		///   会根据时间的大小自动变化时间单位
		/// </summary>
		/// <param name="iTime">时间(单位：毫微秒)</param>
		/// <param name="iIsShort">剪短表示(true:仅表示当前最大单位时间戳)</param>
		/// <returns>两个时间刻度数之间的间隔(单位:ms/s/m/h)</returns>
		public static string GetDisplayDeltaTime(long iTime, bool iIsShort = true)
		{
			var totalValue = GetMilliseconL(iTime);
			if (0L <= totalValue && totalValue <= 999L)
			{
				return $"{totalValue}ms";
			}

			// 1s ~ 59.9s
			if (1000L <= totalValue && totalValue <= 59999L)
			{
				// 取得毫秒数
				var millTmp = totalValue % 1000;
				// 取得秒数
				var sec = (totalValue - millTmp) / 1000;

				return iIsShort ? $"{sec}s" : $"{sec}s{millTmp}ms";
			}

			// 1min ~ 59.9min 
			if (60000L <= totalValue && totalValue <= 3599999L)
			{
				// 取得毫秒数
				var millTmp = totalValue % 1000;
				// 取得秒数
				var sec = (totalValue - millTmp) / 1000;
				sec = sec % 60;
				// 取得分钟数
				var min = (totalValue - millTmp - sec * 60) / (60 * 1000);
				
				return iIsShort ? $"{min}m{sec}s" : $"{min}m{sec}s{millTmp}ms";
			}
			// >= 1h
			else
			{
				// 取得毫秒数
				var millTmp = totalValue % 1000;
				// 取得秒数
				var sec = (totalValue - millTmp) / 1000;
				sec = sec % 60;
				// 取得分钟数
				var min = (totalValue - millTmp - sec * 60) / (60 * 1000);
				min = min % 60;
				// 取得小时数
				var hour = (totalValue - millTmp - sec * 60 - min * 60 * 60) / (60 * 60 * 1000);
				return iIsShort ? $"{hour}h{min}m{sec}s" : $"{hour}h{min}m{sec}s{millTmp}ms";
			}
		}
	}
}
