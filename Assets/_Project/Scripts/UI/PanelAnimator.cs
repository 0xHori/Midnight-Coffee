using UnityEngine;
using System.Collections;

public class PanelAnimator : MonoBehaviour
{
    public float duration = 0.18f;
    public float startScale = 0.85f;

    private Coroutine popCoroutine;

    void OnEnable()
    {
        if (popCoroutine != null) StopCoroutine(popCoroutine);
        popCoroutine = StartCoroutine(PopIn());
    }

    IEnumerator PopIn()
    {
        float time = 0f;
        Vector3 from = Vector3.one * startScale;
        Vector3 to = Vector3.one;

        while (time < duration)
        {
            // Плавная кривая EaseOutBack (лёгкий пружинящий эффект)
            float t = time / duration;
            float c1 = 1.70158f;
            float c3 = c1 + 1f;
            float eased = 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);

            transform.localScale = Vector3.LerpUnclamped(from, to, eased);
            time += Time.unscaledDeltaTime;
            yield return null;
        }

        transform.localScale = to;
    }
}