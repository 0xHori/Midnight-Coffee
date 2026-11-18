using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public ClickerManager clickerManager;
    public GachaUI gachaUI;

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

    [Header("Галерея")]
    public GameObject galleryPanel;
    public GalleryManager galleryManager;

    void OnEnable()
    {
        GameEvents.OnUpdateUI += UpdateUI;
        GameEvents.OnShowOfflinePanel += ShowOfflinePanel;
    }

    void OnDisable()
    {
        GameEvents.OnUpdateUI -= UpdateUI;
        GameEvents.OnShowOfflinePanel -= ShowOfflinePanel;
    }

    public void UpdateUI()
    {
        if (clickerManager == null) return;

        if (scoreText != null)
        {
            string multiplierString = clickerManager.totalCrystals > 0
                ? LocalizationManager.Get("ui.revenue_mult_suffix", clickerManager.PrestigeMultiplier.ToCompactString())
                : "";

            scoreText.text = LocalizationManager.Get("ui.revenue", clickerManager.score.ToCompactString(), multiplierString);
        }

        if (incomeText != null)
        {
            double passiveIncome = clickerManager.GetTotalAutoIncomePerSecond();
            float autoTaps = clickerManager.GetAutoTapsPerSecond();
            double avgCritBonus = 1.0 + clickerManager.GetTotalCritChance() * (clickerManager.GetTotalCritMultiplier() - 1.0);
            double autoTapIncome = autoTaps * clickerManager.GetTotalClickPower() * avgCritBonus;
            double totalShownIncome = passiveIncome + autoTapIncome;

            incomeText.text = LocalizationManager.Get("ui.income_per_sec", totalShownIncome.ToCompactString());
        }

        if (prestigeInfoText != null)
        {
            double potential = clickerManager.CalculatePotentialCrystals();
            prestigeInfoText.text = LocalizationManager.Get(
                "ui.prestige_info",
                potential.ToCompactString(),
                clickerManager.totalCrystals.ToCompactString()
            );

            if (prestigeButtonComponent != null)
                prestigeButtonComponent.interactable = (potential > 0);
        }
    }

    public void ShowOfflinePanel(double earned, int hours, int minutes)
    {
        if (offlinePanel != null)
            offlinePanel.SetActive(true);

        if (offlineText != null)
            offlineText.text = LocalizationManager.Get("ui.offline_earned", hours, minutes, earned.ToCompactString());
    }

    private void SetModalState(GameObject panel, bool isOpen)
    {
        if (panel != null) panel.SetActive(isOpen);
        if (dimBackground != null) dimBackground.SetActive(isOpen);
    }

    public void OpenPrestigePanel() => SetModalState(prestigePanel, true);
    public void ClosePrestigePanel() => SetModalState(prestigePanel, false);

    public void ConfirmPrestige()
    {
        if (clickerManager != null)
            clickerManager.DoPrestige();
        ClosePrestigePanel();
    }

    public void OpenSkillPanel() => SetModalState(skillTreePanel, true);
    public void CloseSkillPanel() => SetModalState(skillTreePanel, false);

    public void OpenGachaPanel()
    {
        if (gachaUI != null) gachaUI.Open();
    }

    public void CloseGachaPanel()
    {
        if (gachaUI != null) gachaUI.Close();
    }

    public void OpenGalleryPanel()
    {
        SetModalState(galleryPanel, true);
        if (galleryManager != null) galleryManager.OpenGallery();
    }

    public void CloseGalleryPanel() => SetModalState(galleryPanel, false);
}