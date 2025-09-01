using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteractionController : MonoBehaviour
{
    [Header("Dependencias")]
    public Raycast raycast;
    [SerializeField] ScreenFader fader;
    [SerializeField] FirstPersonLook firstPersonLook;

    [Header("Cámara")]
    [SerializeField] private Camera myCamera;

    // --- Estado interno ---
    private ISelectable _currentSelection;
    private ISelectable _lastSelection;

    private Coroutine teleportCoroutine;
    private bool _crRunning;

    // Soporte mínimo de TAP táctil (WebGL/móvil)
    [HideInInspector] public bool _wasTouching;
    [HideInInspector] public float _touchStartTime;
    [HideInInspector] public float _touchMoveAccum;
    [HideInInspector] public Vector2 _lastTouchPos;
    [HideInInspector] public const float TapMaxDuration = 0.25f; // s
    [HideInInspector] public const float TapMaxMove = 25f;       // px acumulados

    void Update()
    {
        RayToMouse();
        HandleInputs();
        raycast.CheckSelection();
    }

    #region Modify raycast

    /// <summary>
    /// Hace que el raycaster apunte hacia el ratón en pantalla.
    /// </summary>
    void RayToMouse()
    {
        if (myCamera == null || raycast == null) return;

        Vector3 target = myCamera.ScreenPointToRay(Input.mousePosition).direction;
        raycast.transform.forward = target;
    }

    #endregion

    // Apuntar el ray a una posición de pantalla específica (para tap)
    public void AimRayFromScreenPosition(Vector2 screenPos)
    {
        if (myCamera == null || raycast == null) return;
        Vector3 dir = myCamera.ScreenPointToRay(screenPos).direction;
        raycast.transform.forward = dir;
    }

    public void Select(ISelectable selection)
    {
        if (selection == null) return;

        _currentSelection = selection;

        if (_lastSelection != null) _lastSelection.SelectionDeselect();

        _lastSelection = _currentSelection;
        selection.SelectionSelect();
    }

    public void TeleportPlayer(Transform newTransform)
    {
        if (_crRunning) return;

        if (teleportCoroutine != null)
        {
            StopCoroutine(teleportCoroutine);
        }

        teleportCoroutine = StartCoroutine(TeleportAction(newTransform, true));
    }

    public void TeleportPlayer(Transform newTransform, bool setRotation)
    {
        if (_crRunning) return;

        if (teleportCoroutine != null)
        {
            StopCoroutine(teleportCoroutine);
        }

        teleportCoroutine = StartCoroutine(TeleportAction(newTransform, setRotation));
    }

    IEnumerator TeleportAction(Transform newTransform, bool setRotation)
    {
        // Guardamos la pose objetivo al inicio
        Pose teleportPose = new Pose { position = newTransform.position, rotation = newTransform.rotation };

        _crRunning = true;
        fader.FadeIn();

        yield return new WaitForSeconds(fader.fadeDuration);

        if (setRotation)
        {
            ChangePlayerPose(teleportPose);
        }
        else
        {
            ChangePlayerPosition(newTransform.position);
        }

        yield return new WaitForSeconds(0.5f);

        fader.FadeOut();

        yield return new WaitForSeconds(fader.fadeDuration / 2);

        _crRunning = false;
    }

    public void ChangePlayerPosition(Vector3 position)
    {
        transform.position = position;
    }

    public void ChangePlayerPose(Pose newPose)
    {
        transform.position = newPose.position;
        transform.rotation = newPose.rotation;
        firstPersonLook.CenterLook();
    }

    private void HandleInputs()
    {
        var svc = InputService.Instance;
        if (svc == null) return;

        // Desktop/Web: clic izquierdo
        if (svc.Mouse != null && svc.Mouse.LeftDown)
        {
            bool canTeleport = raycast != null && raycast.teleportGizmo != null && raycast.teleportGizmo.activeSelf;
            if (canTeleport)
            {
                TeleportPlayer(raycast.teleportGizmo.transform, false);
            }
            else
            {
                Select(raycast.CheckSelection());
            }
        }

        // WebGL/Móvil: TAP básico con un dedo
        var touch = svc.Touch;
        if (touch != null && touch.IsTouchSupported)
        {
            if (touch.IsTouching && touch.ActiveTouchCount == 1)
            {
                if (!_wasTouching)
                {
                    _wasTouching = true;
                    _touchStartTime = Time.unscaledTime;
                    _touchMoveAccum = 0f;
                }
                _lastTouchPos = touch.PrimaryPosition;
                _touchMoveAccum += touch.PrimaryDelta.magnitude;
            }
            // Soltó el dedo: evaluar TAP
            if (!touch.IsTouching && _wasTouching)
            {
                _wasTouching = false;
                float duration = Time.unscaledTime - _touchStartTime;
                bool isTap = duration <= TapMaxDuration && _touchMoveAccum <= TapMaxMove && !touch.IsPinching;
                if (isTap)
                {
                    AimRayFromScreenPosition(_lastTouchPos);
                    bool canTeleportTouch = raycast != null && raycast.teleportGizmo != null && raycast.teleportGizmo.activeSelf;
                    if (canTeleportTouch)
                    {
                        TeleportPlayer(raycast.teleportGizmo.transform, false);
                    }
                    else
                    {
                        Select(raycast.CheckSelection());
                    }
                }
            }
        }
    }
}


