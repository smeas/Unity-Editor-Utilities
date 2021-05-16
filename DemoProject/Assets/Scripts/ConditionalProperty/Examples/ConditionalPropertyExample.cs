using UnityEngine;

public class ConditionalPropertyExample : MonoBehaviour {
	[SerializeField]
	private Mode mode;

	[SerializeField, ShowIf(nameof(mode), Mode.Random)]
	private float randomWeight;

	[Space]
	[SerializeField]
	private bool enableAdvancedSettings;

	[SerializeField, EnableIf(nameof(enableAdvancedSettings))]
	private int timeout;

	[SerializeField, EnableIf(nameof(enableAdvancedSettings))]
	private float destroyAfter;

	public enum Mode {
		Default,
		Random,
	}
}

