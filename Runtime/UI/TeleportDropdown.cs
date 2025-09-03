using TMPro;
using UnityEngine;

public class TeleportDropdown : MonoBehaviour
{

    [SerializeField] TMP_Dropdown dropdown;
    [SerializeField] PlayerInteractionController player;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (player != null && player.useDropdown)
        {
            SetDropdownActive(true);
            FillDropdown();
            HookEvents();
        }
        else
        {
            SetDropdownActive(false);
        }
    }

    void FillDropdown()
    {
        if (dropdown == null) return;

        dropdown.ClearOptions();

        var options = new System.Collections.Generic.List<TMP_Dropdown.OptionData>();
        var optionIds = new System.Collections.Generic.List<string>();

        if (player != null && player.teleportPoints != null)
        {
            for (int i = 0; i < player.teleportPoints.Length; i++)
            {
                var tp = player.teleportPoints[i];
                if (tp == null) continue;
                string label = string.IsNullOrEmpty(tp.id) ? $"Punto {i}" : tp.id;
                options.Add(new TMP_Dropdown.OptionData(label));
                optionIds.Add(tp.id);
            }
        }

        if (options.Count == 0)
        {
            SetDropdownActive(false);
            _optionIds = null;
            return;
        }

        dropdown.AddOptions(options);

        // Guardar mapping en el componente para usarlo en el callback
        _optionIds = optionIds;

        // Seleccionar la primera opción válida
        dropdown.value = 0;
        dropdown.RefreshShownValue();
    }

    void HookEvents()
    {
        if (dropdown == null) return;
        dropdown.onValueChanged.AddListener(OnDropdownChanged);
    }

    void OnDestroy()
    {
        if (dropdown != null)
        {
            dropdown.onValueChanged.RemoveListener(OnDropdownChanged);
        }
    }

    System.Collections.Generic.List<string> _optionIds;

    void OnDropdownChanged(int index)
    {
        if (player == null || _optionIds == null || index < 0 || index >= _optionIds.Count) return;
        var id = _optionIds[index];
        player.TeleportToId(id);
    }

    void SetDropdownActive(bool isActive)
    {
        if (dropdown == null) return;
        var go = dropdown.gameObject;
        if (go != null) go.SetActive(isActive);
        dropdown.interactable = isActive;
    }
}
