using Packages.Common.Base;
using Packages.Common.Extend;
using Packages.Settings;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Packages.UI
{
	/// <summary>
	/// App版本信息
	/// </summary>
	[AddComponentMenu("Packages/UI/AppVersion")]
	[RequireComponent(typeof(Text))]
	public class AppVersionInfo : MonoBehaviour {

		/// <summary>
		/// 版本信息
		/// </summary>
		[FormerlySerializedAs("AppVersionText")] 
		public Text appVersionText;

		/// <summary>
		/// 
		/// </summary>
		void Awake() {
			// 设定App版本信息
			SetAppVersionInfo();

			// 绑定资源信息变更函数
			SysSettings.GetInstance().data.General.AppVersionInfoChanged += ResetAppVersionInfo;
		}

		void OnDisable() {
			// 解除绑定资源信息变更函数
			SysSettings.GetInstance().data.General.AppVersionInfoChanged -= ResetAppVersionInfo;
		}

		public void ResetAppVersionInfo() {
			SetAppVersionInfo();
		}

		private void SetAppVersionInfo() {
			var sysSettings = SysSettings.GetInstance();
			if(null == sysSettings) {
				this.Error("Awake():The SysSettings is invalid!!!");
				return;
			}
			var appVersion =
				$"App Version : {sysSettings.BuildVersion}.{sysSettings.BuildShortVersion}.{sysSettings.BuildVersionCode} ( Res : {sysSettings.ResourceNo} ) ( No : {sysSettings.BuildNumber} )";
			
			if(null != appVersionText)
			{
				appVersionText.text = appVersion;
			}
		}
	}
}

