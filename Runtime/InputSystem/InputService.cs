using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;

public sealed class InputService : MonoBehaviour
{
	public static InputService Instance { get; private set; }

	public KeyboardInput Keyboard { get; private set; }
	public MouseInput Mouse { get; private set; }
	public TouchInput Touch { get; private set; }

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	private static void Bootstrap()
	{
		if (Instance != null) return;
		var go = new GameObject("InputService");
		go.AddComponent<InputService>();
	}

	private void Awake()
	{
		if (Instance != null && Instance != this)
		{
			Destroy(gameObject);
			return;
		}

		Instance = this;
		DontDestroyOnLoad(gameObject);

		Keyboard = gameObject.AddComponent<KeyboardInput>();
		Mouse = gameObject.AddComponent<MouseInput>();
		Touch = gameObject.AddComponent<TouchInput>();
	}
}


