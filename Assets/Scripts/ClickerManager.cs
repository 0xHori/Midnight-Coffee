using UnityEngine;
using TMPro;
using UnityEngine.InputSystem; 

public class ClickerManager : MonoBehaviour
{
    // ВАЖНО: Заменили int на double, чтобы числа не переполнялись
    public double score = 0;           
    public double clickPower = 1;      
    public double upgradeCost = 10;    

    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI upgradeText; 
    public UnityEngine.UI.Button upgradeButtonComponent; 
    public GameObject floatingTextPrefab; // Ссылка на наш префаб
    public Transform canvasTransform;     // Ссылка на Canvas, чтобы текст не убегал за экран

    void Start()
    {
        LoadGame();
        UpdateUI();
    }

    public void AddScore()
    {
        score += clickPower; 
        
        // --- НОВЫЙ КОД ДЛЯ СПАВНА ТЕКСТА ---
        if (floatingTextPrefab != null && canvasTransform != null)
        {
            GameObject textObj = Instantiate(floatingTextPrefab, canvasTransform);
            textObj.GetComponent<TextMeshProUGUI>().text = "+" + FormatNumber(clickPower);
            
            // Ставим текст туда, где находится мышка (или палец)
            Vector3 mousePos = Mouse.current.position.ReadValue();
            
            // 1. Сдвигаем спавн чуть выше пальца, чтобы текст не появлялся прямо на кнопке
            mousePos.y += 100f; 
            
            // 2. Сильно увеличиваем разброс (150 пикселей в стороны)
            mousePos.x += Random.Range(-150f, 150f); 
            mousePos.y += Random.Range(-50f, 100f);
            
            textObj.transform.position = mousePos;
            
            // 3. Делаем так, чтобы текст рисовался ПОВЕРХ всех остальных элементов UI
            textObj.transform.SetAsLastSibling();
        }
        // -----------------------------------

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

    void UpdateUI()
    {
        // Теперь используем нашу новую функцию форматирования
        scoreText.text = "Счет: " + FormatNumber(score);
        
        if (upgradeText != null)
        {
            upgradeText.text = "Улучшение x2\nЦена: " + FormatNumber(upgradeCost);
        }

        if (upgradeButtonComponent != null)
        {
            upgradeButtonComponent.interactable = (score >= upgradeCost);
        }
    }

    // --- НОВАЯ ФУНКЦИЯ ФОРМАТИРОВАНИЯ ---
    string FormatNumber(double num)
    {
        // Если число меньше 1000, просто выводим его (без десятичных дробей)
        if (num < 1000) 
            return num.ToString("0"); 
        
        // Массив суффиксов для больших чисел
        string[] suffixes = { "", "K", "M", "B", "T", "Qa", "Qi", "Sx", "Sp", "Oc", "No", "Dc" };
        int suffixIndex = 0;
        
        // Делим число на 1000, пока оно не станет меньше 1000
        while (num >= 1000 && suffixIndex < suffixes.Length - 1)
        {
            num /= 1000;
            suffixIndex++;
        }
        
        // Выводим с одним знаком после запятой (например, 1.5K)
        return num.ToString("0.0") + suffixes[suffixIndex];
    }

    void SaveGame()
    {
        // ВАЖНО: Теперь сохраняем как строку (SetString), так как double нельзя сохранить через SetInt
        PlayerPrefs.SetString("SavedScore", score.ToString());
        PlayerPrefs.SetString("SavedClickPower", clickPower.ToString());
        PlayerPrefs.SetString("SavedUpgradeCost", upgradeCost.ToString());
        PlayerPrefs.Save(); 
    }

    void LoadGame()
    {
        // Загружаем строку и конвертируем обратно в double
        score = double.Parse(PlayerPrefs.GetString("SavedScore", "0"));
        clickPower = double.Parse(PlayerPrefs.GetString("SavedClickPower", "1"));
        upgradeCost = double.Parse(PlayerPrefs.GetString("SavedUpgradeCost", "10"));
    }
}