using System;
using System.Text;
using Packages.Common.Base;

namespace Packages.NetWork.Http
{
	
	/// <summary>
	/// Http助手
	/// </summary>
	public class HttpHelper : SingletonBase<HttpHelper> {
		
		/// <summary>
		/// Http Encode 
		/// </summary>
		/// <param name="iStrCode">转换字节码用的字符串</param>
		/// <returns>Encode字符串</returns>
		public string ConvertToEncode (string iStrCode)
		{ 
			var sb = new StringBuilder (); 
			var byStr = Encoding.UTF8.GetBytes (iStrCode); //默认是System.Text.Encoding.General.GetBytes(str) 
			var regKey = new System.Text.RegularExpressions.Regex ("^[A-Za-z0-9]+$"); 
			for (var _ = 0; _ < byStr.Length; _++) { 
				var strBy = Convert.ToChar (byStr [_]).ToString (); 
				if (regKey.IsMatch (strBy)) { 
					//是字母或者数字则不进行转换  
					sb.Append (strBy); 
				} else { 
					sb.Append (@"%" + Convert.ToString (byStr [_], 16)); 
				} 
			} 
			return sb.ToString (); 
		}
		
		/// <summary>
		/// 忽略Https证书相关设定（跳过证书验证）
		/// </summary>
		public static void IgnoreHttpsCertificateSettings() {

			// 解决WebClient不能通过https下载内容问题
			System.Net.ServicePointManager.ServerCertificateValidationCallback +=
				(iSender, iCertificate, iChain, iSslPolicyErrors) => true;
		}
	}
	
}
