using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GalleryManager : MonoBehaviour
{
    public GachaManager gachaManager;
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
            portraitImage.sprite = character.portrait;
            portraitImage.preserveAspect = true;

            int level = gachaManager.GetCharacterLevel(character.characterId);
            bool isOwned = level > 0;

            Button cardBtn = newCard.GetComponent<Button>();

            if (!isOwned)
            {
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
                nameText.text = isEquipped
                    ? $"<color=#FFD700>[АКТИВНА]</color>\n{character.characterName} [Ур.{level}]"
                    : $"{character.characterName} [Ур.{level}]";

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
            detailsText.text = "Нажмите на открытого персонажа,\nчтобы выбрать его и увидеть бонусы";
            return;
        }

        int level = gachaManager.GetCharacterLevel(character.characterId);
        if (level <= 0)
        {
            detailsText.text = $"<b>???</b> — персонаж ещё не открыт.\nВыбейте в Гаче, чтобы узнать бонусы!";
            return;
        }

        double passiveBonus = character.GetPassiveBonus(level) * 100.0;
        double activeBonus = character.GetActiveBonus(level) * 100.0;

        string passiveDesc = GetStatDescription(character.passiveStat);
        string activeDesc = GetStatDescription(character.activeStat);

        string abilityLine = !string.IsNullOrEmpty(character.abilityDescription)
            ? $"\n<i><color=#CCCCCC>{character.abilityDescription}</color></i>"
            : "";

        detailsText.text = $"<b>{character.characterName}</b> (Ур. {level})\n" +
                           $"<color=#A3E635>Пассивно: +{passiveBonus:0.#}% {passiveDesc}</color>  |  " +
                           $"<color=#38BDF8>Активна: +{activeBonus:0.#}% {activeDesc}</color>" +
                           abilityLine;
    }

    private string GetStatDescription(StatType type)
    {
        switch (type)
        {
            case StatType.ClickMultiplier: return "к силе клика";
            case StatType.AutoMultiplier: return "к автодоходу";
            case StatType.CritChance: return "к шансу крита";
            // case StatType.CritMultiplier: return "к силе крита";
            // case StatType.PrestigeGain: return "к кристаллам престижа";
            default: return "к доходу";
        }
    }
}