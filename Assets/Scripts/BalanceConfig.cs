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
}