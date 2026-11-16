using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AffectionUI : MonoBehaviour
{
    [Header("Ссылки на менеджеры")]
    public GachaManager gachaManager;
    public AffectionManager affectionManager;

    [Header("Шкала близости (Affection Bar)")]
    public GameObject barRoot;
    public Image fillBar;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI bonusText;

    [Header("Облачко реплики (Speech Bubble)")]
    public CanvasGroup speechBubbleGroup;
    public TextMeshProUGUI speechText;
    [Tooltip("Сколько секунд висит облачко с репликой")]
    public float speechDisplayDuration = 3.5f;

    private float _speechTimer = 0f;
    private bool _isBubbleVisible = false;

    void OnEnable()
    {
        GameEvents.OnUpdateUI += RefreshBar;
        GameEvents.OnPlayerTapped += HandleTapped;
        GameEvents.OnCharacterEquipped += HandleCharacterChanged;
        GameEvents.OnCharacterSpeak += ShowSpeech;
    }

    void OnDisable()
    {
        GameEvents.OnUpdateUI -= RefreshBar;
        GameEvents.OnPlayerTapped -= HandleTapped;
        GameEvents.OnCharacterEquipped -= HandleCharacterChanged;
        GameEvents.OnCharacterSpeak -= ShowSpeech;
    }

    void Start()
    {
        if (speechBubbleGroup != null)
        {
            speechBubbleGroup.alpha = 0f;
            speechBubbleGroup.blocksRaycasts = false;
        }
        RefreshBar();
    }

    void Update()
    {
        if (speechBubbleGroup == null) return;

        if (_isBubbleVisible)
        {
            _speechTimer -= Time.deltaTime;
            speechBubbleGroup.alpha = Mathf.MoveTowards(speechBubbleGroup.alpha, 1f, Time.deltaTime * 8f);
            if (_speechTimer <= 0f)
                _isBubbleVisible = false;
        }
        else if (speechBubbleGroup.alpha > 0f)
        {
            speechBubbleGroup.alpha = Mathf.MoveTowards(speechBubbleGroup.alpha, 0f, Time.deltaTime * 4f);
        }
    }

    private void HandleTapped(double earned, bool isCrit, bool isFever) => RefreshBar();
    private void HandleCharacterChanged(CharacterData character) => RefreshBar();

    public void RefreshBar()
    {
        if (gachaManager == null || affectionManager == null) return;

        CharacterData equipped = gachaManager.GetEquippedCharacter();
        if (equipped == null)
        {
            if (barRoot != null) barRoot.SetActive(false);
            return;
        }

        if (barRoot != null) barRoot.SetActive(true);

        int lvl = affectionManager.GetAffectionLevel(equipped.characterId);
        float progress = affectionManager.GetNormalizedProgress(equipped);
        double bonusPercent = (equipped.GetAffectionMultiplier(lvl, affectionManager.balanceConfig) - 1.0) * 100.0;

        if (fillBar != null)
            fillBar.fillAmount = progress;

        if (levelText != null)
            levelText.text = lvl >= equipped.maxAffectionLevel ? "♥ MAX" : $"♥ Ур. {lvl}";

        if (bonusText != null)
            bonusText.text = bonusPercent > 0 ? $"+{bonusPercent:F0}% к чаевым" : "+0% к чаевым";
    }

    public void ShowSpeech(string quote)
    {
        if (string.IsNullOrEmpty(quote) || speechText == null || speechBubbleGroup == null) return;

        speechText.text = quote;
        _speechTimer = speechDisplayDuration;
        _isBubbleVisible = true;
    }
}