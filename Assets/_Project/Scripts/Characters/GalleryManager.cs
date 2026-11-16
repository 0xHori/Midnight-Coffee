using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GalleryManager : MonoBehaviour
{
    public GachaManager gachaManager;
    public AffectionManager affectionManager;
    public GameObject cardPrefab;
    public Transform contentContainer;

    [Header("UI деталей персонажа")]
    public TextMeshProUGUI detailsText;

    private readonly Dictionary<string, CharacterCardUI> _spawnedCards = new Dictionary<string, CharacterCardUI>();
    private bool _isBuilt = false;

    public void OpenGallery()
    {
        RefreshGallery();
        ShowEquippedOrDefaultDetails();
    }

    private void EnsureBuilt()
    {
        if (_isBuilt || gachaManager == null || cardPrefab == null || contentContainer == null) return;
        _isBuilt = true;

        foreach (Transform child in contentContainer)
            Destroy(child.gameObject);
        _spawnedCards.Clear();

        foreach (CharacterData character in gachaManager.allCharacters)
        {
            if (character == null || string.IsNullOrEmpty(character.characterId)) continue;

            GameObject newCard = Instantiate(cardPrefab, contentContainer);
            CharacterCardUI cardUI = newCard.GetComponent<CharacterCardUI>();
            if (cardUI != null)
            {
                cardUI.Bind(character, HandleCardClicked);
                _spawnedCards[character.characterId] = cardUI;
            }
        }
    }

    public void RefreshGallery()
    {
        if (gachaManager == null) return;
        EnsureBuilt();

        foreach (CharacterData character in gachaManager.allCharacters)
        {
            if (character == null || !_spawnedCards.TryGetValue(character.characterId, out CharacterCardUI cardUI))
                continue;

            int level = gachaManager.GetCharacterLevel(character.characterId);
            bool isEquipped = gachaManager.equippedCharacterId == character.characterId;
            int affectionLvl = affectionManager != null
                ? affectionManager.GetAffectionLevel(character.characterId)
                : 1;

            cardUI.Refresh(level, isEquipped, affectionLvl);
        }
    }

    private void HandleCardClicked(CharacterData character)
    {
        if (character == null || gachaManager == null) return;

        int level = gachaManager.GetCharacterLevel(character.characterId);
        if (level > 0)
        {
            gachaManager.EquipCharacter(character.characterId);
            RefreshGallery();
        }
        UpdateCharacterDetails(character);
    }

    private void ShowEquippedOrDefaultDetails()
    {
        if (gachaManager != null && !string.IsNullOrEmpty(gachaManager.equippedCharacterId))
        {
            CharacterData equipped = gachaManager.GetEquippedCharacter();
            if (equipped != null)
            {
                UpdateCharacterDetails(equipped);
                return;
            }
        }

        UpdateCharacterDetails(null);
    }

    public void UpdateCharacterDetails(CharacterData character)
    {
        if (detailsText == null) return;

        if (character == null)
        {
            detailsText.text = LocalizationManager.Get("gallery.select_hint");
            return;
        }

        int level = gachaManager.GetCharacterLevel(character.characterId);
        if (level <= 0)
        {
            detailsText.text = LocalizationManager.Get("gallery.locked_hint");
            return;
        }

        BalanceConfig balance = affectionManager != null ? affectionManager.balanceConfig : null;
        int affectionLvl = affectionManager != null
            ? affectionManager.GetAffectionLevel(character.characterId)
            : 1;

        double affectionBonusPct = (character.GetAffectionMultiplier(affectionLvl, balance) - 1.0) * 100.0;
        double passiveBonus = character.GetPassiveBonus(level, affectionLvl, balance) * 100.0;
        double activeBonus = character.GetActiveBonus(level, affectionLvl, balance) * 100.0;

        string passiveDesc = GetStatDescription(character.passiveStat);
        string activeDesc = GetStatDescription(character.activeStat);

        string affectionLine = affectionLvl >= character.maxAffectionLevel
            ? LocalizationManager.Get("gallery.affection_max", $"{affectionBonusPct:0.#}")
            : LocalizationManager.Get("gallery.affection_lvl", affectionLvl, character.maxAffectionLevel, $"{affectionBonusPct:0.#}");

        string abilityLine = !string.IsNullOrEmpty(character.LocalizedAbility)
            ? $"\n<i><color=#CCCCCC>{character.LocalizedAbility}</color></i>"
            : "";

        detailsText.text = LocalizationManager.Get(
            "gallery.details_body",
            character.LocalizedName,
            level,
            affectionLine,
            $"{passiveBonus:0.#}",
            passiveDesc,
            $"{activeBonus:0.#}",
            activeDesc,
            abilityLine
        );
    }

    private string GetStatDescription(StatType type)
    {
        switch (type)
        {
            case StatType.ClickMultiplier: return LocalizationManager.Get("stat.click");
            case StatType.AutoMultiplier: return LocalizationManager.Get("stat.auto");
            case StatType.CritChance: return LocalizationManager.Get("stat.crit");
            default: return LocalizationManager.Get("stat.default");
        }
    }
}