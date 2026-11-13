using UnityEngine;
using UnityEngine.InputSystem;

public class TapFeedbackController : MonoBehaviour
{
    [Header("Звуки")]
    public AudioSource audioSource;
    public AudioClip clickSound;
    public AudioClip critSound;
    public AudioClip prestigeSound;

    [Header("Пулы эффектов")]
    public FloatingTextPool floatingTextPool;
    public TapParticlePool tapParticlePool;

    void OnEnable()
    {
        GameEvents.OnPlayerTapped += HandlePlayerTapped;
        GameEvents.OnFeverTriggered += HandleFeverTriggered;
        GameEvents.OnPrestigeCompleted += HandlePrestigeCompleted;
    }

    void OnDisable()
    {
        GameEvents.OnPlayerTapped -= HandlePlayerTapped;
        GameEvents.OnFeverTriggered -= HandleFeverTriggered;
        GameEvents.OnPrestigeCompleted -= HandlePrestigeCompleted;
    }

    private void HandlePlayerTapped(double earnedPoints, bool isCrit, bool isFever)
    {
        PlayTapAudio(isCrit, isFever);
        SpawnTapVisuals(earnedPoints, isCrit, isFever);
    }

    private void HandleFeverTriggered()
    {
        if (floatingTextPool == null) return;
        Vector3 centerPos = new Vector3(Screen.width / 2f, Screen.height * 0.65f, 0);
        floatingTextPool.SpawnText("FEVER MODE x3!", new Color(1f, 0.65f, 0f), centerPos, 2.2f);
    }

    private void HandlePrestigeCompleted(double crystalsGained)
    {
        if (audioSource != null && prestigeSound != null)
        {
            audioSource.pitch = 0.8f;
            audioSource.PlayOneShot(prestigeSound);
        }

        if (floatingTextPool != null)
        {
            Vector3 centerPos = new Vector3(Screen.width / 2f, Screen.height / 2f, 0);
            floatingTextPool.SpawnText(
                $"ПРЕСТИЖ! +{crystalsGained.ToCompactString()} КРИСТАЛЛОВ",
                new Color(0.8f, 0.2f, 1f),
                centerPos,
                2.5f
            );
        }
    }

    private void PlayTapAudio(bool isCrit, bool isFever)
    {
        if (audioSource == null) return;

        if (isCrit && critSound != null)
        {
            audioSource.pitch = isFever ? 1.15f : 1f;
            audioSource.PlayOneShot(critSound);
        }
        else if (!isCrit && clickSound != null)
        {
            float basePitch = isFever ? 1.15f : 1.0f;
            audioSource.pitch = Random.Range(basePitch - 0.1f, basePitch + 0.1f);
            audioSource.PlayOneShot(clickSound);
        }
    }

    private void SpawnTapVisuals(double earnedPoints, bool isCrit, bool isFever)
    {
        if (floatingTextPool == null || Pointer.current == null) return;

        Vector2 pointerPos = Pointer.current.position.ReadValue();
        Vector3 screenPos = new Vector3(pointerPos.x, pointerPos.y, 0);

        if (tapParticlePool != null)
            tapParticlePool.SpawnBurst(screenPos, isCrit || isFever);

        screenPos.y += 20f;
        screenPos.x += Random.Range(-150f, 150f);
        screenPos.y += Random.Range(-30f, 40f);

        if (isCrit)
        {
            floatingTextPool.SpawnText($"КРИТ! +{earnedPoints.ToCompactString()}", Color.red, screenPos, 1.5f);
        }
        else
        {
            Color textColor = isFever ? new Color(1f, 0.85f, 0.2f) : Color.white;
            floatingTextPool.SpawnText($"+{earnedPoints.ToCompactString()}", textColor, screenPos, isFever ? 1.15f : 1f);
        }
    }
}