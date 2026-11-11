using UnityEngine;
using TMPro;

public class ClickerManager : MonoBehaviour
{
    public int score = 0;           
    public int clickPower = 1;      
    public int upgradeCost = 10;    

    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI upgradeText; 
    public UnityEngine.UI.Button upgradeButtonComponent; 

    // Start вызывается один раз при запуске игры
    void Start()
    {
        LoadGame(); // Загружаем сохраненные данные
        UpdateUI(); // Обновляем экран
    }

    public void AddScore()
    {
        score = score + clickPower; 
        UpdateUI();
        SaveGame(); // Сохраняем после каждого клика (для простоты)
    }

    public void BuyUpgrade()
    {
        if (score >= upgradeCost)
        {
            score = score - upgradeCost; 
            clickPower = clickPower * 2; 
            upgradeCost = upgradeCost * 2; 
            
            UpdateUI(); 
            SaveGame(); // Сохраняем после покупки
        }
    }

    void UpdateUI()
    {
        scoreText.text = "Счет: " + score.ToString();
        
        if (upgradeText != null)
        {
            upgradeText.text = "Улучшение x2\nЦена: " + upgradeCost.ToString();
        }

        if (upgradeButtonComponent != null)
        {
            upgradeButtonComponent.interactable = (score >= upgradeCost);
        }
    }

    // --- НОВЫЕ МЕТОДЫ ДЛЯ СОХРАНЕНИЯ ---

    void SaveGame()
    {
        // Сохраняем наши переменные под определенными именами
        PlayerPrefs.SetInt("SavedScore", score);
        PlayerPrefs.SetInt("SavedClickPower", clickPower);
        PlayerPrefs.SetInt("SavedUpgradeCost", upgradeCost);
        
        // Физически записываем данные на диск (Unity делает это не сразу, а по команде)
        PlayerPrefs.Save(); 
    }

    void LoadGame()
    {
        // Если данные существуют, загружаем их. Если нет (первый запуск) - оставляем по умолчанию
        score = PlayerPrefs.GetInt("SavedScore", 0);
        clickPower = PlayerPrefs.GetInt("SavedClickPower", 1);
        upgradeCost = PlayerPrefs.GetInt("SavedUpgradeCost", 10);
    }
}