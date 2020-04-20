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
		[MenuItem("GameObject/UI/Base")]	
		static void CreateUIBase () {	
			var selectedObj = Selection.activeGameObject;
			if (selectedObj == null) {
				return;
			}
			var progressBar = GameObject.Instantiate (Resources.Load<GameObject> ("Develop/Prefab/UI_Base"), selectedObj.transform, true);
			progressBar.transform.localScale = new Vector3 (1.0f, 1.0f, 1.0f);

			var rectTrans = progressBar.transform as RectTransform;
			if (rectTrans == null) return;
			rectTrans.pivot = new Vector2 (0.5f, 0.5f);
			// 上边距 5
			var offsetMax = rectTrans.offsetMax;
			offsetMax = new Vector2 (offsetMax.x, -5);
			// 右边距 5
			offsetMax = new Vector2 (-5, offsetMax.y);
			rectTrans.offsetMax = offsetMax;

			// 下边距 5
			var offsetMin = rectTrans.offsetMin;
			offsetMin = new Vector2 (offsetMin.x, 5);
			// 左边距 5
			offsetMin = new Vector2 (5, offsetMin.y);
			rectTrans.offsetMin = offsetMin;
		}

		/// <summary>
		/// 创建下载用Bar.
		/// </summary>
		[MenuItem("GameObject/UI/ProgressBar")]	
		static void CreateProgressBar () {	
			GameObject selectedObj = Selection.activeGameObject;
			if (selectedObj == null) {
				return;
			}
			GameObject progressBar = GameObject.Instantiate (Resources.Load<GameObject> ("Default/Prefab/UI_ProgressBar"));
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
