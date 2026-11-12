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

    [Header("Цвета доступности")]
    public Image backgroundImage;
    public Color affordableColor = new Color(0.15f, 0.17f, 0.21f, 1f);
    public Color unaffordableColor = new Color(0.10f, 0.11f, 0.14f, 0.75f);

    private UpgradeData data;
    private ShopManager shopManager;
    private ClickerManager clickerManager;
    private Coroutine shakeCoroutine;

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

        string amountSuffix = amount > 1 ? $" (+{amount})" : "";
        titleText.text = $"{data.upgradeName} [Ур.{lvl}]{amountSuffix}";

        double autoMult = clickerManager.GetTotalAutoMultiplier();
        switch (data.upgradeType)
        {
            case UpgradeType.FlatClick:
                bonusText.text = $"+{clickerManager.FormatNumber(data.baseValue * amount)} к клику";
                break;
            case UpgradeType.ClickMultiplier:
                double multGain = System.Math.Pow(data.baseValue, amount);
                bonusText.text = $"Сила клика x{clickerManager.FormatNumber(multGain)}";
                break;
            case UpgradeType.AutoIncome:
                double incomeGain = data.baseValue * amount * autoMult;
                bonusText.text = $"+{clickerManager.FormatNumber(incomeGain)} / сек";
                break;
        }

        bool canAfford = clickerManager.score >= cost;
        costText.text = $"Цена: {clickerManager.FormatNumber(cost)}";
        costText.color = canAfford ? new Color(1f, 0.84f, 0f) : new Color(0.95f, 0.35f, 0.35f);

        if (backgroundImage != null)
            backgroundImage.color = canAfford ? affordableColor : unaffordableColor;
    }

    IEnumerator ShakeCard()
    {
        RectTransform rt = GetComponent<RectTransform>();
        float startX = rt.anchoredPosition.x;
        float elapsed = 0f;
        float duration = 0.2f;

        while (elapsed < duration)
        {
            float offset = Mathf.Sin(elapsed * 55f) * 12f * (1f - elapsed / duration);
            rt.anchoredPosition = new Vector2(startX + offset, rt.anchoredPosition.y);
            elapsed += Time.deltaTime;
            yield return null;
        }
        rt.anchoredPosition = new Vector2(startX, rt.anchoredPosition.y);
    }
}