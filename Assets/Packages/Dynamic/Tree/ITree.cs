using UnityEngine;
using System.Collections.Generic;
using UINT32 = System.UInt32;

namespace Packages.Dynamic.Tree
{
	/// <summary>
	/// 检测器接口，用于检测和场景物件的触发
	/// </summary>
	public interface IDetector
	{
		/// <summary>
		/// 是否检测成功
		/// </summary>
		/// <param name="iBounds">包围盒</param>
		/// <returns>检测物体是否被包围</returns>
		bool IsDetected(Bounds iBounds);
 
		/// <summary>
		/// 触发器位置
		/// </summary>
		Vector3 Position { get; }
	}

	/// <summary>
	/// 地形预制体接口
	///  备注：地形预制体 -> 在地形上存在的预制体
	/// </summary>
	public interface ITerrainPrefab
	{
		/// <summary>
		/// 包围盒信息(世界坐标)
		/// </summary>
		Bounds Bounds { get; }

		/// <summary>
		/// 隐藏
		/// </summary>
		void OnHide();

		/// <summary>
		/// 显示
		/// </summary>
		void OnShow();
	}

	/// <summary>
	/// 地形块接口
	/// </summary>
	public interface ITerrainChunk<T> 
		where T : ITerrainPrefab
	{
		/// <summary>
		/// 莫顿码
		///  备注:用以标示地形区块位置
		/// </summary>
		UINT32 Morton { get; }
		
		/// <summary>
		/// 取得地形块节点列表
		/// </summary>
		LinkedListNode<T> GetPrefabs();

		/// <summary>
		/// 追加预制体信息
		/// </summary>
		/// <param name="iTPrefab">地形预制体</param>
		void AddPrefab(T iTPrefab);

		/// <summary>
		/// 取得上一层地形块
		/// </summary>
		ITerrainChunk<T> GetUplevelChunk();
		/// <summary>
		/// 取得左侧地形块
		/// </summary>
		ITerrainChunk<T> GetLeftChunk();
		/// <summary>
		/// 取得右侧地形块
		/// </summary>
		ITerrainChunk<T> GetRightChunk();
		/// <summary>
		/// 取得前面地形块
		/// </summary>
		ITerrainChunk<T> GetForwardChunk();
		/// <summary>
		/// 取得后面地形块
		/// </summary>
		ITerrainChunk<T> GetBackwardChunk();

		/// <summary>
		/// 加载
		/// </summary>
		void Load();
		
		/// <summary>
		/// 卸载
		/// </summary>
		void UnLoad();

	}

}


