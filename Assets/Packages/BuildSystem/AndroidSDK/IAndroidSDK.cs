using System;
using UnityEngine;
using UnityEngine.Serialization;
using Packages.Common.Base;

namespace Packages.BuildSystem.AndroidSDK {

	/// <summary>
	/// SDK登录状态.
	/// </summary>
	public enum SdkStatus {
		/// <summary>
		/// 无效.
		/// </summary>
		Invalid = -1,
		/// <summary>
		/// 正常.
		/// </summary>
		OK = 0,
		/// <summary>
		/// SDK初始化中.
		/// </summary>
		SDKIniting = 100,
		/// <summary>
		/// SDK初始化成功.
		/// </summary>
		SDKInitSucceeded = 101,
		/// <summary>
		/// SDK初始化成功.
		/// </summary>
		SDKInitFailed = 109,
		/// <summary>
		/// 登录中.
		/// </summary>
		Logining = 200,
		/// <summary>
		/// 登录完成.
		/// </summary>
		LoginCompleted = 201,
		/// <summary>
		/// 登录失败.
		/// </summary>
		LoginFailed = 209,
		/// <summary>
		/// 登录校验中.
		/// </summary>
		LoginChecking = 210,
		/// <summary>
		/// 登录校验成功.
		/// </summary>
		LoginCheckSucceeded = 211,
		/// <summary>
		/// 登录校验失败.
		/// </summary>
		LoginCheckFailed = 219,
		/// <summary>
		/// 自动重登.
		/// </summary>
		AutoReLogining = 220,
		/// <summary>
		/// 重登失败(多次重登录，依然登录失败).
		/// </summary>
		AutoReLoginFailed = 229,
		/// <summary>
		/// 登出.
		/// </summary>
		Logouted = 300,
		/// <summary>
		/// 交易中.
		/// </summary>
		Purchasing = 900,
		/// <summary>
		/// 交易完成.
		/// </summary>
		PurchaseCompleted = 901,
		/// <summary>
		/// 交易订单中.
		/// </summary>
		PurchaseOrdered = 902,
		/// <summary>
		/// 交易成功.
		/// </summary>
		PurchaseSucceeded = 903,
		/// <summary>
		/// 交易失败
		/// </summary>
		PurchaseFailed = 909,
		/// <summary>
		/// 未知.
		/// </summary>
		Unknown = 999
	}

	/// <summary>
	/// SDK 账号信息基类.
	/// </summary>
	[Serializable]
	public class SdkAccountBaseInfo : JsonDataBase<SdkAccountBaseInfo> {

		/// <summary>
		/// 状态.
		/// </summary>
		[FormerlySerializedAs("Status")] 
		public SdkStatus status;

		/// <summary>
		/// 详细状态（各个平台自己设置的详细状态信息）.
		/// </summary>
		[FormerlySerializedAs("DetailStatus")] 
		public string detailStatus;

		/// <summary>
		/// 清空.
		/// </summary>
		public override void Clear () {
			base.Clear ();
			status = SdkStatus.Invalid;
			detailStatus = null;
		}
	}

	/// <summary>
	/// SDK 支付信息基类.
	/// </summary>
	[Serializable]
	public class SdkPaymentBaseInfo : JsonDataBase<SdkPaymentBaseInfo> {

		/// <summary>
		/// 是否OK（true:支付成功; false:支付失败;
		/// </summary>
		[FormerlySerializedAs("Succeeded")] 
		public bool succeeded;

		/// <summary>
		/// 状态.
		/// </summary>
		[FormerlySerializedAs("Status")] 
		public SdkStatus status;

		/// <summary>
		/// 清空.
		/// </summary>
		public override void Clear () {
			base.Clear ();
			succeeded = false;
			status = SdkStatus.Invalid;
		}
	}

	/// <summary>
	/// SDK角色信息基类.
	/// </summary>
	[Serializable]
	public class SdkRoleBaseInfo : JsonDataBase<SdkRoleBaseInfo> {

		/// <summary>
		/// 角色ID(必须为数字).
		/// </summary>
		[FormerlySerializedAs("ID")] 
		public string id;

		/// <summary>
		/// 角色名（不能为空，不能为null）.
		/// </summary>
		[FormerlySerializedAs("Name")] 
		public string name;

		/// <summary>
		/// 角色等级（必须为数字，不能为空，不能为0，默认1）.
		/// </summary>
		[FormerlySerializedAs("Level")] 
		public string level;

		/// <summary>
		/// 游戏区ID（必须为数字，且不能为0，默认1）.
		/// </summary>
		[FormerlySerializedAs("ZoneID")] 
		public string zoneId;

		/// <summary>
		/// 游戏区名（不能为空，不能为null）.
		/// </summary>
		[FormerlySerializedAs("ZoneName")] 
		public string zoneName;

		/// <summary>
		/// 游戏币余额（若无，传入0）.
		/// </summary>
		[FormerlySerializedAs("Balance")] 
		public string balance;

		/// <summary>
		/// 角色创建时间（单位：秒）.
		/// </summary>
		[FormerlySerializedAs("CTime")] 
		public string cTime;

		/// <summary>
		/// 清空.
		/// </summary>
		public override void Clear () {
			base.Clear ();

			id = null;
			name = null;
			level = "1";
			zoneId = null;
			zoneName = null;
			balance = "0";
			cTime = null;
		}
	}

	/// <summary>
	/// AndroidSDK接口.
	/// </summary>
	public interface IAndroidSdk {

		/// <summary>
		/// 显示Debug信息.
		/// </summary>
		/// <param name="iDebugInfo">Debug信息.</param>
		void ShowDebugInfo (string iDebugInfo);

		/// <summary>
		/// 初始化.
		/// </summary>
		/// <param name="iTarget">游戏对象.</param>
		/// <param name="iOnCompleted">完成回调函数.</param>
		void Init (GameObject iTarget, Action<string> iOnCompleted);

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
		void Login (
			GameObject iTarget, 
			Action<string> iLoginCompleted,
			string iLoginCheckBaseUrl,
			Action<string, Action<string>, Action<string>> iLoginCheckCallBack,
			Action<string> iLoginCheckSucceeded,
			Action<string> iLoginCheckFailed,
			int iAutoReloginMaxCount,
			Action<float> iAutoReloginCallback);

		/// <summary>
		/// 解析用户信息.
		/// </summary>
		/// <returns>用户信息.</returns>
		/// <param name="iUserInfo">用户信息(Json格式数据).</param>
		SdkAccountBaseInfo ParserAccountInfo(string iUserInfo);

		/// <summary>
		/// 重登录.
		/// </summary>
		void Relogin();
		
		/// <summary>
		/// 登出.
		/// </summary>
		void Logout();

		/// <summary>
		/// 添加玩家信息.
		/// </summary>
		/// <param name="iTarget">游戏对象.</param>
		/// <param name="iGameRank">游戏等级.</param>
		/// <param name="iGameRole">游戏角色.</param>
		/// <param name="iGameArea">游戏区.</param>
		/// <param name="iGameSociaty">游戏工会.</param>
		/// <param name="iOnCompleted">完成回调函数.</param>
		void AddPlayerInfo (
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
		void GetPlayerInfo (
			GameObject iTarget, 
			Action<string> iOnCompleted = null);

		/// <summary>
		/// 创建SDK角色信息.
		/// </summary>
		/// <returns>SDK角色信息.</returns>
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
		SdkRoleBaseInfo CreateRoleInfo (
			string iRoleId, string iRoleName, string iRoleLevel, string iZoneID, string iZoneName,
			string iBalance, string iVip, string iPartyName, string iRoleCTime, string iRoleLevelMTime);

		/// <summary>
		/// 创建角色.
		/// </summary>
		/// <param name="iRoleInfo">角色信息.</param>
		void CreateRole (SdkRoleBaseInfo iRoleInfo);

		/// <summary>
		/// 更新等级信息（升级时）.
		/// </summary>
		/// <param name="iRoleInfo">角色信息.</param>
		void UpdateRoleInfoWhenLevelup (SdkRoleBaseInfo iRoleInfo);

		/// <summary>
		/// 更新等级信息（升级时）.
		/// </summary>
		/// <param name="iRoleInfo">角色信息.</param>
		void UpdateRoleInfoWhenEnterServer (SdkRoleBaseInfo iRoleInfo);

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
		void Pay (
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
		/// <param name="iPayInfo">支付信息(Json格式数据)</param>
		/// <param name="iOnPaymentSucceeded">支付成功回调函数</param>
		SdkPaymentBaseInfo ParserPaymentInfo(
			string iPayInfo, 
			Action<SdkAccountBaseInfo, string> iOnPaymentSucceeded);

		/// <summary>
		/// 退出.
		/// </summary>
		/// <param name="iTarget">游戏对象.</param>
		/// <param name="iOnExited">退出回调函数.</param>
		void Exit(
			GameObject iTarget = null, 
			Action<string> iOnExited = null);
	}
}
