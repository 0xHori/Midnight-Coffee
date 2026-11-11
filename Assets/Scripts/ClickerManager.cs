using UnityEngine;
using TMPro;

public class ClickerManager : MonoBehaviour
{
    public UnityEngine.UI.Button upgradeButtonComponent; // Ссылка на компонент кнопки

    // --- Переменные игры ---
    public int score = 0;           // Текущие очки
    public int clickPower = 1;      // Сколько очков дает 1 клик
    public int upgradeCost = 10;    // Сколько стоит улучшение

    // --- Ссылки на UI ---
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI upgradeText; // Текст на кнопке улучшения

    // Метод для обычного клика
    public void AddScore()
    {
        score = score + clickPower; // Теперь прибавляем силу клика, а не 1
        UpdateUI();
    }

    // Метод для покупки улучшения
    public void BuyUpgrade()
    {
        // Проверяем, хватает ли очков
        if (score >= upgradeCost)
        {
            score = score - upgradeCost; // Списываем цену
            clickPower = clickPower * 2; // Увеличиваем силу клика в 2 раза
            upgradeCost = upgradeCost * 2; // Следующее улучшение будет стоить дороже
            
            UpdateUI(); // Обновляем текст на экране
        }
    }

    // Вспомогательный метод для обновления всех надписей
    void UpdateUI()
    {
        scoreText.text = "Счет: " + score.ToString();
        
        if (upgradeText != null)
        {
            upgradeText.text = "Улучшение x2\nЦена: " + upgradeCost.ToString();
        }

        // Проверяем, хватает ли денег, чтобы включить/выключить кнопку
        if (upgradeButtonComponent != null)
        {
            if (score >= upgradeCost)
            {
                upgradeButtonComponent.interactable = true; // Включаем (можно нажимать)
            }
            else
            {
                upgradeButtonComponent.interactable = false; // Выключаем (серая, не нажимается)
            }
        }
    }
}