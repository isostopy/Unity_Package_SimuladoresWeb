using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;

// Attach to a pivot (for yaw) or directly to the camera.
// If you have separate yaw/pitch transforms (recommended), assign them in the inspector.
public class FirstPersonLook : MonoBehaviour
{
	[Header("Transforms")]
	public Transform yawTransform;   // Horizontal rotation (player body)
	public Transform pitchTransform; // Vertical rotation (camera)

	[Header("Sensitivity")]
	public float mouseSensitivity = 0.12f; // degrees per pixel
	public float touchSensitivity = 0.10f; // degrees per pixel

	[Header("Zoom")]
	public float minFov = 30f;
	public float maxFov = 90f;
	public float mouseZoomSpeed = 5f; // How much FOV change per mouse scroll unit
	public float pinchZoomSpeed = 0.05f; // Scale factor for pinch delta to FOV change
	public bool invertZoom = false;

	[Header("Zoom scaling")]
	public bool scaleSensitivityWithZoom = true; // If true, reduce sensitivity when zooming in
	public Camera fovCamera; // Camera to read FOV from. If null, tries pitchTransform/main camera
	[Min(0f)] public float sensitivityMultiplierAtMinFov = 0.4f; // applied at strongest zoom-in (min FOV)
	[Min(0f)] public float sensitivityMultiplierAtMaxFov = 1.0f; // applied at zoom-out (max FOV)

	[Header("Options")]
	public bool mouseRotateOnRightClickOnly = true;
	public bool mouseDragOpposite = true; // drag style: opposite to cursor movement
	public bool touchDragOpposite = true; // drag style: opposite to touch movement
	public bool invertX = false;
	public bool invertY = false;
	public float minPitch = -80f;
	public float maxPitch = 80f;

	private float _yaw;
	private float _pitch;

	// Touch rotation state
	private bool _rotatingWithTouch;
	private int _activeFingerIndex = -1;
	private bool _ignoreNextTouchDelta;

	private void Awake()
	{
		if (yawTransform == null) yawTransform = transform;
		if (pitchTransform == null) pitchTransform = transform;

		_yaw = NormalizeAngle(yawTransform.localEulerAngles.y);
		_pitch = NormalizeAngle(pitchTransform.localEulerAngles.x);

		// Try to auto-wire FOV sources for sensitivity scaling and zooming
		if (scaleSensitivityWithZoom)
		{
			if (fovCamera == null)
			{
				// Prefer the camera on pitch transform, then main camera
				fovCamera = pitchTransform != null ? pitchTransform.GetComponent<Camera>() : null;
				if (fovCamera == null) fovCamera = Camera.main;
			}
		}
	}

	private void Update()
	{
		if (InputService.Instance == null)
		{
			return;
		}

		var touchService = InputService.Instance.Touch;
		Vector2 lookDelta = Vector2.zero;
		bool usedTouch = false;

		// Handle zoom (mouse wheel and pinch)
		if (fovCamera != null)
		{
			float fov = fovCamera.fieldOfView;
			// Mouse wheel zoom
			var mouseDevice = Mouse.current;
			if (mouseDevice != null)
			{
				float scrollY = mouseDevice.scroll.ReadValue().y; // positive up, negative down
				if (Mathf.Abs(scrollY) > Mathf.Epsilon)
				{
					float delta = scrollY * mouseZoomSpeed;
					fov += invertZoom ? delta : -delta;
				}
			}
			// Touch pinch zoom
			if (touchService.IsPinching && Mathf.Abs(touchService.PinchDelta) > Mathf.Epsilon)
			{
				float delta = touchService.PinchDelta * pinchZoomSpeed;
				// Igualamos dirección con el ratón: delta positivo (separar) => reducir FOV si no invertimos
				fov += invertZoom ? delta : -delta;
			}
			fovCamera.fieldOfView = Mathf.Clamp(fov, minFov, maxFov);
		}

		// Block rotation during pinch
		if (touchService.IsPinching)
		{
			_rotatingWithTouch = false;
			_activeFingerIndex = -1;
			_ignoreNextTouchDelta = true; // When pinch ends and we resume with one finger, avoid jump
		}
		else
		{
			// Solo procesar gestos táctiles si hay pantalla táctil real
			if (touchService.IsTouchSupported)
			{
				// Single-finger touch look
				var touches = UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches;
				if (touches.Count == 1)
				{
					var t = touches[0];
					if (!_rotatingWithTouch)
					{
						_rotatingWithTouch = true;
						_activeFingerIndex = t.finger.index;
						_ignoreNextTouchDelta = true; // begin without jump
					}
					else
					{
						// Ensure we are tracking the current finger; if changed, adopt and skip one frame
						if (_activeFingerIndex != t.finger.index)
						{
							_activeFingerIndex = t.finger.index;
							_ignoreNextTouchDelta = true;
						}
					}

					lookDelta = t.delta;
					if (_ignoreNextTouchDelta)
					{
						lookDelta = Vector2.zero;
						_ignoreNextTouchDelta = false;
					}

					if (touchDragOpposite)
					{
						lookDelta = -lookDelta; // opposite of finger movement
					}

					usedTouch = true;
				}
				else
				{
					_rotatingWithTouch = false;
					_activeFingerIndex = -1;
				}
			}
			else
			{
				// No hay soporte táctil: aseguramos que no nos quedamos en modo touch
				_rotatingWithTouch = false;
				_activeFingerIndex = -1;
			}
		}

		// Mouse look when not using touch
		if (!usedTouch)
		{
			var mouse = Mouse.current;
			if (mouse != null)
			{
				bool canRotate = !mouseRotateOnRightClickOnly || mouse.rightButton.isPressed;
				if (canRotate)
				{
					lookDelta = mouse.delta.ReadValue();
					if (mouseDragOpposite)
					{
						lookDelta = -lookDelta; // opposite of cursor movement
					}
				}
				else
				{
					lookDelta = Vector2.zero;
				}
			}
		}

		// Apply rotation
		float sensitivity = usedTouch ? touchSensitivity : mouseSensitivity;
		if (scaleSensitivityWithZoom)
		{
			sensitivity *= ComputeZoomSensitivityMultiplier();
		}
		float dx = lookDelta.x * sensitivity * (invertX ? -1f : 1f);
		float dy = lookDelta.y * sensitivity * (invertY ? 1f : -1f); // standard FPS: up moves pitch down unless inverted

		_yaw = NormalizeAngle(_yaw + dx);
		_pitch = Mathf.Clamp(NormalizeAngle(_pitch + dy), minPitch, maxPitch);

		// Assign
		if (yawTransform != null)
		{
			yawTransform.localRotation = Quaternion.Euler(0f, _yaw, 0f);
		}
		if (pitchTransform != null)
		{
			pitchTransform.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
		}
	}

	private static float NormalizeAngle(float angle)
	{
		angle %= 360f;
		if (angle > 180f) angle -= 360f;
		return angle;
	}

	private float ComputeZoomSensitivityMultiplier()
	{
		if (!scaleSensitivityWithZoom)
		{
			return 1f;
		}

		// Determine FOV and mapping range
		float fov = 60f;
		if (fovCamera != null)
		{
			fov = fovCamera.fieldOfView;
		}

		float minFov = this.minFov;
		float maxFov = this.maxFov;

		// Ensure proper ordering
		if (maxFov < minFov)
		{
			var tmp = maxFov; maxFov = minFov; minFov = tmp;
		}

		// Normalize FOV to [0..1], 0 => minFov (zoomed in), 1 => maxFov (zoomed out)
		float t = Mathf.InverseLerp(minFov, maxFov, fov);
		// Interpolate multiplier so that at min FOV sensitivity is lower
		float multiplier = Mathf.Lerp(sensitivityMultiplierAtMinFov, sensitivityMultiplierAtMaxFov, t);
		return Mathf.Max(0f, multiplier);
	}

    public void CenterLook()
    {
        yawTransform.rotation = Quaternion.identity;
        pitchTransform.rotation = Quaternion.identity;
        _yaw = 0;
        _pitch = 0;
    }
}


