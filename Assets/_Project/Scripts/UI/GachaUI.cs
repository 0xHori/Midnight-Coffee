using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GachaUI : MonoBehaviour
{
    [Header("Менеджеры")]
    public ClickerManager clickerManager;
    public GachaManager gachaManager;

    [Header("Элементы окна Гачи")]
    public GameObject gachaPanel;
    public GameObject dimBackground;
    public TextMeshProUGUI resultText;
    public TextMeshProUGUI crystalsText;
    public TextMeshProUGUI pityText;
    public Image resultImage;

    private Coroutine _gachaAnimRoutine;
    private readonly StringBuilder _multiSummaryBuilder = new StringBuilder(256);

    void OnEnable()
    {
        GameEvents.OnGachaResult += ShowGachaResult;
        GameEvents.OnGachaMultiResult += ShowGachaMultiResult;
    }

    void OnDisable()
    {
        GameEvents.OnGachaResult -= ShowGachaResult;
        GameEvents.OnGachaMultiResult -= ShowGachaMultiResult;
    }

    public void Open()
    {
        StopGachaAnimation();

        if (gachaPanel != null) gachaPanel.SetActive(true);
        if (dimBackground != null) dimBackground.SetActive(true);

        if (resultText != null)
        {
            resultText.gameObject.SetActive(true);
            resultText.text = LocalizationManager.Get("ui.gacha_prompt");
            resultText.color = Color.white;
        }

        if (resultImage != null)
        {
            resultImage.rectTransform.localScale = Vector3.one;
            resultImage.gameObject.SetActive(false);
        }

        RefreshHeaderStats();
    }

    public void Close()
    {
        StopGachaAnimation();
        if (gachaPanel != null) gachaPanel.SetActive(false);
        if (dimBackground != null) dimBackground.SetActive(false);
    }

    public void RefreshHeaderStats()
    {
        if (crystalsText != null && clickerManager != null)
        {
            crystalsText.text = LocalizationManager.Get("ui.gacha_reputation", clickerManager.totalCrystals.ToCompactString());
        }

        if (pityText != null && gachaManager != null)
        {
            int threshold = (gachaManager.affectionManager != null && gachaManager.affectionManager.balanceConfig != null)
                ? gachaManager.affectionManager.balanceConfig.gachaPityThreshold
                : 35;
            int remaining = Mathf.Max(1, threshold - gachaManager.rollsSinceLastLegendary);
            pityText.text = LocalizationManager.Get("ui.gacha_pity", remaining);
        }
    }

    private void StopGachaAnimation()
    {
        if (_gachaAnimRoutine != null)
        {
            StopCoroutine(_gachaAnimRoutine);
            _gachaAnimRoutine = null;
        }
    }

    private void ShowGachaResult(string charName, Sprite sprite, Color color)
    {
        StopGachaAnimation();
        RefreshHeaderStats();

        if (sprite == null || resultImage == null || gachaManager == null || gachaManager.allCharacters.Count == 0)
        {
            if (resultText != null)
            {
                resultText.gameObject.SetActive(true);
                resultText.text = charName;
                resultText.color = color;
            }
            if (resultImage != null)
                resultImage.gameObject.SetActive(false);
            return;
        }

        _gachaAnimRoutine = StartCoroutine(AnimateSingleGachaRoutine(charName, sprite, color));
    }

    private void ShowGachaMultiResult(List<GachaRollEntry> results)
    {
        StopGachaAnimation();
        RefreshHeaderStats();

        if (results == null || results.Count == 0) return;
        _gachaAnimRoutine = StartCoroutine(AnimateMultiGachaRoutine(results));
    }

    private IEnumerator AnimateSingleGachaRoutine(string finalTitle, Sprite finalSprite, Color rarityColor)
    {
        if (resultText != null)
        {
            resultText.gameObject.SetActive(true);
            resultText.text = "...";
            resultText.color = Color.white;
        }

        resultImage.gameObject.SetActive(true);

        // 1. Прокрутка силуэтов (0.55 сек)
        float spinDuration = 0.55f;
        float elapsed = 0f;
        float stepTimer = 0f;
        int idx = 0;

        while (elapsed < spinDuration)
        {
            float dt = Time.unscaledDeltaTime;
            elapsed += dt;
            stepTimer += dt;

            if (stepTimer >= 0.06f)
            {
                stepTimer = 0f;
                var ch = gachaManager.allCharacters[idx % gachaManager.allCharacters.Count];
                idx++;
                if (ch != null && ch.portrait != null)
                {
                    resultImage.sprite = ch.portrait;
                    resultImage.color = new Color(0.08f, 0.08f, 0.1f, 1f);
                }
            }

            float scale = Mathf.Lerp(0.85f, 1.15f, elapsed / spinDuration);
            resultImage.rectTransform.localScale = Vector3.one * scale;
            yield return null;
        }

        // 2. Раскрытие портрета + отскок масштаба (0.2 сек)
        resultImage.sprite = finalSprite;
        resultImage.color = Color.white;

        if (resultText != null)
        {
            resultText.text = finalTitle;
            resultText.color = rarityColor;
        }

        float popDuration = 0.2f;
        elapsed = 0f;
        while (elapsed < popDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / popDuration;
            float punch = Mathf.Lerp(1.25f, 1f, 1f - Mathf.Pow(1f - t, 3f));
            resultImage.rectTransform.localScale = Vector3.one * punch;
            yield return null;
        }

        resultImage.rectTransform.localScale = Vector3.one;
        _gachaAnimRoutine = null;
    }

    private IEnumerator AnimateMultiGachaRoutine(List<GachaRollEntry> results)
    {
        if (resultText != null)
            resultText.gameObject.SetActive(true);

        if (resultImage != null)
            resultImage.gameObject.SetActive(true);

        GachaRollEntry bestEntry = results[0];

        // Каскадное раскрытие всех 10 круток по очереди
        for (int i = 0; i < results.Count; i++)
        {
            GachaRollEntry entry = results[i];
            if (entry.character != null && bestEntry.character != null &&
                entry.character.rarity > bestEntry.character.rarity)
            {
                bestEntry = entry;
            }

            if (resultImage != null && entry.character != null && entry.character.portrait != null)
            {
                resultImage.sprite = entry.character.portrait;
                resultImage.color = Color.white;
            }

            if (resultText != null)
            {
                resultText.text = $"[{i + 1}/{results.Count}] {entry.displayTitle}";
                resultText.color = entry.rarityColor;
            }

            // Короткий импульс масштаба на каждом выпадении (редкие задерживаются чуть дольше)
            float itemDuration = (entry.character != null && entry.character.rarity >= 2) ? 0.28f : 0.16f;
            float elapsed = 0f;
            while (elapsed < itemDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / itemDuration);
                if (resultImage != null)
                {
                    float punch = Mathf.Lerp(1.2f, 1f, 1f - Mathf.Pow(1f - t, 3f));
                    resultImage.rectTransform.localScale = Vector3.one * punch;
                }
                yield return null;
            }
        }

        // Финальный кадр: лучший персонаж из 10 + общая сводка
        if (resultImage != null)
        {
            resultImage.rectTransform.localScale = Vector3.one;
            if (bestEntry.character != null && bestEntry.character.portrait != null)
                resultImage.sprite = bestEntry.character.portrait;
        }

        if (resultText != null)
        {
            _multiSummaryBuilder.Clear();
            for (int i = 0; i < results.Count; i++)
            {
                if (i > 0) _multiSummaryBuilder.Append(", ");
                string hex = ColorUtility.ToHtmlStringRGB(results[i].rarityColor);
                string shortName = results[i].character != null ? results[i].character.LocalizedName : "?";
                int cutIdx = shortName.IndexOf('(');
                if (cutIdx > 1) shortName = shortName.Substring(0, cutIdx - 1);

                _multiSummaryBuilder.Append("<color=#").Append(hex).Append(">")
                    .Append(shortName)
                    .Append(results[i].isNew ? " ★" : "")
                    .Append("</color>");
            }

            resultText.color = Color.white;
            resultText.text = _multiSummaryBuilder.ToString();
        }

        _gachaAnimRoutine = null;
    }
}