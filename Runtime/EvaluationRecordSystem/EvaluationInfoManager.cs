using System.Collections.Generic;
using System.Linq;
using System.Collections;
using UnityEngine;

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

public class EvaluationInfoManager : MonoBehaviour
{
    [System.Serializable]
    public class LanguageItem
    {
        public string languageCode;
        public TextAsset jsonFile;
    }

    [Header("Idiomas (asignar TextAssets por inspector)")]
    public List<LanguageItem> languages = new List<LanguageItem>();

    // Lista de logs generales de la evaluación
    private readonly List<string> evaluationLogs = new List<string>();

    private Dictionary<string, Dictionary<string, string>> allLanguagesMessages = new Dictionary<string, Dictionary<string, string>>();
    private HashSet<string> failuresStoredIds = new HashSet<string>();
    private string currentLanguage = "es";
    private bool isInitialized = false;

    private static EvaluationInfoManager _instance;
    public static EvaluationInfoManager Instance
    {
        get
        {
            if (_instance == null)
                _instance = FindFirstObjectByType<EvaluationInfoManager>();
            return _instance;
        }
    }

    public static bool IsInitialized => Instance != null && Instance.isInitialized;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
    }

    private void Start()
    {
        if (!isInitialized)
            StartCoroutine(InitializeAsync());
    }

    public static IEnumerator InitializeAsync()
    {
        if (Instance != null && Instance.isInitialized) yield break;

        if (Instance != null && Instance.languages != null && Instance.languages.Count > 0)
        {
            LoadLanguagesFromInspector();
        }
        else
        {
            Debug.LogWarning("EvaluationInfoManager: no hay idiomas asignados en el inspector.");
        }

        if (Instance != null) Instance.isInitialized = true;
    }

    /// <summary>
    /// Detecta y carga todos los idiomas disponibles automáticamente
    /// </summary>
    // Eliminado soporte de carga automática por StreamingAssets o ScriptableObject

    private static void LoadLanguagesFromInspector()
    {
        Instance.allLanguagesMessages.Clear();

        foreach (var item in Instance.languages)
        {
            if (item == null || item.jsonFile == null || string.IsNullOrWhiteSpace(item.languageCode))
                continue;

            try
            {
                var evaluationMessages = JsonUtility.FromJson<EvaluationMessages>(item.jsonFile.text);

                var languageMessages = new Dictionary<string, string>();
                if (evaluationMessages?.messages != null)
                {
                    foreach (var msg in evaluationMessages.messages)
                    {
                        if (!string.IsNullOrEmpty(msg.id) && !string.IsNullOrEmpty(msg.message))
                            languageMessages[msg.id] = msg.message;
                    }

                    if (languageMessages.Count > 0)
                    {
                        Instance.allLanguagesMessages[item.languageCode] = languageMessages;
                        Debug.Log($"Idioma '{item.languageCode}' cargado desde inspector con {languageMessages.Count} mensajes");
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error al deserializar idioma '{item?.languageCode}' desde inspector: " + ex.Message);
            }
        }

        if (Instance.allLanguagesMessages.Count == 0)
        {
            Debug.LogWarning("No se cargaron idiomas desde el inspector.");
        }
    }

    // Eliminado proveedor externo; solo se usa la lista del inspector

    /// <summary>
    /// Carga los mensajes de un idioma específico desde su archivo JSON
    /// </summary>
    // Eliminado: utilidades de StreamingAssets

    #region Failure Messages

    public static void AddIdToList(string id)
    {
        GuardInitialized();
        Instance.failuresStoredIds.Add(id);
    }

    public static string GetFailureMessages()
    {
        GuardInitialized();
        var messages = Instance.failuresStoredIds.Select(GetMessage).ToList();
        return string.Join(", ", messages);
    }

    public static string GetMessage(string id)
    {
        GuardInitialized();

        if (Instance.allLanguagesMessages.TryGetValue(Instance.currentLanguage, out var languageMessages) &&
            languageMessages.TryGetValue(id, out var msg) && 
            !string.IsNullOrWhiteSpace(msg))
            return msg;

        return $"[Mensaje no encontrado: {id}]";
    }

    public static void ClearFailures() => Instance.failuresStoredIds.Clear();

    #endregion

    #region Evaluation Log

    public static void AddLog(string log)
    {
        GuardInitialized();
        if (!string.IsNullOrWhiteSpace(log))
            Instance.evaluationLogs.Add(log.Trim());
    }

    /// <summary>
    /// Devuelve el log completo (logs generales + mensajes de failure) en un único string, separado por comas.
    /// </summary>
    public static string GetFullEvaluationLog()
    {
        GuardInitialized();

        // Empezamos con los logs generales
        var combined = new List<string>(Instance.evaluationLogs);

        // Integramos los failures como mensajes legibles
        if (Instance.failuresStoredIds.Count > 0)
            combined.AddRange(Instance.failuresStoredIds.Select(GetMessage));

        return string.Join(", ", combined);
    }

    public static void ClearLogs()
    {
        Instance.evaluationLogs.Clear();
    }

    #endregion

    #region Language Management

    public static void SetLanguage(string langCode)
    {
        if (Instance.currentLanguage == langCode) return;

        // Verificar si el idioma está disponible
        if (!Instance.allLanguagesMessages.ContainsKey(langCode))
        {
            Debug.LogWarning($"Idioma '{langCode}' no está disponible. Idiomas disponibles: {string.Join(", ", Instance.allLanguagesMessages.Keys)}");
            return;
        }

        Instance.currentLanguage = langCode;
        Debug.Log($"Idioma cambiado a: {langCode}");
    }

    /// <summary>
    /// Obtiene todos los idiomas disponibles
    /// </summary>
    public static string[] GetAvailableLanguages()
    {
        GuardInitialized();
        return Instance.allLanguagesMessages.Keys.ToArray();
    }

    /// <summary>
    /// Verifica si un idioma está disponible
    /// </summary>
    public static bool IsLanguageAvailable(string langCode)
    {
        GuardInitialized();
        return Instance.allLanguagesMessages.ContainsKey(langCode);
    }

    public static string GetCurrentLanguage() => Instance.currentLanguage;

    /// <summary>
    /// Carga un idioma personalizado desde una ruta específica
    /// </summary>
    // Eliminado: carga de idiomas por URL/Resources y API de idiomas personalizados

    #endregion

    private static void GuardInitialized()
    {
        if (Instance == null || !Instance.isInitialized)
            Debug.LogError("EvaluationInfoManager no está inicializado. Agrega el componente a la escena, asigna idiomas y llama a StartCoroutine(EvaluationInfoManager.InitializeAsync()).");
    }
}