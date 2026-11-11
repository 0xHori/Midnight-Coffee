using UnityEngine;
using System;

public class SaveManager : MonoBehaviour
{
    public ClickerManager clickerManager;
    public UIManager uiManager;
    
    private double maxOfflineHours = 8;

    public void SaveGame()
    {
        PlayerPrefs.SetString("SavedScore", clickerManager.score.ToString());
        PlayerPrefs.SetString("SavedClickPower", clickerManager.clickPower.ToString());
        PlayerPrefs.SetString("SavedUpgradeCost", clickerManager.upgradeCost.ToString());
        PlayerPrefs.SetString("SavedAutoIncome", clickerManager.autoIncome.ToString());
        PlayerPrefs.SetString("SavedAutoUpgradeCost", clickerManager.autoUpgradeCost.ToString());
        PlayerPrefs.Save(); 
    }

    public void LoadGame()
    {
        clickerManager.score = double.Parse(PlayerPrefs.GetString("SavedScore", "0"));
        clickerManager.clickPower = double.Parse(PlayerPrefs.GetString("SavedClickPower", "1"));
        clickerManager.upgradeCost = double.Parse(PlayerPrefs.GetString("SavedUpgradeCost", "10"));
        clickerManager.autoIncome = double.Parse(PlayerPrefs.GetString("SavedAutoIncome", "0"));
        clickerManager.autoUpgradeCost = double.Parse(PlayerPrefs.GetString("SavedAutoUpgradeCost", "50"));
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