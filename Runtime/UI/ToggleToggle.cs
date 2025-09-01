using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Permite activar y desactivar un Toggle de la UI de Unity con una sola funcion. </summary>
public class ToggleToggle : MonoBehaviour
{
    [Space]
    [SerializeField] Toggle toggle = null;

    /// <summary>
    /// Si el toggle esta en true, lo pone a false. Si esta en false, lo pone a true. </summary>
    public void Toggle()
    {
        toggle.isOn = !toggle.isOn;
	}
}
