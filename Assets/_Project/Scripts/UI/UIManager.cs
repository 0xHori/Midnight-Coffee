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

    public void OpenPrestigePanel()
    {
        if (prestigePanel != null) prestigePanel.SetActive(true);
        if (dimBackground != null) dimBackground.SetActive(true);
    }

    public void ClosePrestigePanel()
    {
        if (prestigePanel != null) prestigePanel.SetActive(false);
        if (dimBackground != null) dimBackground.SetActive(false);
    }

    public void ConfirmPrestige()
    {
        if (clickerManager != null)
            clickerManager.DoPrestige();
        ClosePrestigePanel();
    }

    public void OpenSkillPanel()
    {
        if (skillTreePanel != null) skillTreePanel.SetActive(true);
        if (dimBackground != null) dimBackground.SetActive(true);
    }

    public void CloseSkillPanel()
    {
        if (skillTreePanel != null) skillTreePanel.SetActive(false);
        if (dimBackground != null) dimBackground.SetActive(false);
    }

    public void OpenGachaPanel()
    {
        if (gachaPanel != null) gachaPanel.SetActive(true);
        if (dimBackground != null) dimBackground.SetActive(true);

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

    public void CloseGachaPanel()
    {
        if (gachaPanel != null) gachaPanel.SetActive(false);
        if (dimBackground != null) dimBackground.SetActive(false);
    }

    private void ShowGachaResult(string name, Sprite sprite, Color color)
    {
        if (gachaResultText != null)
        {
            gachaResultText.gameObject.SetActive(true);
            gachaResultText.text = name;
            gachaResultText.color = color;
        }

        if (gachaResultImage != null)
        {
            if (sprite != null)
            {
                gachaResultImage.gameObject.SetActive(true);
                gachaResultImage.sprite = sprite;
                gachaResultImage.color = Color.white;
            }
            else
            {
                gachaResultImage.gameObject.SetActive(false);
            }
        }
        UpdateGachaCrystalsUI();
    }

    public void OpenGalleryPanel()
    {
        if (galleryPanel != null) galleryPanel.SetActive(true);
        if (dimBackground != null) dimBackground.SetActive(true);
        if (galleryManager != null) galleryManager.OpenGallery();
    }

    public void CloseGalleryPanel()
    {
        if (galleryPanel != null) galleryPanel.SetActive(false);
        if (dimBackground != null) dimBackground.SetActive(false);
    }

    public void UpdateGachaCrystalsUI()
    {
        if (gachaCrystalsText != null && clickerManager != null)
        {
            gachaCrystalsText.text = LocalizationManager.Get("ui.gacha_reputation", clickerManager.totalCrystals.ToCompactString());
        }
    }
}