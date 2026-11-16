using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class LocalizationEntry
{
    public string key;
    [TextArea] public string value;
}

[Serializable]
public class LocalizationFile
{
    public List<LocalizationEntry> entries = new List<LocalizationEntry>();
}

public static class LocalizationManager
{
    private const string PrefsKey = "MidnightCoffee_Language";
    private const string DefaultLang = "ru";

    private static Dictionary<string, string> _currentDict;
    private static Dictionary<string, string> _fallbackDict;
    private static bool _isInitialized = false;

    public static string CurrentLanguage { get; private set; } = DefaultLang;
    public static event Action OnLanguageChanged;

    private static void EnsureInitialized()
    {
        if (_isInitialized) return;
        _isInitialized = true;

        _fallbackDict = LoadDictionary(DefaultLang);
        string savedLang = PlayerPrefs.GetString(PrefsKey, DefaultLang);
        SetLanguage(savedLang, notify: false);
    }

    public static void SetLanguage(string langCode, bool notify = true)
    {
        if (!_isInitialized)
        {
            _isInitialized = true;
            _fallbackDict = LoadDictionary(DefaultLang);
        }

        if (string.IsNullOrEmpty(langCode)) langCode = DefaultLang;

        CurrentLanguage = langCode;
        _currentDict = (langCode == DefaultLang) ? _fallbackDict : LoadDictionary(langCode);

        PlayerPrefs.SetString(PrefsKey, CurrentLanguage);
        PlayerPrefs.Save();

        if (notify)
        {
            OnLanguageChanged?.Invoke();
        }
    }

    public static void ToggleLanguage()
    {
        SetLanguage(CurrentLanguage == "ru" ? "en" : "ru");
    }

    public static string Get(string key)
    {
        EnsureInitialized();
        if (string.IsNullOrEmpty(key)) return string.Empty;

        if (_currentDict != null && _currentDict.TryGetValue(key, out string val))
            return val;

        if (_fallbackDict != null && _fallbackDict.TryGetValue(key, out string fbVal))
            return fbVal;

        return $"[{key}]";
    }

    public static string Get(string key, params object[] args)
    {
        string template = Get(key);
        if (args == null || args.Length == 0) return template;

        try
        {
            return string.Format(template, args);
        }
        catch (FormatException)
        {
            return template;
        }
    }

    /// <summary>
    /// Склонение числительных: ищет ключи baseKey.one / baseKey.few / baseKey.many
    /// </summary>
    public static string GetPlural(string baseKey, int count)
    {
        EnsureInitialized();
        int absCount = Mathf.Abs(count);
        string suffix;

        if (CurrentLanguage == "ru")
        {
            int mod100 = absCount % 100;
            int mod10 = absCount % 10;
            if (mod100 >= 11 && mod100 <= 14) suffix = ".many";
            else if (mod10 == 1) suffix = ".one";
            else if (mod10 >= 2 && mod10 <= 4) suffix = ".few";
            else suffix = ".many";
        }
        else
        {
            suffix = (absCount == 1) ? ".one" : ".many";
        }

        return Get(baseKey + suffix, count);
    }

    private static Dictionary<string, string> LoadDictionary(string langCode)
    {
        var dict = new Dictionary<string, string>();
        TextAsset asset = Resources.Load<TextAsset>($"Localization/{langCode}");
        if (asset == null)
        {
            Debug.LogWarning($"[Localization] Файл Resources/Localization/{langCode}.json не найден.");
            return dict;
        }

        LocalizationFile file = JsonUtility.FromJson<LocalizationFile>(asset.text);
        if (file != null && file.entries != null)
        {
            foreach (var entry in file.entries)
            {
                if (!string.IsNullOrEmpty(entry.key))
                    dict[entry.key] = entry.value;
            }
        }
        return dict;
    }
}