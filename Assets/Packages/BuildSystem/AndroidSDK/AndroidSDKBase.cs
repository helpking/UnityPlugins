using System;
using Packages.Common.Base;
using UnityEngine;

namespace Packages.BuildSystem.AndroidSDK {

	/// <summary>
	/// AndroidSDK 接入基类.
	/// </summary>
	public abstract class AndroidSdkBase : ClassExtension, IAndroidSdk, IDisposable {

		/// <summary>
		/// 状态更新函数.
		/// </summary>
		private Action<SdkStatus> _updateStatusCallback;

		/// <summary>
		/// 登录校验Base Url.
		/// </summary>
		protected string LoginCheckBaseUrl;

		/// <summary>
		/// SDK初始化完成回调函数.
		/// </summary>
		protected Action<string> SdkInitCompletedCallback;

		/// <summary>
		/// 登录校验回调函数.
		/// </summary>
		protected Action<string, Action<string>, Action<string>> LoginCheckCallback;

		/// <summary>
		/// 登录校验成功回调函数.
		/// </summary>
		protected Action<string> LoginCheckSucceeded;

		/// <summary>
		/// 登录校验失败回调函数.
		/// </summary>
		protected Action<string> LoginCheckFailed;

		/// <summary>
		/// 自动重登最大次数.
		/// </summary>
		protected int AutoReloginMaxCount = 3;

		/// <summary>
		/// 自动重登回调函数.
		/// </summary>
		protected Action<float> AutoReloginCallback;

		/// <summary>
		/// 释放函数.
		/// </summary>
		public void Dispose() {
			Info ("Dispose ()");
			SdkDispose ();
		}

#region interface - implement

		/// <summary>
		/// 显示Debug信息.
		/// </summary>
		/// <param name="iDebugInfo">Debug信息.</param>
		public void ShowDebugInfo(string iDebugInfo) {

			//通过查看源码，我们可以发现UnityPlayer这个类可以获取当前的Activity
			//帮助手册上 AndroidJavaClass：通过指定类名可以构造出一个类
			var jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");

			//currentActivity字符串对应源码中UnityPlayer类下 的 Activity 变量名。
			//通过构造出的Activity根据字符串获取对象
			var jo = jc.GetStatic<AndroidJavaObject>("currentActivity");

			//根据方法名调用方法，传入一个参数数组，这里我们只有一个，就只传一个
			jo.Call("UToA_ShowDebugInfo", iDebugInfo);
		}

		/// <summary>
		/// 设定状态更新回调函数.
		/// </summary>
		/// <param name="iUpdateStatus">I update status.</param>
		public virtual void SetUpdateStatusCallback(Action<SdkStatus> iUpdateStatus) {
			_updateStatusCallback = iUpdateStatus;
		}

		/// <summary>
		/// 初始化.
		/// </summary>
		/// <param name="iTarget">游戏对象.</param>
		/// <param name="iOnCompleted">完成回调函数.</param>
		public void Init (GameObject iTarget, Action<string> iOnCompleted) {
			Info ("Init()");
			SdkInitCompletedCallback = iOnCompleted;
			SdkInit (iTarget, OnSdkInitCompleted);
		}

		/// <summary>
		/// SDK初始化完成.
		/// </summary>
		/// <param name="iDetail">详细信息.</param>
		protected virtual void OnSdkInitCompleted(string iDetail) {
			Info ("OnSDKInitCompleted()::Detail:{0}", iDetail);
			SdkInitCompletedCallback?.Invoke (iDetail);
		}

		/// <summary>
		/// 登录.
		/// </summary>
		/// <param name="iTarget">游戏对象.</param>
		/// <param name="iLoginCompleted">登录/登出完成回调函数.</param>
		/// <param name="iLoginCheckBaseUrl">登录校验Base Url.</param>
		/// <param name="iLoginCheckCallBack">登录校验回调函数.</param>
		/// <param name="iLoginCheckSucceeded">登录校验成功回调函数.</param>
		/// <param name="iLoginCheckFailed">登录校验失败回调函数.</param>
		/// <param name="iAutoReloginMaxCount">自动重登最大次数.</param>
		/// <param name="iAutoReloginCallback">自动重登回调函数.</param>
		public void Login (
			GameObject iTarget, 
			Action<string> iLoginCompleted, string iLoginCheckBaseUrl,
			Action<string, Action<string>, Action<string>> iLoginCheckCallBack,
			Action<string> iLoginCheckSucceeded, Action<string> iLoginCheckFailed, 
			int iAutoReloginMaxCount, Action<float> iAutoReloginCallback) {
			Info ("Login()");

			// 设置回调函数
			LoginCheckBaseUrl = iLoginCheckBaseUrl;
			LoginCheckCallback = iLoginCheckCallBack;
			LoginCheckSucceeded = iLoginCheckSucceeded;
			LoginCheckFailed = iLoginCheckFailed;
			AutoReloginMaxCount = iAutoReloginMaxCount;
			AutoReloginCallback = iAutoReloginCallback;

			// 登录
			SdkLogin (iTarget, iLoginCompleted);
		}
			
		/// <summary>
		/// 重登录.
		/// </summary>
		public void Relogin()  {
			Info ("Relogin()");
			SdkRelogin ();
		}

		/// <summary>
		/// 登录校验成功.
		/// </summary>
		/// <param name="iDetail">详细信息.</param>
		protected virtual void OnLoginCheckSucceeded(string iDetail) {
			Info ("OnLoginCheckSucceeded()::Detail:{0}", iDetail);
			LoginCheckSucceeded?.Invoke (iDetail);
		}

		/// <summary>
		/// 登录校验成功.
		/// </summary>
		/// <param name="iDetail">详细信息.</param>
		protected virtual void OnLoginCheckFailed(string iDetail) {
			Info ("OnLoginCheckFailed()::Detail:{0}", iDetail);
			LoginCheckFailed?.Invoke (iDetail);
		}

		/// <summary>
		/// 登出.
		/// </summary>
		public void Logout() {
			Info ("Logout()");
			SdkLogout ();

			// 释放
			Dispose ();
		}
			
		/// <summary>
		/// 添加玩家信息.
		/// </summary>
		/// <param name="iTarget">游戏对象.</param>
		/// <param name="iGameRank">游戏等级.</param>
		/// <param name="iGameRole">游戏角色.</param>
		/// <param name="iGameArea">游戏区.</param>
		/// <param name="iGameSociaty">游戏工会.</param>
		/// <param name="iOnCompleted">完成回调函数.</param>
		public void AddPlayerInfo (
			GameObject iTarget, 
			string iGameRank,
			string iGameRole,
			string iGameArea,
			string iGameSociaty,
			Action<string> iOnCompleted = null) {
			Info ("AddPlayerInfo()");
			SdkAddPlayerInfo (
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
			Info ("GetPlayerInfo()");
			SdkGetPlayerInfo (iTarget, iOnCompleted);
		}

		/// <summary>
		/// 解析用户信息.
		/// </summary>
		/// <returns>用户信息.</returns>
		/// <param name="iUserInfo">用户信息(Json格式数据).</param>>
		public SdkAccountBaseInfo ParserAccountInfo(string iUserInfo) {
			Info ("ParserAccountInfo()");
			return SdkParserAccountInfo(iUserInfo);
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

			Info ("Pay()");
			SdkPay (
				iTarget, iIapItemPrice, iIapItemName,
				iIapItemCount, iOtherDiyInfo, iNotifyUrl, iOnPayCompleted);
		}

		/// <summary>
		/// 解析支付信息.
		/// </summary>
		/// <returns>支付信息.</returns>
		/// <param name="iPayInfo">支付信息(Json格式数据).</param>
		/// <param name="iOnPaymentSucceeded">支付成功回调函数.</param>
		public SdkPaymentBaseInfo ParserPaymentInfo(string iPayInfo, 
			Action<SdkAccountBaseInfo, string> iOnPaymentSucceeded) {
			Info ("ParserPaymentInfo()");
			if (null == iOnPaymentSucceeded) {
				Warning ("ParserPaymentInfo()::OnPaymentSucceeded is null!!!");
			}
			return SdkParserPaymentInfo(iPayInfo, iOnPaymentSucceeded);
		}
			
		/// <summary>
		/// 创建SDK角色信息.
		/// </summary>
		/// <returns>SDK角色信息.</returns>
		/// <param name="iRoleId">角色ID（必须为数字）.</param>
		/// <param name="iRoleName">角色名（不能为空，不能为null）.</param>
		/// <param name="iRoleLevel">角色等级（必须为数字，不能为0，默认1）.</param>
		/// <param name="iZoneId">游戏区ID（必须为数字，不能为0，默认为1）.</param>
		/// <param name="iZoneName">游戏区名（不能为空，不能为null）.</param>
		/// <param name="iBalance">游戏币余额（必须为数字，默认0）.</param>
		/// <param name="iVip">VIP等级（必须为数字，默认诶1）.</param>
		/// <param name="iPartyName">当前所属帮派（不能为空，不能为null，默认：无帮派）.</param>
		/// <param name="iRoleCTime">角色创建时间（单位：秒）.</param>
		/// <param name="iRoleLevelMTime">角色等级变化时间（单位：秒）.</param>
		public SdkRoleBaseInfo CreateRoleInfo (
			string iRoleId, string iRoleName, string iRoleLevel, string iZoneId, string iZoneName,
			string iBalance, string iVip, string iPartyName, string iRoleCTime, string iRoleLevelMTime) {

			Info ("CreateRoleInfo()::RoleID:{0} RoleName:{1} RoleLevel:{2} ZoneID:{3} ZoneName:{4} " +
				"Balance:{5} Vip:{6} PartyName:{7} RoleCTime:{8} RoleLevelMTime:{9}",
				iRoleId, iRoleName, iRoleLevel, iZoneId, iZoneName, iBalance, iVip, iPartyName, iRoleCTime, iRoleLevelMTime);
			return SdkCreateRoleInfo (
				iRoleId, iRoleName, iRoleLevel, iZoneId, iZoneName, 
				iBalance, iVip, iPartyName, iRoleCTime, iRoleLevelMTime);
		}
			
		/// <summary>
		/// 创建角色.
		/// </summary>
		/// <param name="iRoleInfo">角色信息.</param>
		public void CreateRole (SdkRoleBaseInfo iRoleInfo) {
			Info ("CreateRole()::RoleInfo:{0}", iRoleInfo.ToString());
			SdkCreateRole (iRoleInfo);
		}

		/// <summary>
		/// 更新等级信息（升级时）.
		/// </summary>
		/// <param name="iRoleInfo">角色信息.</param>
		public void UpdateRoleInfoWhenLevelup (SdkRoleBaseInfo iRoleInfo) {
			Info ("UpdateRoleInfoWhenLevelup()::RoleInfo:{0}", iRoleInfo.ToString());
			SdkUpdateRoleInfoWhenLevelUp (iRoleInfo);
		}

		/// <summary>
		/// 更新等级信息（升级时）.
		/// </summary>
		/// <param name="iRoleInfo">角色信息.</param>
		public void UpdateRoleInfoWhenEnterServer (SdkRoleBaseInfo iRoleInfo) {
			Info ("UpdateRoleInfoWhenEnterServer()::RoleInfo:{0}", iRoleInfo.ToString());
			SdkUpdateRoleInfoWhenEnterServer (iRoleInfo);
		}
			
		/// <summary>
		/// 退出.
		/// </summary>
		/// <param name="iTarget">游戏对象.</param>
		/// <param name="iOnExited">退出回调函数.</param>
		public void Exit(
			GameObject iTarget = null, 
			Action<string> iOnExited = null) {
		
			Info ("Exit()::Target:{0} Method:{1}",
				(null == iTarget) ? "null" : iTarget.name,
				null == iOnExited ? "null" : iOnExited.Method.Name);
			SdkExit (iTarget, iOnExited);
		}

#endregion

#region abstract

		/// <summary>
		/// 释放函数.
		/// </summary>
		protected abstract void SdkDispose();

		/// <summary>
		/// 初始化.
		/// </summary>
		/// <param name="iTarget">游戏对象.</param>
		/// <param name="iOnCompleted">完成回调函数.</param>
		protected abstract void SdkInit (GameObject iTarget, Action<string> iOnCompleted);

		/// <summary>
		/// 登录.
		/// </summary>
		/// <param name="iTarget">登陆启动的目标对象.</param>
		/// <param name="iLoginCompleted">登录/登出完成回调函数.</param>
		protected abstract void SdkLogin (
			GameObject iTarget, 
			Action<string> iLoginCompleted);

		/// <summary>
		/// 重登录.
		/// </summary>
		protected abstract void SdkRelogin();

		/// <summary>
		/// 登出.
		/// </summary>
		protected abstract void SdkLogout ();

		/// <summary>
		/// 添加玩家信息.
		/// </summary>
		/// <param name="iTarget">游戏对象.</param>
		/// <param name="iGameRank">游戏等级.</param>
		/// <param name="iGameRole">游戏角色.</param>
		/// <param name="iGameArea">游戏区.</param>
		/// <param name="iGameSociaty">游戏工会.</param>
		/// <param name="iOnCompleted">完成回调函数.</param>
		protected abstract void SdkAddPlayerInfo (
			GameObject iTarget, 
			string iGameRank,
			string iGameRole,
			string iGameArea,
			string iGameSociaty,
			Action<string> iOnCompleted = null);

		/// <summary>
		/// 取得玩家信息.
		/// </summary>
		/// <param name="iTarget">游戏对象.</param>
		/// <param name="iOnCompleted">完成回调函数.</param>
		protected abstract void SdkGetPlayerInfo (
			GameObject iTarget, 
			Action<string> iOnCompleted = null);

		/// <summary>
		/// SDK解析用户信息.
		/// </summary>
		/// <returns>用户信息.</returns>
		/// <param name="iUserInfo">用户信息(Json格式数据).</param>>
		protected abstract SdkAccountBaseInfo SdkParserAccountInfo (string iUserInfo);

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
		protected abstract void SdkPay (
			GameObject iTarget, 
			int iIapItemPrice,
			string iIapItemName,
			int iIapItemCount, 
			string iOtherDiyInfo, 
			string iNotifyUrl, 
			Action<string> iOnPayCompleted);

		/// <summary>
		/// 解析支付信息.
		/// </summary>
		/// <returns>支付信息.</returns>
		/// <param name="iPayInfo">支付信息(Json格式数据).</param>
		/// <param name="iOnPaymentSucceeded">支付成功回调函数.</param>
		protected abstract SdkPaymentBaseInfo SdkParserPaymentInfo(
			string iPayInfo, 
			Action<SdkAccountBaseInfo, string> iOnPaymentSucceeded);

		/// <summary>
		/// 创建SDK角色信息.
		/// </summary>
		/// <returns>SDK角色信息.</returns>
		/// <param name="iRoleId">角色ID（必须为数字）.</param>
		/// <param name="iRoleName">角色名（不能为空，不能为null）.</param>
		/// <param name="iRoleLevel">角色等级（必须为数字，不能为0，默认1）.</param>
		/// <param name="iZoneId">游戏区ID（必须为数字，不能为0，默认为1）.</param>
		/// <param name="iZoneName">游戏区名（不能为空，不能为null）.</param>
		/// <param name="iBalance">游戏币余额（必须为数字，默认0）.</param>
		/// <param name="iVip">VIP等级（必须为数字，默认诶1）.</param>
		/// <param name="iPartyName">当前所属帮派（不能为空，不能为null，默认：无帮派）.</param>
		/// <param name="iRoleCTime">角色创建时间（单位：秒）.</param>
		/// <param name="iRoleLevelMTime">角色等级变化时间（单位：秒）.</param>
		public abstract SdkRoleBaseInfo SdkCreateRoleInfo (
			string iRoleId, string iRoleName, string iRoleLevel, string iZoneId, string iZoneName,
			string iBalance, string iVip, string iPartyName, string iRoleCTime, string iRoleLevelMTime);

		/// <summary>
		/// 创建角色.
		/// </summary>
		/// <param name="iRoleInfo">角色信息.</param>
		public abstract void SdkCreateRole (SdkRoleBaseInfo iRoleInfo);

		/// <summary>
		/// 更新等级信息（升级时）.
		/// </summary>
		/// <param name="iRoleInfo">角色信息.</param>
		public abstract void SdkUpdateRoleInfoWhenLevelUp (SdkRoleBaseInfo iRoleInfo);

		/// <summary>
		/// 更新等级信息（升级时）.
		/// </summary>
		/// <param name="iRoleInfo">角色信息.</param>
		public abstract void SdkUpdateRoleInfoWhenEnterServer (SdkRoleBaseInfo iRoleInfo);

		/// <summary>
		/// 退出.
		/// </summary>
		/// <param name="iTarget">游戏对象.</param>
		/// <param name="iOnExited">退出回调函数.</param>
		public abstract void SdkExit(
			GameObject iTarget = null, 
			Action<string> iOnExited = null);

#endregion

	}
}
