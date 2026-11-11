using UnityEngine;
using TMPro;
using UnityEngine.InputSystem; 

public class ClickerManager : MonoBehaviour
{
    [Header("Звуки")]
    public AudioSource audioSource;
    public AudioClip clickSound;
    public AudioClip critSound;
    public AudioClip buySound;

    [Header("Данные игрока")]
    public double score = 0;           
    public double clickPower = 1;      
    public double flatClickPower = 0;  
    public double critChance = 0.1;    
    public double critMultiplier = 10; 
    public double autoIncome = 0;        

    [Header("Цены улучшений")]
    public double upgradeCost = 10;       
    public double flatUpgradeCost = 100;  
    public double autoUpgradeCost = 50;   
    public double managerCost = 500;      

    [Header("Престиж")]
    public double totalCrystals = 0; // Всего кристаллов
    public double prestigeThreshold = 10000; // Сколько нужно очков для перерождения (для теста сделай 1000)
    
    // Вычисляемый множитель (1 + (кристаллы * 0.1)) -> каждый кристалл дает +10%
    public double PrestigeMultiplier => 1 + (totalCrystals * 0.1); 

    public UIManager uiManager;
    public SaveManager saveManager;
    public GameObject floatingTextPrefab; 
    public Transform canvasTransform;     

    void Start()
    {
        saveManager.LoadGame();
        saveManager.CalculateOfflineIncome(); 
        uiManager.UpdateUI();
        InvokeRepeating("AutoIncome", 1f, 1f); 
    }

    void AutoIncome()
    {
        if (autoIncome > 0)
        {
            score += autoIncome * PrestigeMultiplier; // Учитываем престиж
            uiManager.UpdateUI();
        }
    }

    public void AddScore()
    {
        double baseClick = clickPower + flatClickPower; 
        double earnedPoints = baseClick * PrestigeMultiplier; // Учитываем престиж
        bool isCrit = false;

        if (UnityEngine.Random.value <= critChance)
        {
            earnedPoints *= critMultiplier;
            isCrit = true;
            if (audioSource != null && critSound != null) { audioSource.pitch = 1f; audioSource.PlayOneShot(critSound); }
        }
        else
        {
            if (audioSource != null && clickSound != null) { audioSource.pitch = UnityEngine.Random.Range(0.9f, 1.1f); audioSource.PlayOneShot(clickSound); }
        }

        score += earnedPoints; 
        
        if (floatingTextPrefab != null && canvasTransform != null)
        {
            GameObject textObj = Instantiate(floatingTextPrefab, canvasTransform);
            TextMeshProUGUI textComponent = textObj.GetComponent<TextMeshProUGUI>();
            
            if (isCrit) {
                textComponent.text = "КРИТ! +" + FormatNumber(earnedPoints);
                textComponent.color = Color.red; 
                textObj.transform.localScale = new Vector3(1.5f, 1.5f, 1f); 
            } else {
                textComponent.text = "+" + FormatNumber(earnedPoints);
            }
            
            Vector2 pointerPos = Pointer.current.position.ReadValue();
            Vector3 screenPos = new Vector3(pointerPos.x, pointerPos.y, 0);
            screenPos.y += 100f; 
            screenPos.x += UnityEngine.Random.Range(-150f, 150f); 
            screenPos.y += UnityEngine.Random.Range(-50f, 100f);
            textObj.transform.position = screenPos;
            textObj.transform.SetAsLastSibling();
        }

        uiManager.UpdateUI();
        saveManager.SaveGame();
    }

    public void BuyUpgrade() // x2
    {
        if (score >= upgradeCost) { score -= upgradeCost; clickPower *= 2; upgradeCost *= 2.5; PlayBuySound(); uiManager.UpdateUI(); saveManager.SaveGame(); }
    }

    public void BuyFlatUpgrade() // +5
    {
        if (score >= flatUpgradeCost) { score -= flatUpgradeCost; flatClickPower += 5; flatUpgradeCost *= 3; PlayBuySound(); uiManager.UpdateUI(); saveManager.SaveGame(); }
    }

    public void BuyAutoClicker() // +1/сек
    {
        if (score >= autoUpgradeCost) { score -= autoUpgradeCost; autoIncome += 1; autoUpgradeCost *= 2.5; PlayBuySound(); uiManager.UpdateUI(); saveManager.SaveGame(); }
    }

    public void BuyManager() // +10/сек
    {
        if (score >= managerCost) { score -= managerCost; autoIncome += 10; managerCost *= 4; PlayBuySound(); uiManager.UpdateUI(); saveManager.SaveGame(); }
    }

    // --- НОВЫЕ МЕТОДЫ ДЛЯ ПРЕСТИЖА ---

    // Сколько кристаллов игрок получит прямо сейчас
    public double CalculatePotentialCrystals()
    {
        if (score < prestigeThreshold) return 0;
        return Mathf.Floor((float)(score / prestigeThreshold)); 
    }

    // Само перерождение
    public void DoPrestige()
    {
        double crystalsToAdd = CalculatePotentialCrystals();
        if (crystalsToAdd <= 0) return; // Защита от случайного нажатия

        totalCrystals += crystalsToAdd;

        // СБРОС ПРОГРЕССА
        score = 0;           
        clickPower = 1;      
        flatClickPower = 0;  
        autoIncome = 0;        
        upgradeCost = 10;       
        flatUpgradeCost = 100;  
        autoUpgradeCost = 50;   
        managerCost = 500;      

        PlayBuySound();
        saveManager.SaveGame();
        uiManager.UpdateUI();
    }

    void PlayBuySound() {
        if (audioSource != null && buySound != null) { audioSource.pitch = 1f; audioSource.PlayOneShot(buySound); }
    }

    public string FormatNumber(double num)
    {
        if (num < 1000) return num.ToString("0", System.Globalization.CultureInfo.InvariantCulture); 
        string[] suffixes = { "", "K", "M", "B", "T", "Qa", "Qi", "Sx", "Sp", "Oc", "No", "Dc" };
        int suffixIndex = 0;
        while (num >= 1000 && suffixIndex < suffixes.Length - 1) { num /= 1000; suffixIndex++; }
        return num.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture) + suffixes[suffixIndex];
    }
}