using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization; 
using UnityEditor;
using System;
using System.Collections;
using Common;

namespace Download {

	/// <summary>
	/// 下载管理器编辑器.
	/// </summary>
	[CustomEditor(typeof(DownloadManager))]  
	public class DownloadManagerEditor : Editor {

		SerializedProperty m_serializedTrigger = null; 

		/// <summary>
		/// Raises the enable event.
		/// </summary>
		void OnEnable() {
			m_serializedTrigger = serializedObject.FindProperty("_downloadEvents"); 
		}

		public override void OnInspectorGUI ()  {
			serializedObject.Update();

			DownloadManager manager = target as DownloadManager; 
			if (manager != null) {
				manager._downloadBar = EditorGUILayout.ObjectField("DownloadBar", 
					manager._downloadBar, typeof(UIProgressBar), true) as UIProgressBar;
			}

			EditorGUILayout.PropertyField(m_serializedTrigger,true);

			serializedObject.ApplyModifiedProperties();  
		}

	}
}
