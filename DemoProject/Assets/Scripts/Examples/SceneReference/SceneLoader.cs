using UnityEngine;

public class SceneLoader : MonoBehaviour {
	[SerializeField] private SceneReference menu;
	[SerializeField] private SceneReference[] levels;

	public void LoadMenu() {
		menu.Load();
	}

	public void LoadLevel(int index) {
		levels[index].Load();
	}
}