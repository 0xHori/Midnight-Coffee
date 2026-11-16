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

        statsContentText.text = LocalizationManager.Get(
            "stats.full_report",
            lifetimeStr,
            $"{clickerManager.totalClicks:N0}",
            $"{clickerManager.totalCritClicks:N0}",
            clickerManager.totalPrestiges,
            gachaManager?.totalGachaRolls ?? 0,
            ownedChars,
            totalChars,
            clickPowerStr,
            autoIncomeStr,
            $"{prestigeMult:0.##}",
            $"{autoMult:0.##}",
            $"{critChancePct:0.#}",
            $"{critMult:0.#}"
        );
    }
}