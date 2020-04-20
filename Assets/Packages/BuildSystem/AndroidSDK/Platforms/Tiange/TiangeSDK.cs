using System;
using Packages.BuildSystem.AndroidSDK.Options.OneSDK;
using Packages.Common.Base;
using Packages.Settings;
using Packages.Utils;
using UnityEngine;
using UnityEngine.Serialization;

#if UNITY_ANDROID

namespace Packages.BuildSystem.AndroidSDK.Platforms.Tiange {
	
	/// <summary>
	/// 天鸽账号信息.
	/// </summary>
	[Serializable]
	public class TiangeAccountInfo : SdkAccountBaseInfo {

		/// <summary>
		/// 用户ID.
		/// </summary>
		[FormerlySerializedAs("UserInfo")] 
		public JsonDataBase userInfo;

		/// <summary>
		/// 清空.
		/// </summary>
		public override void Clear() {
			base.Clear ();

			userInfo?.Clear ();
		}
	}
	
	/// <summary>
	/// 天鸽支付信息.
	/// </summary>
	[Serializable]
	public class TiangePaymentInfo : SdkPaymentBaseInfo {
		
	}

	/// <summary>
	/// 天鸽SDK设定.
	/// </summary>
	public sealed class TiangeSDK : AndroidSdkBase {

		/// <summary>
		/// 释放函数.
		/// </summary>
		protected override void SdkDispose() {
			LoginCheckBaseUrl = null;
			LoginCheckCallback = null;
			LoginCheckSucceeded = null;
			LoginCheckFailed = null;

			// 易接SDK释放
			OneSdkLibs.Instance.Dispose ();
		}
			
		/// <summary>
		/// 设定状态更新回调函数.
		/// </summary>
		/// <param name="iUpdateStatus">I update status.</param>
		public override void SetUpdateStatusCallback(Action<SdkStatus> iUpdateStatus) {
			base.SetUpdateStatusCallback (iUpdateStatus);
			OneSdkLibs.Instance.SetUpdateStatusCallback (iUpdateStatus);
		}

		/// <summary>
		/// 初始化.
		/// </summary>
		/// <param name="iTarget">登陆启动的目标对象.</param>
		/// <param name="iOnCompleted">完成回调函数.</param>
		protected override void SdkInit (GameObject iTarget, Action<string> iOnCompleted) {
			Info ("SdkInit()");
			iOnCompleted?.Invoke (((int)SdkStatus.OK).ToString());
		}

		/// <summary>
		/// 登录.
		/// </summary>
		/// <param name="iTarget">登陆启动的目标对象.</param>
		/// <param name="iOnCompleted">完成回调函数.</param>
		protected override void SdkLogin (
			GameObject iTarget, 
			Action<string> iOnCompleted) {
			Info ("SdkLogin()");

			// 接入易接SDK的场合
			if (SysSettings.GetInstance ().data.Options.IsOptionValid (SDKOptions.OneSDK)) {
				OneSdkLibs.Instance.Login (
					iTarget, iOnCompleted,
					LoginCheckBaseUrl, LoginCheckCallback,
					OnLoginCheckSucceeded, OnLoginCheckFailed,
					AutoReloginMaxCount, AutoReloginCallback);
			} else {
				Error("SDKLogin():There is invalid options settings in sdk settings!!!");
			}
		}
			
		/// <summary>
		/// 重登录.
		/// </summary>
		protected override void SdkRelogin() {
			Info ("SdkRelogin()");

			// 接入易接SDK的场合
			if (SysSettings.GetInstance ().data.Options.IsOptionValid (SDKOptions.OneSDK)) {
				OneSdkLibs.Instance.ReLogin ();
			} else {
				Error("SdkRelogin():There is invalid options settings in sdk settings!!!");
			}
		}

		/// <summary>
		/// 登出.
		/// </summary>
		protected override void SdkLogout() {
			Info ("SdkLogout()");

			// 接入易接SDK的场合
			if (SysSettings.GetInstance ().data.Options.IsOptionValid (SDKOptions.OneSDK)) {
				OneSdkLibs.Instance.Logout ();
			} else {
				Error("SDKLogout():There is invalid options settings in sdk settings!!!");
			}

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
		protected override void SdkAddPlayerInfo(
			GameObject iTarget, 
			string iGameRank,
			string iGameRole,
			string iGameArea,
			string iGameSociaty,
			Action<string> iOnCompleted = null) {
			Info ("SdkAddPlayerInfo()");

			// 天鸽不添加角色信息
			iOnCompleted?.Invoke (null);
		}

		/// <summary>
		/// 取得玩家信息.
		/// </summary>
		/// <param name="iTarget">游戏对象.</param>
		/// <param name="iOnCompleted">完成回调函数.</param>
		protected override void SdkGetPlayerInfo (
			GameObject iTarget, Action<string> iOnCompleted = null)
		{
			Info ("SdkGetPlayerInfo()");
			iOnCompleted?.Invoke (null);
		}
			
		/// <summary>
		/// SDK解析用户信息.
		/// </summary>
		/// <returns>用户信息.</returns>
		/// <param name="iUserInfo">用户信息(Json格式数据).</param>>
		protected override SdkAccountBaseInfo SdkParserAccountInfo (string iUserInfo) {
			Info ("SdkParserAccountInfo()");
			// 接入易接SDK的场合
			return SysSettings.GetInstance ().data.Options.IsOptionValid (SDKOptions.OneSDK) ? 
				OneSdkLibs.Instance.ParserLoginResponseInfo (iUserInfo) : 
				JsonUtility.FromJson<TiangeAccountInfo> (iUserInfo);
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
		protected override void SdkPay (
			GameObject iTarget, 
			int iIapItemPrice,
			string iIapItemName,
			int iIapItemCount, 
			string iOtherDiyInfo, 
			string iNotifyUrl, 
			Action<string> iOnPayCompleted) {
			Info ("SdkPay()::Price:{0} Name:{1} Count:{2} OtherDIYInfo:{3} NotifyUrl:{4}/ Target:{5} Callback:{6}",
				iIapItemPrice, iIapItemName, iIapItemCount, iOtherDiyInfo, iNotifyUrl,
				iTarget.name, iOnPayCompleted.Method.Name);

			// 接入易接SDK的场合
			if (SysSettings.GetInstance ().data.Options.IsOptionValid (SDKOptions.OneSDK)) {
				OneSdkLibs.Instance.Pay (
					iTarget, iIapItemPrice, iIapItemName, iIapItemCount,
					iOtherDiyInfo, iNotifyUrl, iOnPayCompleted);
			} else {
				Error("SDKLogout():There is invalid options settings in sdk settings!!!");
			}
		}

		/// <summary>
		/// 解析支付信息.
		/// </summary>
		/// <returns>支付信息.</returns>
		/// <param name="iPayInfo">支付信息(Json格式数据).</param>
		/// <param name="iOnPaymentSucceeded">支付成功回调函数.</param>
		protected override SdkPaymentBaseInfo SdkParserPaymentInfo(
			string iPayInfo, 
			Action<SdkAccountBaseInfo, string> iOnPaymentSucceeded) {
			Info ("SdkParserPaymentInfo()");

#if IAP_UI_TEST
			iOnPaymentSucceeded(null, null);
			return null;
#else
			if (null == iOnPaymentSucceeded) {
				Warning ("SDKParserPaymentInfo()::OnPaymentSucceeded is null!!!");
			}

			// 接入易接SDK的场合
			return SysSettings.GetInstance ().data.Options.IsOptionValid (SDKOptions.OneSDK) ? 
				OneSdkLibs.Instance.ParserPayResponseInfo (iPayInfo, iOnPaymentSucceeded) : 
				UtilsJson<TiangePaymentInfo>.ConvertFromJsonString(iPayInfo);
#endif
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
		public override SdkRoleBaseInfo SdkCreateRoleInfo (
			string iRoleId, string iRoleName, string iRoleLevel, string iZoneId, string iZoneName,
			string iBalance, string iVip, string iPartyName, string iRoleCTime, string iRoleLevelMTime) {
			// 接入易接SDK的场合
			if (true != SysSettings.GetInstance().data.Options.IsOptionValid(SDKOptions.OneSDK)) return null;
			var roleIfo = new OneSdkRoleInfo ();
			roleIfo.Reset ();

			roleIfo.id = iRoleId;
			roleIfo.name = iRoleName;
			roleIfo.level = iRoleLevel;
			roleIfo.zoneId = iZoneId;
			roleIfo.zoneName = iZoneName;
			roleIfo.balance = iBalance;
//				_roleIfo.Vip = iVip;
//				_roleIfo.PartyName = iPartyName;
			roleIfo.cTime = iRoleCTime;
			roleIfo.roleLevelMTime = iRoleLevelMTime;
			return roleIfo;
		}

		/// <summary>
		/// 创建角色信息.
		/// </summary>
		/// <returns>角色信息.</returns>
		public static SdkRoleBaseInfo CreateRoleInfo()
		{
			return true != SysSettings.GetInstance().data.Options.IsOptionValid(SDKOptions.OneSDK) ? 
				null : new OneSdkRoleInfo ();
		}
			
		/// <summary>
		/// 创建角色.
		/// </summary>
		/// <param name="iRoleInfo">角色信息.</param>
		public override void SdkCreateRole (SdkRoleBaseInfo iRoleInfo) {
			// 接入易接SDK的场合
			if (SysSettings.GetInstance ().data.Options.IsOptionValid (SDKOptions.OneSDK)) {
				 OneSdkLibs.Instance.CreateRole ((OneSdkRoleInfo)iRoleInfo);
			}
		}

		/// <summary>
		/// 更新等级信息（升级时）.
		/// </summary>
		/// <param name="iRoleInfo">角色信息.</param>
		public override void SdkUpdateRoleInfoWhenLevelUp (SdkRoleBaseInfo iRoleInfo) {
			// 接入易接SDK的场合
			if (SysSettings.GetInstance ().data.Options.IsOptionValid (SDKOptions.OneSDK)) {
				 OneSdkLibs.Instance.UpdateRoleInfoWhenLevelUp ((OneSdkRoleInfo)iRoleInfo);
			}
		}

		/// <summary>
		/// 更新等级信息（升级时）.
		/// </summary>
		/// <param name="iRoleInfo">角色信息.</param>
		public override void SdkUpdateRoleInfoWhenEnterServer (SdkRoleBaseInfo iRoleInfo) {
			// 接入易接SDK的场合
			if (SysSettings.GetInstance ().data.Options.IsOptionValid (SDKOptions.OneSDK)) {
				 OneSdkLibs.Instance.UpdateRoleInfoWhenEnterServer ((OneSdkRoleInfo)iRoleInfo);
			}
		}

		/// <summary>
		/// 退出.
		/// </summary>
		/// <param name="iTarget">游戏对象.</param>
		/// <param name="iOnExited">退出回调函数.</param>
		public override void SdkExit(
			GameObject iTarget = null, 
			Action<string> iOnExited = null) {
			// 接入易接SDK的场合
			if (SysSettings.GetInstance ().data.Options.IsOptionValid (SDKOptions.OneSDK)) {
				OneSdkLibs.Instance.Exit (iTarget, iOnExited);
			}
		}
	}
}

#endif

