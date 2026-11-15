using System;
using UnityEngine;

public class ClickerManager : MonoBehaviour
{
    [Header("Конфиг баланса")]
    public BalanceConfig balanceConfig;

    [Header("Менеджеры")]
    public ShopManager shopManager;
    public SkillTreeManager skillTreeManager;
    public GachaManager gachaManager;
    public SaveManager saveManager;
    public FeverManager feverManager;

    [Header("Данные игрока")]
    public double score = 0;
    public double totalCrystals = 0;
    public double lifetimeCrystals = 0;
    public double prestigeThreshold = 50000;

    [Header("Статистика")]
    public long totalClicks = 0;
    public long totalCritClicks = 0;
    public double lifetimeScore = 0;
    public int totalPrestiges = 0;

    [Header("UI и Эффекты (для бонусов)")]
    public FloatingTextPool floatingTextPool;

    public GameStatsCalculator Stats { get; private set; }
    public double PrestigeMultiplier => Stats.GetPrestigeMultiplier(lifetimeCrystals);

    private const double MAX_SAFE_DOUBLE = 1e300;

    void Awake()
    {
        if (balanceConfig != null)
            prestigeThreshold = balanceConfig.initialPrestigeThreshold;

        Stats = new GameStatsCalculator(balanceConfig, shopManager, skillTreeManager, gachaManager, feverManager);
    }

    void Start()
    {
        if (saveManager != null) saveManager.LoadGame();
        SanitizeNumbers();
        GameEvents.UpdateUI();

        InvokeRepeating(nameof(AutoIncome), 1f, 1f);
        InvokeRepeating(nameof(AutoSave), 20f, 20f);
    }

    private void SanitizeNumbers()
    {
        score = SanitizeDouble(score);
        totalCrystals = SanitizeDouble(totalCrystals);
        lifetimeCrystals = SanitizeDouble(Math.Max(lifetimeCrystals, totalCrystals));
        lifetimeScore = SanitizeDouble(lifetimeScore);
    }

    private double SanitizeDouble(double val)
    {
        if (double.IsNaN(val) || double.IsInfinity(val) || val > MAX_SAFE_DOUBLE) return MAX_SAFE_DOUBLE;
        return val < 0 ? 0 : val;
    }

    void AutoSave() => GameEvents.SaveGame();

    public double GetFeverMultiplier() => Stats.GetFeverMultiplier();
    public double GetTotalClickPower() => Stats.GetTotalClickPower(lifetimeCrystals);
    public double GetTotalCritChance() => Stats.GetTotalCritChance();
    public double GetTotalCritMultiplier() => Stats.GetTotalCritMultiplier();
    public double GetTotalAutoIncomePerSecond() => Stats.GetTotalAutoIncomePerSecond(lifetimeCrystals);
    public double GetTotalAutoMultiplier() => Stats.GetTotalAutoMultiplier(lifetimeCrystals);
    public double CalculatePotentialCrystals() => Stats.CalculatePotentialCrystals(score, prestigeThreshold);

    public void AddEarnedScore(double amount)
    {
        if (double.IsNaN(amount) || double.IsInfinity(amount) || amount <= 0) return;

        score += amount;
        lifetimeScore += amount;
        SanitizeNumbers();
        GameEvents.UpdateUI();
    }

    void AutoIncome()
    {
        double income = GetTotalAutoIncomePerSecond();
        if (income > 0) AddEarnedScore(income);
    }

    public void AddScore()
    {
        totalClicks++;

        bool wasFeverActive = feverManager != null && feverManager.isFeverActive;
        if (feverManager != null)
            feverManager.RegisterTap();

        bool isFeverNow = feverManager != null && feverManager.isFeverActive;
        if (!wasFeverActive && isFeverNow)
            GameEvents.FeverTriggered();

        double earnedPoints = GetTotalClickPower();
        bool isCrit = UnityEngine.Random.value <= GetTotalCritChance();

        if (isCrit)
        {
            earnedPoints *= GetTotalCritMultiplier();
            totalCritClicks++;
            AudioManager.TriggerHaptic();
        }

        score += earnedPoints;
        lifetimeScore += earnedPoints;
        SanitizeNumbers();

        GameEvents.PlayerTapped(earnedPoints, isCrit, isFeverNow);
        GameEvents.UpdateUI();
    }

    public void DoPrestige()
    {
        double crystalsToAdd = CalculatePotentialCrystals();
        if (crystalsToAdd <= 0) return;

        totalCrystals += crystalsToAdd;
        lifetimeCrystals += crystalsToAdd;
        totalPrestiges++;
        SanitizeNumbers();

        GameEvents.PrestigeCompleted(crystalsToAdd);

        score = 0;
        double growth = balanceConfig != null ? balanceConfig.prestigeThresholdGrowth : 1.65;
        prestigeThreshold *= growth;

        if (shopManager != null)
            shopManager.ResetOnPrestige();

        GameEvents.SaveGame();
        GameEvents.UpdateUI();
    }
}