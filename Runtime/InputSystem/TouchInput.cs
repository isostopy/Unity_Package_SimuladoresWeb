using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;

public class TouchInput : MonoBehaviour
{
	public bool IsTouchSupported => Touchscreen.current != null;
	public int ActiveTouchCount { get; private set; }
	public bool IsTouching => ActiveTouchCount > 0;
	public Vector2 PrimaryPosition { get; private set; }
	public Vector2 PrimaryDelta { get; private set; }
	public bool IsPinching { get; private set; }
	public float PinchDelta { get; private set; }

	private float _lastPinchDistance;
	private bool _wasPinching;

	private void OnEnable()
	{
		EnhancedTouchSupport.Enable();
#if UNITY_EDITOR
		// Solo simulamos toque con ratón en el Editor para pruebas.
		TouchSimulation.Enable();
#endif
	}

	private void OnDisable()
	{
#if UNITY_EDITOR
		TouchSimulation.Disable();
#endif
		EnhancedTouchSupport.Disable();
	}

	private void Update()
	{
		if (!IsTouchSupported)
		{
			ActiveTouchCount = 0;
			PrimaryPosition = Vector2.zero;
			PrimaryDelta = Vector2.zero;
			IsPinching = false;
			PinchDelta = 0f;
			_lastPinchDistance = 0f;
			_wasPinching = false;
			return;
		}

		var touches = UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches;

		ActiveTouchCount = touches.Count;

		if (ActiveTouchCount > 0)
		{
			var primary = touches[0];
			PrimaryPosition = primary.screenPosition;
			PrimaryDelta = primary.delta;
		}
		else
		{
			PrimaryPosition = Vector2.zero;
			PrimaryDelta = Vector2.zero;
		}

		// Pinch detection using first two touches
		if (ActiveTouchCount >= 2)
		{
			var t1 = touches[0];
			var t2 = touches[1];
			float currentDistance = Vector2.Distance(t1.screenPosition, t2.screenPosition);

			if (_wasPinching)
			{
				PinchDelta = currentDistance - _lastPinchDistance; // >0 fingers apart, <0 pinch in
			}
			else
			{
				PinchDelta = 0f;
			}

			_lastPinchDistance = currentDistance;
			IsPinching = true;
			_wasPinching = true;
		}
		else
		{
			IsPinching = false;
			PinchDelta = 0f;
			_lastPinchDistance = 0f;
			_wasPinching = false;
		}
	}
}


