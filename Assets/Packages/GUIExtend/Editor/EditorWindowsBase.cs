using UnityEditor;
using UnityEngine;

namespace Packages.GUIExtend.Editor
{
    /// <summary>
    /// 编辑器窗口接口
    /// </summary>
    public interface IEditorWindow
    {
        /// <summary>
        /// 重绘标识位
        /// </summary>
        bool IsRepaint { get; set; }
        
        /// <summary>
        /// 生成
        /// </summary>
        void Generate();

        /// <summary>
        /// 清空数据
        /// </summary>
        void ClearData();
    }
    
    public class EditorWindowsBase<T> : EditorWindow, ISerializationCallbackReceiver, IEditorWindow
        where T : IEditorWindow, new()
    {
        
#region Serialization

        protected IEditorWindow _editorWindow;
        protected T serializedMM_Target;
        
        public void OnBeforeSerialize () 
        { 
            if (_editorWindow is T window) serializedMM_Target = window;
        }
        public void OnAfterDeserialize () 
        { 
            if (null != serializedMM_Target)  _editorWindow = serializedMM_Target;
        }

#endregion

#region Implement

        /// <summary>
        /// 重绘标识位
        /// </summary>
        public bool IsRepaint { get; set; }
                
        /// <summary>
        /// 生成
        /// </summary>
        public virtual void Generate() {}
                
        /// <summary>
        /// 清空数据
        /// </summary>
        public virtual void ClearData() {}

#endregion

#region ControlFocus

        /// <summary>
        /// 移除控件焦点
        /// </summary>
        /// <param name="iControlName">控件名</param>
        protected void RemoveFocusOnControl(string iControlName = null)
        {
            GUIEditorUtils.RemoveFocusOnControl(iControlName);
        }
        
        /// <summary>
        /// 取得焦点控件名
        /// </summary>
        /// <returns>焦点控件名</returns>
        protected string GetFocusedControl()
        {
            return GUIEditorUtils.GetFocusedControl();
        }

#endregion

#region GUI

        /// <summary>
        /// GUI绘制
        /// </summary>
        protected void OnGUI() { DrawWindow(); if (IsRepaint) DrawWindow(); IsRepaint = false; }
        
        /// <summary>
        /// 绘制Window
        /// </summary>
        protected virtual void DrawWindow() {}

#endregion
        
    }    
}


