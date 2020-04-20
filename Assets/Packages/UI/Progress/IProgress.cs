namespace Packages.UI.Progress
{

	/// <summary>
	/// 委托定义：取得进度总数
	/// </summary>
	/// <returns>总数</returns>
	public delegate long GetTotalCount();

	/// <summary>
	/// 委托定义：取得进度完成
	/// </summary>
	/// <returns>完成数</returns>
	public delegate long GetCompletedCount();

	/// <summary>
	/// 更新计数
	/// </summary>
	/// <param name="iCount">计数(-1:未指定(未制定的场合，每次计数计1); 其他:计数值)</param>
	public delegate void UpdateCount(int iCount = -1);

	/// <summary>
	/// 委托定义：更新进度信息
	/// </summary>
	public delegate float UpdateProgress();

	/// <summary>
	/// 进度接口
	/// </summary>
	public interface IProgress
	{
		/// <summary>
		/// 委托定义：取得进度总数.
		/// </summary>
		GetTotalCount GetTotalCount { get; set; }

		/// <summary>
		/// 委托定义：取得进度完成
		/// </summary>
		GetCompletedCount GetCompletedCount { get; set; }

		/// <summary>
		/// 委托定义：更新进度信息
		/// </summary>
		UpdateProgress UpdateProgress { get; set; }

		/// <summary>
		/// 更新计数
		/// </summary>
		UpdateCount UpdateCount { get; set; }

	}

	/// <summary>
	/// 进度委托接口
	/// </summary>
	public interface IProgressDelegate {

		/// <summary>
		/// 总数
		/// </summary>
		long TotalCount { get; }

		/// <summary>
		/// 完成数
		/// </summary>
		long CompletedCount { get; }

		/// <summary>
		/// 取得进度总数
		/// </summary>
		/// <returns>总数</returns>
		long GetTotalCount();

		/// <summary>
		/// 委托定义：取得进度完成
		/// </summary>
		/// <returns>完成数</returns>
		long GetCompletedCount();

		/// <summary>
		/// 更新计数
		/// </summary>
		/// <param name="iCount">计数(-1:未指定(未制定的场合，每次计数计1); 其他:计数值)</param>
		void UpdateCount(int iCount = -1);

		/// <summary>
		/// 委托定义：更新进度信息
		/// </summary>
		float UpdateProgress();
	}

}
