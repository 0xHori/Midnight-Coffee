using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class SettingsUI : MonoBehaviour
{
    [Header("Панель настроек")]
    public GameObject settingsPanel;

    [Header("Элементы управления")]
    public Slider musicSlider;
    public TextMeshProUGUI musicValueText;

    public Slider sfxSlider;
    public TextMeshProUGUI sfxValueText;

    public Toggle vibrationToggle;

    [Header("Плеер")]
    public TextMeshProUGUI currentTrackText;
    public Button nextTrackButton;

    [Header("Сброс прогресса")]
    public Button resetProgressButton;
    public TextMeshProUGUI resetButtonText;

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
        if (musicValueText != null) musicValueText.text = $"{Mathf.RoundToInt(val * 100)}%";
    }

    private void UpdateSfxLabel(float val)
    {
        if (sfxValueText != null) sfxValueText.text = $"{Mathf.RoundToInt(val * 100)}%";
    }

    private void UpdateTrackLabel()
    {
        if (currentTrackText != null && AudioManager.Instance != null)
        {
            currentTrackText.text = $"Трек: {AudioManager.Instance.GetCurrentTrackName()}";
        }
    }

    private void OnResetButtonClicked()
    {
        if (!confirmResetArmed)
        {
            confirmResetArmed = true;
            resetArmTimer = 3.5f;
            if (resetButtonText != null) resetButtonText.text = "Точно сбросить? Нажми ещё раз";
            return;
        }

        // Удаляем все json-файлы сохранений в persistentDataPath
        string[] saveFiles = Directory.GetFiles(Application.persistentDataPath, "*.json");
        foreach (string file in saveFiles)
        {
            File.Delete(file);
        }

        // Перезагружаем текущую сцену с чистого листа
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void DisarmResetButton()
    {
        confirmResetArmed = false;
        if (resetButtonText != null) resetButtonText.text = "Сбросить прогресс";
    }
}