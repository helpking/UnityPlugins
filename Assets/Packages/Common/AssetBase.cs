using UnityEngine;
using System.Collections;

namespace Common {

	/// <summary>
	/// Asset基类.
	/// </summary>
	public abstract class AssetBase : ScriptableObject {

		/// <summary>
		/// 初始化.
		/// </summary>
		public abstract void Init ();

		/// <summary>
		/// 应用数据.
		/// </summary>
		/// <param name="iData">数据.</param>
		protected abstract void ApplyData(AssetBase iData);

		/// <summary>
		/// 清空.
		/// </summary>
		public abstract void Clear ();

		/// <summary>
		/// 从JSON文件，导入打包配置信息.
		/// </summary>
		public abstract void ImportFromJsonFile();

		/// <summary>
		/// 导出成JSON文件.
		/// </summary>
		/// <returns>导出路径.</returns>
		public abstract string ExportToJsonFile();
	}
}
