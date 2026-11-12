using System;
using UnityEngine; // <--- ВОТ ЭТА СТРОЧКА РЕШАЕТ ВСЁ

public static class GameEvents
{
    // События (на них будут подписываться другие скрипты)
    public static Action OnUpdateUI;
    public static Action OnUpdateSkillUI;
    public static Action OnSaveGame;
    public static Action<double, int, int> OnShowOfflinePanel;

    // Событие для Гачи (теперь Unity знает, что такое Sprite и Color)
    public static Action<string, Sprite, Color> OnGachaResult; 

    // Методы для вызова событий (чтобы не писать везде .Invoke() и проверки на null)
    public static void UpdateUI() => OnUpdateUI?.Invoke();
    public static void UpdateSkillUI() => OnUpdateSkillUI?.Invoke();
    public static void SaveGame() => OnSaveGame?.Invoke();
    public static void ShowOfflinePanel(double earned, int hours, int minutes) => OnShowOfflinePanel?.Invoke(earned, hours, minutes);
    
    // Метод для вызова события Гачи
    public static void ShowGachaResult(string name, Sprite sprite, Color color) => OnGachaResult?.Invoke(name, sprite, color);
}