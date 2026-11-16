using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class LocalizedText : MonoBehaviour
{
    [Tooltip("Ключ перевода из ru.json / en.json")]
    public string localizationKey;

    private TextMeshProUGUI _text;

    void Awake()
    {
        _text = GetComponent<TextMeshProUGUI>();
    }

    void OnEnable()
    {
        LocalizationManager.OnLanguageChanged += Refresh;
        Refresh();
    }

    void OnDisable()
    {
        LocalizationManager.OnLanguageChanged -= Refresh;
    }

    public void SetKey(string newKey)
    {
        localizationKey = newKey;
        Refresh();
    }

    public void Refresh()
    {
        if (_text == null) _text = GetComponent<TextMeshProUGUI>();
        if (_text == null || string.IsNullOrEmpty(localizationKey)) return;

        _text.text = LocalizationManager.Get(localizationKey.Trim());
    }
}