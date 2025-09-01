using System.Collections.Generic;
using System.Linq;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json; // Asegúrate de tener el paquete instalado. Añade "com.unity.nuget.newtonsoft-json": "3.0.2" en manifest.json

public static class EvaluationInfoManager
{
    // Lista de logs generales de la evaluación
    private static readonly List<string> evaluationLogs = new List<string>();

    private static Dictionary<string, Dictionary<string, string>> allMessages;
    private static HashSet<string> failuresStoredIds = new HashSet<string>();
    private static string currentLanguage = "es";
    private static bool isInitialized = false;

    public static bool IsInitialized => isInitialized;

    public static IEnumerator InitializeAsync()
    {
        if (isInitialized) yield break;

        string url = GetStreamingAssetsUrl("evaluation_messages.json");

        using (var req = UnityWebRequest.Get(url))
        {
            yield return req.SendWebRequest();

#if UNITY_2020_2_OR_NEWER
            bool ok = req.result == UnityWebRequest.Result.Success;
#else
            bool ok = !req.isNetworkError && !req.isHttpError;
#endif

            if (!ok)
            {
                Debug.LogError($"No se pudo cargar evaluation_messages.json: {url} | {req.error}");
                yield break;
            }

            try
            {
                string json = req.downloadHandler.text;
                allMessages = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(json);
                isInitialized = true;
            }
            catch (System.Exception ex)
            {
                Debug.LogError("Error al deserializar el JSON: " + ex.Message);
            }
        }
    }

    #region Failure Messages

    private static string GetStreamingAssetsUrl(string filename)
    {
        string path = Path.Combine(Application.streamingAssetsPath, filename);

        // Si ya es una URL (http/https/jar), la devolvemos tal cual
        if (path.StartsWith("http", System.StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("jar:", System.StringComparison.OrdinalIgnoreCase))
            return path;

        // Para rutas locales, UnityWebRequest necesita el prefijo file://
        if (!path.StartsWith("file://"))
            path = "file://" + path;

        return path;
    }

    public static void AddIdToList(string id)
    {
        GuardInitialized();
        failuresStoredIds.Add(id);
    }

    public static string GetFailureMessages()
    {
        GuardInitialized();

        if (!allMessages.ContainsKey(currentLanguage))
            return "[Idioma no soportado]";

        var messages = failuresStoredIds.Select(GetMessage).ToList();
        return string.Join(", ", messages);
    }

    public static string GetMessage(string id)
    {
        GuardInitialized();

        if (allMessages.TryGetValue(currentLanguage, out var dict) &&
            dict.TryGetValue(id, out var msg) &&
            !string.IsNullOrWhiteSpace(msg))
            return msg;

        return $"[Mensaje no encontrado o vacío: {id}]";
    }
    public static void ClearFailures() => failuresStoredIds.Clear();

    #endregion


    #region Evaluation Log

    public static void AddLog(string log)
    {
        GuardInitialized();
        if (!string.IsNullOrWhiteSpace(log))
            evaluationLogs.Add(log.Trim());
    }

    /// <summary>
    /// Devuelve el log completo (logs generales + mensajes de failure) en un único string, separado por comas.
    /// </summary>
    public static string GetFullEvaluationLog()
    {
        GuardInitialized();

        // Empezamos con los logs generales
        var combined = new List<string>(evaluationLogs);

        // Integramos los failures como mensajes legibles
        if (failuresStoredIds.Count > 0)
            combined.AddRange(failuresStoredIds.Select(GetMessage));

        return string.Join(", ", combined);
    }

    public static void ClearLogs()
    {
        evaluationLogs.Clear();
    }

    #endregion

    public static void SetLanguage(string langCode)
    {
        GuardInitialized();
        if (allMessages.ContainsKey(langCode)) currentLanguage = langCode;
        else Debug.LogWarning("Idioma no soportado: " + langCode);
    }

    private static void GuardInitialized()
    {
        if (!isInitialized)
            Debug.LogError("EvaluationInfoManager no está inicializado. Llama antes a StartCoroutine(EvaluationInfoManager.InitializeAsync()).");
    }
}
