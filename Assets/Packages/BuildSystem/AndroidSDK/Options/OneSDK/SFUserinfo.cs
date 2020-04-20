using Packages.Common.Base;

namespace Packages.BuildSystem.AndroidSDK.Options.OneSDK
{
	/// <summary>
	/// 易接专用用户信息.
	/// </summary>
	public sealed class SFOnlineUser : ClassExtension {

		private long _id;
		private string _channelId;
		private string _channelUserId;

		private string _userName;
		private string _token;
		private string _productCode;

		public SFOnlineUser(long iId, string iChannelId, string iChannelUserId,
			string iUserName, string iToken, string iProductCode) {
			_id = iId;
			_channelId = iChannelId;
			_channelUserId = iChannelUserId;

			_userName = iUserName;
			_token = iToken;
			_productCode = iProductCode;
		}

		public long getId() {
			return _id;
		}

		public void setId(long iId) {
			_id = iId;
		}

		public string getChannelId() {
			return _channelId;
		}

		public void setChannelId(string iChannelId) {
			_channelId = iChannelId;
		}

		public string getChannelUserId() {
			return _channelUserId;
		}

		public void setChannelUserId(string iChannelUserId) {
			_channelUserId = iChannelUserId;
		}

		public string getUserName() {
			return _userName;
		}

		public void setUserName(string iUserName) {
			_userName = iUserName;
		}

		public string getToken() {
			return _token;
		}

		public void setToken(string iToken) {
			_token = iToken;
		}

		public string getProductCode() {
			return _productCode;
		}

		public void setProductCode(string iProductCode) {
			_productCode = iProductCode;
		}

	}
}