using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class KeyboardInput : MonoBehaviour
{
	public Vector2 Move { get; private set; }
	public bool AnyKeyPressed => Keyboard.current != null && Keyboard.current.anyKey.isPressed;
	public bool EscapeDown => Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;

	private readonly List<Key> _pressedKeys = new List<Key>(16);
	public IReadOnlyList<Key> PressedKeys => _pressedKeys;

	private static readonly Key[] _allKeyEnums = (Key[])System.Enum.GetValues(typeof(Key));

	private void Update()
	{
		var keyboard = Keyboard.current;
		if (keyboard == null)
		{
			Move = Vector2.zero;
			return;
		}

		Vector2 direction = Vector2.zero;

		if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) direction.y += 1f;
		if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) direction.y -= 1f;
		if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) direction.x -= 1f;
		if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) direction.x += 1f;

		Move = direction.normalized;

		// Collect currently pressed keys (robust to missing controls)
		_pressedKeys.Clear();
		for (int i = 0; i < _allKeyEnums.Length; i++)
		{
			Key key = _allKeyEnums[i];
			if (key == Key.None) continue;
			var ctrl = keyboard[key];
			if (ctrl != null && ctrl.isPressed)
			{
				_pressedKeys.Add(key);
			}
		}
	}

	public bool IsKeyPressed(Key key)
	{
		var kb = Keyboard.current;
		return kb != null && kb[key].isPressed;
	}

	public bool IsKeyDown(Key key)
	{
		var kb = Keyboard.current;
		return kb != null && kb[key].wasPressedThisFrame;
	}

	public bool IsKeyUp(Key key)
	{
		var kb = Keyboard.current;
		return kb != null && kb[key].wasReleasedThisFrame;
	}
}


