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

    public double score = 0;           
    public double clickPower = 1;      
    public double upgradeCost = 10;    
    public double critChance = 0.1;    
    public double critMultiplier = 10; 

    public double autoIncome = 0;        
    public double autoUpgradeCost = 50;  

    // Ссылки на другие менеджеры
    public UIManager uiManager;
    public SaveManager saveManager;

    // Для спавна текста
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
            score += autoIncome;
            uiManager.UpdateUI();
        }
    }

    public void AddScore()
    {
        double earnedPoints = clickPower; 
        bool isCrit = false;

        if (UnityEngine.Random.value <= critChance)
        {
            earnedPoints *= critMultiplier;
            isCrit = true;

            // Воспроизводим звук крита
            if (audioSource != null && critSound != null)
            {
                audioSource.pitch = 1f; 
                audioSource.PlayOneShot(critSound);
            }
        }
        else
        {
            // Воспроизводим звук обычного клика со случайным питчем
            if (audioSource != null && clickSound != null)
            {
                audioSource.pitch = UnityEngine.Random.Range(0.9f, 1.1f); 
                audioSource.PlayOneShot(clickSound);
            }
        }

        score += earnedPoints; 
        
        if (floatingTextPrefab != null && canvasTransform != null)
        {
            GameObject textObj = Instantiate(floatingTextPrefab, canvasTransform);
            TextMeshProUGUI textComponent = textObj.GetComponent<TextMeshProUGUI>();
            
            if (isCrit)
            {
                textComponent.text = "КРИТ! +" + FormatNumber(earnedPoints);
                textComponent.color = Color.red; 
                textObj.transform.localScale = new Vector3(1.5f, 1.5f, 1f); 
            }
            else
            {
                textComponent.text = "+" + FormatNumber(earnedPoints);
            }
            
            // --- ИСПРАВЛЕНИЕ: Берем текущую позицию указателя (мыши или пальца) ---
            Vector2 pointerPos = Pointer.current.position.ReadValue();
            Vector3 screenPos = new Vector3(pointerPos.x, pointerPos.y, 0);
            
            screenPos.y += 100f; 
            screenPos.x += UnityEngine.Random.Range(-150f, 150f); 
            screenPos.y += UnityEngine.Random.Range(-50f, 100f);
            
            textObj.transform.position = screenPos;
            textObj.transform.SetAsLastSibling();
        }

        // Теперь этот код гарантированно выполнится!
        uiManager.UpdateUI();
        saveManager.SaveGame();
    }

    public void BuyUpgrade()
    {
        if (score >= upgradeCost)
        {
            score -= upgradeCost; 
            clickPower *= 2; 
            upgradeCost *= 2; 
            
            // Воспроизводим звук покупки
            if (audioSource != null && buySound != null)
            {
                audioSource.pitch = 1f;
                audioSource.PlayOneShot(buySound);
            }

            uiManager.UpdateUI(); 
            saveManager.SaveGame();
        }
    }

    public void BuyAutoClicker()
    {
        if (score >= autoUpgradeCost)
        {
            score -= autoUpgradeCost;
            autoIncome += 1; 
            autoUpgradeCost *= 2.5; 
            
            // Воспроизводим звук покупки
            if (audioSource != null && buySound != null)
            {
                audioSource.pitch = 1f;
                audioSource.PlayOneShot(buySound);
            }

            uiManager.UpdateUI();
            saveManager.SaveGame();
        }
    }

    // Сделали метод публичным (добавили слово public)
    public string FormatNumber(double num)
    {
        if (num < 1000) 
            return num.ToString("0", System.Globalization.CultureInfo.InvariantCulture); 
            
        string[] suffixes = { "", "K", "M", "B", "T", "Qa", "Qi", "Sx", "Sp", "Oc", "No", "Dc" };
        int suffixIndex = 0;
        
        while (num >= 1000 && suffixIndex < suffixes.Length - 1)
        {
            num /= 1000;
            suffixIndex++;
        }
        
        return num.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture) + suffixes[suffixIndex];
    }
}