/*
 * Author: Jonatan Johansson
 * Updated: 2020-11-17
 * Project: https://github.com/smeas/Unity-Editor-Utilities
 */

using System;
using UnityEngine;

/// <summary>
/// Display a tag picker for a string field in the inspector.
/// </summary>
public class TagAttribute : PropertyAttribute { }


#if UNITY_EDITOR
namespace PropertyDrawers {
	using UnityEditor;
	using UnityEditorInternal;

	[CustomPropertyDrawer(typeof(TagAttribute))]
	public class TagPropertyDrawer : PropertyDrawer {
		private readonly GUIContent editStringText = new GUIContent("Edit String" ,"Edit the raw tag string");
		private bool editRaw;

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
			HandleContextMenu(position);

			if (editRaw) {
				EditorGUI.PropertyField(position, property, label);
			}
			else {
				string value = property.stringValue;
				label = EditorGUI.BeginProperty(position, label, property);

				Color oldColor = GUI.backgroundColor;
				if (!EditorGUI.showMixedValue && value != "" && Array.IndexOf(InternalEditorUtility.tags, value) == -1) {
					label.tooltip = "*Undefined tag* " + label.tooltip;
					GUI.backgroundColor = Color.yellow;
				}

				property.stringValue = EditorGUI.TagField(position, label, property.stringValue);
				GUI.backgroundColor = oldColor;
				EditorGUI.EndProperty();
			}
		}

		private void HandleContextMenu(Rect position) {
			// Only show when right clicking on the value to preserve access the prefab override context menu.
			position.x += EditorGUIUtility.labelWidth;
			position.width -= EditorGUIUtility.labelWidth;

			Event e = Event.current;
			if (e.type == EventType.MouseDown && e.button == 1 && position.Contains(e.mousePosition)) {
				var menu = new GenericMenu();
				menu.AddItem(editStringText, editRaw, () => editRaw = !editRaw);
				menu.ShowAsContext();
			}
		}
	}
}
#endif