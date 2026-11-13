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

        string lifetimeStr = clickerManager.FormatNumber(clickerManager.lifetimeScore);
        string clickPowerStr = clickerManager.FormatNumber(clickerManager.GetTotalClickPower());
        string autoIncomeStr = clickerManager.FormatNumber(clickerManager.GetTotalAutoIncomePerSecond());

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
            $"<b><color=#FFD700>— ПРОГРЕСС —</color></b>\n" +
            $"Заработано за всё время: <b>{lifetimeStr}</b>\n" +
            $"Всего кликов: <b>{clickerManager.totalClicks:N0}</b>\n" +
            $"Критических кликов: <b>{clickerManager.totalCritClicks:N0}</b>\n" +
            $"Совершено престижей: <b>{clickerManager.totalPrestiges}</b>\n" +
            $"Круток в Гаче: <b>{gachaManager?.totalGachaRolls ?? 0}</b>\n" +
            $"Открыто персонажей: <b>{ownedChars} / {totalChars}</b>\n\n" +
            $"<b><color=#38BDF8>— ТЕКУЩИЕ МНОЖИТЕЛИ —</color></b>\n" +
            $"Итоговая сила клика: <b>{clickPowerStr}</b>\n" +
            $"Итоговый автодоход: <b>{autoIncomeStr} / сек</b>\n" +
            $"Множитель престижа: <b>x{prestigeMult:0.##}</b>\n" +
            $"Общий множитель автодохода: <b>x{autoMult:0.##}</b>\n" +
            $"Шанс крита: <b>{critChancePct:0.#}%</b>\n" +
            $"Множитель крита: <b>x{critMult:0.#}</b>";
    }
}