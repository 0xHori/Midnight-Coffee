using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class ClickerManager : MonoBehaviour
{
    [Header("Менеджеры")]
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
    public double clickPower = 1;
    public double flatClickPower = 0;
    public double critChance = 0.1;
    public double critMultiplier = 10;
    public double autoIncome = 0;

    [Header("Цены улучшений")]
    public double upgradeCost = 10;
    public double flatUpgradeCost = 100;
    public double autoUpgradeCost = 50;
    public double managerCost = 500;

    [Header("Престиж")]
    public double totalCrystals = 0;
    public double prestigeThreshold = 10000;
    public double PrestigeMultiplier => 1 + (totalCrystals * 0.1);

    [Header("UI и Эффекты")]
    public FloatingTextPool floatingTextPool;

    void Start()
    {
        if (saveManager != null) saveManager.LoadGame();
        GameEvents.UpdateUI();

        InvokeRepeating(nameof(AutoIncome), 1f, 1f);
        InvokeRepeating(nameof(AutoSave), 20f, 20f); // Автосейв раз в 20 сек вместо спама при кликах
    }

    void AutoSave()
    {
        GameEvents.SaveGame();
    }

    // Единый источник правды для автодохода (используется везде: в игре, UI и оффлайн-расчёте)
    public double GetTotalAutoIncomePerSecond()
    {
        double skillMult = skillTreeManager != null ? skillTreeManager.AutoMultiplier : 1.0;
        double charMult = gachaManager != null ? (1.0 + gachaManager.GetTotalCharacterBonus(StatType.AutoMultiplier)) : 1.0;
        return autoIncome * PrestigeMultiplier * skillMult * charMult;
    }

    // Множитель для отображения дохода одного юнита (Автокликера или Менеджера) в магазине
    public double GetTotalAutoMultiplier()
    {
        double skillMult = skillTreeManager != null ? skillTreeManager.AutoMultiplier : 1.0;
        double charMult = gachaManager != null ? (1.0 + gachaManager.GetTotalCharacterBonus(StatType.AutoMultiplier)) : 1.0;
        return PrestigeMultiplier * skillMult * charMult;
    }

    void AutoIncome()
    {
        if (autoIncome > 0)
        {
            score += GetTotalAutoIncomePerSecond();
            GameEvents.UpdateUI();
        }
    }

    public void AddScore()
    {
        double skillClickMult = skillTreeManager != null ? skillTreeManager.ClickMultiplier : 1.0;
        double charClickMult = gachaManager != null ? (1.0 + gachaManager.GetTotalCharacterBonus(StatType.ClickMultiplier)) : 1.0;

        double baseClick = (clickPower + flatClickPower) * skillClickMult * charClickMult;
        double earnedPoints = baseClick * PrestigeMultiplier;
        bool isCrit = false;

        // Суммируем шанс крита из навыков и персонажей
        double totalCritChance = (skillTreeManager != null ? skillTreeManager.CritChance : critChance)
                               + (gachaManager != null ? gachaManager.GetTotalCharacterBonus(StatType.CritChance) : 0.0);

        if (UnityEngine.Random.value <= totalCritChance)
        {
            double totalCritMult = critMultiplier
                                 + (skillTreeManager != null ? skillTreeManager.CritDamageBonus : 0.0)
                                 + (gachaManager != null ? gachaManager.GetTotalCharacterBonus(StatType.CritDamage) : 0.0);

            earnedPoints *= totalCritMult;
            isCrit = true;

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

        if (floatingTextPool != null && Pointer.current != null)
        {
            Vector2 pointerPos = Pointer.current.position.ReadValue();
            Vector3 screenPos = new Vector3(pointerPos.x, pointerPos.y, 0);
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

    public void BuyUpgrade() // x2
    {
        if (score >= upgradeCost)
        {
            score -= upgradeCost;
            clickPower *= 2;
            upgradeCost *= 2.5;
            PlayBuySound();
            GameEvents.UpdateUI();
            GameEvents.SaveGame();
        }
    }

    public void BuyFlatUpgrade() // +5
    {
        if (score >= flatUpgradeCost)
        {
            score -= flatUpgradeCost;
            flatClickPower += 5;
            flatUpgradeCost *= 3;
            PlayBuySound();
            GameEvents.UpdateUI();
            GameEvents.SaveGame();
        }
    }

    public void BuyAutoClicker() // +1/сек
    {
        if (score >= autoUpgradeCost)
        {
            score -= autoUpgradeCost;
            autoIncome += 1;
            autoUpgradeCost *= 2.5;
            PlayBuySound();
            GameEvents.UpdateUI();
            GameEvents.SaveGame();
        }
    }

    public void BuyManager() // +10/сек
    {
        if (score >= managerCost)
        {
            score -= managerCost;
            autoIncome += 10;
            managerCost *= 4;
            PlayBuySound();
            GameEvents.UpdateUI();
            GameEvents.SaveGame();
        }
    }

    public double CalculatePotentialCrystals()
    {
        if (score < prestigeThreshold) return 0;

        double prestigeMult = (skillTreeManager != null ? skillTreeManager.PrestigeBonusMultiplier : 1.0)
                            + (gachaManager != null ? gachaManager.GetTotalCharacterBonus(StatType.PrestigeBonus) : 0.0);

        return System.Math.Floor((score / prestigeThreshold) * prestigeMult);
    }

    public void DoPrestige()
    {
        double crystalsToAdd = CalculatePotentialCrystals();
        if (crystalsToAdd <= 0) return;

        totalCrystals += crystalsToAdd;

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
        clickPower = 1;
        flatClickPower = 0;
        autoIncome = 0;
        upgradeCost = 10;
        flatUpgradeCost = 100;
        autoUpgradeCost = 50;
        managerCost = 500;
        prestigeThreshold *= 2.5;

        GameEvents.SaveGame();
        GameEvents.UpdateUI();
    }

    void PlayBuySound()
    {
        if (audioSource != null && buySound != null)
        {
            audioSource.pitch = 1f;
            audioSource.PlayOneShot(buySound);
        }
    }

    public string FormatNumber(double num)
    {
        if (num < 1000) return num.ToString("0", System.Globalization.CultureInfo.InvariantCulture);

        string[] suffixes = { "", "K", "M", "B", "T", "Qa", "Qi", "Sx", "Sp", "Oc", "No", "Dc" };
        int suffixIndex = 0;
        double originalNum = num;

        while (num >= 1000 && suffixIndex < suffixes.Length - 1)
        {
            num /= 1000;
            suffixIndex++;
        }

        // Если число перевалило за самый большой суффикс (Dc), выводим в компактной научной нотации (например, 1.15e43)
        if (num >= 1000 && suffixIndex == suffixes.Length - 1)
        {
            return originalNum.ToString("0.00e0", System.Globalization.CultureInfo.InvariantCulture);
        }

        return num.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture) + suffixes[suffixIndex];
    }
}