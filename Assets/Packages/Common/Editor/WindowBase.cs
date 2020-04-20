using System.IO;
using UnityEditor;
using UnityEngine;
using Packages.Common.Base;
using Packages.Common.Extend;
using Packages.Common.Extend.Editor;
using Packages.Utils;
using UnityEngine.Serialization;

namespace Packages.Common.Editor {

#region WindowConfInfo

	/// <summary>
	/// 窗口配置信息.
	/// </summary>
	public abstract class WindowConfInfoBase {

		/// <summary>
		/// 构造函数.
		/// </summary>
		public WindowConfInfoBase () {

			// 初始化
			Init();

		}

		/// <summary>
		/// 窗口名.
		/// </summary>
		public string WindowName;

		/// <summary>
		/// 标题.
		/// </summary>
		public string Title;

		/// <summary>
		/// 行高.
		/// </summary>
		public float LineHeight;

		/// <summary>
		/// 表示范围.
		/// </summary>
		public Rect DisplayRect;

		/// <summary>
		/// 初始化.
		/// </summary>
		public abstract void Init();

	}

#endregion

#region WindowDataBase

	/// <summary>
	/// 设定信息.
	/// </summary>
	public class WindowDataBase : JsonDataBase<WindowDataBase> {

		/// <summary>
		/// 应用.
		/// </summary>
		public virtual void Apply() { }

	}

#endregion

	/// <summary>
	/// 窗口基类.
	/// </summary>
	public abstract class WindowBase<T1, T2> : EditorWindow 
		where T1 : WindowDataBase, new() 
		where T2 : WindowConfInfoBase, new() {
		
		protected const float DefaultLineHeight = 20.0f;

		/// <summary>
		/// 暂停绘制.
		/// </summary>
		protected bool IsPause = false;

		/// <summary>
		/// 数据.
		/// </summary>
		[FormerlySerializedAs("Data")] 
		public T1 data = new T1();

#region WindowBase - ConfInfo

		/// <summary>
		/// 配置信息.
		/// </summary>
		protected static T2 ConfInfo = new T2();

		/// <summary>
		/// 窗口名.
		/// </summary>
		protected static string Name => ConfInfo?.WindowName;

		/// <summary>
		/// 标题.
		/// </summary>
		protected static string Title => ConfInfo?.Title;

		/// <summary>
		/// 行高.
		/// </summary>
		protected static float LineHeight { 
			get { 
				if (null == ConfInfo) {
					return DefaultLineHeight;
				}
				return 0.0f >= ConfInfo.LineHeight ? DefaultLineHeight : ConfInfo.LineHeight;
			}
		}

		/// <summary>
		/// 表示范围.
		/// </summary>
		protected static Rect DisplayRect => ConfInfo?.DisplayRect ?? new Rect(0.0f, 0.0f, 100.0f, 100.0f);

		#endregion

		protected string _jsonPath;
		/// <summary>
		/// 路径(Json).
		/// </summary>
		/// <value>路径.</value>
		public string JsonPath => string.IsNullOrEmpty (_jsonPath) ? null : _jsonPath;

		/// <summary>
		/// 清空数据.
		/// </summary>
		protected void ClearData()
		{
			data?.Clear ();
		}

#region WindowBase - virtual

		/// <summary>
		/// 从JSON文件，导入打包配置信息.
		/// </summary>
		/// <returns><c>true</c>, 导入成功, <c>false</c> 导入失败.</returns>
		/// <param name="iForceClear">强制清空.</param>
		public virtual bool ImportFromJsonFile(bool iForceClear = true) {
			return ImportFromJsonFile (JsonPath, iForceClear);
		}

		/// <summary>
		/// 从JSON文件，导入打包配置信息.
		/// </summary>
		/// <returns><c>true</c>, 导入成功, <c>false</c> 导入失败.</returns>
		/// <param name="iImportDir">导入路径.</param>
		/// <param name="iForceClear">强制清空.</param>
		public virtual bool ImportFromJsonFile(string iImportDir, bool iForceClear = true) {
			var importDir = iImportDir;
			if (string.IsNullOrEmpty (iImportDir)) {
				importDir = JsonPath;
			}
			bool fileExistFlg;
			var jsonData = UtilsAsset.ImportDataByDir<T1> (out fileExistFlg, importDir);
			if (jsonData == null) return false;
			ApplyImportData (jsonData, iForceClear);
			return true;
		}

		/// <summary>
		/// 导出成JSON文件.
		/// </summary>
		/// <returns>导出文件(Json格式).</returns>
		public virtual string ExportToJsonFile() {
			return ExportToJsonFile (JsonPath);
		}

		/// <summary>
		/// 导出成JSON文件.
		/// </summary>
		/// <returns>导出文件(Json格式).</returns>
		/// <param name="iExportDir">导出路径.</param>
		public virtual string ExportToJsonFile(string iExportDir) {
			var exportDir = iExportDir;
			if (string.IsNullOrEmpty (exportDir)) {
				exportDir = JsonPath;
			}
			var fileInfo = new FileInfo (JsonPath);
			if (fileInfo.Directory != null && UtilsTools.CheckAndCreateDirByFullDir(fileInfo.Directory.FullName))
				return UtilsAsset.ExportData<T1>(data, exportDir);
			this.Error("ExportToJsonFile -> CheckAndCreateDirByFullDir Failed!!! \n (AssetPath:{0})",
				JsonPath);
			return null;
		}

		/// <summary>
		/// 初始化.
		/// </summary>
		/// <param name="iJsonFilePath">Json文件路径.</param>
		public virtual bool Init(string iJsonFilePath = null) {
			// Json保存文件路径
			_jsonPath = iJsonFilePath;
			// 初始化窗口尺寸大小
			InitWindowSizeInfo (DisplayRect);

			// 初始化时导入最新
			return ImportFromJsonFile();
		}

		/// <summary>
		/// 清空.
		/// </summary>
		/// <param name="iIsFileDelete">删除数据文件标志位.</param>
		public virtual void Clear(bool iIsFileDelete = false) {

			// 清空数据
			ClearData();

			// 删除数据文件
			if (iIsFileDelete) {
				UtilsAsset.DeleteFile<T1> ();
			}

			UtilsAsset.SetAssetDirty (this);
		}

#endregion

#region WindowBase - GUI

		void OnGUI () {

			if (IsPause) {
				return;
			}
			var _diyRect = ConfInfo.DisplayRect;

			EditorGUILayout.LabelField (Title);
			_diyRect.y += LineHeight;
			_diyRect.height -= LineHeight;
			_diyRect.height -= LineHeight * 1.5f;

			// 绘制Window
			OnWindowGui ();

			EditorGUILayout.BeginHorizontal ();
			var buttonWidth = _diyRect.width - 6.0f * 4;
			buttonWidth /= 4.0f;

			if(GUILayout.Button("Clear",GUILayout.Width(buttonWidth)))
			{
				OnClearClick();
			}
			if(GUILayout.Button("Import",GUILayout.Width(buttonWidth)))
			{
				OnImportClick();
			}
			if(GUILayout.Button("Export",GUILayout.Width(buttonWidth)))
			{
				OnExportClick();
			}
			if(GUILayout.Button("Apply",GUILayout.Width(buttonWidth)))
			{
				OnApplyClick();
			}
			EditorGUILayout.EndHorizontal ();
		}

#endregion

#region WindowBase - virtual

		/// <summary>
		/// 清空按钮点击事件.
		/// </summary>
		protected virtual void OnClearClick() {
			this.Info("OnClearClick()");
			// 清空自定义宏一览
			Clear(true);
		}
			
		/// <summary>
		/// 导入按钮点击事件.
		/// </summary>
		protected virtual void OnImportClick() {
			this.Info("OnImportClick()");
			ImportFromJsonFile ();
		}

		/// <summary>
		/// 导出按钮点击事件.
		/// </summary>
		protected virtual void OnExportClick() {
			this.Info("OnExportClick()");
			ExportToJsonFile ();
		}

		/// <summary>
		/// 应用按钮点击事件.
		/// </summary>
		protected virtual void OnApplyClick() {
			this.Info("OnApplyClick()");
			// 先导出保存
			ExportToJsonFile ();

			// 应用
			Apply();
		}

		/// <summary>
		/// 初始化窗口尺寸信息.
		/// </summary>
		/// <param name="iDisplayRect">表示范围.</param>
		protected virtual void InitWindowSizeInfo(Rect iDisplayRect) {
			this.Info("InitWindowSizeInfo Rect(X:{0} Y:{1} Width:{2} Height:{3})",
				iDisplayRect.x, iDisplayRect.y, iDisplayRect.width, iDisplayRect.height);
		}
			
		/// <summary>
		/// 应用.
		/// </summary>
		public virtual void Apply () {
			data?.Apply ();
		}
	
#endregion

#region WindowBase - abstract

		/// <summary>
		/// 绘制WindowGUI.
		/// </summary>
		protected abstract void OnWindowGui ();

		/// <summary>
		/// 应用导入数据数据.
		/// </summary>
		/// <param name="iData">数据.</param>
		/// <param name="iForceClear">强制清空标志位.</param>
		protected abstract void ApplyImportData (T1 iData, bool iForceClear);

#endregion
	}

}
