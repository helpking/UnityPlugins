using System.Collections.Generic;
using Packages.BuildSystem.AndroidSDK.Manifest;
using Packages.Common.Base;
using UnityEngine.Serialization;

namespace Packages.BuildSystem.AndroidSDK.Options.OneSDK {

	/// <summary>
	/// 易接SDK 选项数据定义.
	/// </summary>
	[System.Serializable]
	public class OneSdkOptionsData : OptionBaseData {

		/// <summary>
		/// 易接自定义类名：用于闪屏后，跳转到游戏Main Activity名.
		/// </summary>
		[FormerlySerializedAs("ZyClassName")] 
		public string zyClassName;

		/// <summary>
		/// <meta-data/>节点数据列表.
		/// </summary>
		[FormerlySerializedAs("MetaDatas")] 
		public List<MetaDataInfo> metaDatas = new List<MetaDataInfo>();

		/// <summary>
		/// 初始化.
		/// </summary>
		public override void Init() {
			base.Init ();

			Option = SDKOptions.OneSDK;
			zyClassName = null;
			metaDatas.Clear ();
		}

		/// <summary>
		/// 清空.
		/// </summary>
		public override void Clear() {
			base.Clear ();

			Option = SDKOptions.None;
			zyClassName = null;
			metaDatas.Clear ();
		}
	}
}
