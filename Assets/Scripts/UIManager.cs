using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public ClickerManager clickerManager;

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

    public void UpdateUI()
    {
        // Показываем множитель, если есть кристаллы
        string multiplierString = clickerManager.totalCrystals > 0 ? $" (x{clickerManager.PrestigeMultiplier.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture)})" : "";
        // Показываем реальный доход с учетом престижа
        double actualAutoGain = 1 * clickerManager.PrestigeMultiplier;
        double actualManagerGain = 10 * clickerManager.PrestigeMultiplier;


        scoreText.text = "Счет: " + clickerManager.FormatNumber(clickerManager.score) + multiplierString;
        incomeText.text = "Доход: " + clickerManager.FormatNumber(clickerManager.autoIncome * clickerManager.PrestigeMultiplier) + " / сек";
        
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
}