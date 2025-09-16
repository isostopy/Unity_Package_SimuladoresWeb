using System.Runtime.InteropServices;
using UnityEngine;

public class WebRedirect : MonoBehaviour
{
    [SerializeField] private string url;

    [DllImport("__Internal")]
    private static extern void GoToURL(string url);

    public void Redirect()
    {
#if !UNITY_EDITOR && UNITY_WEBGL
        GoToURL(url);
#else
        Application.OpenURL(url); // fallback para editor
#endif
    }
}
