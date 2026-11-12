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
    public double baseCost = 10;          // Начальная цена (на 0 уровне)
    public double costGrowthRate = 2.5;   // Во сколько раз растёт цена за каждый уровень
    public double baseValue = 1;          // Прибавка за 1 уровень (5 для +5, 2 для x2, 1 для +1/сек)

    // Текущая цена следующего (1-го) уровня
    public double GetSingleCost(int currentLevel)
    {
        return baseCost * Math.Pow(costGrowthRate, currentLevel);
    }

    // Точная стоимость покупки сразу `amount` уровней (геометрическая прогрессия)
    public double GetBulkCost(int currentLevel, int amount)
    {
        if (amount <= 0) return 0;
        double currentCost = GetSingleCost(currentLevel);

        if (Math.Abs(costGrowthRate - 1.0) < 0.0001)
            return currentCost * amount;

        return currentCost * (Math.Pow(costGrowthRate, amount) - 1.0) / (costGrowthRate - 1.0);
    }

    // Сколько максимум уровней игрок может купить прямо сейчас на свой баланс (для режима Max)
    public int GetMaxAffordableAmount(int currentLevel, double currentScore)
    {
        double currentCost = GetSingleCost(currentLevel);
        if (currentScore < currentCost) return 0;

        if (Math.Abs(costGrowthRate - 1.0) < 0.0001)
            return (int)Math.Floor(currentScore / currentCost);

        double numerator = (currentScore * (costGrowthRate - 1.0) / currentCost) + 1.0;
        int maxCount = (int)Math.Floor(Math.Log(numerator, costGrowthRate));
        return Math.Max(0, maxCount);
    }
}