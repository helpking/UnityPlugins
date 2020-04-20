using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using UnityEngine;
using Packages.Common.Base;
using Packages.Logs;

namespace Packages.Utils {

	/// <summary>
	/// 工具.
	/// </summary>
	public class UtilsTools : ClassExtension
	{

		/// <summary>
		/// Md5对象.
		/// </summary>
		private static MD5CryptoServiceProvider _md5;

		/// <summary>
		/// 取得文件的Md5码.
		/// </summary>
		/// <returns>文件的Md5码.</returns>
		/// <param name="iFilePath">文件路径.</param>
		public static string GetMd5ByFilePath(string iFilePath)
		{
			if (false == File.Exists(iFilePath))
			{
				Loger.Error($"UtilsTools::GetMD5ByFilePath():The file is not exist!!!(File:{iFilePath})");
				return null;
			}
			if (_md5 == null)
			{
				_md5 = new MD5CryptoServiceProvider();
			}
			byte[] hash;
			using (var fs = new FileStream(iFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				Loger.Info($"UtilsTools::GetMD5ByFilePath():File:{iFilePath}({fs.Length} Byte)");

				hash = _md5.ComputeHash(fs);
				fs.Close();
			}

			if (0 >= hash.Length) return null;
			var strMd5 = BitConverter.ToString(hash);
			strMd5 = strMd5.ToLower();
			strMd5 = strMd5.Replace("-", "");
			return strMd5;
		}

		/// <summary>
		/// 创建实例.
		/// </summary>
		/// <returns>实例.</returns>
		/// <param name="iNameSpace">命名空间.</param>
		/// <param name="iClassName">类名.</param>
		/// <param name="iParams">构造参数.</param>
		public static T CreateInstance<T>(
			string iNameSpace, string iClassName,
			params object[] iParams)
		{
			try
			{
				// 命名空间.类型名
				var fullName = string.Format("{0}.{1}", iNameSpace, iClassName);

				// 加载程序集，创建程序集里面的 命名空间.类型名 实例
				var instance = Assembly.GetExecutingAssembly().CreateInstance(
					fullName, true, BindingFlags.Default,
					null, iParams, null, null);

				// 返回
				return (T) instance;
			}
			catch
			{
				Loger.Fatal($"UtilsTools::CreateInstance():Failed!!!(NameSpace:{iNameSpace} ClassName:{iClassName})");
				// 发生异常，返回类型的默认值
				return default(T);
			}
		}

		/// <summary>
		/// 取得下载数据大小
		/// </summary>
		/// <param name="iTotalSize">数据大小（单位：KB/MB）</param>
		/// <returns>数据大小（单位：KB/MB）</returns>
		public static float GetDownloadDataSize(long iTotalSize)
		{
			float ret = iTotalSize;
			if (1024 * 1024 < iTotalSize)
			{
				ret = ConvertByteToMb(iTotalSize);
			}
			else if (1024 < iTotalSize)
			{
				ret = ConvertByteToKb(iTotalSize);
			}

			return ret;
		}

		/// <summary>
		/// 转换字节（Bytes -> KB）
		/// </summary>
		/// <param name="iByteVale">字节数</param>
		/// <returns>KB</returns>
		public static float ConvertByteToKb(long iByteVale)
		{
			var mbValue = 0.0f;
			if (0L < iByteVale)
			{
				mbValue = iByteVale / 1024.0f;
			}

			return mbValue;
		}

		/// <summary>
		/// 转换字节（Bytes -> MB）
		/// </summary>
		/// <param name="iByteVale">字节数</param>
		/// <returns>Mb</returns>
		public static float ConvertByteToMb(long iByteVale)
		{
			var mbValue = 0.0f;
			if (0L < iByteVale)
			{
				mbValue = iByteVale / (1024.0f * 1024.0f);
			}

			return mbValue;
		}

		/// <summary>
		/// 根据数据大小取得数据单位
		/// </summary>
		/// <param name="iDataSize">数据大小(单位:Byte)</param>
		/// <returns>数据单位</returns>
		public static string GetUnitByDataSize(long iDataSize)
		{
			if (1024 * 1024 < iDataSize)
			{
				return "MB";
			}

			return 1024 < iDataSize ? "KB" : "Bytes";
		}

		/// <summary>
		/// 取得文件名
		/// </summary>
		/// <param name="iFullPath">文件全路径</param>
		/// <param name="iIsIncludeExtension">是否包含文件后缀名</param>
		/// <returns>文件名</returns>
		public static string GetFileName(string iFullPath, bool iIsIncludeExtension = true)
		{
			if (File.Exists(iFullPath))
			{
				var info = new FileInfo(iFullPath);
				return iIsIncludeExtension ? info.Name : info.Name.Replace($"{info.Extension}", "");
			}

			Loger.Error($"UtilsTools::GetFileName():The File is not exist!(path:{iFullPath})");
			return null;
		}
		
		/// <summary>
		/// 取得文件名后缀名
		/// </summary>
		/// <param name="iFullPath">文件全路径</param>
		/// <returns>文件名后缀名</returns>
		public static string GetFileExtension(string iFullPath)
		{
			if (File.Exists(iFullPath))
			{
				var info = new FileInfo(iFullPath);
				return info.Extension;
			}

			Loger.Error($"UtilsTools::GetFileExtension():The File is not exist!(path:{iFullPath})");
			return null;
		}

		/// <summary>
		/// 根据文件路径取得文件所在目录
		/// </summary>
		/// <param name="iFullPath">文件全路径</param>
		/// <returns>文件所在目录</returns>
		public static string GetFileDirByFilePath(string iFullPath)
		{
			var file = new FileInfo(iFullPath);
			// 去除文件名
			var dirPath = iFullPath.Replace(file.Name, "");
			if (Directory.Exists(dirPath))
			{
				return new DirectoryInfo(dirPath).FullName;
			}

			Loger.Error($"UtilsTools::GetFileDirByFilePath():The directory of file is not exist!(path:{iFullPath})");
			return null;
		}

		/// <summary>
		/// 对象深拷贝
		/// </summary>
		/// <param name="iObj">被复制对象</param>
		/// <returns>新对象</returns>
		public static object DeepCopy(object iObj)
		{
			if (iObj == null)
			{
				return null;
			}

			object targetDeepCopyObj;
			var targetType = iObj.GetType();
			//值类型  
			if (targetType.IsValueType == true)
			{
				targetDeepCopyObj = iObj;
			}
			//引用类型   
			else
			{
				targetDeepCopyObj = Activator.CreateInstance(targetType); //创建引用对象   
				var memberCollection = iObj.GetType().GetMembers();

				foreach (var member in memberCollection)
				{
					switch (member.MemberType)
					{
						//拷贝字段
						case MemberTypes.Field:
						{
							var field = (FieldInfo) member;
							var fieldValue = field.GetValue(iObj);
							var cloneable = fieldValue as ICloneable;
							field.SetValue(targetDeepCopyObj,
								cloneable != null ? cloneable.Clone() : DeepCopy(fieldValue));

							break;
						}

						case MemberTypes.Property:
						{
							var myProperty = (PropertyInfo) member;

							var info = myProperty.GetSetMethod(false);
							if (info != null)
							{
								try
								{
									var propertyValue = myProperty.GetValue(iObj, null);
									var cloneable = propertyValue as ICloneable;
									myProperty.SetValue(targetDeepCopyObj,
										cloneable != null ? cloneable.Clone() : DeepCopy(propertyValue), null);
								}
								catch (Exception exp)
								{
									Loger.Fatal($"UtilsTools::DeepCopy():Exception:{exp.Message}\n{exp.StackTrace}");
								}
							}

							break;
						}
					}
				}
			}

			return targetDeepCopyObj;
		}

#region Directory And File

		/// <summary>
		/// 清空目录.
		/// </summary>
		/// <param name="iDir">目录.</param>
		/// <param name="iSelfDel">自身是否删除.</param>
		public static void ClearDirectory(string iDir, bool iSelfDel = true) {

			// 清空文件
			var files = Directory.GetFiles (iDir);
			if (1 <= files.Length) {
				foreach (var file in files) {
					File.Delete (file);
					Loger.Info ($"UtilsTools::ClearDirectory():Delete File:{file}");
				}
			}

			// 清空子目录
			var dirs = Directory.GetDirectories (iDir);
			if (1 <= dirs.Length) {
				foreach (var dir in dirs) {
					ClearDirectory (dir, false);
				}
			} else {
				if(iSelfDel) Directory.Delete (iDir);
				Loger.Info ($"UtilsTools::ClearDirectory():Delete Directory:{iDir}");
			}
		}

		/// <summary>
		/// 拷贝目录.
		/// </summary>
		/// <param name="iFromDir">源目录.</param>
		/// <param name="iToDir">目标目录.</param>
		public static void CopyDirectory(string iFromDir, string iToDir) 
		{ 
			var dirInfo = new DirectoryInfo (iFromDir);
			var toDir = $"{iToDir}/{dirInfo.Name}";
			if (false == Directory.Exists (toDir)) {
				Directory.CreateDirectory (toDir);
			}

			// 拷贝文件
			var allFiles = dirInfo.GetFiles();
			foreach (var file in allFiles) {
				if (file.Name.EndsWith (".meta")) {
					continue;
				}

				// 拷贝文件
				var copyToFile = $"{toDir}/{file.Name}";
				Loger.Info ($"UtilsTools::CopyDirectory():File: {file.FullName} -> {copyToFile}");

				File.Copy (file.FullName, copyToFile, true);
			}

			// 检索子文件夹
			var subDirs = dirInfo.GetDirectories();
			foreach (var subDir in subDirs) {
				var fromDir = $"{iFromDir}/{subDir.Name}";
				CopyDirectory (fromDir, toDir);
			}
		}

		/// <summary>
		/// 校验路径，若无则创建目录.
		/// </summary>
		/// <returns><c>true</c>, OK, <c>false</c> NG.</returns>
		/// <param name="iDir">目录.</param>
		public static bool CheckAndCreateDirByFullDir(string iDir) {

			// 若已经存在则返回
			if (CheckAndCreateDir (iDir)) {
				return true;
			}
			
			// 初始化目录信息
			var dirs = new Stack<string>();
			var parent = new DirectoryInfo (iDir);
			while (null != parent) {
				dirs.Push (parent.Name);
				parent = parent.Parent;
			}
			var isOk = true;
			var dir = "";
#if UNITY_EDITOR_OSX
			dir = Path.DirectorySeparatorChar.ToString();
#endif
			var index = 0;
			while (0 < dirs.Count) {
				if(0 >= index) {
					dir = dirs.Pop ();
				} else if(1 == index) {
					dir = $"{dir}{dirs.Pop()}";
				} else {
					dir = $"{dir}{Path.DirectorySeparatorChar}{dirs.Pop()}";
				}
				if (false == CheckAndCreateDir (dir)) {
					isOk = false;
					break;
				}
				index++;
			}
			return isOk;
		}

		public static bool CheckAndCreateDir(string iDir) {
			if (Directory.Exists(iDir)) return true;
			Directory.CreateDirectory (iDir);
			return Directory.Exists(iDir);
		}
			
		/// <summary>
		/// 检测匹配路径.
		/// 反向校验指定路径。直到Application.dataPath
		/// 并返回相应的相对路径
		/// </summary>
		/// <returns>检测后的路径.</returns>
		/// <param name="iTargetPath">检测并取得匹配路径.</param>
		public static string CheckMatchPath(string iTargetPath) {
			var targetPath = iTargetPath;
			var rootPath = Application.dataPath;
			var targetDir = new DirectoryInfo (targetPath);
			var rootDir = new DirectoryInfo (rootPath);

			var targetParent = targetDir.Parent;
			var checkPath = targetDir.Name;
			while (null != targetParent) {

				var nameTmp = targetParent.Name;
				nameTmp = nameTmp.Replace("\\", "/");

				// Windows的场合
				if (nameTmp.EndsWith (":/")) {
					checkPath = $"{nameTmp}{checkPath}";
				} else if(nameTmp.Equals ("/")) {
					checkPath = $"{nameTmp}{checkPath}";	
				} else {
					checkPath = $"{nameTmp}/{checkPath}";	
				}
				targetParent = targetParent.Name.Equals (rootDir.Name) ? null : targetParent.Parent;
			}
			// Loger.Info ($"UtilsTools::CheckMatchPath():{iTargetPath} -> {checkPath}");
			return checkPath;
		}

		/// <summary>
		/// 检测文件路径
		/// </summary>
		/// <param name="iPath">文件路径</param>
		/// <param name="iPattern">校验模式</param>
		/// <returns>true : OK; false : NG;</returns>
		public static bool CheckFilePath(string iPath, string iPattern)
		{
			if (string.IsNullOrEmpty(iPath) || string.IsNullOrEmpty(iPattern)) return false;
			var rx = new Regex(iPattern);
			return rx.IsMatch(iPath);
		}

#endregion

#region Math

		/// <summary>
		/// 判断数字是否为某数的平方数
		///   备注:
		///     n^2 = (n-1)^2 +2(n-1) + 1
		///     n^2 = (n-1)^2 +(2n -1)
		///     n^2 = (n-2)^2 + (2(n-1) -1) + (2n-1)
		///     ······
		///     推导得到公式：
		///       n^2 = 1+3+5+7+······+(2n-1)
		/// </summary>
		/// <param name="iNum"></param>
		/// <returns>true : 是平方数; false : 不是平方数;</returns>
		public static bool IsSquare(int iNum)
		{
			// 变化步长为2，初值为1，一直减到num不再大于0
			for (var i = 1; iNum > 0; i += 2)
			{
				iNum -= i;
			}
			// 如果num减到最后，恰好等于0，就是平方数；反之，就不是
			return iNum == 0;
		}

		/// <summary>
		/// 判断是不是为2的N次幂
		/// </summary>
		/// <param name="iNum">数字</param>
		/// <returns>true : 是; false : 否;</returns>
		public static bool IsPowerOf2(int iNum)
		{
			if (iNum < 1) return false;
			return (iNum & iNum - 1) == 0;
		}
		
		/// <summary>
		/// 判断字符是否为数字
		/// </summary>
		/// <param name="iC">字符</param>
		/// <returns>true : 是; false : 否;</returns>
		public static bool IsNumber(char iC)
		{
			return iC >= '0' && iC <= '9';
		}

		/// <summary>
		/// 取得百分比文本
		/// </summary>
		/// <param name="iProgress">进度信息(0.0f～1.0f)</param>
		/// <param name="iFormat">格式:如:"{0:N}"</param>
		/// <returns></returns>
		public static string GetPercentText(float iProgress, string iFormat = null)
		{
			var progress = iProgress * 100;
			var format = iFormat;
			if (string.IsNullOrEmpty(format))
			{
				// 保留小数点后0位
				format = "{0:N0}%";
			}
			return string.Format(format, progress);
		}
		
		/// <summary>
		/// 取得百分比文本
		/// </summary>
		/// <param name="iCount">当前计数</param>
		/// <param name="iMaxCount">最大计数</param>
		/// <param name="iFormat">格式:如:"{0:N}"</param>
		/// <returns></returns>
		public static string GetPercentText(int iCount, int iMaxCount, string iFormat = null)
		{
			var progress = (0>= iMaxCount) ? 0.0f : (float)iCount / iMaxCount;
			var format = iFormat;
			if (string.IsNullOrEmpty(format))
			{
				// 保留小数点后0位
				format = "{0:N0}%";
			}
			return string.Format(format, progress);
		}
		
#endregion

#region UnityEngine - Components


#if UNITY_EDITOR

		/// <summary>
		/// 拷贝源对象上所有的Component到目标对象(包含各个属性)
		/// </summary>
		/// <param name="iSrc">源对象</param>
		/// <param name="iDst">目标对象</param>
		public static void CopyAllComponents(GameObject iSrc, GameObject iDst)
		{
			var copiedComponents = iSrc.GetComponents<Component>();
			if (null == iDst || null == copiedComponents || 0 >= copiedComponents.Length) return;
			var targetComponents = new List<Component>(iDst.GetComponents<Component>());
			foreach (var copiedComponent in copiedComponents)
			{
				if (null == copiedComponent) continue;
				UnityEditorInternal.ComponentUtility.CopyComponent(copiedComponent);
				// 在目标对象中查找同种类型的Component
				var targetComponent = targetComponents.Find((iTem) => iTem.GetType() == copiedComponent.GetType());
				// 若存在，则拷贝属性，弱不存在则新建追加
				if (null != targetComponent)
				{
					UnityEditorInternal.ComponentUtility.PasteComponentValues(targetComponent);
				}
				else
				{
					UnityEditorInternal.ComponentUtility.PasteComponentAsNew(iDst);
				}
			}
		}
		
#endif

#endregion
		
	}
}
