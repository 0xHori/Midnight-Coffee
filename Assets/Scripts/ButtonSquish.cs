using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class ButtonSquish : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Header("Настройки пружинки")]
    public float squishAmount = 0.88f;   // Сжатие при нажатии
    public float overshootAmount = 1.06f; // Пружинящий отскок при отпускании
    public float squishSpeed = 0.05f;
    public float maxTiltAngle = 3f;       // Лёгкий случайный наклон при клике (0 = отключить)

    private Vector3 originalScale;
    private Quaternion originalRotation;
    private Coroutine animCoroutine;

    void Start()
    {
        originalScale = transform.localScale;
        originalRotation = transform.localRotation;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (animCoroutine != null) StopCoroutine(animCoroutine);
        float randomTilt = Random.Range(-maxTiltAngle, maxTiltAngle);
        Quaternion targetRot = originalRotation * Quaternion.Euler(0, 0, randomTilt);
        animCoroutine = StartCoroutine(AnimateDown(originalScale * squishAmount, targetRot, squishSpeed));
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (animCoroutine != null) StopCoroutine(animCoroutine);
        animCoroutine = StartCoroutine(AnimateBounceBack());
    }

    IEnumerator AnimateDown(Vector3 targetScale, Quaternion targetRot, float duration)
    {
        float time = 0f;
        Vector3 startScale = transform.localScale;
        Quaternion startRot = transform.localRotation;

        while (time < duration)
        {
            float t = time / duration;
            transform.localScale = Vector3.Lerp(startScale, targetScale, t);
            transform.localRotation = Quaternion.Slerp(startRot, targetRot, t);
            time += Time.deltaTime;
            yield return null;
        }
        transform.localScale = targetScale;
        transform.localRotation = targetRot;
    }

    IEnumerator AnimateBounceBack()
    {
        // Фаза 1: быстрый отскок чуть больше оригинала (1.06)
        float upDuration = squishSpeed * 1.2f;
        float time = 0f;
        Vector3 startScale = transform.localScale;
        Quaternion startRot = transform.localRotation;
        Vector3 peakScale = originalScale * overshootAmount;

        while (time < upDuration)
        {
            float t = time / upDuration;
            transform.localScale = Vector3.Lerp(startScale, peakScale, t);
            transform.localRotation = Quaternion.Slerp(startRot, originalRotation, t);
            time += Time.deltaTime;
            yield return null;
        }

        // Фаза 2: мягкий возврат в 1.0
        float settleDuration = squishSpeed * 1.5f;
        time = 0f;
        while (time < settleDuration)
        {
            float t = time / settleDuration;
            transform.localScale = Vector3.Lerp(peakScale, originalScale, t);
            time += Time.deltaTime;
            yield return null;
        }

        transform.localScale = originalScale;
        transform.localRotation = originalRotation;
    }
}