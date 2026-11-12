using UnityEngine;
using System;
using System.Globalization;

public class SaveManager : MonoBehaviour
{
    public ClickerManager clickerManager;
    public SkillTreeManager skillTreeManager;
    public GachaManager gachaManager;
    private double maxOfflineHours = 8;

    void OnEnable() { GameEvents.OnSaveGame += SaveGame; }
    void OnDisable() { GameEvents.OnSaveGame -= SaveGame; }

    public void SaveGame()
    {
        // ... (тут твой код сохранения без изменений) ...
        PlayerPrefs.SetString("SavedScore", clickerManager.score.ToString(CultureInfo.InvariantCulture));
        PlayerPrefs.SetString("SavedClickPower", clickerManager.clickPower.ToString(CultureInfo.InvariantCulture));
        PlayerPrefs.SetString("SavedFlatClickPower", clickerManager.flatClickPower.ToString(CultureInfo.InvariantCulture));
        PlayerPrefs.SetString("SavedUpgradeCost", clickerManager.upgradeCost.ToString(CultureInfo.InvariantCulture));
        PlayerPrefs.SetString("SavedFlatUpgradeCost", clickerManager.flatUpgradeCost.ToString(CultureInfo.InvariantCulture));
        PlayerPrefs.SetString("SavedAutoIncome", clickerManager.autoIncome.ToString(CultureInfo.InvariantCulture));
        PlayerPrefs.SetString("SavedAutoUpgradeCost", clickerManager.autoUpgradeCost.ToString(CultureInfo.InvariantCulture));
        PlayerPrefs.SetString("SavedManagerCost", clickerManager.managerCost.ToString(CultureInfo.InvariantCulture));
        PlayerPrefs.SetString("SavedCrystals", clickerManager.totalCrystals.ToString(CultureInfo.InvariantCulture));
        PlayerPrefs.SetString("SavedThreshold", clickerManager.prestigeThreshold.ToString(CultureInfo.InvariantCulture));
        PlayerPrefs.SetInt("SavedClickBoost", skillTreeManager.clickBoostLvl);
        PlayerPrefs.SetInt("SavedAutoBoost", skillTreeManager.autoBoostLvl);
        PlayerPrefs.SetInt("SavedCritBoost", skillTreeManager.critBoostLvl);
        string chars = string.Join(",", gachaManager.ownedCharacterNames);
        PlayerPrefs.SetString("SavedCharacters", chars);
        PlayerPrefs.Save(); 
    }

    public void LoadGame()
    {
        // ... (тут твой код загрузки без изменений) ...
        clickerManager.score = double.Parse(PlayerPrefs.GetString("SavedScore", "0"), CultureInfo.InvariantCulture);
        clickerManager.clickPower = double.Parse(PlayerPrefs.GetString("SavedClickPower", "1"), CultureInfo.InvariantCulture);
        clickerManager.flatClickPower = double.Parse(PlayerPrefs.GetString("SavedFlatClickPower", "0"), CultureInfo.InvariantCulture);
        clickerManager.upgradeCost = double.Parse(PlayerPrefs.GetString("SavedUpgradeCost", "10"), CultureInfo.InvariantCulture);
        clickerManager.flatUpgradeCost = double.Parse(PlayerPrefs.GetString("SavedFlatUpgradeCost", "100"), CultureInfo.InvariantCulture);
        clickerManager.autoIncome = double.Parse(PlayerPrefs.GetString("SavedAutoIncome", "0"), CultureInfo.InvariantCulture);
        clickerManager.autoUpgradeCost = double.Parse(PlayerPrefs.GetString("SavedAutoUpgradeCost", "50"), CultureInfo.InvariantCulture);
        clickerManager.managerCost = double.Parse(PlayerPrefs.GetString("SavedManagerCost", "500"), CultureInfo.InvariantCulture);
        clickerManager.totalCrystals = double.Parse(PlayerPrefs.GetString("SavedCrystals", "0"), CultureInfo.InvariantCulture);
        clickerManager.prestigeThreshold = double.Parse(PlayerPrefs.GetString("SavedThreshold", "10000"), CultureInfo.InvariantCulture);
        skillTreeManager.clickBoostLvl = PlayerPrefs.GetInt("SavedClickBoost", 0);
        skillTreeManager.autoBoostLvl = PlayerPrefs.GetInt("SavedAutoBoost", 0);
        skillTreeManager.critBoostLvl = PlayerPrefs.GetInt("SavedCritBoost", 0);
        gachaManager.ownedCharacterNames.Clear();
        string savedChars = PlayerPrefs.GetString("SavedCharacters", "");
        if (!string.IsNullOrEmpty(savedChars))
            gachaManager.ownedCharacterNames.AddRange(savedChars.Split(','));
    }

    public void CalculateOfflineIncome()
    {
        if (PlayerPrefs.HasKey("LastExitTime") && clickerManager.autoIncome > 0)
        {
            DateTime lastTime = DateTime.Parse(PlayerPrefs.GetString("LastExitTime"), CultureInfo.InvariantCulture);
            TimeSpan timeAway = DateTime.UtcNow - lastTime;
            double secondsAway = timeAway.TotalSeconds;
            double maxSeconds = maxOfflineHours * 60 * 60;
            if (secondsAway > maxSeconds) secondsAway = maxSeconds;

            if (secondsAway > 60)
            {
                double earned = secondsAway * clickerManager.autoIncome * clickerManager.PrestigeMultiplier;
                clickerManager.score += earned;
                int hours = (int)(secondsAway / 3600);
                int minutes = (int)((secondsAway % 3600) / 60);
                GameEvents.ShowOfflinePanel(earned, hours, minutes); // Вызов через событие
            }
        }
    }

    void OnApplicationQuit() { PlayerPrefs.SetString("LastExitTime", DateTime.UtcNow.ToString(CultureInfo.InvariantCulture)); PlayerPrefs.Save(); }
    void OnApplicationPause(bool pauseStatus) { if (pauseStatus) { PlayerPrefs.SetString("LastExitTime", DateTime.UtcNow.ToString(CultureInfo.InvariantCulture)); PlayerPrefs.Save(); } }
}