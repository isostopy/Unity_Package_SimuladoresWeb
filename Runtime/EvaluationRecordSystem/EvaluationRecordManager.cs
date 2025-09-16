using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.Events;

public class EvaluationRecordManager : MonoBehaviour
{
    public static EvaluationRecordManager Instance { get; private set; }
    public string url = "https://test.isostopyserver.net/api/auth/me";
    
    [Header("Mensaje de estado del POST")]
    [SerializeField] private TextMeshProUGUI postStatusText;
    [SerializeField] private string sendingMessage = "Enviando...";
    [SerializeField] private string successMessage = "POST enviado con éxito";
    [SerializeField] private string errorMessage = "Error al enviar POST";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // Versi�n est�tica: generar JSON directo
    public static string BuildJson(
        string simulatorId,
        string dateStart,
        string dateEnd,
        string score,
        string info)
    {
        SimulationData data = new SimulationData
        {
            simulatorId = simulatorId,
            dateStart = dateStart,
            dateEnd = dateEnd,
            score = score,
            info = info
        };

        return JsonUtility.ToJson(data);
    }

    // POST
    public void SendPost(string url, string jsonData, UnityAction<bool> onCompleted = null)
    {
        StartCoroutine(PostRequest(url, jsonData, onCompleted));
    }

    private IEnumerator PostRequest(string endpoint, string jsonData, UnityAction<bool> onCompleted = null)
    {
        if (postStatusText != null)
        {
            postStatusText.text = sendingMessage;
        }

        // Cuerpo
        var bodyBytes = Encoding.UTF8.GetBytes(jsonData);

        // Request
        var req = new UnityWebRequest(endpoint, UnityWebRequest.kHttpVerbPOST);
        req.uploadHandler = new UploadHandlerRaw(bodyBytes);
        req.downloadHandler = new DownloadHandlerBuffer();

        req.SetRequestHeader("Content-Type", "application/json");

        // En WebGL, Unity usa Fetch/XHR bajo el capó:
        // - En mismo origen, el navegador adjunta la cookie automáticamente.
        // - Si alguna vez llamas a otro ORIGEN, necesitarás CORS + credenciales en servidor (y no recomendado aquí).

        yield return req.SendWebRequest();

        // Resultado
        if (req.result != UnityWebRequest.Result.Success)
        {
            // Error de transporte o similar
            Debug.Log($"❌ Error: {req.error}\nHTTP {(int)req.responseCode}\n{req.downloadHandler.text}");
            if (postStatusText != null)
            {
                postStatusText.text = $"{errorMessage} (HTTP {(int)req.responseCode})";
            }
            onCompleted?.Invoke(false);
        }
        else
        {
            // Éxito (aunque puede ser 2xx o 4xx/5xx con cuerpo)
            var status = (int)req.responseCode;
            var text = req.downloadHandler.text;
            Debug.Log($"✅ HTTP {status}\n{text}");
            if (postStatusText != null)
            {
                postStatusText.text = successMessage;
            }
            onCompleted?.Invoke(true);
        }
    }

}
