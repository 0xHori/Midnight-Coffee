using UnityEngine;
using TMPro;

public class GoldenButton : MonoBehaviour
{
    public ClickerManager clickerManager;
    public float lifeTime = 3f; 
    private bool isClicked = false;

    void Start()
    {
        // Уничтожаем через 3 секунды
        Destroy(gameObject, lifeTime); 
    }

    public void OnClick()
    {
        if (isClicked) return;
        isClicked = true;

        double reward = (clickerManager.autoIncome * clickerManager.PrestigeMultiplier * 60) + (clickerManager.clickPower * 50);
        if (reward < 100) reward = 100; 

        clickerManager.score += reward;
        
        if (clickerManager.floatingTextPrefab != null && clickerManager.canvasTransform != null)
        {
            GameObject textObj = Instantiate(clickerManager.floatingTextPrefab, clickerManager.canvasTransform);
            TextMeshProUGUI textComponent = textObj.GetComponent<TextMeshProUGUI>();
            textComponent.text = "БОНУС! +" + clickerManager.FormatNumber(reward);
            textComponent.color = Color.yellow;
            textObj.transform.localScale = new Vector3(2f, 2f, 1f);
            
            textObj.transform.position = transform.position;
            textObj.transform.SetAsLastSibling();
        }

        // ИСПОЛЬЗУЕМ СОБЫТИЯ ВМЕСТО ПРЯМЫХ ВЫЗОВОВ
        GameEvents.UpdateUI();
        GameEvents.SaveGame();

        Destroy(gameObject); 
    }
}