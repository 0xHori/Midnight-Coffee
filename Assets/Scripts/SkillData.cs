using UnityEngine;

public enum SkillBranch
{
    Click,      // Вкладка 1: Сила клика
    AutoIncome, // Вкладка 2: Пассивный доход
    CritAndLuck // Вкладка 3: Криты и бонусы
}

public enum StatType
{
    ClickMultiplier,    // Множитель клика (например, +0.5 за уровень)
    AutoMultiplier,     // Множитель автодохода
    CritChance,         // Шанс крита (+0.02 = +2%)
    CritDamage,         // Множитель крита (+1.0 = +100%)
    PrestigeBonus,      // Бонус к получаемым кристаллам
    GoldenButtonReward  // Бонус к награде золотой кнопки
}

[CreateAssetMenu(fileName = "NewSkill", menuName = "Clicker/Skill Data")]
public class SkillData : ScriptableObject
{
    [Header("Идентификация")]
    public string skillId;          // Уникальный ID для сохранения, например: "click_power_1"
    public string title;            // Название в UI
    [TextArea] public string description;
    public Sprite icon;

    [Header("RPG-ветка и требования")]
    public SkillBranch branch;      // К какой вкладке относится
    public int orderInBranch;       // Порядок в линейке (0, 1, 2...)
    public SkillData requiredSkill; // Предыдущий навык в ветке (null, если первый)
    public int requiredSkillLevel;  // Какой уровень предыдущего навыка нужен для открытия

    [Header("Прокачка и экономика")]
    public int maxLevel = 10;
    public double baseCost = 5;     // Базовая цена в кристаллах
    public double costMultiplier = 1.5; // Во сколько раз растет цена за уровень

    [Header("Эффект")]
    public StatType statType;
    public double valuePerLevel;    // Прибавка за 1 уровень

    // Расчет цены для конкретного уровня
    public double GetCost(int currentLevel)
    {
        return System.Math.Floor(baseCost * System.Math.Pow(costMultiplier, currentLevel));
    }

    // Расчет текущего бонуса
    public double GetBonus(int currentLevel)
    {
        return valuePerLevel * currentLevel;
    }
}