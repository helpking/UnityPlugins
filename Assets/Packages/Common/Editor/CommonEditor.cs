using UnityEngine;
using UnityEditor;
using System.Collections;

namespace Common {

	/// <summary>
	/// 公共编辑器.
	/// </summary>
	public class CommonEditor : Editor {

		#region UI

		/// <summary>
		/// 创建下载用Bar.
		/// </summary>
		[MenuItem("GameObject/UI/ProgressBar")]	
		static void CreateProgressBar () {	
			GameObject selectedObj = Selection.activeGameObject;
			if (selectedObj == null) {
				return;
			}
			GameObject progressBar = GameObject.Instantiate (Resources.Load<GameObject> ("Prefab/ProgressBar"));
			progressBar.transform.parent = selectedObj.transform;
			progressBar.transform.localScale = new Vector3 (1.0f, 1.0f, 1.0f);

			RectTransform rectTrans = progressBar.transform as RectTransform;
			if (rectTrans != null) {
				rectTrans.pivot = new Vector2 (0.5f, 0.5f);
				// 上边距 5
				rectTrans.offsetMax = new Vector2 (rectTrans.offsetMax.x, -5);
				// 右边距 5
				rectTrans.offsetMax = new Vector2 (-5, rectTrans.offsetMax.y);

				// 下边距 5
				rectTrans.offsetMin = new Vector2 (rectTrans.offsetMin.x, 5);
				// 左边距 5
				rectTrans.offsetMin = new Vector2 (5, rectTrans.offsetMin.y);
			}

		}

		#endregion
	}
}
