using UnityEngine;
using UnityEngine.UI;

public class TeleportToSelection_UI : MonoBehaviour
{
    [SerializeField] PlayerInteractionController player;
    [SerializeField] Button teleportFrontButton;
    
    bool _hiddenUntilSelectionChanges;

    void Awake()
    {
        if (teleportFrontButton != null)
        {
            teleportFrontButton.onClick.AddListener(OnTeleportFrontClicked);
        }
        if (player != null)
        {
            player.OnSelectionChanged += HandleSelectionChanged;
        }
        UpdateVisibility(true);
    }

    void OnDestroy()
    {
        if (teleportFrontButton != null)
        {
            teleportFrontButton.onClick.RemoveListener(OnTeleportFrontClicked);
        }
        if (player != null)
        {
            player.OnSelectionChanged -= HandleSelectionChanged;
        }
    }

    void Update()
    {
        UpdateVisibility(false);
    }

    void UpdateVisibility(bool selectionChanged = false)
    {
        if (teleportFrontButton == null) return;
        bool visible = player != null && player.SelectionHasTeleportAnchor();
        var go = teleportFrontButton.gameObject;
        if (selectionChanged)
        {
            // Si cambió la selección, sincronizamos visibilidad con el estado actual
            _hiddenUntilSelectionChanges = false;
            if (go.activeSelf != visible)
            {
                go.SetActive(visible);
            }
        }
        else
        {
            // Sin cambio de selección: mantener oculto si fue forzado a ocultarse tras teletransporte
            if (_hiddenUntilSelectionChanges && go.activeSelf)
            {
                go.SetActive(false);
            }
            // Solo actualizamos a false si ya no es válido mostrarlo
            if (!visible && go.activeSelf)
            {
                go.SetActive(false);
            }
        }
        teleportFrontButton.interactable = visible;
    }

    void OnTeleportFrontClicked()
    {
        if (player == null) return;
        Debug.Log("Click On Go To selection");
        player.TeleportToSelectionAnchor();
        // Ocultar hasta que el usuario cambie la selección
        if (teleportFrontButton != null)
        {
            teleportFrontButton.gameObject.SetActive(false);
            _hiddenUntilSelectionChanges = true;
        }
    }

    void HandleSelectionChanged(ISelectable _)
    {
        UpdateVisibility(true);
    }
}


