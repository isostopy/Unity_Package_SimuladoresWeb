using UnityEngine;
using UnityEngine.UI;

public class TeleportToSelection_UI : MonoBehaviour
{
    [SerializeField] PlayerInteractionController player;
    [SerializeField] Button teleportFrontButton;

    void Awake()
    {
        if (teleportFrontButton != null)
        {
            Debug.Log("Listener added");
            teleportFrontButton.onClick.AddListener(OnTeleportFrontClicked);
        }
        UpdateVisibility();
    }

    void OnDestroy()
    {
        if (teleportFrontButton != null)
        {
            teleportFrontButton.onClick.RemoveListener(OnTeleportFrontClicked);
        }
    }

    void Update()
    {
        UpdateVisibility();
    }

    void UpdateVisibility()
    {
        if (teleportFrontButton == null) return;
        bool visible = player != null && player.SelectionHasTeleportAnchor();
        var go = teleportFrontButton.gameObject;
        if (go.activeSelf != visible)
        {
            go.SetActive(visible);
        }
        teleportFrontButton.interactable = visible;
    }

    void OnTeleportFrontClicked()
    {
        if (player == null) return;
        Debug.Log("Click On Go To selection");
        player.TeleportToSelectionAnchor();
    }
}


