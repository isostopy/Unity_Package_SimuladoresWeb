using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UI_WS_Button : MonoBehaviour, ISelectable
{
    [Header("Base")]
    public Image img;
    public Color baseColor;
    public Color selectedColor;

    // When true, the button dont change its colors.
    bool locked = false;

    [Header("Options")]
    public bool disableOnClick;
    public bool delayOnClick;
    public float delayTime;

    private Collider _collider_element;

    private bool _isSelected;

    [Header("Actions")]
    public UnityEvent OnHover;
    public UnityEvent OnSelection;

    private bool hover;

    void Start()
    {
        if (img.color != baseColor && img.color != selectedColor)
        {
            img.color = baseColor;
        }
    }

    void Update()
    {
        if (hover)
        {
            OnHover.Invoke();
        }
    }


    // -----------------------------
    #region ISelectable

    public void SelectionEnter()
    {
        hover = true;

        ChangeColor(selectedColor);
    }

    public void SelectionExit()
    {
        hover = false;

        if (_isSelected) { return; }

        ChangeColor(baseColor);
    }


    public void SelectionSelect()
    {
        Debug.Log("ButtonSelected");

        OnSelection.Invoke();
    }


    public void SelectionDeselect()
    {
        _isSelected = false;
        ChangeColor(baseColor);
    }

    #endregion


    // -----------------------------
    #region Button Functions

    /// <summary>
    /// Set the button as selected.
    /// </summary>
    public void EnableSelection()
    {
        _isSelected = true;
        ChangeColor(selectedColor);
    }

    /// <summary>
	/// Set the button as unselected.
	/// </summary>
    public void DisableSelection()
    {
        _isSelected = false;
        ChangeColor(baseColor);
    }

    // Change the color of the background image.
    private void ChangeColor(Color color)
    {
        if (locked == false)
            img.color = color;
    }

    /// <summary>
    /// Lock or unlock the button. Allowing it or not to change the image colors.
    /// </summary>
    public void SetLockedState(bool state)
    {
        locked = state;
        ChangeColor(baseColor);
    }

    #endregion

    public void EnableCollider(bool value)
    {
        if (!_collider_element)
        {
            _collider_element = GetComponent<Collider>();
        }

        if (!_collider_element) { return; }

        _collider_element.enabled = value;
    }

    IEnumerator DelayOnClick()
    {
        EnableCollider(false);
        Debug.Log("Enabled FALSE");

        yield return new WaitForSeconds(delayTime);

        EnableCollider(true);
    }
}
