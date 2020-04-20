using Packages.BuildSystem.AndroidSDK.Options.OneSDK;
using Packages.Common.Base;

namespace Packages.BuildSystem.AndroidSDK.Options {
	
	/// <summary>
	/// 选项定义.
	/// </summary>
	[System.Serializable]
	public class BuildSettingOptionsData : OptionsBaseData {

		/// <summary>
		/// 易接SDK选项.
		/// </summary>
		public OneSdkOptionsData OneSDK = new OneSdkOptionsData();

		/// <summary>
		/// 清空.
		/// </summary>
		public override void Clear() {
			base.Clear ();

			OneSDK.Clear ();
		}
	}

}