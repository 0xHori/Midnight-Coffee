using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public ClickerManager clickerManager;
    public SkillTreeManager skillTreeManager;
    
    public GameObject skillTreePanel;

    [Header("Тексты")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI incomeText;
    public TextMeshProUGUI upgradeText;
    public TextMeshProUGUI flatUpgradeText; 
    public TextMeshProUGUI autoText;
    public TextMeshProUGUI managerText;     
    public TextMeshProUGUI offlineText;

    [Header("Престиж UI")]
    public GameObject prestigePanel;
    public GameObject dimBackground;
    public TextMeshProUGUI prestigeInfoText;
    public UnityEngine.UI.Button prestigeButtonComponent;

    [Header("Кнопки и Панели")]
    public GameObject offlinePanel;
    public UnityEngine.UI.Button upgradeButtonComponent;
    public UnityEngine.UI.Button flatUpgradeButtonComponent; 
    public UnityEngine.UI.Button autoButtonComponent;
    public UnityEngine.UI.Button managerButtonComponent;     

    [Header("Дерево Навыков")]
    public TextMeshProUGUI clickBoostText;
    public TextMeshProUGUI autoBoostText;
    public TextMeshProUGUI critBoostText;
    public UnityEngine.UI.Button clickBoostBtn;
    public UnityEngine.UI.Button autoBoostBtn;
    public UnityEngine.UI.Button critBoostBtn;

    public GameObject gachaPanel;
    public TextMeshProUGUI gachaResultText;
    public UnityEngine.UI.Image gachaResultImage;

    public void OpenGachaPanel() => gachaPanel.SetActive(true);
    public void CloseGachaPanel() => gachaPanel.SetActive(false);

    // --- ПОДПИСКА НА СОБЫТИЯ ---
    void OnEnable()
    {
        GameEvents.OnUpdateUI += UpdateUI;
        GameEvents.OnUpdateSkillUI += UpdateSkillUI;
        GameEvents.OnShowOfflinePanel += ShowOfflinePanel;
        GameEvents.OnGachaResult += ShowGachaResult; // <--- ДОБАВИТЬ
    }

    void OnDisable()
    {
        GameEvents.OnUpdateUI -= UpdateUI;
        GameEvents.OnUpdateSkillUI -= UpdateSkillUI;
        GameEvents.OnShowOfflinePanel -= ShowOfflinePanel;
        GameEvents.OnGachaResult -= ShowGachaResult; // <--- ДОБАВИТЬ
    }

    public void UpdateUI()
    {
        string multiplierString = clickerManager.totalCrystals > 0 ? $" (x{clickerManager.PrestigeMultiplier.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture)})" : "";
        double autoMult = (skillTreeManager != null) ? skillTreeManager.AutoMultiplier : 1.0;

        double actualAutoGain = 1 * clickerManager.PrestigeMultiplier * autoMult;
        double actualManagerGain = 10 * clickerManager.PrestigeMultiplier * autoMult;

        scoreText.text = "Счет: " + clickerManager.FormatNumber(clickerManager.score) + multiplierString;
        incomeText.text = "Доход: " + clickerManager.FormatNumber(clickerManager.autoIncome * clickerManager.PrestigeMultiplier * autoMult) + " / сек";
        
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
            prestigeInfoText.text = $"Перерождение\nВы получите: {potential} кристаллов\n(Каждый дает +10% к доходу)\nВсего: {clickerManager.totalCrystals}";
            prestigeButtonComponent.interactable = (potential > 0);
        }
    }

    public void UpdateSkillUI()
    {
        if (skillTreeManager == null) return;

        if (skillTreeManager.clickBoostLvl > 0) {
            clickBoostText.text = "Мощный палец: Куплено";
            clickBoostBtn.interactable = false;
        } else {
            clickBoostText.text = "Мощный палец (x1.5 клик)\nЦена: " + skillTreeManager.clickBoostCost + " кр.";
            clickBoostBtn.interactable = (clickerManager.totalCrystals >= skillTreeManager.clickBoostCost);
        }

        if (skillTreeManager.autoBoostLvl > 0) {
            autoBoostText.text = "Бизнес-хватка: Куплено";
            autoBoostBtn.interactable = false;
        } else {
            autoBoostText.text = "Бизнес-хватка (x1.5 автоклик)\nЦена: " + skillTreeManager.autoBoostCost + " кр.";
            autoBoostBtn.interactable = (clickerManager.totalCrystals >= skillTreeManager.autoBoostCost);
        }

        if (skillTreeManager.critBoostLvl > 0) {
            critBoostText.text = "Удача новичка: Куплено";
            critBoostBtn.interactable = false;
        } else {
            critBoostText.text = "Удача новичка (Крит 25%)\nЦена: " + skillTreeManager.critBoostCost + " кр.";
            critBoostBtn.interactable = (clickerManager.totalCrystals >= skillTreeManager.critBoostCost);
        }
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

    private void ShowGachaResult(string name, Sprite sprite, Color color)
    {
        // Включаем текст и картинку, если они были выключены
        gachaResultText.gameObject.SetActive(true);
        gachaResultImage.gameObject.SetActive(true);

        gachaResultText.text = name;
        gachaResultText.color = color;
        
        if (sprite != null)
        {
            gachaResultImage.sprite = sprite;
            gachaResultImage.color = Color.white; // Чтобы картинка не была прозрачной
        }
        else 
        {
            // Если картинки нет, прячем саму картинку (белый квадрат)
            gachaResultImage.gameObject.SetActive(false);
        }
    }
}