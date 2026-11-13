using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FeverManager : MonoBehaviour
{
    [Header("Конфиг баланса")]
    public BalanceConfig balanceConfig;

    [Header("Настройки баланса (если не задан BalanceConfig)")]
    public float chargePerTap = 0.03f;
    public float decayDelay = 0.8f;
    public float decaySpeed = 0.25f;
    public float feverDuration = 10f;
    public double feverMultiplier = 3.0;

    [Header("UI Элементы")]
    public Image feverFillImage;
    public TextMeshProUGUI feverStatusText;
    public Color normalBarColor = new Color(0.22f, 0.74f, 0.97f, 1f);
    public Color feverBarColor = new Color(1f, 0.55f, 0.0f, 1f);

    [Header("Состояние (только чтение)")]
    public bool isFeverActive = false;
    [Range(0f, 1f)] public float currentCharge = 0f;

    private float timeSinceLastTap = 0f;
    private float feverTimer = 0f;

    public double CurrentMultiplier => isFeverActive ? feverMultiplier : 1.0;

    void Awake()
    {
        if (balanceConfig != null)
        {
            chargePerTap = balanceConfig.feverChargePerTap;
            decayDelay = balanceConfig.feverDecayDelay;
            decaySpeed = balanceConfig.feverDecaySpeed;
            feverDuration = balanceConfig.feverDuration;
            feverMultiplier = balanceConfig.feverMultiplier;
        }
    }

    void Start()
    {
        UpdateFeverUI();
    }

    void Update()
    {
        if (isFeverActive)
        {
            feverTimer -= Time.deltaTime;
            currentCharge = Mathf.Clamp01(feverTimer / feverDuration);

            if (feverTimer <= 0f)
            {
                EndFever();
            }
            UpdateFeverUI();
        }
        else if (currentCharge > 0f)
        {
            timeSinceLastTap += Time.deltaTime;
            if (timeSinceLastTap >= decayDelay)
            {
                currentCharge = Mathf.Max(0f, currentCharge - decaySpeed * Time.deltaTime);
                UpdateFeverUI();
            }
        }
    }

    public void RegisterTap()
    {
        timeSinceLastTap = 0f;
        if (isFeverActive) return;

        currentCharge = Mathf.Clamp01(currentCharge + chargePerTap);
        if (currentCharge >= 1f)
            StartFever();
        else
            UpdateFeverUI();
    }

    private void StartFever()
    {
        isFeverActive = true;
        feverTimer = feverDuration;
        currentCharge = 1f;
        AudioManager.TriggerHaptic();
        UpdateFeverUI();
        GameEvents.UpdateUI();
    }

    private void EndFever()
    {
        isFeverActive = false;
        currentCharge = 0f;
        UpdateFeverUI();
        GameEvents.UpdateUI();
    }

    private void UpdateFeverUI()
    {
        if (feverFillImage != null)
        {
            feverFillImage.rectTransform.anchorMax = new Vector2(currentCharge, 1f);
            feverFillImage.color = isFeverActive ? feverBarColor : normalBarColor;
        }

        if (feverStatusText != null)
        {
            if (isFeverActive)
            {
                feverStatusText.text = $"FEVER x{feverMultiplier:0.#}! ({feverTimer:0.0}с)";
                feverStatusText.color = Color.white;
            }
            else if (currentCharge > 0.01f)
            {
                feverStatusText.text = $"КОМБО: {Mathf.RoundToInt(currentCharge * 100f)}%";
                feverStatusText.color = Color.white;
            }
            else
            {
                feverStatusText.text = "ТАПАЙ ДЛЯ FEVER";
                feverStatusText.color = new Color(1f, 1f, 1f, 0.55f);
            }
        }
    }
}