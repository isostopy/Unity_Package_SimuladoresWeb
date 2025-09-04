using TMPro;
using UnityEngine;

public class TeleportDropdown_UI : MonoBehaviour
{

    [SerializeField] PlayerInteractionController player;
    [SerializeField] TMP_Dropdown dropdown;
    [SerializeField] string headerText = "— Selecciona destino —";

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

        // Insertar cabecera en la primera posición
        string header = string.IsNullOrEmpty(headerText) ? "— Selecciona destino —" : headerText;
        options.Insert(0, new TMP_Dropdown.OptionData(header));
        optionIds.Insert(0, string.Empty);

        dropdown.AddOptions(options);

        // Guardar mapping en el componente para usarlo en el callback
        _optionIds = optionIds;

        // Seleccionar la primera opción válida
        dropdown.value = 0; // cabecera
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
        // Ignorar selección de la cabecera
        if (index == 0) return;
        var id = _optionIds[index];
        player.TeleportToId(id);
        // Volver a seleccionar la cabecera sin notificar para evitar bucles e
        // inducir cambio de valor en la próxima selección (aunque repita opción)
        dropdown.SetValueWithoutNotify(0);
        dropdown.RefreshShownValue();
    }

    void SetDropdownActive(bool isActive)
    {
        if (dropdown == null) return;
        var go = dropdown.gameObject;
        if (go != null) go.SetActive(isActive);
        dropdown.interactable = isActive;
    }
}
