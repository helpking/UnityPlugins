using System.IO;
using UnityEditor;
using UnityEngine;
using Packages.Common.Base;
using Packages.Common.Base.Editor;
using Packages.Common.Extend;
using Packages.Common.Extend.Editor;
using Packages.Utils;

namespace Packages.Common.Editor {

	/// <summary>
	/// Asset 文件编辑器基类.
	/// </summary>
	public class AssetEditorBase<T1, T2> : EditorBase 
		where T1 : ScriptableObject, IAssetBase, new()
		where T2 : JsonDataBase, new() {

		protected T1 AssetSetting;

		/// <summary>
		/// 导入路径.
		/// </summary>
		protected string ImportDir;

		/// <summary>
		/// 导出路径.
		/// </summary>
		protected string ExportDir;

#region Inspector

		public override void OnInspectorGUI ()  {
			//base.OnInspectorGUI ();
			serializedObject.Update ();

			AssetSetting = target as T1; 
			if (AssetSetting != null) {

				// 初始化标题信息
				InitTitleInfo(AssetSetting);

				// 刷新按钮
				EditorGUILayout.BeginHorizontal();
				InitTopButtons (AssetSetting);
				EditorGUILayout.EndHorizontal ();

				// 初始化主面板
				InitMainPanel(AssetSetting);

				// 刷新按钮
				EditorGUILayout.BeginHorizontal();
				InitBottomButtons (AssetSetting);
				EditorGUILayout.EndHorizontal ();
			}

			// 保存变化后的值
			serializedObject.ApplyModifiedProperties();
		}

		/// <summary>
		/// 初始化标题信息.
		/// </summary>
		/// <param name="iTarget">目标信息.</param>
		protected virtual void InitTitleInfo(T1 iTarget) {
			if (string.IsNullOrEmpty (ImportDir)) {
				ImportDir = UtilsTools.CheckMatchPath (AssetSetting.GetImportPath ());
			}
			DrawLabel(0, "Import Dir", Color.white,ImportDir);
			if (string.IsNullOrEmpty (ExportDir)) {
				ExportDir = UtilsTools.CheckMatchPath (AssetSetting.GetExportPath ());
			}
			DrawLabel(0, "Export Dir", Color.white, ExportDir);
		}

		/// <summary>
		/// 初始化顶部按钮列表.
		/// </summary>
		/// <param name="iTarget">目标信息.</param>
		protected virtual void InitTopButtons(T1 iTarget) {
			
			// 清空按钮
			if(GUILayout.Button("Clear"))
			{
				Clear();
			}

			// 强力清空按钮
			if(GUILayout.Button("ForceClear"))
			{
				Clear(true);
			}

			// 导入按钮
			if(GUILayout.Button("Import"))
			{
				Import();
			}

			// 导出按钮
			if(GUILayout.Button("Export"))
			{
				Export();
			}
		}

		/// <summary>
		/// 初始化主面板.
		/// </summary>
		/// <param name="iTarget">目标信息.</param>
		protected virtual void InitMainPanel(T1 iTarget) {
			if (default(T1) == iTarget) {
				return;
			}
			var data = serializedObject.FindProperty ("data");
			if (null == data) {
				this.Error ("InitMainPanel():The data is null!!!");
				return;
			}
			EditorGUILayout.PropertyField (data, true);
		}
			
		/// <summary>
		/// 初始化底部按钮列表.
		/// </summary>
		/// <param name="iTarget">目标信息.</param>
		protected virtual void InitBottomButtons(T1 iTarget) {

			// 清空按钮
			if(GUILayout.Button("Clear"))
			{
				Clear();
			}

			// 强力清空按钮
			if(GUILayout.Button("ForceClear"))
			{
				Clear(true);
			}

			// 导入按钮
			if(GUILayout.Button("Import"))
			{
				Import();
			}

			// 导出按钮
			if(GUILayout.Button("Export"))
			{
				Export();
			}
		}

		/// <summary>
		/// 清空.
		/// </summary>
		/// <param name="iForceClear">强力清除标识位（删除Json文件）.</param>
		protected virtual void Clear(bool iForceClear = false) {
			if (default(T1) == AssetSetting) {
				return;
			}
			AssetSetting.Clear (iForceClear);
		}
			
		/// <summary>
		/// 导入.
		/// </summary>
		protected virtual void Import() {
			if (default(T1) == AssetSetting) {
				return;
			}
			AssetSetting.ImportFromJsonFile ();
		}

		/// <summary>
		/// 导出.
		/// </summary>
		protected virtual void Export() {
			if (default(T1) == AssetSetting) {
				return;
			}
			AssetSetting.ExportToJsonFile ();
		}

#endregion

		/// <summary>
		/// 取得当前选中对象所在目录.
		/// </summary>
		/// <returns>当前选中对象所在目录.</returns>
		protected static string GetCurDir()
		{
			var obj = Selection.GetFiltered(typeof(Object), SelectionMode.Assets);
			var path = AssetDatabase.GetAssetPath(obj[0]);

			if(path.Contains(".") == false)
			{
				path += "/";
			}

			return path;
		}

#region Creator

		/// <summary>
		/// 创建设定文件（在当前目录）.
		/// </summary>
		protected static T1 CreateAssetAtCurDir ()	{	

			var curDir = GetCurDir ();
			return Directory.Exists (curDir) == false ? null : UtilsAsset.CreateAsset<T1> (curDir);
		}

		/// <summary>
		/// 创建设定文件（在当前目录）.
		/// </summary>
		/// <returns>创建文件的对象.</returns>
		/// <param name="iDir">创建路径.</param>
		protected static T1 CreateAsset (string iDir = null)	{	
			return UtilsAsset.CreateAsset<T1> (iDir);	
		}

          #endregion

	}

	/// <summary>
	/// Asset 文件编辑器基类.
	/// </summary>
	public class AssetEditorReadOnlyBase<T1, T2> : AssetEditorBase<T1, T2> 
		where T1 : ScriptableObject, IAssetBase, new()
		where T2 : JsonDataBase<T2>, new() {
	
		/// <summary>
		/// 初始化顶部按钮列表.
		/// </summary>
		/// <param name="iTarget">目标信息.</param>
		protected override void InitTopButtons(T1 iTarget) {

			// 导入按钮
			if(GUILayout.Button("Import"))
			{
				Import();
			}

			// 导出按钮
			if(GUILayout.Button("Export"))
			{
				Export();
			}
		}

		/// <summary>
		/// 初始化底部按钮列表.
		/// </summary>
		/// <param name="iTarget">目标信息.</param>
		protected override void InitBottomButtons(T1 iTarget) {

			// 导入按钮
			if(GUILayout.Button("Import"))
			{
				Import();
			}

			// 导出按钮
			if(GUILayout.Button("Export"))
			{
				Export();
			}
		}
	}

	/// <summary>
	/// Asset 文件编辑器基类(Options).
	/// </summary>
	public class AssetOptionsEditorBase<T1, T2, T3, T4> : EditorBase 
		where T1 : ScriptableObject, IAssetBase, new()
		where T2 : OptionsDataBase<T3, T4>, new() 
		where T3 : JsonDataBase, new() 
		where T4 : OptionsBaseData, new() {

		protected T1 AssetSetting;

		/// <summary>
		/// 导入路径.
		/// </summary>
		protected string ImportDir;

		/// <summary>
		/// 导出路径.
		/// </summary>
		protected string ExportDir;

		#region Inspector

		public override void OnInspectorGUI ()  {
			base.OnInspectorGUI ();
			serializedObject.Update ();

			AssetSetting = target as T1; 
			if (AssetSetting != null) {

				// 初始化标题信息
				InitTitleInfo(AssetSetting);

				// 刷新按钮
				EditorGUILayout.BeginHorizontal();
				InitTopButtons (AssetSetting);
				EditorGUILayout.EndHorizontal ();

				// 初始化主面板
				InitMainPanelOfGeneral(AssetSetting);

				// 初始化选项面板
				InitMainPanelOfOptions(AssetSetting);

				// 刷新按钮
				EditorGUILayout.BeginHorizontal();
				InitBottomButtons (AssetSetting);
				EditorGUILayout.EndHorizontal ();
			}

			// 保存变化后的值
			serializedObject.ApplyModifiedProperties();
		}

		/// <summary>
		/// 初始化标题信息.
		/// </summary>
		/// <param name="iTarget">目标信息.</param>
		protected virtual void InitTitleInfo(T1 iTarget) {
			if (string.IsNullOrEmpty (ImportDir)) {
				ImportDir = UtilsTools.CheckMatchPath (AssetSetting.GetImportPath ());
			}
			EditorGUILayout.LabelField ("Import Dir", ImportDir);
			if (string.IsNullOrEmpty (ExportDir)) {
				ExportDir = UtilsTools.CheckMatchPath (AssetSetting.GetExportPath ());
			}
			EditorGUILayout.LabelField ("Export Dir", ExportDir);
		}

		/// <summary>
		/// 初始化顶部按钮列表.
		/// </summary>
		/// <param name="iTarget">目标信息.</param>
		protected virtual void InitTopButtons(T1 iTarget) {

			// 清空按钮
			if(GUILayout.Button("Clear"))
			{
				Clear();
			}

			// 强力清空按钮
			if(GUILayout.Button("ForceClear"))
			{
				Clear(true);
			}

			// 导入按钮
			if(GUILayout.Button("Import"))
			{
				Import();
			}

			// 导出按钮
			if(GUILayout.Button("Export"))
			{
				Export();
			}
		}

		/// <summary>
		/// 初始化主面板（默认）.
		/// </summary>
		/// <param name="iTarget">目标信息.</param>
		protected virtual void InitMainPanelOfGeneral(T1 iTarget) {
			if (default(T1) == iTarget) {
				return;
			}
			var general = serializedObject.FindProperty ("data.General");
			if (null == general) {
				this.Error("InitMainPanelOfDefault():The Data.General is null!!!");
				return;
			}
			EditorGUILayout.PropertyField (general, true);
		}

		/// <summary>
		/// 初始化主面板（选项）.
		/// </summary>
		/// <param name="iTarget">目标信息.</param>
		protected virtual void InitMainPanelOfOptions(T1 iTarget) {
			if (default(T1) == iTarget) {
				return;
			}
			var options = serializedObject.FindProperty ("data.Options");
			if (null == options) {
				this.Error ("InitMainPanelOfOptions():The Data.Options is null!!!");
				return;
			}
			EditorGUILayout.PropertyField (options, true);
		}

		/// <summary>
		/// 初始化底部按钮列表.
		/// </summary>
		/// <param name="iTarget">目标信息.</param>
		protected virtual void InitBottomButtons(T1 iTarget) {

			// 清空按钮
			if(GUILayout.Button("Clear"))
			{
				Clear();
			}

			// 强力清空按钮
			if(GUILayout.Button("ForceClear"))
			{
				Clear(true);
			}

			// 导入按钮
			if(GUILayout.Button("Import"))
			{
				Import();
			}

			// 导出按钮
			if(GUILayout.Button("Export"))
			{
				Export();
			}
		}

		/// <summary>
		/// 清空.
		/// </summary>
		/// <param name="iForceClear">强力清除标识位（删除Json文件）.</param>
		protected virtual void Clear(bool iForceClear = false) {
			if (default(T1) == AssetSetting) {
				return;
			}
			AssetSetting.Clear (iForceClear);
		}

		/// <summary>
		/// 导入.
		/// </summary>
		protected virtual void Import() {
			if (default(T1) == AssetSetting) {
				return;
			}
			AssetSetting.ImportFromJsonFile ();
		}

		/// <summary>
		/// 导出.
		/// </summary>
		protected virtual void Export() {
			if (default(T1) == AssetSetting) {
				return;
			}
			AssetSetting.ExportToJsonFile ();
		}

		#endregion

		/// <summary>
		/// 取得当前选中对象所在目录.
		/// </summary>
		/// <returns>当前选中对象所在目录.</returns>
		protected static string GetCurDir()
		{
			var obj = Selection.GetFiltered(typeof(Object), SelectionMode.Assets);
			var path = AssetDatabase.GetAssetPath(obj[0]);

			if(path.Contains(".") == false)
			{
				path += "/";
			}

			return path;
		}

#region Creator

		/// <summary>
		/// 创建设定文件（在当前目录）.
		/// </summary>
		protected static T1 CreateAssetAtCurDir () {	

			var curDir = GetCurDir ();
			return Directory.Exists (curDir) == false ? null : UtilsAsset.CreateAsset<T1> (curDir);
		}

		/// <summary>
		/// 创建设定文件（在当前目录）.
		/// </summary>
		protected static T1 CreateAsset () {	
			return UtilsAsset.CreateAsset<T1> ();	
		}

#endregion

	}
}
