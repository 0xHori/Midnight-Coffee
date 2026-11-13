using UnityEngine;
using UnityEngine.UI;

public class CharacterEmotionController : MonoBehaviour
{
    [Header("Ссылки")]
    public Image characterImage;
    public FeverManager feverManager;
    public CharacterData currentCharacter;

    [Header("Тайминги")]
    [Tooltip("Через сколько секунд после реакции персонаж вернётся в спокойное состояние")]
    public float emotionResetDelay = 1.2f;
    [Tooltip("Сколько быстрых тапов подряд нужно для эмоции удивления")]
    public int comboThresholdForExcited = 6;

    private CharacterEmotion _currentEmotion = CharacterEmotion.Normal;
    private float _resetTimer = 0f;
    private int _rapidTapCount = 0;
    private float _lastTapTime = 0f;

    void Start()
    {
        if (currentCharacter != null)
            SetCharacter(currentCharacter);
    }

    void Update()
    {
        // Если активен Fever Mode — держим эмоцию сердечек (Love)
        if (feverManager != null && feverManager.isFeverActive)
        {
            if (_currentEmotion != CharacterEmotion.Love)
                ApplyEmotion(CharacterEmotion.Love);
            return;
        }

        // Таймер возврата в обычное состояние
        if (_currentEmotion != CharacterEmotion.Normal)
        {
            _resetTimer -= Time.deltaTime;
            if (_resetTimer <= 0f)
            {
                ApplyEmotion(CharacterEmotion.Normal);
                _rapidTapCount = 0;
            }
        }
    }

    /// <summary>
    /// Вызывай этот метод из GachaManager / GalleryManager при смене активного персонажа
    /// </summary>
    public void SetCharacter(CharacterData newCharacter)
    {
        currentCharacter = newCharacter;
        _rapidTapCount = 0;
        _resetTimer = 0f;
        ApplyEmotion(CharacterEmotion.Normal);
    }

    /// <summary>
    /// Вызывай при каждом клике по персонажу (можно повесить прямо на Button.OnClick или вызывать из ClickerManager)
    /// </summary>
    public void OnCharacterTapped()
    {
        if (feverManager != null && feverManager.isFeverActive)
        {
            ApplyEmotion(CharacterEmotion.Love);
            return;
        }

        if (Time.time - _lastTapTime <= 0.45f)
            _rapidTapCount++;
        else
            _rapidTapCount = 1;

        _lastTapTime = Time.time;

        if (_rapidTapCount >= comboThresholdForExcited)
        {
            SetTemporaryEmotion(CharacterEmotion.Excited, emotionResetDelay);
        }
    }

    /// <summary>
    /// Вызывай из ShopManager / GachaManager / SkillTreeManager, когда игроку не хватило валюты
    /// </summary>
    public void OnNotEnoughCurrency()
    {
        if (feverManager != null && feverManager.isFeverActive) return;
        SetTemporaryEmotion(CharacterEmotion.Angry, 1.5f);
    }

    public void SetTemporaryEmotion(CharacterEmotion emotion, float duration)
    {
        _resetTimer = duration;
        ApplyEmotion(emotion);
    }

    private void ApplyEmotion(CharacterEmotion emotion)
    {
        if (currentCharacter == null || characterImage == null) return;

        _currentEmotion = emotion;
        Sprite nextSprite = currentCharacter.GetEmotionSprite(emotion);
        if (nextSprite != null)
            characterImage.sprite = nextSprite;
    }
}