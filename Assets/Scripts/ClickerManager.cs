using UnityEngine;
using TMPro; // Обязательно для работы с текстом

public class ClickerManager : MonoBehaviour
{
    // Переменная для хранения очков
    public int score = 0;

    // Ссылка на текст, который мы создали
    public TextMeshProUGUI scoreText;

    // Метод, который будет вызываться при клике
    public void AddScore()
    {
        score = score + 1; // Увеличиваем счет на 1
        
        // Обновляем текст на экране
        scoreText.text = "Счет: " + score.ToString(); 
    }
}