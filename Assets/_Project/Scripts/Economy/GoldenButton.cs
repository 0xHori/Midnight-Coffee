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

        double reward = clickerManager.Stats.CalculateGoldenButtonReward(clickerManager.lifetimeCrystals);
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