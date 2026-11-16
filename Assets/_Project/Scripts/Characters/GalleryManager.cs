using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GalleryManager : MonoBehaviour
{
    public GachaManager gachaManager;
    public AffectionManager affectionManager;
    public GameObject cardPrefab;
    public Transform contentContainer;

    [Header("UI деталей персонажа")]
    public TextMeshProUGUI detailsText;

    public void OpenGallery()
    {
        RefreshGallery();
        ShowEquippedOrDefaultDetails();
    }

    public void RefreshGallery()
    {
        if (gachaManager == null || cardPrefab == null || contentContainer == null) return;

        foreach (Transform child in contentContainer)
        {
            Destroy(child.gameObject);
        }

        foreach (CharacterData character in gachaManager.allCharacters)
        {
            if (character == null) continue;

            GameObject newCard = Instantiate(cardPrefab, contentContainer);

            TextMeshProUGUI nameText = newCard.transform.Find("NameText").GetComponent<TextMeshProUGUI>();
            Image portraitImage = newCard.transform.Find("Portrait").GetComponent<Image>();
            Image cardBg = newCard.GetComponent<Image>();

            portraitImage.sprite = character.portrait;
            portraitImage.preserveAspect = true;

            int level = gachaManager.GetCharacterLevel(character.characterId);
            bool isOwned = level > 0;

            Button cardBtn = newCard.GetComponent<Button>();

            if (!isOwned)
            {
                if (cardBg != null)
                    cardBg.color = new Color(0.16f, 0.16f, 0.18f, 1f);

                portraitImage.color = new Color(0, 0, 0, 1f);
                nameText.text = "???";

                if (cardBtn != null)
                {
                    CharacterData charCopy = character;
                    cardBtn.onClick.RemoveAllListeners();
                    cardBtn.onClick.AddListener(() => UpdateCharacterDetails(charCopy));
                }
            }
            else
            {
                portraitImage.color = Color.white;
                bool isEquipped = (gachaManager.equippedCharacterId == character.characterId);

                if (cardBg != null)
                {
                    cardBg.color = isEquipped
                        ? new Color(0.28f, 0.24f, 0.14f, 1f)
                        : new Color(0.16f, 0.16f, 0.18f, 1f);
                }

                int affectionLvl = affectionManager != null
                    ? affectionManager.GetAffectionLevel(character.characterId)
                    : 1;
                string affectionTag = affectionLvl >= character.maxAffectionLevel
                    ? $"<color=#FF6B9E>{LocalizationManager.Get("affection.max")}</color>"
                    : $"<color=#FF6B9E>{LocalizationManager.Get("affection.level", affectionLvl)}</color>";

                string nameLine = isEquipped
                    ? $"<color=#FFD700>✦ {character.LocalizedName}</color>"
                    : character.LocalizedName;

                nameText.text = LocalizationManager.Get("gallery.card_subtitle", nameLine, level, affectionTag);

                if (cardBtn != null)
                {
                    CharacterData charCopy = character;
                    cardBtn.onClick.RemoveAllListeners();
                    cardBtn.onClick.AddListener(() =>
                    {
                        gachaManager.EquipCharacter(charCopy.characterId);
                        RefreshGallery();
                        UpdateCharacterDetails(charCopy);
                    });
                }
            }
        }
    }
    private void ShowEquippedOrDefaultDetails()
    {
        if (gachaManager != null && !string.IsNullOrEmpty(gachaManager.equippedCharacterId))
        {
            CharacterData equipped = gachaManager.allCharacters.Find(c => c != null && c.characterId == gachaManager.equippedCharacterId);
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