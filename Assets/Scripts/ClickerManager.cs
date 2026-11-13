using UnityEngine;

public class ClickerManager : MonoBehaviour
{
    [Header("Менеджеры")]
    public ShopManager shopManager;
    public SkillTreeManager skillTreeManager;
    public GachaManager gachaManager;
    public SaveManager saveManager;
    public FeverManager feverManager;

    [Header("Данные игрока")]
    public double score = 0;
    public double baseClickPower = 1;
    public double critChance = 0.1;
    public double critMultiplier = 10;

    [Header("Статистика")]
    public long totalClicks = 0;
    public long totalCritClicks = 0;
    public double lifetimeScore = 0;
    public int totalPrestiges = 0;

    [Header("Престиж")]
    public double totalCrystals = 0;
    public double prestigeThreshold = 100000;
    public double PrestigeMultiplier => 1 + (totalCrystals * 0.02);

    [Header("UI и Эффекты (для бонусов)")]
    public FloatingTextPool floatingTextPool;

    private const double MAX_SAFE_DOUBLE = 1e300;

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
        if (double.IsNaN(score) || double.IsInfinity(score) || score > MAX_SAFE_DOUBLE)
            score = MAX_SAFE_DOUBLE;
        else if (score < 0)
            score = 0;

        if (double.IsNaN(totalCrystals) || double.IsInfinity(totalCrystals) || totalCrystals > MAX_SAFE_DOUBLE)
            totalCrystals = MAX_SAFE_DOUBLE;
        else if (totalCrystals < 0)
            totalCrystals = 0;

        if (double.IsNaN(lifetimeScore) || double.IsInfinity(lifetimeScore) || lifetimeScore > MAX_SAFE_DOUBLE)
            lifetimeScore = MAX_SAFE_DOUBLE;
        else if (lifetimeScore < 0)
            lifetimeScore = 0;
    }

    void AutoSave() => GameEvents.SaveGame();

    public double GetFeverMultiplier() => feverManager != null ? feverManager.CurrentMultiplier : 1.0;

    public double GetTotalClickPower()
    {
        double shopFlat = shopManager != null ? shopManager.TotalFlatClick : 0;
        double shopMult = shopManager != null ? shopManager.TotalClickMultiplier : 1.0;
        double skillClickMult = skillTreeManager != null ? skillTreeManager.ClickMultiplier : 1.0;
        double charClickMult = gachaManager != null ? (1.0 + gachaManager.GetTotalCharacterBonus(StatType.ClickMultiplier)) : 1.0;

        return (baseClickPower + shopFlat) * shopMult * skillClickMult * charClickMult * PrestigeMultiplier * GetFeverMultiplier();
    }

    public double GetTotalCritChance()
    {
        double chance = (skillTreeManager != null ? skillTreeManager.CritChance : critChance)
                      + (gachaManager != null ? gachaManager.GetTotalCharacterBonus(StatType.CritChance) : 0.0);
        return System.Math.Min(1.0, chance);
    }

    public double GetTotalCritMultiplier()
    {
        return critMultiplier
             + (skillTreeManager != null ? skillTreeManager.CritDamageBonus : 0.0)
             + (gachaManager != null ? gachaManager.GetTotalCharacterBonus(StatType.CritDamage) : 0.0);
    }

    public double GetTotalAutoIncomePerSecond()
    {
        double baseAuto = shopManager != null ? shopManager.TotalBaseAutoIncome : 0;
        if (baseAuto <= 0) return 0;

        double skillMult = skillTreeManager != null ? skillTreeManager.AutoMultiplier : 1.0;
        double charMult = gachaManager != null ? (1.0 + gachaManager.GetTotalCharacterBonus(StatType.AutoMultiplier)) : 1.0;
        return baseAuto * PrestigeMultiplier * skillMult * charMult * GetFeverMultiplier();
    }

    public double GetTotalAutoMultiplier()
    {
        double skillMult = skillTreeManager != null ? skillTreeManager.AutoMultiplier : 1.0;
        double charMult = gachaManager != null ? (1.0 + gachaManager.GetTotalCharacterBonus(StatType.AutoMultiplier)) : 1.0;
        return PrestigeMultiplier * skillMult * charMult * GetFeverMultiplier();
    }

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
        if (income > 0)
        {
            AddEarnedScore(income);
        }
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

    public double CalculatePotentialCrystals()
    {
        if (score < prestigeThreshold) return 0;

        double prestigeMult = (skillTreeManager != null ? skillTreeManager.PrestigeBonusMultiplier : 1.0)
                            + (gachaManager != null ? gachaManager.GetTotalCharacterBonus(StatType.PrestigeBonus) : 0.0);

        double rawCrystals = System.Math.Pow(score / prestigeThreshold, 0.4) * 5.0;
        return System.Math.Floor(rawCrystals * prestigeMult);
    }

    public void DoPrestige()
    {
        double crystalsToAdd = CalculatePotentialCrystals();
        if (crystalsToAdd <= 0) return;

        totalCrystals += crystalsToAdd;
        totalPrestiges++;
        SanitizeNumbers();

        GameEvents.PrestigeCompleted(crystalsToAdd);

        score = 0;
        prestigeThreshold *= 2.5;

        if (shopManager != null)
            shopManager.ResetOnPrestige();

        GameEvents.SaveGame();
        GameEvents.UpdateUI();
    }
}