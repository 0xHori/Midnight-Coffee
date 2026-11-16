using System;
using UnityEngine;

public static class GameEvents
{
    public static Action OnUpdateUI;
    public static Action OnUpdateSkillUI;
    public static Action OnSaveGame;
    public static Action<double, int, int> OnShowOfflinePanel;
    public static Action<string, Sprite, Color> OnGachaResult;

    // События игрового процесса для визуала, звука и реакций персонажа
    public static event Action<double, bool, bool> OnPlayerTapped;
    public static event Action<double, bool, bool, bool> OnPlayerTappedDetailed; // + bool isManual
    public static Action OnFeverTriggered;
    public static Action OnFeverEnded;
    public static Action<double> OnPrestigeCompleted;
    public static Action OnNotEnoughCurrency;
    public static Action<CharacterData> OnCharacterEquipped;
    public static Action<CharacterData, int> OnAffectionLevelUp;
    public static Action<string> OnCharacterSpeak;
    public static Action OnLanguageChanged;

    public static void UpdateUI() => OnUpdateUI?.Invoke();
    public static void UpdateSkillUI() => OnUpdateSkillUI?.Invoke();
    public static void SaveGame() => OnSaveGame?.Invoke();
    public static void ShowOfflinePanel(double earned, int hours, int minutes) => OnShowOfflinePanel?.Invoke(earned, hours, minutes);
    public static void ShowGachaResult(string name, Sprite sprite, Color color) => OnGachaResult?.Invoke(name, sprite, color);

    public static void PlayerTapped(double earnedPoints, bool isCrit, bool isFever, bool isManual = true)
    {
        OnPlayerTapped?.Invoke(earnedPoints, isCrit, isFever);
        OnPlayerTappedDetailed?.Invoke(earnedPoints, isCrit, isFever, isManual);
    }
    public static void FeverTriggered() => OnFeverTriggered?.Invoke();
    public static void FeverEnded() => OnFeverEnded?.Invoke();
    public static void PrestigeCompleted(double crystalsGained) => OnPrestigeCompleted?.Invoke(crystalsGained);
    public static void NotEnoughCurrency() => OnNotEnoughCurrency?.Invoke();
    public static void CharacterEquipped(CharacterData character) => OnCharacterEquipped?.Invoke(character);
    public static void AffectionLevelUp(CharacterData character, int newLevel) => OnAffectionLevelUp?.Invoke(character, newLevel);
    public static void CharacterSpeak(string quote) => OnCharacterSpeak?.Invoke(quote);
    public static void LanguageChanged() => OnLanguageChanged?.Invoke();
}