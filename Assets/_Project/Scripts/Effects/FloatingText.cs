using System;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(TextMeshProUGUI))]
public class FloatingText : MonoBehaviour
{
    public float moveSpeed = 150f;
    public float fadeSpeed = 1.5f;

    private TextMeshProUGUI textMesh;
    private float alpha;
    private Vector3 moveDirection;
    private Action<FloatingText> onReturnToPool;

    void Awake()
    {
        textMesh = GetComponent<TextMeshProUGUI>();
        textMesh.raycastTarget = false; // Гарантируем, что текст не перехватывает клики
    }

    public void Initialize(Action<FloatingText> returnAction)
    {
        onReturnToPool = returnAction;
    }

    public void Play(string message, Color color, Vector3 startPosition, float scale = 1f)
    {
        transform.position = startPosition;
        transform.localScale = new Vector3(scale, scale, 1f);

        textMesh.text = message;
        textMesh.color = color;
        alpha = 1f;
        textMesh.alpha = 1f;

        float randomX = UnityEngine.Random.Range(-0.5f, 0.5f);
        moveDirection = new Vector3(randomX, 1f, 0f).normalized;

        gameObject.SetActive(true);
    }

    void Update()
    {
        transform.position += moveDirection * moveSpeed * Time.deltaTime;

        alpha -= fadeSpeed * Time.deltaTime;
        textMesh.alpha = alpha;

        if (alpha <= 0f)
        {
            gameObject.SetActive(false);
            onReturnToPool?.Invoke(this);
        }
    }
}