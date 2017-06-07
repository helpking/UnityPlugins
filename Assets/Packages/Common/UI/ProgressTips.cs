using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Upload;

namespace Download {

	/// <summary>
	/// Progress tips.
	/// </summary>
	public class ProgressTips : MonoBehaviour {

		/// <summary>
		/// The tip text.
		/// </summary>
		public Text _tipText = null;

		// Use this for initialization
		void Awake () {
			StartCoroutine (UpdateTips());
		}

		private IEnumerator UpdateTips() {

			if (this._tipText == null) {
				yield return null;
			}

			while (true) {
			
				string tip = ServersConf.GetInstance ().GetProgressTipByRandom ();
				if (string.IsNullOrEmpty (tip) == true) {
					break;
				}
				this._tipText.text = tip;
				yield return new WaitForSeconds(ServersConf.GetInstance ().ProgressTips.Interval);

			}
			yield return null;
		}
	}
}
