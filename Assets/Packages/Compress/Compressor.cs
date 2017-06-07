using UnityEngine;
using System.Collections;
using Download;

namespace Compress {

	/// <summary>
	/// 压缩器.
	/// </summary>
	public class Compressor {

		/// <summary>
		/// 实例.
		/// </summary>
		private static Compressor _instance = null;

		/// <summary>
		/// 取得实例.
		/// </summary>
		/// <returns>实例.</returns>
		public static Compressor GetInstance() {
			if (_instance == null) {
				_instance = new Compressor ();
			}
			return _instance;
		} 


	}
}
