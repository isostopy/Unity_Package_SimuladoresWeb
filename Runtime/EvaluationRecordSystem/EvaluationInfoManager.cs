using System.Collections.Generic;
using System.Linq;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

// Estructura simple para mensajes de evaluación
[System.Serializable]
public class EvaluationMessages
{
    public MessageEntry[] messages;
}

[System.Serializable]
public class MessageEntry
{
    public string id;
    public string message;
}

public static class EvaluationInfoManager
{
    // Lista de logs generales de la evaluación
    private static readonly List<string> evaluationLogs = new List<string>();

    private static Dictionary<string, Dictionary<string, string>> allLanguagesMessages = new Dictionary<string, Dictionary<string, string>>();
    private static HashSet<string> failuresStoredIds = new HashSet<string>();
    private static string currentLanguage = "es";
    private static bool isInitialized = false;

    public static bool IsInitialized => isInitialized;

    public static IEnumerator InitializeAsync()
    {
        if (isInitialized) yield break;

        // Detectar y cargar todos los idiomas disponibles
        yield return LoadAllAvailableLanguages();
        isInitialized = true;
    }

    /// <summary>
    /// Detecta y carga todos los idiomas disponibles automáticamente
    /// </summary>
    private static IEnumerator LoadAllAvailableLanguages()
    {
        // Lista de idiomas comunes a intentar cargar
        string[] commonLanguages = { "es","en","fr","de","it","pt","ru","zh"};
        
        allLanguagesMessages.Clear();
        
        foreach (string language in commonLanguages)
        {
            yield return LoadLanguageMessages(language);
        }
        
        Debug.Log($"Cargados {allLanguagesMessages.Count} idiomas: {string.Join(", ", allLanguagesMessages.Keys)}");
    }

    /// <summary>
    /// Carga los mensajes de un idioma específico desde su archivo JSON
    /// </summary>
    private static IEnumerator LoadLanguageMessages(string language)
    {
        string filename = $"evaluation_messages_{language}.json";
        string url = GetStreamingAssetsUrl(filename);

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
                // No es error si el archivo no existe, simplemente no cargamos ese idioma
                Debug.Log($"Archivo de idioma no encontrado: {filename}");
                yield break;
            }

            try
            {
                string json = req.downloadHandler.text;
                var evaluationMessages = JsonUtility.FromJson<EvaluationMessages>(json);
                
                var languageMessages = new Dictionary<string, string>();
                
                // Cargar mensajes del idioma
                if (evaluationMessages?.messages != null)
                {
                    foreach (var msg in evaluationMessages.messages)
                    {
                        if (!string.IsNullOrEmpty(msg.id) && !string.IsNullOrEmpty(msg.message))
                        {
                            languageMessages[msg.id] = msg.message;
                        }
                    }
                    
                    // Solo agregar si tiene mensajes válidos
                    if (languageMessages.Count > 0)
                    {
                        allLanguagesMessages[language] = languageMessages;
                        Debug.Log($"Idioma '{language}' cargado con {languageMessages.Count} mensajes");
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error al deserializar {filename}: " + ex.Message);
            }
        }
    }

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

    #region Failure Messages

    public static void AddIdToList(string id)
    {
        GuardInitialized();
        failuresStoredIds.Add(id);
    }

    public static string GetFailureMessages()
    {
        GuardInitialized();
        var messages = failuresStoredIds.Select(GetMessage).ToList();
        return string.Join(", ", messages);
    }

    public static string GetMessage(string id)
    {
        GuardInitialized();

        if (allLanguagesMessages.TryGetValue(currentLanguage, out var languageMessages) &&
            languageMessages.TryGetValue(id, out var msg) && 
            !string.IsNullOrWhiteSpace(msg))
            return msg;

        return $"[Mensaje no encontrado: {id}]";
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

    #region Language Management

    public static void SetLanguage(string langCode)
    {
        if (currentLanguage == langCode) return;

        // Verificar si el idioma está disponible
        if (!allLanguagesMessages.ContainsKey(langCode))
        {
            Debug.LogWarning($"Idioma '{langCode}' no está disponible. Idiomas disponibles: {string.Join(", ", allLanguagesMessages.Keys)}");
            return;
        }

        currentLanguage = langCode;
        Debug.Log($"Idioma cambiado a: {langCode}");
    }

    /// <summary>
    /// Obtiene todos los idiomas disponibles
    /// </summary>
    public static string[] GetAvailableLanguages()
    {
        GuardInitialized();
        return allLanguagesMessages.Keys.ToArray();
    }

    /// <summary>
    /// Verifica si un idioma está disponible
    /// </summary>
    public static bool IsLanguageAvailable(string langCode)
    {
        GuardInitialized();
        return allLanguagesMessages.ContainsKey(langCode);
    }

    public static string GetCurrentLanguage() => currentLanguage;

    /// <summary>
    /// Carga un idioma personalizado desde una ruta específica
    /// </summary>
    public static IEnumerator LoadCustomLanguage(string languageCode, string customPath)
    {
        string url = GetStreamingAssetsUrl(customPath);

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
                Debug.LogError($"No se pudo cargar idioma personalizado desde {customPath}: {req.error}");
                yield break;
            }

            try
            {
                string json = req.downloadHandler.text;
                var evaluationMessages = JsonUtility.FromJson<EvaluationMessages>(json);
                
                var languageMessages = new Dictionary<string, string>();
                
                if (evaluationMessages?.messages != null)
                {
                    foreach (var msg in evaluationMessages.messages)
                    {
                        if (!string.IsNullOrEmpty(msg.id) && !string.IsNullOrEmpty(msg.message))
                        {
                            languageMessages[msg.id] = msg.message;
                        }
                    }
                    
                    if (languageMessages.Count > 0)
                    {
                        allLanguagesMessages[languageCode] = languageMessages;
                        Debug.Log($"Idioma personalizado '{languageCode}' cargado desde {customPath} con {languageMessages.Count} mensajes");
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error al deserializar idioma personalizado desde {customPath}: " + ex.Message);
            }
        }
    }

    /// <summary>
    /// Agrega idiomas personalizados a la lista de idiomas a cargar
    /// </summary>
    public static void AddCustomLanguagesToLoad(string[] customLanguages)
    {
        // Esto se puede usar para extender la lista de idiomas comunes
        // antes de llamar a InitializeAsync()
        Debug.Log($"Idiomas personalizados registrados: {string.Join(", ", customLanguages)}");
    }

    #endregion

    private static void GuardInitialized()
    {
        if (!isInitialized)
            Debug.LogError("EvaluationInfoManager no está inicializado. Llama antes a StartCoroutine(EvaluationInfoManager.InitializeAsync()).");
    }
}