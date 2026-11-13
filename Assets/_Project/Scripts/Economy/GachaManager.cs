using System.Collections.Generic;
using UnityEngine;

public class GachaManager : MonoBehaviour
{
    public ClickerManager clickerManager;
    public CharacterEmotionController emotionController;

    [Header("Пул персонажей")]
    public List<CharacterData> allCharacters = new List<CharacterData>();

    [Header("Настройки")]
    public int rollCost = 10;
    public int totalGachaRolls = 0;

    // characterId -> уровень (1 = выбит 1 раз, 2 = один дубликат и т.д.)
    private Dictionary<string, int> characterLevels = new Dictionary<string, int>();
    public string equippedCharacterId;

    void Start()
    {
        SyncEquippedCharacterVisual();
    }

    public int GetCharacterLevel(string charId)
    {
        return characterLevels.TryGetValue(charId, out int lvl) ? lvl : 0;
    }

    public CharacterData GetEquippedCharacter()
    {
        if (string.IsNullOrEmpty(equippedCharacterId) || allCharacters == null) return null;
        return allCharacters.Find(c => c != null && c.characterId == equippedCharacterId);
    }

    public void EquipCharacter(string charId)
    {
        if (GetCharacterLevel(charId) > 0)
        {
            equippedCharacterId = charId;
            SyncEquippedCharacterVisual();
            GameEvents.UpdateUI();
            GameEvents.SaveGame();
        }
    }

    private void SyncEquippedCharacterVisual()
    {
        if (emotionController == null) return;
        CharacterData equipped = GetEquippedCharacter();
        if (equipped != null)
        {
            emotionController.SetCharacter(equipped);
        }
    }

    public void RollGacha()
    {
        if (clickerManager.totalCrystals < rollCost)
        {
            emotionController?.OnNotEnoughCurrency();
            GameEvents.ShowGachaResult("Не хватает кристаллов!", null, Color.red);
            return;
        }

        if (allCharacters == null || allCharacters.Count == 0) return;

        clickerManager.totalCrystals -= rollCost;
        totalGachaRolls++;

        float roll = Random.Range(0f, 100f);
        int targetRarity = 1;
        if (roll > 70f && roll <= 95f) targetRarity = 2;
        else if (roll > 95f) targetRarity = 3;

        List<CharacterData> pool = allCharacters.FindAll(c => c.rarity == targetRarity);
        if (pool.Count == 0) pool = allCharacters;

        CharacterData result = pool[Random.Range(0, pool.Count)];

        // Прокачка уровня при дубликате
        int currentLvl = GetCharacterLevel(result.characterId);
        int newLvl = currentLvl + 1;
        characterLevels[result.characterId] = newLvl;

        // Если это первый выбитый персонаж — ставим его главным автоматически
        if (string.IsNullOrEmpty(equippedCharacterId))
        {
            equippedCharacterId = result.characterId;
            SyncEquippedCharacterVisual();
        }

        Color textColor = Color.white;
        if (targetRarity == 2) textColor = new Color(0.2f, 0.6f, 1f);
        if (targetRarity == 3) textColor = new Color(1f, 0.8f, 0f);

        string displayTitle = currentLvl > 0
            ? $"{result.characterName} (Ур. {newLvl}!)"
            : $"{result.characterName} (НОВЫЙ!)";

        // В баннере гачи показываем основной portrait (9:16), а на главном экране остаются стикеры
        GameEvents.ShowGachaResult(displayTitle, result.portrait, textColor);
        GameEvents.UpdateUI();
        GameEvents.SaveGame();
    }

    // Считаем суммарный бонус от всей коллекции + активного персонажа
    public double GetTotalCharacterBonus(StatType type)
    {
        double total = 0;
        foreach (var ch in allCharacters)
        {
            if (ch == null) continue;
            int lvl = GetCharacterLevel(ch.characterId);
            if (lvl <= 0) continue;

            // Пассивный бонус работает у всех открытых
            if (ch.passiveStat == type)
                total += ch.GetPassiveBonus(lvl);

            // Активный бонус работает только у выбранного главным
            if (ch.characterId == equippedCharacterId && ch.activeStat == type)
                total += ch.GetActiveBonus(lvl);
        }
        return total;
    }

    public List<IdLevelPair> ExportSaveData()
    {
        List<IdLevelPair> list = new List<IdLevelPair>();
        foreach (var kvp in characterLevels)
        {
            list.Add(new IdLevelPair(kvp.Key, kvp.Value));
        }
        return list;
    }

    public void ImportSaveData(List<IdLevelPair> savedList, string equippedId)
    {
        characterLevels.Clear();
        if (savedList != null)
        {
            foreach (var pair in savedList)
            {
                characterLevels[pair.id] = pair.level;
            }
        }
        equippedCharacterId = equippedId;
        SyncEquippedCharacterVisual();
    }
}