using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class ButtonSquish : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    [Header("Настройки пружинки при клике")]
    public float squishAmount = 0.88f;
    public float overshootAmount = 1.06f;
    public float squishSpeed = 0.05f;
    public float maxTiltAngle = 3f;

    [Header("Удержание для авто-клика (Hold-to-Tap)")]
    public bool enableHoldToTap = true;
    [Tooltip("Задержка перед началом авто-кликов при зажатии (сек)")]
    public float holdDelay = 0.25f;
    [Tooltip("Частота авто-кликов при зажатии (тапов в секунду)")]
    public float holdTapsPerSecond = 5f;

    [Header("Дыхание в покое (для персонажа)")]
    public bool enableIdleBreathing = false;
    public float breatheSpeed = 2.2f;
    public float breatheScaleY = 0.025f;
    public float breatheScaleX = 0.012f;
    public float idleSwayAngle = 1.2f;

    private Vector3 originalScale;
    private Quaternion originalRotation;
    private Coroutine animCoroutine;
    private bool isAnimatingTap = false;
    private float idlePhase = 0f;

    private Button _button;
    private bool _isPointerHeld = false;
    private float _holdTimer = 0f;
    private float _holdRepeatTimer = 0f;
    private bool _isFeverActive = false;

    void Awake()
    {
        _button = GetComponent<Button>();
        originalScale = transform.localScale;
        originalRotation = transform.localRotation;
    }

    void OnDisable()
    {
        _isFeverActive = false;
    }

    public void SetFeverActive(bool active)
    {
        _isFeverActive = active;
    }

    void Update()
    {
        ProcessHoldToTap();

        if (!enableIdleBreathing || isAnimatingTap) return;

        float speedMult = _isFeverActive ? 1.85f : 1f;
        float ampMult = _isFeverActive ? 1.4f : 1f;

        idlePhase += Time.deltaTime * breatheSpeed * speedMult;
        float wave = Mathf.Sin(idlePhase);

        float scaleX = originalScale.x * (1f - wave * breatheScaleX * ampMult);
        float scaleY = originalScale.y * (1f + wave * breatheScaleY * ampMult);
        transform.localScale = new Vector3(scaleX, scaleY, originalScale.z);

        if (idleSwayAngle > 0f)
        {
            float sway = Mathf.Sin(idlePhase * 0.5f) * idleSwayAngle * ampMult;
            transform.localRotation = originalRotation * Quaternion.Euler(0f, 0f, sway);
        }
    }

    private void ProcessHoldToTap()
    {
        if (!enableHoldToTap || !_isPointerHeld || _button == null || !_button.interactable) return;

        _holdTimer += Time.deltaTime;
        if (_holdTimer < holdDelay) return;

        _holdRepeatTimer += Time.deltaTime;
        float interval = 1f / Mathf.Max(1f, holdTapsPerSecond);

        if (_holdRepeatTimer >= interval)
        {
            _holdRepeatTimer -= interval;
            _button.onClick?.Invoke();

            if (animCoroutine != null) StopCoroutine(animCoroutine);
            animCoroutine = StartCoroutine(AnimateHoldPulse());
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        _isPointerHeld = true;
        _holdTimer = 0f;
        _holdRepeatTimer = 0f;

        isAnimatingTap = true;
        if (animCoroutine != null) StopCoroutine(animCoroutine);

        float randomTilt = Random.Range(-maxTiltAngle, maxTiltAngle);
        Quaternion targetRot = originalRotation * Quaternion.Euler(0, 0, randomTilt);
        animCoroutine = StartCoroutine(AnimateDown(originalScale * squishAmount, targetRot, squishSpeed));
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        ReleasePointer();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (_isPointerHeld) ReleasePointer();
    }

    private void ReleasePointer()
    {
        _isPointerHeld = false;
        if (animCoroutine != null) StopCoroutine(animCoroutine);
        animCoroutine = StartCoroutine(AnimateBounceBack());
    }

    IEnumerator AnimateHoldPulse()
    {
        Vector3 pulseScale = originalScale * Mathf.Lerp(1f, squishAmount, 0.55f);
        transform.localScale = pulseScale;
        float t = 0f;
        while (t < 0.08f)
        {
            t += Time.deltaTime;
            transform.localScale = Vector3.Lerp(pulseScale, originalScale * squishAmount, t / 0.08f);
            yield return null;
        }
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
        idlePhase = 0f;
        isAnimatingTap = false;
    }
}