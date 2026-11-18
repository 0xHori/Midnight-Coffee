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
    [Header("Ссылки")]
    public ClickerManager clickerManager;

    [Header("Вехи уровней (Milestones)")]
    [Tooltip("Каждые N уровней эффективность улучшения удваивается (x2)")]
    public int milestoneStep = 25;

    [Header("Все товары магазина")]
    public List<UpgradeData> allUpgrades = new List<UpgradeData>();

    public BuyMode CurrentBuyMode { get; private set; } = BuyMode.X1;

    private Dictionary<string, int> upgradeLevels = new Dictionary<string, int>();

    public double TotalFlatClick { get; private set; } = 0;
    public double TotalClickMultiplier { get; private set; } = 1.0;
    public double TotalBaseAutoIncome { get; private set; } = 0;
    private readonly HashSet<string> _processedIds = new HashSet<string>();

    void Awake()
    {
        RecalculateStats();
    }

    public int GetUpgradeLevel(string upgradeId)
    {
        return upgradeLevels.TryGetValue(upgradeId, out int lvl) ? lvl : 0;
    }

    public int GetMilestoneCount(int level)
    {
        if (milestoneStep <= 0 || level <= 0) return 0;
        return level / milestoneStep;
    }

    public double GetMilestoneMultiplier(int level)
    {
        int milestones = GetMilestoneCount(level);
        return milestones > 0 ? Math.Pow(2.0, milestones) : 1.0;
    }

    public int GetLevelsToNextMilestone(int level)
    {
        if (milestoneStep <= 0) return 0;
        int remainder = level % milestoneStep;
        return milestoneStep - remainder;
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
        if (upgrade == null) return 0;
        int currentLvl = GetUpgradeLevel(upgrade.upgradeId);
        int remaining = Mathf.Max(0, upgrade.maxLevel - currentLvl);
        if (remaining <= 0) return 0;

        switch (CurrentBuyMode)
        {
            case BuyMode.X1:
                return Mathf.Min(1, remaining);
            case BuyMode.X10:
                return Mathf.Min(10, remaining);
            case BuyMode.Max:
                int maxAffordable = upgrade.GetMaxAffordableAmount(currentLvl, clickerManager != null ? clickerManager.score : 0);
                return maxAffordable > 0 ? maxAffordable : 1;
            default:
                return Mathf.Min(1, remaining);
        }
    }

    public bool TryBuyUpgrade(UpgradeData upgrade)
    {
        if (upgrade == null || clickerManager == null) return false;

        int currentLvl = GetUpgradeLevel(upgrade.upgradeId);
        int amountToBuy = GetPurchaseAmount(upgrade);
        if (amountToBuy <= 0) return false;
        double totalCost = upgrade.GetBulkCost(currentLvl, amountToBuy);

        if (clickerManager.score < totalCost)
        {
            GameEvents.NotEnoughCurrency();
            return false;
        }

        int oldMilestones = GetMilestoneCount(currentLvl);
        int newLvl = currentLvl + amountToBuy;
        int newMilestones = GetMilestoneCount(newLvl);
        bool reachedNewMilestone = newMilestones > oldMilestones;

        clickerManager.score -= totalCost;
        upgradeLevels[upgrade.upgradeId] = newLvl;

        RecalculateStats();

        GameEvents.UpgradePurchased(upgrade.LocalizedName, newLvl, reachedNewMilestone);
        GameEvents.UpdateUI();
        GameEvents.SaveGame();
        return true;
    }

    void OnValidate()
    {
        if (allUpgrades == null) return;
        HashSet<string> seen = new HashSet<string>();
        for (int i = 0; i < allUpgrades.Count; i++)
        {
            var upg = allUpgrades[i];
            if (upg == null || string.IsNullOrEmpty(upg.upgradeId)) continue;
            if (!seen.Add(upg.upgradeId))
                Debug.LogWarning($"[ShopManager] Дублирующийся upgradeId в списке: {upg.upgradeId}", this);
        }
    }

    public void RecalculateStats()
    {
        double flatClick = 0;
        double clickMult = 1.0;
        double baseAuto = 0;

        if (allUpgrades != null)
        {
            _processedIds.Clear();

            foreach (var upgrade in allUpgrades)
            {
                if (upgrade == null || string.IsNullOrEmpty(upgrade.upgradeId)) continue;
                if (!_processedIds.Add(upgrade.upgradeId)) continue;

                int lvl = GetUpgradeLevel(upgrade.upgradeId);
                if (lvl <= 0) continue;

                double milestoneMult = GetMilestoneMultiplier(lvl);

                switch (upgrade.upgradeType)
                {
                    case UpgradeType.FlatClick:
                        flatClick += upgrade.baseValue * lvl * milestoneMult;
                        break;
                    case UpgradeType.ClickMultiplier:
                        double rawMult = Math.Pow(upgrade.baseValue, lvl) * milestoneMult;
                        if (double.IsInfinity(rawMult) || rawMult > 1e300)
                            clickMult = 1e300;
                        else
                            clickMult = Math.Min(1e300, clickMult * rawMult);
                        break;
                    case UpgradeType.AutoIncome:
                        baseAuto += upgrade.baseValue * lvl * milestoneMult;
                        break;
                }
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

    public double GetUpgradeGainDelta(UpgradeData upgrade, int amount)
    {
        if (upgrade == null || amount <= 0) return 0;
        int lvl = GetUpgradeLevel(upgrade.upgradeId);
        double currMult = GetMilestoneMultiplier(lvl);
        double nextMult = GetMilestoneMultiplier(lvl + amount);

        switch (upgrade.upgradeType)
        {
            case UpgradeType.FlatClick:
            case UpgradeType.AutoIncome:
                return (upgrade.baseValue * (lvl + amount) * nextMult) - (upgrade.baseValue * lvl * currMult);
            case UpgradeType.ClickMultiplier:
                double prevTotal = Math.Pow(upgrade.baseValue, lvl) * currMult;
                double nextTotal = Math.Pow(upgrade.baseValue, lvl + amount) * nextMult;
                return (double.IsInfinity(nextTotal) || double.IsInfinity(prevTotal) || prevTotal <= 0)
                    ? Math.Pow(upgrade.baseValue, amount)
                    : nextTotal / prevTotal;
            default:
                return 0;
        }
    }
}