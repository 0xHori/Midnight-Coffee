using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TapParticlePool : MonoBehaviour
{
    public Sprite particleSprite;
    public Transform canvasTransform;
    public int poolSize = 40;

    private struct ActiveParticle
    {
        public Image image;
        public RectTransform rect;
        public Vector3 velocity;
        public float elapsed;
        public float duration;
        public float startScale;
        public Color color;
    }

    private readonly Queue<Image> _pool = new Queue<Image>();
    private readonly List<ActiveParticle> _activeParticles = new List<ActiveParticle>();

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
            _pool.Enqueue(img);
        }
    }

    void OnDisable()
    {
        // Гарантированный возврат всех активных частиц в пул при выключении объекта
        for (int i = _activeParticles.Count - 1; i >= 0; i--)
        {
            Image img = _activeParticles[i].image;
            if (img != null)
            {
                img.gameObject.SetActive(false);
                _pool.Enqueue(img);
            }
        }
        _activeParticles.Clear();
    }

    public void SpawnBurst(Vector3 screenPos, bool isCrit)
    {
        int count = isCrit ? 8 : 4;
        for (int i = 0; i < count; i++)
        {
            if (_pool.Count == 0) break;

            Image img = _pool.Dequeue();
            img.gameObject.SetActive(true);
            img.transform.position = screenPos;

            Color col = isCrit
                ? new Color(1f, 0.85f, 0.2f, 1f)   // Золотые при крите (#FFD933)
                : new Color(1f, 0.45f, 0.65f, 1f); // Нежно-розовые при обычном тапе (#FF73A6)

            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float speed = Random.Range(140f, 280f);

            _activeParticles.Add(new ActiveParticle
            {
                image = img,
                rect = img.rectTransform,
                velocity = new Vector3(Mathf.Cos(angle) * speed, Mathf.Sin(angle) * speed + 80f, 0f),
                elapsed = 0f,
                duration = Random.Range(0.35f, 0.55f),
                startScale = Random.Range(0.7f, 1.2f),
                color = col
            });
        }
    }

    void Update()
    {
        if (_activeParticles.Count == 0) return;

        float dt = Time.deltaTime;
        for (int i = _activeParticles.Count - 1; i >= 0; i--)
        {
            ActiveParticle p = _activeParticles[i];
            p.elapsed += dt;

            if (p.elapsed >= p.duration)
            {
                p.image.gameObject.SetActive(false);
                _pool.Enqueue(p.image);
                _activeParticles.RemoveAt(i);
                continue;
            }

            float t = p.elapsed / p.duration;
            p.rect.position += p.velocity * dt;
            p.velocity.y -= 350f * dt;
            p.rect.localScale = Vector3.one * Mathf.Lerp(p.startScale, 0f, t);

            Color c = p.color;
            c.a = 1f - t;
            p.image.color = c;

            _activeParticles[i] = p;
        }
    }
}