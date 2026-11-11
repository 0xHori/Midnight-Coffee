using UnityEngine;
using System;
using System.Globalization; // Обязательно для CultureInfo

public class SaveManager : MonoBehaviour
{
    public ClickerManager clickerManager;
    public UIManager uiManager;
    
    private double maxOfflineHours = 8;

    public void SaveGame()
    {
        // Сохраняем, явно указывая InvariantCulture, чтобы десятичный разделитель всегда был точкой
        PlayerPrefs.SetString("SavedScore", clickerManager.score.ToString(CultureInfo.InvariantCulture));
        PlayerPrefs.SetString("SavedClickPower", clickerManager.clickPower.ToString(CultureInfo.InvariantCulture));
        PlayerPrefs.SetString("SavedUpgradeCost", clickerManager.upgradeCost.ToString(CultureInfo.InvariantCulture));
        PlayerPrefs.SetString("SavedAutoIncome", clickerManager.autoIncome.ToString(CultureInfo.InvariantCulture));
        PlayerPrefs.SetString("SavedAutoUpgradeCost", clickerManager.autoUpgradeCost.ToString(CultureInfo.InvariantCulture));
        PlayerPrefs.Save(); 
    }

    public void LoadGame()
    {
        // Загружаем, также используя InvariantCulture, и добавляем проверку на пустую строку
        string scoreStr = PlayerPrefs.GetString("SavedScore", "0");
        clickerManager.score = string.IsNullOrEmpty(scoreStr) ? 0 : double.Parse(scoreStr, CultureInfo.InvariantCulture);

        string clickPowerStr = PlayerPrefs.GetString("SavedClickPower", "1");
        clickerManager.clickPower = string.IsNullOrEmpty(clickPowerStr) ? 1 : double.Parse(clickPowerStr, CultureInfo.InvariantCulture);

        string upgradeCostStr = PlayerPrefs.GetString("SavedUpgradeCost", "10");
        clickerManager.upgradeCost = string.IsNullOrEmpty(upgradeCostStr) ? 10 : double.Parse(upgradeCostStr, CultureInfo.InvariantCulture);

        string autoIncomeStr = PlayerPrefs.GetString("SavedAutoIncome", "0");
        clickerManager.autoIncome = string.IsNullOrEmpty(autoIncomeStr) ? 0 : double.Parse(autoIncomeStr, CultureInfo.InvariantCulture);

        string autoUpgradeCostStr = PlayerPrefs.GetString("SavedAutoUpgradeCost", "50");
        clickerManager.autoUpgradeCost = string.IsNullOrEmpty(autoUpgradeCostStr) ? 50 : double.Parse(autoUpgradeCostStr, CultureInfo.InvariantCulture);
    }

    public void CalculateOfflineIncome()
    {
        if (PlayerPrefs.HasKey("LastExitTime") && clickerManager.autoIncome > 0)
        {
            string lastTimeString = PlayerPrefs.GetString("LastExitTime");
            DateTime lastTime = DateTime.Parse(lastTimeString);
            TimeSpan timeAway = DateTime.UtcNow - lastTime;
            double secondsAway = timeAway.TotalSeconds;

            double maxSeconds = maxOfflineHours * 60 * 60;
            if (secondsAway > maxSeconds) secondsAway = maxSeconds;

            if (secondsAway > 60)
            {
                double earned = secondsAway * clickerManager.autoIncome;
                clickerManager.score += earned;

                int hours = (int)(secondsAway / 3600);
                int minutes = (int)((secondsAway % 3600) / 60);
                
                uiManager.ShowOfflinePanel(earned, hours, minutes);
            }
        }
    }

    void OnApplicationQuit()
    {
        PlayerPrefs.SetString("LastExitTime", DateTime.UtcNow.ToString());
        PlayerPrefs.Save();
    }

    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            PlayerPrefs.SetString("LastExitTime", DateTime.UtcNow.ToString());
            PlayerPrefs.Save();
        }
    }
}