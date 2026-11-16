using System.Collections.Generic;
using UnityEngine;

public class GachaManager : MonoBehaviour
{
    public ClickerManager clickerManager;
    public AffectionManager affectionManager;

    [Header("Пул персонажей")]
    public List<CharacterData> allCharacters = new List<CharacterData>();

    [Header("Настройки")]
    public int rollCost = 10;
    public int totalGachaRolls = 0;

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
        for (int i = 0; i < allCharacters.Count; i++)
        {
            if (allCharacters[i] != null && allCharacters[i].characterId == equippedCharacterId)
                return allCharacters[i];
        }
        return null;
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
        GameEvents.CharacterEquipped(GetEquippedCharacter());
    }

    public void RollGacha()
    {
        if (clickerManager.totalCrystals < rollCost)
        {
            GameEvents.NotEnoughCurrency();
            GameEvents.ShowGachaResult(LocalizationManager.Get("gacha.not_enough"), null, Color.red);
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

        int currentLvl = GetCharacterLevel(result.characterId);
        int newLvl = currentLvl + 1;
        characterLevels[result.characterId] = newLvl;

        if (string.IsNullOrEmpty(equippedCharacterId))
        {
            equippedCharacterId = result.characterId;
            SyncEquippedCharacterVisual();
        }

        Color textColor = Color.white;
        if (targetRarity == 2) textColor = new Color(0.2f, 0.6f, 1f);
        if (targetRarity == 3) textColor = new Color(1f, 0.8f, 0f);

        string displayTitle = currentLvl > 0
            ? LocalizationManager.Get("gacha.result_dup", result.LocalizedName, newLvl)
            : LocalizationManager.Get("gacha.result_new", result.LocalizedName);

        GameEvents.ShowGachaResult(displayTitle, result.portrait, textColor);
        GameEvents.UpdateUI();
        GameEvents.SaveGame();
    }

    public double GetTotalCharacterBonus(StatType type)
    {
        double total = 0;
        BalanceConfig balance = affectionManager != null ? affectionManager.balanceConfig : null;

        foreach (var ch in allCharacters)
        {
            if (ch == null) continue;
            int lvl = GetCharacterLevel(ch.characterId);
            if (lvl <= 0) continue;

            int affectionLvl = affectionManager != null ? affectionManager.GetAffectionLevel(ch.characterId) : 1;

            if (ch.passiveStat == type)
                total += ch.GetPassiveBonus(lvl, affectionLvl, balance);

            if (ch.characterId == equippedCharacterId && ch.activeStat == type)
                total += ch.GetActiveBonus(lvl, affectionLvl, balance);
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