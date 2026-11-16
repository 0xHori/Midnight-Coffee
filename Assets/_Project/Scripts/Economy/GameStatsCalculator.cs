using System;

public class GameStatsCalculator
{
    private readonly BalanceConfig _config;
    private readonly ShopManager _shop;
    private readonly SkillTreeManager _skills;
    private readonly GachaManager _gacha;
    private readonly FeverManager _fever;

    public GameStatsCalculator(
        BalanceConfig config,
        ShopManager shop,
        SkillTreeManager skills,
        GachaManager gacha,
        FeverManager fever)
    {
        _config = config;
        _shop = shop;
        _skills = skills;
        _gacha = gacha;
        _fever = fever;
    }

    public double BaseClickPower => _config != null ? _config.baseClickPower : 1.0;
    public double BaseCritChance => _config != null ? _config.baseCritChance : 0.1;
    public double BaseCritMultiplier => _config != null ? _config.baseCritMultiplier : 10.0;
    public double CrystalBonusPerUnit => _config != null ? _config.crystalBonusPerUnit : 0.02;

    public double GetFeverMultiplier() => _fever != null ? _fever.CurrentMultiplier : 1.0;

    public double GetPrestigeMultiplier(double totalCrystals)
    {
        return 1.0 + (totalCrystals * CrystalBonusPerUnit);
    }

    public double GetTotalClickPower(double totalCrystals)
    {
        double shopFlat = _shop != null ? _shop.TotalFlatClick : 0.0;
        double shopMult = _shop != null ? _shop.TotalClickMultiplier : 1.0;
        double skillMult = _skills != null ? _skills.ClickMultiplier : 1.0;
        double charMult = _gacha != null ? (1.0 + _gacha.GetTotalCharacterBonus(StatType.ClickMultiplier)) : 1.0;

        double total = (BaseClickPower + shopFlat)
                     * shopMult
                     * skillMult
                     * charMult
                     * GetPrestigeMultiplier(totalCrystals)
                     * GetFeverMultiplier();

        return (double.IsInfinity(total) || total > 1e300) ? 1e300 : total;
    }

    public double GetTotalCritChance()
    {
        double skillBonus = _skills != null ? _skills.GetTotalStatBonus(StatType.CritChance) : 0.0;
        double charBonus = _gacha != null ? _gacha.GetTotalCharacterBonus(StatType.CritChance) : 0.0;
        return Math.Min(1.0, BaseCritChance + skillBonus + charBonus);
    }

    public double GetTotalCritMultiplier()
    {
        double skillBonus = _skills != null ? _skills.CritDamageBonus : 0.0;
        double charBonus = _gacha != null ? _gacha.GetTotalCharacterBonus(StatType.CritDamage) : 0.0;
        return BaseCritMultiplier + skillBonus + charBonus;
    }

    public double GetTotalAutoMultiplier(double totalCrystals)
    {
        double skillMult = _skills != null ? _skills.AutoMultiplier : 1.0;
        double charMult = _gacha != null ? (1.0 + _gacha.GetTotalCharacterBonus(StatType.AutoMultiplier)) : 1.0;
        return GetPrestigeMultiplier(totalCrystals) * skillMult * charMult * GetFeverMultiplier();
    }

    public double GetTotalAutoIncomePerSecond(double totalCrystals)
    {
        double baseAuto = _shop != null ? _shop.TotalBaseAutoIncome : 0.0;
        if (baseAuto <= 0) return 0.0;
        double total = baseAuto * GetTotalAutoMultiplier(totalCrystals);
        return (double.IsInfinity(total) || total > 1e300) ? 1e300 : total;
    }

    public double CalculatePotentialCrystals(double score, double prestigeThreshold)
    {
        if (score < prestigeThreshold || prestigeThreshold <= 0) return 0.0;

        double exp = _config != null ? _config.prestigeFormulaExponent : 0.4;
        double scale = _config != null ? _config.prestigeFormulaScale : 5.0;

        double prestigeMult = (_skills != null ? _skills.PrestigeBonusMultiplier : 1.0)
                            + (_gacha != null ? _gacha.GetTotalCharacterBonus(StatType.PrestigeBonus) : 0.0);

        double rawCrystals = Math.Pow(score / prestigeThreshold, exp) * scale;
        return Math.Floor(rawCrystals * prestigeMult);
    }

    public double CalculateGoldenButtonReward(double totalCrystals)
    {
        double autoSec = _config != null ? _config.goldenButtonAutoSeconds : 60.0;
        double clickMult = _config != null ? _config.goldenButtonClickMult : 50.0;
        double minReward = _config != null ? _config.goldenButtonMinReward : 100.0;

        double bonusMult = 1.0
                         + (_skills != null ? _skills.GetTotalStatBonus(StatType.GoldenButtonReward) : 0.0)
                         + (_gacha != null ? _gacha.GetTotalCharacterBonus(StatType.GoldenButtonReward) : 0.0);

        double baseReward = (GetTotalAutoIncomePerSecond(totalCrystals) * autoSec)
                          + (GetTotalClickPower(totalCrystals) * clickMult);

        return Math.Max(minReward, baseReward * bonusMult);
    }
}