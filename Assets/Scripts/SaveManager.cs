using System.Collections.Generic;
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
        // Путь к файлу сохранения. В Windows это AppData, на Android/iOS - внутренняя память приложения.
        saveFilePath = Application.persistentDataPath + "/MyClickerSave.json";
    }

    void OnEnable() { GameEvents.OnSaveGame += SaveGame; }
    void OnDisable() { GameEvents.OnSaveGame -= SaveGame; }

    public void SaveGame()
    {
        GameData data = new GameData();

        // 1. Собираем данные из менеджеров
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

        data.clickBoostLvl = skillTreeManager.clickBoostLvl;
        data.autoBoostLvl = skillTreeManager.autoBoostLvl;
        data.critBoostLvl = skillTreeManager.critBoostLvl;

        if (gachaManager != null)
        {
            data.ownedCharacterNames = new List<string>(gachaManager.ownedCharacterNames);
        }

        // 2. Записываем время сохранения (в формате UTC)
        data.lastExitTime = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture);

        // 3. Конвертируем в красивый JSON
        string json = JsonUtility.ToJson(data, true); // true = форматирование с отступами (удобно читать)

        // 4. Сохраняем в файл
        try
        {
            File.WriteAllText(saveFilePath, json);
            Debug.Log("Игра сохранена в: " + saveFilePath);
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
            return; // Выходим, оставляя дефолтные значения из инспектора
        }

        try
        {
            // 1. Читаем файл
            string json = File.ReadAllText(saveFilePath);
            
            // 2. Конвертируем обратно в класс
            GameData data = JsonUtility.FromJson<GameData>(json);

            // 3. Раздаем данные обратно менеджерам
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

            skillTreeManager.clickBoostLvl = data.clickBoostLvl;
            skillTreeManager.autoBoostLvl = data.autoBoostLvl;
            skillTreeManager.critBoostLvl = data.critBoostLvl;

            if (gachaManager != null && data.ownedCharacterNames != null)
            {
                gachaManager.ownedCharacterNames = new List<string>(data.ownedCharacterNames);
            }

            // 4. Считаем офлайн-доход используя время из файла
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

            if (secondsAway > 60) // Если прошло больше минуты
            {
                double earned = secondsAway * clickerManager.autoIncome * clickerManager.PrestigeMultiplier;
                clickerManager.score += earned;
                
                int hours = (int)(secondsAway / 3600);
                int minutes = (int)((secondsAway % 3600) / 60);
                
                GameEvents.ShowOfflinePanel(earned, hours, minutes);
            }
        }
    }

    // --- Убираем PlayerPrefs для игровых данных, но оставляем для времени выхода ---
    // При сворачивании игры мы не хотим писать весь JSON (это может занять время), 
    // достаточно быстро сохранить текущее время.
    void OnApplicationQuit() 
    { 
        SaveGame(); // На всякий случай сохраняем всё перед выходом
    }
    
    void OnApplicationPause(bool pauseStatus) 
    { 
        if (pauseStatus) 
        { 
            SaveGame(); // Сохраняем игру при сворачивании на телефоне
        } 
    }
}