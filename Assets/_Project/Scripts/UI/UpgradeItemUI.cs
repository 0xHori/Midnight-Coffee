using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class UpgradeItemUI : MonoBehaviour
{
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI bonusText;
    public TextMeshProUGUI costText;
    public Button buyButton;

    [Tooltip("Внутренний контейнер для анимации тряски, чтобы не конфликтовать с LayoutGroup")]
    public RectTransform visualRoot;

    [Header("Цвета доступности")]
    public Image backgroundImage;
    public Color affordableColor = new Color(0.15f, 0.17f, 0.21f, 1f);
    public Color unaffordableColor = new Color(0.10f, 0.11f, 0.14f, 0.75f);
    public Color milestoneFlashColor = new Color(0.85f, 0.65f, 0.12f, 1f);

    private UpgradeData data;
    private ShopManager shopManager;
    private ClickerManager clickerManager;
    private Coroutine shakeCoroutine;
    private float _visualRootBaseX;
    private bool _hasVisualRootBaseX = false;
    private Coroutine flashCoroutine;
    private int _lastKnownMilestones = -1;

    public void Setup(UpgradeData upgradeData, ShopManager shop, ClickerManager clicker)
    {
        data = upgradeData;
        shopManager = shop;
        clickerManager = clicker;

        if (backgroundImage == null) backgroundImage = GetComponent<Image>();

        buyButton.onClick.RemoveAllListeners();
        buyButton.onClick.AddListener(OnCardClicked);

        Refresh();
    }

    private void OnCardClicked()
    {
        bool bought = shopManager.TryBuyUpgrade(data);
        if (!bought)
        {
            if (shakeCoroutine != null) StopCoroutine(shakeCoroutine);
            shakeCoroutine = StartCoroutine(ShakeCard());
        }
    }

    public void Refresh()
    {
        if (data == null || shopManager == null || clickerManager == null) return;

        int lvl = shopManager.GetUpgradeLevel(data.upgradeId);
        int amount = shopManager.GetPurchaseAmount(data);
        double cost = data.GetBulkCost(lvl, amount);

        int milestones = shopManager.GetMilestoneCount(lvl);
        double currentMilestoneMult = shopManager.GetMilestoneMultiplier(lvl);
        int toNextMilestone = shopManager.GetLevelsToNextMilestone(lvl);

        if (_lastKnownMilestones >= 0 && milestones > _lastKnownMilestones && backgroundImage != null)
        {
            if (flashCoroutine != null) StopCoroutine(flashCoroutine);
            flashCoroutine = StartCoroutine(FlashMilestoneCard());
        }
        _lastKnownMilestones = milestones;

        string amountSuffix = amount > 1 ? LocalizationManager.Get("shop.buy_amount_suffix", amount) : "";
        string multBadge = milestones > 0 ? $" <color=#FFD700>[x{currentMilestoneMult:0.##}]</color>" : "";
        titleText.text = LocalizationManager.Get("shop.item_title", data.LocalizedName, lvl, amountSuffix, multBadge);

        bool willHitMilestone = (lvl + amount) / Mathf.Max(1, shopManager.milestoneStep) > milestones;
        string milestoneHint = willHitMilestone
            ? LocalizationManager.Get("shop.milestone_hit")
            : LocalizationManager.Get("shop.milestone_next", toNextMilestone);

        double autoMult = clickerManager.GetTotalAutoMultiplier();
        double nextMilestoneMult = shopManager.GetMilestoneMultiplier(lvl + amount);

        switch (data.upgradeType)
        {
            case UpgradeType.FlatClick:
                double flatGain = (data.baseValue * (lvl + amount) * nextMilestoneMult) - (data.baseValue * lvl * currentMilestoneMult);
                bonusText.text = LocalizationManager.Get("shop.bonus_click", flatGain.ToCompactString(), milestoneHint);
                break;
            case UpgradeType.ClickMultiplier:
                double prevTotal = Math.Pow(data.baseValue, lvl) * currentMilestoneMult;
                double nextTotal = Math.Pow(data.baseValue, lvl + amount) * nextMilestoneMult;
                double multGain = (double.IsInfinity(nextTotal) || double.IsInfinity(prevTotal) || prevTotal <= 0)
                    ? Math.Pow(data.baseValue, amount)
                    : nextTotal / prevTotal;

                string multDisplay = (double.IsNaN(multGain) || double.IsInfinity(multGain) || multGain > 1e300)
                    ? "MAX"
                    : (multGain < 1000.0 ? multGain.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture) : multGain.ToCompactString());
                bonusText.text = LocalizationManager.Get("shop.bonus_mult", multDisplay, milestoneHint);
                break;
            case UpgradeType.AutoIncome:
                double baseGain = (data.baseValue * (lvl + amount) * nextMilestoneMult) - (data.baseValue * lvl * currentMilestoneMult);
                double incomeGain = baseGain * autoMult;
                string tapInfo = "";
                if (data.autoTapsPerLevel > 0f)
                {
                    float currentTaps = clickerManager.GetAutoTapsPerSecond();
                    float maxTaps = clickerManager.maxAutoTapsPerSecond;
                    float addedTaps = Mathf.Max(0f, Mathf.Min(maxTaps, (lvl + amount) * data.autoTapsPerLevel) - currentTaps);
                    if (addedTaps > 0.001f)
                        tapInfo = LocalizationManager.Get("shop.tap_info", $"{addedTaps:F1}");
                }
                bonusText.text = LocalizationManager.Get("shop.bonus_auto", incomeGain.ToCompactString(), tapInfo, milestoneHint);
                break;
        }
        bool canAfford = clickerManager.score >= cost;
        costText.text = LocalizationManager.Get("shop.cost", cost.ToCompactString());
        costText.color = canAfford ? new Color(1f, 0.84f, 0f) : new Color(0.95f, 0.35f, 0.35f);

        if (double.IsInfinity(cost) || cost >= 1e300)
        {
            costText.text = LocalizationManager.Get("shop.max_reached");
            costText.color = new Color(0.6f, 0.65f, 0.75f);
            if (backgroundImage != null && flashCoroutine == null)
                backgroundImage.color = unaffordableColor;
            return;
        }

        if (backgroundImage != null && flashCoroutine == null)
            backgroundImage.color = canAfford ? affordableColor : unaffordableColor;
    }

    IEnumerator FlashMilestoneCard()
    {
        float elapsed = 0f;
        float duration = 0.55f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            backgroundImage.color = Color.Lerp(milestoneFlashColor, affordableColor, t);
            yield return null;
        }
        flashCoroutine = null;
        Refresh();
    }

    IEnumerator ShakeCard()
    {
        RectTransform target = visualRoot != null ? visualRoot : GetComponent<RectTransform>();
        if (!_hasVisualRootBaseX)
        {
            _visualRootBaseX = target.anchoredPosition.x;
            _hasVisualRootBaseX = true;
        }

        float elapsed = 0f;
        float duration = 0.2f;

        while (elapsed < duration)
        {
            float offset = Mathf.Sin(elapsed * 55f) * 12f * (1f - elapsed / duration);
            target.anchoredPosition = new Vector2(_visualRootBaseX + offset, target.anchoredPosition.y);
            elapsed += Time.deltaTime;
            yield return null;
        }
        target.anchoredPosition = new Vector2(_visualRootBaseX, target.anchoredPosition.y);
    }
}