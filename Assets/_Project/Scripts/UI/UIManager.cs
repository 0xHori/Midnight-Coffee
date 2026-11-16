using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public ClickerManager clickerManager;
    public GachaManager gachaManager;

    [Header("Главный экран - Тексты")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI incomeText;
    public TextMeshProUGUI offlineText;

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
        GameEvents.OnShowOfflinePanel += ShowOfflinePanel;
        GameEvents.OnGachaResult += ShowGachaResult;
    }

    void OnDisable()
    {
        GameEvents.OnUpdateUI -= UpdateUI;
        GameEvents.OnShowOfflinePanel -= ShowOfflinePanel;
        GameEvents.OnGachaResult -= ShowGachaResult;
    }

    public void UpdateUI()
    {
        if (clickerManager == null) return;

        string multiplierString = clickerManager.totalCrystals > 0
            ? $" (x{clickerManager.PrestigeMultiplier.ToCompactString()})"
            : "";

        scoreText.text = LocalizationManager.Get("ui.revenue", clickerManager.score.ToCompactString(), multiplierString);
        double passiveIncome = clickerManager.GetTotalAutoIncomePerSecond();
        float autoTaps = clickerManager.GetAutoTapsPerSecond();
        double avgCritBonus = 1.0 + clickerManager.GetTotalCritChance() * (clickerManager.GetTotalCritMultiplier() - 1.0);
        double autoTapIncome = autoTaps * clickerManager.GetTotalClickPower() * avgCritBonus;
        double totalShownIncome = passiveIncome + autoTapIncome;

        incomeText.text = LocalizationManager.Get("ui.income_per_sec", totalShownIncome.ToCompactString());


        if (prestigeInfoText != null)
        {
            double potential = clickerManager.CalculatePotentialCrystals();
            prestigeInfoText.text = LocalizationManager.Get(
                "ui.prestige_info",
                potential.ToCompactString(),
                clickerManager.totalCrystals.ToCompactString()
            );
            prestigeButtonComponent.interactable = (potential > 0);
        }
    }

    public void ShowOfflinePanel(double earned, int hours, int minutes)
    {
        offlinePanel.SetActive(true);
        offlineText.text = LocalizationManager.Get("ui.offline_earned", hours, minutes, earned.ToCompactString());   
    }

    public void OpenPrestigePanel() { prestigePanel.SetActive(true); if (dimBackground != null) dimBackground.SetActive(true); }
    public void ClosePrestigePanel() { prestigePanel.SetActive(false); if (dimBackground != null) dimBackground.SetActive(false); }
    public void ConfirmPrestige() { clickerManager.DoPrestige(); ClosePrestigePanel(); }
    public void OpenSkillPanel() { skillTreePanel.SetActive(true); }
    public void CloseSkillPanel() { skillTreePanel.SetActive(false); }

    public void OpenGachaPanel()
    {
        gachaPanel.SetActive(true);

        if (gachaResultText != null)
        {
            gachaResultText.gameObject.SetActive(true);
            gachaResultText.text = LocalizationManager.Get("ui.gacha_prompt");
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
            gachaCrystalsText.text = LocalizationManager.Get("ui.gacha_reputation", clickerManager.totalCrystals.ToCompactString());   
        }
    }
}