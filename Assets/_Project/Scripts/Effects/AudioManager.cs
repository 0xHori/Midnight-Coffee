using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Источники звука")]
    public AudioSource musicSource; // Отдельный AudioSource для фоновой музыки
    public AudioSource sfxSource;   // Твой существующий AudioSource для кликов, покупок и гачи

    [Header("Плейлист фоновой музыки (Lo-Fi / Cafe)")]
    public List<AudioClip> musicPlaylist = new List<AudioClip>();
    public bool shufflePlaylist = true;
    public float fadeDuration = 1.5f;

    public float MusicVolume { get; private set; } = 0.6f;
    public float SfxVolume { get; private set; } = 1.0f;
    public bool VibrationEnabled { get; private set; } = true;

    private const string PREF_MUSIC_VOL = "Settings_MusicVolume";
    private const string PREF_SFX_VOL = "Settings_SfxVolume";
    private const string PREF_VIBRATION = "Settings_Vibration";

    private int currentTrackIndex = -1;
    private Coroutine trackRoutine;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        LoadSettings();
    }

    void Start()
    {
        if (musicPlaylist.Count > 0 && musicSource != null)
        {
            PlayNextTrack();
        }
    }

    public void LoadSettings()
    {
        MusicVolume = PlayerPrefs.GetFloat(PREF_MUSIC_VOL, 0.6f);
        SfxVolume = PlayerPrefs.GetFloat(PREF_SFX_VOL, 1.0f);
        VibrationEnabled = PlayerPrefs.GetInt(PREF_VIBRATION, 1) == 1;

        ApplyVolumes();
    }

    public void SetMusicVolume(float value)
    {
        MusicVolume = Mathf.Clamp01(value);
        PlayerPrefs.SetFloat(PREF_MUSIC_VOL, MusicVolume);
        PlayerPrefs.Save();
        ApplyVolumes();
    }

    public void SetSfxVolume(float value)
    {
        SfxVolume = Mathf.Clamp01(value);
        PlayerPrefs.SetFloat(PREF_SFX_VOL, SfxVolume);
        PlayerPrefs.Save();
        ApplyVolumes();
    }

    public void SetVibration(bool enabled)
    {
        VibrationEnabled = enabled;
        PlayerPrefs.SetInt(PREF_VIBRATION, enabled ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void ApplyVolumes()
    {
        if (musicSource != null) musicSource.volume = MusicVolume;
        if (sfxSource != null) sfxSource.volume = SfxVolume;
    }

    public void PlayNextTrack()
    {
        if (musicPlaylist == null || musicPlaylist.Count == 0 || musicSource == null) return;

        if (shufflePlaylist && musicPlaylist.Count > 1)
        {
            int nextIndex = currentTrackIndex;
            while (nextIndex == currentTrackIndex)
            {
                nextIndex = Random.Range(0, musicPlaylist.Count);
            }
            currentTrackIndex = nextIndex;
        }
        else
        {
            currentTrackIndex = (currentTrackIndex + 1) % musicPlaylist.Count;
        }

        if (trackRoutine != null) StopCoroutine(trackRoutine);
        trackRoutine = StartCoroutine(CrossfadeToTrack(musicPlaylist[currentTrackIndex]));
    }

    public string GetCurrentTrackName()
    {
        if (currentTrackIndex >= 0 && currentTrackIndex < musicPlaylist.Count && musicPlaylist[currentTrackIndex] != null)
        {
            return musicPlaylist[currentTrackIndex].name;
        }
        return "Нет треков";
    }

    private IEnumerator CrossfadeToTrack(AudioClip nextClip)
    {
        if (nextClip == null) yield break;

        // 1. Плавное затухание текущего трека
        if (musicSource.isPlaying && musicSource.volume > 0.01f)
        {
            float startVol = musicSource.volume;
            float t = 0f;
            while (t < fadeDuration)
            {
                t += Time.unscaledDeltaTime;
                musicSource.volume = Mathf.Lerp(startVol, 0f, t / fadeDuration);
                yield return null;
            }
        }

        // 2. Переключение клипа и плавное нарастание громкости
        musicSource.Stop();
        musicSource.clip = nextClip;
        musicSource.loop = false;
        musicSource.Play();

        float fadeInTime = 0f;
        while (fadeInTime < fadeDuration)
        {
            fadeInTime += Time.unscaledDeltaTime;
            musicSource.volume = Mathf.Lerp(0f, MusicVolume, fadeInTime / fadeDuration);
            yield return null;
        }
        musicSource.volume = MusicVolume;

        // 3. Ожидание конца трека с запасом на затухание и автоматический переход к следующему
        while (musicSource.isPlaying && musicSource.time < nextClip.length - fadeDuration)
        {
            yield return null;
        }

        PlayNextTrack();
    }

    public static void TriggerHaptic()
    {
        if (Instance != null && !Instance.VibrationEnabled) return;

#if UNITY_ANDROID || UNITY_IOS
        Handheld.Vibrate();
#endif
    }
}