using System;
using System.Collections.Generic;

namespace Packages.Common.Comparer
{
	/// <summary>
	/// 字符串比较器
	/// </summary>
	public class StrComparer : IComparer<string>
	{
		public int Compare(string iX, string iY)
		{
			return iX != null ? string.Compare(iX, iY, StringComparison.Ordinal) : 0;
		}  
	}
}