using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public ClickerManager clickerManager;
    public GachaManager gachaManager;

    [Header("Главный экран - Тексты")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI incomeText;
    public TextMeshProUGUI offlineText;

    [Header("Престиж UI")]
    public GameObject prestigePanel;
    public GameObject dimBackground;
    public TextMeshProUGUI prestigeInfoText;
    public Button prestigeButtonComponent;

    [Header("Офлайн панель")]
    public GameObject offlinePanel;

    [Header("Дерево Навыков")]
    public GameObject skillTreePanel;

    [Header("Гача и Галерея")]
    public GameObject gachaPanel;
    public TextMeshProUGUI gachaResultText;
    public TextMeshProUGUI gachaCrystalsText;
    public Image gachaResultImage;
    public GameObject galleryPanel;
    public GalleryManager galleryManager;
    public TextMeshProUGUI gachaPityText;

    private Coroutine _gachaAnimRoutine;
    private readonly StringBuilder _multiSummaryBuilder = new StringBuilder(256);

    void OnEnable()
    {
        GameEvents.OnUpdateUI += UpdateUI;
        GameEvents.OnShowOfflinePanel += ShowOfflinePanel;
        GameEvents.OnGachaResult += ShowGachaResult;
        GameEvents.OnGachaMultiResult += ShowGachaMultiResult;
    }

    void OnDisable()
    {
        GameEvents.OnUpdateUI -= UpdateUI;
        GameEvents.OnShowOfflinePanel -= ShowOfflinePanel;
        GameEvents.OnGachaResult -= ShowGachaResult;
        GameEvents.OnGachaMultiResult -= ShowGachaMultiResult;
    }

    public void UpdateUI()
    {
        if (clickerManager == null) return;

        if (scoreText != null)
        {
            string multiplierString = clickerManager.totalCrystals > 0
                ? LocalizationManager.Get("ui.revenue_mult_suffix", clickerManager.PrestigeMultiplier.ToCompactString())
                : "";

            scoreText.text = LocalizationManager.Get("ui.revenue", clickerManager.score.ToCompactString(), multiplierString);
        }

        if (incomeText != null)
        {
            double passiveIncome = clickerManager.GetTotalAutoIncomePerSecond();
            float autoTaps = clickerManager.GetAutoTapsPerSecond();
            double avgCritBonus = 1.0 + clickerManager.GetTotalCritChance() * (clickerManager.GetTotalCritMultiplier() - 1.0);
            double autoTapIncome = autoTaps * clickerManager.GetTotalClickPower() * avgCritBonus;
            double totalShownIncome = passiveIncome + autoTapIncome;

            incomeText.text = LocalizationManager.Get("ui.income_per_sec", totalShownIncome.ToCompactString());
        }

        if (prestigeInfoText != null)
        {
            double potential = clickerManager.CalculatePotentialCrystals();
            prestigeInfoText.text = LocalizationManager.Get(
                "ui.prestige_info",
                potential.ToCompactString(),
                clickerManager.totalCrystals.ToCompactString()
            );

            if (prestigeButtonComponent != null)
                prestigeButtonComponent.interactable = (potential > 0);
        }
    }

    public void ShowOfflinePanel(double earned, int hours, int minutes)
    {
        if (offlinePanel != null)
            offlinePanel.SetActive(true);

        if (offlineText != null)
            offlineText.text = LocalizationManager.Get("ui.offline_earned", hours, minutes, earned.ToCompactString());
    }

    public void OpenPrestigePanel()
    {
        if (prestigePanel != null) prestigePanel.SetActive(true);
        if (dimBackground != null) dimBackground.SetActive(true);
    }

    public void ClosePrestigePanel()
    {
        if (prestigePanel != null) prestigePanel.SetActive(false);
        if (dimBackground != null) dimBackground.SetActive(false);
    }

    public void ConfirmPrestige()
    {
        if (clickerManager != null)
            clickerManager.DoPrestige();
        ClosePrestigePanel();
    }

    public void OpenSkillPanel()
    {
        if (skillTreePanel != null) skillTreePanel.SetActive(true);
        if (dimBackground != null) dimBackground.SetActive(true);
    }

    public void CloseSkillPanel()
    {
        if (skillTreePanel != null) skillTreePanel.SetActive(false);
        if (dimBackground != null) dimBackground.SetActive(false);
    }

    public void OpenGachaPanel()
    {
        StopGachaAnimation();

        if (gachaPanel != null) gachaPanel.SetActive(true);
        if (dimBackground != null) dimBackground.SetActive(true);

        if (gachaResultText != null)
        {
            gachaResultText.gameObject.SetActive(true);
            gachaResultText.text = LocalizationManager.Get("ui.gacha_prompt");
            gachaResultText.color = Color.white;
        }

        if (gachaResultImage != null)
        {
            gachaResultImage.rectTransform.localScale = Vector3.one;
            gachaResultImage.gameObject.SetActive(false);
        }
        UpdateGachaCrystalsUI();
    }

    public void CloseGachaPanel()
    {
        StopGachaAnimation();
        if (gachaPanel != null) gachaPanel.SetActive(false);
        if (dimBackground != null) dimBackground.SetActive(false);
    }

    private void StopGachaAnimation()
    {
        if (_gachaAnimRoutine != null)
        {
            StopCoroutine(_gachaAnimRoutine);
            _gachaAnimRoutine = null;
        }
    }

    private void ShowGachaResult(string name, Sprite sprite, Color color)
    {
        StopGachaAnimation();
        UpdateGachaCrystalsUI();

        if (sprite == null || gachaResultImage == null || gachaManager == null || gachaManager.allCharacters.Count == 0)
        {
            if (gachaResultText != null)
            {
                gachaResultText.gameObject.SetActive(true);
                gachaResultText.text = name;
                gachaResultText.color = color;
            }
            if (gachaResultImage != null)
                gachaResultImage.gameObject.SetActive(false);
            return;
        }

        _gachaAnimRoutine = StartCoroutine(AnimateSingleGachaRoutine(name, sprite, color));
    }

    private void ShowGachaMultiResult(List<GachaRollEntry> results)
    {
        StopGachaAnimation();
        UpdateGachaCrystalsUI();

        if (results == null || results.Count == 0) return;
        _gachaAnimRoutine = StartCoroutine(AnimateMultiGachaRoutine(results));
    }

    private IEnumerator AnimateSingleGachaRoutine(string finalTitle, Sprite finalSprite, Color rarityColor)
    {
        if (gachaResultText != null)
        {
            gachaResultText.gameObject.SetActive(true);
            gachaResultText.text = "...";
            gachaResultText.color = Color.white;
        }

        gachaResultImage.gameObject.SetActive(true);

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
                    gachaResultImage.sprite = ch.portrait;
                    gachaResultImage.color = new Color(0.08f, 0.08f, 0.1f, 1f);
                }
            }

            float scale = Mathf.Lerp(0.85f, 1.15f, elapsed / spinDuration);
            gachaResultImage.rectTransform.localScale = Vector3.one * scale;
            yield return null;
        }

        // 2. Раскрытие портрета + отскок масштаба (0.2 сек)
        gachaResultImage.sprite = finalSprite;
        gachaResultImage.color = Color.white;

        if (gachaResultText != null)
        {
            gachaResultText.text = finalTitle;
            gachaResultText.color = rarityColor;
        }

        float popDuration = 0.2f;
        elapsed = 0f;
        while (elapsed < popDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / popDuration;
            float punch = Mathf.Lerp(1.25f, 1f, 1f - Mathf.Pow(1f - t, 3f));
            gachaResultImage.rectTransform.localScale = Vector3.one * punch;
            yield return null;
        }

        gachaResultImage.rectTransform.localScale = Vector3.one;
        _gachaAnimRoutine = null;
    }

    private IEnumerator AnimateMultiGachaRoutine(List<GachaRollEntry> results)
    {
        if (gachaResultText != null)
            gachaResultText.gameObject.SetActive(true);

        if (gachaResultImage != null)
            gachaResultImage.gameObject.SetActive(true);

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

            if (gachaResultImage != null && entry.character != null && entry.character.portrait != null)
            {
                gachaResultImage.sprite = entry.character.portrait;
                gachaResultImage.color = Color.white;
            }

            if (gachaResultText != null)
            {
                gachaResultText.text = $"[{i + 1}/{results.Count}] {entry.displayTitle}";
                gachaResultText.color = entry.rarityColor;
            }

            // Короткий импульс масштаба на каждом выпадении (редкие задерживаются чуть дольше)
            float itemDuration = (entry.character != null && entry.character.rarity >= 2) ? 0.28f : 0.16f;
            float elapsed = 0f;
            while (elapsed < itemDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / itemDuration);
                if (gachaResultImage != null)
                {
                    float punch = Mathf.Lerp(1.2f, 1f, 1f - Mathf.Pow(1f - t, 3f));
                    gachaResultImage.rectTransform.localScale = Vector3.one * punch;
                }
                yield return null;
            }
        }

        // Финальный кадр: лучший персонаж из 10 + общая сводка
        if (gachaResultImage != null)
        {
            gachaResultImage.rectTransform.localScale = Vector3.one;
            if (bestEntry.character != null && bestEntry.character.portrait != null)
                gachaResultImage.sprite = bestEntry.character.portrait;
        }

        if (gachaResultText != null)
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

            gachaResultText.color = Color.white;
            gachaResultText.text = _multiSummaryBuilder.ToString();
        }

        _gachaAnimRoutine = null;
    }

    public void OpenGalleryPanel()
    {
        if (galleryPanel != null) galleryPanel.SetActive(true);
        if (dimBackground != null) dimBackground.SetActive(true);
        if (galleryManager != null) galleryManager.OpenGallery();
    }

    public void CloseGalleryPanel()
    {
        if (galleryPanel != null) galleryPanel.SetActive(false);
        if (dimBackground != null) dimBackground.SetActive(false);
    }

    public void UpdateGachaCrystalsUI()
    {
        if (gachaCrystalsText != null && clickerManager != null)
        {
            gachaCrystalsText.text = LocalizationManager.Get("ui.gacha_reputation", clickerManager.totalCrystals.ToCompactString());
        }

        if (gachaPityText != null && gachaManager != null)
        {
            int threshold = (gachaManager.affectionManager != null && gachaManager.affectionManager.balanceConfig != null)
                ? gachaManager.affectionManager.balanceConfig.gachaPityThreshold
                : 35;
            int remaining = Mathf.Max(1, threshold - gachaManager.rollsSinceLastLegendary);
            gachaPityText.text = LocalizationManager.Get("ui.gacha_pity", remaining);
        }
    }
}