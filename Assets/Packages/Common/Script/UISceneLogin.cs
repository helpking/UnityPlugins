using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using AssetBundles;

public class UISceneLogin : MonoBehaviour {

	/// <summary>
	/// 点击登陆按钮.
	/// </summary>
	public void OnLoginBtnClick() {

		// 加载场景
		SceneManager.LoadScene ("SceneDownload");
//		AssetBundlesManager.GetInstance().LoadScene("SceneDownload");
	}
}
