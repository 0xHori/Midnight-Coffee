using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class ButtonSquish : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Header("Настройки пружинки при клике")]
    public float squishAmount = 0.88f;    // Сжатие при нажатии
    public float overshootAmount = 1.06f; // Пружинящий отскок при отпускании
    public float squishSpeed = 0.05f;
    public float maxTiltAngle = 3f;       // Лёгкий случайный наклон при клике (0 = отключить)

    [Header("Дыхание в покое (для персонажа)")]
    public bool enableIdleBreathing = false;
    public float breatheSpeed = 2.2f;     // Скорость цикла дыхания
    public float breatheScaleY = 0.025f;  // Растяжение по вертикали (2.5%)
    public float breatheScaleX = 0.012f;  // Лёгкое сжатие по горизонтали для сохранения объёма
    public float idleSwayAngle = 1.2f;    // Мягкое покачивание в градусах

    private Vector3 originalScale;
    private Quaternion originalRotation;
    private Coroutine animCoroutine;
    private bool isAnimatingTap = false;
    private float idlePhase = 0f;

    void Start()
    {
        originalScale = transform.localScale;
        originalRotation = transform.localRotation;
    }

    void Update()
    {
        if (!enableIdleBreathing || isAnimatingTap) return;

        idlePhase += Time.deltaTime * breatheSpeed;
        float wave = Mathf.Sin(idlePhase);

        // Мягкий squash & stretch (вдох-выдох)
        float scaleX = originalScale.x * (1f - wave * breatheScaleX);
        float scaleY = originalScale.y * (1f + wave * breatheScaleY);
        transform.localScale = new Vector3(scaleX, scaleY, originalScale.z);

        // Лёгкое покачивание с половинной частотой
        if (idleSwayAngle > 0f)
        {
            float sway = Mathf.Sin(idlePhase * 0.5f) * idleSwayAngle;
            transform.localRotation = originalRotation * Quaternion.Euler(0f, 0f, sway);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isAnimatingTap = true;
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
        // Фаза 1: быстрый отскок чуть больше оригинала
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
        idlePhase = 0f; // Чтобы вдох начался плавно из нейтрального положения
        isAnimatingTap = false;
    }
}