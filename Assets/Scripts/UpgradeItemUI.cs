using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpgradeItemUI : MonoBehaviour
{
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI bonusText;
    public TextMeshProUGUI costText;
    public Button buyButton;

    private UpgradeData data;
    private ShopManager shopManager;
    private ClickerManager clickerManager;

    public void Setup(UpgradeData upgradeData, ShopManager shop, ClickerManager clicker)
    {
        data = upgradeData;
        shopManager = shop;
        clickerManager = clicker;

        buyButton.onClick.RemoveAllListeners();
        buyButton.onClick.AddListener(() => shopManager.BuyUpgrade(data));

        Refresh();
    }

    public void Refresh()
    {
        if (data == null || shopManager == null || clickerManager == null) return;

        int lvl = shopManager.GetUpgradeLevel(data.upgradeId);
        int amount = shopManager.GetPurchaseAmount(data);
        double cost = data.GetBulkCost(lvl, amount);

        string amountSuffix = amount > 1 ? $" (+{amount})" : "";
        titleText.text = $"{data.upgradeName} [Ур.{lvl}]{amountSuffix}";

        // Считаем актуальную прибавку с учётом текущих множителей
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

        costText.text = $"Цена: {clickerManager.FormatNumber(cost)}";
        buyButton.interactable = (clickerManager.score >= cost);
    }
}