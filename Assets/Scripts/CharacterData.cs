using UnityEngine;

public enum CharacterEmotion
{
    Normal,     // Спокойная / радостная (по умолчанию)
    Excited,    // Удивление / азарт (серия кликов, крит)
    Love,       // Сердечки (Fever Mode / повышение близости)
    Angry       // Надутая / сердитая (не хватает валюты / долгий простой)
}

[System.Serializable]
public struct EmotionSprite
{
    public CharacterEmotion emotion;
    public Sprite sprite;
}

[CreateAssetMenu(fileName = "NewCharacter", menuName = "Gacha/Character")]
public class CharacterData : ScriptableObject
{
    [Header("Основное")]
    public string characterId;      // Уникальный ID (например: "char_rem")
    public string characterName;
    public Sprite portrait;         // Базовый портрет / иконка для галереи и гачи
    [Range(1, 3)] public int rarity = 1; // 1 - Common, 2 - Rare, 3 - Legendary

    [Header("Эмоции (4 стикера)")]
    public EmotionSprite[] emotions;

    [Header("Близость (Affection)")]
    public int maxAffectionLevel = 10;

    [Header("Пассивный бонус (работает всегда из коллекции)")]
    public StatType passiveStat;
    public double basePassiveValue; // Например: 0.05 (+5% к доходу)

    [Header("Активный бонус (если персонаж выбран главным)")]
    public StatType activeStat;
    public double baseActiveValue;  // Например: 0.25 (+25% к доходу или шансу крита)
    [TextArea] public string abilityDescription;

    // Возвращает спрайт нужной эмоции (если не задан — откатывается на Normal или portrait)
    public Sprite GetEmotionSprite(CharacterEmotion targetEmotion)
    {
        if (emotions != null)
        {
            for (int i = 0; i < emotions.Length; i++)
            {
                if (emotions[i].emotion == targetEmotion && emotions[i].sprite != null)
                    return emotions[i].sprite;
            }

            // Если запрошенной эмоции нет, пробуем вернуть Normal
            if (targetEmotion != CharacterEmotion.Normal)
            {
                for (int i = 0; i < emotions.Length; i++)
                {
                    if (emotions[i].emotion == CharacterEmotion.Normal && emotions[i].sprite != null)
                        return emotions[i].sprite;
                }
            }
        }

        return portrait;
    }

    // Уровень растет от дубликатов (1 - базовый, 2 - один дубликат и т.д.)
    public double GetPassiveBonus(int level)
    {
        if (level <= 0) return 0;
        return basePassiveValue * (1.0 + (level - 1) * 0.2);
    }

    public double GetActiveBonus(int level)
    {
        if (level <= 0) return 0;
        return baseActiveValue * (1.0 + (level - 1) * 0.2);
    }
}