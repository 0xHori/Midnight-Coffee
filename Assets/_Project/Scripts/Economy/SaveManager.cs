using UnityEngine;
using System;
using System.IO;
using System.Globalization;
using System.Runtime.InteropServices;

public class SaveManager : MonoBehaviour
{
#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void JS_FileSystem_Sync();
#endif

    public static void SyncWebGLFileSystem()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        try
        {
            Application.ExternalEval("FS.syncfs(false, function (err) {});");
        }
        catch (Exception) { }
#endif
    }
    public BalanceConfig balanceConfig;
    public ClickerManager clickerManager;
    public ShopManager shopManager;
    public SkillTreeManager skillTreeManager;
    public GachaManager gachaManager;
    public AffectionManager affectionManager;

    private string SaveFilePath => Path.Combine(Application.persistentDataPath, "MyClickerSave.json");

    void OnEnable() { GameEvents.OnSaveGame += SaveGame; }
    void OnDisable() { GameEvents.OnSaveGame -= SaveGame; }

    public void SaveGame()
    {
        GameData data = new GameData();

        data.score = clickerManager.score;
        data.totalCrystals = clickerManager.totalCrystals;
        data.lifetimeCrystals = clickerManager.lifetimeCrystals;
        data.prestigeThreshold = clickerManager.prestigeThreshold;

        data.totalClicks = clickerManager.totalClicks;
        data.totalCritClicks = clickerManager.totalCritClicks;
        data.lifetimeScore = clickerManager.lifetimeScore;
        data.totalPrestiges = clickerManager.totalPrestiges;

        if (shopManager != null)
            data.purchasedUpgrades = shopManager.ExportSaveData();

        if (skillTreeManager != null)
            data.unlockedSkills = skillTreeManager.ExportSaveData();

        if (affectionManager != null)
            data.characterAffections = affectionManager.ExportSaveData();

        if (gachaManager != null)
        {
            data.ownedCharacters = gachaManager.ExportSaveData();
            data.equippedCharacterId = gachaManager.equippedCharacterId;
            data.totalGachaRolls = gachaManager.totalGachaRolls;
        }

        data.lastExitTime = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture);

        try
        {
            File.WriteAllText(SaveFilePath, JsonUtility.ToJson(data, true));
            SyncWebGLFileSystem();
        }
        catch (Exception e)
        {
            Debug.LogError("Ошибка при сохранении файла: " + e.Message);
        }
    }

    public void LoadGame()
    {
        if (!File.Exists(SaveFilePath))
        {
            Debug.Log("Файл сохранения не найден. Начинаем новую игру.");
            return;
        }

        try
        {
            string json = File.ReadAllText(SaveFilePath);
            GameData data = JsonUtility.FromJson<GameData>(json);

            clickerManager.score = data.score;
            clickerManager.totalCrystals = data.totalCrystals;
            if (data.prestigeThreshold > 0)
                clickerManager.prestigeThreshold = data.prestigeThreshold;
            else if (balanceConfig != null)
                clickerManager.prestigeThreshold = balanceConfig.initialPrestigeThreshold;

            clickerManager.totalClicks = data.totalClicks;
            clickerManager.totalCritClicks = data.totalCritClicks;
            clickerManager.lifetimeScore = data.lifetimeScore;
            clickerManager.totalPrestiges = data.totalPrestiges;

            if (shopManager != null)
                shopManager.ImportSaveData(data.purchasedUpgrades);

            if (skillTreeManager != null)
                skillTreeManager.ImportSaveData(data.unlockedSkills);

            if (affectionManager != null)
                affectionManager.ImportSaveData(data.characterAffections);

            if (gachaManager != null)
            {
                gachaManager.ImportSaveData(data.ownedCharacters, data.equippedCharacterId);
                gachaManager.totalGachaRolls = data.totalGachaRolls;
            }

            // Миграция старых сохранений, где lifetimeCrystals ещё не записывался
            if (data.lifetimeCrystals > 0)
            {
                clickerManager.lifetimeCrystals = Math.Max(data.lifetimeCrystals, data.totalCrystals);
            }
            else
            {
                double spentOnGacha = gachaManager != null ? data.totalGachaRolls * gachaManager.rollCost : 0;
                double spentOnSkills = CalculateSpentSkillCrystals();
                clickerManager.lifetimeCrystals = data.totalCrystals + spentOnGacha + spentOnSkills;
            }

            CalculateOfflineIncome(data.lastExitTime);
            clickerManager.SanitizeNumbers();
        }
        catch (Exception e)
        {
            Debug.LogError("Ошибка при загрузке файла: " + e.Message);
        }
    }

    private double CalculateSpentSkillCrystals()
    {
        if (skillTreeManager == null || skillTreeManager.allSkills == null) return 0;

        double totalSpent = 0;
        foreach (var skill in skillTreeManager.allSkills)
        {
            if (skill == null) continue;
            int lvl = skillTreeManager.GetSkillLevel(skill.skillId);
            for (int i = 0; i < lvl; i++)
                totalSpent += skill.GetCost(i);
        }
        return totalSpent;
    }

    private void CalculateOfflineIncome(string lastExitTimeStr)
    {
        double autoPerSec = clickerManager.GetTotalAutoIncomePerSecond();
        if (string.IsNullOrEmpty(lastExitTimeStr) || autoPerSec <= 0) return;

        double maxHours = balanceConfig != null ? balanceConfig.maxOfflineHours : 8.0;

        if (DateTime.TryParse(lastExitTimeStr, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out DateTime lastTime))
        {
            TimeSpan timeAway = DateTime.UtcNow - lastTime;
            double secondsAway = Math.Min(timeAway.TotalSeconds, maxHours * 3600.0);

            if (secondsAway > 60)
            {
                double earned = secondsAway * autoPerSec;
                clickerManager.score += earned;
                clickerManager.lifetimeScore += earned;

                int hours = (int)(secondsAway / 3600);
                int minutes = (int)((secondsAway % 3600) / 60);

                GameEvents.ShowOfflinePanel(earned, hours, minutes);
                SaveGame();
            }
        }
    }

    void OnApplicationQuit() => SaveGame();

    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus) SaveGame();
    }
}