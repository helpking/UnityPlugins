using System;
using UnityEngine;
using UnityEditor;
using Packages.Common.Extend;

namespace Packages.Common.Base.Editor {

	/// <summary>
	/// 编辑器基类.
	/// </summary>
	public class EditorBase : UnityEditor.Editor {

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

#region GUI

		/// <summary>
		/// 重新绘制Inspector窗口
		/// </summary>
		public override void OnInspectorGUI ()  {
			// base.OnInspectorGUI ();
			serializedObject.Update ();
			if (null != target) {
				InitInspectorUI(target);
			}

			// 保存变化后的值
			serializedObject.ApplyModifiedProperties();
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
		/// <param name="iTitle">标签标题</param>
		/// <param name="iColor">标签文本颜色</param>
		/// <param name="iText">标签文本</param>
		/// <param name="iWidth">宽度</param>
		/// <param name="iHeight">高度</param>
		protected void DrawLabel(
			int iLevel, string iTitle,
			Color iColor, string iText = null, 
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
					if (string.IsNullOrEmpty(iText))
					{
						EditorGUILayout.LabelField (iTitle);
					}
					else
					{
						EditorGUILayout.LabelField(iTitle, iText);
					}
				} else if (LabelNoneWidth == iWidth)
				{
					if (string.IsNullOrEmpty(iText))
					{
						EditorGUILayout.LabelField (iTitle, GUILayout.Height(iHeight));
					}
					else
					{
						EditorGUILayout.LabelField (iTitle, iText, GUILayout.Height(iHeight));
					}
					
				} else if (RowNoneHeight == iHeight)
				{
					if (string.IsNullOrEmpty(iText))
					{
						EditorGUILayout.LabelField (iTitle, GUILayout.MaxWidth(iWidth));
					}
					else
					{
						EditorGUILayout.LabelField (iTitle, iText, GUILayout.MaxWidth(iWidth));
					}
					
				}
				else
				{
					if (string.IsNullOrEmpty(iText))
					{
						EditorGUILayout.LabelField (iTitle, 
							GUILayout.MaxWidth(iWidth), GUILayout.Height(iHeight));
					}
					else
					{
						EditorGUILayout.LabelField (iTitle, iText, 
							GUILayout.MaxWidth(iWidth), GUILayout.Height(iHeight));
					}
					
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
		/// <param name="iTitle">标签标题</param>
		/// <param name="iText">标签文本</param>
		protected void DrawShortLabel(int iLevel, string iTitle, string iText = null)
		{
			DrawLabel(iLevel, iTitle, Color.white, iText, LabelShortWidth);
		}
		
		/// <summary>
		/// 绘制标签（长度中，高度中）
		/// </summary>
		/// <param name="iLevel">层级(影响到左边缩进的余白)</param>
		/// <param name="iTitle">标签标题</param>
		/// <param name="iText">标签文本</param>
		protected void DrawMiddleLabel(int iLevel, string iTitle, string iText = null)
		{
			DrawLabel(iLevel, iTitle, Color.white, iText, LabelMiddleWidth);
		}
		
		/// <summary>
		/// 绘制标签（长度中，高度中）
		/// </summary>
		/// <param name="iLevel">层级(影响到左边缩进的余白)</param>
		/// <param name="iTitle">标签标题</param>
		/// <param name="iText">标签文本</param>
		protected void DrawLongLabel(int iLevel, string iTitle, string iText = null)
		{
			DrawLabel(iLevel, iTitle, Color.white, iText, LabelLongWidth);
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

#endregion

#region Inspector - Draw - Enum - SelectList

		/// <summary>
		/// 绘制下拉菜单 - 枚举体（长度无，高度无）
		/// </summary>
		/// <param name="iLevel">层级(影响到左边缩进的余白)</param>
		/// <param name="iTitle">标题</param>
		/// <param name="iSelectedValue">选中值</param>
		protected Enum DrawSelectList(
			int iLevel, string iTitle, 
			Enum iSelectedValue)
		{
			// 设定缩进
			for (var i = 0; i < iLevel; ++i)
			{
				++EditorGUI.indentLevel;
			}

			var ret = iSelectedValue;
			EditorGUILayout.BeginHorizontal ();
			ret = EditorGUILayout.EnumPopup (iTitle, iSelectedValue);
			EditorGUILayout.EndHorizontal();
			
			// 恢复缩进
			for (var i = 0; i < iLevel; ++i)
			{
				--EditorGUI.indentLevel;
			}
			return ret;
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
				iValue = EditorGUILayout.Slider(iLabelText,
					iValue, iMinVale, iMaxValue);
			} 
			else if (LabelNoneWidth == iLabelWidth)
			{
				iValue = EditorGUILayout.Slider(iLabelText,
					iValue, iMinVale, iMaxValue,
					GUILayout.Height(iHeight));
			}
			else if (RowNoneHeight == iHeight)
			{
				iValue = EditorGUILayout.Slider(iLabelText,
					iValue, iMinVale, iMaxValue);
			}
			else
			{
				iValue = EditorGUILayout.Slider(iLabelText,
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
			
			EditorGUILayout.BeginHorizontal ();
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
			EditorGUILayout.EndHorizontal();
			
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

#endregion

#region Inspector - Draw - Toggle(Single)

		/// <summary>
		/// 单个单选框值变更事件委托
		/// </summary>
		/// <param name="iIndex">值变更的索引</param>
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

#endregion

#region Inspector - Draw - Toggle(Multiply)

		/// <summary>
		/// 绘制单行多个选择项（默认：长度无限制，高度无限制）
		/// </summary>
		/// <param name="iLevel">层级(影响到左边缩进的余白)</param>
		/// <param name="iToggleText">选项标题文本</param>
		/// <param name="iValue">值</param>
		/// <param name="iLabelWidth">标题文本宽度</param>
		/// <param name="iHeight">高度</param>
		protected void DrawMultiplyToggle(
			int iLevel, string[] iToggleText, ref bool[] iValue, 
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
				for (var i = 0; i < iToggleText.Length; i++)
				{
					EditorGUILayout.LabelField (iToggleText[i]);
					iValue[i] = EditorGUILayout.Toggle(iValue[i],
						GUILayout.Width(LabelShortWidth));
				}
			}
			else if (LabelNoneWidth == iLabelWidth)
			{
				for (var i = 0; i < iToggleText.Length; i++)
				{
					EditorGUILayout.LabelField (iToggleText[i]);
					iValue[i] = EditorGUILayout.Toggle(iValue[i],
						GUILayout.Width(LabelShortWidth),
						GUILayout.Height(iHeight));
				}
			}
			else if (RowNoneHeight == iHeight)
			{
				for (var i = 0; i < iToggleText.Length; i++)
				{
					EditorGUILayout.LabelField (iToggleText[i], GUILayout.Width(iLabelWidth));
					iValue[i] = EditorGUILayout.Toggle(iValue[i], 
						GUILayout.Width(iLabelWidth));
				}
			}
			else
			{
				for (var i = 0; i < iToggleText.Length; i++)
				{
					EditorGUILayout.LabelField (iToggleText[i]);
					iValue[i] = EditorGUILayout.Toggle(iValue[i], 
						GUILayout.Width(iLabelWidth), GUILayout.Height(iHeight));
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
		/// 绘制单行多个选择项（默认：长度短，高度中）
		/// </summary>
		/// <param name="iLevel">层级(影响到左边缩进的余白)</param>
		/// <param name="iToggleText">选项标题文本</param>
		/// <param name="iValue">值</param>
		/// <param name="iHeight">高度</param>
		protected void DrawShortMultiplyToggle(
			int iLevel, string[] iToggleText, ref bool[] iValue,
			int iHeight = RowNoneHeight)
		{
			DrawMultiplyToggle(iLevel, iToggleText, ref iValue, LabelShortWidth, iHeight);	
		}

		/// <summary>
		/// 绘制单行多个选择项（默认：长度中，高度中）
		/// </summary>
		/// <param name="iLevel">层级(影响到左边缩进的余白)</param>
		/// <param name="iToggleText">选项标题文本</param>
		/// <param name="iValue">值</param>
		/// <param name="iHeight">高度</param>
		protected void DrawMiddleMultiplyToggle(
			int iLevel, string[] iToggleText, ref bool[] iValue,
			int iHeight = RowNoneHeight)
		{
			DrawMultiplyToggle(iLevel, iToggleText, ref iValue, LabelMiddleWidth, iHeight);	
		}

		/// <summary>
		/// 绘制单行多个选择项（默认：长度长，高度中）
		/// </summary>
		/// <param name="iLevel">层级(影响到左边缩进的余白)</param>
		/// <param name="iToggleText">选项标题文本</param>
		/// <param name="iValue">值</param>
		/// <param name="iHeight">高度</param>
		protected void DrawLongMultiplyToggle(
			int iLevel, string[] iToggleText, ref bool[] iValue,
			int iHeight = RowNoneHeight)
		{
			DrawMultiplyToggle(iLevel, iToggleText, ref iValue, LabelLongWidth, iHeight);	
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
		/// <param name="iLabelWidth"></param>
		/// <param name="iHeight"></param>
		protected void DrawToolBar(
			int iLevel, string[] iLabelText, ref int iSelectIndex,
			int iLabelWidth = LabelNoneWidth, int iHeight = RowNoneHeight)
		{
			
			if(null == iLabelText || 0 >= iLabelText.Length) return;
			
			// 设定缩进
			for (var i = 0; i < iLevel; ++i)
			{
				++EditorGUI.indentLevel;
			}

			// 空行
			EditorGUILayout.Space();
			if (LabelNoneWidth == iLabelWidth && RowNoneHeight == iHeight)
			{
				iSelectIndex = GUILayout.Toolbar (
					iSelectIndex, iLabelText);
			}
			else if (LabelNoneWidth == iLabelWidth)
			{
				iSelectIndex = GUILayout.Toolbar (
					iSelectIndex, iLabelText,
					GUILayout.Height(iHeight));
			}
			else if (RowNoneHeight == iHeight)
			{
				iSelectIndex = GUILayout.Toolbar (
					iSelectIndex, iLabelText,
					GUILayout.Width(iLabelWidth));
			}
			else
			{
				iSelectIndex = GUILayout.Toolbar (
					iSelectIndex, iLabelText,
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

#region Inspector - Draw - Vector2

		/// <summary>
		/// 绘制数据对象Vector2
		/// </summary>
		/// <param name="iLevel">层级(影响到左边缩进的余白)</param>
		/// <param name="iLabelText">文本</param>
		/// <param name="iValue">值</param>
		/// <param name="iLabelWidth">文本宽度</param>
		/// <param name="iHeight">高度</param>
		protected void DrawVector2Field(
			int iLevel, string iLabelText, ref Vector2 iValue,
			int iLabelWidth = LabelNoneWidth, int iHeight = RowNoneHeight)
		{
			// 设定缩进
			for (var i = 0; i < iLevel; ++i)
			{
				++EditorGUI.indentLevel;
			}
					
			EditorGUILayout.BeginHorizontal ();
			using (new ColorScope(Color.yellow))
			{
				if (LabelNoneWidth == iLabelWidth && RowNoneHeight == iHeight)
				{
					iValue = EditorGUILayout.Vector2Field(iLabelText, iValue);
				} else if (LabelNoneWidth == iLabelWidth)
				{
					iValue = EditorGUILayout.Vector2Field(iLabelText, iValue, 
						GUILayout.Height(iLabelWidth));
				} else if (RowNoneHeight == iHeight)
				{
					iValue = EditorGUILayout.Vector2Field(iLabelText, iValue, 
						GUILayout.Width(iLabelWidth));
				}
				else
				{
					iValue = EditorGUILayout.Vector2Field(iLabelText, iValue, 
						GUILayout.Width(iLabelWidth), 
						GUILayout.Height(iLabelWidth));
				}
			}
			EditorGUILayout.EndHorizontal();
					
			// 恢复缩进
			for (var i = 0; i < iLevel; ++i)
			{
				--EditorGUI.indentLevel;
			}
		}

#endregion

#region Inspector - Draw - Vector3

		/// <summary>
		/// 绘制数据对象Vector2
		/// </summary>
		/// <param name="iLevel">层级(影响到左边缩进的余白)</param>
		/// <param name="iLabelText">文本</param>
		/// <param name="iValue">值</param>
		/// <param name="iLabelWidth">文本宽度</param>
		/// <param name="iHeight">高度</param>
		protected void DrawVector3Field(
			int iLevel, string iLabelText, ref Vector3 iValue,
			int iLabelWidth = LabelNoneWidth, int iHeight = RowNoneHeight)
		{
			// 设定缩进
			for (var i = 0; i < iLevel; ++i)
			{
				++EditorGUI.indentLevel;
			}
							
			EditorGUILayout.BeginHorizontal ();
			using (new ColorScope(Color.yellow))
			{
				if (LabelNoneWidth == iLabelWidth && RowNoneHeight == iHeight)
				{
					iValue = EditorGUILayout.Vector3Field(iLabelText, iValue);
				} else if (LabelNoneWidth == iLabelWidth)
				{
					iValue = EditorGUILayout.Vector3Field(iLabelText, iValue, 
						GUILayout.Height(iLabelWidth));
				} else if (RowNoneHeight == iHeight)
				{
					iValue = EditorGUILayout.Vector3Field(iLabelText, iValue, 
						GUILayout.Width(iLabelWidth));
				}
				else
				{
					iValue = EditorGUILayout.Vector3Field(iLabelText, iValue, 
						GUILayout.Width(iLabelWidth), 
						GUILayout.Height(iLabelWidth));
				}
			} 
			EditorGUILayout.EndHorizontal();
							
			// 恢复缩进
			for (var i = 0; i < iLevel; ++i)
			{
				--EditorGUI.indentLevel;
			}
		}

#endregion

#region Draw - SerializedObject

		/// <summary>
		/// 绘制序列化对象名
		/// </summary>
		/// <param name="iLevel">层级(影响到左边缩进的余白)</param>
		/// <param name="iSerializedObjectName">序列化对象名</param>
		protected void DrawSerializedObject(
			int iLevel, string iSerializedObjectName)
		{
			// 设定缩进
			for (var i = 0; i < iLevel; ++i)
			{
				++EditorGUI.indentLevel;
			}
							
			// 音效
			EditorGUILayout.BeginHorizontal ();
			var sobj = serializedObject.FindProperty (iSerializedObjectName);
			if (null == sobj) {
				this.Error(
					"DrawSerializedObject():The SerializedObject is null or invalid(name:{0})!!!", 
					iSerializedObjectName);
				return;
			}
			EditorGUILayout.PropertyField (sobj, true);
			EditorGUILayout.EndHorizontal ();
							
			// 恢复缩进
			for (var i = 0; i < iLevel; ++i)
			{
				--EditorGUI.indentLevel;
			}
		}

#endregion

#region GUI - virtual

		/// <summary>
		/// 初始化
		/// </summary>
		/// <param name="iTarget">目标</param>
		protected virtual void InitInspectorUI (UnityEngine.Object iTarget) {
			//Info("InitInspectorUI()");
		}
		
#endregion

	}
}
