using UnityEngine;
using UnityEngine.InputSystem;

public class InputDebugOverlay : MonoBehaviour
{
	[Header("Overlay")]
	public bool visible = true;
	public Key toggleKey = Key.F1;
	public int anchorMargin = 10;
	public int panelWidth = 420;
	public int lineHeight = 20;
	[Range(0.1f, 2.0f)] public float scrollHoldSeconds = 0.8f;
	[Range(0.1f, 2.0f)] public float mouseButtonHoldSeconds = 0.8f;


	private GUIStyle _titleStyle;
	private GUIStyle _labelStyle;
	private GUIStyle _valueStyle;
	private Vector2 _scrollPos;
	private bool _stylesInitialized;

	// Scroll hold state
	private float _lastScrollTime;
	private string _lastScrollDir = "None";

	// Mouse button hold state
	private float _lastLeftDownTime;
	private float _lastLeftUpTime;
	private float _lastRightDownTime;
	private float _lastRightUpTime;

    private void EnsureStylesInitialized()
    {
        if (_stylesInitialized) return;
        _titleStyle = new GUIStyle(GUI.skin.label)
        {
            fontStyle = FontStyle.Bold
        };
        _labelStyle = new GUIStyle(GUI.skin.label);
        _valueStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleRight
        };
        _stylesInitialized = true;
    }

	private void Update()
	{
		if (Keyboard.current != null && Keyboard.current[toggleKey].wasPressedThisFrame)
		{
			visible = !visible;
		}

		// Capture scroll direction and hold for readability
		var mouse = Mouse.current;
		if (mouse != null)
		{
			float sy = mouse.scroll.ReadValue().y;
			if (Mathf.Abs(sy) > 0.01f)
			{
				_lastScrollTime = Time.unscaledTime;
				_lastScrollDir = sy > 0f ? "Up" : "Down";
			}
		}

		// Capture mouse button events and hold
		var svc = InputService.Instance;
		if (svc != null)
		{
			var mi = svc.Mouse;
			if (mi.LeftDown) _lastLeftDownTime = Time.unscaledTime;
			if (mi.LeftUp) _lastLeftUpTime = Time.unscaledTime;
			if (mi.RightDown) _lastRightDownTime = Time.unscaledTime;
			if (mi.RightUp) _lastRightUpTime = Time.unscaledTime;
		}
	}

	private void OnGUI()
	{
		if (!visible) return;

        EnsureStylesInitialized();

		var input = InputService.Instance;
		if (input == null)
		{
			DrawPanelStart("Input Debug");
			GUILayout.Label("InputService not initialized", _labelStyle);
			DrawPanelEnd();
			return;
		}

		DrawPanelStart("Input Debug (toggle: " + toggleKey + ")");

		// Keyboard
		GUILayout.Label("Keyboard", _titleStyle);
		DrawRow("Move", ToString(input.Keyboard.Move));
		DrawRow("AnyKeyPressed", input.Keyboard.AnyKeyPressed.ToString());
		// Show pressed keys list
		string keys = input.Keyboard.PressedKeys.Count > 0 ? string.Join(", ", input.Keyboard.PressedKeys) : "None";
		DrawRow("PressedKeys", keys);
		GUILayout.Space(6);

		// Mouse
		GUILayout.Label("Mouse", _titleStyle);
		DrawRow("Position", ToString(input.Mouse.Position));
		DrawRow("Delta", ToString(input.Mouse.Delta));
		string scrollDir = (Time.unscaledTime - _lastScrollTime) < scrollHoldSeconds ? _lastScrollDir : "None";
		DrawRow("Scroll", scrollDir);
		bool leftPressedNow = input.Mouse.LeftPressed;
		bool rightPressedNow = input.Mouse.RightPressed;
		string leftEvent = leftPressedNow
			? "Down"
			: ((Time.unscaledTime - _lastLeftUpTime) < mouseButtonHoldSeconds
				? "Up"
				: ((Time.unscaledTime - _lastLeftDownTime) < mouseButtonHoldSeconds ? "Down" : "None"));
		string rightEvent = rightPressedNow
			? "Down"
			: ((Time.unscaledTime - _lastRightUpTime) < mouseButtonHoldSeconds
				? "Up"
				: ((Time.unscaledTime - _lastRightDownTime) < mouseButtonHoldSeconds ? "Down" : "None"));
		DrawRow("Left Event", leftEvent);
		DrawRow("Right Event", rightEvent);
		GUILayout.Space(6);

		// Touch
		GUILayout.Label("Touch", _titleStyle);
		DrawRow("Supported", input.Touch.IsTouchSupported.ToString());
		DrawRow("ActiveTouchCount", input.Touch.ActiveTouchCount.ToString());
		DrawRow("IsTouching", input.Touch.IsTouching.ToString());
		DrawRow("PrimaryPosition", ToString(input.Touch.PrimaryPosition));
		DrawRow("PrimaryDelta", ToString(input.Touch.PrimaryDelta));
		DrawRow("IsPinching", input.Touch.IsPinching.ToString());
		DrawRow("PinchDelta", input.Touch.PinchDelta.ToString("0.00"));

		DrawPanelEnd();
	}

	private void DrawPanelStart(string title)
	{
		Rect area = new Rect(anchorMargin, anchorMargin, panelWidth, Screen.height - anchorMargin * 2);
		GUILayout.BeginArea(area, GUI.skin.window);
		GUILayout.Label(title, _titleStyle);
		_scrollPos = GUILayout.BeginScrollView(_scrollPos);
	}

	private void DrawPanelEnd()
	{
		GUILayout.EndScrollView();
		GUILayout.EndArea();
	}

	private void DrawRow(string label, string value)
	{
		GUILayout.BeginHorizontal(GUILayout.Height(lineHeight));
		GUILayout.Label(label, _labelStyle);
		GUILayout.FlexibleSpace();
		GUILayout.Label(value, _valueStyle, GUILayout.Width(panelWidth / 2));
		GUILayout.EndHorizontal();
	}

	private static string ToString(Vector2 v)
	{
		return string.Format("({0,6:0.00}, {1,6:0.00})", v.x, v.y);
	}
}


