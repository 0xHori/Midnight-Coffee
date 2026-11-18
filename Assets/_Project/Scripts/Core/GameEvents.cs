using System;
using System.Collections.Generic;
using UnityEngine;

public struct GachaRollEntry
{
    public CharacterData character;
    public string displayTitle;
    public Color rarityColor;
    public bool isNew;
}

public static class GameEvents
{
    public static event Action OnUpdateUI;
    public static event Action OnUpdateSkillUI;
    public static event Action OnSaveGame;
    public static event Action<double, int, int> OnShowOfflinePanel;
    public static event Action<string, Sprite, Color> OnGachaResult;
    public static event Action<List<GachaRollEntry>> OnGachaMultiResult;

    // События игрового процесса для визуала, звука и реакций персонажа
    public static event Action<double, bool, bool> OnPlayerTapped;
    public static event Action<double, bool, bool, bool> OnPlayerTappedDetailed; // + bool isManual
    public static event Action OnFeverTriggered;
    public static event Action OnFeverEnded;
    public static event Action<double> OnPrestigeCompleted;
    public static event Action OnNotEnoughCurrency;
    public static event Action<CharacterData> OnCharacterEquipped;
    public static event Action<CharacterData, int> OnAffectionLevelUp;
    public static event Action<string> OnCharacterSpeak;
    public static event Action<string, int, bool> OnUpgradePurchased;

    public static void UpdateUI() => OnUpdateUI?.Invoke();
    public static void UpdateSkillUI() => OnUpdateSkillUI?.Invoke();
    public static void SaveGame() => OnSaveGame?.Invoke();
    public static void ShowOfflinePanel(double earned, int hours, int minutes) => OnShowOfflinePanel?.Invoke(earned, hours, minutes);
    public static void ShowGachaResult(string name, Sprite sprite, Color color) => OnGachaResult?.Invoke(name, sprite, color);
    public static void ShowGachaMultiResult(List<GachaRollEntry> results) => OnGachaMultiResult?.Invoke(results);

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
    public static void UpgradePurchased(string localizedName, int newLevel, bool isMilestone) => OnUpgradePurchased?.Invoke(localizedName, newLevel, isMilestone);
}