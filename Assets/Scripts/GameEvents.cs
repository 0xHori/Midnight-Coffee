using System;
using UnityEngine;

public static class GameEvents
{
    public static Action OnUpdateUI;
    public static Action OnUpdateSkillUI;
    public static Action OnSaveGame;
    public static Action<double, int, int> OnShowOfflinePanel;
    public static Action<string, Sprite, Color> OnGachaResult;

    // События игрового процесса для визуала, звука и будущих ачивок
    public static Action<double, bool, bool> OnPlayerTapped; // earnedPoints, isCrit, isFever
    public static Action OnFeverTriggered;
    public static Action<double> OnPrestigeCompleted;        // crystalsGained

    public static void UpdateUI() => OnUpdateUI?.Invoke();
    public static void UpdateSkillUI() => OnUpdateSkillUI?.Invoke();
    public static void SaveGame() => OnSaveGame?.Invoke();
    public static void ShowOfflinePanel(double earned, int hours, int minutes) => OnShowOfflinePanel?.Invoke(earned, hours, minutes);
    public static void ShowGachaResult(string name, Sprite sprite, Color color) => OnGachaResult?.Invoke(name, sprite, color);

    public static void PlayerTapped(double earnedPoints, bool isCrit, bool isFever) => OnPlayerTapped?.Invoke(earnedPoints, isCrit, isFever);
    public static void FeverTriggered() => OnFeverTriggered?.Invoke();
    public static void PrestigeCompleted(double crystalsGained) => OnPrestigeCompleted?.Invoke(crystalsGained);
}