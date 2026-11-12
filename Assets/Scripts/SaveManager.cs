using UnityEngine;
using System;
using System.IO;
using System.Globalization;

public class SaveManager : MonoBehaviour
{
    public ClickerManager clickerManager;
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
        data.clickPower = clickerManager.clickPower;
        data.flatClickPower = clickerManager.flatClickPower;
        data.upgradeCost = clickerManager.upgradeCost;
        data.flatUpgradeCost = clickerManager.flatUpgradeCost;
        data.autoIncome = clickerManager.autoIncome;
        data.autoUpgradeCost = clickerManager.autoUpgradeCost;
        data.managerCost = clickerManager.managerCost;
        data.totalCrystals = clickerManager.totalCrystals;
        data.prestigeThreshold = clickerManager.prestigeThreshold;

        if (skillTreeManager != null)
            data.unlockedSkills = skillTreeManager.ExportSaveData();

        if (gachaManager != null)
        {
            data.ownedCharacters = gachaManager.ExportSaveData();
            data.equippedCharacterId = gachaManager.equippedCharacterId;
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
            clickerManager.clickPower = data.clickPower;
            clickerManager.flatClickPower = data.flatClickPower;
            clickerManager.upgradeCost = data.upgradeCost;
            clickerManager.flatUpgradeCost = data.flatUpgradeCost;
            clickerManager.autoIncome = data.autoIncome;
            clickerManager.autoUpgradeCost = data.autoUpgradeCost;
            clickerManager.managerCost = data.managerCost;
            clickerManager.totalCrystals = data.totalCrystals;
            clickerManager.prestigeThreshold = data.prestigeThreshold;

            if (skillTreeManager != null)
                skillTreeManager.ImportSaveData(data.unlockedSkills);

            if (gachaManager != null)
                gachaManager.ImportSaveData(data.ownedCharacters, data.equippedCharacterId);

            CalculateOfflineIncome(data.lastExitTime);
        }
        catch (Exception e)
        {
            Debug.LogError("Ошибка при загрузке файла: " + e.Message);
        }
    }

    private void CalculateOfflineIncome(string lastExitTimeStr)
    {
        if (string.IsNullOrEmpty(lastExitTimeStr) || clickerManager.autoIncome <= 0) return;

        if (DateTime.TryParse(lastExitTimeStr, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out DateTime lastTime))
        {
            TimeSpan timeAway = DateTime.UtcNow - lastTime;
            double secondsAway = timeAway.TotalSeconds;
            double maxSeconds = maxOfflineHours * 60 * 60;

            if (secondsAway > maxSeconds) secondsAway = maxSeconds;

            if (secondsAway > 60)
            {
                // Теперь учитываем все актуальные множители!
                double earned = secondsAway * clickerManager.GetTotalAutoIncomePerSecond();
                clickerManager.score += earned;

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