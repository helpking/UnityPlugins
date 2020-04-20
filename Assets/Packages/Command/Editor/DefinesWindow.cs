using System;
using System.Collections.Generic;
using System.Linq;
using Packages.Common.Base;
using Packages.Common.Editor;
using Packages.Common.Extend;
using Packages.Common.Extend.Editor;
using Packages.Defines.Editor;
using Packages.Logs;
using Packages.Utils;
using Packages.Utils.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace Packages.Command.Editor {

	/// <summary>
	/// 自定义宏配置信息.
	/// </summary>
	[Serializable]
	public class DefinesConfInfo : WindowConfInfoBase {

		/// <summary>
		/// 输入项：新追加宏定义.
		/// </summary>
		[FormerlySerializedAs("InputNewDefine")] 
		public string inputNewDefine;

		/// <summary>
		/// 输入项：安卓.
		/// </summary>
		[FormerlySerializedAs("InputAndroidSelected")] 
		public bool inputAndroidSelected;

		/// <summary>
		/// 输入项：iOS.
		/// </summary>
		[FormerlySerializedAs("InputIOSSelected")] 
		public bool inputIOSSelected;

		/// <summary>
		/// 列表显示范围.
		/// </summary>
		[FormerlySerializedAs("ScrollViewRect")] 
		public Rect scrollViewRect;

#region Implement

		/// <summary>
		/// 初始化.
		/// </summary>
		public override  void Init() {
			inputNewDefine = null;
			inputAndroidSelected = false;
			inputIOSSelected = false;
			WindowName = "自定义宏追加";
			Title = "宏一览";
			LineHeight = 20.0f;
			DisplayRect = new Rect (0.0f, 0.0f, 400.0f, 200.0f);
		}

#endregion
	}

	/// <summary>
	/// 宏定义.
	/// </summary>
	[Serializable]
	public class DefineInfo : JsonDataBase<DefineInfo> {
		[FormerlySerializedAs("Name")] 
		public string name;
		[FormerlySerializedAs("Android")] 
		public bool android;
		public bool iOS;

		public static DefineInfo Create(string iName, bool iAndroid, bool iIOS)
		{
			return new DefineInfo()
			{
				name = iName,
				android = iAndroid,
				iOS = iIOS
			};
		}
	}

	/// <summary>
	/// 自定义宏数据.
	/// </summary>
	[Serializable]
	public class DefinesData : WindowDataBase {
		
		/// <summary>
		/// XCode工程设定情报.
		/// </summary>
		[FormerlySerializedAs("Defines")] 
		public List<DefineInfo> defines = new List<DefineInfo> ();

		/// <summary>
		/// 清空.
		/// </summary>
		public override void Clear()
		{
			defines?.Clear();
		}

		/// <summary>
		/// 追加宏.
		/// </summary>
		/// <param name="iNames">宏定义列表</param>
		/// <param name="iAndroid">安卓标识位</param>
		/// <param name="iIOS">iOS标识位</param>
		public void AddDefines(string[] iNames, bool iAndroid, bool iIOS) {

			if (null == iNames || 0 >= iNames.Length ||
				null == defines) {
				return;
			}
			foreach (var defineName in iNames) {
				var isExist = false;
				foreach (var define in defines) {
					if (string.IsNullOrEmpty (define.name)) {
						continue;
					}
					if (!defineName.Equals(define.name)) continue;
					if (iAndroid) {
						define.android = iAndroid;
					}
					if (iIOS) {
						define.iOS = iIOS;
					}
					isExist = true;
					break;
				}

				if (isExist) continue;
				
				// 追加宏定义设定
				{
					var define = DefineInfo.Create(defineName, iAndroid, iIOS);
					defines.Add (define);
					this.Info("AddDefine -> Name:{0} Andorid:{1} iOS:{2}", 
						defineName, iAndroid, iIOS);
				}
			}

			// 按名字排序
			defines.Sort((iX,iY) => (string.Compare(iX.name, iY.name, StringComparison.Ordinal)));

		}

		/// <summary>
		/// 应用.
		/// </summary>
		public override void Apply() {

			// 安卓设定
			var androids = defines
				.Where (iO => iO.android)
				.OrderBy (iO => iO.name)
				.ToArray ();
			DefinesSetting.SetDefines (androids, BuildTargetGroup.Android);

			// iOS设定
			var iOSs = defines
				.Where (iO => iO.iOS)
				.OrderBy (iO => iO.name)
				.ToArray ();
			DefinesSetting.SetDefines (iOSs, BuildTargetGroup.iOS);
		}
	}

	/// <summary>
	/// 自定义宏窗口.
	/// </summary>
	public class DefinesWindow : WindowBase<DefinesData, DefinesConfInfo> {
		public const string JsonFileDir = "Assets/Packages/Command/Editor/Json";

		/// <summary>
		/// 输入项：新追加宏定义.
		/// </summary>
		private static string InputNewDefine {
			get
			{
				return ConfInfo?.inputNewDefine;
			}	
			set { 
				if(null == ConfInfo) {
					return;
				}
				ConfInfo.inputNewDefine = value;
			}
		}

		/// <summary>
		/// 输入项：安卓.
		/// </summary>
		private static bool InputAndroidSelected {
			get
			{
				return null != ConfInfo && ConfInfo.inputAndroidSelected;
			}	
			set { 
				if(null == ConfInfo) {
					return;
				}
				ConfInfo.inputAndroidSelected = value;
			}
		}

		/// <summary>
		/// 输入项：iOS.
		/// </summary>
		private static bool InputIOSSelected {
			get
			{
				return null != ConfInfo && ConfInfo.inputIOSSelected;
			}	
			set { 
				if(null == ConfInfo) {
					return;
				}
				ConfInfo.inputIOSSelected = value;
			}
		}

		/// <summary>
		/// 列表显示范围.
		/// </summary>
		private static Rect ScrollViewRect {
			get
			{
				return ConfInfo?.scrollViewRect ?? new Rect(0.0f, 0.0f, 100.0f, 100.0f);
			}	
			set { 
				if(null == ConfInfo) {
					return;
				}
				ConfInfo.scrollViewRect = value;
			}
		}

		/// <summary>
		/// 滚动列表开始位置.
		/// </summary>
		private Vector2 _scrollViewStartPos = Vector2.zero;

		/// <summary>
		/// 自定义宏一览.
		/// </summary>
		public List<DefineInfo> Defines {
			get
			{
				return data?.defines;
			}
			private set {
				if (null == data) {
					return;
				}
				data.defines = value;
			}
		}

		/// <summary>
		/// 显示宏定义窗口.
		/// </summary>
		[MenuItem("Tools/Defines", false, 800)]
		static void ShowDefinesWindow() {

			//创建窗口	
			var window = UtilsWindow.CreateWindow<DefinesWindow, DefinesConfInfo>(ConfInfo);	
			if (null == window) {
				Loger.Error ("DefinesWindow::ShowDefinesWindow() -> Create Window Failed!!");
				return;
			}
			window.Show();
		}

		/// <summary>
        /// 窗口类不要写构造函数，初始化写在Awake里
        /// </summary>
        void Awake()
        {
            if (false == Init(JsonFileDir))
            {
	            this.Error("Awake()::DefinesWindow Init Failed!!!");
            }
        }

		/// <summary>
		/// 追加宏.
		/// </summary>
		/// <param name="iName">宏定义</param>
		/// <param name="iAndroid">安卓标识位</param>
		/// <param name="iIOS">iOS标识位</param>
		private void AddDefine(string iName, bool iAndroid, bool iIOS) {
		
			if (string.IsNullOrEmpty (iName) || null == Defines) {
				return;
			}
			var isExist = false;
			foreach (var define in data.defines) {
				if (string.IsNullOrEmpty (define.name)) {
					continue;
				}

				if (!iName.Equals(define.name)) continue;
				define.android = iAndroid;
				define.iOS = iIOS;
				isExist = true;
				break;
			}
			if (false == isExist) {
				var define = DefineInfo.Create(iName, iAndroid, iIOS);
				Defines.Add (define);
			}

			// 按名字排序
			Defines.Sort((iX,iY) => (string.Compare(iX.name, iY.name, StringComparison.Ordinal)));

			this.Info("AddDefine -> Name:{0} Andorid:{1} iOS:{2}", 
				iName, iAndroid, iIOS);

		}

		/// <summary>
		/// 删除宏.
		/// </summary>
		/// <param name="iDelDefineIdx">删除的宏索引</param>
		private void DelDefine(int iDelDefineIdx) {

			if (-1 >= iDelDefineIdx || null == Defines) {
				return;
			}
			IsPause = true;
			var delDefine = Defines[iDelDefineIdx];
			Defines.RemoveAt (iDelDefineIdx);
		
			// 按名字排序
			Defines.Sort((iX,iY) => (string.Compare(iX.name, iY.name, StringComparison.Ordinal)));

			this.Info("DelDefine -> Name:{0} Andorid:{1} iOS:{2}", 
				delDefine.name, delDefine.android, delDefine.iOS);
			IsPause = false;
		}
			
#region Implement

		/// <summary>
		/// 清空按钮点击事件.
		/// </summary>
		protected override void OnClearClick() {
			IsPause = true;
			base.OnClearClick ();
			ConfInfo.inputNewDefine = null;
			IsPause = false;
		}

		/// <summary>
		/// 初始化窗口尺寸信息.
		/// </summary>
		/// <param name="iDisplayRect">表示范围.</param>
		protected override void InitWindowSizeInfo(Rect iDisplayRect) {
			base.InitWindowSizeInfo (iDisplayRect);
			var scrollViewRect = iDisplayRect;

			// 标题行
			scrollViewRect.y += LineHeight;
			scrollViewRect.height -= LineHeight;

			// 输入框
			scrollViewRect.height -= LineHeight;

			// 清空，导入，导出按钮行
			scrollViewRect.height -= LineHeight * 1.5f;

			ScrollViewRect = scrollViewRect;

			this.Info("InitWindowSizeInfo ScrollViewRect(X:{0} Y:{1} Width:{2} Height:{3})",
				ScrollViewRect.x, ScrollViewRect.y, ScrollViewRect.width, ScrollViewRect.height);

		}

		/// <summary>
		/// 绘制WindowGUI.
		/// </summary>
		protected override void OnWindowGui ()
		{

			var backgroundStyle = GUIEditorHelper.CloneStyle("ProfilerScrollviewBackground");
			if (null != backgroundStyle)
			{
				backgroundStyle.overflow.left = 5;
				backgroundStyle.overflow.right = 5;
				backgroundStyle.overflow.top = 5;
				backgroundStyle.overflow.bottom = 5;
			}
			// 列表
			_scrollViewStartPos = EditorGUILayout.BeginScrollView (
				_scrollViewStartPos, 
				backgroundStyle,
				GUILayout.Width(ScrollViewRect.width), 
				GUILayout.Height(ScrollViewRect.height));
			if(null != Defines) {
				for(var idx = 0; idx < Defines.Count; ++idx) {
					var defineInfo = Defines[idx];
					if (string.IsNullOrEmpty (defineInfo.name)) {
						continue;
					}
					EditorGUILayout.BeginHorizontal ();
					EditorGUILayout.LabelField (defineInfo.name, 
						GUILayout.Width(ScrollViewRect.width - 175.0f), GUILayout.Height(LineHeight));

					EditorGUILayout.LabelField ("Android", 
						GUILayout.Width(48.0f), GUILayout.Height(LineHeight));
					defineInfo.android = EditorGUILayout.Toggle(defineInfo.android, 
						GUILayout.Width(10.0f), GUILayout.Height(LineHeight));

					EditorGUILayout.LabelField ("iOS", 
						GUILayout.Width(22.0f), GUILayout.Height(LineHeight));
					defineInfo.iOS = EditorGUILayout.Toggle(defineInfo.iOS, 
						GUILayout.Width(10.0f), GUILayout.Height(LineHeight));

					if(GUILayout.Button("Del",GUILayout.Width(40.0f)))
					{
						// 删除宏
						DelDefine(idx);
						InputNewDefine = null;
					}

					EditorGUILayout.EndHorizontal ();
				}
			}
			EditorGUILayout.EndScrollView ();

			// 追加宏
			EditorGUILayout.BeginHorizontal ();
			EditorGUILayout.LabelField ("追加宏:", 
				GUILayout.Width(40.0f), GUILayout.Height(LineHeight));
			InputNewDefine = EditorGUILayout.TextField (
				InputNewDefine, GUILayout.Width(195.0f), GUILayout.Height(ConfInfo.LineHeight));
			     
			EditorGUILayout.LabelField ("Android", 
				GUILayout.Width(48.0f), GUILayout.Height(LineHeight));
			InputAndroidSelected = EditorGUILayout.Toggle(InputAndroidSelected, 
				GUILayout.Width(10.0f), GUILayout.Height(LineHeight));

			EditorGUILayout.LabelField ("iOS", 
				GUILayout.Width(22.0f), GUILayout.Height(LineHeight));
			InputIOSSelected = EditorGUILayout.Toggle(InputIOSSelected, 
				GUILayout.Width(10.0f), GUILayout.Height(LineHeight));

			if(GUILayout.Button("Add",GUILayout.Width(40.0f)))
			{
				// 追加宏
				AddDefine(InputNewDefine, InputAndroidSelected, InputIOSSelected);
				InputNewDefine = null;
				InputAndroidSelected = false;
				InputIOSSelected = false;
			}
			EditorGUILayout.EndHorizontal ();

			Repaint ();
		}

		/// <summary>
		/// 应用导入数据数据.
		/// </summary>
		/// <param name="iData">数据.</param>
		/// <param name="iForceClear">强制清空标志位.</param>
		protected override void ApplyImportData (DefinesData iData, bool iForceClear) {

			if (null == iData) {
				return;
			}

			// 清空
			if (iForceClear) {
				Clear ();
			}
			data.defines.AddRange(iData.defines);
			data.defines.Sort((iX,iY) => (string.Compare(iX.name, iY.name, StringComparison.Ordinal)));

			UtilsAsset.SetAssetDirty (this);
		}

#endregion
	}
}
