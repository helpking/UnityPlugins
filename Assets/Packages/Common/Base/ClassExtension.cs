using UnityEngine;
using Packages.Logs;

namespace Packages.Common.Base {
	
	/// <summary>
	/// 类扩展.
	/// </summary>
	public class ClassExtension {

		/// <summary>
		/// 类名.
		/// </summary>
		private string _className;
		protected string ClassName {
			get { 
				if(false == string.IsNullOrEmpty(_className)) {
					return _className;
				}
				_className = GetType().Name;
				return _className;
			}
		}

		/// <summary>
		/// 测试日志.
		/// </summary>
		/// <param name="iFormat">格式.</param>
		/// <param name="iArgs">参数.</param>
		public void Test(string iFormat, params object[] iArgs)
		{
			var log = string.Format(iFormat, iArgs);
			Loger.Test ($"{ClassName} {log}");
		}

		/// <summary>
		/// 信息日志(默认：运行日志).
		/// </summary>
		/// <param name="iFormat">格式.</param>
		/// <param name="iArgs">参数.</param>
		public void Info(string iFormat, params object[] iArgs) {
			var log = string.Format(iFormat, iArgs);
			Loger.Info ($"{ClassName} {log}");
		}

		/// <summary>
		/// 警告日志.
		/// </summary>
		/// <param name="iFormat">格式.</param>
		/// <param name="iArgs">参数.</param>
		public void Warning(string iFormat, params object[] iArgs) {
			var log = string.Format(iFormat, iArgs);
			Loger.Warning ($"{ClassName} {log}");
		}

		/// <summary>
		/// 信息:逻辑(LI).
		/// </summary>
		/// <param name="iFormat">格式.</param>
		/// <param name="iArgs">参数.</param>
		public void LInfo(string iFormat, params object[] iArgs) {
			var log = string.Format(iFormat, iArgs);
			Loger.LInfo ($"{ClassName} {log}");
		}

		/// <summary>
		/// 错误日志.
		/// </summary>
		/// <param name="iFormat">格式.</param>
		/// <param name="iArgs">参数.</param>
		public void Error(string iFormat, params object[] iArgs) {
			var log = string.Format(iFormat, iArgs);
			Loger.Error ($"{ClassName} {log}");
		}

		/// <summary>
		/// 致命日志.
		/// </summary>
		/// <param name="iFormat">格式.</param>
		/// <param name="iArgs">参数.</param>
		public void Fatal(string iFormat, params object[] iArgs) {
			var log = string.Format(iFormat, iArgs);
			Loger.Fatal ($"{ClassName} {log}");
		}
	}

	/// <summary>
	/// 脚本类扩展.
	/// </summary>
	public class UObjectExtension : Object {

		/// <summary>
		/// 类名.
		/// </summary>
		private string _className;

		private string ClassName {
			get { 
				if(false == string.IsNullOrEmpty(_className)) {
					return _className;
				}
				_className = GetType().Name;
				return _className;
			}
		}

		/// <summary>
		/// 测试日志.
		/// </summary>
		/// <param name="iFormat">格式.</param>
		/// <param name="iArgs">参数.</param>
		public void Test(string iFormat, params object[] iArgs)
		{
			var log = string.Format(iFormat, iArgs);
			Loger.Test ($"{ClassName} {log}");
		}

		/// <summary>
		/// 信息日志(默认：运行日志).
		/// </summary>
		/// <param name="iFormat">格式.</param>
		/// <param name="iArgs">参数.</param>
		public void Info(string iFormat, params object[] iArgs) {
			var log = string.Format(iFormat, iArgs);
			Loger.Info ($"{ClassName} {log}");
		}

		/// <summary>
		/// 警告日志.
		/// </summary>
		/// <param name="iFormat">格式.</param>
		/// <param name="iArgs">参数.</param>
		public void Warning(string iFormat, params object[] iArgs) {
			var log = string.Format(iFormat, iArgs);
			Loger.Warning ($"{ClassName} {log}");
		}

		/// <summary>
		/// 信息:逻辑(LI).
		/// </summary>
		/// <param name="iFormat">格式.</param>
		/// <param name="iArgs">参数.</param>
		public void LInfo(string iFormat, params object[] iArgs) {
			var log = string.Format(iFormat, iArgs);
			Loger.LInfo ($"{ClassName} {log}");
		}

		/// <summary>
		/// 错误日志.
		/// </summary>
		/// <param name="iFormat">格式.</param>
		/// <param name="iArgs">参数.</param>
		public void Error(string iFormat, params object[] iArgs) {
			var log = string.Format(iFormat, iArgs);
			Loger.Error ($"{ClassName} {log}");
		}

		/// <summary>
		/// 致命日志.
		/// </summary>
		/// <param name="iFormat">格式.</param>
		/// <param name="iArgs">参数.</param>
		public void Fatal(string iFormat, params object[] iArgs) {
			var log = string.Format(iFormat, iArgs);
			Loger.Fatal ($"{ClassName} {log}");
		}
	}

	/// <summary>
	/// 脚本类扩展.
	/// </summary>
	public class SObjectExtension : object {

		/// <summary>
		/// 类名.
		/// </summary>
		private string _className;

		private string ClassName {
			get { 
				if(false == string.IsNullOrEmpty(_className)) {
					return _className;
				}
				_className = GetType().Name;
				return _className;
			}
		}

		/// <summary>
		/// 测试日志.
		/// </summary>
		/// <param name="iFormat">格式.</param>
		/// <param name="iArgs">参数.</param>
		public void Test(string iFormat, params object[] iArgs)
		{
			var log = string.Format(iFormat, iArgs);
			Loger.Test ($"{ClassName} {log}");
		}

		/// <summary>
		/// 信息日志(默认：运行日志).
		/// </summary>
		/// <param name="iFormat">格式.</param>
		/// <param name="iArgs">参数.</param>
		public void Info(string iFormat, params object[] iArgs) {
			var log = string.Format(iFormat, iArgs);
			Loger.Info ($"{ClassName} {log}");
		}

		/// <summary>
		/// 警告日志.
		/// </summary>
		/// <param name="iFormat">格式.</param>
		/// <param name="iArgs">参数.</param>
		public void Warning(string iFormat, params object[] iArgs) {
			var log = string.Format(iFormat, iArgs);
			Loger.Warning ($"{ClassName} {log}");
		}

		/// <summary>
		/// 信息:逻辑(LI).
		/// </summary>
		/// <param name="iFormat">格式.</param>
		/// <param name="iArgs">参数.</param>
		public void LInfo(string iFormat, params object[] iArgs) {
			var log = string.Format(iFormat, iArgs);
			Loger.LInfo ($"{ClassName} {log}");
		}

		/// <summary>
		/// 错误日志.
		/// </summary>
		/// <param name="iFormat">格式.</param>
		/// <param name="iArgs">参数.</param>
		public void Error(string iFormat, params object[] iArgs) {
			var log = string.Format(iFormat, iArgs);
			Loger.Error ($"{ClassName} {log}");
		}

		/// <summary>
		/// 致命日志.
		/// </summary>
		/// <param name="iFormat">格式.</param>
		/// <param name="iArgs">参数.</param>
		public void Fatal(string iFormat, params object[] iArgs) {
			var log = string.Format(iFormat, iArgs);
			Loger.Fatal ($"{ClassName} {log}");
		}
	}

}
