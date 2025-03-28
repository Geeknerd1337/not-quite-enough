using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FadeCanvas : MonoBehaviour
{
    public Image Image;

    public static FadeCanvas Instance;

    void Awake()
    {
        Instance = this;
    }

    private IEnumerator FadeRoutine(Color startColor, Color targetColor, float duration)
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float normalizedTime = elapsedTime / duration;
            Image.color = Color.Lerp(startColor, targetColor, normalizedTime);
            yield return null;
        }

        Image.color = targetColor;
    }

    private IEnumerator FadeRoutineWithCurve(Color startColor, Color targetColor, float duration, AnimationCurve curve)
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float normalizedTime = elapsedTime / duration;
            float curveValue = curve.Evaluate(normalizedTime);
            Image.color = Color.Lerp(startColor, targetColor, curveValue);
            yield return null;
        }

        Image.color = targetColor;
    }

    public static void FadeTo(Color startColor, Color targetColor, float duration)
    {
        if (Instance != null)
        {
            Instance.StartCoroutine(Instance.FadeRoutine(startColor, targetColor, duration));
        }
        else
        {
            Debug.LogWarning("FadeCanvas instance not found!");
        }
    }

    public static void FadeTo(Color startColor, Color targetColor, float duration, AnimationCurve curve)
    {
        if (Instance != null)
        {
            Instance.StartCoroutine(Instance.FadeRoutineWithCurve(startColor, targetColor, duration, curve));
        }
        else
        {
            Debug.LogWarning("FadeCanvas instance not found!");
        }
    }
}
