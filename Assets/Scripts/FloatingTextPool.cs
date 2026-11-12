using System.Collections.Generic;
using UnityEngine;

public class FloatingTextPool : MonoBehaviour
{
    [Header("Настройки пула")]
    public GameObject floatingTextPrefab;
    public Transform canvasTransform;
    public int initialPoolSize = 25;

    private readonly Queue<FloatingText> pool = new Queue<FloatingText>();

    void Awake()
    {
        WarmupPool();
    }

    private void WarmupPool()
    {
        if (floatingTextPrefab == null || canvasTransform == null) return;

        for (int i = 0; i < initialPoolSize; i++)
        {
            CreateNewInstance();
        }
    }

    private FloatingText CreateNewInstance()
    {
        GameObject obj = Instantiate(floatingTextPrefab, canvasTransform);
        obj.SetActive(false);

        FloatingText ft = obj.GetComponent<FloatingText>();
        if (ft == null)
        {
            ft = obj.AddComponent<FloatingText>();
        }

        ft.Initialize(ReturnToPool);
        pool.Enqueue(ft);
        return ft;
    }

    private void ReturnToPool(FloatingText ft)
    {
        pool.Enqueue(ft);
    }

    public void SpawnText(string message, Color color, Vector3 position, float scale = 1f)
    {
        if (floatingTextPrefab == null || canvasTransform == null) return;

        FloatingText ft = pool.Count > 0 ? pool.Dequeue() : CreateNewInstance();
        ft.Play(message, color, position, scale);
    }
}