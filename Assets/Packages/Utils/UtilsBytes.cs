using UnityEngine;

namespace Packages.Utils
{
	/// <summary>
	/// 位检查&校验类
	/// </summary>
	public static class UtilsBytes {

		/// <summary>
		/// 检测&检验指定位是否为1
		///   备注：可以同时检测多位
		///   如：
		///   iDstVale ： 0x000000111
		///   iCheckByte : 0x00000100 || 0x00000001 => 0x00000101
		///   如上返回值 => true
		/// </summary>
		/// <param name="iDstVale">目标值</param>
		/// <param name="iCheckByte">检测位</param>
		/// <returns></returns>
		public static bool CheckByte(int iDstVale, int iCheckByte)
		{
			return (iDstVale & iCheckByte) == iCheckByte;
		}
	}
}

