using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

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
    private Transform _teleportHelper; // Transform auxiliar reutilizable para llamadas a TeleportPlayer

    // Soporte mínimo de TAP táctil (WebGL/móvil)
    [HideInInspector] public bool _wasTouching;
    [HideInInspector] public float _touchStartTime;
    [HideInInspector] public float _touchMoveAccum;
    [HideInInspector] public Vector2 _lastTouchPos;
    [HideInInspector] public const float TapMaxDuration = 0.25f; // s
    [HideInInspector] public const float TapMaxMove = 25f;       // px acumulados

    [Header("Teleport")]
    public bool useDropdown;
    public TeleportPoint[] teleportPoints;

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

    // Apuntar el ray a una posición de pantalla específica (para tap)
    public void AimRayFromScreenPosition(Vector2 screenPos)
    {
        if (myCamera == null || raycast == null) return;
        Vector3 dir = myCamera.ScreenPointToRay(screenPos).direction;
        raycast.transform.forward = dir;
    }

    #endregion

    #region Teleport

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
            ChangePlayerPosition(teleportPose.position);
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

    public void TeleportToSelectionAnchor()
    {
        if (_currentSelection == null) return;

        var go = _currentSelection.gameObject;
        if (go == null) return;

        var anchorComp = go.GetComponent<SelectableTeleportAnchor>();
        if (anchorComp != null && anchorComp.anchor != null)
        {
            if (_teleportHelper == null)
            {
                var helperGo = new GameObject("__TeleportHelper");
                helperGo.hideFlags = HideFlags.HideAndDontSave;
                _teleportHelper = helperGo.transform;
            }

            _teleportHelper.position = anchorComp.anchor.position;
            _teleportHelper.rotation = anchorComp.useAnchorRotation ? anchorComp.anchor.rotation : transform.rotation;
            TeleportPlayer(_teleportHelper, anchorComp.useAnchorRotation);
        }
    }

    public void TeleportToId(string teleportId)
    {
        if (string.IsNullOrEmpty(teleportId) || teleportPoints == null || teleportPoints.Length == 0) return;

        for (int i = 0; i < teleportPoints.Length; i++)
        {
            var tp = teleportPoints[i];
            if (tp == null) continue;
            if (tp.id == teleportId && tp.teleportPoint != null)
            {
                TeleportPlayer(tp.teleportPoint, true);
                return;
            }
        }

        Debug.LogWarning($"TeleportPoint con id '{teleportId}' no encontrado o sin Transform asignado.");
    }

    #endregion

    #region Inputs & Selection


    public bool HasSelection => _currentSelection != null;

    public void Select(ISelectable selection)
    {
        if (selection == null)
        {
            if (_lastSelection != null) _lastSelection.SelectionDeselect();
            _currentSelection = null;
            _lastSelection = null;
            return;
        }

        _currentSelection = selection;

        if (_lastSelection != null) _lastSelection.SelectionDeselect();

        _lastSelection = _currentSelection;
        selection.SelectionSelect();
    }

    // Indica si la selección actual tiene un SelectableTeleportAnchor con anchor válido
    public bool SelectionHasTeleportAnchor()
    {
        if (_currentSelection == null) return false;
        var go = _currentSelection.gameObject;
        if (go == null) return false;
        var anchorComp = go.GetComponent<SelectableTeleportAnchor>();
        return anchorComp != null && anchorComp.anchor != null;
    }

    private void HandleInputs()
    {
        var svc = InputService.Instance;
        if (svc == null) return;

        // Evitar interferir con la UI: si el puntero está sobre un elemento UI, ignorar inputs de interacción
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        // Desktop/Web: clic izquierdo
        if (svc.Mouse != null && svc.Mouse.LeftDown)
        {
            OnClickLogic();
        }

        // WebGL/Móvil: TAP básico con un dedo
        var touch = svc.Touch;
        if (touch != null && touch.IsTouchSupported)
        {
            // Nota: si se necesita, se puede extender con IsPointerOverGameObject(fingerId)
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
                    OnClickLogic();
                }
            }
        }
    }

    void OnClickLogic()
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


    #endregion

}


