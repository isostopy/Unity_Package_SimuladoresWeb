using System.Runtime.InteropServices;
using UnityEngine;

public class WebRedirect : MonoBehaviour
{
    [DllImport("__Internal")]
    private static extern void GoToURL(string url);

    public void Redirect(string url)
    {
#if !UNITY_EDITOR && UNITY_WEBGL
        GoToURL(url);
#else
        Application.OpenURL(url); // fallback para editor
#endif
    }
}
