using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GalleryManager : MonoBehaviour
{
    public GachaManager gachaManager;
    public GameObject cardPrefab;
    public Transform contentContainer;

    public void OpenGallery()
    {
        RefreshGallery();
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
            portraitImage.preserveAspect = true; // Чтобы карточки в галерее тоже не плющило

            int level = gachaManager.GetCharacterLevel(character.characterId);
            bool isOwned = level > 0;

            if (!isOwned)
            {
                portraitImage.color = new Color(0, 0, 0, 1f);
                nameText.text = "???";
            }
            else
            {
                portraitImage.color = Color.white;
                bool isEquipped = (gachaManager.equippedCharacterId == character.characterId);
                nameText.text = isEquipped
                    ? $"<color=#FFD700>[АКТИВНА]</color>\n{character.characterName} [Ур.{level}]"
                    : $"{character.characterName} [Ур.{level}]";

                Button cardBtn = newCard.GetComponent<Button>();
                if (cardBtn != null)
                {
                    string idCopy = character.characterId;
                    cardBtn.onClick.RemoveAllListeners();
                    cardBtn.onClick.AddListener(() =>
                    {
                        gachaManager.EquipCharacter(idCopy);
                        RefreshGallery();
                    });
                }
            }
        }
    }
}