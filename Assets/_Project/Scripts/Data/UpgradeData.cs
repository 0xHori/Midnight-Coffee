using System;
using UnityEngine;

public enum UpgradeType
{
    FlatClick,       // Добавляет +X к базовому клику (как "Клик +5")
    ClickMultiplier, // Умножает силу клика в X раз за уровень (как "Улучшение x2")
    AutoIncome       // Добавляет +X/сек к пассивному доходу (как "Автокликер" или "Менеджер")
}

[CreateAssetMenu(fileName = "NewUpgrade", menuName = "Shop/Upgrade")]
public class UpgradeData : ScriptableObject
{
    [Header("Основное")]
    public string upgradeId;         // Уникальный ID (например: "click_mult", "click_flat", "auto_clicker", "manager")
    public string upgradeName;       // Отображаемое название (например: "Автокликер")
    public int orderInShop;          // Порядок сортировки в списке (0, 1, 2, 3...)
    public UpgradeType upgradeType;

    [Header("Баланс и экономика")]
    public int maxLevel = 500;            // Потолок прокачки (защита от переполнения double и int)
    public double baseCost = 10;          // Начальная цена (на 0 уровне)
    public double costGrowthRate = 2.5;   // Во сколько раз растёт цена за каждый уровень
    public double baseValue = 1;          // Прибавка за 1 уровень (5 для +5, 2 для x2, 1 для +1/сек)
    [Tooltip("Сколько авто-тапов в секунду даёт 1 уровень этого улучшения (0 — не даёт авто-тапы)")]
    public float autoTapsPerLevel = 0f;

    // Текущая цена следующего (1-го) уровня
    public double GetSingleCost(int currentLevel)
    {
        if (currentLevel < 0 || currentLevel >= maxLevel) return double.PositiveInfinity;
        double cost = baseCost * Math.Pow(costGrowthRate, currentLevel);
        return double.IsNaN(cost) ? double.PositiveInfinity : cost;
    }

    // Точная стоимость покупки сразу `amount` уровней (геометрическая прогрессия)
    public double GetBulkCost(int currentLevel, int amount)
    {
        if (amount <= 0 || currentLevel < 0 || currentLevel >= maxLevel) return double.PositiveInfinity;
        if (currentLevel + amount > maxLevel) amount = maxLevel - currentLevel;

        double currentCost = GetSingleCost(currentLevel);
        if (double.IsInfinity(currentCost)) return double.PositiveInfinity;

        double total;
        if (Math.Abs(costGrowthRate - 1.0) < 0.0001)
            total = currentCost * amount;
        else
            total = currentCost * (Math.Pow(costGrowthRate, amount) - 1.0) / (costGrowthRate - 1.0);

        return double.IsNaN(total) ? double.PositiveInfinity : total;
    }

    // Сколько максимум уровней игрок может купить прямо сейчас на свой баланс (для режима Max)
    public int GetMaxAffordableAmount(int currentLevel, double currentScore)
    {
        if (currentLevel < 0 || currentLevel >= maxLevel) return 0;
        if (double.IsNaN(currentScore) || currentScore <= 0) return 0;

        int remainingLevels = maxLevel - currentLevel;
        if (double.IsInfinity(currentScore)) return remainingLevels;

        double currentCost = GetSingleCost(currentLevel);
        if (currentScore < currentCost || double.IsInfinity(currentCost)) return 0;

        double rawCount;
        if (Math.Abs(costGrowthRate - 1.0) < 0.0001)
        {
            rawCount = Math.Floor(currentScore / currentCost);
        }
        else
        {
            double numerator = (currentScore * (costGrowthRate - 1.0) / currentCost) + 1.0;
            if (double.IsInfinity(numerator) || double.IsNaN(numerator))
                return remainingLevels;

            rawCount = Math.Floor(Math.Log(numerator, costGrowthRate));
        }

        if (double.IsNaN(rawCount) || double.IsInfinity(rawCount) || rawCount > remainingLevels)
            return remainingLevels;

        return Mathf.Clamp((int)rawCount, 0, remainingLevels);
    }

    public string LocalizedName
    {
        get
        {
            if (string.IsNullOrEmpty(upgradeId)) return upgradeName;
            string key = $"upgrade.{upgradeId}.name";
            string val = LocalizationManager.Get(key);
            return val == $"[{key}]" ? upgradeName : val;
        }
    }
}