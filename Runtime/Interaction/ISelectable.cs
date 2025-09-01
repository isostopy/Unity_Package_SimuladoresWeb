using UnityEngine;

public interface ISelectable
{
    void SelectionEnter();
    void SelectionExit();
    void SelectionSelect();
    void SelectionDeselect();

    GameObject gameObject { get; }
}
