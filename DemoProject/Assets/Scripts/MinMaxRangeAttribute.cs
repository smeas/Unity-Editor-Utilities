// Draws a min/max slider for a Vector2 field.
//
// Updated: 2020-12-04

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

using UnityEngine;

/// <summary>
/// Display a min max slider for a Vector2 field in the inspector.
/// </summary>
public class MinMaxRangeAttribute : PropertyAttribute {
	public float Min { get; }
	public float Max { get; }

	public MinMaxRangeAttribute(float min, float max) {
		Min = min;
		Max = max;
	}
}


#if UNITY_EDITOR
namespace PropertyDrawers {
	using UnityEditor;

	[CustomPropertyDrawer(typeof(MinMaxRangeAttribute))]
	public class MinMaxRangePropertyDrawer : PropertyDrawer {
		private const float FieldSize = 45f;
		private const float FieldMargin = 5f;

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
			if (property.propertyType != SerializedPropertyType.Vector2) {
				Debug.LogError($"{nameof(MinMaxRangeAttribute)} is only supported on fields of type Vector2.");
				EditorGUI.PropertyField(position, property, label, true);
				return;
			}

			MinMaxRangeAttribute attr = (MinMaxRangeAttribute)attribute;
			Vector2 value = property.vector2Value;

			label = EditorGUI.BeginProperty(position, label, property);

			position = EditorGUI.PrefixLabel(position,label);
			Rect minFieldPos = position;
			Rect maxFieldPos = position;
			Rect sliderPos = position;

			minFieldPos.xMax = minFieldPos.xMin + FieldSize;
			maxFieldPos.xMin = maxFieldPos.xMax - FieldSize;
			sliderPos.xMin = minFieldPos.xMax + FieldMargin;
			sliderPos.xMax = maxFieldPos.xMin - FieldMargin;

			value.x = Mathf.Clamp(EditorGUI.FloatField(minFieldPos, value.x), attr.Min, value.y);
			EditorGUI.MinMaxSlider(sliderPos, ref value.x, ref value.y, attr.Min, attr.Max);
			value.y = Mathf.Clamp(EditorGUI.FloatField(maxFieldPos, value.y), value.x, attr.Max);

			EditorGUI.EndProperty();

			property.vector2Value = value;
		}
	}
}
#endif