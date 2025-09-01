using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ScreenFader : MonoBehaviour
{
    [Header("Fader Settings")]
    public Image fader;
    public float fadeDuration = 0.5f;

    private Coroutine fadeCoroutine;

    void Awake()
    {
        if (fader == null)
        {
            //Debug.LogError("[ScreenFader] Fader Image is not assigned!");
            return;
        }

        fader.gameObject.SetActive(true);
        fader.enabled = true;


        //Debug.Log($"[ScreenFader] Awake complete. Initial color: {fader.color}");
    }

    public void FadeIn(Action onComplete = null)
    {
        //Debug.Log("[ScreenFader] FadeIn called");
        StartFade(1f, onComplete);
    }

    public void FadeOut(Action onComplete = null)
    {
        //Debug.Log("[ScreenFader] FadeOut called");
        StartFade(0f, onComplete);
    }

    private void StartFade(float targetAlpha, Action onComplete)
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
            //Debug.Log("[ScreenFader] Stopped previous fade coroutine");
        }

        fadeCoroutine = StartCoroutine(FadeRoutine(targetAlpha, onComplete));
    }

    private IEnumerator FadeRoutine(float targetAlpha, Action onComplete)
    {
        //Debug.Log("[ScreenFader] Starting FadeRoutine");

        // Esperar a que Time.deltaTime > 0 (por WebGL)
        while (Time.deltaTime == 0f)
        {
            //Debug.Log("[ScreenFader] Waiting for Time.deltaTime > 0...");
            yield return null;
        }

        EnsureFaderReady();

        Color color = fader.color;
        float startAlpha = targetAlpha == 1 ? 0 : 1;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);
            color.a = Mathf.Lerp(startAlpha, targetAlpha, t);
            fader.color = color;

            //Debug.Log($"[ScreenFader] Alpha: {color.a}  | t: {t}");

            //Debug.Log(elapsed + " < " + fadeDuration + " ---> " + (elapsed < fadeDuration) + " - " + t);
            yield return null;
        }

        color.a = targetAlpha;
        fader.color = color;

        //Debug.Log($"[ScreenFader] Fade complete. Final alpha: {color.a}");

        if (Mathf.Approximately(targetAlpha, 0f))
        {
            fader.enabled = false;
            //Debug.Log("[ScreenFader] Fader disabled after fade out");
        }

        //Debug.Log("-----------------------------------------------------------------------");

        onComplete?.Invoke();
    }

    public void StartSequence(float delay = 1f, Action onComplete = null)
    {
        //Debug.Log("[ScreenFader] Starting sequence...");
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(StartSequenceRoutine(delay, onComplete));
    }

    private IEnumerator StartSequenceRoutine(float delay, Action onComplete)
    {
        SetAlpha(1f);
        //Debug.Log("[ScreenFader] Sequence: alpha set to 1 (black)");

        //while (Time.deltaTime == 0f)
        //{
        //    Debug.Log("[ScreenFader] Waiting for Time.deltaTime > 0 before delay...");
        //    yield return null;
        //}

        yield return new WaitForSeconds(delay);
        //Debug.Log("[ScreenFader] Sequence: delay complete");

        bool done = false;
        FadeOut(() => done = true);
        while (!done)
        {
            yield return null;
        }

        //Debug.Log("[ScreenFader] Sequence complete");
        onComplete?.Invoke();
    }

    public void SetAlpha(float alpha)
    {
        if (fader == null) return;

        EnsureFaderReady();

        Color color = fader.color;
        color.a = alpha;
        fader.color = color;

        fader.enabled = alpha > 0f;

        //Debug.Log($"[ScreenFader] SetAlpha to {alpha}");
    }

    private void EnsureFaderReady()
    {
        fader.gameObject.SetActive(true);
        fader.enabled = true;
        fader.material = null;

        Canvas.ForceUpdateCanvases();
    }
}
