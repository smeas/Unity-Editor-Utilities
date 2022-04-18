// Tool window for visualizing colliders and triggers in the scene.
//
// Updated: 2021-09-27

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

using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

public class ColliderVisualizerWindow : EditorWindow {
	private const string MenuItemPath = "Tools/Collider Visualizer";
	private const string StatePrefsKey = nameof(ColliderVisualizerWindow) + "_state";

	private static ColliderVisualizerWindow window;

	[SerializeField] private bool showColliders = true;
	[SerializeField] private bool showTriggers = true;
	[SerializeField] private bool showDisabled;

	[SerializeField] private bool layerFilterFoldout;
	[SerializeField] private LayerMask layerFilter = -1;
	[SerializeField] private List<string> tags = new List<string>();

	[SerializeField] private bool colliderTypeFoldout;
	[SerializeField] private bool showBoxColliders = true;
	[SerializeField] private bool showSphereColliders = true;
	[SerializeField] private bool showCapsuleColliders = true;
	[SerializeField] private bool showMeshColliders;
	[SerializeField] private bool showConvexMeshColliders;

	[SerializeField] private Color colliderColor = new Color(0.18f, 0.81f, 0.17f);
	[SerializeField] private Color triggerColor = Color.yellow;
	[SerializeField] private bool fill = true;
	[SerializeField, Range(0, 1)] private float fillOpacity = 0.1f;

	private SerializedObject serializedObject;
	private Vector2 scrollPosition;

	[MenuItem(MenuItemPath)]
	private static void ShowWindow() {
		window = GetWindow<ColliderVisualizerWindow>();
		window.titleContent = new GUIContent("Collider Visualizer");
		window.Show();
	}

	private void OnEnable() {
		window = this;
		serializedObject = new SerializedObject(this);

		if (EditorPrefs.HasKey(StatePrefsKey))
			JsonUtility.FromJsonOverwrite(EditorPrefs.GetString(StatePrefsKey), this);
	}

	private void OnDisable() {
		EditorPrefs.SetString(StatePrefsKey, JsonUtility.ToJson(this));
	}

	private void OnGUI() {
		serializedObject.UpdateIfRequiredOrScript();

		EditorGUI.BeginChangeCheck();

		ToolbarGUI();

		scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
		DrawFiltersGUI();
		DrawColorsGUI();
		EditorGUILayout.EndScrollView();

		if (EditorGUI.EndChangeCheck())
			SceneView.RepaintAll();

		serializedObject.ApplyModifiedProperties();
	}

	private void Reset() {
		showColliders = true;
		showTriggers = true;
		showBoxColliders = true;
		showSphereColliders = true;
		showCapsuleColliders = true;
		showMeshColliders = false;
		showConvexMeshColliders = false;
		layerFilter = -1;
		tags.Clear();

		colliderColor = new Color(0.18f, 0.81f, 0.17f);
		triggerColor = Color.yellow;
		fill = true;
		fillOpacity = 0.1f;
	}

	private void ToolbarGUI() {
		EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
		GUILayout.FlexibleSpace();
		if (GUILayout.Button("Reset", EditorStyles.toolbarButton))
			Reset();
		EditorGUILayout.EndHorizontal();
	}

	private void DrawColorsGUI() {
		EditorGUILayout.LabelField("Colors", EditorStyles.boldLabel);
		EditorGUILayout.BeginVertical(EditorStyles.helpBox);

		DrawProperty(nameof(colliderColor));
		DrawProperty(nameof(triggerColor));
		using (new EditorGUI.DisabledScope(!DrawProperty(nameof(fill)).boolValue))
			DrawProperty(nameof(fillOpacity));

		EditorGUILayout.EndVertical();
	}

	private void DrawFiltersGUI() {
		EditorGUILayout.LabelField("Filters", EditorStyles.boldLabel);
		EditorGUILayout.BeginVertical(EditorStyles.helpBox);

		DrawProperty(nameof(showColliders));
		DrawProperty(nameof(showTriggers));
		DrawProperty(nameof(showDisabled));

		// Collider type filter foldout
		colliderTypeFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(colliderTypeFoldout, "Collider Types");

		if (colliderTypeFoldout) {
			DrawProperty(nameof(showBoxColliders), new GUIContent("Box"));
			DrawProperty(nameof(showSphereColliders), new GUIContent("Sphere"));
			DrawProperty(nameof(showCapsuleColliders), new GUIContent("Capsule"));
			DrawProperty(nameof(showMeshColliders), new GUIContent("Mesh"));
			DrawProperty(nameof(showConvexMeshColliders), new GUIContent("Convex Mesh*", "The normal mesh will be displayed"));
		}

		EditorGUILayout.EndFoldoutHeaderGroup();

		// Layer filter foldout
		layerFilterFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(layerFilterFoldout, "Layers");

		if (layerFilterFoldout) {
			SerializedProperty layersFilterProperty = serializedObject.FindProperty(nameof(layerFilter));

			// Quick select buttons
			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button("All")) layersFilterProperty.intValue = -1;
			if (GUILayout.Button("None")) layersFilterProperty.intValue = 0;
			EditorGUILayout.EndHorizontal();

			// Layer mask checkboxes
			EditorGUI.BeginChangeCheck();
			int concatenatedLayersMask = InternalEditorUtility.LayerMaskToConcatenatedLayersMask(layerFilter);
			string[] layers = InternalEditorUtility.layers;

			for (int i = 0; i < layers.Length; i++) {
				if (EditorGUILayout.Toggle(layers[i], (concatenatedLayersMask & (1 << i)) != 0))
					concatenatedLayersMask |= 1 << i;
				else
					concatenatedLayersMask &= ~(1 << i);
			}

			if (EditorGUI.EndChangeCheck())
				layersFilterProperty.intValue =
					InternalEditorUtility.ConcatenatedLayersMaskToLayerMask(concatenatedLayersMask);
		}

		EditorGUILayout.EndFoldoutHeaderGroup();

		// Tag filter
		DrawProperty(nameof(tags));

		EditorGUILayout.EndVertical();
	}

	private SerializedProperty DrawProperty(string propertyName, GUIContent label = null) {
		SerializedProperty property = serializedObject.FindProperty(propertyName);
		if (label != null)
			EditorGUILayout.PropertyField(property, label, true);
		else
			EditorGUILayout.PropertyField(property, true);

		return property;
	}

	private Color GetColor(Collider target, bool isFill = false) {
		Color color = target.isTrigger ? triggerColor : colliderColor;
		if (isFill)
			color.a = fillOpacity;

		return color;
	}

	private bool CheckFilter(Collider target) {
		if (!showDisabled && !target.enabled)
			return false;

		bool isTrigger = target.isTrigger;
		if (isTrigger && !showTriggers || !isTrigger && !showColliders)
			return false;

		if ((layerFilter.value & (1 << target.gameObject.layer)) == 0)
			return false;

		if (tags.Count > 0 && !tags.Contains(target.tag))
			return false;

		return true;
	}

	// I couldn't get any combination of flags to do what I want here. Setting all bits seems to work though for some reason...
	[DrawGizmo((GizmoType)~0)]
	private static void DrawGizmo(Collider target, GizmoType gizmoType) {
		if (window == null)
			return;

		if (!window.CheckFilter(target))
			return;

		switch (target) {
			case BoxCollider box: {
				if (!window.showBoxColliders)
					break;

				Gizmos.matrix = target.transform.localToWorldMatrix;
				if (window.fill) {
					Gizmos.color = window.GetColor(target, true);
					Gizmos.DrawCube(box.center, box.size);
				}

				Gizmos.color = window.GetColor(target);
				Gizmos.DrawWireCube(box.center, box.size);
				break;
			}

			case SphereCollider sphere: {
				if (!window.showSphereColliders)
					break;

				Gizmos.matrix = target.transform.localToWorldMatrix;
				if (window.fill) {
					Gizmos.color = window.GetColor(target, true);
					Gizmos.DrawSphere(sphere.center, sphere.radius);
				}

				Gizmos.color = window.GetColor(target);
				Gizmos.DrawWireSphere(sphere.center, sphere.radius);
				break;
			}

			case CapsuleCollider capsule: {
				if (!window.showCapsuleColliders)
					break;

				Handles.matrix = target.transform.localToWorldMatrix;
				Handles.color = window.GetColor(target);
				DrawWireCapsule(capsule.center, capsule.height, capsule.radius);
				// We're not drawing a fill for capsules, as there is no built-in function for that. Just drawing a
				// scaled version of the built-in capsule mesh wouldn't work very well, as the scaling would cause
				// stretching. So, one would have to generate a capsule mesh based on the height and radius, and draw
				// that with a flat color.
				break;
			}

			case MeshCollider mesh: {
				if (mesh.sharedMesh == null)
					break;

				if (window.showMeshColliders && !mesh.convex || window.showConvexMeshColliders && mesh.convex) {
					Gizmos.matrix = target.transform.localToWorldMatrix;
					Gizmos.color = window.GetColor(target);
					// There is no way to get the generated convex mesh from a mesh collider. So the normal mesh will
					// have to do for both convex, and non-convex, mesh colliders.
					Gizmos.DrawWireMesh(mesh.sharedMesh);
				}

				break;
			}
		}
	}

	// There is no built in function for drawing a wireframe capsule.
	private static void DrawWireCapsule(Vector3 center, float height, float radius) {
		float cylinderHeight = Mathf.Max(0f, height - radius * 2f);
		float pointOffset = cylinderHeight / 2f;

		// Top
		Vector3 topPoint = Vector3.up * pointOffset;
		Handles.DrawWireArc(center + topPoint, Vector3.left, Vector3.back, -180f, radius);
		Handles.DrawWireArc(center + topPoint, Vector3.back, Vector3.left, 180f, radius);
		Handles.DrawWireDisc(center + topPoint, Vector3.up, radius);

		// Bottom
		Vector3 bottomPoint = Vector3.down * pointOffset;
		Handles.DrawWireArc(center + bottomPoint, Vector3.left, Vector3.back, 180f, radius);
		Handles.DrawWireArc(center + bottomPoint, Vector3.back, Vector3.left, -180f, radius);
		Handles.DrawWireDisc(center + bottomPoint, Vector3.up, radius);

		// Middle
		if (cylinderHeight > 0f) {
			Handles.DrawLine(center + new Vector3(radius, bottomPoint.y, 0),
			                 center + new Vector3(radius, topPoint.y, 0));
			Handles.DrawLine(center + new Vector3(-radius, bottomPoint.y, 0),
			                 center + new Vector3(-radius, topPoint.y, 0));
			Handles.DrawLine(center + new Vector3(0, bottomPoint.y, radius),
			                 center + new Vector3(0, topPoint.y, radius));
			Handles.DrawLine(center + new Vector3(0, bottomPoint.y, -radius),
			                 center + new Vector3(0, topPoint.y, -radius));
		}
	}
}