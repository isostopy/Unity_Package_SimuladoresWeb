using UnityEngine;
using UnityEngine.InputSystem;

public class MouseInput : MonoBehaviour
{
	public Vector2 Position { get; private set; }
	public Vector2 Delta { get; private set; }
	public Vector2 Scroll { get; private set; }

	public bool LeftPressed { get; private set; }
	public bool LeftDown { get; private set; }
	public bool LeftUp { get; private set; }

	public bool RightPressed { get; private set; }
	public bool RightDown { get; private set; }
	public bool RightUp { get; private set; }

	private void Update()
	{
		var mouse = Mouse.current;
		if (mouse == null)
		{
			Position = Vector2.zero;
			Delta = Vector2.zero;
			Scroll = Vector2.zero;
			LeftPressed = LeftDown = LeftUp = false;
			RightPressed = RightDown = RightUp = false;
			return;
		}

		Position = mouse.position.ReadValue();
		Delta = mouse.delta.ReadValue();
		Scroll = mouse.scroll.ReadValue();

		LeftPressed = mouse.leftButton.isPressed;
		LeftDown = mouse.leftButton.wasPressedThisFrame;
		LeftUp = mouse.leftButton.wasReleasedThisFrame;

		RightPressed = mouse.rightButton.isPressed;
		RightDown = mouse.rightButton.wasPressedThisFrame;
		RightUp = mouse.rightButton.wasReleasedThisFrame;
	}
}


