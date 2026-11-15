using System;
using System.Collections.Generic;
using UnityEngine;

public enum BuyMode
{
    X1,
    X10,
    Max
}

public class ShopManager : MonoBehaviour
{
    [Header("Звуки")]
    public AudioSource audioSource;
    public AudioClip buySound;
    public AudioClip errorSound;

    [Header("Ссылки")]
    public ClickerManager clickerManager;

    [Header("Все товары магазина")]
    public List<UpgradeData> allUpgrades = new List<UpgradeData>();

    public BuyMode CurrentBuyMode { get; private set; } = BuyMode.X1;

    private Dictionary<string, int> upgradeLevels = new Dictionary<string, int>();

    public double TotalFlatClick { get; private set; } = 0;
    public double TotalClickMultiplier { get; private set; } = 1.0;
    public double TotalBaseAutoIncome { get; private set; } = 0;

    void Awake()
    {
        RecalculateStats();
    }

    public int GetUpgradeLevel(string upgradeId)
    {
        return upgradeLevels.TryGetValue(upgradeId, out int lvl) ? lvl : 0;
    }

    public void CycleBuyMode()
    {
        switch (CurrentBuyMode)
        {
            case BuyMode.X1:
                CurrentBuyMode = BuyMode.X10;
                break;
            case BuyMode.X10:
                CurrentBuyMode = BuyMode.Max;
                break;
            case BuyMode.Max:
                CurrentBuyMode = BuyMode.X1;
                break;
        }
        GameEvents.UpdateUI();
    }

    public int GetPurchaseAmount(UpgradeData upgrade)
    {
        int currentLvl = GetUpgradeLevel(upgrade.upgradeId);
        switch (CurrentBuyMode)
        {
            case BuyMode.X1:
                return 1;
            case BuyMode.X10:
                return 10;
            case BuyMode.Max:
                int maxAffordable = upgrade.GetMaxAffordableAmount(currentLvl, clickerManager != null ? clickerManager.score : 0);
                return maxAffordable > 0 ? maxAffordable : 1;
            default:
                return 1;
        }
    }

    public bool TryBuyUpgrade(UpgradeData upgrade)
    {
        if (upgrade == null || clickerManager == null) return false;

        int currentLvl = GetUpgradeLevel(upgrade.upgradeId);
        int amountToBuy = GetPurchaseAmount(upgrade);
        double totalCost = upgrade.GetBulkCost(currentLvl, amountToBuy);

        if (clickerManager.score < totalCost)
        {
            GameEvents.NotEnoughCurrency();

            if (audioSource != null)
            {
                AudioClip clip = errorSound != null ? errorSound : buySound;
                if (clip != null)
                {
                    audioSource.pitch = 0.5f;
                    audioSource.PlayOneShot(clip, 0.6f);
                }
            }
            return false;
        }

        clickerManager.score -= totalCost;
        upgradeLevels[upgrade.upgradeId] = currentLvl + amountToBuy;

        RecalculateStats();

        if (audioSource != null && buySound != null)
        {
            audioSource.pitch = 1f;
            audioSource.PlayOneShot(buySound);
        }

        GameEvents.UpdateUI();
        GameEvents.SaveGame();
        return true;
    }

    public void RecalculateStats()
    {
        double flatClick = 0;
        double clickMult = 1.0;
        double baseAuto = 0;

        foreach (var upgrade in allUpgrades)
        {
            if (upgrade == null) continue;
            int lvl = GetUpgradeLevel(upgrade.upgradeId);
            if (lvl <= 0) continue;

            switch (upgrade.upgradeType)
            {
                case UpgradeType.FlatClick:
                    flatClick += upgrade.baseValue * lvl;
                    break;
                case UpgradeType.ClickMultiplier:
                    clickMult *= Math.Pow(upgrade.baseValue, lvl);
                    break;
                case UpgradeType.AutoIncome:
                    baseAuto += upgrade.baseValue * lvl;
                    break;
            }
        }

        TotalFlatClick = flatClick;
        TotalClickMultiplier = clickMult;
        TotalBaseAutoIncome = baseAuto;
    }

    public void ResetOnPrestige()
    {
        upgradeLevels.Clear();
        RecalculateStats();
    }

    public List<IdLevelPair> ExportSaveData()
    {
        List<IdLevelPair> list = new List<IdLevelPair>();
        foreach (var kvp in upgradeLevels)
        {
            if (kvp.Value > 0)
            {
                list.Add(new IdLevelPair(kvp.Key, kvp.Value));
            }
        }
        return list;
    }

    public void ImportSaveData(List<IdLevelPair> savedList)
    {
        upgradeLevels.Clear();
        if (savedList != null)
        {
            foreach (var pair in savedList)
            {
                if (!string.IsNullOrEmpty(pair.id) && pair.level > 0)
                {
                    upgradeLevels[pair.id] = pair.level;
                }
            }
        }
        RecalculateStats();
    }
}