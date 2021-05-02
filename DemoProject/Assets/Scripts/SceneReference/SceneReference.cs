/*
 * Author: Jonatan Johansson
 * Updated: 2021-05-02
 */

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// A reference to a scene which you can select from the inspector.
/// </summary>
[Serializable]
public struct SceneReference : ISerializationCallbackReceiver {
	// Only used in builds
	[SerializeField] private int buildIndex;

#if UNITY_EDITOR
	// Only used in the editor
	[SerializeField] private SceneAsset sceneAsset;
#endif

	/// <summary>
	/// Returns the build index of the scene.
	/// </summary>
	public int BuildIndex {
		get {
		#if UNITY_EDITOR
			return SceneUtility.GetBuildIndexByScenePath(ScenePath);
		#else
			return buildIndex;
		#endif
		}
	}

	/// <summary>
	/// Returns the path to the scene relative to the project folder, e.g. "Assets/Scenes/MyScene.unity".
	/// </summary>
	public string ScenePath {
		get {
		#if UNITY_EDITOR
			if (sceneAsset != null)
				return AssetDatabase.GetAssetPath(sceneAsset);
			return "";
		#else
			return SceneUtility.GetScenePathByBuildIndex(buildIndex);
		#endif
		}
	}

	// Convenience methods for loading the scene.
	public void Load() => SceneManager.LoadScene(BuildIndex);
	public void Load(LoadSceneMode mode) => SceneManager.LoadScene(BuildIndex, mode);
	public AsyncOperation LoadAsync() => SceneManager.LoadSceneAsync(BuildIndex);
	public AsyncOperation LoadAsync(LoadSceneMode mode) => SceneManager.LoadSceneAsync(BuildIndex, mode);

	void ISerializationCallbackReceiver.OnBeforeSerialize() {
	#if UNITY_EDITOR
		// Update the buildIndex field before serialization so that it will be correct in builds.
		buildIndex = BuildIndex;
	#endif
	}

	void ISerializationCallbackReceiver.OnAfterDeserialize() { }

	// An implicit conversion to build index allows for passing a SceneReference directly to methods like
	// SceneManager.LoadScene(int). It also makes it easier to update old code to use SceneReference.
	public static implicit operator int(SceneReference sceneReference) => sceneReference.BuildIndex;
}


#if UNITY_EDITOR
namespace PropertyDrawers {
	[CustomPropertyDrawer(typeof(SceneReference))]
	public class SceneReferencePropertyDrawer : PropertyDrawer {
		private bool hasScene;
		private SerializedProperty sceneAssetProperty;
		private SceneInfo sceneInfo;

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
			sceneAssetProperty = property.FindPropertyRelative("sceneAsset");

			// Check selected scene.
			SceneAsset sceneAsset = sceneAssetProperty.objectReferenceValue as SceneAsset;
			hasScene = sceneAsset != null;
			if (hasScene) sceneInfo = GetSceneInfo(sceneAsset);

			// Context menu.
			Event e = Event.current;
			if (e.type == EventType.MouseDown && e.button == 1 && position.Contains(e.mousePosition))
				DoContextMenu();

			// Indicator color.
			Color oldColor = GUI.backgroundColor;
			if (hasScene) {
				if (!sceneInfo.InBuildSettings) {
					GUI.backgroundColor = Color.red;
					label.tooltip = "*Not in build* " + label.tooltip;
				}
				else if (!sceneInfo.enabled) {
					GUI.backgroundColor = Color.yellow;
					label.tooltip = $"*Disabled at build index: {sceneInfo.buildIndex}* " + label.tooltip;
				}
				else {
					GUI.backgroundColor = Color.green;
					label.tooltip = $"*Enabled at build index: {sceneInfo.buildIndex}* " + label.tooltip;
				}
			}

			// Draw property.
			EditorGUI.PropertyField(position, sceneAssetProperty, label);

			// Restore color.
			GUI.backgroundColor = oldColor;
		}

		private void DoContextMenu() {
			if (!hasScene) return;
			GenericMenu menu = new GenericMenu();

			if (hasScene) {
				if (sceneInfo.InBuildSettings)
					menu.AddItem(new GUIContent("Remove scene from build settings"), false, RemoveFromBuild, sceneAssetProperty);
				else
					menu.AddItem(new GUIContent("Add scene to build settings"), false, AddToBuild, sceneAssetProperty);

				if (sceneInfo.InBuildSettings)
					menu.AddItem(new GUIContent("Enabled in build"), sceneInfo.enabled, ToggleEnableInBuild, sceneAssetProperty);
			}

			menu.ShowAsContext();
		}


		//
		// Context actions
		//

		private static void AddToBuild(object arg) {
			((SerializedProperty)arg).serializedObject.Update();
			if (!TryGetSceneFromProperty((SerializedProperty)arg, out SceneInfo sceneInfo)) return;
			if (!sceneInfo.InBuildSettings) {
				EditorBuildSettingsScene[] buildScenes = EditorBuildSettings.scenes
					.Append(new EditorBuildSettingsScene(sceneInfo.path, true)).ToArray();
				EditorBuildSettings.scenes = buildScenes;
			}
		}

		private static void RemoveFromBuild(object arg) {
			SerializedProperty property = (SerializedProperty)arg;
			property.serializedObject.Update();
			if (!TryGetSceneFromProperty(property, out SceneInfo scene)) return;
			if (!scene.InBuildSettings) return;

			List<EditorBuildSettingsScene> buildScenes = EditorBuildSettings.scenes.ToList();
			int index = scene.buildIndex;
			if (index < buildScenes.Count && buildScenes[index].path == scene.path) {
				buildScenes.RemoveAt(index);
				EditorBuildSettings.scenes = buildScenes.ToArray();
			}
		}

		private static void ToggleEnableInBuild(object arg) {
			SerializedProperty property = (SerializedProperty)arg;
			property.serializedObject.Update();
			if (!TryGetSceneFromProperty(property, out SceneInfo scene)) return;
			if (!scene.InBuildSettings) return;

			EditorBuildSettingsScene[] buildScenes = EditorBuildSettings.scenes;
			int index = scene.buildIndex;
			if (index < buildScenes.Length && buildScenes[index].path == scene.path) {
				buildScenes[index].enabled = !scene.enabled;
				EditorBuildSettings.scenes = buildScenes;
			}
		}

		//
		// Helpers
		//

		private struct SceneInfo {
			public string path;
			public bool enabled;
			public int buildIndex;

			public bool InBuildSettings => buildIndex != -1;
			public bool IsValid => path != null;
		}

		private static SceneInfo GetSceneInfo(SceneAsset sceneAsset) {
			string assetPath = AssetDatabase.GetAssetPath(sceneAsset);
			if (assetPath == null) return new SceneInfo();

			EditorBuildSettingsScene[] buildScenes = EditorBuildSettings.scenes;
			int index = Array.FindIndex(buildScenes, buildScene => buildScene.path == assetPath);

			return new SceneInfo {
				path = assetPath,
				buildIndex = index,
				enabled = index != -1 && buildScenes[index].enabled
			};
		}

		private static bool TryGetSceneFromProperty(SerializedProperty property, out SceneInfo sceneInfo) {
			sceneInfo = default;
			var sceneAsset = property.objectReferenceValue as SceneAsset;
			if (sceneAsset == null) return false;

			sceneInfo = GetSceneInfo(sceneAsset);
			return sceneInfo.IsValid;
		}
	}
}
#endif