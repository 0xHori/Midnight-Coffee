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

        int oldMilestones = GetMilestoneCount(currentLvl);
        int newLvl = currentLvl + amountToBuy;
        int newMilestones = GetMilestoneCount(newLvl);
        bool reachedNewMilestone = newMilestones > oldMilestones;

        clickerManager.score -= totalCost;
        upgradeLevels[upgrade.upgradeId] = newLvl;

        RecalculateStats();

        if (audioSource != null && buySound != null)
        {
            audioSource.pitch = reachedNewMilestone ? 1.35f : 1f;
            audioSource.PlayOneShot(buySound);
        }

        if (reachedNewMilestone)
        {
            AudioManager.TriggerHaptic();
            if (clickerManager.floatingTextPool != null)
            {
                Vector3 pos = new Vector3(Screen.width / 2f, Screen.height * 0.45f, 0);
                clickerManager.floatingTextPool.SpawnText(
                    $"ВЕХА УР.{newLvl}! {upgrade.upgradeName} x2!",
                    new Color(1f, 0.84f, 0f),
                    pos,
                    1.6f
                );
            }
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

            double milestoneMult = GetMilestoneMultiplier(lvl);

            switch (upgrade.upgradeType)
            {
                case UpgradeType.FlatClick:
                    flatClick += upgrade.baseValue * lvl * milestoneMult;
                    break;
                case UpgradeType.ClickMultiplier:
                    double effectiveBase = 1.0 + (upgrade.baseValue - 1.0) * milestoneMult;
                    clickMult *= Math.Pow(effectiveBase, lvl);
                    break;
                case UpgradeType.AutoIncome:
                    baseAuto += upgrade.baseValue * lvl * milestoneMult;
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