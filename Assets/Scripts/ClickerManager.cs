using UnityEngine;
using TMPro;
using UnityEngine.InputSystem; 
using System; // Добавили для работы с DateTime

public class ClickerManager : MonoBehaviour
{
    public double score = 0;           
    public double clickPower = 1;      
    public double upgradeCost = 10;    

    public double autoIncome = 0;        
    public double autoUpgradeCost = 50;  
    public TextMeshProUGUI autoText;     
    public UnityEngine.UI.Button autoButtonComponent; 

    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI upgradeText; 
    public UnityEngine.UI.Button upgradeButtonComponent; 
    public GameObject floatingTextPrefab; 
    public Transform canvasTransform;     

    // --- НОВЫЕ ПЕРЕМЕННЫЕ ДЛЯ ОФФЛАЙН-ДОХОДА ---
    public GameObject offlinePanel;       // Панель (окно), которое покажет доход
    public TextMeshProUGUI offlineText;   // Текст внутри этой панели
    private double maxOfflineHours = 8;   // Максимум часов, за которые начислим доход
    // --------------------------------------------

    void Start()
    {
        LoadGame();
        CalculateOfflineIncome(); // Считаем оффлайн-доход при старте
        UpdateUI();
        
        InvokeRepeating("AutoIncome", 1f, 1f); 
    }

    void AutoIncome()
    {
        if (autoIncome > 0)
        {
            score += autoIncome;
            UpdateUI();
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
            mousePos.x += UnityEngine.Random.Range(-150f, 150f); 
            mousePos.y += UnityEngine.Random.Range(-50f, 100f);
            
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

    public void BuyAutoClicker()
    {
        if (score >= autoUpgradeCost)
        {
            score -= autoUpgradeCost;
            autoIncome += 1; 
            autoUpgradeCost *= 2.5; 
            
            UpdateUI();
            SaveGame();
        }
    }

    // --- НОВЫЙ МЕТОД: РАСЧЕТ ОФФЛАЙН-ДОХОДА ---
    void CalculateOfflineIncome()
    {
        if (PlayerPrefs.HasKey("LastExitTime") && autoIncome > 0)
        {
            // Загружаем время выхода
            string lastTimeString = PlayerPrefs.GetString("LastExitTime");
            DateTime lastTime = DateTime.Parse(lastTimeString);
            
            // Считаем, сколько прошло времени (используем UtcNow, чтобы не зависеть от часового пояса)
            TimeSpan timeAway = DateTime.UtcNow - lastTime;
            double secondsAway = timeAway.TotalSeconds;

            // ОГРАНИЧЕНИЕ: максимум 8 часов (8 * 60 * 60 секунд)
            double maxSeconds = maxOfflineHours * 60 * 60;
            if (secondsAway > maxSeconds) 
            {
                secondsAway = maxSeconds;
            }

            // Если игрок отсутствовал больше 1 минуты, начисляем доход и показываем окно
            if (secondsAway > 60)
            {
                double earned = secondsAway * autoIncome;
                score += earned;

                if (offlinePanel != null && offlineText != null)
                {
                    offlinePanel.SetActive(true); // Показываем окно
                    
                    int hours = (int)(secondsAway / 3600);
                    int minutes = (int)((secondsAway % 3600) / 60);
                    
                    offlineText.text = $"Пока вас не было\n({hours}ч {minutes}м),\nвы заработали:\n{FormatNumber(earned)}";
                }
            }
        }
    }

    // Метод закрытия игры (для ПК)
    void OnApplicationQuit()
    {
        PlayerPrefs.SetString("LastExitTime", DateTime.UtcNow.ToString());
        PlayerPrefs.Save();
    }

    // Метод сворачивания игры (для телефонов, когда игрок нажимает кнопку "Домой")
    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            PlayerPrefs.SetString("LastExitTime", DateTime.UtcNow.ToString());
            PlayerPrefs.Save();
        }
    }
    // ------------------------------------------

    void UpdateUI()
    {
        scoreText.text = "Счет: " + FormatNumber(score);
        
        if (upgradeText != null)
            upgradeText.text = "Улучшение x2\nЦена: " + FormatNumber(upgradeCost);

        if (upgradeButtonComponent != null)
            upgradeButtonComponent.interactable = (score >= upgradeCost);

        if (autoText != null)
            autoText.text = "Автокликер (+1/сек)\nЦена: " + FormatNumber(autoUpgradeCost);

        if (autoButtonComponent != null)
            autoButtonComponent.interactable = (score >= autoUpgradeCost);
    }

    string FormatNumber(double num)
    {
        if (num < 1000) return num.ToString("0"); 
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
        PlayerPrefs.SetString("SavedAutoIncome", autoIncome.ToString());
        PlayerPrefs.SetString("SavedAutoUpgradeCost", autoUpgradeCost.ToString());
        PlayerPrefs.Save(); 
    }

    void LoadGame()
    {
        score = double.Parse(PlayerPrefs.GetString("SavedScore", "0"));
        clickPower = double.Parse(PlayerPrefs.GetString("SavedClickPower", "1"));
        upgradeCost = double.Parse(PlayerPrefs.GetString("SavedUpgradeCost", "10"));
        autoIncome = double.Parse(PlayerPrefs.GetString("SavedAutoIncome", "0"));
        autoUpgradeCost = double.Parse(PlayerPrefs.GetString("SavedAutoUpgradeCost", "50"));
    }
}