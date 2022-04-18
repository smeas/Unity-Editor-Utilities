// Used on a serialized array to give its elements their names from an enum.
//
// Updated: 2022-04-18

/* MIT License
 *
 * Copyright (c) 2022 Jonatan Johansson
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

public class EnumNamedArrayAttribute : PropertyAttribute
{
	public Type EnumType { get; }
	public bool EnforceArraySize { get; }

	public EnumNamedArrayAttribute(Type enumType, bool enforceArraySize = true)
	{
		EnumType = enumType;
		EnforceArraySize = enforceArraySize;
	}
}

#if UNITY_EDITOR
namespace PropertyDrawers
{
	using UnityEditor;

	[CustomPropertyDrawer(typeof(EnumNamedArrayAttribute))]
	public class EnumNamedArrayPropertyDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			EnumNamedArrayAttribute attr = (EnumNamedArrayAttribute)attribute;

			string[] enumNames = Enum.GetNames(attr.EnumType);

			if (TryGetArrayFromElementProperty(property, out SerializedProperty arrayProperty))
			{
				// ISSUE: Changing the size of the array while drawing causes an error message to be printed for each removed element.
				arrayProperty.arraySize = enumNames.Length;
			}

			if (TryGetPropertyIndex(property, out int index))
			{
				label.text = index < enumNames.Length
					? enumNames[index]
					: index.ToString();
			}

			EditorGUI.PropertyField(position, property, label);
		}

		private static bool TryGetArrayFromElementProperty(SerializedProperty property, out SerializedProperty arrayProperty)
		{
			arrayProperty = null;

			string path = property.propertyPath;
			if (!path.EndsWith("]")) return false;

			int length = path.LastIndexOf(".Array.data[", StringComparison.Ordinal);
			arrayProperty = property.serializedObject.FindProperty(path.Substring(0, length));
			return arrayProperty != null && arrayProperty.isArray;
		}

		private static bool TryGetPropertyIndex(SerializedProperty property, out int index)
		{
			index = -1;

			string path = property.propertyPath;
			if (!path.EndsWith("]")) return false;

			int begin = path.LastIndexOf('[') + 1;
			if (begin == -1) return false;
			int end = path.IndexOf(']', begin);
			if (end == -1) return false;
			if (end - begin <= 0) return false;

			string indexSubstr = path.Substring(begin, end - begin);
			return int.TryParse(indexSubstr, out index);
		}
	}
}
#endif

