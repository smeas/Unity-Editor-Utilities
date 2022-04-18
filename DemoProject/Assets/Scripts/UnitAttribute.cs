// Draws a little string in the right of a number or string field in the inspector. Useful for indicating
// in what unit a value is.
//
// Updated: 2022-03-03

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

using UnityEngine;

public class UnitAttribute : PropertyAttribute
{
	public string Value { get; }

	public UnitAttribute(string value)
	{
		Value = value;
	}
}

#if UNITY_EDITOR
namespace PropertyDrawers
{
	using UnityEditor;

	[CustomPropertyDrawer(typeof(UnitAttribute))]
	public class UnitPropertyDrawer : PropertyDrawer
	{
		private GUIStyle m_unitLabelStyle;

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			EditorGUI.PropertyField(position, property, label, true);

			DrawUnit(position);
		}

		private void DrawUnit(Rect position)
		{
			m_unitLabelStyle ??= new GUIStyle()
			{
				fontSize = 9,
				alignment = TextAnchor.MiddleRight,
				normal =
				{
					textColor = Color.gray,
				},
			};

			string unitString = ((UnitAttribute)attribute).Value;
			float unitRectWidth = unitString.Length * 8f + 8f;
			Rect unitRect = new Rect(
				position.x + position.width - unitRectWidth - 5, position.y,
				unitRectWidth, position.height);

			EditorGUI.LabelField(unitRect, unitString, m_unitLabelStyle);
		}
	}
}
#endif
