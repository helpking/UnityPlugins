using UnityEngine;
using UnityEngine.UI;
using Packages.Common.Base;
using Packages.Common.Extend;

namespace Packages.UI {

	[RequireComponent(typeof(Image))]
	[AddComponentMenu("Packages/UI/UIImageAutoNativeSize")]
	public class UiImageAutoNativeSize : MonoBehaviour {

		void Start () {
			var image = GetComponent<Image> ();
			if (null != image) {
				image.SetNativeSize ();
			} else {
				this.Error("Start()");
			}
		}
	}
}
