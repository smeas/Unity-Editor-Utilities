using UnityEngine;

public class LoadSceneOnStart : MonoBehaviour {
	[SerializeField] private SceneReference sceneToLoad;

	private void Start() {
		Debug.Log($"Loading scene: {sceneToLoad.ScenePath}");
		sceneToLoad.Load();
	}
}