using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class SettingsUI : MonoBehaviour
{
    [Header("Панель настроек")]
    public GameObject settingsPanel;

    [Header("Музыка")]
    public TextMeshProUGUI musicLabelText;
    public Slider musicSlider;
    public TextMeshProUGUI musicValueText;
    public Button musicMuteButton;

    [Header("Звуки и эффекты (SFX)")]
    public TextMeshProUGUI sfxLabelText;
    public Slider sfxSlider;
    public TextMeshProUGUI sfxValueText;
    public Button sfxMuteButton;

    [Header("Вибрация")]
    public Toggle vibrationToggle;

    [Header("Плеер")]
    public TextMeshProUGUI currentTrackText;
    public Button nextTrackButton;

    [Header("Сброс прогресса")]
    public Button resetProgressButton;
    public TextMeshProUGUI resetButtonText;

    [Header("Язык (Localization)")]
    public Button languageButton;
    public TextMeshProUGUI languageButtonText;

    private TextMeshProUGUI _musicMuteBtnText;
    private TextMeshProUGUI _sfxMuteBtnText;

    private bool confirmResetArmed = false;
    private float resetArmTimer = 0f;

    void Start()
    {
        if (musicSlider != null)
            musicSlider.onValueChanged.AddListener(OnMusicSliderChanged);

        if (sfxSlider != null)
            sfxSlider.onValueChanged.AddListener(OnSfxSliderChanged);

        if (vibrationToggle != null)
            vibrationToggle.onValueChanged.AddListener(OnVibrationToggled);

        if (nextTrackButton != null)
            nextTrackButton.onClick.AddListener(OnNextTrackClicked);

        if (resetProgressButton != null)
            resetProgressButton.onClick.AddListener(OnResetButtonClicked);

        if (musicMuteButton != null)
        {
            musicMuteButton.onClick.AddListener(OnMusicMuteClicked);
            _musicMuteBtnText = musicMuteButton.GetComponentInChildren<TextMeshProUGUI>();
        }

        if (sfxMuteButton != null)
        {
            sfxMuteButton.onClick.AddListener(OnSfxMuteClicked);
            _sfxMuteBtnText = sfxMuteButton.GetComponentInChildren<TextMeshProUGUI>();
        }

        if (languageButton != null)
            languageButton.onClick.AddListener(OnLanguageButtonClicked);

        UpdateLanguageLabel();
    }

    void Update()
    {
        if (confirmResetArmed)
        {
            resetArmTimer -= Time.unscaledDeltaTime;
            if (resetArmTimer <= 0f)
            {
                DisarmResetButton();
            }
        }
    }

    void OnEnable()
    {
        SyncUIWithManager();
        UpdateLanguageLabel();
    }

    public void OpenSettings()
    {
        if (settingsPanel != null) settingsPanel.SetActive(true);
        SyncUIWithManager();
        DisarmResetButton();
    }

    public void CloseSettings()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
    }

    private void SyncUIWithManager()
    {
        if (AudioManager.Instance == null) return;

        if (musicSlider != null)
            musicSlider.SetValueWithoutNotify(AudioManager.Instance.MusicVolume);
        UpdateMusicLabel(AudioManager.Instance.MusicVolume);

        if (sfxSlider != null)
            sfxSlider.SetValueWithoutNotify(AudioManager.Instance.SfxVolume);
        UpdateSfxLabel(AudioManager.Instance.SfxVolume);

        if (vibrationToggle != null)
            vibrationToggle.SetIsOnWithoutNotify(AudioManager.Instance.VibrationEnabled);

        UpdateTrackLabel();
    }

    private void OnMusicMuteClicked()
    {
        if (AudioManager.Instance == null) return;
        AudioManager.Instance.ToggleMusicMute();
        SyncUIWithManager();
    }

    private void OnSfxMuteClicked()
    {
        if (AudioManager.Instance == null) return;
        AudioManager.Instance.ToggleSfxMute();
        SyncUIWithManager();
    }

    private void OnMusicSliderChanged(float val)
    {
        if (AudioManager.Instance != null) AudioManager.Instance.SetMusicVolume(val);
        UpdateMusicLabel(val);
    }

    private void OnSfxSliderChanged(float val)
    {
        if (AudioManager.Instance != null) AudioManager.Instance.SetSfxVolume(val);
        UpdateSfxLabel(val);
    }

    private void OnVibrationToggled(bool isOn)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetVibration(isOn);
            if (isOn) AudioManager.TriggerHaptic();
        }
    }

    private void OnNextTrackClicked()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayNextTrack();
            UpdateTrackLabel();
        }
    }

    private void UpdateMusicLabel(float val)
    {
        bool muted = val <= 0.001f;
        if (musicValueText != null)
        {
            musicValueText.text = $"{Mathf.RoundToInt(val * 100)}%";
            musicValueText.color = muted ? new Color(0.95f, 0.4f, 0.4f) : Color.white;
        }
        if (musicLabelText != null)
            musicLabelText.color = muted ? new Color(0.7f, 0.7f, 0.7f) : Color.white;

        UpdateMuteButtonVisual(musicMuteButton, _musicMuteBtnText, muted);
    }

    private void UpdateSfxLabel(float val)
    {
        bool muted = val <= 0.001f;
        if (sfxValueText != null)
        {
            sfxValueText.text = $"{Mathf.RoundToInt(val * 100)}%";
            sfxValueText.color = muted ? new Color(0.95f, 0.4f, 0.4f) : Color.white;
        }
        if (sfxLabelText != null)
            sfxLabelText.color = muted ? new Color(0.7f, 0.7f, 0.7f) : Color.white;

        UpdateMuteButtonVisual(sfxMuteButton, _sfxMuteBtnText, muted);
    }

    private void UpdateMuteButtonVisual(Button btn, TextMeshProUGUI btnText, bool muted)
    {
        if (btn == null) return;
        if (btnText != null)
        {
            btnText.text = muted ? LocalizationManager.Get("settings.on") : LocalizationManager.Get("settings.off");
            btnText.color = muted ? Color.white : new Color(0.12f, 0.14f, 0.17f);
        }
        if (btn.targetGraphic != null)
        {
            btn.targetGraphic.color = muted
                ? new Color(0.75f, 0.25f, 0.25f) // Красная, когда звук выключен
                : new Color(0.92f, 0.92f, 0.92f); // Светлая в стиле твоих кнопок, когда включен
        }
    }

    private void UpdateTrackLabel()
    {
        if (currentTrackText != null && AudioManager.Instance != null)
        {
            currentTrackText.text = LocalizationManager.Get("settings.track", AudioManager.Instance.GetCurrentTrackName());
        }
    }

    private void OnResetButtonClicked()
    {
        if (!confirmResetArmed)
        {
            confirmResetArmed = true;
            resetArmTimer = 3.5f;
            if (resetButtonText != null) resetButtonText.text = LocalizationManager.Get("settings.reset_confirm");
            return;
        }

        string[] saveFiles = Directory.GetFiles(Application.persistentDataPath, "*.json");
        foreach (string file in saveFiles)
        {
            File.Delete(file);
        }

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void DisarmResetButton()
    {
        confirmResetArmed = false;
        if (resetButtonText != null) resetButtonText.text = LocalizationManager.Get("settings.reset");
    }

    private void OnLanguageButtonClicked()
    {
        LocalizationManager.ToggleLanguage();
        GameEvents.UpdateUI();
        UpdateLanguageLabel();
        SyncUIWithManager();
    }

    private void UpdateLanguageLabel()
    {
        if (languageButtonText != null)
            languageButtonText.text = LocalizationManager.Get("settings.language_btn");
    }
}