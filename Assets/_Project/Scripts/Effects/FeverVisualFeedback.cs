using UnityEngine;
using UnityEngine.UI;

public class FeverVisualFeedback : MonoBehaviour
{
    [Header("Ссылки")]
    public Transform canvasTransform;

    [Header("Настройки мягкой виньетки Fever")]
    public Color feverGlowColor = new Color(1f, 0.62f, 0.15f, 0.45f);
    [Tooltip("Толщина рамки по краям экрана (в долях от 0.05 до 0.35)")]
    [Range(0.05f, 0.35f)]
    public float borderThickness = 0.16f;
    public float pulseSpeed = 4f;
    public float fadeSpeed = 3.5f;

    private Image _vignetteImage;
    private float _currentAlpha = 0f;
    private bool _isFeverActive = false;

    void Start()
    {
        if (canvasTransform == null)
        {
            Canvas rootCanvas = FindAnyObjectByType<Canvas>();
            if (rootCanvas != null) canvasTransform = rootCanvas.transform;
        }

        CreateVignetteOverlay();
    }

    public void SetFeverActive(bool active)
    {
        _isFeverActive = active;
    }

    private void CreateVignetteOverlay()
    {
        if (canvasTransform == null) return;

        GameObject vigObj = new GameObject("FeverVignetteOverlay");
        vigObj.transform.SetParent(canvasTransform, false);

        // Ставим сразу после заднего фона (индекс 1), чтобы виньетка была поверх фона, но под персонажем и UI
        int targetIndex = canvasTransform.childCount > 1 ? 1 : 0;
        vigObj.transform.SetSiblingIndex(targetIndex);

        RectTransform vigRt = vigObj.AddComponent<RectTransform>();
        vigRt.anchorMin = Vector2.zero;
        vigRt.anchorMax = Vector2.one;
        vigRt.offsetMin = Vector2.zero;
        vigRt.offsetMax = Vector2.zero;

        _vignetteImage = vigObj.AddComponent<Image>();
        _vignetteImage.sprite = GenerateVignetteSprite(256, borderThickness);
        _vignetteImage.type = Image.Type.Simple;
        _vignetteImage.raycastTarget = false;
        _vignetteImage.color = new Color(feverGlowColor.r, feverGlowColor.g, feverGlowColor.b, 0f);
    }

    void Update()
    {
        if (_vignetteImage == null) return;

        float targetAlpha = 0f;
        if (_isFeverActive)
        {
            float pulse = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f;
            targetAlpha = Mathf.Lerp(feverGlowColor.a * 0.5f, feverGlowColor.a, pulse);
        }

        _currentAlpha = Mathf.MoveTowards(_currentAlpha, targetAlpha, Time.deltaTime * fadeSpeed);
        _vignetteImage.color = new Color(feverGlowColor.r, feverGlowColor.g, feverGlowColor.b, _currentAlpha);
    }

    private Sprite GenerateVignetteSprite(int size, float thickness)
    {
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;

        float borderPx = Mathf.Max(4f, size * thickness);
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float distX = Mathf.Min(x, size - 1 - x) / borderPx;
                float distY = Mathf.Min(y, size - 1 - y) / borderPx;
                float edgeFactor = 1f - Mathf.Clamp01(Mathf.Min(distX, distY));
                float alpha = Mathf.Pow(edgeFactor, 2.2f);
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }
        tex.Apply();

        return Sprite.Create(
            tex,
            new Rect(0, 0, size, size),
            new Vector2(0.5f, 0.5f),
            100f
        );
    }
}