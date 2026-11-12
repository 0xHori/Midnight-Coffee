using System.Collections.Generic;

[System.Serializable]
public class GameData
{
    // Данные ClickerManager
    public double score;
    public double clickPower;
    public double flatClickPower;
    public double autoIncome;
    public double upgradeCost;
    public double flatUpgradeCost;
    public double autoUpgradeCost;
    public double managerCost;
    public double totalCrystals;
    public double prestigeThreshold;

    // Данные SkillTreeManager
    public int clickBoostLvl;
    public int autoBoostLvl;
    public int critBoostLvl;

    // Данные GachaManager
    public List<string> ownedCharacterNames = new List<string>();

    // Время выхода (для офлайн-дохода)
    public string lastExitTime; 
}