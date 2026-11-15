using UnityEngine;
using System.Collections;

public class BonusSpawner : MonoBehaviour
{
    public GameObject goldenButtonPrefab;
    public ClickerManager clickerManager;
    public Transform canvasTransform;

    [Header("Интервал появления (сек)")]
    public float minSpawnDelay = 35f;
    public float maxSpawnDelay = 70f;

    [Header("Безопасные отступы от краёв экрана")]
    public float horizontalPadding = 110f;
    public float topPadding = 260f;
    public float bottomPadding = 380f;

    private bool isButtonActive = false;

    void Start()
    {
        StartCoroutine(SpawnRoutine());
    }

    IEnumerator SpawnRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(minSpawnDelay, maxSpawnDelay));

            if (!isButtonActive && goldenButtonPrefab != null && canvasTransform != null)
            {
                isButtonActive = true;
                GameObject newBtn = Instantiate(goldenButtonPrefab, canvasTransform);
                newBtn.transform.SetAsLastSibling();

                RectTransform rect = newBtn.GetComponent<RectTransform>();
                RectTransform parentRect = canvasTransform.GetComponent<RectTransform>();

                float halfW = parentRect.rect.width * 0.5f;
                float halfH = parentRect.rect.height * 0.5f;

                float minX = -halfW + horizontalPadding;
                float maxX = halfW - horizontalPadding;
                float minY = -halfH + bottomPadding;
                float maxY = halfH - topPadding;

                if (minX > maxX) minX = maxX = 0f;
                if (minY > maxY) minY = maxY = 0f;

                rect.anchoredPosition = new Vector2(Random.Range(minX, maxX), Random.Range(minY, maxY));
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