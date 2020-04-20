using Packages.Common.Base;
using UnityEngine;

namespace Packages.Common.NetWork {

	/// <summary>
	/// Json转化类.
	/// </summary>
	public class JsonConvert<T> where T : JsonDataBase, new() {

		/// <summary>
		/// 将Json字符串转换成对象.
		/// </summary>
		/// <returns>对象.</returns>
		/// <param name="iJsonStr">Json字符串.</param>
		public static T ConvertFromJsonString(string iJsonStr)
		{
			return string.IsNullOrEmpty (iJsonStr) ? default : JsonUtility.FromJson<T>(iJsonStr);
		}

		/// <summary>
		/// 将对象转换成Json字符串.
		/// </summary>
		/// <returns>Json字符串.</returns>
		/// <param name="iObject">对象.</param>
		public static string ConvertToJsonString(T iObject)
		{
			return default(T) == iObject ? null : JsonUtility.ToJson(iObject);
		} 
	}

	/// <summary>
	/// 请求数据基类.
	/// </summary>
	public class RequestDataBase : JsonDataBase<RequestDataBase> {

		/// <summary>
		/// 构造函数.
		/// </summary>
		public RequestDataBase () {}

		/// <summary>
		/// 初始化.
		/// </summary>
		public override void Init() {
			base.Init ();
		}

		/// <summary>
		/// 重置.
		/// </summary>
		public override void Reset() {
			base.Reset ();
		}

		/// <summary>
		/// 清空.
		/// </summary>
		public override void Clear() {
			base.Clear ();
		}

		public override string ToString() {
			return base.ToString();
		}

	}

	/// <summary>
	/// 回复数据基类.
	/// </summary>
	public class ResponseDataBase : JsonDataBase<ResponseDataBase> {

		/// <summary>
		/// 状态.
		/// </summary>
		/// <value>The status.</value>
		public int Status;

		/// <summary>
		/// 错误详细Code.
		/// </summary>
		public int ErrorDetailCode;

		/// <summary>
		/// 错误详细.
		/// </summary>
		public string ErrorDetail;

		/// <summary>
		/// 构造函数.
		/// </summary>
		public ResponseDataBase () {}

		/// <summary>
		/// 初始化.
		/// </summary>
		public override void Init() {
			base.Init ();

			Status = 200;
			ErrorDetailCode = -1;
			ErrorDetail = null;
		}

		/// <summary>
		/// 重置.
		/// </summary>
		public override void Reset() {
			base.Reset ();
		}

		/// <summary>
		/// 清空.
		/// </summary>
		public override void Clear() {
			base.Clear ();

			Status = 200;
		}

		/// <summary>
		/// 回复是否OK.
		/// </summary>
		/// <value><c>true</c> OK; NG, <c>false</c>.</value>
		public bool isOK => 200 == Status;

	}

	/// <summary>
	/// 回复事件委托.
	/// </summary>
	public delegate void OnResponsedDelegate(ResponseDataBase iResponseData);

	/// <summary>
	/// 请求基类.
	/// </summary>
	public abstract class RequestBase<T1, T2> : ClassExtension 
		where T1 : RequestDataBase, new() 
		where T2 : ResponseDataBase, new() {

		/// <summary>
		/// 请求数据.
		/// </summary>
		protected T1 RequestData = new T1();

		/// <summary>
		/// 回复数据.
		/// </summary>
		protected T2 ResponseData = new T2();

		/// <summary>
		/// 回复回调函数.
		/// </summary>
		protected OnResponsedDelegate OnResponsedCallback = null;

		/// <summary>
		/// 发送请求.
		/// </summary>
		public void Send() {
			if (null == RequestData) {
				return;
			}
			Info ("Send():Data::{0}", RequestData.ToString());
			SendRequest (RequestData, OnResponsed);
		}

		/// <summary>
		/// 回复.
		/// </summary>
		/// <param name="iData">数据.</param>
		public virtual void OnResponsed(ResponseDataBase iData) {
			if (null == iData) {
				Error ("OnResponsed():The data of response is null!!");
				return;
			}
			var _tmp = iData.ToString ();
			Info ("OnResponsed():{0}", string.IsNullOrEmpty(_tmp) ? "-" : _tmp);
			OnResponsedCallback?.Invoke (iData);
		}

		/// <summary>
		/// 发送请求.
		/// </summary>
		/// <param name="iData">请求数据.</param>
		/// <param name="iOnResponsed">回复回调委托</param>
		protected abstract void SendRequest(T1 iData, OnResponsedDelegate iOnResponsed);

	}

}
