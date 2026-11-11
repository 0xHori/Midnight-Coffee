using UnityEngine;
using TMPro;
using UnityEngine.InputSystem; 

public class ClickerManager : MonoBehaviour
{
    public double score = 0;           
    public double clickPower = 1;      
    public double upgradeCost = 10;    

    // --- НОВЫЕ ПЕРЕМЕННЫЕ ДЛЯ АВТОКЛИКЕРА ---
    public double autoIncome = 0;        // Сколько очков в секунду мы получаем
    public double autoUpgradeCost = 50;  // Цена автокликера
    public TextMeshProUGUI autoText;     // Ссылка на текст кнопки автокликера
    public UnityEngine.UI.Button autoButtonComponent; // Ссылка на кнопку автокликера
    // ----------------------------------------

    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI upgradeText; 
    public UnityEngine.UI.Button upgradeButtonComponent; 
    public GameObject floatingTextPrefab; 
    public Transform canvasTransform;     

    void Start()
    {
        LoadGame();
        UpdateUI();
        
        // Запускаем таймер: каждую 1 секунду вызываем метод AutoIncome
        InvokeRepeating("AutoIncome", 1f, 1f); 
    }

    void AutoIncome()
    {
        if (autoIncome > 0)
        {
            score += autoIncome;
            UpdateUI();
            // Сохранять игру каждую секунду - это тяжеловато, поэтому сохраняем раз в 5-10 секунд или при выходе
            // Пока для простоты оставим без сохранения в этом методе, чтобы не грузить диск
        }
    }

    public void AddScore()
    {
        score += clickPower; 
        
        if (floatingTextPrefab != null && canvasTransform != null)
        {
            GameObject textObj = Instantiate(floatingTextPrefab, canvasTransform);
            textObj.GetComponent<TextMeshProUGUI>().text = "+" + FormatNumber(clickPower);
            
            Vector3 mousePos = Mouse.current.position.ReadValue();
            mousePos.y += 100f; 
            mousePos.x += Random.Range(-150f, 150f); 
            mousePos.y += Random.Range(-50f, 100f);
            
            textObj.transform.position = mousePos;
            textObj.transform.SetAsLastSibling();
        }

        UpdateUI();
        SaveGame();
    }

    public void BuyUpgrade()
    {
        if (score >= upgradeCost)
        {
            score -= upgradeCost; 
            clickPower *= 2; 
            upgradeCost *= 2; 
            
            UpdateUI(); 
            SaveGame();
        }
    }

    // --- НОВЫЙ МЕТОД ДЛЯ ПОКУПКИ АВТОКЛИКЕРА ---
    public void BuyAutoClicker()
    {
        if (score >= autoUpgradeCost)
        {
            score -= autoUpgradeCost;
            autoIncome += 1; // Добавляем 1 очко в секунду
            autoUpgradeCost *= 2.5; // Следующий автокликер будет стоить дороже (в 2.5 раза)
            
            UpdateUI();
            SaveGame();
        }
    }

    void UpdateUI()
    {
        scoreText.text = "Счет: " + FormatNumber(score);
        
        if (upgradeText != null)
            upgradeText.text = "Улучшение x2\nЦена: " + FormatNumber(upgradeCost);

        if (upgradeButtonComponent != null)
            upgradeButtonComponent.interactable = (score >= upgradeCost);

        // Обновляем текст и доступность кнопки автокликера
        if (autoText != null)
            autoText.text = "Автокликер (+1/сек)\nЦена: " + FormatNumber(autoUpgradeCost);

        if (autoButtonComponent != null)
            autoButtonComponent.interactable = (score >= autoUpgradeCost);
    }

    string FormatNumber(double num)
    {
        if (num < 1000) 
            return num.ToString("0"); 
        
        string[] suffixes = { "", "K", "M", "B", "T", "Qa", "Qi", "Sx", "Sp", "Oc", "No", "Dc" };
        int suffixIndex = 0;
        
        while (num >= 1000 && suffixIndex < suffixes.Length - 1)
        {
            num /= 1000;
            suffixIndex++;
        }
        
        return num.ToString("0.0") + suffixes[suffixIndex];
    }

    void SaveGame()
    {
        PlayerPrefs.SetString("SavedScore", score.ToString());
        PlayerPrefs.SetString("SavedClickPower", clickPower.ToString());
        PlayerPrefs.SetString("SavedUpgradeCost", upgradeCost.ToString());
        PlayerPrefs.SetString("SavedAutoIncome", autoIncome.ToString());       // Новое
        PlayerPrefs.SetString("SavedAutoUpgradeCost", autoUpgradeCost.ToString()); // Новое
        PlayerPrefs.Save(); 
    }

    void LoadGame()
    {
        score = double.Parse(PlayerPrefs.GetString("SavedScore", "0"));
        clickPower = double.Parse(PlayerPrefs.GetString("SavedClickPower", "1"));
        upgradeCost = double.Parse(PlayerPrefs.GetString("SavedUpgradeCost", "10"));
        autoIncome = double.Parse(PlayerPrefs.GetString("SavedAutoIncome", "0"));       // Новое
        autoUpgradeCost = double.Parse(PlayerPrefs.GetString("SavedAutoUpgradeCost", "50")); // Новое
    }
}