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

    [Header("Авто-тапы (от Менеджеров)")]
    [Tooltip("Сколько авто-тапов в секунду даёт 1 уровень Менеджера")]
    public float autoTapsPerManagerLevel = 0.1f;
    [Tooltip("Максимум авто-тапов в секунду")]
    public float maxAutoTapsPerSecond = 5f;

    [Header("Статистика")]
    public long totalClicks = 0;
    public long totalCritClicks = 0;
    public double lifetimeScore = 0;
    public int totalPrestiges = 0;

    [Header("UI и Эффекты (для бонусов)")]
    public FloatingTextPool floatingTextPool;

    private GameStatsCalculator _stats;
    public GameStatsCalculator Stats
    {
        get
        {
            if (_stats == null)
                _stats = new GameStatsCalculator(balanceConfig, shopManager, skillTreeManager, gachaManager, feverManager);
            return _stats;
        }
    }
    public double PrestigeMultiplier => Stats.GetPrestigeMultiplier(lifetimeCrystals);

    private const double MAX_SAFE_DOUBLE = 1e300;
    private float _autoTapTimer;
    private float _guaranteedCritTimer;
    public bool IsCritRushActive => _guaranteedCritTimer > 0f;

    void Awake()
    {
        if (balanceConfig != null)
            prestigeThreshold = balanceConfig.initialPrestigeThreshold;

        _stats = new GameStatsCalculator(balanceConfig, shopManager, skillTreeManager, gachaManager, feverManager);

        if (saveManager != null)
            saveManager.LoadGame();

        SanitizeNumbers();
    }

    void Start()
    {
        GameEvents.UpdateUI();

        InvokeRepeating(nameof(AutoIncome), 1f, 1f);
        InvokeRepeating(nameof(AutoSave), 20f, 20f);
    }

    void Update()
    {
        if (_guaranteedCritTimer > 0f)
            _guaranteedCritTimer -= Time.deltaTime;

        ProcessAutoTaps();
    }

    private void ProcessAutoTaps()
    {
        float tapsPerSec = GetAutoTapsPerSecond();
        if (tapsPerSec <= 0f) return;

        _autoTapTimer += Time.deltaTime;
        float interval = 1f / tapsPerSec;

        int tapsThisFrame = 0;
        while (_autoTapTimer >= interval && tapsThisFrame < 5)
        {
            _autoTapTimer -= interval;
            tapsThisFrame++;
            PerformTap(isManual: false);
        }

        if (_autoTapTimer > interval)
            _autoTapTimer = 0f;
    }

    public float GetAutoTapsPerSecond()
    {
        if (shopManager == null || shopManager.allUpgrades == null) return 0f;
        float totalTaps = 0f;
        for (int i = 0; i < shopManager.allUpgrades.Count; i++)
        {
            UpgradeData upg = shopManager.allUpgrades[i];
            if (upg != null && upg.autoTapsPerLevel > 0f)
            {
                totalTaps += shopManager.GetUpgradeLevel(upg.upgradeId) * upg.autoTapsPerLevel;
            }
        }
        return Mathf.Min(maxAutoTapsPerSecond, totalTaps);
    }

    public void SanitizeNumbers()
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
    public double GetTotalCritChance() => IsCritRushActive ? 1.0 : Stats.GetTotalCritChance();
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
        PerformTap(isManual: true);
    }

    private void PerformTap(bool isManual)
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
            if (isManual)
                AudioManager.TriggerHaptic();
        }

        score += earnedPoints;
        lifetimeScore += earnedPoints;
        SanitizeNumbers();

        GameEvents.PlayerTapped(earnedPoints, isCrit, isFeverNow, isManual);
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
        double growth = balanceConfig != null ? balanceConfig.prestigeThresholdGrowth : 3.2;
        prestigeThreshold *= growth;

        if (shopManager != null)
            shopManager.ResetOnPrestige();

        GameEvents.SaveGame();
        GameEvents.UpdateUI();
    }

    public void ActivateCritRush(float duration)
    {
        _guaranteedCritTimer = Mathf.Max(_guaranteedCritTimer, duration);
    }
}