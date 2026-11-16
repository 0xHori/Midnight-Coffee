using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CharacterCardUI : MonoBehaviour
{
    [Header("Ссылки префаба")]
    public Image backgroundImage;
    public Image portraitImage;
    public TextMeshProUGUI nameText;
    public Button cardButton;

    // Цвета карточки (HEX: #29292E обычный, #473D24 выбранный)
    private static readonly Color DefaultBgColor = new Color(0.16f, 0.16f, 0.18f, 1f);
    private static readonly Color EquippedBgColor = new Color(0.28f, 0.24f, 0.14f, 1f);

    private CharacterData _character;
    private Action<CharacterData> _onClicked;

    void Awake()
    {
        if (backgroundImage == null) backgroundImage = GetComponent<Image>();
        if (cardButton == null) cardButton = GetComponent<Button>();
        if (cardButton != null)
        {
            cardButton.onClick.RemoveAllListeners();
            cardButton.onClick.AddListener(() => _onClicked?.Invoke(_character));
        }
    }

    public void Bind(CharacterData character, Action<CharacterData> onClicked)
    {
        _character = character;
        _onClicked = onClicked;

        if (portraitImage != null && character != null)
        {
            portraitImage.sprite = character.portrait;
            portraitImage.preserveAspect = true;
        }
    }

    public void Refresh(int level, bool isEquipped, int affectionLvl)
    {
        if (_character == null) return;
        bool isOwned = level > 0;

        if (!isOwned)
        {
            if (backgroundImage != null) backgroundImage.color = DefaultBgColor;
            if (portraitImage != null) portraitImage.color = Color.black;
            if (nameText != null) nameText.text = "???";
            return;
        }

        if (portraitImage != null) portraitImage.color = Color.white;
        if (backgroundImage != null)
            backgroundImage.color = isEquipped ? EquippedBgColor : DefaultBgColor;

        if (nameText != null)
        {
            string affectionTag = affectionLvl >= _character.maxAffectionLevel
                ? $"<color=#FF6B9E>{LocalizationManager.Get("affection.max")}</color>"
                : $"<color=#FF6B9E>{LocalizationManager.Get("affection.level", affectionLvl)}</color>";

            string nameLine = isEquipped
                ? $"<color=#FFD700>✦ {_character.LocalizedName}</color>"
                : _character.LocalizedName;

            nameText.text = LocalizationManager.Get("gallery.card_subtitle", nameLine, level, affectionTag);
        }
    }
}