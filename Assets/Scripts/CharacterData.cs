using UnityEngine;

[CreateAssetMenu(fileName = "NewCharacter", menuName = "Gacha/Character")]
public class CharacterData : ScriptableObject
{
    [Header("Основное")]
    public string characterId;      // Уникальный ID (например: "char_rem") — надежнее, чем имя!
    public string characterName;
    public Sprite portrait;         // Картинка (9:16)
    [Range(1, 3)] public int rarity = 1; // 1 - Common, 2 - Rare, 3 - Legendary

    [Header("Пассивный бонус (работает всегда из коллекции)")]
    public StatType passiveStat;
    public double basePassiveValue; // Например: 0.05 (+5% к доходу)

    [Header("Активный бонус (если персонаж выбран главным)")]
    public StatType activeStat;
    public double baseActiveValue;  // Например: 0.25 (+25% к доходу или шансу крита)
    [TextArea] public string abilityDescription;

    // Уровень растет от дубликатов (1 - базовый, 2 - один дубликат и т.д.)
    public double GetPassiveBonus(int level)
    {
        if (level <= 0) return 0;
        // Каждый дубликат усиливает бонус на 20%
        return basePassiveValue * (1.0 + (level - 1) * 0.2);
    }

    public double GetActiveBonus(int level)
    {
        if (level <= 0) return 0;
        return baseActiveValue * (1.0 + (level - 1) * 0.2);
    }
}