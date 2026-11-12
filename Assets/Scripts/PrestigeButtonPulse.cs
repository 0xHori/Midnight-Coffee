using UnityEngine;

public class PrestigeButtonPulse : MonoBehaviour
{
    public ClickerManager clickerManager;
    private Vector3 originalScale;

    void Start()
    {
        originalScale = transform.localScale;
    }

    void Update()
    {
        // Если игрок может получить кристаллы — пульсируем
        if (clickerManager.CalculatePotentialCrystals() > 0)
        {
            // Формула синуса создает плавную пульсацию от 1.0 до 1.2
            float scale = 1f + Mathf.Sin(Time.time * 6f) * 0.1f;
            transform.localScale = originalScale * scale;
        }
        else
        {
            // Если нельзя — возвращаем обычный размер
            transform.localScale = originalScale;
        }
    }
}