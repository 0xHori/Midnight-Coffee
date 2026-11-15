using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Источники звука")]
    public AudioSource musicSource;
    public AudioSource sfxSource;

    [Header("Плейлист фоновой музыки (Lo-Fi / Cafe)")]
    public List<AudioClip> musicPlaylist = new List<AudioClip>();
    public bool shufflePlaylist = true;
    public float fadeDuration = 1.5f;

    public float MusicVolume { get; private set; } = 0.6f;
    public float SfxVolume { get; private set; } = 1.0f;
    public bool VibrationEnabled { get; private set; } = true;

    public bool IsMusicMuted => MusicVolume <= 0.001f;
    public bool IsSfxMuted => SfxVolume <= 0.001f;

    private float _lastNonZeroMusicVol = 0.6f;
    private float _lastNonZeroSfxVol = 1.0f;

    private const string PREF_MUSIC_VOL = "Settings_MusicVolume";
    private const string PREF_SFX_VOL = "Settings_SfxVolume";
    private const string PREF_LAST_MUSIC_VOL = "Settings_LastMusicVolume";
    private const string PREF_LAST_SFX_VOL = "Settings_LastSfxVolume";
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
        _lastNonZeroMusicVol = Mathf.Max(0.1f, PlayerPrefs.GetFloat(PREF_LAST_MUSIC_VOL, MusicVolume > 0.01f ? MusicVolume : 0.6f));
        _lastNonZeroSfxVol = Mathf.Max(0.1f, PlayerPrefs.GetFloat(PREF_LAST_SFX_VOL, SfxVolume > 0.01f ? SfxVolume : 1.0f));
        VibrationEnabled = PlayerPrefs.GetInt(PREF_VIBRATION, 1) == 1;

        ApplyVolumes();
    }

    public void SetMusicVolume(float value)
    {
        MusicVolume = Mathf.Clamp01(value);
        if (MusicVolume > 0.01f)
        {
            _lastNonZeroMusicVol = MusicVolume;
            PlayerPrefs.SetFloat(PREF_LAST_MUSIC_VOL, _lastNonZeroMusicVol);
        }

        PlayerPrefs.SetFloat(PREF_MUSIC_VOL, MusicVolume);
        PlayerPrefs.Save();
        ApplyVolumes();
    }

    public void SetSfxVolume(float value)
    {
        SfxVolume = Mathf.Clamp01(value);
        if (SfxVolume > 0.01f)
        {
            _lastNonZeroSfxVol = SfxVolume;
            PlayerPrefs.SetFloat(PREF_LAST_SFX_VOL, _lastNonZeroSfxVol);
        }

        PlayerPrefs.SetFloat(PREF_SFX_VOL, SfxVolume);
        PlayerPrefs.Save();
        ApplyVolumes();
    }

    public void ToggleMusicMute()
    {
        if (IsMusicMuted)
            SetMusicVolume(_lastNonZeroMusicVol);
        else
            SetMusicVolume(0f);
    }

    public void ToggleSfxMute()
    {
        if (IsSfxMuted)
            SetSfxVolume(_lastNonZeroSfxVol);
        else
            SetSfxVolume(0f);
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