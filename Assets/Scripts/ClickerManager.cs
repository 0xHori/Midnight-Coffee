using UnityEngine;
using UnityEngine.InputSystem;

public class ClickerManager : MonoBehaviour
{
    [Header("Менеджеры")]
    public ShopManager shopManager;
    public SkillTreeManager skillTreeManager;
    public GachaManager gachaManager;
    public SaveManager saveManager;

    [Header("Звуки")]
    public AudioSource audioSource;
    public AudioClip clickSound;
    public AudioClip critSound;
    public AudioClip buySound;

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

    [Header("UI и Эффекты")]
    public FloatingTextPool floatingTextPool;
    public TapParticlePool tapParticlePool;

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

    void AutoSave()
    {
        GameEvents.SaveGame();
    }

    public double GetTotalClickPower()
    {
        double shopFlat = shopManager != null ? shopManager.TotalFlatClick : 0;
        double shopMult = shopManager != null ? shopManager.TotalClickMultiplier : 1.0;
        double skillClickMult = skillTreeManager != null ? skillTreeManager.ClickMultiplier : 1.0;
        double charClickMult = gachaManager != null ? (1.0 + gachaManager.GetTotalCharacterBonus(StatType.ClickMultiplier)) : 1.0;

        return (baseClickPower + shopFlat) * shopMult * skillClickMult * charClickMult * PrestigeMultiplier;
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
        return baseAuto * PrestigeMultiplier * skillMult * charMult;
    }

    public double GetTotalAutoMultiplier()
    {
        double skillMult = skillTreeManager != null ? skillTreeManager.AutoMultiplier : 1.0;
        double charMult = gachaManager != null ? (1.0 + gachaManager.GetTotalCharacterBonus(StatType.AutoMultiplier)) : 1.0;
        return PrestigeMultiplier * skillMult * charMult;
    }

    void AutoIncome()
    {
        double income = GetTotalAutoIncomePerSecond();
        if (income > 0)
        {
            score += income;
            lifetimeScore += income;
            SanitizeNumbers();
            GameEvents.UpdateUI();
        }
    }

    public void AddScore()
    {
        totalClicks++;

        double shopFlat = shopManager != null ? shopManager.TotalFlatClick : 0;
        double shopMult = shopManager != null ? shopManager.TotalClickMultiplier : 1.0;
        double skillClickMult = skillTreeManager != null ? skillTreeManager.ClickMultiplier : 1.0;
        double charClickMult = gachaManager != null ? (1.0 + gachaManager.GetTotalCharacterBonus(StatType.ClickMultiplier)) : 1.0;

        double baseClick = (baseClickPower + shopFlat) * shopMult * skillClickMult * charClickMult;
        double earnedPoints = baseClick * PrestigeMultiplier;
        bool isCrit = false;

        double totalCritChance = (skillTreeManager != null ? skillTreeManager.CritChance : critChance)
                               + (gachaManager != null ? gachaManager.GetTotalCharacterBonus(StatType.CritChance) : 0.0);

        if (UnityEngine.Random.value <= totalCritChance)
        {
            double totalCritMult = critMultiplier
                                 + (skillTreeManager != null ? skillTreeManager.CritDamageBonus : 0.0)
                                 + (gachaManager != null ? gachaManager.GetTotalCharacterBonus(StatType.CritDamage) : 0.0);

            earnedPoints *= totalCritMult;
            isCrit = true;
            totalCritClicks++;
            AudioManager.TriggerHaptic();

            if (audioSource != null && critSound != null)
            {
                audioSource.pitch = 1f;
                audioSource.PlayOneShot(critSound);
            }
        }
        else
        {
            if (audioSource != null && clickSound != null)
            {
                audioSource.pitch = UnityEngine.Random.Range(0.9f, 1.1f);
                audioSource.PlayOneShot(clickSound);
            }
        }

        score += earnedPoints;
        lifetimeScore += earnedPoints;
        SanitizeNumbers();

        if (floatingTextPool != null && Pointer.current != null)
        {
            Vector2 pointerPos = Pointer.current.position.ReadValue();
            Vector3 screenPos = new Vector3(pointerPos.x, pointerPos.y, 0);
            if (tapParticlePool != null) tapParticlePool.SpawnBurst(screenPos, isCrit);

            screenPos.y += 20f;
            screenPos.x += UnityEngine.Random.Range(-150f, 150f);
            screenPos.y += UnityEngine.Random.Range(-30f, 40f);
            
            if (isCrit)
            {
                floatingTextPool.SpawnText("КРИТ! +" + FormatNumber(earnedPoints), Color.red, screenPos, 1.5f);
            }
            else
            {
                floatingTextPool.SpawnText("+" + FormatNumber(earnedPoints), Color.white, screenPos, 1f);
            }
        }

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

        if (audioSource != null && buySound != null)
        {
            audioSource.pitch = 0.8f;
            audioSource.PlayOneShot(buySound);
        }

        if (floatingTextPool != null)
        {
            Vector3 centerPos = new Vector3(Screen.width / 2f, Screen.height / 2f, 0);
            floatingTextPool.SpawnText(
                "ПРЕСТИЖ! +" + FormatNumber(crystalsToAdd) + " КРИСТАЛЛОВ",
                new Color(0.8f, 0.2f, 1f),
                centerPos,
                2.5f
            );
        }

        score = 0;
        prestigeThreshold *= 2.5;

        if (shopManager != null)
            shopManager.ResetOnPrestige();

        GameEvents.SaveGame();
        GameEvents.UpdateUI();
    }

    public string FormatNumber(double num)
    {
        if (double.IsNaN(num)) return "0";
        if (double.IsPositiveInfinity(num)) return "MAX";
        if (num < 1000) return num.ToString("0", System.Globalization.CultureInfo.InvariantCulture);

        string[] suffixes = { "", "K", "M", "B", "T", "Qa", "Qi", "Sx", "Sp", "Oc", "No", "Dc" };
        int suffixIndex = 0;
        double originalNum = num;

        while (num >= 1000 && suffixIndex < suffixes.Length - 1)
        {
            num /= 1000;
            suffixIndex++;
        }

        if (num >= 1000 && suffixIndex == suffixes.Length - 1)
        {
            return originalNum.ToString("0.00e0", System.Globalization.CultureInfo.InvariantCulture);
        }

        return num.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture) + suffixes[suffixIndex];
    }

    public void AddEarnedScore(double amount)
    {
        if (double.IsNaN(amount) || double.IsInfinity(amount) || amount <= 0) return;

        score += amount;
        lifetimeScore += amount;
        SanitizeNumbers();
        GameEvents.UpdateUI();
    }
}