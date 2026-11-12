using UnityEngine;
using TMPro;

public class GalleryManager : MonoBehaviour
{
    public GachaManager gachaManager;
    public GameObject cardPrefab;
    public Transform contentContainer;

    public void OpenGallery()
    {
        RefreshGallery(); // Обновляем карточки при каждом открытии
    }

    public void RefreshGallery()
    {
        // Очищаем старые карточки (если они были)
        foreach (Transform child in contentContainer)
        {
            Destroy(child.gameObject);
        }

        // Проходимся по ВСЕМ персонажам в игре
        foreach (CharacterData character in gachaManager.allCharacters)
        {
            GameObject newCard = Instantiate(cardPrefab, contentContainer);
            
            // Настраиваем имя
            TextMeshProUGUI nameText = newCard.transform.Find("NameText").GetComponent<TextMeshProUGUI>();
            nameText.text = character.characterName;

            // Настраиваем картинку
            UnityEngine.UI.Image portraitImage = newCard.transform.Find("Portrait").GetComponent<UnityEngine.UI.Image>();
            portraitImage.sprite = character.portrait;

            // Проверяем: выбит ли этот персонаж?
            bool isOwned = gachaManager.ownedCharacterNames.Contains(character.characterName);

            if (!isOwned)
            {
                // Если не выбит — делаем силуэт (черный цвет)
                portraitImage.color = new Color(0, 0, 0, 1f);
                nameText.text = "???";
            }
            else
            {
                // Если выбит — показываем в цвете
                portraitImage.color = Color.white;
            }
        }
    }
}