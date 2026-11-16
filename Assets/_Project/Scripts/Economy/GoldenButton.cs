using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class GoldenButton : MonoBehaviour, IPointerDownHandler
{
    public ClickerManager clickerManager;
    public float lifeTime = 7f;
    public float critRushDuration = 8f;

    private bool isClicked = false;
    private float _elapsed = 0f;
    private Image _image;
    private Vector3 _baseScale;

    void Awake()
    {
        _image = GetComponent<Image>();
    }

    void Start()
    {
        _baseScale = transform.localScale == Vector3.zero ? Vector3.one : transform.localScale;
        StartCoroutine(PopInRoutine());
    }

    void Update()
    {
        if (isClicked) return;

        _elapsed += Time.deltaTime;
        float remaining = lifeTime - _elapsed;

        if (remaining <= 0f)
        {
            Destroy(gameObject);
            return;
        }

        // Пульсация для привлечения внимания
        float pulseSpeed = remaining <= 2.2f ? 14f : 4.5f;
        float pulseAmp = remaining <= 2.2f ? 0.14f : 0.08f;
        float scaleFactor = 1f + Mathf.Sin(_elapsed * pulseSpeed) * pulseAmp;
        transform.localScale = _baseScale * scaleFactor;

        // Предупреждающее мигание в последние 2 секунды
        if (_image != null && remaining <= 2.2f)
        {
            Color c = _image.color;
            c.a = Mathf.Lerp(0.35f, 1f, (Mathf.Sin(_elapsed * 18f) + 1f) * 0.5f);
            _image.color = c;
        }
    }

    private IEnumerator PopInRoutine()
    {
        float t = 0f;
        float duration = 0.22f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float p = t / duration;
            float overshoot = Mathf.Sin(p * Mathf.PI * 0.7f) * 1.15f;
            transform.localScale = _baseScale * Mathf.Clamp(overshoot, 0f, 1.15f);
            yield return null;
        }
        transform.localScale = _baseScale;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        OnClick();
    }

    public void OnClick()
    {
        if (isClicked || clickerManager == null) return;
        isClicked = true;

        AudioManager.TriggerHaptic();
        RollSurpriseReward();

        GameEvents.UpdateUI();
        GameEvents.SaveGame();
        Destroy(gameObject);
    }

    private void RollSurpriseReward()
    {
        float roll = Random.value;

        if (roll < 0.45f)
        {
            // 1. Крупный куш очков (45%)
            double reward = clickerManager.Stats.CalculateGoldenButtonReward(clickerManager.lifetimeCrystals);
            clickerManager.AddEarnedScore(reward);
            SpawnFeedback($"ЗОЛОТОЙ КУШ! +{reward.ToCompactString()}", new Color(1f, 0.84f, 0f), 2.0f);
        }
        else if (roll < 0.70f && clickerManager.feverManager != null)
        {
            // 2. Двойной эспрессо: мгновенный Fever Mode (25%)
            clickerManager.feverManager.ForceActivateFever();
            SpawnFeedback("ЭСПРЕССО: МГНОВЕННОЕ ВДОХНОВЕНИЕ!", new Color(1f, 0.55f, 0.1f), 2.1f);
        }
        else if (roll < 0.90f)
        {
            // 3. Час пик: 100% криты на 8 секунд (20%)
            clickerManager.ActivateCritRush(critRushDuration);
            SpawnFeedback($"ЧАС ПИК! 100% ЛАТТЕ-АРТ ({critRushDuration:0}с)!", new Color(1f, 0.25f, 0.35f), 2.1f);
        }
        else
        {
            // 4. Редкий джекпот: +1 или +2 кристалла (10%)
            int crystals = Random.value < 0.25f ? 2 : 1;
            clickerManager.totalCrystals += crystals;
            clickerManager.lifetimeCrystals += crystals;
            SpawnFeedback($"УДАЧА! +{crystals} ★ РЕПУТАЦИИ{(crystals > 1 ? "А" : "")}!", new Color(0.35f, 0.85f, 1f), 2.2f);
        }
    }

    private void SpawnFeedback(string message, Color color, float scale)
    {
        if (clickerManager.floatingTextPool != null)
        {
            clickerManager.floatingTextPool.SpawnText(message, color, transform.position, scale);
        }
    }
}