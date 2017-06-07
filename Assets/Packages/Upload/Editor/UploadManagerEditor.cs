using UnityEngine;
using UnityEditor;
using System.Collections;
using Common;

namespace Upload {
	
	/// <summary>
	/// 上传管理器器编辑器.
	/// </summary>
	[CustomEditor(typeof(UploadManager))]  
	public class UploadManagerEditor : Editor {

		SerializedProperty m_serializedTrigger = null; 

		/// <summary>
		/// Raises the enable event.
		/// </summary>
		void OnEnable() {
			m_serializedTrigger = serializedObject.FindProperty("_uploadEvents"); 
		}

		public override void OnInspectorGUI ()  {
			serializedObject.Update();

			UploadManager manager = target as UploadManager; 
			if (manager != null) {
				manager._uploadBar = EditorGUILayout.ObjectField("UploadBar", 
					manager._uploadBar, typeof(UIProgressBar), true) as UIProgressBar;
			}

			EditorGUILayout.PropertyField(m_serializedTrigger,true);

			serializedObject.ApplyModifiedProperties();  
		}
	}
}
