using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TapParticlePool : MonoBehaviour
{
    public Sprite particleSprite; // Перетащи сюда спрайт сердечка, звёздочки или стандартный Knob/UISprite
    public Transform canvasTransform;
    public int poolSize = 40;

    private Queue<Image> pool = new Queue<Image>();

    void Awake()
    {
        if (canvasTransform == null) return;

        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = new GameObject("TapParticle");
            obj.transform.SetParent(canvasTransform, false);
            Image img = obj.AddComponent<Image>();
            img.sprite = particleSprite;
            img.raycastTarget = false;
            obj.GetComponent<RectTransform>().sizeDelta = new Vector2(28, 28);
            obj.SetActive(false);
            pool.Enqueue(img);
        }
    }

    public void SpawnBurst(Vector3 screenPos, bool isCrit)
    {
        int count = isCrit ? 8 : 4;
        for (int i = 0; i < count; i++)
        {
            if (pool.Count == 0) break;
            Image img = pool.Dequeue();
            img.gameObject.SetActive(true);
            img.transform.position = screenPos;
            img.transform.SetAsLastSibling();

            Color col = isCrit
                ? new Color(1f, 0.85f, 0.2f, 1f)   // Золотые при крите
                : new Color(1f, 0.45f, 0.65f, 1f); // Нежно-розовые при обычном тапе

            StartCoroutine(AnimateParticle(img, col));
        }
    }

    IEnumerator AnimateParticle(Image img, Color startColor)
    {
        float duration = Random.Range(0.35f, 0.55f);
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        float speed = Random.Range(140f, 280f);
        Vector3 velocity = new Vector3(Mathf.Cos(angle) * speed, Mathf.Sin(angle) * speed + 80f, 0);

        float time = 0f;
        float startScale = Random.Range(0.7f, 1.2f);

        while (time < duration)
        {
            float t = time / duration;
            img.transform.position += velocity * Time.deltaTime;
            velocity.y -= 350f * Time.deltaTime; // Лёгкая гравитация
            img.transform.localScale = Vector3.one * Mathf.Lerp(startScale, 0f, t);

            Color c = startColor;
            c.a = 1f - t;
            img.color = c;

            time += Time.deltaTime;
            yield return null;
        }

        img.gameObject.SetActive(false);
        pool.Enqueue(img);
    }
}