using System;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;
using UnityEngine.Serialization;
using Packages.Common.Base;
using Packages.NetWork.Http;
using Packages.Settings;
using Packages.Utils;

#if UNITY_ANDROID

namespace Packages.BuildSystem.AndroidSDK.Options.OneSDK {

	/// <summary>
	/// 易接登陆校验信息.
	/// </summary>
	[Serializable]
	public class OneSdkLoginCheckInfo : JsonDataBase<OneSdkLoginCheckInfo> {
		/// <summary>
		/// Product Code.
		/// </summary>
		public string app;
		/// <summary>
		/// 渠道ID.
		/// </summary>
		public string sdk;
		/// <summary>
		/// 渠道用户ID.
		/// </summary>
		public string uin;
		/// <summary>
		/// 用户登录相关渠道后的Session ID.
		/// </summary>
		public string sess;
		/// <summary>
		///  渠道ID.
		/// </summary>
		public string channel;
		/// <summary>
		/// 平台类型.
		/// </summary>
		public string platform;
		/// <summary>
		/// 选项.
		/// </summary>
		public string option;

		/// <summary>
		/// 清空.
		/// </summary>
		public override void Clear() {
			base.Clear ();
			app = null;
			sdk = null;
			uin = null;
			sess = null;
			channel = null;
			platform = null;
			option = null;
		}

		/// <summary>
		/// 应用.
		/// </summary>
		/// <param name="iUserInfo">用户数据.</param>
		public void Apply(OneSdkUserInfo iUserInfo) {
			Reset ();

			app = iUserInfo.productCode;
			sdk = iUserInfo.channelId;
			uin = iUserInfo.channelUserId;
			sess = iUserInfo.token;
			channel = iUserInfo.channelId;
			platform = SysSettings.GetInstance ().PlatformType.ToString ();
			option = "OneSDK";
		}
	}

	/// <summary>
	/// 易接用户信息.
	/// </summary>
	[Serializable]
	public class OneSdkUserInfo : SdkAccountBaseInfo {
		/// <summary>
		/// 易接内部 userid，该值可能为 0，请不要以此参数作为判定
		/// </summary>
		[FormerlySerializedAs("UserID")] 
		public long userId;
		/// <summary>
		/// 易接平台标示的渠道SDK ID.
		/// </summary>
		[FormerlySerializedAs("ChannelId")] 
		public string channelId;
		/// <summary>
		/// 渠道SDK标示的用户ID.
		/// </summary>
		[FormerlySerializedAs("ChannelUserId")] 
		public string channelUserId;
		/// <summary>
		/// 渠道SDK的用户名称.
		/// </summary>
		[FormerlySerializedAs("UserName")] 
		public string userName;
		/// <summary>
		/// 渠道SDK登录完成后的SessionID.
		/// 特别提醒醒:
		/// 部分渠道此参数会包含特殊值如‘+’，空格之类的，
		/// 如直接使用URL参数传输到游戏服务器请求校验，
		/// 请使用 URLEncoder 编码。
		/// </summary>
		[FormerlySerializedAs("Token")] 
		public string token;
		/// <summary>
		/// 易接平台创建的游戏ID（appId）.
		/// </summary>
		[FormerlySerializedAs("ProductCode")] 
		public string productCode;

		/// <summary>
		/// 清空.
		/// </summary>
		public override void Clear() {
			base.Clear ();
			userId = -1;
			channelId = null;
			channelUserId = null;
			userName = null;
			token = null;
			productCode = null;
		}

		/// <summary>
		/// 初始化.
		/// </summary>
		public override void Init() {
			base.Init ();
			userId = -1;
			channelId = null;
			channelUserId = null;
			userName = null;
			token = null;
			productCode = null;
		}
	}

	/// <summary>
	/// 易接支付信息.
	/// </summary>
	[Serializable]
	public class OneSdkPaymentInfo : SdkPaymentBaseInfo {

		/// <summary>
		/// 订单号.
		/// </summary>
		[FormerlySerializedAs("OrderNo")] 
		public string orderNo;

		/// <summary>
		/// 清空.
		/// </summary>
		public override void Clear() {
			base.Clear ();
			orderNo = null;
		}
	}

	/// <summary>
	/// 易接角色信息.
	/// </summary>
	[Serializable]
	public class OneSdkRoleInfo : SdkRoleBaseInfo {

		/// <summary>
		/// 当前用户VIP等级，必须为数字，若无，传入1.
		/// </summary>
		[FormerlySerializedAs("Vip")] 
		public string vip;

		/// <summary>
		/// 当前角色所属帮派，不能为空，不能为null，若无， 传入“无帮派”.
		/// </summary>
		[FormerlySerializedAs("PartyName")] 
		public string partyName;

		/// <summary>
		/// 角色等级变化时间(单位：秒).
		/// </summary>
		[FormerlySerializedAs("RoleLevelMTime")] 
		public string roleLevelMTime;

		/// <summary>
		/// 清空.
		/// </summary>
		public override void Clear() {
			base.Clear ();
			vip = "1";
			partyName = "无帮派";
			roleLevelMTime = null;
		}
	}
	
	/// <summary>
	/// 易接SDK库.
	/// </summary>
	public sealed class OneSdkLibs : SingletonBase<OneSdkLibs>, IDisposable {
		/// <summary>
		/// 登出.
		/// </summary>
		private const string SLogout = "0";

		/// <summary>
		/// 登录成功.
		/// </summary>
		private const string SLoginSuccess = "1";

		/// <summary>
		/// 登录失败.
		/// </summary>
		private const string SLoginFailed = "2";

		/// <summary>
		/// 支付成功.
		/// </summary>
		private const string SPaySuccess = "0";

		/// <summary>
		/// 支付失败.
		/// </summary>
		private const string SPayFailed = "1";

		/// <summary>
		/// 支付订单No.
		/// </summary>
		private const string SPayOrderNo = "2";

		/// <summary>
		/// 用户信息(易接).
		/// </summary>
		private SFOnlineUser _sfUserInfo;

		/// <summary>
		/// 用户信息.
		/// </summary>
		private OneSdkUserInfo _userInfo;

		/// <summary>
		/// 支付信息.
		/// </summary>
		private OneSdkPaymentInfo _payment;

		/// <summary>
		/// 登录标志位.
		/// </summary>
		private bool _isLogin;

		/// <summary>
		/// 状态更新函数.
		/// </summary>
		private Action<SdkStatus> _updateStatusCallback;

		/// <summary>
		/// 目标游戏对象(挂接相关脚本的游戏对象).
		/// </summary>
		private GameObject _targetGameObject;
	
		/// <summary>
		/// 登录校验Base Url.
		/// </summary>
		private string _loginCheckBaseUrl;

		/// <summary>
		/// 登录校验回调函数.
		/// </summary>
		private Action<string, Action<string>, Action<string>> _loginCheckCallback;

		/// <summary>
		/// 登录校验成功回调函数.
		/// </summary>
		private Action<string> _loginCheckSucceeded;

		/// <summary>
		/// 登录校验失败回调函数.
		/// </summary>
		private Action<string> _loginCheckFailed;

		/// <summary>
		/// 自动重登最大次数.
		/// </summary>
		private int _autoReloginMaxCount = 3;

		/// <summary>
		/// 自动重登次数.
		/// </summary>
		private int _autoReloginCount;

		/// <summary>
		/// 自动重登回调函数.
		/// </summary>
		private Action<float> _autoReloginCallback;

#region Login - Logout

		/// <summary>
		/// login接口用于SDK登陆.
		/// </summary>
		/// <param name="iContext">上下文Activity.</param>
		/// <param name="iCustomParams">自定义参数.</param>
		[DllImport("gangaOnlineUnityHelper")]
		private static extern void login(IntPtr iContext, string iCustomParams);

		/// <summary>
		/// setLoginListener方法用于设置登陆监听.
		/// </summary>
		/// <param name="iContext">上下文Activity.</param>
		/// <param name="iTargetGameObject">监听游戏对象.</param>
		/// <param name="iListener">监听器(方法名).</param>
		[DllImport("gangaOnlineUnityHelper")]
		private static extern void setLoginListener (IntPtr iContext, string iTargetGameObject, string iListener);

		/// <summary>
		/// logout接口用于SDK登出.
		/// </summary>
		/// <param name="iContext">上下文Activity.</param>
		/// <param name="iCustomParams">自定义参数.</param>
		[DllImport("gangaOnlineUnityHelper")]
		private static extern void logout(IntPtr iContext, string iCustomParams);

#endregion

		/// <summary>
		/// exit接口用于系统全局退出.
		/// </summary>
		/// <param name="iContext">上下文Activity.</param>
		/// <param name="iTargetGameObject">监听游戏对象.</param>
		/// <param name="iListener">监听器(方法名).</param>
		[DllImport("gangaOnlineUnityHelper")]
		private static extern void exit(IntPtr iContext, string iTargetGameObject, string iListener);

#region Payment

		/// <summary>
		/// pay接口用于用户触发定额计费.
		/// </summary>
		/// <param name="iContext">上下文Activity.</param>
		/// <param name="iTargetGameObject">监听游戏对象.</param>
		/// <param name="iUnitPrice">游戏道具单位价格(单位:分).</param>
		/// <param name="iUnitName">虚拟货币名称.</param>
		/// <param name="iCount">商品或道具数量.</param>
		/// <param name="iCallBackInfo">由游戏开发者定义传入的字符串，会与支付结果一同发送给游戏服务器.游戏服务器可通过该字段判断交易的详细内容（金额角色等）</param>
		/// <param name="iCallBackUrl">将支付结果通知给游戏服务器时的通知地址url，交易结束后，系统会向该url发送http请求，通知交易的结果金额callbackInfo等信息.</param>
		/// <param name="iPayResultListener">支付监听接口，隶属于gameObject对象的运行时脚本的方法名称，该方法会在收到通知后触发.</param>
		[DllImport("gangaOnlineUnityHelper")]
		private static extern void pay(
			IntPtr iContext, string iTargetGameObject, int iUnitPrice, 
			string iUnitName, int iCount, string iCallBackInfo, 
			string iCallBackUrl, string iPayResultListener);

		/// <summary>
		/// charge接口用于用户触发非定额计费.
		/// </summary>
		/// <param name="iContext">上下文Activity.</param>
		/// <param name="iTargetGameObject">监听游戏对象.</param>
		/// <param name="iUnitName">虚拟货币名称.</param>
		/// <param name="iUnitPrice">游戏道具单位价格(单位:分).</param>
		/// <param name="iCount">商品或道具数量.</param>
		/// <param name="iCallBackInfo">由游戏开发者定义传入的字符串，会与支付结果一同发送给游戏服务器.游戏服务器可通过该字段判断交易的详细内容（金额角色等）</param>
		/// <param name="iCallBackUrl">将支付结果通知给游戏服务器时的通知地址url，交易结束后，系统会向该url发送http请求，通知交易的结果金额callbackInfo等信息.</param>
		/// <param name="iPayResultListener">支付监听接口，隶属于gameObject对象的运行时脚本的方法名称，该方法会在收到通知后触发.</param>
		[DllImport("gangaOnlineUnityHelper")]
		private static extern void charge(
			IntPtr iContext, string iTargetGameObject, string iUnitName, 
			int iUnitPrice, int iCount, string iCallBackInfo, 
			string iCallBackUrl, string iPayResultListener);

#endregion

#region Player/Role info

		/// <summary>
		/// 部分渠道如UC渠道，要对游戏人物数据进行统计，而且为接入规范，调用时间：在游戏角色登录成功后调用.
		/// </summary>
		/// <param name="iContext">上下文Activity.</param>
		/// <param name="iRoleId">角色唯一标识(类似于GUID/token).</param>
		/// <param name="iRoleName">角色名.</param>
		/// <param name="iRoleLevel">角色等级.</param>
		/// <param name="iZoneId">区域ID.</param>
		/// <param name="iZoneName">区域名.</param>
		[DllImport("gangaOnlineUnityHelper")]
		private static extern void setRoleData(
			IntPtr iContext, string iRoleId, string iRoleName, 
			string iRoleLevel, string iZoneId, string iZoneName);

#endregion

#region Data

		/// <summary>
		/// 设定数据（备用接口）.
		/// </summary>
		/// <param name="iContext">上下文Activity.</param>
		/// <param name="iKey">Key.</param>
		/// <param name="iValue">Value.</param>
		[DllImport("gangaOnlineUnityHelper")]
		private static extern void setData (IntPtr iContext, string iKey, string iValue);

#endregion

#region Interface - Extension

		/// <summary>
		/// extend扩展接口.
		/// 扩展接口，有些 SDK， 要求必须接入统计接口或者其它特殊的接口
		/// 并且有返回值或者回调的函数，用户可以使用此接口调用，具体可以参考易接工具上的SDK的参数填写帮助。
		/// </summary>
		/// <param name="iContext">上下文Activity</param>
		/// <param name="iData">Data.</param>
		/// <param name="iGameObject">Game object.</param>
		/// <param name="iListener">Listener.</param>
		[DllImport("gangaOnlineUnityHelper")]
		private static extern void extend (
			IntPtr iContext, string iData, 
			string iGameObject, string iListener);

#endregion

		/// <summary>
		/// 释放函数.
		/// </summary>
		public void Dispose() {
			_targetGameObject = null;
			_updateStatusCallback = null;
			_loginCheckBaseUrl = null;
			_loginCheckCallback = null;
			_loginCheckSucceeded = null;
			_loginCheckFailed = null;
			_autoReloginCallback = null;
		}

		/// <summary>
		/// 更新状态.
		/// </summary>
		/// <param name="iStatus">状态.</param>
		private void UpdateStatus(SdkStatus iStatus) {
			_updateStatusCallback?.Invoke (iStatus);
			if (null != _userInfo) {
				_userInfo.status = iStatus;
			}
			if (null != _payment) {
				_payment.status = iStatus;
			}
		}
			
		/// <summary>
		/// 设定状态更新回调函数.
		/// </summary>
		/// <param name="iUpdateStatus">I update status.</param>
		public void SetUpdateStatusCallback(Action<SdkStatus> iUpdateStatus) {
			_updateStatusCallback = iUpdateStatus;
		}

		/// <summary>
		/// 登录.
		/// </summary>
		/// <param name="iTarget">登陆启动的目标对象.</param>
		/// <param name="iLoginCompleted">登录/登出完成回调函数.</param>
		/// <param name="iLoginCheckBaseUrl">登录校验Base Url.</param>
		/// <param name="iLoginCheckCallBack">登录校验回调函数.</param>
		/// <param name="iLoginCheckSucceeded">登录校验成功回调函数.</param>
		/// <param name="iLoginCheckFailed">登录校验失败回调函数.</param>
		/// <param name="iAutoReloginMaxCount">自动重登最大次数.</param>
		/// <param name="iAutoReloginCallback">自动重登回调函数.</param>
		public void Login(
			GameObject iTarget = null, 
			Action<string> iLoginCompleted = null, string iLoginCheckBaseUrl = null,
			Action<string, Action<string>, Action<string>> iLoginCheckCallBack = null,
			Action<string> iLoginCheckSucceeded = null, Action<string> iLoginCheckFailed = null,
			int iAutoReloginMaxCount = 3, Action<float> iAutoReloginCallback = null) {

			if (null == iTarget) {
				return;
			}
			_targetGameObject = iTarget;

			var unityPlayer = new AndroidJavaClass ("com.unity3d.player.UnityPlayer");
			var curActivity = unityPlayer.GetStatic<AndroidJavaObject> ("currentActivity");
			if (null == curActivity) {
				Error ("Login():The current activity is invalid!!!");
				return;
			}
			Info ("Login()");
			UpdateStatus (SdkStatus.Logining);

			// 登录校验Base Url
			if(false == string.IsNullOrEmpty(iLoginCheckBaseUrl)) {
				_loginCheckBaseUrl = iLoginCheckBaseUrl;

				Info ("Login()::LoginCheckBaseUrl:{0}", 
					_loginCheckBaseUrl);
			}

			// 设置登录校验回调函数
			if (null != iLoginCheckCallBack) {
				_loginCheckCallback = iLoginCheckCallBack;
			}
			if (null != iLoginCheckSucceeded) {
				_loginCheckSucceeded = iLoginCheckSucceeded;
			}
			if (null != iLoginCheckFailed) {
				_loginCheckFailed = iLoginCheckFailed;
			}
			_autoReloginMaxCount = iAutoReloginMaxCount;
			if (null != iAutoReloginCallback) {
				_autoReloginCallback = iAutoReloginCallback;
			}

			if (null != iTarget && null != iLoginCompleted) {
				
				Info ("Login()::TargetName:{0} LoginCallback:{1}", 
					iTarget.name, iLoginCompleted.Method.Name);
				
				// 设定回调函数
				setLoginListener (curActivity.GetRawObject (), iTarget.name, iLoginCompleted.Method.Name);
			}
			_isLogin = true;
			login (curActivity.GetRawObject (), "Login");

		}

		/// <summary>
		/// 重登录.
		/// </summary>
		public void ReLogin() {
			Info ("ReLogin():: --> {0}/{1}", _autoReloginCount, _autoReloginMaxCount);
			UpdateStatus (SdkStatus.AutoReLogining);

			// 超过重登最大尝试次数
			if (_autoReloginMaxCount < _autoReloginCount) {
				Error ("ReLogin()::Over --> {0}/{1}", _autoReloginCount, _autoReloginMaxCount);
				UpdateStatus (SdkStatus.AutoReLoginFailed);
				return;
			}

			if (null != _autoReloginCallback) {
				_autoReloginCallback (0.2f);
			} else {
				++_autoReloginCount;
				Login (_targetGameObject);
			}
		}

		/// <summary>
		/// 登出.
		/// </summary>
		public void Logout() {
			var unityPlayer = new AndroidJavaClass ("com.unity3d.player.UnityPlayer");
			var curActivity = unityPlayer.GetStatic<AndroidJavaObject> ("currentActivity");
			if (null == curActivity) {
				Error ("Logout()::The current activity is invalid!!!");
				return;
			}
			Info ("Logout()");

			_isLogin = false;
			logout(curActivity.GetRawObject (), "Logout");
			UpdateStatus (SdkStatus.Logouted);
		}

		/// <summary>
		/// 解析登录信息.
		/// </summary>
		/// <returns>Json数据对象.</returns>
		/// <param name="iJsonDetail">Json详细.</param>
		public SdkAccountBaseInfo ParserLoginResponseInfo (string iJsonDetail) {

			// 登录完成
			UpdateStatus (SdkStatus.LoginCompleted);

			if (null == _userInfo) {
				_userInfo = new OneSdkUserInfo ();
			}
			if (null == _userInfo) {
				Error ("ParserLoginResponseInfo()::Memory New Error!!!!");
				return _userInfo;
			}
			_userInfo.Reset ();

			var sfjson = new SfjsonObject (iJsonDetail);
			var status = (string)sfjson.get ("result");
			var customParams = (string)sfjson.get ("customParams");
			Info ("ParserLoginResponseInfo()::CustomParams:{0}", customParams);

			switch (status)
			{
				// 登出
				case SLogout:
				{
					_sfUserInfo = null;
					UpdateStatus (SdkStatus.Logouted);

					// 重登录
					if(_isLogin) {
						ReLogin ();
					}

					// 登录成功
					break;
				}

				case SLoginSuccess:
				{
					UpdateStatus (SdkStatus.LoginCompleted);

					var userinfoTmp = (SfjsonObject)sfjson.get ("userinfo");
					if (null != userinfoTmp) {
						_userInfo.userId = long.Parse ((string)userinfoTmp.get ("id"));
						_userInfo.channelId = (string)userinfoTmp.get ("channelid");
						_userInfo.channelUserId = (string)userinfoTmp.get ("channeluserid");
						_userInfo.userName = (string)userinfoTmp.get ("username");
						_userInfo.token = (string)userinfoTmp.get ("token");
						_userInfo.productCode = (string)userinfoTmp.get ("productcode");
						_sfUserInfo = new SFOnlineUser (
							_userInfo.userId, 
							_userInfo.channelId, 
							_userInfo.channelUserId,
							_userInfo.userName, 
							_userInfo.token, 
							_userInfo.productCode);
					}
					if (null != _loginCheckCallback) {

						// 校验数据
						var checkInfo = GetLoginCheckInfoJson(_userInfo);
						if (string.IsNullOrEmpty (checkInfo)) {
							Error ("ParserLoginResponseInfo():JsonConvert Failed!!!(Data:{0})",
								_userInfo.ToString());
							UpdateStatus(SdkStatus.LoginCheckFailed);
							return null;
						}
						Info ("ParserLoginResponseInfo()::CheckInfo:{0}", checkInfo);
						_loginCheckCallback (checkInfo, _loginCheckSucceeded, _loginCheckFailed);
					} else {

						// 登录校验&更新状态
						LoginCheck (_userInfo);
					}

					// 登录失败
					break;
				}

				case SLoginFailed:
				{
					UpdateStatus (SdkStatus.LoginFailed);
					// 重登录
					if(_isLogin) {
						ReLogin ();
					}

					break;
				}
			} 
			Info ("ParserLoginResponseInfo()::UserResultInfo:{0}", _userInfo.ToString());
			return _userInfo;
		}

		/// <summary>
		/// 取得登录校验用信息（Json格式）.
		/// </summary>
		/// <returns>登录校验用信息（Json格式）.</returns>
		/// <param name="iUserInfo">易接用户信息.</param>
		private string GetLoginCheckInfoJson(OneSdkUserInfo iUserInfo) {
			var checkInfo = new OneSdkLoginCheckInfo ();
			checkInfo.Apply (iUserInfo);

			// 详细数据
			var jsonData = UtilsJson<OneSdkLoginCheckInfo>.ConvertToJsonString(checkInfo);
			if (!string.IsNullOrEmpty(jsonData)) return jsonData;
			Error ("createLoginCheckURL():JsonConvert Failed!!!(Data:{0})",
				checkInfo.ToString());
			UpdateStatus(SdkStatus.LoginCheckFailed);
			return null;
		}

		/// <summary>
		/// 登录校验.
		/// </summary>
		/// <returns>校验状态.</returns>
		/// <param name="iUserInfo">用户信息.</param>
		private void LoginCheck (OneSdkUserInfo iUserInfo) {

			UpdateStatus(SdkStatus.LoginChecking);
			if (string.IsNullOrEmpty (_loginCheckBaseUrl)) {
				Error ("loginCheck()::The base url of login check is invalid!!!");
				UpdateStatus(SdkStatus.LoginCheckFailed);
				return;
			}

			var loginCheckUrl = _loginCheckBaseUrl;
			Info ("loginCheck()::Url:{0}", loginCheckUrl);
			if (string.IsNullOrEmpty (loginCheckUrl)) {
				Error ("loginCheck()::The url of login check is invalid!!!");
				UpdateStatus(SdkStatus.LoginCheckFailed);
				return;
			}

			// 详细数据
			var jsonData = GetLoginCheckInfoJson(iUserInfo);
			if (string.IsNullOrEmpty (jsonData)) {
				Error ("createLoginCheckURL():JsonConvert Failed!!!(Data:{0})",
					iUserInfo.ToString());
				UpdateStatus(SdkStatus.LoginCheckFailed);
				return;
			}

			var response = ExecuteHttpPost (loginCheckUrl, jsonData);
			if (null == response) {
				UpdateStatus(SdkStatus.LoginCheckFailed);
				return;
			}
			Info ("loginCheck()::Result:{0}", response.StatusCode);
			if (HttpStatusCode.OK != response.StatusCode) {
				_loginCheckFailed?.Invoke (response.StatusCode.ToString());
				UpdateStatus(SdkStatus.LoginCheckFailed);
				return;
			}

			_loginCheckSucceeded?.Invoke (response.StatusCode.ToString());

			UpdateStatus(SdkStatus.LoginCheckSucceeded);
		}

		/// <summary>
		/// 执行Http请求(Post)
		/// </summary>
		/// <param name="iUrl">Url</param>
		/// <param name="iJsonDataDetail">数据详细(Json格式)</param>
		/// <returns></returns>
		private HttpWebResponse ExecuteHttpPost (string iUrl, string iJsonDataDetail)
		{
			HttpWebRequest request;
			//如果是发送HTTPS请求  
			if (iUrl.StartsWith("https", StringComparison.OrdinalIgnoreCase))
			{
				request = WebRequest.Create(iUrl) as HttpWebRequest;
			}
			else
			{
				request = WebRequest.Create(iUrl) as HttpWebRequest;
			}

			if (null == request) return null;
			
			// 请求参数设定
			request.Method = "POST";
			request.ContentType = "application/json";
			request.Headers["X-MC-GAME-ID"] = "2";

			//发送POST数据  
			var buffer = new StringBuilder();
			buffer.AppendFormat("data={0}", HttpHelper.Instance.ConvertToEncode(iJsonDataDetail));
			Info("executeHttpPost()::Params:{0}", buffer.ToString());

			var data = Encoding.UTF8.GetBytes(buffer.ToString());
			using (var stream = request.GetRequestStream())
			{
				stream.Write(data, 0, data.Length);
			}

			// string[] values = request.Headers.GetValues("Content-Type");
			return request.GetResponse() as HttpWebResponse;
		}
			
		/// <summary>
		/// 支付函数.
		/// </summary>
		/// <param name="iTarget">游戏对象.</param>
		/// <param name="iIapItemPrice">购买道具价格.</param>
		/// <param name="iIapItemName">购买道具名.</param>
		/// <param name="iIapItemCount">购买道具数量.</param>
		/// <param name="iOtherDiyInfo">其他自定义信息.</param>
		/// <param name="iNotifyUrl">支付结果通知URL（一般与游戏服务器上设置该URL）.</param>
		/// <param name="iOnPayCompleted">支付完成回调函数.</param>
		public void Pay (
			GameObject iTarget, 
			int iIapItemPrice,
			string iIapItemName,
			int iIapItemCount, 
			string iOtherDiyInfo, 
			string iNotifyUrl, 
			Action<string> iOnPayCompleted) {
			
			if (null == iTarget) {
				return;
			}
			_targetGameObject = iTarget;

			var unityPlayer = new AndroidJavaClass ("com.unity3d.player.UnityPlayer");
			var curActivity = unityPlayer.GetStatic<AndroidJavaObject> ("currentActivity");
			if (null == curActivity) {
				Error ("Pay():The current activity is invalid!!!");
				return;
			}
			Info ("Pay()");
			UpdateStatus (SdkStatus.Purchasing);

			// 支付
			pay (curActivity.GetRawObject (), 
				iTarget.name, iIapItemPrice, iIapItemName, 
				iIapItemCount, iOtherDiyInfo, iNotifyUrl, 
				iOnPayCompleted.Method.Name);

		}
			
		/// <summary>
		/// 解析支付信息.
		/// </summary>
		/// <returns>Json数据对象.</returns>
		/// <param name="iJsonDetail">Json详细.</param>
		/// <param name="iOnPaymentSucceeded">支付成功回调函数.</param>
		public SdkPaymentBaseInfo ParserPayResponseInfo (
			string iJsonDetail, 
			Action<SdkAccountBaseInfo, string> iOnPaymentSucceeded) {

			if (null == iOnPaymentSucceeded) {
				Warning ("SDKParserPaymentInfo()::OnPaymentSucceeded is null!!!");
			}

			UpdateStatus (SdkStatus.PurchaseCompleted);
			Info ("ParserPayResponseInfo()::Detail:{0}", iJsonDetail);
			var sfjson = new SfjsonObject (iJsonDetail);
			var status = (string)sfjson.get ("result");
			if (string.IsNullOrEmpty (status)) {
				Error ("ParserPayResponseInfo()::Data Format Invalid!!!!(Detail:{0})", iJsonDetail);
				return null;
			}
			var data = (string)sfjson.get ("data");
			if (string.IsNullOrEmpty (data)) {
				Error ("ParserPayResponseInfo()::Data Format Invalid!!!!(Detail:{0})", iJsonDetail);
				return null;
			}

			if (null == _payment) {
				_payment = new OneSdkPaymentInfo ();
				_payment.Reset ();
			}
			if (null == _payment) {
				Error ("ParserPayResponseInfo()::(OneSDKPaymentInfo)Memory New Error!!!!");
				return null;
			}

			switch (status)
			{
				case SPaySuccess:
				{
					UpdateStatus (SdkStatus.PurchaseSucceeded);
					_payment.succeeded = true;

					if (null != iOnPaymentSucceeded) {
						iOnPaymentSucceeded (_userInfo, _payment.orderNo);
					} else {
						Warning ("ParserPayResponseInfo()::OnPaymentSucceeded is null!!!!");
					}

					break;
				}

				case SPayFailed:
					UpdateStatus (SdkStatus.PurchaseFailed);
					_payment.succeeded = false;
					break;
				case SPayOrderNo:
					UpdateStatus (SdkStatus.PurchaseOrdered);
					_payment.succeeded = true;
					_payment.orderNo = data;
					break;
			}
			Info ("ParserPayResponseInfo()::PayInfo:{0}", _payment.ToString());
			return _payment;
		}

		/// <summary>
		/// 设定角色信息.
		/// </summary>
		/// <param name="iRoleId">角色ID.</param>
		/// <param name="iRoleName">角色名.</param>
		/// <param name="iRoleLevel">角色等级.</param>
		/// <param name="iZoneId">游戏区ID.</param>
		/// <param name="iZoneName">游戏区名.</param>
		public void SetRoleData(
			string iRoleId, string iRoleName, 
			string iRoleLevel, string iZoneId, string iZoneName) {
			var unityPlayer = new AndroidJavaClass ("com.unity3d.player.UnityPlayer");
			var curActivity = unityPlayer.GetStatic<AndroidJavaObject> ("currentActivity");
			if (null == curActivity) {
				Error ("SetRoleData():The current activity is invalid!!!");
				return;
			}
			Info ("SetRoleData()");

			setRoleData (curActivity.GetRawObject (),
				iRoleId, iRoleName, iRoleLevel, iZoneId, iZoneName);

		}

		/// <summary>
		/// 创建角色.
		/// </summary>
		/// <param name="iKey">Key.</param>
		/// <param name="iRoleInfo">角色信息.</param>
		private void SetData(
			string iKey, OneSdkRoleInfo iRoleInfo) {

			var unityPlayer = new AndroidJavaClass ("com.unity3d.player.UnityPlayer");
			var curActivity = unityPlayer.GetStatic<AndroidJavaObject> ("currentActivity");
			if (null == curActivity) {
				Error ("SetData():The current activity is invalid!!!");
				return;
			}
			Info ("SetData()::Key:{0} RoleInfo:{1}",iKey, iRoleInfo.ToString());
			var roleInfo = new SfjsonObject ();
			
			roleInfo.put("roleId", iRoleInfo.id);
			roleInfo.put("roleName", iRoleInfo.name);
			roleInfo.put("roleLevel", iRoleInfo.level);
			roleInfo.put("zoneId", iRoleInfo.zoneId);
			roleInfo.put("zoneName", iRoleInfo.zoneName);
			roleInfo.put("balance", iRoleInfo.balance);
			roleInfo.put("vip", iRoleInfo.vip);
			roleInfo.put("partyName", iRoleInfo.partyName);
			roleInfo.put("roleCTime", iRoleInfo.cTime);
			roleInfo.put("roleLevelMTime", iRoleInfo.roleLevelMTime);

			Info ("SetData()::RoleInfo:{0}", roleInfo.toString ());
			// 设定信息
			setData (curActivity.GetRawObject(), iKey, roleInfo.toString ());
		}

		/// <summary>
		/// 创建角色.
		/// </summary>
		/// <param name="iRoleInfo">角色信息.</param>
		public void CreateRole(OneSdkRoleInfo iRoleInfo) {

			Info ("CreateRole()::RoleInfo:{0}", iRoleInfo.ToString());
			// 设定数据
			SetData("createrole", iRoleInfo);
		}

		/// <summary>
		/// 更新等级信息（升级时）.
		/// </summary>
		/// <param name="iRoleInfo">角色信息.</param>
		public void UpdateRoleInfoWhenLevelUp(OneSdkRoleInfo iRoleInfo) {

			Info ("UpdateRoleInfoWhenLevelUp()::RoleInfo:{0}", iRoleInfo.ToString ());
			
			// 设定数据
			SetData("levelUp", iRoleInfo);
		}

		/// <summary>
		/// 更新角色信息（登录服务器后）.
		/// </summary>
		/// <param name="iRoleInfo">角色信息.</param>
		public void UpdateRoleInfoWhenEnterServer(OneSdkRoleInfo iRoleInfo) {

			Info ("UpdateRoleInfoWhenEnterServer()::RoleInfo:{0}", iRoleInfo.ToString ());

			// 设定数据
			SetData("enterServer", iRoleInfo);
		}

		/// <summary>
		/// 退出.
		/// </summary>
		/// <param name="iTarget">游戏对象.</param>
		/// <param name="iOnExited">退出回调函数.</param>
		public void Exit(
			GameObject iTarget = null, 
			Action<string> iOnExited = null) {
			var unityPlayer = new AndroidJavaClass ("com.unity3d.player.UnityPlayer");
			var curActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
			if (null == curActivity) {
				Error ("SetRoleData():The current activity is invalid!!!");
				return;
			}
			Info ("Exit()::Target:{0} Method:{1}",
				null == iTarget ? "null" : iTarget.name,
				null == iOnExited ? "null" : iOnExited.Method.Name);

			exit (curActivity.GetRawObject (), 
				(null == iTarget) ? "" : iTarget.name, 
				(null == iOnExited) ? "" : iOnExited.Method.Name);
		}
	}
}

#endif
