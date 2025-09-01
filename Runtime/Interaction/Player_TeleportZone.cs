using UnityEngine;

public class Player_TeleportZone : MonoBehaviour, ISelectable
{
    private string _id;

    public void Start()
    {
        if (gameObject.GetComponent<Renderer>())
        {
            gameObject.GetComponent<Renderer>().enabled = false;
        }
    }

    public void SetId(string value)
    {
        _id = value;
    }

    public string GetId()
    {
        return _id;
    }

    #region Selection

    public void SelectionEnter()
    {
        return;
    }

    public void SelectionExit()
    {
        return;
    }

    public void SelectionSelect()
    {
        return;
    }

    public void SelectionDeselect()
    {
        return;
    }

    #endregion
}
