using UnityEditor;
using UnityEngine;
using Packages.Logs;

namespace Packages.Common.Editor {

	[CustomPropertyDrawer(typeof(InspectorReadOnlyAttribute))]
	public class InspectorReadOnlyDrawer : PropertyDrawer {
		
		public override float GetPropertyHeight(SerializedProperty iProperty, GUIContent iLabel)
		{
			return EditorGUI.GetPropertyHeight(iProperty, iLabel, true);
		}

		public override void OnGUI(Rect iPosition, SerializedProperty iProperty, GUIContent iLabel)
		{
			GUI.enabled = false;
			var readOnly = (InspectorReadOnlyAttribute)attribute;
			if (null != readOnly)
			{
				Loger.Info($"InspectorReadOnlyDrawer::OnGUI():{readOnly.GetType().Name}");
			}
			EditorGUI.PropertyField(iPosition, iProperty, iLabel, true); 
			GUI.enabled = true;
		}

	}
}
