using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public ClickerManager clickerManager;
    public SkillTreeManager skillTreeManager;
    public GachaManager gachaManager;

    [Header("Главный экран - Тексты")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI incomeText;
    public TextMeshProUGUI upgradeText;
    public TextMeshProUGUI flatUpgradeText;
    public TextMeshProUGUI autoText;
    public TextMeshProUGUI managerText;
    public TextMeshProUGUI offlineText;

    [Header("Главный персонаж на экране (опционально)")]
    public Image mainCharacterImage;

    [Header("Главный экран - Кнопки")]
    public Button upgradeButtonComponent;
    public Button flatUpgradeButtonComponent;
    public Button autoButtonComponent;
    public Button managerButtonComponent;

    [Header("Престиж UI")]
    public GameObject prestigePanel;
    public GameObject dimBackground;
    public TextMeshProUGUI prestigeInfoText;
    public Button prestigeButtonComponent;

    [Header("Офлайн панель")]
    public GameObject offlinePanel;

    [Header("Дерево Навыков")]
    public GameObject skillTreePanel;

    [Header("Гача и Галерея")]
    public GameObject gachaPanel;
    public TextMeshProUGUI gachaResultText;
    public TextMeshProUGUI gachaCrystalsText;
    public Image gachaResultImage;
    public GameObject galleryPanel;
    public GalleryManager galleryManager;
    

    void OnEnable()
    {
        GameEvents.OnUpdateUI += UpdateUI;
        GameEvents.OnUpdateSkillUI += UpdateSkillUI;
        GameEvents.OnShowOfflinePanel += ShowOfflinePanel;
        GameEvents.OnGachaResult += ShowGachaResult;
    }

    void OnDisable()
    {
        GameEvents.OnUpdateUI -= UpdateUI;
        GameEvents.OnUpdateSkillUI -= UpdateSkillUI;
        GameEvents.OnShowOfflinePanel -= ShowOfflinePanel;
        GameEvents.OnGachaResult -= ShowGachaResult;
    }

    public void UpdateUI()
    {
        if (clickerManager == null) return;

        string multiplierString = clickerManager.totalCrystals > 0
            ? $" (x{clickerManager.FormatNumber(clickerManager.PrestigeMultiplier)})"
            : "";

        double totalAutoMult = clickerManager.GetTotalAutoMultiplier();
        double actualAutoGain = 1 * totalAutoMult;
        double actualManagerGain = 10 * totalAutoMult;

        scoreText.text = "Счет: " + clickerManager.FormatNumber(clickerManager.score) + multiplierString;
        incomeText.text = "Доход: " + clickerManager.FormatNumber(clickerManager.GetTotalAutoIncomePerSecond()) + " / сек";

        upgradeText.text = "Улучшение x2\nЦена: " + clickerManager.FormatNumber(clickerManager.upgradeCost);
        upgradeButtonComponent.interactable = (clickerManager.score >= clickerManager.upgradeCost);

        flatUpgradeText.text = "Клик +5\nЦена: " + clickerManager.FormatNumber(clickerManager.flatUpgradeCost);
        flatUpgradeButtonComponent.interactable = (clickerManager.score >= clickerManager.flatUpgradeCost);

        autoText.text = "Автокликер (+" + clickerManager.FormatNumber(actualAutoGain) + "/сек)\nЦена: " + clickerManager.FormatNumber(clickerManager.autoUpgradeCost);
        autoButtonComponent.interactable = (clickerManager.score >= clickerManager.autoUpgradeCost);

        managerText.text = "Менеджер (+" + clickerManager.FormatNumber(actualManagerGain) + "/сек)\nЦена: " + clickerManager.FormatNumber(clickerManager.managerCost);
        managerButtonComponent.interactable = (clickerManager.score >= clickerManager.managerCost);

        if (prestigeInfoText != null)
        {
            double potential = clickerManager.CalculatePotentialCrystals();
            prestigeInfoText.text = $"Перерождение\nВы получите: {clickerManager.FormatNumber(potential)} кристаллов\n(Каждый дает +10% к доходу)\nВсего: {clickerManager.FormatNumber(clickerManager.totalCrystals)}";
            prestigeButtonComponent.interactable = (potential > 0);
        }

        UpdateMainCharacterVisual();
    }

    private void UpdateMainCharacterVisual()
    {
        if (mainCharacterImage == null || gachaManager == null) return;
        if (string.IsNullOrEmpty(gachaManager.equippedCharacterId)) return;

        CharacterData equipped = gachaManager.allCharacters.Find(c => c != null && c.characterId == gachaManager.equippedCharacterId);
        if (equipped != null && equipped.portrait != null)
        {
            mainCharacterImage.sprite = equipped.portrait;
        }
    }

    public void UpdateSkillUI()
    {
        // Сюда подключим обновление динамического списка навыков на следующем шаге
    }

    public void ShowOfflinePanel(double earned, int hours, int minutes)
    {
        offlinePanel.SetActive(true);
        offlineText.text = $"Пока вас не было\n({hours}ч {minutes}м),\nвы заработали:\n{clickerManager.FormatNumber(earned)}";
    }

    public void OpenPrestigePanel() { prestigePanel.SetActive(true); if (dimBackground != null) dimBackground.SetActive(true); }
    public void ClosePrestigePanel() { prestigePanel.SetActive(false); if (dimBackground != null) dimBackground.SetActive(false); }
    public void ConfirmPrestige() { clickerManager.DoPrestige(); ClosePrestigePanel(); }
    public void OpenSkillPanel() { skillTreePanel.SetActive(true); UpdateSkillUI(); }
    public void CloseSkillPanel() { skillTreePanel.SetActive(false); }
    public void OpenGachaPanel()
    {
        gachaPanel.SetActive(true);

        if (gachaResultText != null)
        {
            gachaResultText.gameObject.SetActive(true);
            gachaResultText.text = "Испытай удачу!";
            gachaResultText.color = Color.white;
        }

        if (gachaResultImage != null)
        {
            gachaResultImage.gameObject.SetActive(false);
        }
        UpdateGachaCrystalsUI();
    }
    public void CloseGachaPanel() => gachaPanel.SetActive(false);

    private void ShowGachaResult(string name, Sprite sprite, Color color)
    {
        gachaResultText.gameObject.SetActive(true);
        gachaResultImage.gameObject.SetActive(true);

        gachaResultText.text = name;
        gachaResultText.color = color;

        if (sprite != null)
        {
            gachaResultImage.sprite = sprite;
            gachaResultImage.color = Color.white;
        }
        else
        {
            gachaResultImage.gameObject.SetActive(false);
        }
        UpdateGachaCrystalsUI();
    }

    public void OpenGalleryPanel()
    {
        galleryPanel.SetActive(true);
        galleryManager.OpenGallery();
    }

    public void CloseGalleryPanel() => galleryPanel.SetActive(false);

    public void UpdateGachaCrystalsUI()
    {
        if (gachaCrystalsText != null && clickerManager != null)
        {
            gachaCrystalsText.text = $"Кристаллы: {clickerManager.FormatNumber(clickerManager.totalCrystals)}";
        }
    }
}