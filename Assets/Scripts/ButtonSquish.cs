using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class ButtonSquish : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public float squishAmount = 0.9f; // Насколько сжимается (0.9 = 90% от размера)
    public float squishSpeed = 0.05f; // Скорость сжатия
    private Vector3 originalScale;
    private Coroutine squishCoroutine;

    void Start()
    {
        // Запоминаем изначальный размер кнопки
        originalScale = transform.localScale;
    }

    // Вызывается, когда палец/мышь нажимает на кнопку
    public void OnPointerDown(PointerEventData eventData)
    {
        if (squishCoroutine != null) StopCoroutine(squishCoroutine);
        squishCoroutine = StartCoroutine(Squish(originalScale * squishAmount, squishSpeed));
    }

    // Вызывается, когда палец/мышь отпускает кнопку
    public void OnPointerUp(PointerEventData eventData)
    {
        if (squishCoroutine != null) StopCoroutine(squishCoroutine);
        squishCoroutine = StartCoroutine(Squish(originalScale, squishSpeed));
    }

    // Плавное изменение размера
    IEnumerator Squish(Vector3 targetScale, float duration)
    {
        float time = 0;
        Vector3 startScale = transform.localScale;
        while (time < duration)
        {
            transform.localScale = Vector3.Lerp(startScale, targetScale, time / duration);
            time += Time.deltaTime;
            yield return null;
        }
        transform.localScale = targetScale;
    }
}