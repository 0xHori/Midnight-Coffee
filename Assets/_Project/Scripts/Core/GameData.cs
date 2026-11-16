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
public class IdProgressPair
{
    public string id;
    public int level;
    public int xp;

    public IdProgressPair(string id, int level, int xp)
    {
        this.id = id;
        this.level = level;
        this.xp = xp;
    }
}

[System.Serializable]
public class GameData
{
    // Основные ресурсы
    public double score;
    public double totalCrystals;
    public double lifetimeCrystals;
    public double prestigeThreshold = 0;

    // Статистика игрока за всё время
    public long totalClicks;
    public long totalCritClicks;
    public double lifetimeScore;
    public int totalPrestiges;
    public int totalGachaRolls;

    // Масштабируемые списки для Магазина, RPG-навыков, Гачи и Близости
    public List<IdLevelPair> purchasedUpgrades = new List<IdLevelPair>();
    public List<IdLevelPair> unlockedSkills = new List<IdLevelPair>();
    public List<IdLevelPair> ownedCharacters = new List<IdLevelPair>();
    public List<IdProgressPair> characterAffections = new List<IdProgressPair>();
    public string equippedCharacterId;

    // Время выхода
    public string lastExitTime;
}