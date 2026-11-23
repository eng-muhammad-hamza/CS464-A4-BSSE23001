using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Health))]
[RequireComponent(typeof(PlayerController))]
public class PlayerJuice : MonoBehaviour
{
    [Header("Tuning")]
    [SerializeField] float hitStopSeconds = 0.06f;
    [SerializeField] float boostEndingWarningSeconds = 1.2f;
    [SerializeField] float invulnerabilityBlinkRate = 14f;

    Health health;
    PlayerController controller;
    Renderer bodyRenderer;
    Material bodyMaterial;
    Color baseBodyColor;
    TrailRenderer speedTrail;
    ParticleSystem speedSparks;
    Color flashColor;
    float flashSecondsLeft;
    float flashTotalSeconds;
    bool isDead;

    void Awake()
    {
        health = GetComponent<Health>();
        controller = GetComponent<PlayerController>();
        bodyRenderer = GetComponent<Renderer>();
        if (bodyRenderer == null) bodyRenderer = GetComponentInChildren<Renderer>();
        bodyMaterial = bodyRenderer.material;
        baseBodyColor = bodyMaterial.color;
        buildSpeedEffects();
    }

    void buildSpeedEffects()
    {
        speedTrail = gameObject.AddComponent<TrailRenderer>();
        speedTrail.time = 0.4f;
        speedTrail.startWidth = 0.8f;
        speedTrail.endWidth = 0f;
        speedTrail.minVertexDistance = 0.05f;
        speedTrail.sharedMaterial = JuiceFx.particleMaterial;
        Color trailColor = JuiceFx.speedColor;
        speedTrail.startColor = new Color(trailColor.r, trailColor.g, trailColor.b, 0.85f);
        speedTrail.endColor = new Color(trailColor.r, trailColor.g, trailColor.b, 0f);
        speedTrail.emitting = false;

        speedSparks = JuiceFx.createTrailSparks(transform, JuiceFx.speedColor);
    }

    void OnDisable()
    {
        if (Time.timeScale < 1f && !isGameFinished()) Time.timeScale = 1f;
    }

    public void tookDamage(int heartsLost)
    {
        Vector3 position = transform.position;
        JuiceFx.ring(position, JuiceFx.damageColor, 2.6f, 0.4f);
        JuiceFx.floatingText(position + Vector3.up * 1.2f, $"-{heartsLost}", JuiceFx.damageColor, 18f);
        shakeCamera(0.28f, 0.35f);
        startBodyFlash(JuiceFx.damageColor, 0.25f);
        StartCoroutine(hitStopRoutine(hitStopSeconds));
    }

    public void healed(int heartsGained)
    {
        JuiceFx.burst(transform.position, JuiceFx.positiveColor, 24, 5f, 0.18f, 0.6f);
        JuiceFx.floatingText(transform.position + Vector3.up * 1.2f, $"+{heartsGained}", JuiceFx.positiveColor, 18f);
    }

    public void died()
    {
        isDead = true;
        speedTrail.emitting = false;
        Vector3 position = transform.position;
        JuiceFx.burst(position, JuiceFx.damageColor, 60, 8f, 0.25f, 0.9f);
        JuiceFx.burst(position, Color.white, 20, 5f, 0.15f, 0.6f);
        JuiceFx.ring(position, JuiceFx.damageColor, 4.5f, 0.55f);
        shakeCamera(0.5f, 0.6f);
        StartCoroutine(hitStopRoutine(hitStopSeconds * 2f));
    }

    public void respawned()
    {
        isDead = false;
        speedTrail.Clear();
        Vector3 position = transform.position;
        JuiceFx.burst(position, Color.white, 28, 6f, 0.18f, 0.6f);
        JuiceFx.ring(position, Color.white, 3.5f, 0.5f);
    }

    public void speedBoostStarted()
    {
        speedTrail.emitting = true;
        speedSparks.Play();
        JuiceFx.ring(transform.position, JuiceFx.speedColor, 3f, 0.4f);
    }

    public void speedBoostEnded()
    {
        speedTrail.emitting = false;
        speedSparks.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        JuiceFx.burst(transform.position, JuiceFx.speedColor, 14, 3f, 0.14f, 0.4f);
    }

    void Update()
    {
        bool blinkHidden = health.isInvulnerable && Mathf.FloorToInt(Time.time * invulnerabilityBlinkRate) % 2 == 0;
        bodyRenderer.enabled = !isDead && !blinkHidden;

        Color finalColor = baseBodyColor;
        if (controller.isSpeedBoosted)
        {
            float blend = 0.5f + 0.25f * Mathf.Sin(Time.time * 12f);
            if (controller.speedBoostSecondsRemaining < boostEndingWarningSeconds)
                blend *= Mathf.Repeat(Time.time * 8f, 1f) < 0.5f ? 0f : 1f;
            finalColor = Color.Lerp(baseBodyColor, JuiceFx.speedColor, blend);
        }
        if (flashSecondsLeft > 0f)
        {
            flashSecondsLeft -= Time.unscaledDeltaTime;
            finalColor = Color.Lerp(finalColor, flashColor, Mathf.Clamp01(flashSecondsLeft / flashTotalSeconds));
        }
        bodyMaterial.color = finalColor;
    }

    void startBodyFlash(Color color, float seconds)
    {
        flashColor = color;
        flashTotalSeconds = Mathf.Max(0.05f, seconds);
        flashSecondsLeft = flashTotalSeconds;
    }

    static void shakeCamera(float seconds, float magnitude)
    {
        if (CameraShake.instance != null) CameraShake.instance.shake(seconds, magnitude);
    }

    IEnumerator hitStopRoutine(float seconds)
    {
        if (seconds <= 0f || isGameFinished() || PauseMenu.isPaused) yield break;
        Time.timeScale = 0.05f;
        yield return new WaitForSecondsRealtime(seconds);
        Time.timeScale = isGameFinished() || PauseMenu.isPaused ? 0f : 1f;
    }

    static bool isGameFinished() => GameManager.instance != null && GameManager.instance.isFinished;
}
