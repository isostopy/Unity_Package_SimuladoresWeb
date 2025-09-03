using UnityEngine;

/// <summary>
/// Ancla opcional de teletransporte asociado a un ISelectable.
/// Si se asigna, el jugador se moverá a este Transform al teletransportarse hacia el selectable.
/// </summary>
public class SelectableTeleportAnchor : MonoBehaviour
{
    [Tooltip("Destino de teletransporte para este objeto seleccionable.")]
    public Transform anchor;

    [Tooltip("Si es true, se aplicará también la rotación del ancla al jugador.")]
    public bool useAnchorRotation = true;
}


