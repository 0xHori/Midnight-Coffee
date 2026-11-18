using UnityEngine;

[CreateAssetMenu(fileName = "BalanceConfig", menuName = "Clicker/Balance Config")]
public class BalanceConfig : ScriptableObject
{
    [Header("Базовые статы игрока")]
    public double baseClickPower = 1.0;
    [Range(0f, 1f)] public double baseCritChance = 0.1;
    public double baseCritMultiplier = 10.0;

    [Header("Престиж и Кристаллы")]
    public double initialPrestigeThreshold = 100000.0;
    public double prestigeThresholdGrowth = 2.5;
    public double crystalBonusPerUnit = 0.02; // +2% за каждый кристалл
    public double prestigeFormulaExponent = 0.4;
    public double prestigeFormulaScale = 5.0;

    [Header("Fever Mode")]
    public float feverChargePerTap = 0.03f;
    public float feverDecayDelay = 0.8f;
    public float feverDecaySpeed = 0.25f;
    public float feverDuration = 10f;
    public double feverMultiplier = 3.0;

    [Header("Офлайн и Бонусы")]
    public double maxOfflineHours = 8.0;
    public double goldenButtonAutoSeconds = 60.0;
    public double goldenButtonClickMult = 50.0;
    public double goldenButtonMinReward = 100.0;

    [Header("Персонажи")]
    public double characterDuplicateBonusPerLevel = 0.2; // +20% за каждый дубликат
    public int maxCharacterDuplicateLevel = 15;
    [Tooltip("Гарантированная легендарка (3★), если она не выпадала N круток подряд")]
    public int gachaPityThreshold = 35;

    [Header("Близость (Affection)")]
    [Tooltip("Базовый опыт для перехода с 1 на 2 уровень близости")]
    public int baseAffectionXp = 150;
    [Tooltip("Множитель роста требуемого опыта на каждый следующий уровень")]
    public float affectionXpGrowth = 1.6f;
    [Tooltip("Бонус к статам персонажа за каждый уровень близости (0.03 = +3%)")]
    public double affectionBonusPerLevel = 0.03;

    public int GetRequiredAffectionXp(int currentLevel, int maxLevel)
    {
        if (currentLevel >= maxLevel) return 0;
        return Mathf.RoundToInt(baseAffectionXp * Mathf.Pow(affectionXpGrowth, Mathf.Max(0, currentLevel - 1)));
    }

    public double GetAffectionMultiplier(int affectionLevel)
    {
        if (affectionLevel <= 1) return 1.0;
        return 1.0 + (affectionLevel - 1) * affectionBonusPerLevel;
    }
}