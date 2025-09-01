using UnityEngine;
using UnityEngine.Events;

public class TriggerVolume : MonoBehaviour, ISelectable
{
    public UnityEvent OnSelection;

    #region ISelectable

    public void SelectionEnter()
    {

    }

    public void SelectionExit()
    {

    }


    public void SelectionSelect()
    {
        OnSelection.Invoke();
    }

    public void SelectionDeselect()
    {

    }

    #endregion
}
