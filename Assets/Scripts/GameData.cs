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

    // Масштабируемые списки для Магазина, RPG-навыков и Гачи
    public List<IdLevelPair> purchasedUpgrades = new List<IdLevelPair>();
    public List<IdLevelPair> unlockedSkills = new List<IdLevelPair>();
    public List<IdLevelPair> ownedCharacters = new List<IdLevelPair>();
    public string equippedCharacterId;

    // Время выхода
    public string lastExitTime;
}