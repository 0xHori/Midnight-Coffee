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
    public int rollsSinceLastLegendary = 0;

    private Dictionary<string, int> characterLevels = new Dictionary<string, int>();
    public string equippedCharacterId;
    private readonly HashSet<string> _processedIds = new HashSet<string>();

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
        if (clickerManager == null || allCharacters == null || allCharacters.Count == 0) return;

        if (clickerManager.totalCrystals < rollCost)
        {
            GameEvents.NotEnoughCurrency();
            GameEvents.ShowGachaResult(LocalizationManager.Get("gacha.not_enough"), null, Color.red);
            return;
        }

        BalanceConfig balance = affectionManager != null ? affectionManager.balanceConfig : null;
        int pityThreshold = balance != null ? balance.gachaPityThreshold : 35;
        int maxDupLvl = balance != null ? balance.maxCharacterDuplicateLevel : 15;

        clickerManager.totalCrystals -= rollCost;
        totalGachaRolls++;
        rollsSinceLastLegendary++;

        int targetRarity = 1;
        if (pityThreshold > 0 && rollsSinceLastLegendary >= pityThreshold)
        {
            targetRarity = 3;
        }
        else
        {
            float roll = Random.Range(0f, 100f);
            if (roll > 70f && roll <= 95f) targetRarity = 2;
            else if (roll > 95f) targetRarity = 3;
        }

        // Выбираем случайного персонажа нужной редкости без лишних аллокаций списков
        CharacterData result = PickRandomCharacterByRarity(targetRarity);
        if (result == null)
            result = allCharacters[Random.Range(0, allCharacters.Count)];

        if (result.rarity == 3)
            rollsSinceLastLegendary = 0;

        int currentLvl = GetCharacterLevel(result.characterId);
        int affLvl = affectionManager != null ? affectionManager.GetAffectionLevel(result.characterId) : result.maxAffectionLevel;
        bool canGainAffection = affectionManager != null && affLvl < result.maxAffectionLevel;
        string displayTitle;

        if (currentLvl < maxDupLvl)
        {
            int newLvl = currentLvl + 1;
            characterLevels[result.characterId] = newLvl;

            if (currentLvl > 0)
            {
                if (canGainAffection)
                    affectionManager.AddAffectionXp(result, 50 * result.rarity);
                else
                    clickerManager.totalCrystals += Mathf.Max(1, rollCost / 2);
            }

            displayTitle = currentLvl > 0
                ? LocalizationManager.Get("gacha.result_dup", result.LocalizedName, newLvl)
                : LocalizationManager.Get("gacha.result_new", result.LocalizedName);
        }
        else
        {
            if (canGainAffection)
                affectionManager.AddAffectionXp(result, 150 * result.rarity);
            else
                clickerManager.totalCrystals += Mathf.Max(1, rollCost / 2);

            displayTitle = LocalizationManager.Get("gacha.result_dup", result.LocalizedName, maxDupLvl);
        }

        if (string.IsNullOrEmpty(equippedCharacterId))
        {
            equippedCharacterId = result.characterId;
            SyncEquippedCharacterVisual();
        }

        Color textColor = Color.white;
        if (result.rarity == 2) textColor = new Color(0.2f, 0.6f, 1f);
        if (result.rarity == 3) textColor = new Color(1f, 0.8f, 0f);

        GameEvents.ShowGachaResult(displayTitle, result.portrait, textColor);
        GameEvents.UpdateUI();
        GameEvents.SaveGame();
    }

    private CharacterData PickRandomCharacterByRarity(int rarity)
    {
        int matchCount = 0;
        for (int i = 0; i < allCharacters.Count; i++)
        {
            if (allCharacters[i] != null && allCharacters[i].rarity == rarity)
                matchCount++;
        }

        if (matchCount == 0) return null;

        int targetIndex = Random.Range(0, matchCount);
        int currentIndex = 0;
        for (int i = 0; i < allCharacters.Count; i++)
        {
            if (allCharacters[i] != null && allCharacters[i].rarity == rarity)
            {
                if (currentIndex == targetIndex)
                    return allCharacters[i];
                currentIndex++;
            }
        }

        return null;
    }

    void OnValidate()
    {
        if (allCharacters == null) return;
        HashSet<string> seen = new HashSet<string>();
        for (int i = 0; i < allCharacters.Count; i++)
        {
            var ch = allCharacters[i];
            if (ch == null || string.IsNullOrEmpty(ch.characterId)) continue;
            if (!seen.Add(ch.characterId))
                Debug.LogWarning($"[GachaManager] Дублирующийся characterId в списке: {ch.characterId}", this);
        }
    }

    public double GetTotalCharacterBonus(StatType type)
    {
        if (allCharacters == null) return 0;

        double total = 0;
        BalanceConfig balance = affectionManager != null ? affectionManager.balanceConfig : null;
        _processedIds.Clear();

        foreach (var ch in allCharacters)
        {
            if (ch == null || string.IsNullOrEmpty(ch.characterId)) continue;
            if (!_processedIds.Add(ch.characterId)) continue;

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