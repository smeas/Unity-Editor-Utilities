// Updated: 2020-11-17

/* MIT License
 *
 * Copyright (c) 2021 Jonatan Johansson
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
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