# Unity Editor Utilities

A collection of single file editor utilities for the Unity game engine.

## Scene Reference ([link](DemoProject/Assets/Scripts/SceneReference/SceneReference.cs))

A utility type that holds a reference to a scene. The type is efficiently
implemented as a struct that stores just the build index of the scene (only in
builds), making it essentially equivalent to just storing an int.

![Scene reference demo](Images/SceneReference_Demo.gif)

### Features
- No runtime overhead (in builds)
- Rename scenes without needing to update references
- Warns you if a scene is not in the build settings
- Easily add/remove scenes from build settings right in the inspector

### Usage

```c#
[SerializeField] private SceneReference myScene;

private void Start() {
    myScene.Load();
    // Or
    SceneManager.Load(myScene); // Implicit cast to int (build index)
}
```
