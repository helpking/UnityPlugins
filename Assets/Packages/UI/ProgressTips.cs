using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;
using Packages.Common.Base;
using Packages.Settings;

namespace Packages.UI {

	/// <summary>
	/// Progress tips.
	/// </summary>
	[AddComponentMenu("Packages/UI/ProgressTips")]
	public class ProgressTips : MonoBehaviour {

		/// <summary>
		/// The tip text.
		/// </summary>
		[FormerlySerializedAs("_tipText")] 
		public Text tipText;

		// Use this for initialization
		void Awake () {
			StartCoroutine (UpdateTips());
		}

		private IEnumerator UpdateTips() {

			if (tipText == null) {
				yield return null;
			}

			while (true) {
			
				var tip = SysSettings.GetInstance ().GetProgressTipByRandom ();
				if (string.IsNullOrEmpty (tip)) {
					break;
				}
				tipText.text = tip;
				yield return new WaitForSeconds(SysSettings.GetInstance ().data.tips.interval);

			}
			yield return null;
		}
	}
}
