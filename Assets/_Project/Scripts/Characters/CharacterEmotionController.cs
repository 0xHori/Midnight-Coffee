using UnityEngine;
using UnityEngine.UI;

public class CharacterEmotionController : MonoBehaviour
{
    [Header("Ссылки")]
    public Image characterImage;
    public Sprite defaultClickerSprite;
    public FeverManager feverManager;
    public GachaManager gachaManager;
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

    void OnEnable()
    {
        GameEvents.OnNotEnoughCurrency += OnNotEnoughCurrency;
        GameEvents.OnCharacterEquipped += SetCharacter;
        GameEvents.OnAffectionLevelUp += HandleAffectionLevelUp;
    }

    void OnDisable()
    {
        GameEvents.OnNotEnoughCurrency -= OnNotEnoughCurrency;
        GameEvents.OnCharacterEquipped -= SetCharacter;
        GameEvents.OnAffectionLevelUp -= HandleAffectionLevelUp;
    }

    void Start()
    {
        if (gachaManager != null)
        {
            CharacterData equipped = gachaManager.GetEquippedCharacter();
            if (equipped != null)
            {
                SetCharacter(equipped);
                return;
            }
        }

        if (currentCharacter != null)
            SetCharacter(currentCharacter);
        else
            ResetToDefaultSprite();
    }

    void Update()
    {
        if (feverManager != null && feverManager.isFeverActive)
        {
            if (_currentEmotion != CharacterEmotion.Love)
                ApplyEmotion(CharacterEmotion.Love);
            return;
        }

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

    public void SetCharacter(CharacterData newCharacter)
    {
        currentCharacter = newCharacter;
        _rapidTapCount = 0;
        _resetTimer = 0f;

        if (newCharacter == null)
        {
            ResetToDefaultSprite();
            return;
        }

        ApplyEmotion(CharacterEmotion.Normal);
    }

    private void HandleAffectionLevelUp(CharacterData character, int newLevel)
    {
        if (currentCharacter == character)
            SetTemporaryEmotion(CharacterEmotion.Love, 2.0f);
    }

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

    private void ResetToDefaultSprite()
    {
        _currentEmotion = CharacterEmotion.Normal;
        if (characterImage != null && defaultClickerSprite != null)
            characterImage.sprite = defaultClickerSprite;
    }
}