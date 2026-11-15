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
                    ? "<color=#FF6B9E>♥ MAX</color>"
                    : $"<color=#FF6B9E>♥ Ур.{affectionLvl}</color>";

                string nameLine = isEquipped
                    ? $"<color=#FFD700>✦ {character.characterName}</color>"
                    : character.characterName;

                nameText.text = $"{nameLine}\nУр.{level}  •  {affectionTag}";

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
            ? $"<color=#FF6B9E>♥ Близость: MAX (+{affectionBonusPct:0.#}% к статам)</color>"
            : $"<color=#FF6B9E>♥ Близость: Ур. {affectionLvl}/{character.maxAffectionLevel} (+{affectionBonusPct:0.#}% к статам)</color>";

        string abilityLine = !string.IsNullOrEmpty(character.abilityDescription)
            ? $"\n<i><color=#CCCCCC>{character.abilityDescription}</color></i>"
            : "";

        detailsText.text = $"<b>{character.characterName}</b> (Ур. {level})  |  {affectionLine}\n" +
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