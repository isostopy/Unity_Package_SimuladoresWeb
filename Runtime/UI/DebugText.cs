using UnityEngine;
using TMPro;

public class DebugText : MonoBehaviour
{
	static DebugText instance;

	[Space] [SerializeField] TextMeshProUGUI text = null;

	/// <summary> Escribe un mensaje en el texto de la UI. </summary>
	public static void Log(string message)
	{
		if (instance == null)
			instance = FindObjectOfType<DebugText>();
		if (instance == null)
		{
			Debug.LogError("No hay instancia de DebugText en la escena.");
			return;
		}

		instance.text.text += message  + "\n";
		Debug.Log(message);
	}
}
