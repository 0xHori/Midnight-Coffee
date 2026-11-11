using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BonusSpawner : MonoBehaviour
{
    public GameObject goldenButtonPrefab;
    public ClickerManager clickerManager;
    public Transform canvasTransform;
    
    private bool isButtonActive = false; // Флаг: висит ли кнопка на экране

    void Start()
    {
        StartCoroutine(SpawnRoutine());
    }

    IEnumerator SpawnRoutine()
    {
        while (true)
        {
            // Ждем от 15 до 30 секунд (для теста поставил меньше, потом можешь вернуть 45-90)
            yield return new WaitForSeconds(Random.Range(45f, 90f));

            // Спавним только если кнопки сейчас нет на экране
            if (!isButtonActive)
            {
                isButtonActive = true;
                GameObject newBtn = Instantiate(goldenButtonPrefab, canvasTransform);
                
                RectTransform rect = newBtn.GetComponent<RectTransform>();
                
                // Берем размеры Canvas
                float canvasW = canvasTransform.GetComponent<RectTransform>().rect.width;
                float canvasH = canvasTransform.GetComponent<RectTransform>().rect.height;
                
                // Спавним в случайном месте, отступая от краев
                float randomX = Random.Range(-canvasW / 2 + 100, canvasW / 2 - 100);
                float randomY = Random.Range(-canvasH / 2 + 100, canvasH / 2 - 100);
                
                rect.anchoredPosition = new Vector2(randomX, randomY);
                rect.localScale = new Vector3(1, 1, 1);

                // Привязываем ссылки
                GoldenButton gb = newBtn.GetComponent<GoldenButton>();
                if (gb != null)
                {
                    gb.clickerManager = clickerManager;
                    Button btn = newBtn.GetComponent<Button>();
                    if (btn != null)
                    {
                        btn.onClick.AddListener(() => gb.OnClick());
                    }
                }
                
                // Ждем, пока кнопка не уничтожится (у нас в скрипте это 8 секунд)
                // Или пока её не нажмут
                yield return new WaitUntil(() => newBtn == null);
                
                // Раз кнопки больше нет, сбрасываем флаг
                isButtonActive = false;
            }
        }
    }
}