using UnityEditor;
using UnityEngine;

namespace Packages.GUIExtend
{
    /// <summary>
    /// GUI编辑器Utils
    /// </summary>
    public static class GUIEditorUtils
    {
        /// <summary>
        /// 移除控件焦点
        /// </summary>
        /// <param name="iControlName">控件名</param>
        public static void RemoveFocusOnControl(string iControlName = null)
        {
            var control = string.IsNullOrEmpty(iControlName) ? "Temp" : iControlName;
            
            // GUI.FocusControl(null) is not reliable, so creating a temporary control and focusing on it
            GUI.SetNextControlName(control);
            EditorGUI.FloatField(new Rect(-10,-10,0,0), 0);
            GUI.FocusControl(control);
        }

        /// <summary>
        /// 取得焦点控件名
        /// </summary>
        /// <returns>焦点控件名</returns>
        public static string GetFocusedControl()
        {
            return GUI.GetNameOfFocusedControl();
        }
    }
}
