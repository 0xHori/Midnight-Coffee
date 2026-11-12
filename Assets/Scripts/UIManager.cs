using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public ClickerManager clickerManager;

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
    public GameObject dimBackground; // Добавь в переменные
    public TextMeshProUGUI prestigeInfoText;
    public UnityEngine.UI.Button prestigeButtonComponent;

    [Header("Кнопки и Панели")]
    public GameObject offlinePanel;
    public UnityEngine.UI.Button upgradeButtonComponent;
    public UnityEngine.UI.Button flatUpgradeButtonComponent; 
    public UnityEngine.UI.Button autoButtonComponent;
    public UnityEngine.UI.Button managerButtonComponent;     

    public SkillTreeManager skillTreeManager;
    
    [Header("Дерево Навыков")]
    public TextMeshProUGUI clickBoostText;
    public TextMeshProUGUI autoBoostText;
    public TextMeshProUGUI critBoostText;
    public UnityEngine.UI.Button clickBoostBtn;
    public UnityEngine.UI.Button autoBoostBtn;
    public UnityEngine.UI.Button critBoostBtn;

    public void UpdateUI()
    {
        // Показываем множитель, если есть кристаллы
        string multiplierString = clickerManager.totalCrystals > 0 ? $" (x{clickerManager.PrestigeMultiplier.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture)})" : "";
        
        // Берем множитель из Дерева Навыков (с защитой от пустого объекта)
        double autoMult = (skillTreeManager != null) ? skillTreeManager.AutoMultiplier : 1.0;

        // Показываем реальный доход с учетом престижа И навыка
        double actualAutoGain = 1 * clickerManager.PrestigeMultiplier * autoMult;
        double actualManagerGain = 10 * clickerManager.PrestigeMultiplier * autoMult;

        scoreText.text = "Счет: " + clickerManager.FormatNumber(clickerManager.score) + multiplierString;
        // Добавляем autoMult в общий доход
        incomeText.text = "Доход: " + clickerManager.FormatNumber(clickerManager.autoIncome * clickerManager.PrestigeMultiplier * autoMult) + " / сек";
        
        upgradeText.text = "Улучшение x2\nЦена: " + clickerManager.FormatNumber(clickerManager.upgradeCost);
        upgradeButtonComponent.interactable = (clickerManager.score >= clickerManager.upgradeCost);

        flatUpgradeText.text = "Клик +5\nЦена: " + clickerManager.FormatNumber(clickerManager.flatUpgradeCost);
        flatUpgradeButtonComponent.interactable = (clickerManager.score >= clickerManager.flatUpgradeCost);

        autoText.text = "Автокликер (+" + clickerManager.FormatNumber(actualAutoGain) + "/сек)\nЦена: " + clickerManager.FormatNumber(clickerManager.autoUpgradeCost);
        autoButtonComponent.interactable = (clickerManager.score >= clickerManager.autoUpgradeCost);

        managerText.text = "Менеджер (+" + clickerManager.FormatNumber(actualManagerGain) + "/сек)\nЦена: " + clickerManager.FormatNumber(clickerManager.managerCost);
        managerButtonComponent.interactable = (clickerManager.score >= clickerManager.managerCost);

        // Обновление текста престижа
        if (prestigeInfoText != null)
        {
            double potential = clickerManager.CalculatePotentialCrystals();
            prestigeInfoText.text = $"Перерождение\nВы получите: {potential} кристаллов\n(Каждый дает +10% к доходу)\nВсего: {clickerManager.totalCrystals}";
            prestigeButtonComponent.interactable = (potential > 0);
        }
    }

    public void ShowOfflinePanel(double earned, int hours, int minutes)
    {
        offlinePanel.SetActive(true);
        offlineText.text = $"Пока вас не было\n({hours}ч {minutes}м),\nвы заработали:\n{clickerManager.FormatNumber(earned)}";
    }

    // Методы для кнопок престижа
    public void OpenPrestigePanel() 
    { 
        prestigePanel.SetActive(true); 
        if (dimBackground != null) dimBackground.SetActive(true); // Включаем затемнение
    }
    public void ClosePrestigePanel() 
    { 
        prestigePanel.SetActive(false); 
        if (dimBackground != null) dimBackground.SetActive(false); // Выключаем
    }
    
    public void ConfirmPrestige() 
    { 
        clickerManager.DoPrestige(); 
        ClosePrestigePanel(); 
    }

    public void UpdateSkillUI()
    {
        if (skillTreeManager == null) return;

        // Мощный палец
        if (skillTreeManager.clickBoostLvl > 0) {
            clickBoostText.text = "Мощный палец: Куплено";
            clickBoostBtn.interactable = false;
        } else {
            clickBoostText.text = "Мощный палец (x1.5 клик)\nЦена: " + skillTreeManager.clickBoostCost + " кр.";
            clickBoostBtn.interactable = (clickerManager.totalCrystals >= skillTreeManager.clickBoostCost);
        }

        // Бизнес-хватка
        if (skillTreeManager.autoBoostLvl > 0) {
            autoBoostText.text = "Бизнес-хватка: Куплено";
            autoBoostBtn.interactable = false;
        } else {
            autoBoostText.text = "Бизнес-хватка (x1.5 автоклик)\nЦена: " + skillTreeManager.autoBoostCost + " кр.";
            autoBoostBtn.interactable = (clickerManager.totalCrystals >= skillTreeManager.autoBoostCost);
        }

        // Удача
        if (skillTreeManager.critBoostLvl > 0) {
            critBoostText.text = "Удача новичка: Куплено";
            critBoostBtn.interactable = false;
        } else {
            critBoostText.text = "Удача новичка (Крит 25%)\nЦена: " + skillTreeManager.critBoostCost + " кр.";
            critBoostBtn.interactable = (clickerManager.totalCrystals >= skillTreeManager.critBoostCost);
        }
    }

        // Открытие и закрытие Дерева Навыков
    public void OpenSkillPanel()
    {
        // Если используешь затемнение (DimBackground), включи его здесь:
        // if (dimBackground != null) dimBackground.SetActive(true); 
        
        // Находим панель по имени или добавь переменную public GameObject skillTreePanel;
        // Если добавил переменную вверху, используй её:
        skillTreePanel.SetActive(true);
        UpdateSkillUI(); // Обновляем кнопки при открытии
    }

    public void CloseSkillPanel()
    {
        skillTreePanel.SetActive(false);
        // if (dimBackground != null) dimBackground.SetActive(false);
    }
}