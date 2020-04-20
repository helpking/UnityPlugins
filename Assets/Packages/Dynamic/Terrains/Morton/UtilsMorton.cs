using UnityEngine;
using UINT32 = System.UInt32;

namespace Packages.Dynamic.Terrains.Morton
{

	/// <summary>
	/// 莫顿码相关工具类
	/// </summary>
	public class UtilsMorton {

#region Morton

	/// <summary>
	/// 莫顿码左移1位
	/// </summary>
	/// <param name="iValue">值</param>
	/// <returns>左移1位后的值</returns>
	private static UINT32 Part1By1(UINT32 iValue)
	{
		iValue &= 0x0000ffff;                           // x = ---- ---- ---- ---- fedc ba98 7654 3210
		iValue = (iValue ^ (iValue << 8)) & 0x00ff00ff; // x = ---- ---- fedc ba98 ---- ---- 7654 3210 
		iValue = (iValue ^ (iValue << 4)) & 0x0f0f0f0f; // x = ---- fedc ---- ba98 ---- 7654 ---- 3210
		iValue = (iValue ^ (iValue << 2)) & 0x33333333; // x = --fe --dc --ba --98 --76 --54 --32 --10
		iValue = (iValue ^ (iValue << 1)) & 0x55555555; // x = -f-e -d-c -b-a -9-8 -7-6 -5-4 -3-2 -1-0
		return iValue;
	}
	
	/// <summary>
	/// 莫顿码左移2位
	/// </summary>
	/// <param name="iValue">值</param>
	/// <returns>左移2位后的值</returns>
	private static UINT32 Part1By2(UINT32 iValue)
	{
		iValue &= 0x000003ff;                            // x = ---- ---- ---- ---- ---- --98 7654 3210
		iValue = (iValue ^ (iValue << 16)) & 0xff0000ff; // x = ---- --98 ---- ---- ---- ---- 7654 3210
		iValue = (iValue ^ (iValue << 8)) & 0x0300f00f;  // x = ---- --98 ---- ---- 7654 ---- ---- 3210
		iValue = (iValue ^ (iValue << 4)) & 0x030c30c3;  // x = ---- --98 ---- 76-- --54 ---- 32-- --10
		iValue = (iValue ^ (iValue << 2)) & 0x09249249;  // x = ---- 9--8 --7- -6-- 5--4 --3- -2-- 1--0
		return iValue;
	}

	/// <summary>
	/// 莫顿码右移1位(还原/反算)
	/// </summary>
	/// <param name="iValue">值</param>
	/// <returns>右移1位(还原/反算)后的值</returns>
	private static UINT32 Inverse1By1(UINT32 iValue)
	{
		iValue &= 0x55555555;                           // x = -f-e -d-c -b-a -9-8 -7-6 -5-4 -3-2 -1-0
		iValue = (iValue ^ (iValue >> 1)) & 0x33333333; // x = --fe --dc --ba --98 --76 --54 --32 --10
		iValue = (iValue ^ (iValue >> 2)) & 0x0f0f0f0f; // x = ---- fedc ---- ba98 ---- 7654 ---- 3210
		iValue = (iValue ^ (iValue >> 4)) & 0x00ff00ff; // x = ---- ---- fedc ba98 ---- ---- 7654 3210
		iValue = (iValue ^ (iValue >> 8)) & 0x0000ffff; // x = ---- ---- ---- ---- fedc ba98 7654 3210
		return iValue;
	}
	
	/// <summary>
	/// 莫顿码右移2位(还原/反算)
	/// </summary>
	/// <param name="iValue">值</param>
	/// <returns>右移2位(还原/反算)后的值</returns>
	private static UINT32 Inverse1By2(UINT32 iValue)
	{
		iValue &= 0x09249249;                            // x = ---- 9--8 --7- -6-- 5--4 --3- -2-- 1--0
		iValue = (iValue ^ (iValue >> 2)) & 0x030c30c3;  // x = ---- --98 ---- 76-- --54 ---- 32-- --10
		iValue = (iValue ^ (iValue >> 4)) & 0x0300f00f;  // x = ---- --98 ---- ---- 7654 ---- ---- 3210
		iValue = (iValue ^ (iValue >> 8)) & 0xff0000ff;  // x = ---- --98 ---- ---- ---- ---- 7654 3210
		iValue = (iValue ^ (iValue >> 16)) & 0x000003ff; // x = ---- ---- ---- ---- ---- --98 7654 3210
		return iValue;
	}

#endregion
		
#region Morton - 3D

		/// <summary>
		/// 计算三维坐标的莫顿码
		/// </summary>
		/// <param name="iPosX">坐标X</param>
		/// <param name="iPosY">坐标Y</param>
		/// <param name="iPosZ">坐标Z</param>
		/// <returns>三维坐标的莫顿码</returns>
		public static UINT32 Morton3d(UINT32 iPosX, UINT32 iPosY, UINT32 iPosZ)
		{
			iPosX = Part1By2(iPosX);
			iPosY = Part1By2(iPosY);
			iPosZ = Part1By2(iPosZ);
			return iPosX | (iPosY << 1) | (iPosZ << 2);
		}
		
		/// <summary>
		/// 计算三维坐标的莫顿码
		/// </summary>
		/// <param name="iPosX">坐标X</param>
		/// <param name="iPosY">坐标Y</param>
		/// <param name="iPosZ">坐标Z</param>
		/// <returns>三维坐标的莫顿码</returns>
		public static UINT32 Morton3d(float iPosX, float iPosY, float iPosZ)
		{
			return Morton3d((UINT32)iPosX, (UINT32)iPosY, (UINT32)iPosZ);
		}

		/// <summary>
		/// 计算三维坐标的莫顿码
		/// </summary>
		/// <param name="iPos">坐标</param>
		/// <returns>三维坐标的莫顿码</returns>
		public static UINT32 Morton3d(Vector3 iPos)
		{
			return Morton3d(iPos.x, iPos.y, iPos.z);
		}

		/// <summary>
		/// 反算莫顿码
		/// </summary>
		/// <param name="ioPosX">坐标X</param>
		/// <param name="ioPosY">坐标Y</param>
		/// <param name="ioPosZ">坐标Z</param>
		/// <param name="iMorton">莫顿码</param>
		public static void InverseMorton3d(ref UINT32 ioPosX, ref UINT32 ioPosY, ref UINT32 ioPosZ, UINT32 iMorton)
		{
			ioPosX = Inverse1By2(iMorton);
			ioPosY = Inverse1By2(iMorton >> 1);
			ioPosZ = Inverse1By2(iMorton >> 2);
		}

		/// <summary>
		/// 反算莫顿码
		/// </summary>
		/// <param name="ioPosX">坐标X</param>
		/// <param name="ioPosY">坐标Y</param>
		/// <param name="ioPosZ">坐标Z</param>
		/// <param name="iMorton">莫顿码</param>
		public static void InverseMorton3d(ref float ioPosX, ref float ioPosY, ref float ioPosZ, UINT32 iMorton)
		{
			UINT32 posX = 0; 
			UINT32 posY = 0; 
			UINT32 posZ = 0;
			
			// 反算
			InverseMorton3d(ref posX, ref posY, ref posZ, iMorton);
			
			ioPosX = posX;
			ioPosY = posY;
			ioPosZ = posZ;

		}
		
		/// <summary>
		/// 反算莫顿码
		/// </summary>
		/// <param name="ioPos">坐标X</param>
		/// <param name="iMorton">莫顿码</param>
		public static void InverseMorton3d(ref Vector3 ioPos, UINT32 iMorton)
		{
			var posX = 0.0f; 
			var posY = 0.0f; 
			var posZ = 0.0f;
			
			// 反算
			InverseMorton3d(ref posX, ref posY, ref posZ, iMorton);
			
			ioPos.x = posX;
			ioPos.y = posY;
			ioPos.z = posZ;
		}

#endregion

#region Morton - 2D

		/// <summary>
		/// 计算二维坐标的莫顿码
		/// </summary>
		/// <param name="iPosX">坐标X</param>
		/// <param name="iPosY">坐标Y</param>
		/// <returns>二维坐标的莫顿码</returns>
		public static UINT32 Morton2d(UINT32 iPosX, UINT32 iPosY)
		{
			iPosX = Part1By1(iPosX);
			iPosY = Part1By1(iPosY);
			return iPosX | (iPosY << 1);
		}

		/// <summary>
		/// 计算二维坐标的莫顿码
		/// </summary>
		/// <param name="iPosX">坐标X</param>
		/// <param name="iPosY">坐标Y</param>
		/// <returns>二维坐标的莫顿码</returns>
		public static UINT32 Morton2d(float iPosX, float iPosY)
		{
			return Morton2d((UINT32) iPosX, (UINT32) iPosY);
		}

		/// <summary>
		/// 计算二维坐标的莫顿码
		/// </summary>
		/// <param name="iPos">坐标</param>
		/// <returns>二维坐标的莫顿码</returns>
		public static UINT32 Morton2d(Vector2 iPos)
		{
			return Morton2d(iPos.x, iPos.y);
		}

		/// <summary>
		/// 反算莫顿码
		/// </summary>
		/// <param name="ioPosX">坐标X</param>
		/// <param name="ioPosY">坐标Y</param>
		/// <param name="iMorton">莫顿码</param>
		public static void InverseMorton2d(ref UINT32 ioPosX, ref UINT32 ioPosY, UINT32 iMorton)
		{
			ioPosX = Inverse1By1(iMorton);
			ioPosY = Inverse1By1(iMorton >> 1);
		}

		/// <summary>
		/// 反算莫顿码
		/// </summary>
		/// <param name="ioPosX">坐标X</param>
		/// <param name="ioPosY">坐标Y</param>
		/// <param name="iMorton">莫顿码</param>
		public static void InverseMorton2d(ref float ioPosX, ref float ioPosY, UINT32 iMorton)
		{
			ioPosX = Inverse1By1(iMorton);
			ioPosY = Inverse1By1(iMorton >> 1);
		}

		/// <summary>
		/// 反算莫顿码
		/// </summary>
		/// <param name="ioPos">坐标</param>
		/// <param name="iMorton">莫顿码</param>
		public static void InverseMorton2d(ref Vector2 ioPos, UINT32 iMorton)
		{
			var posX = Inverse1By1(iMorton);
			var posY = Inverse1By1(iMorton >> 1);

			ioPos.x = posX;
			ioPos.y = posY;
		}
		
#endregion
		
	}

}

