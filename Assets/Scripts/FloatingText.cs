using UnityEngine;
using TMPro;

public class FloatingText : MonoBehaviour
{
    public float moveSpeed = 150f;   // Увеличили скорость
    public float fadeSpeed = 1.5f;   // Увеличили скорость исчезновения
    private TextMeshProUGUI textMesh;
    private float alpha = 1f;
    private Vector3 moveDirection;   // Направление полета

    void Start()
    {
        textMesh = GetComponent<TextMeshProUGUI>();
        
        // Задаем случайное направление: всегда вверх, но немного влево или вправо
        float randomX = Random.Range(-0.5f, 0.5f);
        moveDirection = new Vector3(randomX, 1f, 0).normalized; 
    }

    void Update()
    {
        // Двигаем текст по нашему случайному направлению
        transform.position += moveDirection * moveSpeed * Time.deltaTime;
        
        // Постепенно уменьшаем прозрачность
        alpha -= fadeSpeed * Time.deltaTime;
        textMesh.alpha = alpha;

        if (alpha <= 0)
        {
            Destroy(gameObject);
        }
    }
}