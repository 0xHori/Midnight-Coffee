using UnityEngine;
using UnityEngine.EventSystems;

public class GoldenButton : MonoBehaviour, IPointerDownHandler
{
    public ClickerManager clickerManager;
    public float lifeTime = 6f; // 6 секунд вместо 3, чтобы успеть нажать на телефоне
    private bool isClicked = false;

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    // Срабатывает мгновенно при касании экрана пальцем, а не при отпускании
    public void OnPointerDown(PointerEventData eventData)
    {
        OnClick();
    }

    public void OnClick()
    {
        if (isClicked || clickerManager == null) return;
        isClicked = true;

        // Теперь учитываем ВСЕ множители автодохода (престиж + навыки + персонажи)
        double totalAutoPerSec = clickerManager.GetTotalAutoIncomePerSecond();
        double reward = (totalAutoPerSec * 60.0) + (clickerManager.clickPower * 50.0);
        if (reward < 100) reward = 100;

        clickerManager.score += reward;

        if (clickerManager.floatingTextPool != null)
        {
            clickerManager.floatingTextPool.SpawnText(
                "БОНУС! +" + clickerManager.FormatNumber(reward),
                Color.yellow,
                transform.position,
                2f
            );
        }

        GameEvents.UpdateUI();
        GameEvents.SaveGame();

        Destroy(gameObject);
    }
}