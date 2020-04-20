using System;
using Packages.Common;
using Packages.Settings;
using UnityEngine;
using UnityEngine.Serialization;

#if UNITY_ANDROID

namespace Packages.BuildSystem.AndroidSDK.Platforms.Huawei {

	/// <summary>
	/// 华为SDK状态.
	/// </summary>
	public enum HuaweiSdkResultStatus {
		/// <summary>
		/// 无效.
		/// </summary>
		Invalid = -1,
		/// <summary>
		/// OK.
		/// </summary>
		OK = 0,
		/// <summary>
		/// 签名错误.
		/// </summary>
		ErrSign = 1,
		/// <summary>
		/// 一般错误.
		/// </summary>
		ErrComm = 2,
		/// <summary>
		/// 游戏中心版本低，不支持.
		/// </summary>
		ErrUnSupport = 3,
		/// <summary>
		/// 参数错误(开发人员需要检查接口的 参数传入是否合法，参数是否为 null).
		/// </summary>
		ErrParam = 4,
		/// <summary>
		/// 网络错误.
		/// </summary>
		ErrNetWork = 5,
		/// <summary>
		/// 帐号鉴权失败(建议游戏收到此错误 码后退出游戏重新登录).
		/// </summary>
		ErrAuth = 6,
		/// <summary>
		/// 用户取消操作(比如用户取消安装游 戏中心，取消同意用户协议).
		/// </summary>
		ErrCancel = 7,
		/// <summary>
		/// 服务尚未初始化(游戏收到此错误码 后重新调用初始化接口).
		/// </summary>
		ErrNotInit = 8,
		/// <summary>
		/// 用户取消登录.
		/// </summary>
		ErrCancelLogin = 9,
		/// <summary>
		/// 无法拉起游戏中心.
		/// </summary>
		ErrBindHigame = 10,
		/// <summary>
		/// 未知类型.
		/// </summary>
		UnKnown
	}

	/// <summary>
	/// SDK账号信息.
	/// </summary>
	[Serializable]
	public class SdkHuaweiAccountInfo : SdkAccountBaseInfo
	{
		/// <summary>
		/// 玩家ID.
		/// </summary>
		[FormerlySerializedAs("PlayerId")] 
		public string playerId;

		/// <summary>
		/// 玩家等级.
		/// </summary>
		[FormerlySerializedAs("PlayerLevel")] 
		public int playerLevel;

		/// <summary>
		/// 玩家名.
		/// </summary>
		[FormerlySerializedAs("DisplayName")] 
		public string displayName;

		/// <summary>
		/// 账号切换标志位.
		/// </summary>
		[FormerlySerializedAs("IsChange")] 
		public bool isChange;
		
	}

	/// <summary>
	/// 华为SDK.
	/// </summary>
	public sealed class HuaweiSdk : AndroidSdkBase {

		/// <summary>
		/// 释放函数.
		/// </summary>
		protected override void SdkDispose() {
			LoginCheckBaseUrl = null;
			LoginCheckCallback = null;
			LoginCheckSucceeded = null;
			LoginCheckFailed = null;
		}

		/// <summary>
		/// 初始化SDK.
		/// </summary>
		/// <param name="iTarget">游戏对象.</param>
		/// <param name="iOnCompleted">完成回调函数.</param>
		protected override void SdkInit(
			GameObject iTarget, 
			Action<string> iOnCompleted) {

			if (null == iTarget) {
				Error ("SDKInit():The ProjectName is null!!!");
				return;
			}

			var jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
			var jo = jc.GetStatic<AndroidJavaObject>("currentActivity");
			jo.Call ("UToA_SDKInit", iTarget.name, iOnCompleted.Method.Name);
		}

		/// <summary>
		/// 登陆.
		/// </summary>
		/// <param name="iTarget">游戏对象.</param>
		/// <param name="iOnCompleted">完成回调函数.</param>
		protected override void SdkLogin(
			GameObject iTarget, 
			Action<string> iOnCompleted) {

			if (null == iTarget) {
				return;
			}

			var jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
			var jo = jc.GetStatic<AndroidJavaObject>("currentActivity");
			jo.Call ("UToA_Login", iTarget.name, iOnCompleted.Method.Name);
		}

		/// <summary>
		/// 重登录.
		/// </summary>
		protected override void SdkRelogin() {
			
		}

		/// <summary>
		/// 登出.
		/// </summary>
		protected override void SdkLogout() {
			var jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
			var jo = jc.GetStatic<AndroidJavaObject>("currentActivity");
			jo.Call ("UToA_Logout");
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

			if (PlatformType.None == SysSettings.GetInstance ().PlatformType) {
				Error ("AddPlayerInfo():The platformType is none in AppSettings.asset file!!!");
				return;
			}

			if (false == Application.isMobilePlatform) {
				return;
			}

			var jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
			var jo = jc.GetStatic<AndroidJavaObject>("currentActivity");
			jo.Call ("UToA_AddPlayerInfo", 
				iTarget.name, 
				iGameRank, iGameRole, iGameArea, iGameSociaty,
				iOnCompleted?.Method.Name);
		}

		/// <summary>
		/// 取得玩家信息.
		/// </summary>
		/// <param name="iTarget">游戏对象.</param>
		/// <param name="iOnCompleted">完成回调函数.</param>
		protected override void SdkGetPlayerInfo(
			GameObject iTarget, 
			Action<string> iOnCompleted = null) {

			if (PlatformType.None == SysSettings.GetInstance ().PlatformType) {
				Error ("GetPlayerInfo():The platformType is none in buildinfo.asset file!!!");
				return;
			}

			if (false == Application.isMobilePlatform) {
				return;
			}

			var jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
			var jo = jc.GetStatic<AndroidJavaObject>("currentActivity");
			jo.Call ("UToA_GetPlayerInfo", 
				iTarget.name, 
				iOnCompleted?.Method.Name);
		}

		/// <summary>
		/// SDK解析用户信息.
		/// </summary>
		/// <returns>用户信息.</returns>
		/// <param name="iUserInfo">用户信息(Json格式数据).</param>>
		protected override SdkAccountBaseInfo SdkParserAccountInfo (string iUserInfo) {
			return JsonUtility.FromJson<SdkHuaweiAccountInfo> (iUserInfo);
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
			return null;
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
			return null;
		}
			
		/// <summary>
		/// 创建角色.
		/// </summary>
		/// <param name="iRoleInfo">角色信息.</param>
		public override void SdkCreateRole (SdkRoleBaseInfo iRoleInfo) {}

		/// <summary>
		/// 更新等级信息（升级时）.
		/// </summary>
		/// <param name="iRoleInfo">角色信息.</param>
		public override void SdkUpdateRoleInfoWhenLevelUp (SdkRoleBaseInfo iRoleInfo) {}

		/// <summary>
		/// 更新等级信息（升级时）.
		/// </summary>
		/// <param name="iRoleInfo">角色信息.</param>
		public override void SdkUpdateRoleInfoWhenEnterServer (SdkRoleBaseInfo iRoleInfo) {}

		/// <summary>
		/// 退出.
		/// </summary>
		/// <param name="iTarget">游戏对象.</param>
		/// <param name="iOnExited">退出回调函数.</param>
		public override void SdkExit(
			GameObject iTarget = null, 
			Action<string> iOnExited = null) {}
	}
}

#endif