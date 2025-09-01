using System.Collections.Generic;
using UnityEngine;

public class UI_PanelController : MonoBehaviour
{
	/// <summary> Lista con los paneles controlados por este controller. </summary>
	[SerializeField] List<GameObject> panels = new List<GameObject>();


	private void Start()
	{
		// Por defecto, el primero panel.
		if (panels.Count > 0)
			ShowPanels(panels[0].name);	
	}

	/// <summary> Muestra el panel con el nombre indicado. </summary>
	public virtual void ShowPanels(string selectedPanel)
	{
		foreach (GameObject panel in panels)
		{
			panel.SetActive(selectedPanel == panel.name);
		}
	}

	/// <summary> Oculta todos los paneles. </summary>
	public virtual void HidePanels()
	{
		foreach (GameObject panel in panels)
		{
			panel.SetActive(false);
		}
	}

}
