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
    [Tooltip("Через сколько секунд без кликов персонаж начинает скучать (Sad)")]
    public float idleThresholdForSad = 25f;

    private CharacterEmotion _currentEmotion = CharacterEmotion.Normal;
    private float _resetTimer = 0f;
    private float _idleTimer = 0f;
    private int _rapidTapCount = 0;
    private float _lastTapTime = 0f;
    private int _lastProcessedTapFrame = -1;

    void OnEnable()
    {
        GameEvents.OnNotEnoughCurrency += OnNotEnoughCurrency;
        GameEvents.OnCharacterEquipped += SetCharacter;
        GameEvents.OnAffectionLevelUp += HandleAffectionLevelUp;
        GameEvents.OnPlayerTappedDetailed += HandlePlayerTapped;
        GameEvents.OnGachaResult += HandleGachaResult;
    }

    void OnDisable()
    {
        GameEvents.OnNotEnoughCurrency -= OnNotEnoughCurrency;
        GameEvents.OnCharacterEquipped -= SetCharacter;
        GameEvents.OnAffectionLevelUp -= HandleAffectionLevelUp;
        GameEvents.OnPlayerTappedDetailed -= HandlePlayerTapped;
        GameEvents.OnGachaResult -= HandleGachaResult;
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
        bool isFever = feverManager != null && feverManager.isFeverActive;

        if (isFever)
        {
            _idleTimer = 0f;
        }
        else
        {
            _idleTimer += Time.deltaTime;
        }

        if (_resetTimer > 0f)
        {
            _resetTimer -= Time.deltaTime;
            if (_resetTimer <= 0f)
            {
                _rapidTapCount = 0;
                ApplyEmotion(isFever ? CharacterEmotion.Love : CharacterEmotion.Normal);
            }
            return;
        }

        // Базовое состояние, когда нет активной временной реакции
        CharacterEmotion baseEmotion = isFever
            ? CharacterEmotion.Love
            : (_idleTimer >= idleThresholdForSad ? CharacterEmotion.Sad : CharacterEmotion.Normal);

        if (_currentEmotion != baseEmotion)
        {
            ApplyEmotion(baseEmotion);
        }
    }

    public void SetCharacter(CharacterData newCharacter)
    {
        currentCharacter = newCharacter;
        _rapidTapCount = 0;
        _resetTimer = 0f;
        _idleTimer = 0f;

        if (newCharacter == null)
        {
            ResetToDefaultSprite();
            return;
        }

        bool isFever = feverManager != null && feverManager.isFeverActive;
        ApplyEmotion(isFever ? CharacterEmotion.Love : CharacterEmotion.Normal);
    }

    private void HandleAffectionLevelUp(CharacterData character, int newLevel)
    {
        if (currentCharacter == character)
        {
            _idleTimer = 0f;
            SetTemporaryEmotion(CharacterEmotion.Love, 2.0f);
        }
    }

    private void HandleGachaResult(string charName, Sprite sprite, Color color)
    {
        _idleTimer = 0f;
        SetTemporaryEmotion(CharacterEmotion.Love, 2.0f);
    }

    private void HandlePlayerTapped(double earnedPoints, bool isCrit, bool isFever, bool isManual)
    {
        if (!isManual) return;
        OnCharacterTapped();
    }

    public void OnCharacterTapped()
    {
        // Защита от двойного вызова, если метод также привязан в Button.onClick
        if (_lastProcessedTapFrame == Time.frameCount) return;
        _lastProcessedTapFrame = Time.frameCount;

        _idleTimer = 0f;

        if (feverManager != null && feverManager.isFeverActive) return;

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
        _idleTimer = 0f;
        SetTemporaryEmotion(CharacterEmotion.Sad, 1.2f);
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
        _idleTimer = 0f;
        if (characterImage != null && defaultClickerSprite != null)
            characterImage.sprite = defaultClickerSprite;
    }
}