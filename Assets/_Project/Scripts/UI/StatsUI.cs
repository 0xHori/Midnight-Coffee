using UnityEngine;
using TMPro;

public class StatsUI : MonoBehaviour
{
    [Header("Менеджеры")]
    public ClickerManager clickerManager;
    public ShopManager shopManager;
    public GachaManager gachaManager;
    public SkillTreeManager skillTreeManager;

    [Header("Панель и текст")]
    public GameObject statsPanel;
    public TextMeshProUGUI statsContentText;

    private float refreshTimer = 0f;

    void Update()
    {
        if (statsPanel != null && statsPanel.activeSelf)
        {
            refreshTimer -= Time.unscaledDeltaTime;
            if (refreshTimer <= 0f)
            {
                refreshTimer = 0.5f;
                RefreshStats();
            }
        }
    }

    public void OpenStats()
    {
        if (statsPanel != null) statsPanel.SetActive(true);
        refreshTimer = 0.5f;
        RefreshStats();
    }

    public void CloseStats()
    {
        if (statsPanel != null) statsPanel.SetActive(false);
    }

    public void RefreshStats()
    {
        if (statsContentText == null || clickerManager == null) return;

        string lifetimeStr = clickerManager.lifetimeScore.ToCompactString();
        string clickPowerStr = clickerManager.GetTotalClickPower().ToCompactString();
        string autoIncomeStr = clickerManager.GetTotalAutoIncomePerSecond().ToCompactString();

        double critChancePct = clickerManager.GetTotalCritChance() * 100.0;
        double critMult = clickerManager.GetTotalCritMultiplier();
        double prestigeMult = clickerManager.PrestigeMultiplier;
        double autoMult = clickerManager.GetTotalAutoMultiplier();

        int ownedChars = 0;
        int totalChars = 0;
        if (gachaManager != null && gachaManager.allCharacters != null)
        {
            totalChars = gachaManager.allCharacters.Count;
            foreach (var ch in gachaManager.allCharacters)
            {
                if (ch != null && gachaManager.GetCharacterLevel(ch.characterId) > 0)
                    ownedChars++;
            }
        }

        statsContentText.text =
            $"<b><color=#FFD700>— ЖУРНАЛ КОФЕЙНИ —</color></b>\n" +
            $"Выручка за всё время: <b>{lifetimeStr}</b>\n" +
            $"Приготовлено напитков: <b>{clickerManager.totalClicks:N0}</b>\n" +
            $"Идеальный латте-арт: <b>{clickerManager.totalCritClicks:N0}</b>\n" +
            $"Открыто кофеен: <b>{clickerManager.totalPrestiges}</b>\n" +
            $"Приглашений гостей: <b>{gachaManager?.totalGachaRolls ?? 0}</b>\n" +
            $"Постоянных гостей: <b>{ownedChars} / {totalChars}</b>\n\n" +
            $"<b><color=#38BDF8>— ПОКАЗАТЕЛИ СМЕНЫ —</color></b>\n" +
            $"Выручка за напиток: <b>{clickPowerStr}</b>\n" +
            $"Автовыручка: <b>{autoIncomeStr} / сек</b>\n" +
            $"Бонус репутации: <b>x{prestigeMult:0.##}</b>\n" +
            $"Множитель оборудования: <b>x{autoMult:0.##}</b>\n" +
            $"Шанс латте-арта: <b>{critChancePct:0.#}%</b>\n" +
            $"Бонус за латте-арт: <b>x{critMult:0.#}</b>";
    }
}