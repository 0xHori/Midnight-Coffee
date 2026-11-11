using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public ClickerManager clickerManager; // Ссылка на наши данные

    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI upgradeText;
    public TextMeshProUGUI autoText;
    public TextMeshProUGUI offlineText;
    public TextMeshProUGUI incomeText;
    public GameObject offlinePanel;
    
    public UnityEngine.UI.Button upgradeButtonComponent;
    public UnityEngine.UI.Button autoButtonComponent;

    public void UpdateUI()
    {
        scoreText.text = "Счет: " + clickerManager.FormatNumber(clickerManager.score);
        
        // --- НОВАЯ СТРОКА ---
        if (incomeText != null)
            incomeText.text = "Доход: " + clickerManager.FormatNumber(clickerManager.autoIncome) + " / сек";
        // --------------------

        upgradeText.text = "Улучшение x2\nЦена: " + clickerManager.FormatNumber(clickerManager.upgradeCost);
        upgradeButtonComponent.interactable = (clickerManager.score >= clickerManager.upgradeCost);

        autoText.text = "Автокликер (+1/сек)\nЦена: " + clickerManager.FormatNumber(clickerManager.autoUpgradeCost);
        autoButtonComponent.interactable = (clickerManager.score >= clickerManager.autoUpgradeCost);
    }

    public void ShowOfflinePanel(double earned, int hours, int minutes)
    {
        offlinePanel.SetActive(true);
        offlineText.text = $"Пока вас не было\n({hours}ч {minutes}м),\nвы заработали:\n{clickerManager.FormatNumber(earned)}";
    }
}