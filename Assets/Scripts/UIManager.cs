using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public ClickerManager clickerManager;

    [Header("Тексты")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI incomeText;
    public TextMeshProUGUI upgradeText;
    public TextMeshProUGUI flatUpgradeText; // Новое
    public TextMeshProUGUI autoText;
    public TextMeshProUGUI managerText;     // Новое
    public TextMeshProUGUI offlineText;

    [Header("Кнопки и Панели")]
    public GameObject offlinePanel;
    public UnityEngine.UI.Button upgradeButtonComponent;
    public UnityEngine.UI.Button flatUpgradeButtonComponent; // Новое
    public UnityEngine.UI.Button autoButtonComponent;
    public UnityEngine.UI.Button managerButtonComponent;     // Новое

    public void UpdateUI()
    {
        scoreText.text = "Счет: " + clickerManager.FormatNumber(clickerManager.score);
        incomeText.text = "Доход: " + clickerManager.FormatNumber(clickerManager.autoIncome) + " / сек";
        
        upgradeText.text = "Улучшение x2\nЦена: " + clickerManager.FormatNumber(clickerManager.upgradeCost);
        upgradeButtonComponent.interactable = (clickerManager.score >= clickerManager.upgradeCost);

        flatUpgradeText.text = "Клик +5\nЦена: " + clickerManager.FormatNumber(clickerManager.flatUpgradeCost);
        flatUpgradeButtonComponent.interactable = (clickerManager.score >= clickerManager.flatUpgradeCost);

        autoText.text = "Автокликер (+1/сек)\nЦена: " + clickerManager.FormatNumber(clickerManager.autoUpgradeCost);
        autoButtonComponent.interactable = (clickerManager.score >= clickerManager.autoUpgradeCost);

        managerText.text = "Менеджер (+10/сек)\nЦена: " + clickerManager.FormatNumber(clickerManager.managerCost);
        managerButtonComponent.interactable = (clickerManager.score >= clickerManager.managerCost);
    }

    public void ShowOfflinePanel(double earned, int hours, int minutes)
    {
        offlinePanel.SetActive(true);
        offlineText.text = $"Пока вас не было\n({hours}ч {minutes}м),\nвы заработали:\n{clickerManager.FormatNumber(earned)}";
    }
}