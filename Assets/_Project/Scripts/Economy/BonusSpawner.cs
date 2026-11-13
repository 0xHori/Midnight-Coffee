using UnityEngine;
using System.Collections;

public class BonusSpawner : MonoBehaviour
{
    public GameObject goldenButtonPrefab;
    public ClickerManager clickerManager;
    public Transform canvasTransform; // Сюда перетащи --- Main Gameplay ---
    
    private bool isButtonActive = false;

    void Start()
    {
        StartCoroutine(SpawnRoutine());
    }

    IEnumerator SpawnRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(45f, 90f));

            if (!isButtonActive && goldenButtonPrefab != null && canvasTransform != null)
            {
                isButtonActive = true;
                GameObject newBtn = Instantiate(goldenButtonPrefab, canvasTransform);
                
                RectTransform rect = newBtn.GetComponent<RectTransform>();
                RectTransform parentRect = canvasTransform.GetComponent<RectTransform>();
                
                float canvasW = parentRect.rect.width;
                float canvasH = parentRect.rect.height;
                
                float randomX = Random.Range(-canvasW / 2f + 100f, canvasW / 2f - 100f);
                float randomY = Random.Range(-canvasH / 2f + 100f, canvasH / 2f - 100f);
                
                rect.anchoredPosition = new Vector2(randomX, randomY);
                rect.localScale = Vector3.one;

                GoldenButton gb = newBtn.GetComponent<GoldenButton>();
                if (gb != null)
                {
                    gb.clickerManager = clickerManager;
                }
                
                yield return new WaitUntil(() => newBtn == null);
                isButtonActive = false;
            }
        }
    }
}