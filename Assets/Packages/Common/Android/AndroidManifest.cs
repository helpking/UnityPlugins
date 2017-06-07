using UnityEngine;
using System.Xml;
using System.Collections;
using BuildSystem;

namespace Common {

#if UNITY_ANDROID

	/// <summary>
	/// 安卓配置文件
	/// </summary>
	public class AndroidManifest : XmlDocument {

		/// <summary>
		/// 实例.
		/// </summary>
		private static AndroidManifest _instance = null;

		/// <summary>
		/// 取得实例.
		/// </summary>
		/// <returns>实例.</returns>
		public static AndroidManifest GetInstance() {
			if (_instance == null) {
				try {
					_instance = new AndroidManifest ();
				} catch (UnityException exp) {
					BuildLogger.LogException ("[AndroidManifest Create Failed] Exeption : {0}",
						exp.Message);
					_instance = null;
				}
			}

			return _instance;
		}


	}

#endif

}
