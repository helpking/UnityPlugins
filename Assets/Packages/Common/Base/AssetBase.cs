using System.IO;
using UnityEngine;
using UnityEngine.Serialization;
using Packages.Utils;
using Packages.Logs;

namespace Packages.Common.Base {

	/// <summary>
	/// Asset设定接口.
	/// </summary>
	public interface IAssetBase {

		/// <summary>
		/// 初始化.
		/// </summary>
		/// <param name="iAssetFilePath">asset文件路径.</param>
		bool Init(string iAssetFilePath = null);

		/// <summary>
		/// 刷新.
		/// </summary>
		void Refresh();

		/// <summary>
		/// 清空.
		/// </summary>
		/// <param name="iIsFileDelete">删除数据文件标志位.</param>
		/// <param name="iDirPath">Asset存放目录文件（不指定：当前选定对象所在目录）.</param>
		void Clear(bool iIsFileDelete = false, string iDirPath = null);

		/// <summary>
		/// 取得导入路径.
		/// </summary>
		/// <returns>导入路径.</returns>
		string GetImportPath ();

		/// <summary>
		/// 从JSON文件，导入打包配置信息.
		/// </summary>
		/// <returns><c>true</c>, 导入成功, <c>false</c> 导入失败.</returns>
		/// <param name="iFileName">导入文件名.</param>
		/// <param name="iForceClear">强制清空.</param>
		bool ImportFromJsonFile(string iFileName = null, bool iForceClear = true);

		/// <summary>
		/// 从JSON文件，导入打包配置信息.
		/// </summary>
		/// <returns><c>true</c>, 导入成功, <c>false</c> 导入失败.</returns>
		/// <param name="iImportDir">导入路径.</param>
		/// <param name="iFileName">导入文件名.</param>
		/// <param name="iForceClear">强制清空.</param>
		bool ImportFromJsonFile(string iImportDir, string iFileName = null, bool iForceClear = true);

		/// <summary>
		/// 取得导出路径.
		/// </summary>
		/// <returns>导出路径.</returns>
		string GetExportPath ();

		/// <summary>
		/// 导出成JSON文件.
		/// </summary>
		/// <returns>导出文件(Json格式)路径.</returns>
		string ExportToJsonFile();

		/// <summary>
		/// 导出成JSON文件.
		/// </summary>
		/// <returns>导出文件(Json格式)路径.</returns>
		/// <param name="iExportDir">导出目录路径.</param>
		/// <param name="iFileName">导入文件名.</param>
		string ExportToJsonFile(string iExportDir, string iFileName = null);
	}

	/// <summary>
	/// Asset设定类.
	/// </summary>
	public abstract class AssetBase<T1, T2> : ScriptableObject, IAssetBase
		where T1 : ScriptableObject, IAssetBase, new()
		where T2 : JsonDataBase, new() {

		/// <summary>
		/// 实例.
		/// </summary>
		private static T1 _instance;    

		/// <summary>
		/// 取得实例.
		/// </summary>
		/// <returns>实例.</returns>
		/// <param name="iPath">读取路径.</param>
		public static T1 GetInstance(string iPath = null) {
			if (_instance != null) return _instance;
			_instance = UtilsAsset.ReadSetting<T1>(iPath);
			var name = typeof(T1).Name;
			if (null != _instance && _instance.Init ())
			{
				var log =
					$"{name} GetInstance()::Succeeded!!!(AssetPath:{iPath})";
				Loger.Info(log);
			} else {
				var log =
					$"{name} GetInstance()::Succeeded!!!(AssetPath:{iPath})";
				Loger.Error (log);
				return null;
			}
			return _instance;
		}

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
		/// 数据.
		/// </summary>
		[FormerlySerializedAs("Data")] 
		public T2 data = new T2();

		protected string _path;
		/// <summary>
		/// 路径.
		/// </summary>
		/// <value>路径.</value>
		public string Path {
			get {
				if (!string.IsNullOrEmpty(_path)) return _path;
				_path = UtilsAsset.GetAssetFileDir ();
				_path = UtilsTools.CheckMatchPath (_path);
				return _path;
			}
		}

		private string _importJsonPath;
		/// <summary>
		/// 导入路径(Json).
		/// </summary>
		/// <value>导入路径(Json).</value>
		public string ImportJsonPath {
			get {
				if (!string.IsNullOrEmpty(_importJsonPath)) return _importJsonPath;
				if (string.IsNullOrEmpty(Path)) return _importJsonPath;
				_importJsonPath = $"{Path}/Json";
				if (false == Directory.Exists (_importJsonPath)) {
					Directory.CreateDirectory (_importJsonPath);
				}
				return _importJsonPath;
			}
			protected set { 
				_importJsonPath = value;
			}
		}

		private string _exportJsonPath;
		/// <summary>
		/// 导出路径(Json).
		/// </summary>
		/// <value>导出路径(Json).</value>
		public string ExportJsonPath {
			get {
				if (!string.IsNullOrEmpty(_exportJsonPath)) return _exportJsonPath;
				if (string.IsNullOrEmpty(Path)) return _exportJsonPath;
				_exportJsonPath = $"{Path}/Json";
				if (false == Directory.Exists (_exportJsonPath)) {
					Directory.CreateDirectory (_exportJsonPath);
				}
				return _exportJsonPath;
			}
			protected set { 
				_exportJsonPath = value;
			}
		}

		/// <summary>
		/// 清空数据.
		/// </summary>
		private void ClearData()
		{
			data?.Clear();
		}

#region virtual

		/// <summary>
		/// 取得导入路径.
		/// </summary>
		/// <returns>导入路径.</returns>
		public virtual string GetImportPath () {
			return ImportJsonPath;
		}

		/// <summary>
		/// 从JSON文件，导入打包配置信息.
		/// </summary>
		/// <returns><c>true</c>, 导入成功, <c>false</c> 导入失败.</returns>
		/// <param name="iFileName">文件名.</param>
		/// <param name="iForceClear">强制清空.</param>
		public virtual bool ImportFromJsonFile(string iFileName = null, bool iForceClear = true) {
			return ImportFromJsonFile (GetImportPath(), iFileName, iForceClear);
		}

		/// <summary>
		/// 从JSON文件，导入打包配置信息.
		/// </summary>
		/// <returns><c>true</c>, 导入成功, <c>false</c> 导入失败.</returns>
		/// <param name="iImportDir">导入路径.</param>
		/// <param name="iFileName">文件名.</param>
		/// <param name="iForceClear">强制清空.</param>
		public virtual bool ImportFromJsonFile(string iImportDir, string iFileName = null, bool iForceClear = true) {
			var importDir = iImportDir;
			if (string.IsNullOrEmpty (iImportDir)) {
				importDir = GetImportPath ();
			} else {
				ImportJsonPath = iImportDir;
			}
			bool fileExistFlg;
			var jsonData = UtilsAsset.ImportDataByDir<T2> (out fileExistFlg, importDir, iFileName);
			if (jsonData != null) {
				ApplyData (jsonData, iForceClear);
				return true;
			} 
			// 文件不存在则，视为导入成功
			if (fileExistFlg) return false;
			Init ();
			return true;
		}

		/// <summary>
		/// 取得导出路径.
		/// </summary>
		/// <returns>导出路径.</returns>
		public virtual string GetExportPath () {
			return ExportJsonPath;
		}

		/// <summary>
		/// 导出成JSON文件.
		/// </summary>
		/// <returns>导出文件(Json格式).</returns>
		public virtual string ExportToJsonFile() {
			return ExportToJsonFile (GetExportPath());
		}

		/// <summary>
		/// 导出成JSON文件.
		/// </summary>
		/// <returns>导出文件(Json格式).</returns>
		/// <param name="iExportDir">导出路径.</param>
		/// <param name="iFileName">导入文件名.</param>
		public virtual string ExportToJsonFile(string iExportDir, string iFileName = null) {
			var exportDir = iExportDir;
			if (string.IsNullOrEmpty (exportDir)) {
				exportDir = GetExportPath ();
			} else {
				ExportJsonPath = iExportDir;
			}
			return UtilsAsset.ExportData (data, exportDir);
		}

		/// <summary>
		/// 初始化.
		/// </summary>
		/// <param name="iAssetFilePath">Asset文件路径.</param>
		public virtual bool Init(string iAssetFilePath = null) {
			_path = iAssetFilePath;
			return InitAsset ();
		}

		/// <summary>
		/// 刷新.
		/// </summary>
		public virtual void Refresh() { }

		/// <summary>
		/// 清空.
		/// </summary>
		/// <param name="iIsFileDelete">删除数据文件标志位.</param>
		/// <param name="iDirPath">Asset存放目录文件（不指定：当前选定对象所在目录）.</param>
		public virtual void Clear(bool iIsFileDelete = false, string iDirPath = null) {

			// 清空数据
			ClearData();

			// 删除数据文件
			if (iIsFileDelete) {
				var dir = iDirPath;
				if (string.IsNullOrEmpty (dir)) {
					dir = GetImportPath ();
				}
				UtilsAsset.DeleteFile<T2> (dir);
			}

			UtilsAsset.SetAssetDirty (this);
		}

		/// <summary>
		/// 初始化数据.
		/// </summary>
		/// <returns><c>true</c>, OK, <c>false</c> NG.</returns>
		protected virtual bool InitAsset () { return true; }

#endregion

#region abstract

		/// <summary>
		/// 用用数据.
		/// </summary>
		/// <param name="iData">数据.</param>
		/// <param name="iForceClear">强制清空标志位.</param>
		protected abstract void ApplyData (T2 iData, bool iForceClear = true);

#endregion
	}

	/// <summary>
	/// Asset设定类.
	/// </summary>
	public abstract class AssetReadOnlyBase<T1, T2> : ScriptableObject, IAssetBase
		where T1 : ScriptableObject, IAssetBase, new()
		where T2 : JsonDataBase, new() {
	
		/// <summary>
		/// 实例.
		/// </summary>
		private static T1 _instance;    

		/// <summary>
		/// 取得实例.
		/// </summary>
		/// <returns>实例.</returns>
		/// <param name="iPath">读取路径.</param>
		public static T1 GetInstance(string iPath = null) {
			if (_instance != null) return _instance;
			_instance = UtilsAsset.ReadSetting<T1>(iPath);
			var name = typeof(T1).Name;
			if (null != _instance && _instance.Init ()) {
				var log =
					$"{name} GetInstance()::Succeeded!!!(AssetPath:{iPath})";
				Loger.Error(log);
			} else {
				var log =
					$"{name} GetInstance()::Succeeded!!!(AssetPath:{iPath})";
				Loger.Error(log);
				return null;
			}
			return _instance;
		}

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
		/// 数据.
		/// </summary>
		[InspectorReadOnly(1f, 0.0f, 1f, 1f)]
		public T2 data = new T2();

		protected string _path;
		/// <summary>
		/// 路径.
		/// </summary>
		/// <value>路径.</value>
		public string Path {
			get {
				if (!string.IsNullOrEmpty(_path)) return _path;
				_path = UtilsAsset.GetAssetFileDir ();
				_path = UtilsTools.CheckMatchPath (_path);
				return _path;
			}
		}

		private string _importJsonPath;
		/// <summary>
		/// 导入路径(Json).
		/// </summary>
		/// <value>导入路径(Json).</value>
		public string ImportJsonPath {
			get {
				if (!string.IsNullOrEmpty(_importJsonPath)) return _importJsonPath;
				if (string.IsNullOrEmpty(Path)) return _importJsonPath;
				_importJsonPath = $"{Path}/Json";
				if (false == Directory.Exists (_importJsonPath)) {
					Directory.CreateDirectory (_importJsonPath);
				}
				return _importJsonPath;
			}
			protected set { 
				_importJsonPath = value;
			}
		}

		private string _exportJsonPath;
		/// <summary>
		/// 导出路径(Json).
		/// </summary>
		/// <value>导出路径(Json).</value>
		public string ExportJsonPath {
			get {
				if (true != string.IsNullOrEmpty(_exportJsonPath)) return _exportJsonPath;
				if (string.IsNullOrEmpty(Path)) return _exportJsonPath;
				_exportJsonPath = $"{Path}/Json";
				if (false == Directory.Exists (_exportJsonPath)) {
					Directory.CreateDirectory (_exportJsonPath);
				}
				return _exportJsonPath;
			}
			protected set { 
				_exportJsonPath = value;
			}
		}

		/// <summary>
		/// 清空数据.
		/// </summary>
		private void ClearData()
		{
			data?.Clear();
		}

#region virtual

		/// <summary>
		/// 取得导入路径.
		/// </summary>
		/// <returns>导入路径.</returns>
		public virtual string GetImportPath () {
			return ImportJsonPath;
		}

		/// <summary>
		/// 从JSON文件，导入打包配置信息.
		/// </summary>
		/// <returns><c>true</c>, 导入成功, <c>false</c> 导入失败.</returns>
		/// <param name="iFileName">导入文件名.</param>
		/// <param name="iForceClear">强制清空.</param>
		public virtual bool ImportFromJsonFile(string iFileName = null, bool iForceClear = true) {
			return ImportFromJsonFile (GetImportPath(), iFileName, iForceClear);
		}

		/// <summary>
		/// 从JSON文件，导入打包配置信息.
		/// </summary>
		/// <returns><c>true</c>, 导入成功, <c>false</c> 导入失败.</returns>
		/// <param name="iImportDir">导入路径.</param>
		/// <param name="iFileName">导入文件名.</param>
		/// <param name="iForceClear">强制清空.</param>
		public virtual bool ImportFromJsonFile(
			string iImportDir, string iFileName, bool iForceClear = true) {
			var importDir = iImportDir;
			if (string.IsNullOrEmpty (iImportDir)) {
				importDir = GetImportPath ();
			} else {
				ImportJsonPath = iImportDir;
			}
			bool fileExistFlg;
			var jsonData = UtilsAsset.ImportDataByDir<T2> (out fileExistFlg, importDir, iFileName);
			if (jsonData != null) {
				ApplyData (jsonData, iForceClear);
				return true;
			} 
			// 文件不存在则，视为导入成功
			if (fileExistFlg) return false;
			Init ();
			return true;
		}

		/// <summary>
		/// 取得导出路径.
		/// </summary>
		/// <returns>导出路径.</returns>
		public virtual string GetExportPath () {
			return ExportJsonPath;
		}

		/// <summary>
		/// 导出成JSON文件.
		/// </summary>
		/// <returns>导出文件(Json格式).</returns>
		public virtual string ExportToJsonFile() {
			return ExportToJsonFile (GetExportPath());
		}

		/// <summary>
		/// 导出成JSON文件.
		/// </summary>
		/// <returns>导出文件(Json格式).</returns>
		/// <param name="iExportDir">导出路径.</param>
		/// <param name="iFileName">导入文件名.</param>
		public virtual string ExportToJsonFile(string iExportDir, string iFileName = null) {
			var exportDir = iExportDir;
			if (string.IsNullOrEmpty (exportDir)) {
				exportDir = GetExportPath ();
			} else {
				ExportJsonPath = iExportDir;
			}
			return UtilsAsset.ExportData (data, exportDir, iFileName);
		}

		/// <summary>
		/// 初始化.
		/// </summary>
		/// <param name="iAssetFilePath">Asset文件路径.</param>
		public virtual bool Init(string iAssetFilePath = null) {
			_path = iAssetFilePath;
			return InitAsset ();
		}
			
		/// <summary>
		/// 刷新.
		/// </summary>
		public virtual void Refresh() { }

		/// <summary>
		/// 清空.
		/// </summary>
		/// <param name="iIsFileDelete">删除数据文件标志位.</param>
		/// <param name="iDirPath">Asset存放目录文件（不指定：当前选定对象所在目录）.</param>
		public virtual void Clear(bool iIsFileDelete = false, string iDirPath = null) {

			// 清空数据
			ClearData();

			// 删除数据文件
			if (iIsFileDelete) {
				var _dir = iDirPath;
				if (string.IsNullOrEmpty (_dir)) {
					_dir = GetImportPath ();
				}
				UtilsAsset.DeleteFile<T2> (_dir);
			}

			UtilsAsset.SetAssetDirty (this);
		}

		/// <summary>
		/// 初始化数据.
		/// </summary>
		/// <returns><c>true</c>, OK, <c>false</c> NG.</returns>
		protected virtual bool InitAsset () { return true; }

#endregion

#region abstract

		/// <summary>
		/// 用用数据.
		/// </summary>
		/// <param name="iData">数据.</param>
		/// <param name="iForceClear">强制清空标志位.</param>
		protected abstract void ApplyData (T2 iData, bool iForceClear = true);

#endregion

	}
		
	/// <summary>
	/// Asset设定类(带Options).
	/// </summary>
	public abstract class AssetOptionsBase<T1, T2, T3, T4> : ScriptableObject, IAssetBase
		where T1 : ScriptableObject, IAssetBase, new()
		where T2 : OptionsDataBase<T3, T4>, new()
		where T3 : JsonDataBase, new()
		where T4 : OptionsBaseData, new() {

		/// <summary>
		/// 实例.
		/// </summary>
		private static T1 _instance;    

		/// <summary>
		/// 取得实例.
		/// </summary>
		/// <returns>实例.</returns>
		/// <param name="iPath">读取路径.</param>
		public static T1 GetInstance(string iPath = null) {
			if (_instance != null) return _instance;
			_instance = UtilsAsset.ReadSetting<T1>(iPath);
			var name = typeof(T1).Name;
			if (null != _instance && _instance.Init ()) {
				var log =
					$"{name} GetInstance()::Succeeded!!!(AssetPath:{iPath})";
				Loger.Info(log);
			} else {
				var log =
					$"{name} GetInstance()::Succeeded!!!(AssetPath:{iPath})";
				Loger.Error(log);
				return null;
			}
			return _instance;
		}

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
		/// 数据.
		/// </summary>
		[FormerlySerializedAs("Data")] 
		public T2 data = new T2();

		/// <summary>
		/// Asset文件的导入/导出地址
		/// </summary>
		protected string _assetPath;
		/// <summary>
		/// 路径.
		/// </summary>
		/// <value>路径.</value>
		public string AssetPath {
			get {
				if (!string.IsNullOrEmpty(_assetPath)) return _assetPath;
				_assetPath = UtilsAsset.GetAssetFileDir ();
				_assetPath = UtilsTools.CheckMatchPath (_assetPath);
				return _assetPath;
			}
		}

		private string _importJsonPath;
		/// <summary>
		/// 导入路径(Json).
		/// </summary>
		/// <value>导入路径(Json).</value>
		public string ImportJsonPath {
			get {
				if (!string.IsNullOrEmpty(_importJsonPath)) return _importJsonPath;
				if (string.IsNullOrEmpty(AssetPath)) return _importJsonPath;
				_importJsonPath = $"{AssetPath}/Json";
				if (false == Directory.Exists (_importJsonPath)) {
					Directory.CreateDirectory (_importJsonPath);
				}
				return _importJsonPath;
			}
			protected set { 
				_importJsonPath = value;
			}
		}

		private string _exportJsonPath;
		/// <summary>
		/// 导出路径(Json).
		/// </summary>
		/// <value>导出路径(Json).</value>
		public string ExportJsonPath {
			get {
				if (!string.IsNullOrEmpty(_exportJsonPath)) return _exportJsonPath;
				if (string.IsNullOrEmpty(AssetPath)) return _exportJsonPath;
				_exportJsonPath = $"{AssetPath}/Json";
				if (false == Directory.Exists (_exportJsonPath)) {
					Directory.CreateDirectory (_exportJsonPath);
				}
				return _exportJsonPath;
			}
			protected set { 
				_exportJsonPath = value;
			}
		}

		/// <summary>
		/// 清空数据.
		/// </summary>
		private void ClearData()
		{
			data?.Clear();
		}

#region virtual

		/// <summary>
		/// 取得导入路径.
		/// </summary>
		/// <returns>导入路径.</returns>
		public virtual string GetImportPath () {
			return ImportJsonPath;
		}

		/// <summary>
		/// 从JSON文件，导入打包配置信息.
		/// </summary>
		/// <returns><c>true</c>, 导入成功, <c>false</c> 导入失败.</returns>
		/// <param name="iFileName">文件名.</param>
		/// <param name="iForceClear">强制清空.</param>
		public virtual bool ImportFromJsonFile(string iFileName = null, bool iForceClear = true) {
			return ImportFromJsonFile (GetImportPath(), iFileName, iForceClear);
		}

		/// <summary>
		/// 从JSON文件，导入打包配置信息.
		/// </summary>
		/// <returns><c>true</c>, 导入成功, <c>false</c> 导入失败.</returns>
		/// <param name="iImportDir">导入路径.</param>
		/// <param name="iFileName">文件名.</param>
		/// <param name="iForceClear">强制清空.</param>
		public virtual bool ImportFromJsonFile(string iImportDir, string iFileName = null, bool iForceClear = true) {
			var importDir = iImportDir;
			if (string.IsNullOrEmpty (iImportDir)) {
				importDir = GetImportPath ();
			} else {
				ImportJsonPath = iImportDir;
			}
			bool fileExistFlg;
			var jsonData = UtilsAsset.ImportDataByDir<T2> (out fileExistFlg, importDir, iFileName);
			if (jsonData != null) {
				ApplyData (jsonData, iForceClear);
				return true;
			} 
			// 文件不存在则，视为导入成功
			if (fileExistFlg) return false;
			Init ();
			return true;
		}

		/// <summary>
		/// 取得导出路径.
		/// </summary>
		/// <returns>导出路径.</returns>
		public virtual string GetExportPath () {
			return ExportJsonPath;
		}

		/// <summary>
		/// 导出成JSON文件.
		/// </summary>
		/// <returns>导出文件(Json格式).</returns>
		public virtual string ExportToJsonFile() {
			return ExportToJsonFile (GetExportPath());
		}

		/// <summary>
		/// 导出成JSON文件.
		/// </summary>
		/// <returns>导出文件(Json格式).</returns>
		/// <param name="iExportDir">导出路径.</param>
		/// <param name="iFileName">导入文件名.</param>
		public virtual string ExportToJsonFile(string iExportDir, string iFileName = null) {
			var exportDir = iExportDir;
			if (string.IsNullOrEmpty (exportDir)) {
				exportDir = GetExportPath ();
			} else {
				ExportJsonPath = iExportDir;
			}
			return UtilsAsset.ExportData (data, exportDir);
		}

		/// <summary>
		/// 初始化.
		/// </summary>
		/// <param name="iAssetFilePath">Asset文件路径.</param>
		public virtual bool Init(string iAssetFilePath = null) {
			_assetPath = iAssetFilePath;
			return InitAsset ();
		}

		/// <summary>
		/// 刷新.
		/// </summary>
		public virtual void Refresh() { }

		/// <summary>
		/// 清空.
		/// </summary>
		/// <param name="iIsFileDelete">删除数据文件标志位.</param>
		/// <param name="iDirPath">Asset存放目录文件（不指定：当前选定对象所在目录）.</param>
		public virtual void Clear(bool iIsFileDelete = false, string iDirPath = null) {

			// 清空数据
			ClearData();

			// 删除数据文件
			if (iIsFileDelete) {
				UtilsAsset.DeleteFile<T2> (iDirPath);
			}

			UtilsAsset.SetAssetDirty (this);
		}

		/// <summary>
		/// 初始化数据.
		/// </summary>
		/// <returns><c>true</c>, OK, <c>false</c> NG.</returns>
		protected virtual bool InitAsset () { return true; }

#endregion

#region abstract

		/// <summary>
		/// 用用数据.
		/// </summary>
		/// <param name="iData">数据.</param>
		/// <param name="iForceClear">强制清空标志位.</param>
		protected abstract void ApplyData (T2 iData, bool iForceClear = true);

#endregion

	}
}
