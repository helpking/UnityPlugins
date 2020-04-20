using System;
using System.Collections.Generic;
using Packages.Utils;

namespace Packages.Common.Base {

	/// <summary>
	/// 检测模式.
	/// </summary>
	public enum CheckMode {
		/// <summary>
		/// Unity自带模式(hash128).
		/// </summary>
		Unity3dHash128,
		/// <summary>
		/// 自定义模式(Md5).
		/// </summary>
		CustomMd5
	}

	/// <summary>
	/// Json数据信息.
	/// </summary>
	[Serializable]
	public class JsonDataBase : ClassExtension {
		
		/// <summary>
		/// 清空.
		/// </summary>
		public virtual void Clear() {}

		/// <summary>
		/// 初始化.
		/// </summary>
		public virtual void Init() {}

		/// <summary>
		/// 重置.
		/// </summary>
		public virtual void Reset() {
			// 清空
			Clear ();
			// 初始化
			Init ();
		}

		/// <summary>
		/// 当前对象的字符串化文字
		/// </summary>
		/// <returns>字符串化文字</returns>
		public override string ToString () {
			return UtilsJson<JsonDataBase>.ConvertToJsonString(this);
		}
	}
	
	/// <summary>
	/// Json数据信息.
	/// </summary>
	[Serializable]
	public class JsonDataBase<T> : JsonDataBase
		where T : JsonDataBase, new() {

		/// <summary>
		/// 当前对象的字符串化文字
		/// </summary>
		/// <returns>字符串化文字</returns>
		public override string ToString () {
			return UtilsJson<JsonDataBase<T>>.ConvertToJsonString(this);
		}
	}
															 
	/// <summary>
	/// Json列表数据信息.
	/// </summary>
	public class JsonListDataBase<T> : JsonDataBase
		where T : JsonDataBase, new() {

		/// <summary>
		/// 列表
		/// </summary>
		public readonly List<T> List = new List<T>();

		/// <summary>
		/// 清空.
		/// </summary>
		public override void Clear() {
			base.Clear();

			foreach(T loop in List) {
				loop.Clear();
			}
			List.Clear();
		}

		/// <summary>
		/// 当前对象的字符串化文字
		/// </summary>
		/// <returns>字符串化文字</returns>
		public override string ToString ()
		{
			return UtilsJson<JsonListDataBase<T>>.ConvertToJsonString(this);
		}
	}

	/// <summary>
	/// Json选项数据信息.
	/// </summary>
	public class OptionsDataBase<T1, T2> : JsonDataBase
		where T1 : JsonDataBase , new() 
		where T2 : OptionsBaseData , new() {

		/// <summary>
		/// 一般数据.
		/// </summary>
		public T1 General = new T1();

		/// <summary>
		/// 选项数据.
		/// </summary>
		public T2 Options = new T2();

		/// <summary>
		/// 清空.
		/// </summary>
		public override void Clear() {
			base.Clear ();

			General.Clear ();
			Options.Clear ();
		}
		
		/// <summary>
		/// 当前对象的字符串化文字
		/// </summary>
		/// <returns>字符串化文字</returns>
		public override string ToString ()
		{
			return UtilsJson<OptionsDataBase<T1, T2>>.ConvertToJsonString(this);
		}
	}
}
