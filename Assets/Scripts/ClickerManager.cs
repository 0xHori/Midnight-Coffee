using UnityEngine;
using TMPro;
using UnityEngine.InputSystem; 

public class ClickerManager : MonoBehaviour
{
    public SkillTreeManager skillTreeManager;
    public SaveManager saveManager; // Оставляем только для загрузки в Start()
    
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
    public double totalCrystals = 0; 
    public double prestigeThreshold = 10000; 
    public double PrestigeMultiplier => 1 + (totalCrystals * 0.1); 

    public GameObject floatingTextPrefab; 
    public Transform canvasTransform;     

    void Start()
    {
        saveManager.LoadGame();
        saveManager.CalculateOfflineIncome(); 
        GameEvents.UpdateUI(); // Вызываем событие вместо uiManager
        InvokeRepeating("AutoIncome", 1f, 1f); 
    }

    void AutoIncome()
    {
        if (autoIncome > 0)
        {
            score += autoIncome * PrestigeMultiplier * skillTreeManager.AutoMultiplier;
            GameEvents.UpdateUI(); // Событие
        }
    }

    public void AddScore()
    {
        double baseClick = (clickPower + flatClickPower) * skillTreeManager.ClickMultiplier; 
        double earnedPoints = baseClick * PrestigeMultiplier; 
        bool isCrit = false;

        if (UnityEngine.Random.value <= skillTreeManager.CritChance)
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

        GameEvents.UpdateUI(); // Событие
        GameEvents.SaveGame(); // Событие
    }

    public void BuyUpgrade() // x2
    {
        if (score >= upgradeCost) { score -= upgradeCost; clickPower *= 2; upgradeCost *= 2.5; PlayBuySound(); GameEvents.UpdateUI(); GameEvents.SaveGame(); }
    }

    public void BuyFlatUpgrade() // +5
    {
        if (score >= flatUpgradeCost) { score -= flatUpgradeCost; flatClickPower += 5; flatUpgradeCost *= 3; PlayBuySound(); GameEvents.UpdateUI(); GameEvents.SaveGame(); }
    }

    public void BuyAutoClicker() // +1/сек
    {
        if (score >= autoUpgradeCost) { score -= autoUpgradeCost; autoIncome += 1; autoUpgradeCost *= 2.5; PlayBuySound(); GameEvents.UpdateUI(); GameEvents.SaveGame(); }
    }

    public void BuyManager() // +10/сек
    {
        if (score >= managerCost) { score -= managerCost; autoIncome += 10; managerCost *= 4; PlayBuySound(); GameEvents.UpdateUI(); GameEvents.SaveGame(); }
    }

    public double CalculatePotentialCrystals()
    {
        if (score < prestigeThreshold) return 0;
        return Mathf.Floor((float)(score / prestigeThreshold)); 
    }

    public void DoPrestige()
    {
        double crystalsToAdd = CalculatePotentialCrystals();
        if (crystalsToAdd <= 0) return;

        totalCrystals += crystalsToAdd;

        if (audioSource != null && buySound != null) { audioSource.pitch = 0.8f; audioSource.PlayOneShot(buySound); }

        if (floatingTextPrefab != null && canvasTransform != null)
        {
            GameObject textObj = Instantiate(floatingTextPrefab, canvasTransform);
            TextMeshProUGUI textComponent = textObj.GetComponent<TextMeshProUGUI>();
            textComponent.text = "ПРЕСТИЖ! +" + crystalsToAdd + " КРИСТАЛЛОВ";
            textComponent.color = new Color(0.8f, 0.2f, 1f); 
            textObj.transform.localScale = new Vector3(2.5f, 2.5f, 1f); 
            textObj.transform.position = new Vector3(Screen.width / 2f, Screen.height / 2f, 0);
            textObj.transform.SetAsLastSibling();
        }

        score = 0; clickPower = 1; flatClickPower = 0; autoIncome = 0;        
        upgradeCost = 10; flatUpgradeCost = 100; autoUpgradeCost = 50; managerCost = 500;      
        prestigeThreshold *= 2.5; 

        GameEvents.SaveGame();
        GameEvents.UpdateUI();
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