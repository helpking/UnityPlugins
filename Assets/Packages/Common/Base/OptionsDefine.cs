namespace Packages.Common.Base {

	/// <summary>
	/// Android SDK 追加选项.
	/// </summary>
	public enum SDKOptions {
		/// <summary>
		/// 无.
		/// </summary>
		None = 0x00000000,
		/// <summary>
		/// 易接SDK.
		/// </summary>
		OneSDK = 0x00000001,
		/// <summary>
		/// SQLite.
		/// </summary>
		SQLite = 0x00000002
	}

	/// <summary>
	/// 选项数据(Base).
	/// </summary>
	[System.Serializable]
	public class OptionBaseData : JsonDataBase {
		/// <summary>
		/// 选项.
		/// </summary>
		public SDKOptions Option = SDKOptions.None;
	}

	/// <summary>
	/// 选项定义.
	/// </summary>
	[System.Serializable]
	public class OptionsBaseData : JsonDataBase<OptionsBaseData> {
		/// <summary>
		/// 指定选项.
		/// </summary>
		public int data;

		/// <summary>
		/// 清空.
		/// </summary>
		public override void Clear() {
			base.Clear ();

			data = (int)SDKOptions.None;
		}
			
		/// <summary>
		/// 设定选项.
		/// </summary>
		/// <param name="iOption">选项.</param>
		/// <param name="iIsOn">true:On; false:Off.</param>
		public void SetOptionOnOrOff(SDKOptions iOption, bool iIsOn) {
			if (iIsOn) {
				if (SDKOptions.None == iOption) {
					data = (int)SDKOptions.None;
				} else {
					data |= (int)iOption;
				}
			} else {
				data &= ~(int)iOption;
			}
		}

		/// <summary>
		/// 判断选项是否有效.
		/// </summary>
		/// <returns><c>true</c>, 有效, <c>false</c> 无效.</returns>
		/// <param name="iOption">选项.</param>
		public bool IsOptionValid(SDKOptions iOption)
		{
			if (SDKOptions.None == iOption) {
				return data == (int)iOption;
			}

			return (data & (int)iOption) == (int)iOption;
		}
	}

}