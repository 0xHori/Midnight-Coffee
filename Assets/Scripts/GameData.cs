using System.Collections.Generic;

[System.Serializable]
public class IdLevelPair
{
    public string id;
    public int level;

    public IdLevelPair(string id, int level)
    {
        this.id = id;
        this.level = level;
    }
}

[System.Serializable]
public class GameData
{
    // Основные ресурсы
    public double score;
    public double totalCrystals;
    public double prestigeThreshold = 10000;

    // Базовые апгрейды (пока оставляем твои, на Шаге 2 тоже переведем на SO)
    public double clickPower = 1;
    public double flatClickPower = 0;
    public double autoIncome = 0;
    public double upgradeCost = 10;
    public double flatUpgradeCost = 100;
    public double autoUpgradeCost = 50;
    public double managerCost = 500;

    // Масштабируемые списки для RPG-навыков и Гачи
    public List<IdLevelPair> unlockedSkills = new List<IdLevelPair>();
    public List<IdLevelPair> ownedCharacters = new List<IdLevelPair>();
    public string equippedCharacterId; // ID персонажа, который сейчас стоит на главном экране

    // Время выхода
    public string lastExitTime;
}