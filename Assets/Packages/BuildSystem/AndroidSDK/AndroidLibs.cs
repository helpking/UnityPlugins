using System;
using UnityEngine;
using Packages.Common;
using Packages.Common.Base;
using Packages.Settings;

#if UNITY_ANDROID
using Packages.BuildSystem.AndroidSDK.Platforms.Huawei;
using Packages.BuildSystem.AndroidSDK.Platforms.Tiange;

namespace Packages.BuildSystem.AndroidSDK {

	/// <summary>
	/// Android libs.
	/// </summary>
	public sealed class AndroidLibs : SingletonBase<AndroidLibs>, IDisposable {

		/// <summary>
		/// 实例.
		/// </summary>
		private AndroidSdkBase _sdkInstance;

		private SdkStatus _status = SdkStatus.Invalid;
		/// <summary>
		/// 状态.
		/// </summary>
		public SdkStatus Status {
			get
			{
				return null == _sdkInstance ? SdkStatus.Invalid : _status;
			}
			private set { 
				_status = value;
			}
		}

		/// <summary>
		/// 释放函数.
		/// </summary>
		public void Dispose()
		{
			Info ("Dispose ()");
			_sdkInstance?.Dispose();
		}

		/// <summary>
		/// 初始化.
		/// </summary>
		protected override void Init()
		{
			base.Init ();

			// 取得实例
			_sdkInstance = GetAndroidSdkInstance();
			_sdkInstance?.SetUpdateStatusCallback (UpdateStatus);
		}

		/// <summary>
		/// 更新状态.
		/// </summary>
		/// <param name="iStatus">状态.</param>
		private void UpdateStatus(SdkStatus iStatus) {
			Info ("UpdateStatus():: -> {0}", iStatus);
			Status = iStatus;
		}

		/// <summary>
		/// 取得安卓SDK实例.
		/// </summary>
		/// <returns>The android SDK instance.</returns>
		private AndroidSdkBase GetAndroidSdkInstance() {
		
			AndroidSdkBase objRet = null;
			var _platformType = SysSettings.GetInstance ().PlatformType;
			switch (_platformType) {
			case PlatformType.Huawei:
				{
					objRet = new HuaweiSdk ();
				}
				break;
			case PlatformType.Tiange:
				{
					objRet = new TiangeSDK();
				}
				break;
			case PlatformType.None:
				break;
			case PlatformType.iOS:
				break;
			case PlatformType.Android:
				break;
			default:
				{
					Error ("GetAndroidSDKInstance():The platformType is invalid setting in buildinfo.asset!!!(TPlatformType:{0})",
						_platformType);
				}
				break;
			}
			return objRet;
		}

		/// <summary>
		/// 显示Debug信息.
		/// </summary>
		/// <param name="iDebugInfo">Debug信息.</param>
		public void ShowDebugInfo(string iDebugInfo) {
			if (null == _sdkInstance) {
				Error ("ShowDebugInfo():The instance of android sdk is invalid!!!");
				return;
			}
			Info ("ShowDebugInfo():{0}", iDebugInfo);
			_sdkInstance.ShowDebugInfo (iDebugInfo);
		}

		/// <summary>
		/// 取得状态.
		/// </summary>
		/// <returns>状态.</returns>
		/// <param name="iStatusCode">状态码.</param>
		public SdkStatus GetStatus(string iStatusCode) {
			if (null == _sdkInstance) {
				Error ("GetStatus():The instance of android sdk is invalid!!!");
				return SdkStatus.Invalid;
			}
			int statusValue = Convert.ToInt16 (iStatusCode);
			var status = (SdkStatus)statusValue;
			Info ("GetStatus()::Status:{0}", status);
			return status;
		}

		/// <summary>
		/// 登录.
		/// </summary>
		/// <param name="iTarget">游戏对象.</param>
		/// <param name="iLoginCompleted">登录/登出完成回调函数.</param>
		/// <param name="iLoginCheckBaseUrl">登录校验Base Url.</param>
		/// <param name="iLoginCheckCallBack">登录检测回调函数.</param>
		/// <param name="iLoginCheckSucceeded">登录检测成功回调函数.</param>
		/// <param name="iLoginCheckFailed">登录检测失败回调函数.</param>
		/// <param name="iAutoReloginMaxCount">自动重登最大次数.</param>
		/// <param name="iAutoReloginCallback">自动重登回调函数.</param>
		public void Login (
			GameObject iTarget, 
			Action<string> iLoginCompleted, string iLoginCheckBaseUrl,
			Action<string, Action<string>, Action<string>> iLoginCheckCallBack,
			Action<string> iLoginCheckSucceeded, Action<string> iLoginCheckFailed,
			int iAutoReloginMaxCount, Action<float> iAutoReloginCallback) {

			if (null == _sdkInstance) {
				Error ("Login():The instance of android sdk is invalid!!!");
				return;
			}
			if (null == iTarget) {
				Error ("Login():The ProjectName of game object is invalid!!!");
				return;
			}
			Info ("Login()");
			_sdkInstance.Login (
				iTarget, iLoginCompleted, iLoginCheckBaseUrl, 
				iLoginCheckCallBack, iLoginCheckSucceeded, iLoginCheckFailed,
				iAutoReloginMaxCount, iAutoReloginCallback);
		}

		/// <summary>
		/// 重登录.
		/// </summary>
		public void Relogin()  {
			if (null == _sdkInstance) {
				Error ("Login():The instance of android sdk is invalid!!!");
				return;
			}
			Info ("Relogin()");
			_sdkInstance.Relogin ();
		}

		/// <summary>
		/// 登出.
		/// </summary>
		public void Logout() {
			if (null == _sdkInstance) {
				Error ("Logout():The instance of android sdk is invalid!!!");
				return;
			}
			Info ("Logout()");
			_sdkInstance.Logout ();
		}

		/// <summary>
		/// SDK初始化.
		/// </summary>
		public void SDKInit(
			GameObject iTarget)
		{
			SDKInit(iTarget, null);
		}

		/// <summary>
		/// SDK初始化.
		/// </summary>
		public void SDKInit(
			GameObject iTarget, 
			Action<string> iOnSdkInitCompleted) {
			if (null == _sdkInstance) {
				Error ("SDKInit():The instance of android sdk is invalid!!!");
				return;
			}
			if (null == iTarget) {
				Error ("SDKInit():The ProjectName of game object is invalid!!!");
				return;
			}
			Info ("SDKInit()");
			_sdkInstance.Init (iTarget, iOnSdkInitCompleted);
		}

		/// <summary>
		/// 添加玩家信息.
		/// </summary>
		/// <param name="iTarget">目标对象.</param>
		/// <param name="iGameRank">游戏等级.</param>
		/// <param name="iGameRole">游戏角色.</param>
		/// <param name="iGameArea">游戏区.</param>
		/// <param name="iGameSociaty">游戏工会.</param>
		/// <param name="iOnCompleted">完成回调函数.</param>
		public void AddPlayerInfo(
			GameObject iTarget, 
			String iGameRank,
			String iGameRole,
			String iGameArea,
			String iGameSociaty,
			Action<string> iOnCompleted = null) {
			if (null == _sdkInstance) {
				Error ("AddPlayerInfo():The instance of android sdk is invalid!!!");
				return;
			}
			if (null == iTarget) {
				Error ("AddPlayerInfo():The ProjectName of game object is invalid!!!");
				return;
			}
			Info ("AddPlayerInfo()");
			_sdkInstance.AddPlayerInfo (
				iTarget, iGameRank, iGameRole,
				iGameArea, iGameSociaty, iOnCompleted);
		}
			
		/// <summary>
		/// 取得玩家信息.
		/// </summary>
		/// <param name="iTarget">游戏对象.</param>
		/// <param name="iOnCompleted">完成回调函数.</param>
		public void GetPlayerInfo (
			GameObject iTarget, 
			Action<string> iOnCompleted = null) {

			if (null == _sdkInstance) {
				Error ("GetPlayerInfo():The instance of android sdk is invalid!!!");
				return;
			}
			if (null == iTarget) {
				Error ("GetPlayerInfo():The ProjectName of game object is invalid!!!");
				return;
			}
			Info ("GetPlayerInfo()");
			_sdkInstance.GetPlayerInfo (
				iTarget, iOnCompleted);
		}

		/// <summary>
		/// 解析用户信息.
		/// </summary>
		/// <returns>用户信息.</returns>
		/// <param name="iUserInfo">用户信息(Json格式数据).</param>
		public SdkAccountBaseInfo ParserAccountInfo(string iUserInfo)  {
			if (null == _sdkInstance) {
				Error ("ParserAccountInfo():The instance of android sdk is invalid!!!");
				return null;
			}
			Info ("ParserAccountInfo():{0}", iUserInfo);
			return _sdkInstance.ParserAccountInfo (iUserInfo);

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

			if (null == _sdkInstance) {
				Error ("Pay():The instance of android sdk is invalid!!!");
				return;
			}

			Info ("Pay()::Price:{0} Name:{1} Count:{2} OtherDIYInfo:{3} NotifyUrl:{4}/ Target:{5} Callback:{6}",
				iIapItemPrice, iIapItemName, iIapItemCount, iOtherDiyInfo, iNotifyUrl,
				iTarget.name, iOnPayCompleted.Method.Name);

			// 支付
			_sdkInstance.Pay (
				iTarget, iIapItemPrice, iIapItemName, 
				iIapItemCount, iOtherDiyInfo, iNotifyUrl, iOnPayCompleted);
		}
			
		/// <summary>
		/// 解析支付信息.
		/// </summary>
		/// <returns>用户信息.</returns>
		/// <param name="iPayInfo">支付信息(Json格式数据).</param>
		/// <param name="iOnPaymentSucceeded">支付成功回调函数.</param>
		public SdkPaymentBaseInfo ParserPaymentInfo(
			string iPayInfo,
			Action<SdkAccountBaseInfo, string> iOnPaymentSucceeded)  {

			if (null == iOnPaymentSucceeded) {
				Warning ("ParserPaymentInfo()::OnPaymentSucceeded is null!!!");
			}

			if (null == _sdkInstance) {
				Error ("ParserPaymentInfo():The instance of android sdk is invalid!!!");
				return null;
			}
			Info ("ParserPaymentInfo():{0}", iPayInfo);
			return _sdkInstance.ParserPaymentInfo (iPayInfo, iOnPaymentSucceeded);

		}
			
		/// <summary>
		/// 创建角色.
		/// </summary>
		/// <param name="iRoleId">角色ID（必须为数字）.</param>
		/// <param name="iRoleName">角色名（不能为空，不能为null）.</param>
		/// <param name="iRoleLevel">角色等级（必须为数字，不能为0，默认1）.</param>
		/// <param name="iZoneID">游戏区ID（必须为数字，不能为0，默认为1）.</param>
		/// <param name="iZoneName">游戏区名（不能为空，不能为null）.</param>
		/// <param name="iBalance">游戏币余额（必须为数字，默认0）.</param>
		/// <param name="iVip">VIP等级（必须为数字，默认诶1）.</param>
		/// <param name="iPartyName">当前所属帮派（不能为空，不能为null，默认：无帮派）.</param>
		/// <param name="iRoleCTime">角色创建时间（单位：秒）.</param>
		/// <param name="iRoleLevelMTime">角色等级变化时间（单位：秒）.</param>
		public void CreateRole(
			string iRoleId, string iRoleName, string iRoleLevel, string iZoneID, string iZoneName,
			string iBalance, string iVip, string iPartyName, string iRoleCTime, string iRoleLevelMTime) {

			if (null == _sdkInstance) {
				Error ("CreateRole():The instance of android sdk is invalid!!!");
				return;
			}
			var _roleInfo = _sdkInstance.CreateRoleInfo (
				iRoleId, iRoleName, iRoleLevel, iZoneID, iZoneName, iBalance, iVip, iPartyName, iRoleCTime, iRoleLevelMTime);
			if (null == _roleInfo) {
				Error ("CreateRole():Create Role Info Failed!!!");
				return;
			}
			Info ("CreateRole()::RoleInfo:{0}", _roleInfo.ToString());
			// 设定数据
			_sdkInstance.CreateRole(_roleInfo);
		}

		/// <summary>
		/// 更新等级信息（升级时）.
		/// </summary>
		/// <param name="iRoleId">角色ID（必须为数字）.</param>
		/// <param name="iRoleName">角色名（不能为空，不能为null）.</param>
		/// <param name="iRoleLevel">角色等级（必须为数字，不能为0，默认1）.</param>
		/// <param name="iZoneID">游戏区ID（必须为数字，不能为0，默认为1）.</param>
		/// <param name="iZoneName">游戏区名（不能为空，不能为null）.</param>
		/// <param name="iBalance">游戏币余额（必须为数字，默认0）.</param>
		/// <param name="iVip">VIP等级（必须为数字，默认诶1）.</param>
		/// <param name="iPartyName">当前所属帮派（不能为空，不能为null，默认：无帮派）.</param>
		/// <param name="iRoleCTime">角色创建时间（单位：秒）.</param>
		/// <param name="iRoleLevelMTime">角色等级变化时间（单位：秒）.</param>
		public void UpdateRoleInfoWhenLevelup(
			string iRoleId, string iRoleName, string iRoleLevel, string iZoneID, string iZoneName,
			string iBalance, string iVip, string iPartyName, string iRoleCTime, string iRoleLevelMTime) {

			if (null == _sdkInstance) {
				Error ("UpdateRoleInfoWhenLevelup():The instance of android sdk is invalid!!!");
				return;
			}
			var _roleInfo = _sdkInstance.CreateRoleInfo (
				iRoleId, iRoleName, iRoleLevel, iZoneID, iZoneName, iBalance, iVip, iPartyName, iRoleCTime, iRoleLevelMTime);
			if (null == _roleInfo) {
				Error ("UpdateRoleInfoWhenLevelup():Create Role Info Failed!!!");
				return;
			}
			Info ("UpdateRoleInfoWhenLevelup()::RoleInfo:{0}", _roleInfo.ToString());

			// 设定数据
			_sdkInstance.UpdateRoleInfoWhenLevelup(_roleInfo);
		}

		/// <summary>
		/// 更新角色信息（登录服务器后）.
		/// </summary>
		/// <param name="iRoleId">角色ID（必须为数字）.</param>
		/// <param name="iRoleName">角色名（不能为空，不能为null）.</param>
		/// <param name="iRoleLevel">角色等级（必须为数字，不能为0，默认1）.</param>
		/// <param name="iZoneID">游戏区ID（必须为数字，不能为0，默认为1）.</param>
		/// <param name="iZoneName">游戏区名（不能为空，不能为null）.</param>
		/// <param name="iBalance">游戏币余额（必须为数字，默认0）.</param>
		/// <param name="iVip">VIP等级（必须为数字，默认诶1）.</param>
		/// <param name="iPartyName">当前所属帮派（不能为空，不能为null，默认：无帮派）.</param>
		/// <param name="iRoleCTime">角色创建时间（单位：秒）.</param>
		/// <param name="iRoleLevelMTime">角色等级变化时间（单位：秒）.</param>
		public void UpdateRoleInfoWhenEnterServer(
			string iRoleId, string iRoleName, string iRoleLevel, string iZoneID, string iZoneName,
			string iBalance, string iVip, string iPartyName, string iRoleCTime, string iRoleLevelMTime) {

			if (null == _sdkInstance) {
				Error ("UpdateRoleInfoWhenEnterServer():The instance of android sdk is invalid!!!");
				return;
			}
			var _roleInfo = _sdkInstance.CreateRoleInfo (
				iRoleId, iRoleName, iRoleLevel, iZoneID, iZoneName, iBalance, iVip, iPartyName, iRoleCTime, iRoleLevelMTime);
			if (null == _roleInfo) {
				Error ("UpdateRoleInfoWhenEnterServer():Create Role Info Failed!!!");
				return;
			}
			Info ("UpdateRoleInfoWhenEnterServer()::RoleInfo:{0}", _roleInfo.ToString());

			// 设定数据
			_sdkInstance.UpdateRoleInfoWhenEnterServer(_roleInfo);
		}

		/// <summary>
		/// 退出.
		/// </summary>
		/// <param name="iTarget">游戏对象.</param>
		/// <param name="iOnExited">退出回调函数.</param>
		public void Exit(
			GameObject iTarget = null, 
			Action<string> iOnExited = null) {
			if (null == _sdkInstance) {
				Error ("Exit():The instance of android sdk is invalid!!!");
				return;
			}
			_sdkInstance.Exit (iTarget, iOnExited);
		}
	}
}
#endif