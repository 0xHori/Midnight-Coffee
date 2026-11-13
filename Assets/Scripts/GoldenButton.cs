using UnityEngine;
using UnityEngine.EventSystems;

public class GoldenButton : MonoBehaviour, IPointerDownHandler
{
    public ClickerManager clickerManager;
    public float lifeTime = 6f;
    private bool isClicked = false;

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        OnClick();
    }

    public void OnClick()
    {
        if (isClicked || clickerManager == null) return;
        isClicked = true;

        double totalAutoPerSec = clickerManager.GetTotalAutoIncomePerSecond();
        double currentClickPower = clickerManager.GetTotalClickPower();

        double reward = (totalAutoPerSec * 60.0) + (currentClickPower * 50.0);
        if (reward < 100) reward = 100;

        clickerManager.AddEarnedScore(reward);

        if (clickerManager.floatingTextPool != null)
        {
            clickerManager.floatingTextPool.SpawnText(
                $"БОНУС! +{reward.ToCompactString()}",
                Color.yellow,
                transform.position,
                2f
            );
        }

        GameEvents.SaveGame();
        Destroy(gameObject);
    }
}