using UnityEngine;
using UnityEngine.Events;

public class TriggerVolume : MonoBehaviour, ISelectable
{
    public UnityEvent OnSelection;
    public UnityEvent OnDeselect;
    public UnityEvent OnSelectionEnter;
    public UnityEvent OnSelectionExit;

    #region ISelectable

    public void SelectionEnter()
    {
        OnSelectionEnter.Invoke();
    }

    public void SelectionExit()
    {
        OnSelectionExit.Invoke();
    }


    public void SelectionSelect()
    {
        OnSelection.Invoke();
    }

    public void SelectionDeselect()
    {
        OnDeselect.Invoke();
    }

    #endregion
}
