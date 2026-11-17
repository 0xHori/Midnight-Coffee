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
    [Tooltip("Реплики, открывающиеся по мере роста близости (индекс 0 = 1 ур. и т.д.)")]
    [TextArea] public string[] affectionQuotes;

    [Header("Пассивный бонус (работает всегда из коллекции)")]
    public StatType passiveStat;
    public double basePassiveValue; // Например: 0.05 (+5% к доходу)

    [Header("Активный бонус (если персонаж выбран главным)")]
    public StatType activeStat;
    public double baseActiveValue;  // Например: 0.25 (+25% к доходу или шансу крита)
    [TextArea] public string abilityDescription;

    public Sprite GetEmotionSprite(CharacterEmotion targetEmotion)
    {
        if (emotions != null)
        {
            for (int i = 0; i < emotions.Length; i++)
            {
                if (emotions[i].emotion == targetEmotion && emotions[i].sprite != null)
                    return emotions[i].sprite;
            }

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

    public int GetRequiredAffectionXp(int currentLevel, BalanceConfig balance)
    {
        if (balance == null)
        {
            Debug.LogError($"[CharacterData] BalanceConfig не передан для {characterId}");
            return 0;
        }
        return balance.GetRequiredAffectionXp(currentLevel, maxAffectionLevel);
    }

    public double GetAffectionMultiplier(int affectionLevel, BalanceConfig balance)
    {
        if (balance == null) return 1.0;
        return balance.GetAffectionMultiplier(affectionLevel);
    }

    private string GetLocalizedQuoteByIndex(int index)
    {
        if (!string.IsNullOrEmpty(characterId))
        {
            string key = $"char.{characterId}.quote.{index}";
            string val = LocalizationManager.Get(key);
            if (val != $"[{key}]") return val;
        }
        return (affectionQuotes != null && index >= 0 && index < affectionQuotes.Length)
            ? affectionQuotes[index]
            : string.Empty;
    }

    public string GetQuoteForLevel(int affectionLevel)
    {
        int len = (affectionQuotes != null && affectionQuotes.Length > 0) ? affectionQuotes.Length : 5;
        int index = Mathf.Clamp(affectionLevel - 1, 0, len - 1);
        return GetLocalizedQuoteByIndex(index);
    }

    public string GetRandomUnlockedQuote(int affectionLevel)
    {
        int len = (affectionQuotes != null && affectionQuotes.Length > 0) ? affectionQuotes.Length : 5;
        int maxIndex = Mathf.Clamp(affectionLevel - 1, 0, len - 1);
        int minIndex = 0;
        return GetLocalizedQuoteByIndex(Random.Range(minIndex, maxIndex + 1));
    }

    public double GetPassiveBonus(int duplicateLevel, int affectionLevel = 1, BalanceConfig balance = null)
    {
        if (duplicateLevel <= 0) return 0;
        double dupBonus = balance != null ? balance.characterDuplicateBonusPerLevel : 0.2;
        double baseVal = basePassiveValue * (1.0 + (duplicateLevel - 1) * dupBonus);
        return baseVal * GetAffectionMultiplier(affectionLevel, balance);
    }

    public double GetActiveBonus(int duplicateLevel, int affectionLevel = 1, BalanceConfig balance = null)
    {
        if (duplicateLevel <= 0) return 0;
        double dupBonus = balance != null ? balance.characterDuplicateBonusPerLevel : 0.2;
        double baseVal = baseActiveValue * (1.0 + (duplicateLevel - 1) * dupBonus);
        return baseVal * GetAffectionMultiplier(affectionLevel, balance);
    }

    public string LocalizedName
    {
        get
        {
            if (string.IsNullOrEmpty(characterId)) return characterName;
            string key = $"char.{characterId}.name";
            string val = LocalizationManager.Get(key);
            return val == $"[{key}]" ? characterName : val;
        }
    }

    public string LocalizedAbility
    {
        get
        {
            if (string.IsNullOrEmpty(characterId)) return abilityDescription;
            string key = $"char.{characterId}.ability";
            string val = LocalizationManager.Get(key);
            return val == $"[{key}]" ? abilityDescription : val;
        }
    }
}