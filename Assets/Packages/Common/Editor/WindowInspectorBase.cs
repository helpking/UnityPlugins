using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Packages.Common.Base;
using Packages.Common.Base.Editor;
using Packages.Common.Counter;
using Packages.Common.Extend.Editor;
using Packages.Utils;
 
 namespace Packages.Common.Editor
{

	/// <summary>
	/// 地形拆分进度更新委托
	/// </summary>
	/// <param name="iStatusTxt">状态更新文本</param>
	/// <param name="iStatusCount">状态计数</param>
	public delegate void ProgressBarUpdate(string iStatusTxt, bool iStatusCount = false);

	/// <summary>
	/// 编辑器窗口基类
	/// </summary>
	/// <typeparam name="T1"></typeparam>
	/// <typeparam name="T2"></typeparam>
	public abstract class EditorWindowBase<T1, T2> : EditorWindow
		where T1 : EditorWindow
		where T2 : JsonDataBase, new()
	{

		/// <summary>
		/// GUI绘制暂停标识位
		/// </summary>
		protected bool GUIDrawPause = false;
		
		/// <summary>
		/// 宽度
		/// </summary>
		private Vector2 lastSize;
		protected Vector2 Size;

		protected float Width => Size.x;
		protected float Height => Size.y;

		/// <summary>
		/// 窗口尺寸是否变化
		/// </summary>
		protected bool isSizeChanged => !(lastSize == Size);

		/// <summary>
		/// 拖拽Item
		/// </summary>
		protected class DraggedItem
		{
			public GameObject prefab;
			public string guid;
		}
	
		/// <summary>
		/// 显示模式
		/// </summary>
		protected enum DisplayMode
		{
			/// <summary>
			/// 袖珍模式
			/// </summary>
			CompactMode,
			/// <summary>
			/// Icon模式
			/// </summary>
			IconMode,
			/// <summary>
			/// 详细模式
			/// </summary>
			DetailedMode,
		}
		
		/// <summary>
		/// 类名.
		/// </summary>
		private string _className;
		protected string ClassName {
			get {
				if (string.IsNullOrEmpty (_className)) {
					_className = GetType ().Name;
				}
				return _className;
			}
		}

		/// <summary>
		/// 对象实例
		/// </summary>
		protected static T1 instance = default(T1);
		
#region Inspector - Preview

		/// <summary>
		/// Debug用节点
		/// </summary>
		private GameObject _debugNode = null;
		protected const string DebugNodeName = "Debug";
		protected GameObject DebugNode
		{
			get
			{
				if (null == _debugNode)
				{
					_debugNode = new GameObject(DebugNodeName);
				}
				return _debugNode;
			}
		}

		/// <summary>
		/// Debug用线节点
		/// </summary>
		protected GameObject _boundsNode = null;
		protected const string BoundsNodeName = "Bounds";
		protected GameObject BoundsNode
		{
			get
			{
				if (null != _boundsNode) return _boundsNode;
				_boundsNode = new GameObject(BoundsNodeName);
				_boundsNode.transform.parent = DebugNode.transform;
				return _boundsNode;
			}
		}

		/// <summary>
		/// 预览对象
		/// </summary>
		/// <param name="iData"></param>
		protected void PreViewGUI(TerrainData iData)
		{
			if(null == iData) return;
			//创建Editor 实例
			var editor = UnityEditor.Editor.CreateEditor(this, typeof(T1)); 
			//预览它
			editor.DrawPreview(GUILayoutUtility.GetRect(10, 100));
		}

#endregion
		
#region Inspector - Style

		protected bool reset = false;
		/// <summary>
		/// 基础单元格宽度
		/// </summary>
		protected const int BaseCellUnitWidth = 20;
		/// <summary>
		/// 基础单元格高度
		/// </summary>
		protected const int BaseCellUnitHeight = 20;
		
		// 按钮宽度
		protected const int shortBtnWidth = BaseCellUnitWidth;
		protected const int middleBtnWidth = BaseCellUnitWidth * 3;
		protected const int longBtnWidth = BaseCellUnitWidth * 5;
		
		// 按钮高度
		protected const int btnHeight = BaseCellUnitHeight;
		
		/// <summary>
		/// 基础单元格尺寸(等宽等高)
		/// </summary>
		protected const int cellSize = BaseCellUnitWidth * 2;
		protected const int BasePadding = 4;
		protected float _sizePercent = 0.5f;
		public float SizePercent
		{
			get { return _sizePercent; }
			set 
			{
				if (!(Math.Abs(_sizePercent - value) > 0)) return;
				reset = true;
				_sizePercent = value;
				//cellSize = Mathf.FloorToInt(80 * SizePercent + 10);
				EditorPrefs.SetFloat($"{ClassName}_sizePercent", _sizePercent);
			}
		}
		
		/// <summary>
		/// 风格
		/// </summary>
		protected GUIContent content = null;
		protected GUIStyle style = null;
		

#endregion

#region Inspector - DraggedObject

		/// <summary>
		/// 显示模式
		/// </summary>
		protected DisplayMode displayMode = DisplayMode.CompactMode;
		private Vector2 posTmp = Vector2.zero;
		
		/// <summary>
		/// 鼠标是否在范围内
		/// </summary>
		private bool isMouseInside = false;


		protected bool DraggedObjectIsOurs
		{
			get
			{
				var obj = DragAndDrop.GetGenericData(ClassName);
				if (obj == null) return false;
				return (bool)obj;
			}
			set
			{
				DragAndDrop.SetGenericData(ClassName, value);
			}
		}

		/// <summary>
		/// 被拖动的对象列表
		/// </summary>
		protected UnityEngine.Object[] DraggedObjects
		{
			get
			{
				if (DragAndDrop.objectReferences == null || DragAndDrop.objectReferences.Length == 0) 
					return null;
			
				return DragAndDrop.objectReferences.Where(x=>x as UnityEngine.Object).Cast<UnityEngine.Object>().ToArray();
			}
			set
			{
				if (value != null)
				{
					DragAndDrop.PrepareStartDrag();
					DragAndDrop.objectReferences = value;
					DraggedObjectIsOurs = true;
				}
				else DragAndDrop.AcceptDrag();
			}
		}

		/// <summary>
		/// 被拖动的游戏对象列表
		/// </summary>
		protected GameObject[] DraggedGameObjects
		{
			get
			{
				if (DragAndDrop.objectReferences == null || DragAndDrop.objectReferences.Length == 0) 
					return null;
			
				return DragAndDrop.objectReferences.Where(x=>x as GameObject).Cast<GameObject>().ToArray();
			}
			set
			{
				if (value != null)
				{
					DragAndDrop.PrepareStartDrag();
					DragAndDrop.objectReferences = value;
					DraggedObjectIsOurs = true;
				}
				else DragAndDrop.AcceptDrag();
			}
		}
		
		/// <summary>
		/// 被拖动的地形列表
		/// </summary>
		protected TerrainData[] DraggedTerrain
		{
			get
			{
				if (DragAndDrop.objectReferences == null || DragAndDrop.objectReferences.Length == 0) 
					return null;
			
				return DragAndDrop.objectReferences.Where(x=>x as TerrainData).Cast<TerrainData>().ToArray();
			}
			set
			{
				if (value != null)
				{
					DragAndDrop.PrepareStartDrag();
					DragAndDrop.objectReferences = value;
					DraggedObjectIsOurs = true;
				}
				else DragAndDrop.AcceptDrag();
			}
		}
		
		/// <summary>
		/// 取得鼠标下的单元索引
		/// </summary>
		/// <param name="spacingX">左余白</param>
		/// <param name="spacingY">下余白</param>
		/// <returns>-1：无cell在鼠标直线; 其他:在鼠标线的对象索引</returns>
		private int GetCellIndexUnderMouse (int spacingX, int spacingY)
		{
			var pos = Event.current.mousePosition + posTmp;
			// 居顶余白
			var topPadding = 24;
			
			// x轴和y轴方向上的余白
			int x = BasePadding, y = BasePadding + topPadding;
			// 若在编辑窗口顶部以上，则返回-1（表示无对象）
			if (pos.y < y) return -1;

			// 计算宽度
			var width = Screen.width - BasePadding + posTmp.x;
			// 计算高度
			var height = Screen.height - BasePadding + posTmp.y;
			var index = 0;
			// 遍历
			for (; ; ++index)
			{
				// 计算范围
				var rect = new Rect(x, y, spacingX, spacingY);
				// 范围内包含坐标，则打断遍历，并返回索引
				if (rect.Contains(pos)) break;

				// 继续累加余白
				x += spacingX;
				
				
				if (!(x + spacingX > width)) continue;
				if (pos.x > x) return -1;
				y += spacingY;
				x = BasePadding;
				if (y + spacingY > height) return -1;
			}
			return index;
		}

#endregion

		/// <summary>
		/// 配置数据
		/// </summary>
		protected T2 ConfData = new T2();
		
		protected static List<Light> lights;

		/// <summary>
		/// 重新设置光线
		/// </summary>
		protected static void RectivateLights ()
		{
			if (lights == null) return;
			foreach (var t in lights)
				t.enabled = true;
			lights = null;
		}

		void Load ()
		{
			SizePercent = EditorPrefs.GetFloat(string.Format("{0}_sizePercent", ClassName), 0.5f);

//			var data = EditorPrefs.GetString(saveKey, "");
			//data = "";//For test
//			if (string.IsNullOrEmpty(data))
//			{
//				Reset();
//			}
//			else
//			{
//				if (string.IsNullOrEmpty(data)) return;
//				var guids = data.Split('|');
//				foreach (var s in guids) AddGUID(s, -1);
//				RectivateLights();
//			}
		}
		
		void OnEnable ()
		{
			Load();

			content = new GUIContent();
			style = new GUIStyle();
			style.alignment = TextAnchor.MiddleCenter;
			style.padding = new RectOffset(2, 2, 2, 2);
			style.clipping = TextClipping.Clip;
			style.wordWrap = true;
			style.stretchWidth = false;
			style.stretchHeight = false;
			style.normal.textColor = 
				UnityEditor.EditorGUIUtility.isProSkin ? 
					new Color(1f, 1f, 1f, 0.5f) : new Color(0f, 0f, 0f, 0.5f);
			style.normal.background = null;

			// GUI.changed = true;
		}
		
		/// <summary>
		/// 绘制GUI
		/// </summary>
		void OnGUI()
		{
			// 保留原尺寸
			if (InitWindowsSizeInfo())
			{
				// 重置窗体大小
				ResetWindowsSizeWhenChanged();
			}

			// TODO : 拖拽预制体到编辑器扩展界面押后开发 
//			// 获取事件类型
//			var currentEvent = Event.current;
//			var eventType = currentEvent.type;
//			
////			// 位置&余白计算
////			int x = BasePadding, y = BasePadding;
////			var width = Screen.width - BasePadding;
////			var spacingX = cellSize + BasePadding;
////			var spacingY = spacingX;
////			if (displayMode == DisplayMode.DetailedMode)
////			{
////				spacingY += 32;
////			}
////			var draggeds = DraggedObjects;
////			var isDragging = draggeds != null;
////			var indexUnderMouse = GetCellIndexUnderMouse(spacingX, spacingY);
////			
////			// 判断当前是否拖拽有效
////			var eligibleToDrag = (currentEvent.mousePosition.y < Screen.height - 40);
////			switch (eventType)
////			{
////				case EventType.MouseDown:
////				case EventType.MouseDrag:
////
////				{
////					UtilsLog.Info("WindowEditor", 
////						"OnGUI():Drag:{0} {1} Spacint:X{2} Y{3} index:{4}",
////						isDragging, 
////						(null == draggeds) ? -1 : draggeds.Length,
////						spacingX, spacingY, indexUnderMouse);
////					if (indexUnderMouse != -1 && eligibleToDrag)
////					{
////						if (DraggedObjectIsOurs) DragAndDrop.StartDrag(ClassName);
////						currentEvent.Use();
////					}
////					isMouseInside = true;
////				}
////					break;
////				case EventType.MouseUp:
////					DragAndDrop.PrepareStartDrag();
////					isMouseInside = false;
////					Repaint();
////					break;
////				case EventType.DragUpdated:
////					isMouseInside = true;
//////				UpdateVisual();
////					currentEvent.Use();
////					break;
////				case EventType.DragPerform:
//////				if (draggeds != null)
//////				{
//////					if (_selections != null)
//////					{
//////						foreach (var selection in _selections)
//////						{
//////							DestroyTexture(selection);
//////							mItems.Remove(selection);
//////						}
//////					}
//////
//////					foreach (var dragged in draggeds)
//////					{
//////						AddItem(dragged, indexUnderMouse);
//////						++indexUnderMouse;
//////					}
//////				
//////					draggeds = null;
//////				}
////					isMouseInside = false;
////					currentEvent.Use();
////					break;
////				case EventType.DragExited:
////				case EventType.Ignore:
////					isMouseInside = false;
////					break;
////			}
////
////			if (!isMouseInside)
////			{
//////				_selections.Clear();
////				draggeds = null;
////
////			}
			
			// if(!GUI.changed) return;
			// 绘制WindowGUI
			OnWindowGui();
			
		}

		void OnInspectorUpdate()
		{
			Repaint();
		}

		/// <summary>
		/// 清空Debug调试节点
		/// </summary>
		/// <param name="iLayerIndex">层索引</param>
		protected virtual void ClearDebugNode(int iLayerIndex = -1)
		{
			if (!DebugNode) return;
			DestroyImmediate(DebugNode);
			_debugNode = null;
			_boundsNode = null;
			ClearDrawObjects(iLayerIndex);
		}

		protected virtual void ClearDrawObjects(int iTagIndex = -1)
		{
		}

		private void OnDestroy()
		{
			ClearDebugNode();
		}

		/// <summary>
		/// 初始化窗体尺寸大小
		/// </summary>
		/// <returns>true:窗体大小发生变化; false:窗体大小未发生变化;</returns>
		private bool InitWindowsSizeInfo()
		{
			lastSize = Size;
			// 宽度
			Size.x = Screen.width 
			         - BasePadding      // 左余白
			         - BasePadding;     // 右余白 
			// 高度
			Size.y = Screen.height 
			         - BasePadding      // 上余白
			         - BasePadding;     // 下余白
			
			return lastSize != Size;
		}

		/// <summary>
		/// 窗体变化时，重置窗体尺寸相关信息
		/// </summary>
		protected virtual void  ResetWindowsSizeWhenChanged()
		{
			
		}
		
#region Inspector - abstract

		/// <summary>
		/// 绘制WindowGUI.
		/// </summary>
		protected abstract void OnWindowGui ();
		
#endregion
	}

	/// <summary>
	/// 窗口基类 - Inspector
	/// </summary>
	public abstract class WindowInspectorBase<T1, T2> 
		: EditorWindowBase<T1, T2>, ISerializationCallbackReceiver
		where T1 : EditorWindow
		where T2 : JsonDataBase, new()
	{

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
			ConfData?.Clear ();
		}

		private bool firstOpen = true;

#region SerializationCallback

		public virtual void OnBeforeSerialize()
		{
			
		}

		public virtual void OnAfterDeserialize()
		{
			
		}

#endregion

#region Inspector - ClickEvent


		
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

#endregion

#region Inspector - virtual

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

			var jsonData = UtilsAsset.ImportDataByDir<T2> (out _, importDir);
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
				return UtilsAsset.ExportData<T2>(ConfData, exportDir);
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
				UtilsAsset.DeleteFile<T2> ();
			}

			UtilsAsset.SetAssetDirty (this);
		}

#endregion

#region Inspector - ShowWindow
		/// <summary>
		/// 显示窗口
		/// </summary>
		/// <param name="iName">窗口名字</param>
		/// <param name="iForceOpen">强制打开标识位。(默认：true)</param>
		protected static void ShowWindow(string iName, bool iForceOpen = true)
		{
			//opening if force open
			if (iForceOpen)
				instance = (T1)GetWindow (typeof (T1), false, iName);
			
			//finding instance
			if (instance != default(T1)) return;
			var windows = Resources.FindObjectsOfTypeAll<T1>();
			if (0 == windows.Length) return;
			instance = windows[0];
			
			// 重新绘制窗口
			instance.autoRepaintOnSceneChange = true;
			instance.Show();
			instance.Repaint();
		}
		
		/// <summary>
		/// 绘制WindowGUI.
		/// </summary>
		protected override void OnWindowGui()
		{
			// 初始化顶部按钮信息
			EditorGUILayout.BeginHorizontal ();
			if(GUILayout.Button("Clear",GUILayout.Width(80)))
			{
				OnClearClick();
			}
			if(GUILayout.Button("Import",GUILayout.Width(80)))
			{
				OnImportClick();
			}
			if(GUILayout.Button("Export",GUILayout.Width(80)))
			{
				OnExportClick();
			}
			EditorGUILayout.EndHorizontal ();
		}
		
#endregion

#region Inspector - Draw - Group

		/// <summary>
		/// 绘制Group
		/// </summary>
		/// <param name="iPos">开始位置</param>
		/// <param name="iWidth">宽度</param>
		/// <param name="iHeight">高度</param>
		/// <param name="iDrawContent">绘制内容委托</param>
		/// <param name="iStyleName">风格名</param>
		/// <param name="iContentOffsetX">内容偏移X</param>
		/// <param name="iContentOffsetY">内容偏移Y</param>
		/// <returns>绘制范围</returns>
		protected Rect DrawGroup(
			Vector2 iPos, float iWidth, float iHeight,
			Action iDrawContent = null, string iStyleName = null, 
			float iContentOffsetX = 0.0f, float iContentOffsetY = 0.0f)
		{
			var rect = new Rect(iPos.x, iPos.y, iWidth, iHeight);

			if (string.IsNullOrEmpty(iStyleName))
			{
				GUI.BeginGroup(rect);
				iDrawContent?.Invoke();
				GUI.EndGroup();
			}
			else
			{
				var groupStyle = GUIEditorHelper.CloneStyle(iStyleName);
				groupStyle.contentOffset = new Vector2(iContentOffsetX, iContentOffsetY);
				GUI.BeginGroup(rect, groupStyle);
				iDrawContent?.Invoke();
				GUI.EndGroup();
			}
			
			
			return rect;
		}

#endregion

#region Inspector - Draw - Space

		/// <summary>
		/// 绘制空白
		/// </summary>
		/// <param name="iLevel">层级(影响到左边缩进的余白)</param>
		/// <param name="iSpaceBegin">开始空白宽度</param>
		/// <param name="iHeight">高度</param>
		/// <param name="iSpaceEnd">结束空白宽度</param>
		public void DrawSpacer(int iLevel, 
			float iSpaceBegin = 5, float iHeight = 5, float iSpaceEnd = 5)
		{
			// 设定缩进
			for (var i = 0; i < iLevel; ++i)
			{
				++EditorGUI.indentLevel;
			}
			
			GUILayout.Space(iSpaceBegin - 1);
			EditorGUILayout.BeginHorizontal();
			GUI.color = new Color(0.5f, 0.5f, 0.5f, 1);
			GUILayout.Button("", GUILayout.Height(iHeight));
			EditorGUILayout.EndHorizontal();
			GUILayout.Space(iSpaceEnd);
			GUI.color = Color.white;
			
			// 恢复缩进
			for (var i = 0; i < iLevel; ++i)
			{
				--EditorGUI.indentLevel;
			}
		}

#endregion

#region Inspector - Draw - Label

		/// <summary>
		/// 标签宽度：无限制
		/// </summary>
		protected const int LabelNoneWidth = -1;
		/// <summary>
		/// 标签宽度：短
		/// </summary>
		protected const int LabelShortWidth = 50;
		/// <summary>
		/// 标签宽度：中
		/// </summary>
		protected const int LabelMiddleWidth = 100;
		/// <summary>
		/// 标签宽度：长
		/// </summary>
		protected const int LabelLongWidth = 150;
		/// <summary>
		/// 按钮宽度：无限制
		/// </summary>
		protected const int ButtonNoneWidth = -1;
		/// <summary>
		/// 按钮宽度：短
		/// </summary>
		protected const int ButtonShortWidth = 100;
		/// <summary>
		/// 按钮宽度：中
		/// </summary>
		protected const int ButtonMiddleWidth = 150;
		/// <summary>
		/// 按钮宽度：长
		/// </summary>
		protected const int ButtonLongWidth = 200;
		/// <summary>
		/// 行高：无限制
		/// </summary>
		protected const int RowNoneHeight = -1;
		/// <summary>
		/// 行高：矮
		/// </summary>
		protected const int RowLowHeight = 12;
		/// <summary>
		/// 行高：中
		/// </summary>
		protected const int RowMiddleHeight = 16;
		/// <summary>
		/// 行高：高
		/// </summary>
		protected const int RowHightHeight = 24;
		
		/// <summary>
		/// 绘制标签（默认：长度无限制，高度无限制）
		/// </summary>
		/// <param name="iLevel">层级(影响到左边缩进的余白)</param>
		/// <param name="iText">标签文本</param>
		/// <param name="iColor">标签文本颜色</param>
		/// <param name="iWidth">宽度</param>
		/// <param name="iHeight">高度</param>
		protected void DrawLabel(
			int iLevel, string iText, Color iColor,
			int iWidth = LabelNoneWidth, int iHeight = RowNoneHeight)
		{
			// 设定缩进
			for (var i = 0; i < iLevel; ++i)
			{
				++EditorGUI.indentLevel;
			}

			EditorGUILayout.BeginHorizontal ();
			using (new ColorScope(iColor))
			{
				if (LabelNoneWidth == iWidth && RowNoneHeight == iHeight)
				{
					EditorGUILayout.LabelField (iText);
				} else if (LabelNoneWidth == iWidth)
				{
					EditorGUILayout.LabelField (iText, 
						GUILayout.Height(iHeight));
				} else if (RowNoneHeight == iHeight)
				{
					EditorGUILayout.LabelField (iText, 
						GUILayout.MaxWidth(iWidth));
				}
				else
				{
					EditorGUILayout.LabelField (iText, 
						GUILayout.MaxWidth(iWidth),
						GUILayout.Height(iHeight));
				}
			}
			EditorGUILayout.EndHorizontal();
			
			// 恢复缩进
			for (var i = 0; i < iLevel; ++i)
			{
				--EditorGUI.indentLevel;
			}

		}
		
		/// <summary>
		/// 绘制标签（长度短，高度中）
		/// </summary>
		/// <param name="iLevel">层级(影响到左边缩进的余白)</param>
		/// <param name="iText">标签文本</param>
		protected void DrawShortLabel(int iLevel, string iText)
		{
			DrawLabel(iLevel, iText, Color.white, LabelShortWidth);
		}
		
		/// <summary>
		/// 绘制标签（长度中，高度中）
		/// </summary>
		/// <param name="iLevel">层级(影响到左边缩进的余白)</param>
		/// <param name="iText">标签文本</param>
		protected void DrawMiddleLabel(int iLevel, string iText)
		{
			DrawLabel(iLevel, iText, Color.white);
		}
		
		/// <summary>
		/// 绘制标签（长度中，高度中）
		/// </summary>
		/// <param name="iLevel">层级(影响到左边缩进的余白)</param>
		/// <param name="iText">标签文本</param>
		protected void DrawLongLabel(int iLevel, string iText)
		{
			DrawLabel(iLevel, iText, Color.white, LabelLongWidth);
		}
		
		/// <summary>
		/// 绘制文本
		/// </summary>
		/// <param name="iPos">开始位置</param>
		/// <param name="iWidth">宽度</param>
		/// <param name="iHeight">高度</param>
		/// <param name="iLabel">文本</param>
		/// <param name="iStyleName">风格名</param>
		/// <param name="iFontSize">字体大小</param>
		/// <param name="iTextAnchor">锚点风格</param>
		/// <param name="iPadding">余白</param>
		/// <param name="iBorder">边框厚度</param>
		/// <param name="iMargin">页边空白</param>
		/// <param name="iOverflow">页边溢出</param>
		/// <param name="iContentOffset">相对偏移</param>
		/// <param name="iAccumulation">累加标志位</param>
		/// <returns>绘制范围</returns>
		protected Rect DrawLabel(
			Vector2 iPos, float iWidth, float iHeight,
			string iLabel, string iStyleName, 
			int iFontSize, TextAnchor iTextAnchor, 
			RectOffset iPadding, RectOffset iBorder, 
			RectOffset iMargin, RectOffset iOverflow, 
			Vector2 iContentOffset, bool iAccumulation = false)
		{
			var displayRect = new Rect(iPos.x, iPos.y, iWidth, iHeight);
			if (!string.IsNullOrEmpty(iStyleName))
			{
				var guiStyle = GUIEditorHelper.CloneStyle(iStyleName, iWidth, iHeight);
				guiStyle.alignment = iTextAnchor;
				if (iAccumulation)
				{
					guiStyle.contentOffset = new Vector2(
						guiStyle.contentOffset.x + iContentOffset.x,
						guiStyle.contentOffset.y + iContentOffset.y);

					guiStyle.padding.left += iPadding.left;
					guiStyle.padding.right += iPadding.right;
					guiStyle.padding.top += iPadding.top;
					guiStyle.padding.bottom += iPadding.bottom;
				
					guiStyle.border.left += iBorder.left;
					guiStyle.border.right += iBorder.right;
					guiStyle.border.top += iBorder.top;
					guiStyle.border.bottom += iBorder.bottom;
				
					guiStyle.margin.left += iMargin.left;
					guiStyle.margin.right += iMargin.right;
					guiStyle.margin.top += iMargin.top;
					guiStyle.margin.bottom += iMargin.bottom;
				
					guiStyle.overflow.left += iOverflow.left;
					guiStyle.overflow.right += iOverflow.right;
					guiStyle.overflow.top += iOverflow.top;
					guiStyle.overflow.bottom += iOverflow.bottom;
					
					guiStyle.richText = true;
				}
				else
				{
					guiStyle.contentOffset = iContentOffset;
					
					guiStyle.padding = iPadding;
					guiStyle.border = iBorder;
					guiStyle.margin = iMargin;
					guiStyle.overflow = iOverflow;
				}
				
				guiStyle.fontSize = iFontSize;
				GUI.Label(displayRect, iLabel, guiStyle);
			}
			else
			{
				GUI.Label(displayRect, iLabel);
			}
			
			return displayRect;
		}

		/// <summary>
		/// 绘制文本
		/// </summary>
		/// <param name="iPos">开始位置</param>
		/// <param name="iWidth">宽度</param>
		/// <param name="iHeight">高度</param>
		/// <param name="iLabel">文本</param>
		/// <param name="iStyleName">风格名</param>
		/// <param name="iFontSize">字体大小</param>
		/// <param name="iTextAnchor">锚点风格</param>
		/// <param name="iContentOffsetX">内容偏移X</param>
		/// <param name="iContentOffsetY">内容偏移Y</param>
		/// <param name="iPaddingLeft">余白：左</param>
		/// <param name="iPaddingRight">余白：右</param>
		/// <param name="iPaddingTop">余白：顶部</param>
		/// <param name="iPaddingBottom">余白：底部</param>
		/// <param name="iAccumulation">累加标志位</param>
		/// <returns>绘制范围</returns>
		protected Rect DrawLabel(
			Vector2 iPos, float iWidth, float iHeight, string iLabel,
			string iStyleName = null, int iFontSize = 14,
			TextAnchor iTextAnchor = TextAnchor.MiddleCenter, 
			int iContentOffsetX = 0, int iContentOffsetY = 0,
			int iPaddingLeft = 0, int iPaddingRight = 0, 
			int iPaddingTop = 0, int iPaddingBottom = 0,
			bool iAccumulation = false)
		{
			return DrawLabel(
				iPos, iWidth, iHeight, iLabel, iStyleName, iFontSize, iTextAnchor,
				new RectOffset(iPaddingLeft, iPaddingRight, iPaddingTop, iPaddingBottom),
				new RectOffset(0, 0, 0,0), 
				new RectOffset(0, 0, 0,0),
				new RectOffset(0, 0, 0,0), 
				new Vector2(iContentOffsetX, iContentOffsetY), iAccumulation);
		}

#endregion

#region Inspector - Draw - IntField

		/// <summary>
		/// 绘制:整型(默认:标签长度无限制，高度无限制)
		/// </summary>
		/// <param name="iLevel">层级(影响到左边缩进的余白)</param>
		/// <param name="iLabelText">标签文本</param>
		/// <param name="iValue">值</param>
		/// <param name="iLabelWidth">文本宽度</param>
		/// <param name="iHeight">高度</param>
		protected void DrawIntField(
			int iLevel, string iLabelText, ref int iValue,
			int iLabelWidth = LabelNoneWidth, int iHeight = RowNoneHeight)
		{
			// 设定缩进
			for (var i = 0; i < iLevel; ++i)
			{
				++EditorGUI.indentLevel;
			}
			
			EditorGUILayout.BeginHorizontal ();
			if (LabelNoneWidth == iLabelWidth && RowNoneHeight == iHeight)
			{
				iValue = EditorGUILayout.IntField(iLabelText, iValue);
			} else if (LabelNoneWidth == iLabelWidth)
			{
				iValue = EditorGUILayout.IntField(iLabelText, iValue, 
					GUILayout.Height(iHeight));
			} else if (RowNoneHeight == iHeight)
			{
				iValue = EditorGUILayout.IntField(iLabelText, iValue, 
					GUILayout.MaxWidth(iLabelWidth));
			} else {
				iValue = EditorGUILayout.IntField(iLabelText, iValue, 
					GUILayout.MaxWidth(iLabelWidth), GUILayout.Height(iHeight));
			}
			
			EditorGUILayout.EndHorizontal();
			
			// 恢复缩进
			for (var i = 0; i < iLevel; ++i)
			{
				--EditorGUI.indentLevel;
			}
			
		}

		/// <summary>
		/// 绘制:整型(默认:标签长度无限制，高度无限制)
		/// </summary>
		/// <param name="iLevel">层级(影响到左边缩进的余白)</param>
		/// <param name="iLabelText">标签文本</param>
		/// <param name="iValue">值</param>
		protected void DrawShortIntField(
			int iLevel, string iLabelText, ref int iValue)
		{
			DrawIntField(iLevel, iLabelText, ref iValue, LabelShortWidth);
		}
		
		/// <summary>
		/// 绘制:整型(默认:标签长度无限制，高度无限制)
		/// </summary>
		/// <param name="iLevel">层级(影响到左边缩进的余白)</param>
		/// <param name="iLabelText">标签文本</param>
		/// <param name="iValue">值</param>
		protected void DrawMiddleIntField(
			int iLevel, string iLabelText, ref int iValue)
		{
			DrawIntField(iLevel, iLabelText, ref iValue, LabelMiddleWidth);
		}
		
		/// <summary>
		/// 绘制:整型(默认:标签长度无限制，高度无限制)
		/// </summary>
		/// <param name="iLevel">层级(影响到左边缩进的余白)</param>
		/// <param name="iLabelText">标签文本</param>
		/// <param name="iValue">值</param>
		protected void DrawLongIntField(
			int iLevel, string iLabelText, ref int iValue)
		{
			DrawIntField(iLevel, iLabelText, ref iValue, LabelLongWidth);
		}

		/// <summary>
		/// 绘制Int输入框
		/// </summary>
		/// <param name="iPos">开始位置</param>
		/// <param name="iWidth">宽度</param>
		/// <param name="iHeight">高度</param>
		/// <param name="iValue">值</param>
		/// <param name="iLabelText">文本标签</param>
		/// <param name="iStyleName">风格名</param>
		/// <returns></returns>
		protected Rect DrawIntField(Vector2 iPos, float iWidth, float iHeight,
			ref int iValue, string iLabelText = null, string iStyleName = null)
		{
			var rect = new Rect(iPos.x, iPos.y, iWidth, iHeight);
			if (string.IsNullOrEmpty(iLabelText))
			{
				if (string.IsNullOrEmpty(iStyleName))
				{
					iValue = EditorGUI.IntField(rect, iValue);
				}
				else
				{
					var guiStyle = GUIEditorHelper.CloneStyle(iStyleName, iWidth, iHeight);
					iValue = EditorGUI.IntField(rect, iValue, guiStyle);
				}
				
			}
			else
			{
				if (string.IsNullOrEmpty(iStyleName))
				{
					iValue = EditorGUI.IntField(rect, iLabelText, iValue);
				}
				else
				{
					var guiStyle = GUIEditorHelper.CloneStyle(iStyleName, iWidth, iHeight);
					iValue = EditorGUI.IntField(rect, iLabelText, iValue, guiStyle);
				}
				
			}
			
			return rect;
		}
		
#endregion

#region Inspector - Draw - TextField

		/// <summary>
		/// 绘制:文本(默认:标签长度无限制，高度无限制)
		/// </summary>
		/// <param name="iLevel">层级(影响到左边缩进的余白)</param>
		/// <param name="iLabelText">标签文本</param>
		/// <param name="iValue">值</param>
		/// <param name="iLabelWidth">文本宽度</param>
		/// <param name="iHeight">高度</param>
		protected string DrawTextField(
			int iLevel, string iLabelText, string iValue,
			int iLabelWidth = LabelNoneWidth, int iHeight = RowNoneHeight)
		{
			// 设定缩进
			for (var i = 0; i < iLevel; ++i)
			{
				++EditorGUI.indentLevel;
			}

			var ret = iValue;
			EditorGUILayout.BeginHorizontal ();
			if (LabelNoneWidth == iLabelWidth && RowNoneHeight == iHeight)
			{
				ret = EditorGUILayout.TextField(iLabelText, iValue);
			} else if (LabelNoneWidth == iLabelWidth)
			{
				ret = EditorGUILayout.TextField(iLabelText, iValue, 
					GUILayout.Height(iHeight));
			} else if (RowNoneHeight == iHeight)
			{
				ret = EditorGUILayout.TextField(iLabelText, iValue, 
					GUILayout.MaxWidth(iLabelWidth));
			} else {
				ret = EditorGUILayout.TextField(iLabelText, iValue, 
					GUILayout.MaxWidth(iLabelWidth), GUILayout.Height(iHeight));
			}
					
			EditorGUILayout.EndHorizontal();
					
			// 恢复缩进
			for (var i = 0; i < iLevel; ++i)
			{
				--EditorGUI.indentLevel;
			}
			return ret;
		}
		
		/// <summary>
		/// 绘制:文本(默认:标签长度无限制，高度无限制)
		/// </summary>
		/// <param name="iLevel">层级(影响到左边缩进的余白)</param>
		/// <param name="iLabelText">标签文本</param>
		/// <param name="iValue">值</param>
		/// <param name="iButtonCaption">按钮名</param>
		/// <param name="iOnButtonClick">按钮点击事件</param>
		/// <param name="iLabelWidth">文本宽度</param>
		/// <param name="iButtonWidth">按钮宽度</param>
		/// <param name="iHeight">高度</param>
		protected string DrawTextAndButtonField(
			int iLevel, string iLabelText, string iValue, 
			string iButtonCaption, Action<string> iOnButtonClick,
			int iLabelWidth = LabelNoneWidth, int iButtonWidth = ButtonNoneWidth, 
			int iHeight = RowNoneHeight)
		{
			// 设定缩进
			for (var i = 0; i < iLevel; ++i)
			{
				++EditorGUI.indentLevel;
			}

			string ret;
			Rect displayRect;
			EditorGUILayout.BeginHorizontal ();
			if (LabelNoneWidth == iLabelWidth && RowNoneHeight == iHeight)
			{
				displayRect = EditorGUILayout.GetControlRect();
			} else if (LabelNoneWidth == iLabelWidth)
			{
				displayRect = EditorGUILayout.GetControlRect(
					GUILayout.Height(iHeight));
			} else if (RowNoneHeight == iHeight)
			{
				displayRect = EditorGUILayout.GetControlRect(
					GUILayout.MaxWidth(iLabelWidth));
			} else {
				displayRect = EditorGUILayout.GetControlRect(
					GUILayout.MaxWidth(iLabelWidth), GUILayout.Height(iHeight));
			}
			ret = EditorGUI.TextField(displayRect, iLabelText, iValue);
			// 拖拽物体到制定当前文本输入框时
			if ((Event.current.type == EventType.DragUpdated
			     || Event.current.type == EventType.DragExited)
			    && displayRect.Contains(Event.current.mousePosition))
			{
				DragAndDrop.visualMode = DragAndDropVisualMode.Generic;
				if (DragAndDrop.paths != null && DragAndDrop.paths.Length > 0)
				{
					ret = DragAndDrop.paths[0];
				}
			}
			
			if (GUILayout.Button(iButtonCaption,
				GUILayout.Width(ButtonNoneWidth == iButtonWidth ? 50 : iButtonWidth)))
			{
				iOnButtonClick?.Invoke(ret);
			}
					
			EditorGUILayout.EndHorizontal();
					
			// 恢复缩进
			for (var i = 0; i < iLevel; ++i)
			{
				--EditorGUI.indentLevel;
			}

			return ret;
		}

		/// <summary>
		/// 绘制文本输入框
		/// </summary>
		/// <param name="iPos">开始位置</param>
		/// <param name="iWidth">宽度</param>
		/// <param name="iHeight">高度</param>
		/// <param name="iText">文本</param>
		/// <param name="iStyleName">风格名</param>
		/// <param name="iTextAnchor">锚点风格</param>
		/// <param name="iPaddingLeft">余白：左</param>
		/// <param name="iPaddingRight">余白：右</param>
		/// <param name="iPaddingTop">余白：顶部</param>
		/// <param name="iPaddingBottom">余白：底部</param>
		/// <returns>绘制范围</returns>
		protected Rect DrawTextField(
			Vector2 iPos, float iWidth, float iHeight,
			ref string iText, string iStyleName = null,
			TextAnchor iTextAnchor = TextAnchor.MiddleCenter,
			int iPaddingLeft = 0, int iPaddingRight = 0,
			int iPaddingTop = 0, int iPaddingBottom = 0
		)
		{
			var rect = new Rect(iPos.x, iPos.y, iWidth, iHeight);
			if (!string.IsNullOrEmpty(iStyleName))
			{
				var guiStyle = GUIEditorHelper.CloneStyle("SearchTextField");
				guiStyle.alignment = iTextAnchor;
				guiStyle.padding.left += iPaddingLeft;
				guiStyle.padding.right += iPaddingRight;
				guiStyle.padding.top += iPaddingTop;
				guiStyle.padding.bottom += iPaddingBottom;
				iText = GUI.TextField(rect, iText, guiStyle);
			}
			else
			{
				iText = GUI.TextField(rect, iText);
			}
			
			return rect;
		}

#endregion

#region Inspector - Draw - String - SelectList

		/// <summary>
		/// 绘制下拉菜单（长度无，高度无）
		/// </summary>
		/// <param name="iLevel">层级(影响到左边缩进的余白)</param>
		/// <param name="iTitle">标题</param>
		/// <param name="iListTexts">下拉菜单列表</param>
		/// <param name="iSelectedIndex">下拉菜单选中索引</param>
		/// <param name="iLabelWidth">文本宽度</param>
		/// <param name="iHeight">高度</param>
		protected void DrawSelectList(
			int iLevel, string iTitle, string[] iListTexts, 
			ref int iSelectedIndex,
			int iLabelWidth = LabelNoneWidth, int iHeight = RowNoneHeight)
		{
			// 设定缩进
			for (var i = 0; i < iLevel; ++i)
			{
				++EditorGUI.indentLevel;
			}
			EditorGUILayout.BeginHorizontal ();
			if (LabelNoneWidth == iLabelWidth && RowNoneHeight == iHeight)
			{
				iSelectedIndex = EditorGUILayout.Popup(
					iTitle, iSelectedIndex, iListTexts);
			} else if (LabelNoneWidth == iLabelWidth)
			{
				iSelectedIndex = EditorGUILayout.Popup(
					iTitle, iSelectedIndex, iListTexts,
					GUILayout.Height(iHeight));
			} else if (RowNoneHeight == iHeight)
			{
				iSelectedIndex = EditorGUILayout.Popup(
					iTitle, iSelectedIndex, iListTexts,
					GUILayout.Width(iLabelWidth));
			}
			else
			{
				iSelectedIndex = EditorGUILayout.Popup(
					iTitle, iSelectedIndex, iListTexts,
					GUILayout.Width(iLabelWidth), GUILayout.Height(iHeight));
			}
			EditorGUILayout.EndHorizontal();
			
			// 恢复缩进
			for (var i = 0; i < iLevel; ++i)
			{
				--EditorGUI.indentLevel;
			}
		}

		/// <summary>
		/// 绘制下拉菜单（长度短，高度中）
		/// </summary>
		/// <param name="iLevel">层级(影响到左边缩进的余白)</param>
		/// <param name="iTitle">标题</param>
		/// <param name="iListTexts">下拉菜单列表</param>
		/// <param name="iSelectedIndex">下拉菜单选中索引</param>
		protected void DrawShortSelectList(
			int iLevel, string iTitle, string[] iListTexts,
			ref int iSelectedIndex)
		{
			DrawSelectList(iLevel, iTitle, iListTexts, ref iSelectedIndex,
				LabelShortWidth, RowMiddleHeight);
		}

		/// <summary>
		/// 绘制下拉菜单（长度中，高度中）
		/// </summary>
		/// <param name="iLevel">层级(影响到左边缩进的余白)</param>
		/// <param name="iTitle">标题</param>
		/// <param name="iListTexts">下拉菜单列表</param>
		/// <param name="iSelectedIndex">下拉菜单选中索引</param>
		protected void DrawMiddleSelectList(
			int iLevel, string iTitle, string[] iListTexts,
			ref int iSelectedIndex)
		{
			DrawSelectList(iLevel, iTitle, iListTexts, ref iSelectedIndex,
				LabelMiddleWidth, RowMiddleHeight);
		}

		/// <summary>
		/// 绘制下拉菜单（长度长，高度中）
		/// </summary>
		/// <param name="iLevel">层级(影响到左边缩进的余白)</param>
		/// <param name="iTitle">标题</param>
		/// <param name="iListTexts">下拉菜单列表</param>
		/// <param name="iSelectedIndex">下拉菜单选中索引</param>
		protected void DrawLongSelectList(
			int iLevel, string iTitle, string[] iListTexts,
			ref int iSelectedIndex)
		{
			DrawSelectList(iLevel, iTitle, iListTexts, ref iSelectedIndex,
				LabelLongWidth, RowMiddleHeight);
		}

		/// <summary>
		/// 取得选中索引
		/// </summary>
		/// <param name="iCurText">当前文本</param>
		/// <param name="iSelectList">下来列表</param>
		/// <returns>选中索引</returns>
		private int GetSelectedIndex(string iCurText, string[] iSelectList)
		{
			var selected = -1;
			if (string.IsNullOrEmpty(iCurText) ||
			    null == iSelectList ||
			    0 >= iSelectList.Length)
			{
				return selected;
			}

			for (var i = 0; i < iSelectList.Length; i++)
			{
				if (!iCurText.Equals(iSelectList[i])) continue;
				selected = i;
				break;
			}
			return selected;
		}

		/// <summary>
		/// 绘制下拉菜单（长度无，高度无）
		/// </summary>
		/// <param name="iLevel">层级(影响到左边缩进的余白)</param>
		/// <param name="iTitle">标题</param>
		/// <param name="iListTexts">下拉菜单列表</param>
		/// <param name="iSelectedText">下拉菜单选中文本</param>
		/// <param name="iLabelWidth">文本宽度</param>
		/// <param name="iHeight">高度</param>
		protected void DrawSelectList(
			int iLevel, string iTitle, string[] iListTexts,
			ref string iSelectedText,
			int iLabelWidth = LabelNoneWidth, int iHeight = RowNoneHeight)
		{
			var selected = GetSelectedIndex(iSelectedText, iListTexts);
			var lastSelected = selected;
			DrawSelectList(iLevel, iTitle, iListTexts, ref selected, iLabelWidth, iHeight);
			if (lastSelected != selected)
			{
				iSelectedText = iListTexts[selected];
			}
		}

		/// <summary>
		/// 绘制下拉菜单（长度短，高度中）
		/// </summary>
		/// <param name="iLevel">层级(影响到左边缩进的余白)</param>
		/// <param name="iTitle">标题</param>
		/// <param name="iListTexts">下拉菜单列表</param>
		/// <param name="iSelectedText">下拉菜单选中文本</param>
		protected void DrawShortSelectList(
			int iLevel, string iTitle, string[] iListTexts,
			ref string iSelectedText)
		{
			DrawSelectList(iLevel, iTitle, iListTexts, ref iSelectedText,
				LabelShortWidth, RowMiddleHeight);
		}

		/// <summary>
		/// 绘制下拉菜单（长度中，高度中）
		/// </summary>
		/// <param name="iLevel">层级(影响到左边缩进的余白)</param>
		/// <param name="iTitle">标题</param>
		/// <param name="iListTexts">下拉菜单列表</param>
		/// <param name="iSelectedText">下拉菜单选中文本</param>
		protected void DrawMiddleSelectList(
			int iLevel, string iTitle, string[] iListTexts,
			ref string iSelectedText)
		{
			DrawSelectList(iLevel, iTitle, iListTexts, ref iSelectedText,
				LabelMiddleWidth, RowMiddleHeight);
		}

		/// <summary>
		/// 绘制下拉菜单（长度长，高度中）
		/// </summary>
		/// <param name="iLevel">层级(影响到左边缩进的余白)</param>
		/// <param name="iTitle">标题</param>
		/// <param name="iListTexts">下拉菜单列表</param>
		/// <param name="iSelectedText">下拉菜单选中文本</param>
		protected void DrawLongSelectList(
			int iLevel, string iTitle, string[] iListTexts,
			ref string iSelectedText)
		{
			DrawSelectList(iLevel, iTitle, iListTexts, ref iSelectedText,
				LabelLongWidth, RowMiddleHeight);
		}

		/// <summary>
		/// 绘制选择列表
		/// </summary>
		/// <param name="iPos">开始位置</param>
		/// <param name="iWidth">宽度</param>
		/// <param name="iHeight">高度</param>
		/// <param name="iListTexts">文本列表</param>
		/// <param name="iSelectedValue">选中值</param>
		/// <param name="iLabelText">标签</param>
		/// <param name="iStyleName">风格名</param>
		/// <returns>绘制范围</returns>
		protected Rect DrawSelectList(Vector2 iPos, float iWidth, float iHeight,
			string[] iListTexts, ref int iSelectedValue, 
			string iLabelText = null, string iStyleName = null)
		{
			var rect = new Rect(iPos.x, iPos.y, iWidth, iHeight);
			if (string.IsNullOrEmpty(iLabelText))
			{
				if (string.IsNullOrEmpty(iStyleName))
				{
					iSelectedValue = EditorGUI.Popup(rect, iSelectedValue, iListTexts);
				}
				else
				{
					var guiStyle = GUIEditorHelper.CloneStyle(iStyleName, iWidth, iHeight);
					iSelectedValue = EditorGUI.Popup(rect, iSelectedValue, iListTexts, guiStyle);
				}
				
			}
			else
			{
				if (string.IsNullOrEmpty(iStyleName))
				{
					iSelectedValue = EditorGUI.Popup(rect, iLabelText, iSelectedValue, iListTexts);
				}
				else
				{
					var guiStyle = GUIEditorHelper.CloneStyle(iStyleName, iWidth, iHeight);
					iSelectedValue = EditorGUI.Popup(rect, iLabelText, iSelectedValue, iListTexts, guiStyle);
				}
				
			}
			
			return rect;
		}

		/// <summary>
		/// 绘制选择列表
		/// </summary>
		/// <param name="iPos">开始位置</param>
		/// <param name="iWidth">宽度</param>
		/// <param name="iHeight">高度</param>
		/// <param name="iListValues">列表</param>
		/// <param name="iSelectedValue">选中值</param>
		/// <param name="iLabelText">标签</param>
		/// <param name="iStyleName">风格名</param>
		/// <returns>绘制范围</returns>
		protected Rect DrawSelectList(Vector2 iPos, float iWidth, float iHeight,
			int[] iListValues, ref int iSelectedValue,
			string iLabelText = null, string iStyleName = null)
		{
			
			var rect = new Rect(iPos.x, iPos.y, iWidth, iHeight);
			if (0 >= iListValues.Length) return rect;
			var labelTexts = new string[iListValues.Length];
			for (var idx = 0; idx < iListValues.Length; ++idx)
			{
				labelTexts[idx] = $"{iListValues[idx]}";
			}
			return DrawSelectList(
				rect.position, iWidth, iHeight, labelTexts,
				ref iSelectedValue, iLabelText, iStyleName);
		}
		
		/// <summary>
		/// 绘制选择列表 - 枚举体
		/// </summary>
		/// <param name="iPos">开始位置</param>
		/// <param name="iWidth">宽度</param>
		/// <param name="iHeight">高度</param>
		/// <param name="iEnumValue">枚举体值</param>
		/// <param name="iRect">绘制范围</param>
		/// <param name="iLabelText"></param>
		/// <param name="iStyleName"></param>
		/// <returns>选中值</returns>
		protected Enum DrawSelectList(Vector2 iPos, float iWidth, float iHeight,
			Enum iEnumValue, ref Rect iRect, string iLabelText = null, string iStyleName = null)
		{
			iRect = new Rect(iPos.x, iPos.y, iWidth, iHeight);
			Enum selected;
			if (string.IsNullOrEmpty(iLabelText))
			{
				if (string.IsNullOrEmpty(iStyleName))
				{
					selected = EditorGUI.EnumPopup(iRect, iEnumValue);
				}
				else
				{
					var guiStyle = GUIEditorHelper.CloneStyle(iStyleName, iWidth, iHeight);
					selected = EditorGUI.EnumPopup(iRect, iEnumValue, guiStyle);
				}
				
			}
			else
			{
				if (string.IsNullOrEmpty(iStyleName))
				{
					selected = EditorGUI.EnumPopup(iRect, iLabelText, iEnumValue);
				}
				else
				{
					var guiStyle = GUIEditorHelper.CloneStyle(iStyleName, iWidth, iHeight);
					selected = EditorGUI.EnumPopup(iRect, iLabelText, iEnumValue, guiStyle);
				}
				
			}
			return selected;
		}

#endregion

#region Inspector - Draw - Slider

		/// <summary>
		/// 绘制滑杆:整型(默认:标签长度无限制，高度无限制)
		/// </summary>
		/// <param name="iLevel">层级(影响到左边缩进的余白)</param>
		/// <param name="iLabelText">标签文本</param>
		/// <param name="iValue">值</param>
		/// <param name="iMinVale">最小值</param>
		/// <param name="iMaxValue">最大值</param>
		/// <param name="iLabelWidth">文本宽度</param>
		/// <param name="iHeight">高度</param>
		protected void DrawSlider(
			int iLevel, string iLabelText, ref float iValue, 
			float iMinVale, float iMaxValue,
			int iLabelWidth = LabelNoneWidth, int iHeight = RowNoneHeight)
		{
			// 设定缩进
			for (var i = 0; i < iLevel; ++i)
			{
				++EditorGUI.indentLevel;
			}
			
			EditorGUILayout.BeginHorizontal ();
			if (LabelNoneWidth == iLabelWidth && RowNoneHeight == iHeight)
			{
				EditorGUILayout.LabelField (
					iLabelText);
				iValue = (int)EditorGUILayout.Slider(
					iValue, iMinVale, iMaxValue);
			} 
			else if (LabelNoneWidth == iLabelWidth)
			{
				EditorGUILayout.LabelField (
					iLabelText, 
					GUILayout.Height(iHeight));
				iValue = (int)EditorGUILayout.Slider(
					iValue, iMinVale, iMaxValue,
					GUILayout.Height(iHeight));
			}
			else if (RowNoneHeight == iHeight)
			{
				EditorGUILayout.LabelField (
					iLabelText, 
					GUILayout.Width(iLabelWidth));
				iValue = (int)EditorGUILayout.Slider(
					iValue, iMinVale, iMaxValue);
			}
			else
			{
				EditorGUILayout.LabelField (
					iLabelText, 
					GUILayout.Width(iLabelWidth), GUILayout.Height(iHeight));
				iValue = (int)EditorGUILayout.Slider(
					iValue, iMinVale, iMaxValue,
					GUILayout.Height(iHeight));
			}
			EditorGUILayout.EndHorizontal();
			
			// 恢复缩进
			for (var i = 0; i < iLevel; ++i)
			{
				--EditorGUI.indentLevel;
			}
		}

		/// <summary>
		/// 绘制滑杆:整型(标签长度短，高度中)
		/// </summary>
		/// <param name="iLevel">层级(影响到左边缩进的余白)</param>
		/// <param name="iLabelText">标签文本</param>
		/// <param name="iValue">值</param>
		/// <param name="iMinVale">最小值</param>
		/// <param name="iMaxValue">最大值</param>
		protected void DrawShortSlider(
			int iLevel, string iLabelText, ref float iValue,
			float iMinVale, float iMaxValue)
		{
			DrawSlider(iLevel, iLabelText, ref iValue, iMinVale, iMaxValue, LabelShortWidth);
		}
		
		/// <summary>
		/// 绘制滑杆:整型(标签长度短，高度中)
		/// </summary>
		/// <param name="iLevel">层级(影响到左边缩进的余白)</param>
		/// <param name="iLabelText">标签文本</param>
		/// <param name="iValue">值</param>
		/// <param name="iOnChanged">值变更事件</param>
		/// <param name="iMinVale">最小值</param>
		/// <param name="iMaxValue">最大值</param>
		protected void DrawShortSlider(
			int iLevel, string iLabelText, 
			float iValue, Action<float> iOnChanged,
			float iMinVale, float iMaxValue)
		{
			var valueTmp = iValue;
			DrawSlider(iLevel, iLabelText, ref valueTmp, iMinVale, iMaxValue, LabelShortWidth);
			if (Math.Abs(valueTmp - iValue) > 0)
			{
				iOnChanged(valueTmp);
			}
		}

		/// <summary>
		/// 绘制滑杆:整型(标签长度中，高度中)
		/// </summary>
		/// <param name="iLevel">层级(影响到左边缩进的余白)</param>
		/// <param name="iLabelText">标签文本</param>
		/// <param name="iValue">值</param>
		/// <param name="iMinVale">最小值</param>
		/// <param name="iMaxValue">最大值</param>
		protected void DrawMiddleSlider(
			int iLevel, string iLabelText, ref float iValue,
			float iMinVale, float iMaxValue)
		{
			DrawSlider(iLevel, iLabelText, ref iValue, iMinVale, iMaxValue, LabelMiddleWidth);
		}
		
		/// <summary>
		/// 绘制滑杆:整型(标签长度中，高度中)
		/// </summary>
		/// <param name="iLevel">层级(影响到左边缩进的余白)</param>
		/// <param name="iLabelText">标签文本</param>
		/// <param name="iValue">值</param>
		/// <param name="iOnChanged">值变更事件</param>
		/// <param name="iMinVale">最小值</param>
		/// <param name="iMaxValue">最大值</param>
		protected void DrawMiddleSlider(
			int iLevel, string iLabelText, 
			float iValue, Action<float> iOnChanged,
			float iMinVale, float iMaxValue)
		{
			var valueTmp = iValue;
			DrawSlider(iLevel, iLabelText, ref valueTmp, iMinVale, iMaxValue, LabelMiddleWidth);
			if (Math.Abs(valueTmp - iValue) > 0)
			{
				iOnChanged(valueTmp);
			}
		}

		/// <summary>
		/// 绘制滑杆:整型(标签长度长，高度中)
		/// </summary>
		/// <param name="iLevel">层级(影响到左边缩进的余白)</param>
		/// <param name="iLabelText">标签文本</param>
		/// <param name="iValue">值</param>
		/// <param name="iMinVale">最小值</param>
		/// <param name="iMaxValue">最大值</param>
		protected void DrawLongSlider(
			int iLevel, string iLabelText, ref float iValue,
			float iMinVale, float iMaxValue)
		{
			DrawSlider(iLevel, iLabelText, ref iValue, iMinVale, iMaxValue, LabelLongWidth);
		}
		
		/// <summary>
		/// 绘制滑杆:整型(标签长度长，高度中)
		/// </summary>
		/// <param name="iLevel">层级(影响到左边缩进的余白)</param>
		/// <param name="iLabelText">标签文本</param>
		/// <param name="iValue">值</param>
		/// <param name="iOnChanged">值变更事件</param>
		/// <param name="iMinVale">最小值</param>
		/// <param name="iMaxValue">最大值</param>
		protected void DrawLongSlider(
			int iLevel, string iLabelText, 
			float iValue, Action<float> iOnChanged,
			float iMinVale, float iMaxValue)
		{
			var valueTmp = iValue;
			DrawSlider(iLevel, iLabelText, ref valueTmp, iMinVale, iMaxValue, LabelLongWidth);
			if (Math.Abs(valueTmp - iValue) > 0)
			{
				iOnChanged(valueTmp);
			}
		}

		/// <summary>
		/// 绘制Slider
		/// </summary>
		/// <param name="iPos">开始位置</param>
		/// <param name="iWidth">宽度</param>
		/// <param name="iHeight">高度</param>
		/// <param name="iValue">值</param>
		/// <param name="iMinValue">最小值</param>
		/// <param name="iMaxValue">最大值</param>
		/// <param name="iLabelText">文本</param>
		/// <returns>绘制范围</returns>
		protected Rect DrawSlider(Vector2 iPos, float iWidth, float iHeight,
			ref float iValue, float iMinValue = 0.0f, float iMaxValue = 1.0f, 
			string iLabelText = null)
		{
			var rect = new Rect(iPos.x, iPos.y, iWidth, iHeight);

			iValue = string.IsNullOrEmpty(iLabelText) ? 
				EditorGUI.Slider(rect, iValue, iMinValue, iMaxValue) 
				: EditorGUI.Slider(rect, iLabelText, iValue, iMinValue, iMaxValue);
			return rect;
		}

#endregion

#region Inspector - Draw - Object

		/// <summary>
		/// 绘制Unity3d对象(默认：标签长度无限制，高度无限制)
		/// </summary>
		/// <param name="iLevel">层级(影响到左边缩进的余白)</param>
		/// <param name="iLabelText">标签文本</param>
		/// <param name="iObject">对象</param>
		/// <param name="iLabelWidth">文本宽度</param>
		/// <param name="iHeight">高度</param>
		/// <typeparam name="T">绘制对象类型</typeparam>
		protected void DrawObject<T> (
			int iLevel, string iLabelText, ref T iObject,
			int iLabelWidth = LabelNoneWidth, int iHeight = RowNoneHeight) where T : UnityEngine.Object
		{
			// 设定缩进
			for (var i = 0; i < iLevel; ++i)
			{
				++EditorGUI.indentLevel;
			}
			
			EditorGUILayout.BeginHorizontal ();
			if (LabelNoneWidth == iLabelWidth && RowNoneHeight == iHeight)
			{
				EditorGUILayout.LabelField (iLabelText);
				iObject = EditorGUILayout.ObjectField(
					iObject, typeof(T)) as T;
			}
			else if (LabelNoneWidth == iLabelWidth)
			{
				EditorGUILayout.LabelField (iLabelText, GUILayout.Height(iHeight));
				iObject = EditorGUILayout.ObjectField(
					iObject, typeof(T),
					GUILayout.Height(iHeight)) as T;
			}
			else if (RowNoneHeight == iHeight)
			{
				EditorGUILayout.LabelField (iLabelText, 
					GUILayout.Width(iLabelWidth));
				iObject = EditorGUILayout.ObjectField(
					iObject, typeof(T)) as T;
			}
			else
			{
				EditorGUILayout.LabelField (iLabelText, 
					GUILayout.Width(iLabelWidth), GUILayout.Height(iHeight));
				iObject = EditorGUILayout.ObjectField(
					iObject, typeof(T),
					GUILayout.Height(iHeight)) as T;
			}
			EditorGUILayout.EndHorizontal();
			
			// 恢复缩进
			for (var i = 0; i < iLevel; ++i)
			{
				--EditorGUI.indentLevel;
			}
		}

		/// <summary>
		/// 绘制Unity3d对象(标签长度短，高度中)
		/// </summary>
		/// <param name="iLevel">层级(影响到左边缩进的余白)</param>
		/// <param name="iLabelText">标签文本</param>
		/// <param name="iObject">对象</param>
		/// <typeparam name="T">绘制对象类型</typeparam>
		protected void DrawShortObject<T>(
			int iLevel, string iLabelText, ref T iObject) where T : UnityEngine.Object
		{
			DrawObject<T>(iLevel, iLabelText, ref iObject,
				LabelShortWidth, RowMiddleHeight);
		}
		
		/// <summary>
		/// 绘制Unity3d对象(标签长度中，高度中)
		/// </summary>
		/// <param name="iLevel">层级(影响到左边缩进的余白)</param>
		/// <param name="iLabelText">标签文本</param>
		/// <param name="iObject">对象</param>
		/// <typeparam name="T">绘制对象类型</typeparam>
		protected void DrawMiddleObject<T>(
			int iLevel, string iLabelText, ref T iObject) where T : UnityEngine.Object
		{
			DrawObject<T>(iLevel, iLabelText, ref iObject,
				LabelMiddleWidth, RowMiddleHeight);
		}
		
		/// <summary>
		/// 绘制Unity3d对象(标签长度长，高度中)
		/// </summary>
		/// <param name="iLevel">层级(影响到左边缩进的余白)</param>
		/// <param name="iLabelText">标签文本</param>
		/// <param name="iObject">对象</param>
		/// <typeparam name="T">绘制对象类型</typeparam>
		protected void DrawLongObject<T>(
			int iLevel, string iLabelText, ref T iObject) where T : UnityEngine.Object
		{
			DrawObject<T>(iLevel, iLabelText, ref iObject,
				LabelLongWidth, RowMiddleHeight);
		}
		
#endregion

#region Inspector - Draw - GameObject

		/// <summary>
		/// 绘制游戏对象(默认：标题长度无限制，高度无限制)
		/// </summary>
		/// <param name="iLevel">层级(影响到左边缩进的余白)</param>
		/// <param name="iLabelText">标签文本</param>
		/// <param name="iData">地形数据</param>
		/// <param name="iLabelWidth">文本宽度</param>
		/// <param name="iHeight">高度</param>
		protected void DrawGameObject(
			int iLevel, string iLabelText, ref GameObject iData,
			int iLabelWidth = LabelNoneWidth, int iHeight = RowNoneHeight)
		{
			DrawObject<GameObject>(
				iLevel, iLabelText, ref iData, iLabelWidth, iHeight);
		}
		
		/// <summary>
		/// 绘制游戏对象(标题长度短，高度中)
		/// </summary>
		/// <param name="iLevel">层级(影响到左边缩进的余白)</param>
		/// <param name="iLabelText">标签文本</param>
		/// <param name="iData">地形数据</param>
		protected void DrawShortGameObject(
			int iLevel, string iLabelText, ref GameObject iData)
		{
			DrawObject<GameObject>(
				iLevel, iLabelText, ref iData, LabelShortWidth, RowMiddleHeight);
		}
		
		/// <summary>
		/// 绘制游戏对象(标题长度中，高度中)
		/// </summary>
		/// <param name="iLevel">层级(影响到左边缩进的余白)</param>
		/// <param name="iLabelText">标签文本</param>
		/// <param name="iData">地形数据</param>
		protected void DrawMiddleGameObject(
			int iLevel, string iLabelText, ref GameObject iData)
		{
			DrawObject<GameObject>(
				iLevel, iLabelText, ref iData, LabelMiddleWidth, RowMiddleHeight);
		}
		
		/// <summary>
		/// 绘制游戏对象(标题长度长，高度中)
		/// </summary>
		/// <param name="iLevel">层级(影响到左边缩进的余白)</param>
		/// <param name="iLabelText">标签文本</param>
		/// <param name="iData">地形数据</param>
		protected void DrawLongGameObject(
			int iLevel, string iLabelText, ref GameObject iData)
		{
			DrawObject<GameObject>(
				iLevel, iLabelText, ref iData, LabelLongWidth, RowMiddleHeight);
		}

#endregion

#region Inspector - Draw - Terrain

		/// <summary>
		/// 绘制地形数据对象(默认：标题长度无限制，高度无限制)
		/// </summary>
		/// <param name="iLevel">层级(影响到左边缩进的余白)</param>
		/// <param name="iLabelText">标签文本</param>
		/// <param name="iData">地形数据</param>
		/// <param name="iLabelWidth">文本宽度</param>
		/// <param name="iHeight">高度</param>
		protected void DrawTerrain(
			int iLevel, string iLabelText, ref TerrainData iData,
			int iLabelWidth = LabelNoneWidth, int iHeight = RowNoneHeight)
		{
			DrawObject<TerrainData>(
				iLevel, iLabelText, ref iData, iLabelWidth, iHeight);
		}
		
		/// <summary>
		/// 绘制地形数据对象(标题长度短，高度中)
		/// </summary>
		/// <param name="iLevel">层级(影响到左边缩进的余白)</param>
		/// <param name="iLabelText">标签文本</param>
		/// <param name="iData">地形数据</param>
		protected void DrawShortTerrain(
			int iLevel, string iLabelText, ref TerrainData iData)
		{
			DrawTerrain(
				iLevel, iLabelText, ref iData, LabelShortWidth, RowMiddleHeight);
		}
		
		/// <summary>
		/// 绘制地形数据对象(标题长度中，高度中)
		/// </summary>
		/// <param name="iLevel">层级(影响到左边缩进的余白)</param>
		/// <param name="iLabelText">标签文本</param>
		/// <param name="iData">地形数据</param>
		protected void DrawMiddleTerrain(
			int iLevel, string iLabelText, ref TerrainData iData)
		{
			DrawTerrain(
				iLevel, iLabelText, ref iData, LabelMiddleWidth, RowMiddleHeight);
		}
		
		/// <summary>
		/// 绘制地形数据对象(标题长度长，高度中)
		/// </summary>
		/// <param name="iLevel">层级(影响到左边缩进的余白)</param>
		/// <param name="iLabelText">标签文本</param>
		/// <param name="iData">地形数据</param>
		protected void DrawLongTerrain(
			int iLevel, string iLabelText, ref TerrainData iData)
		{
			DrawTerrain(
				iLevel, iLabelText, ref iData, LabelLongWidth, RowMiddleHeight);
		}

#endregion

#region Inspector - Draw - Button

		/// <summary>
		/// 绘制单行单个按钮（默认：长度无限制，高度无限制）
		/// </summary>
		/// <param name="iLevel">层级(影响到左边缩进的余白)</param>
		/// <param name="iButtonText">按钮文本</param>
		/// <param name="iClickEvent">按钮点击事件</param>
		/// <param name="iBgColor">按钮背景颜色</param>
		/// <param name="iWidth">按钮宽度</param>
		/// <param name="iHeight">高度</param>
		protected void DrawSingleButton(
			int iLevel, string iButtonText, Action iClickEvent, Color iBgColor,
			int iWidth = ButtonNoneWidth, int iHeight = RowNoneHeight)
		{
			// 设定缩进
			for (var i = 0; i < iLevel; ++i)
			{
				++EditorGUI.indentLevel;
			}
			
			//EditorGUILayout.BeginHorizontal ();
			using (new BackgroundColorScope(iBgColor))
			{
				if (ButtonNoneWidth == iWidth && RowNoneHeight == iHeight)
				{
					if (GUILayout.Button(iButtonText))
					{
						// 按钮回调事件
						iClickEvent();
					}
				}
				else if(ButtonNoneWidth == iWidth)
				{
					if (GUILayout.Button(iButtonText, GUILayout.Height(iHeight)))
					{
						// 按钮回调事件
						iClickEvent();
					}
				}
				else if (RowNoneHeight == iHeight)
				{
					if (GUILayout.Button(iButtonText, GUILayout.Width(iWidth)))
					{
						// 按钮回调事件
						iClickEvent();
					}
				}
				else
				{
					if (GUILayout.Button(iButtonText, 
						GUILayout.Width(iWidth), GUILayout.Height(iHeight)))
					{
						// 按钮回调事件
						iClickEvent();
					}
				}

			}
			//EditorGUILayout.EndHorizontal();
			
			// 恢复缩进
			for (var i = 0; i < iLevel; ++i)
			{
				--EditorGUI.indentLevel;
			}
		}

		/// <summary>
		/// 绘制单行单个按钮（长度短，高度中）
		/// </summary>
		/// <param name="iLevel">层级(影响到左边缩进的余白)</param>
		/// <param name="iButtonText">按钮文本</param>
		/// <param name="iClickEvent">按钮点击事件</param>
		/// <param name="iBgColor">按钮背景颜色</param>
		protected void DrawSingleShortButton(
			int iLevel, string iButtonText, Action iClickEvent, Color iBgColor)
		{
			DrawSingleButton(
				iLevel, iButtonText, iClickEvent, iBgColor,
				ButtonShortWidth, RowMiddleHeight);
		}
		
		/// <summary>
		/// 绘制单行单个按钮（长度中，高度中）
		/// </summary>
		/// <param name="iLevel">层级(影响到左边缩进的余白)</param>
		/// <param name="iButtonText">按钮文本</param>
		/// <param name="iClickEvent">按钮点击事件</param>
		/// <param name="iBgColor">按钮背景颜色</param>
		protected void DrawSingleMiddleButton(
			int iLevel, string iButtonText, Action iClickEvent, Color iBgColor)
		{
			DrawSingleButton(
				iLevel, iButtonText, iClickEvent, iBgColor,
				ButtonMiddleWidth, RowMiddleHeight);
		}
		
		/// <summary>
		/// 绘制单行单个按钮（长度长，高度中）
		/// </summary>
		/// <param name="iLevel">层级(影响到左边缩进的余白)</param>
		/// <param name="iButtonText">按钮文本</param>
		/// <param name="iClickEvent">按钮点击事件</param>
		/// <param name="iBgColor">按钮背景颜色</param>
		protected void DrawSingleLongButton(
			int iLevel, string iButtonText, Action iClickEvent, Color iBgColor)
		{
			DrawSingleButton(
				iLevel, iButtonText, iClickEvent, iBgColor,
				ButtonLongWidth, RowMiddleHeight);
		}

		
		/// <summary>
		/// 绘制按钮
		/// </summary>
		/// <param name="iPos">开始位置</param>
		/// <param name="iWidth">宽度</param>
		/// <param name="iHeight">高度</param>
		/// <param name="iCaption">按钮文本</param>
		/// <param name="iStyleName">风格名</param>
		/// <param name="iClickEvent">点击事件</param>
		/// <returns>绘制范围</returns>
		protected Rect DrawButton(
			Vector2 iPos, float iWidth, float iHeight, 
			string iCaption, string iStyleName, Action iClickEvent)
		{
			var guiStyle = GUIEditorHelper.CloneStyle(iStyleName, iWidth, iHeight);
			var displayRect = new Rect(iPos.x, iPos.y, iWidth, iHeight);
			if (GUI.Button(displayRect, iCaption, guiStyle))
			{
				// 清空所有对象
				iClickEvent?.Invoke();
			}
			return displayRect;
		}
		
#endregion

#region Inspector - Draw - Toggle(Single)

		/// <summary>
		/// 单个单选框值变更事件委托
		/// </summary>
		/// <param name="iValue">变更后的值</param>
		public delegate void OnSingleToggleChanged(bool iValue);

		/// <summary>
		/// 绘制单行单个选择项（默认：长度无限制，高度无限制）
		/// </summary>
		/// <param name="iLevel">层级(影响到左边缩进的余白)</param>
		/// <param name="iToggleText">选项标题文本</param>
		/// <param name="iValue">值</param>
		/// <param name="iOnChanged">值变更事件</param>
		/// <param name="iLabelWidth">标题文本宽度</param>
		/// <param name="iHeight">高度</param>
		protected void DrawSingleToggle(
			int iLevel, string iToggleText, bool iValue,
			OnSingleToggleChanged iOnChanged = null,
			int iLabelWidth = LabelNoneWidth, int iHeight = RowNoneHeight)
		{
			// 设定缩进
			for (var i = 0; i < iLevel; ++i)
			{
				++EditorGUI.indentLevel;
			}

			var lastValue = iValue;
			bool curValue;
			EditorGUILayout.BeginHorizontal ();
			if (LabelNoneWidth == iLabelWidth && RowNoneHeight == iHeight)
			{
				curValue = EditorGUILayout.Toggle(iToggleText, lastValue);
			}
			else if (LabelNoneWidth == iLabelWidth)
			{
				curValue = EditorGUILayout.Toggle(iToggleText,
					lastValue,GUILayout.Height(iHeight));
			}
			else if (RowNoneHeight == iHeight)
			{
				curValue = EditorGUILayout.Toggle(iToggleText,
					lastValue, GUILayout.Width(iLabelWidth));
			}
			else
			{
				curValue = EditorGUILayout.Toggle(iToggleText,
					lastValue,GUILayout.Width(iLabelWidth), GUILayout.Height(iHeight));
			}
			EditorGUILayout.EndHorizontal();
			
			// 若值变更
			if (null != iOnChanged && lastValue != curValue) iOnChanged(curValue);
			
			// 恢复缩进
			for (var i = 0; i < iLevel; ++i)
			{
				--EditorGUI.indentLevel;
			}
		}

		/// <summary>
		/// 绘制单行单个选择项（长度短，高度中）
		/// </summary>
		/// <param name="iLevel">层级(影响到左边缩进的余白)</param>
		/// <param name="iToggleText">选项标题文本</param>
		/// <param name="iValue">值</param>
		/// <param name="iOnChanged">值变更事件</param>
		protected void DrawSingleShortToggle(
			int iLevel, string iToggleText, bool iValue,
			OnSingleToggleChanged iOnChanged = null)
		{
			DrawSingleToggle(iLevel, iToggleText, iValue, iOnChanged,
				LabelShortWidth, RowMiddleHeight);
		}

		/// <summary>
		/// 绘制单行单个选择项（长度中，高度中）
		/// </summary>
		/// <param name="iLevel">层级(影响到左边缩进的余白)</param>
		/// <param name="iToggleText">选项标题文本</param>
		/// <param name="iValue">值</param>
		/// <param name="iOnChanged">值变更事件</param>
		protected void DrawSingleMiddleToggle(
			int iLevel, string iToggleText, bool iValue,
			OnSingleToggleChanged iOnChanged = null)
		{
			DrawSingleToggle(iLevel, iToggleText, iValue, iOnChanged, 
				LabelMiddleWidth, RowMiddleHeight);
		}

		/// <summary>
		/// 绘制单行单个选择项（长度长，高度中）
		/// </summary>
		/// <param name="iLevel">层级(影响到左边缩进的余白)</param>
		/// <param name="iToggleText">选项标题文本</param>
		/// <param name="iValue">值</param>
		/// <param name="iOnChanged">值变更事件</param>
		protected void DrawSingleLongToggle(
			int iLevel, string iToggleText, bool iValue,
			OnSingleToggleChanged iOnChanged = null)
		{
			DrawSingleToggle(iLevel, iToggleText, iValue, iOnChanged, 
				LabelLongWidth, RowMiddleHeight);
		}

		/// <summary>
		/// 绘制单选框
		/// </summary>
		/// <param name="iPos">开始位置</param>
		/// <param name="iWidth">宽度</param>
		/// <param name="iHeight">高度</param>
		/// <param name="iValue">选中值</param>
		/// <param name="iText">文本</param>
		/// <param name="iStyleName">风格名</param>
		/// <param name="iPaddingLeft">余白：左</param>
		/// <param name="iPaddingRight">余白：右</param>
		/// <param name="iPaddingTop">余白：顶部</param>
		/// <param name="iPaddingBottom">余白：底部</param>
		/// <returns>绘制范围</returns>
		protected Rect DrawToggle(
			Vector2 iPos, float iWidth, float iHeight, ref bool iValue,
			string iText = null, string iStyleName = null,
			int iPaddingLeft = 0, int iPaddingRight = 0,
			int iPaddingTop = 0, int iPaddingBottom = 0)
		{
			var rect = new Rect(iPos.x, iPos.y, iWidth, iHeight);
			if (!string.IsNullOrEmpty(iStyleName))
			{
				var guiStyle = GUIEditorHelper.CloneStyle(iStyleName);
				guiStyle.padding.left += iPaddingLeft;
				guiStyle.padding.right += iPaddingRight;
				guiStyle.padding.top += iPaddingTop;
				guiStyle.padding.bottom += iPaddingBottom;
				iValue = GUI.Toggle(rect, iValue, iText, guiStyle); 
			}
			else
			{
				iValue = GUI.Toggle(rect, iValue, iText);
			}
			return rect;
		}

#endregion

#region Inspector - Draw - Toggle(Multiply)

		/// <summary>
		/// 单个单选框值变更事件委托
		/// </summary>
		/// <param name="iIndex">值变更的索引</param>
		/// <param name="iValue">变更后的值</param>
		public delegate void OnMultiplyToggleChanged(int iIndex, bool iValue);

		/// <summary>
		/// 绘制单行多个选择项（默认：长度无限制，高度无限制）
		/// </summary>
		/// <param name="iLevel">层级(影响到左边缩进的余白)</param>
		/// <param name="iToggleText">选项标题文本</param>
		/// <param name="iValue">值</param>
		/// <param name="iOnChanged">值变更事件</param>
		/// <param name="iLabelWidth">标题文本宽度</param>
		/// <param name="iHeight">高度</param>
		protected void DrawMultiplyToggle(
			int iLevel, string[] iToggleText, bool[] iValue, 
			OnMultiplyToggleChanged iOnChanged = null,
			int iLabelWidth = LabelNoneWidth, int iHeight = RowNoneHeight)
		{
			// 设定缩进
			for (var i = 0; i < iLevel; ++i)
			{
				++EditorGUI.indentLevel;
			}
			// 值保存
			var lastValues = new bool[iValue.Length];
			var curValues = new bool[iValue.Length];
			for (var i = 0; i < iValue.Length; i++)
			{
				lastValues[i] = iValue[i];
				curValues[i] = iValue[i];
			}
			
			EditorGUILayout.BeginHorizontal ();
			if (LabelNoneWidth == iLabelWidth && RowNoneHeight == iHeight)
			{
				for (var i = 0; i < iValue.Length; i++)
				{
					EditorGUILayout.LabelField (iToggleText[i]);
					curValues[i] = EditorGUILayout.Toggle(lastValues[i],
						GUILayout.Width(LabelShortWidth));
				}
			}
			else if (LabelNoneWidth == iLabelWidth)
			{
				for (var i = 0; i < iValue.Length; i++)
				{
					EditorGUILayout.LabelField (iToggleText[i]);
					curValues[i] = EditorGUILayout.Toggle(lastValues[i],
						GUILayout.Width(LabelShortWidth),
						GUILayout.Height(iHeight));
				}
			}
			else if (RowNoneHeight == iHeight)
			{
				for (var i = 0; i < iValue.Length; i++)
				{
					EditorGUILayout.LabelField (iToggleText[i], GUILayout.Width(iLabelWidth));
					curValues[i] = EditorGUILayout.Toggle(lastValues[i], 
						GUILayout.Width(iLabelWidth));
				}
			}
			else
			{
				for (var i = 0; i < iValue.Length; i++)
				{
					EditorGUILayout.LabelField (iToggleText[i]);
					curValues[i] = EditorGUILayout.Toggle(lastValues[i], 
						GUILayout.Width(iLabelWidth), GUILayout.Height(iHeight));
				}
			}
			EditorGUILayout.EndHorizontal();
			
			// 校验值是否又变更
			if (null != iOnChanged)
			{
				for (var i = 0; i < iValue.Length; i++)
				{
					if (lastValues[i] != curValues[i])
					{
						iOnChanged(i, curValues[i]);
					}
				}
			}
					
			// 恢复缩进
			for (var i = 0; i < iLevel; ++i)
			{
				--EditorGUI.indentLevel;
			}
		}

		/// <summary>
		/// 绘制单行多个选择项（默认：长度短，高度中）
		/// </summary>
		/// <param name="iLevel">层级(影响到左边缩进的余白)</param>
		/// <param name="iToggleText">选项标题文本</param>
		/// <param name="iValue">值</param>
		/// <param name="iOnChanged">值变更事件</param>
		protected void DrawShortMultiplyToggle(
			int iLevel, string[] iToggleText, bool[] iValue,
			OnMultiplyToggleChanged iOnChanged = null)
		{
			DrawMultiplyToggle(iLevel, iToggleText, iValue, iOnChanged, 
				LabelShortWidth);	
		}

		/// <summary>
		/// 绘制单行多个选择项（默认：长度中，高度中）
		/// </summary>
		/// <param name="iLevel">层级(影响到左边缩进的余白)</param>
		/// <param name="iToggleText">选项标题文本</param>
		/// <param name="iValue">值</param>
		/// <param name="iOnChanged">值变更事件</param>
		protected void DrawMiddleMultiplyToggle(
			int iLevel, string[] iToggleText, bool[] iValue,
			OnMultiplyToggleChanged iOnChanged = null)
		{
			DrawMultiplyToggle(iLevel, iToggleText, iValue, iOnChanged, 
				LabelMiddleWidth);	
		}

		/// <summary>
		/// 绘制单行多个选择项（默认：长度长，高度中）
		/// </summary>
		/// <param name="iLevel">层级(影响到左边缩进的余白)</param>
		/// <param name="iToggleText">选项标题文本</param>
		/// <param name="iValue">值</param>
		/// <param name="iOnChanged">值变更事件</param>
		protected void DrawLongMultiplyToggle(
			int iLevel, string[] iToggleText, bool[] iValue,
			OnMultiplyToggleChanged iOnChanged = null)
		{
			DrawMultiplyToggle(iLevel, iToggleText, iValue, iOnChanged, 
				LabelLongWidth);	
		}

#endregion

#region Inspector - Draw - Color

		/// <summary>
		/// 绘制颜色选择面板（默认：长度无限制，高度无限制）
		/// </summary>
		/// <param name="iLevel">层级(影响到左边缩进的余白)</param>
		/// <param name="iLabelText">标题文本</param>
		/// <param name="iColor">颜色</param>
		/// <param name="iOnColorChanged">颜色变更事件</param>
		/// <param name="iLabelWidth">标题文本宽度</param>
		/// <param name="iHeight">高度</param>
		protected void DrawColor(
			int iLevel, string iLabelText, Color iColor, Action<Color> iOnColorChanged,
			int iLabelWidth = LabelNoneWidth, int iHeight = RowNoneHeight)
		{
			// 设定缩进
			for (var i = 0; i < iLevel; ++i)
			{
				++EditorGUI.indentLevel;
			}
			
			EditorGUILayout.BeginHorizontal ();
			if (LabelNoneWidth == iLabelWidth && RowNoneHeight == iHeight)
			{
				EditorGUILayout.LabelField (iLabelText);
				var edgeColor = EditorGUILayout.ColorField(iColor);
				if (!iColor.Equals(edgeColor))
				{
					// 重置所有边的颜色
					iOnColorChanged(edgeColor);
				}
			}
			else if (LabelNoneWidth == iLabelWidth)
			{
				EditorGUILayout.LabelField (iLabelText, GUILayout.Height(iHeight));
				var edgeColor = EditorGUILayout.ColorField(iColor, GUILayout.Height(iHeight));
				if (!iColor.Equals(edgeColor))
				{
					// 重置所有边的颜色
					iOnColorChanged(edgeColor);
				}
			}
			else if (RowNoneHeight == iHeight)
			{
				EditorGUILayout.LabelField (iLabelText, GUILayout.Width(iLabelWidth));
				var edgeColor = EditorGUILayout.ColorField(iColor);
				if (!iColor.Equals(edgeColor))
				{
					// 重置所有边的颜色
					iOnColorChanged(edgeColor);
				}
			}
			else
			{
				EditorGUILayout.LabelField (iLabelText, 
					GUILayout.Width(iLabelWidth), GUILayout.Height(iHeight));
				var edgeColor = EditorGUILayout.ColorField(iColor, GUILayout.Height(iHeight));
				if (!iColor.Equals(edgeColor))
				{
					// 重置所有边的颜色
					iOnColorChanged(edgeColor);
				}
			}
			
			EditorGUILayout.EndHorizontal();
			
			// 恢复缩进
			for (var i = 0; i < iLevel; ++i)
			{
				--EditorGUI.indentLevel;
			}
		}

		/// <summary>
		/// 绘制颜色选择面板（长度短，高度中）
		/// </summary>
		/// <param name="iLevel">层级(影响到左边缩进的余白)</param>
		/// <param name="iLabelText">标题文本</param>
		/// <param name="iColor">颜色</param>
		/// <param name="iOnColorChanged">颜色变更事件</param>
		protected void DrawShortColor(
			int iLevel, string iLabelText, Color iColor, Action<Color> iOnColorChanged)
		{
			DrawColor(iLevel, iLabelText, iColor, iOnColorChanged, LabelShortWidth, RowMiddleHeight);
		}

		/// <summary>
		/// 绘制颜色选择面板（长度中，高度中）
		/// </summary>
		/// <param name="iLevel">层级(影响到左边缩进的余白)</param>
		/// <param name="iLabelText">标题文本</param>
		/// <param name="iColor">颜色</param>
		/// <param name="iOnColorChanged">颜色变更事件</param>
		protected void DrawMiddleColor(
			int iLevel, string iLabelText, Color iColor, Action<Color> iOnColorChanged)
		{
			DrawColor(iLevel, iLabelText, iColor, iOnColorChanged, LabelMiddleWidth, RowMiddleHeight);
		}

		/// <summary>
		/// 绘制颜色选择面板（长度长，高度中）
		/// </summary>
		/// <param name="iLevel">层级(影响到左边缩进的余白)</param>
		/// <param name="iLabelText">标题文本</param>
		/// <param name="iColor">颜色</param>
		/// <param name="iOnColorChanged">颜色变更事件</param>
		protected void DrawLongColor(
			int iLevel, string iLabelText, Color iColor, Action<Color> iOnColorChanged)
		{
			DrawColor(iLevel, iLabelText, iColor, iOnColorChanged, LabelLongWidth, RowMiddleHeight);
		}

#endregion

#region Inspector - Draw - ToolBar

		/// <summary>
		/// 绘制工具Bar（默认：长度无限制，高度无限制）
		/// </summary>
		/// <param name="iLevel">层级(影响到左边缩进的余白)</param>
		/// <param name="iLabelText"></param>
		/// <param name="iSelectIndex"></param>
		/// <param name="iLabelWidth">文本宽度</param>
		/// <param name="iHeight">高度</param>
		protected void DrawToolBars(
			int iLevel, string[] iLabelText, ref int iSelectIndex,
			int iLabelWidth = LabelNoneWidth, int iHeight = RowNoneHeight)
		{
			
			if(null == iLabelText || 0 >= iLabelText.Length) return;
			
			// 设定缩进
			for (var i = 0; i < iLevel; ++i)
			{
				++EditorGUI.indentLevel;
			}

			var toolbarStyle = GUIEditorHelper.CloneStyle("toolbarbutton");
			// toolbarStyle.padding.left = 5;
			// toolbarStyle.padding.right = 5;
			// toolbarStyle.overflow.left = 5;
			// toolbarStyle.overflow.right = 5;
			toolbarStyle.alignment = TextAnchor.MiddleCenter;
			
			// 空行
			EditorGUILayout.Space();
			if (LabelNoneWidth == iLabelWidth && RowNoneHeight == iHeight)
			{
				iSelectIndex = GUILayout.Toolbar (
					iSelectIndex, iLabelText, toolbarStyle);
			}
			else if (LabelNoneWidth == iLabelWidth)
			{
				iSelectIndex = GUILayout.Toolbar (
					iSelectIndex, iLabelText,toolbarStyle,
					GUILayout.Height(iHeight));
			}
			else if (RowNoneHeight == iHeight)
			{
				iSelectIndex = GUILayout.Toolbar (
					iSelectIndex, iLabelText, toolbarStyle,
					GUILayout.Width(iLabelWidth));
			}                                               
			else
			{
				iSelectIndex = GUILayout.Toolbar (
					iSelectIndex, iLabelText, toolbarStyle,
					GUILayout.Width(iLabelWidth), GUILayout.Height(iHeight));
			}
			// 空行
			EditorGUILayout.Space();
			
			// 恢复缩进
			for (var i = 0; i < iLevel; ++i)
			{
				--EditorGUI.indentLevel;
			}
		}

#endregion

#region Inspector - Draw - ScrollView

		/// <summary>
		/// 滚动Cell宽度：无限制
		/// </summary>
		protected const int ScrollCellNoneWidth = -1;
				
		/// <summary>
		/// 滚动Cell高度：无限制
		/// </summary>
		protected const int ScrollCellNoneHeight = -1;
				
		/// <summary>
		/// 滚动Cell高度：默认高度
		/// </summary>
		protected const int ScrollCellDefaultHeight = 20;

		/// <summary>
		/// Scroll View宽度：无限制
		/// </summary>
		protected const int ScrollViewNoneWidth = ScrollCellNoneWidth;
		
		/// <summary>
		/// Scroll View高度：无限制
		/// </summary>
		protected const int ScrollViewNoneHeight = -1;
		
		/// <summary>
		/// Scroll View高度：默认高度
		/// </summary>
		protected const int ScrollViewDefaultHeight = 120;

		/// <summary>
		/// 绘制滚动列表（默认：长度无限制，高度无限制）
		/// </summary>
		/// <param name="iLevel">层级(影响到左边缩进的余白)</param>
		/// <param name="iDisplayRect">显示范围(屏幕中的绘制区域)</param>
		/// <param name="iScrollOffset">滚动偏移</param>
		/// <param name="iViewRect">显示范围</param>
		/// <param name="iDrawContent">绘制具体内容事件</param>
		protected Vector2 DrawScrollView(
			int iLevel, Rect iDisplayRect, Vector2 iScrollOffset,Rect iViewRect, 
			Action<Rect> iDrawContent)
		{
			// 设定缩进
			for (var i = 0; i < iLevel; ++i)
			{
				++EditorGUI.indentLevel;
			}

			
			var backgroundStyle = GUIEditorHelper.CloneStyle("ProfilerScrollviewBackground");
			var hBarStyle = GUIEditorHelper.CloneStyle("HorizontalMinMaxScrollbarThumb");
			var vBarStyle = GUIEditorHelper.CloneStyle("VerticalMinMaxScrollbarThumb");
			backgroundStyle.overflow.left = 0;
			Vector2 curOffset;
			curOffset = GUI.BeginScrollView (
				iDisplayRect, iScrollOffset, iViewRect,
				false, false);
			iDrawContent?.Invoke(iViewRect);
			GUI.EndScrollView ();
			
			// 恢复缩进
			for (var i = 0; i < iLevel; ++i)
			{
				--EditorGUI.indentLevel;
			}
			
			return curOffset;
		}

		/// <summary>
		/// 绘制ScrollView
		/// </summary>
		/// <param name="iOffset">滚动条偏移</param>
		/// <param name="iDisplayRect">显示范围</param>
		/// <param name="iViewRect">View范围</param>
		/// <param name="iDrawContent">绘制内容委托函数</param>
		/// <param name="iBgStyleName">背景风格名</param>
		/// <param name="iBgOverflowLeft">背景溢出余白：左</param>
		/// <param name="iBgOverflowRight">背景溢出余白：右</param>
		/// <param name="iBgOverflowTop">背景溢出余白：顶部</param>
		/// <param name="iBgOverflowBottom">背景溢出余白：底部</param>
		/// <returns>绘制范围</returns>
		protected Rect DrawScrollView(
			ref Vector2 iOffset,
			Rect iDisplayRect, Rect iViewRect,
			Action<Rect> iDrawContent, 
			string iBgStyleName = null,
			int iBgOverflowLeft = 0, int iBgOverflowRight = 0,
			int iBgOverflowTop = 0, int iBgOverflowBottom = 0)
		{
			var bgStyle = GUIEditorHelper.CloneStyle(iBgStyleName);
			bgStyle.overflow.left = iBgOverflowLeft;
			bgStyle.overflow.right = iBgOverflowRight;
			bgStyle.overflow.top = iBgOverflowTop;
			bgStyle.overflow.bottom = iBgOverflowBottom;
			iOffset = GUI.BeginScrollView (
				iDisplayRect, iOffset, iViewRect,
				false, false);
			iDrawContent?.Invoke(iViewRect);
			GUI.EndScrollView ();

			return iDisplayRect;
		} 

#endregion

#region Inspector - Draw - DropArea

		/// <summary>
		/// 取得对象
		/// </summary>
		/// <param name="iLevel">层级(影响到左边缩进的余白)</param>
		/// <param name="iStyleName">拖拽范围显示风格</param>
		/// <param name="iWidth">拖拽范围：宽度</param>
		/// <param name="iHeight">拖拽范围：高度</param>
		/// <param name="iTitle">标题信息</param>
		/// <param name="iNotice">拖拽区域提示信息</param>
		/// <param name="iOnDraged">拖拽事件</param>
		/// <returns>对象</returns>
		protected UnityEngine.Object GetObjectFromDragArea(
			int iLevel, string iStyleName, int iWidth, int iHeight, 
			string iTitle = null, string iNotice = null, 
			Action<UnityEngine.Object, string> iOnDraged = null)
		{
			// 设定缩进
			for (var i = 0; i < iLevel; ++i)
			{
				++EditorGUI.indentLevel;
			}

			var aEvent = Event.current;
		 
			GUI.contentColor = Color.white;
			UnityEngine.Object dragObj = null;
		 
			var dragAreaLabel = new GUIContent(iTitle);
			if (string.IsNullOrEmpty(iTitle))
			{
				dragAreaLabel = new GUIContent("");
			}
			var dragArea = new Rect(0, 0, iWidth, iHeight);
			var dragAreaStyle = GUIEditorHelper.CloneStyle(iStyleName, iWidth, iHeight);
			dragAreaStyle.fixedWidth = iWidth;
			dragAreaStyle.fixedHeight = iHeight;
			dragAreaStyle.alignment = TextAnchor.MiddleCenter;
			dragAreaStyle.fontSize = 12;
			GUI.Box(dragArea, dragAreaLabel, dragAreaStyle);
			switch (aEvent.type)
			{
				case EventType.DragUpdated:
				case EventType.DragPerform:
				case EventType.DragExited:
					if (!dragArea.Contains(aEvent.mousePosition))
					{
						break;
					}
		 
					DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
					if (aEvent.type == EventType.DragPerform)
					{
						DragAndDrop.AcceptDrag();
		 
						for (int i = 0; i < DragAndDrop.objectReferences.Length; ++i)
						{
							dragObj = DragAndDrop.objectReferences[i];
							iOnDraged?.Invoke(dragObj, DragAndDrop.paths[i]);
							
							if (dragObj == null)
							{
								break;
							}
						}
					}
		 
					Event.current.Use();
					break;
				default:
					break;
			}

			// 恢复缩进
			for (var i = 0; i < iLevel; ++i)
			{
				--EditorGUI.indentLevel;
			}
			return dragObj;
		}

#endregion

#region Inspector - Draw - Dialog

		/// <summary>
		/// 显示对话框
		/// </summary>
		/// <param name="iTitle">标题</param>
		/// <param name="iContent">文本内容</param>
		/// <param name="iBtnLabel">左侧按钮文本</param>
		/// <param name="iOnBtnClick">右侧按钮点击事件</param>
		public void ShowDialog(
			string iTitle, string iContent,
			string iBtnLabel, Action iOnBtnClick = null)
		{
			if (EditorUtility.DisplayDialog(
				iTitle, iContent, 
				iBtnLabel))
			{
				iOnBtnClick?.Invoke();
			}
			else
			{
				this.Error("ShowDialog():Unknow Error!!!");
			}
		}

		/// <summary>
		/// 显示对话框
		/// </summary>
		/// <param name="iTitle">标题</param>
		/// <param name="iContent">文本内容</param>
		/// <param name="iLeftBtnLabel">左侧按钮文本</param>
		/// <param name="iRightBtnLabel">右侧按钮文本</param>
		/// <param name="iOnLeftBtnClick">左侧按钮点击事件</param>
		/// <param name="iOnRightBtnClick">右侧按钮点击事件</param>
		public void ShowDialog(
			string iTitle, string iContent,
			string iLeftBtnLabel, string iRightBtnLabel, 
			Action iOnLeftBtnClick = null, Action iOnRightBtnClick = null)
		{
			if (EditorUtility.DisplayDialog(
				iTitle, iContent, 
				iLeftBtnLabel, iRightBtnLabel))
			{
				iOnLeftBtnClick?.Invoke();
			}
			else
			{
				iOnRightBtnClick?.Invoke();
			}
		}
		
		/// <summary>
		/// 显示对话框
		/// </summary>
		/// <param name="iTitle">标题</param>
		/// <param name="iContent">文本内容</param>
		/// <param name="iLeftBtnLabel">左侧按钮文本</param>
		/// <param name="iMiddleBtnLabel">中间按钮文本</param>
		/// <param name="iRightBtnLabel">右侧按钮文本</param>
		/// <param name="iOnLeftBtnClick">左侧按钮点击事件</param>
		/// <param name="iOnMiddleBtnClick">中间按钮点击事件</param>
		/// <param name="iOnRightBtnClick">右侧按钮点击事件</param>
		public void ShowDialog(
			string iTitle, string iContent,
			string iLeftBtnLabel, string iMiddleBtnLabel, string iRightBtnLabel, 
			Action iOnLeftBtnClick = null, Action iOnMiddleBtnClick = null, Action iOnRightBtnClick = null)
		{
			var btnIdx = EditorUtility.DisplayDialogComplex(
				iTitle, iContent,
				iLeftBtnLabel, iMiddleBtnLabel, iRightBtnLabel);
			switch (btnIdx)
			{
				case 0:
					iOnLeftBtnClick?.Invoke();
					break;
				case 1:
					iOnMiddleBtnClick?.Invoke();
					break;
				case 3:
					iOnRightBtnClick?.Invoke();
					break;
				default:
					this.Error("ShowDialog():Unknow Error!!!");
					break;
			}
		}

#endregion

#region Inspector - Draw - Texture

		/// <summary>
		/// 绘制纹理
		/// </summary>
		/// <param name="iPos">开始位置</param>
		/// <param name="iWidth">宽度</param>
		/// <param name="iHeight">高度</param>
		/// <param name="iTex">纹理</param>
		/// <returns>绘制范围</returns>
		protected Rect DrawTexture(
			Vector2 iPos, float iWidth, float iHeight,
			Texture iTex)
		{
			var rect = new Rect(iPos.x, iPos.y, iWidth, iHeight);
			if (null == iTex) return rect;
			var iconSize = iWidth >= iHeight ? iHeight : iWidth;
			var displayRectTmp = new Rect(
				rect.x + (iWidth - iconSize) / 2.0f, 
				rect.y + (iHeight - iconSize) / 2.0f, 
				iconSize, iconSize);
			GUI.DrawTexture(displayRectTmp, iTex);
			return rect;
		}

#endregion

#region ProgressBar

		private bool _progressBarVisiable = false;
		private string _curProgressTitle;

		/// <summary>
		/// 开始进度条
		/// </summary>
		/// <param name="iTitle">标题</param>
		/// <param name="iStatusTxt">状态文本</param>
		protected void ProgressBarStart(string iTitle, string iStatusTxt)
		{
			if(_progressBarVisiable) return;
			_progressBarVisiable = true;
			// 显示处理进度条
			_curProgressTitle = iTitle;
			EditorUtility.DisplayProgressBar(
				iTitle, iStatusTxt, 0.0f);
		}
		
		/// <summary>
		/// 开始进度条
		/// </summary>
		/// <param name="iTitle">标题</param>
		/// <param name="iGetStatusCallback">状态取得回调</param>
		protected void ProgressBarStart(string iTitle, Func<string> iGetStatusCallback)
		{
			var status = iGetStatusCallback?.Invoke();
			if (string.IsNullOrEmpty(iTitle) || string.IsNullOrEmpty(status)) return;
			if(_progressBarVisiable) return;
			_progressBarVisiable = true;
			// 显示处理进度条
			_curProgressTitle = iTitle;
			EditorUtility.DisplayProgressBar(
				iTitle, status, 0.0f);

		}
		
		/// <summary>
		/// 开始进度条(可自动开启和关闭)
		/// </summary>
		/// <param name="iTitle">标题</param>
		/// <param name="iProgressStep">进度计数器Step</param>
		/// <param name="iAutoClear">自动清空标识位(true:当达到100%时，进度条自动清除)</param>
		/// <param name="iOnCompleted">完成回调函数</param>
		protected ProgressCounter ShowProgressBar(
			string iTitle, ProgressCountStep iProgressStep, 
			bool iAutoClear = true, Action<long> iOnCompleted = null)
		{
			if(_progressBarVisiable) return null;
			_progressBarVisiable = true;

			// 显示处理进度条
			_curProgressTitle = iTitle;
			Action<int, int, long, string> onProgressUpdated = (iCurCount, iMaxCount, iDeltaTime, iDesc) =>
			{
				// 计算进度
				var progress = (0>= iMaxCount) ? 0.0f : (float) iCurCount / iMaxCount;
				progress = (1.0f <= progress) ? 1.0f : progress;
				// 显示进度条
				EditorUtility.DisplayProgressBar(
					iTitle, iDesc, progress);
				if (iAutoClear && 1.0f <= progress)
				{
					ProgressBarClear();
					iOnCompleted?.Invoke(iDeltaTime);
				}
			};

			var steps = new List<ProgressCountStep> {iProgressStep};
			// 进度计数器
			return ProgressCounter.Create(steps, iAutoClear, onProgressUpdated); 
		}
		
		/// <summary>
		/// 开始进度条(可自动开启和关闭)
		/// </summary>
		/// <param name="iTitle">标题</param>
		/// <param name="iProgressSteps">进度计数器Steps</param>
		/// <param name="iAutoClear">自动清空标识位(true:当达到100%时，进度条自动清除)</param>
		protected ProgressCounter ShowProgressBar(
			string iTitle, IEnumerable<ProgressCountStep> iProgressSteps, 
			bool iAutoClear = true)
		{
			if(_progressBarVisiable) return null;
			_progressBarVisiable = true;

			// 显示处理进度条
			_curProgressTitle = iTitle;

			void OnProgressUpdated(int iCurCount, int iMaxCount, long iDeltaTime, string iDesc)
			{
				// 计算进度
				var progress = 0 >= iMaxCount ? 0.0f : (float) iCurCount / iMaxCount;
				progress = 1.0f <= progress ? 1.0f : progress;
				// 显示进度条
				EditorUtility.DisplayProgressBar(iTitle, iDesc, progress);
				if (iAutoClear && 1.0f <= progress)
				{
					ProgressBarClear();
				}
			}

			void OnProgressEnd(int iCurCount, int iMaxCount, long iDeltaTime)
			{
				if (iCurCount != iMaxCount)
				{
					this.Error($"ShowProgressBar():Illegal end！({iCurCount}/{iMaxCount})");
				}

				ProgressBarClear();
			}

			// 进度计数器
			return ProgressCounter.Create(
				iProgressSteps, iAutoClear, OnProgressUpdated, OnProgressEnd);
		}
		
		/// <summary>
		/// 开始进度条
		/// </summary>
		/// <param name="iGetTitleCallback">标题取得回调</param>
		/// <param name="iGetStatusCallback">状态取得回调</param>
		protected void ProgressBarStart(Func<string> iGetTitleCallback, Func<string> iGetStatusCallback)
		{
			var title = iGetTitleCallback?.Invoke();
			var status = iGetStatusCallback?.Invoke();
			if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(status)) return;
			if(_progressBarVisiable) return;
			_progressBarVisiable = true;
			// 显示处理进度条
			_curProgressTitle = title;
			EditorUtility.DisplayProgressBar(
				title, status, 0.0f);

		}

		/// <summary>
		/// 更新进度条
		/// </summary>
		/// <param name="iStatusTxt">状态文本</param>
		/// <param name="iProgress">进度</param>
		/// <param name="iAutoClear">自动清空标识位(true:当达到100%时，进度条自动清除)</param>
		protected void ProgressBarUpdate(
			string iStatusTxt, float iProgress, 
			bool iAutoClear = true)
		{
			if(!_progressBarVisiable) return;
			EditorUtility.DisplayProgressBar(
				_curProgressTitle, iStatusTxt, iProgress);
			if (iAutoClear && 1.0f <= iProgress)
			{
				// 移除进度条
				ProgressBarClear();
			}
		}
		
		/// <summary>
		/// 开始进度条
		/// </summary>
		/// <param name="iGetStatusCallback">状态取得回调</param>
		/// <param name="iGetProgressCallback">进度百分比取得回调</param>
		/// <param name="iAutoClear">自动清空标识位(true:当达到100%时，进度条自动清除)</param>
		protected void ProgressBarUpdate(
			Func<string> iGetStatusCallback, Func<float> iGetProgressCallback, 
			bool iAutoClear = true)
		{
			if(!_progressBarVisiable) return;
			var status = iGetStatusCallback?.Invoke();
			var progress = iGetProgressCallback?.Invoke() ?? 0.0f;
			if (string.IsNullOrEmpty(_curProgressTitle) || string.IsNullOrEmpty(status)) return;
			// 显示处理进度条
			EditorUtility.DisplayProgressBar(
				_curProgressTitle, status, progress);
			if (iAutoClear && 1.0f <= progress)
			{
				// 移除进度条
				ProgressBarClear();
			}

		}

		/// <summary>
		/// 移除进度条
		/// </summary>
		protected void ProgressBarClear()
		{
			EditorUtility.ClearProgressBar();
			_progressBarVisiable = false;
		}

#endregion
		
#region JosnDataBase - abstract

		/// <summary>
		/// 应用导入数据数据.
		/// </summary>
		/// <param name="iData">数据.</param>
		/// <param name="iForceClear">强制清空标志位.</param>
		protected abstract void ApplyImportData (T2 iData, bool iForceClear);
		
#endregion

	}
}


