using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class TapFeedbackController : MonoBehaviour
{
    [Header("Звуки")]
    public AudioSource audioSource;
    public AudioClip clickSound;
    public AudioClip critSound;
    public AudioClip prestigeSound;

    [Header("Пулы эффектов и тряска")]
    public FloatingTextPool floatingTextPool;
    public TapParticlePool tapParticlePool;
    public FeverVisualFeedback feverVisualFeedback;
    public ButtonSquish characterButtonSquish;
    [Tooltip("Объект для микро-тряски при критах (например, стикер персонажа или фон)")]
    public RectTransform shakeTarget;

    [Header("Позиционирование текста (чтобы не перекрывать реплики)")]
    public float textVerticalOffset = -45f;
    public float sideHorizontalOffset = 160f;

    private Vector2 _shakeOriginPos;
    private bool _hasShakeOrigin = false;
    private Coroutine _shakeRoutine;

    void Start()
    {
        if (shakeTarget != null)
        {
            _shakeOriginPos = shakeTarget.anchoredPosition;
            _hasShakeOrigin = true;
        }
    }

    void OnEnable()
    {
        GameEvents.OnPlayerTappedDetailed += HandlePlayerTapped;
        GameEvents.OnFeverTriggered += HandleFeverTriggered;
        GameEvents.OnFeverEnded += HandleFeverEnded;
        GameEvents.OnPrestigeCompleted += HandlePrestigeCompleted;
    }

    void OnDisable()
    {
        GameEvents.OnPlayerTappedDetailed -= HandlePlayerTapped;
        GameEvents.OnFeverTriggered -= HandleFeverTriggered;
        GameEvents.OnFeverEnded -= HandleFeverEnded;
        GameEvents.OnPrestigeCompleted -= HandlePrestigeCompleted;
    }

    private void HandlePlayerTapped(double earnedPoints, bool isCrit, bool isFever, bool isManual)
    {
        if (!isManual) return;

        PlayTapAudio(isCrit, isFever);
        SpawnTapVisuals(earnedPoints, isCrit, isFever);

        if (isCrit)
            TriggerScreenShake(isFever ? 14f : 9f, 0.12f);
    }

    private void HandleFeverTriggered()
    {
        if (feverVisualFeedback != null) feverVisualFeedback.SetFeverActive(true);
        if (characterButtonSquish != null) characterButtonSquish.SetFeverActive(true);

        TriggerScreenShake(18f, 0.25f);

        if (floatingTextPool == null) return;
        Vector3 centerPos = new Vector3(Screen.width / 2f, Screen.height * 0.72f, 0);
        floatingTextPool.SpawnText(LocalizationManager.Get("fever.triggered"), new Color(1f, 0.65f, 0f), centerPos, 2.2f);
    }

    private void HandleFeverEnded()
    {
        if (feverVisualFeedback != null) feverVisualFeedback.SetFeverActive(false);
        if (characterButtonSquish != null) characterButtonSquish.SetFeverActive(false);
    }

    private void TriggerScreenShake(float intensity, float duration)
    {
        if (shakeTarget == null) return;
        if (!_hasShakeOrigin)
        {
            _shakeOriginPos = shakeTarget.anchoredPosition;
            _hasShakeOrigin = true;
        }

        if (_shakeRoutine != null) StopCoroutine(_shakeRoutine);
        _shakeRoutine = StartCoroutine(ShakeRoutine(intensity, duration));
    }

    private IEnumerator ShakeRoutine(float intensity, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float damper = 1f - (elapsed / duration);
            Vector2 offset = Random.insideUnitCircle * (intensity * damper);
            shakeTarget.anchoredPosition = _shakeOriginPos + offset;
            yield return null;
        }
        shakeTarget.anchoredPosition = _shakeOriginPos;
    }

    private void HandlePrestigeCompleted(double crystalsGained)
    {
        if (audioSource != null && prestigeSound != null)
        {
            audioSource.pitch = 0.8f;
            audioSource.PlayOneShot(prestigeSound);
        }

        if (floatingTextPool != null)
        {
            Vector3 centerPos = new Vector3(Screen.width / 2f, Screen.height / 2f, 0);
            floatingTextPool.SpawnText(LocalizationManager.Get("tap.prestige", crystalsGained.ToCompactString()), new Color(0.8f, 0.2f, 1f), centerPos, 2.5f);   
        }
    }

    private void PlayTapAudio(bool isCrit, bool isFever)
    {
        if (audioSource == null) return;

        if (isCrit && critSound != null)
        {
            audioSource.pitch = isFever ? 1.15f : 1f;
            audioSource.PlayOneShot(critSound);
        }
        else if (!isCrit && clickSound != null)
        {
            float basePitch = isFever ? 1.15f : 1.0f;
            audioSource.pitch = Random.Range(basePitch - 0.1f, basePitch + 0.1f);
            audioSource.PlayOneShot(clickSound);
        }
    }

    private void SpawnTapVisuals(double earnedPoints, bool isCrit, bool isFever)
    {
        if (floatingTextPool == null) return;

        Vector2 pointerPos = Pointer.current != null
            ? Pointer.current.position.ReadValue()
            : new Vector2(Screen.width / 2f, Screen.height * 0.52f);

        Vector3 screenPos = new Vector3(pointerPos.x, pointerPos.y, 0);

        if (tapParticlePool != null)
            tapParticlePool.SpawnBurst(screenPos, isCrit || isFever);

        float side = Random.value < 0.5f ? -1f : 1f;
        screenPos.x = (Screen.width / 2f) + side * Random.Range(sideHorizontalOffset * 0.7f, sideHorizontalOffset * 1.35f);
        screenPos.y += textVerticalOffset + Random.Range(-35f, 25f);

        if (isCrit)
        {
            floatingTextPool.SpawnText(LocalizationManager.Get("tap.crit", earnedPoints.ToCompactString()), Color.red, screenPos, 1.5f);
        }
        else
        {
            Color textColor = isFever ? new Color(1f, 0.85f, 0.2f) : Color.white;
            floatingTextPool.SpawnText($"+{earnedPoints.ToCompactString()}", textColor, screenPos, isFever ? 1.15f : 1f);
        }
    }
}