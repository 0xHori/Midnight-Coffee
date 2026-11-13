using UnityEngine;
using System;
using System.IO;
using System.Globalization;

public class SaveManager : MonoBehaviour
{
    public ClickerManager clickerManager;
    public ShopManager shopManager;
    public SkillTreeManager skillTreeManager;
    public GachaManager gachaManager;

    private double maxOfflineHours = 8;
    private string saveFilePath;

    void Awake()
    {
        saveFilePath = Application.persistentDataPath + "/MyClickerSave.json";
    }

    void OnEnable() { GameEvents.OnSaveGame += SaveGame; }
    void OnDisable() { GameEvents.OnSaveGame -= SaveGame; }

    public void SaveGame()
    {
        GameData data = new GameData();

        data.score = clickerManager.score;
        data.totalCrystals = clickerManager.totalCrystals;
        data.prestigeThreshold = clickerManager.prestigeThreshold;

        data.totalClicks = clickerManager.totalClicks;
        data.totalCritClicks = clickerManager.totalCritClicks;
        data.lifetimeScore = clickerManager.lifetimeScore;
        data.totalPrestiges = clickerManager.totalPrestiges;

        if (shopManager != null)
            data.purchasedUpgrades = shopManager.ExportSaveData();

        if (skillTreeManager != null)
            data.unlockedSkills = skillTreeManager.ExportSaveData();

        if (gachaManager != null)
        {
            data.ownedCharacters = gachaManager.ExportSaveData();
            data.equippedCharacterId = gachaManager.equippedCharacterId;
            data.totalGachaRolls = gachaManager.totalGachaRolls;
        }

        data.lastExitTime = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture);

        string json = JsonUtility.ToJson(data, true);

        try
        {
            File.WriteAllText(saveFilePath, json);
        }
        catch (Exception e)
        {
            Debug.LogError("Ошибка при сохранении файла: " + e.Message);
        }
    }

    public void LoadGame()
    {
        if (!File.Exists(saveFilePath))
        {
            Debug.Log("Файл сохранения не найден. Начинаем новую игру.");
            return;
        }

        try
        {
            string json = File.ReadAllText(saveFilePath);
            GameData data = JsonUtility.FromJson<GameData>(json);

            clickerManager.score = data.score;
            clickerManager.totalCrystals = data.totalCrystals;
            clickerManager.prestigeThreshold = data.prestigeThreshold;

            clickerManager.totalClicks = data.totalClicks;
            clickerManager.totalCritClicks = data.totalCritClicks;
            clickerManager.lifetimeScore = data.lifetimeScore;
            clickerManager.totalPrestiges = data.totalPrestiges;

            if (shopManager != null)
                shopManager.ImportSaveData(data.purchasedUpgrades);

            if (skillTreeManager != null)
                skillTreeManager.ImportSaveData(data.unlockedSkills);

            if (gachaManager != null)
            {
                gachaManager.ImportSaveData(data.ownedCharacters, data.equippedCharacterId);
                gachaManager.totalGachaRolls = data.totalGachaRolls;
            }

            CalculateOfflineIncome(data.lastExitTime);
        }
        catch (Exception e)
        {
            Debug.LogError("Ошибка при загрузке файла: " + e.Message);
        }
    }

    private void CalculateOfflineIncome(string lastExitTimeStr)
    {
        double autoPerSec = clickerManager.GetTotalAutoIncomePerSecond();
        if (string.IsNullOrEmpty(lastExitTimeStr) || autoPerSec <= 0) return;

        if (DateTime.TryParse(lastExitTimeStr, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out DateTime lastTime))
        {
            TimeSpan timeAway = DateTime.UtcNow - lastTime;
            double secondsAway = timeAway.TotalSeconds;
            double maxSeconds = maxOfflineHours * 60 * 60;

            if (secondsAway > maxSeconds) secondsAway = maxSeconds;

            if (secondsAway > 60)
            {
                double earned = secondsAway * autoPerSec;
                clickerManager.score += earned;
                clickerManager.lifetimeScore += earned;

                int hours = (int)(secondsAway / 3600);
                int minutes = (int)((secondsAway % 3600) / 60);

                GameEvents.ShowOfflinePanel(earned, hours, minutes);
            }
        }
    }

    void OnApplicationQuit() => SaveGame();

    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus) SaveGame();
    }
}