using TMPro;
using UnityEngine;

public static class JuiceFx
{
    public static readonly Color goldColor = new Color(1f, 0.82f, 0.2f);
    public static readonly Color shieldColor = new Color(0.55f, 0.95f, 1f);
    public static readonly Color speedColor = new Color(0.25f, 0.6f, 1f);
    public static readonly Color damageColor = new Color(1f, 0.25f, 0.25f);
    public static readonly Color heartColor = new Color(1f, 0.4f, 0.6f);
    public static readonly Color positiveColor = new Color(0.45f, 1f, 0.55f);
    public static readonly Color timeColor = new Color(0.72f, 0.5f, 1f);
    public static readonly Color ghostColor = new Color(0.9f, 0.95f, 1f);
    public static readonly Color empColor = new Color(1f, 0.55f, 0.15f);
    public static readonly Color keyColor = new Color(1f, 0.92f, 0.35f);
    public static readonly Color magnetColor = new Color(1f, 0.2f, 0.75f);
    public static readonly Color springColor = new Color(0.6f, 1f, 0.2f);
    public static readonly Color cellColor = new Color(0.25f, 1f, 0.55f);
    public static readonly Color dashColor = new Color(0.2f, 1f, 0.75f);
    public static readonly Color freezeColor = new Color(0.65f, 0.85f, 1f);
    public static readonly Color slowColor = new Color(0.95f, 0.95f, 0.6f);
    public static readonly Color portalColor = new Color(0.8f, 0.4f, 1f);

    static Material cachedParticleMaterial;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void resetStatics() => cachedParticleMaterial = null;

    public static Material particleMaterial
    {
        get
        {
            if (cachedParticleMaterial == null) cachedParticleMaterial = new Material(Shader.Find("Sprites/Default"));
            return cachedParticleMaterial;
        }
    }

    public static void burst(Vector3 worldPosition, Color color, int particleCount, float speed, float particleSize,
                             float lifetimeSeconds, bool useUnscaledTime = false, float gravityModifier = 0.6f)
    {
        GameObject effectObject = new GameObject("FxBurst");
        effectObject.transform.position = worldPosition;
        ParticleSystem particles = effectObject.AddComponent<ParticleSystem>();
        particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        ParticleSystem.MainModule main = particles.main;
        main.duration = 0.25f;
        main.loop = false;
        main.playOnAwake = false;
        main.startLifetime = new ParticleSystem.MinMaxCurve(lifetimeSeconds * 0.6f, lifetimeSeconds);
        main.startSpeed = new ParticleSystem.MinMaxCurve(speed * 0.4f, speed);
        main.startSize = new ParticleSystem.MinMaxCurve(particleSize * 0.5f, particleSize);
        main.startColor = color;
        main.gravityModifier = gravityModifier;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.useUnscaledTime = useUnscaledTime;
        main.stopAction = ParticleSystemStopAction.Destroy;

        ParticleSystem.EmissionModule emission = particles.emission;
        emission.rateOverTime = 0f;
        emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, (short)particleCount) });

        ParticleSystem.ShapeModule shape = particles.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.15f;

        applyFadeAndShrink(particles);
        effectObject.GetComponent<ParticleSystemRenderer>().sharedMaterial = particleMaterial;
        particles.Play();
    }

    public static ParticleSystem createTrailSparks(Transform parent, Color color)
    {
        GameObject sparksObject = new GameObject("TrailSparks");
        sparksObject.transform.SetParent(parent, false);
        ParticleSystem sparks = sparksObject.AddComponent<ParticleSystem>();
        sparks.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        ParticleSystem.MainModule main = sparks.main;
        main.loop = true;
        main.playOnAwake = false;
        main.startLifetime = 0.45f;
        main.startSpeed = 0.4f;
        main.startSize = new ParticleSystem.MinMaxCurve(0.1f, 0.22f);
        main.startColor = color;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        ParticleSystem.EmissionModule emission = sparks.emission;
        emission.rateOverTime = 0f;
        emission.rateOverDistance = 8f;

        ParticleSystem.ShapeModule shape = sparks.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.35f;

        applyFadeAndShrink(sparks);
        sparksObject.GetComponent<ParticleSystemRenderer>().sharedMaterial = particleMaterial;
        return sparks;
    }

    static void applyFadeAndShrink(ParticleSystem particles)
    {
        ParticleSystem.SizeOverLifetimeModule sizeModule = particles.sizeOverLifetime;
        sizeModule.enabled = true;
        sizeModule.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 1f, 1f, 0f));

        ParticleSystem.ColorOverLifetimeModule colorModule = particles.colorOverLifetime;
        colorModule.enabled = true;
        Gradient fadeOut = new Gradient();
        fadeOut.SetKeys(
            new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
            new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 0.55f), new GradientAlphaKey(0f, 1f) });
        colorModule.color = fadeOut;
    }

    public static void ring(Vector3 worldPosition, Color color, float maxRadius, float seconds, bool useUnscaledTime = false)
    {
        GameObject ringObject = new GameObject("FxRing");
        ringObject.transform.position = worldPosition;
        ringObject.AddComponent<ExpandingRing>().begin(color, maxRadius, seconds, useUnscaledTime);
    }

    public static void floatingText(Vector3 worldPosition, string message, Color color, float fontSize = 14f)
    {
        GameObject textObject = new GameObject("FloatingText");
        textObject.transform.position = worldPosition;
        TextMeshPro text = textObject.AddComponent<TextMeshPro>();
        text.text = message;
        text.fontSize = fontSize;
        text.color = color;
        text.alignment = TextAlignmentOptions.Center;
        text.fontStyle = FontStyles.Bold;
        text.sortingOrder = 30;
        textObject.AddComponent<FloatingTextMotion>();
    }
}

public class ExpandingRing : MonoBehaviour
{
    const int segmentCount = 48;
    LineRenderer line;
    Color ringColor;
    float maxRadius;
    float durationSeconds;
    float elapsedSeconds;
    bool useUnscaledTime;

    public void begin(Color color, float radius, float seconds, bool unscaled)
    {
        ringColor = color;
        maxRadius = radius;
        durationSeconds = Mathf.Max(0.05f, seconds);
        useUnscaledTime = unscaled;
        line = gameObject.AddComponent<LineRenderer>();
        line.useWorldSpace = false;
        line.loop = true;
        line.positionCount = segmentCount;
        line.sharedMaterial = JuiceFx.particleMaterial;
        apply(0f);
    }

    void Update()
    {
        elapsedSeconds += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
        float progress = Mathf.Clamp01(elapsedSeconds / durationSeconds);
        apply(progress);
        if (progress >= 1f) Destroy(gameObject);
    }

    void apply(float progress)
    {
        float easedProgress = 1f - (1f - progress) * (1f - progress);
        float radius = Mathf.Lerp(0.1f, maxRadius, easedProgress);
        for (int i = 0; i < segmentCount; i++)
        {
            float angle = i * Mathf.PI * 2f / segmentCount;
            line.SetPosition(i, new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius));
        }
        Color fadedColor = ringColor;
        fadedColor.a = 1f - progress;
        line.startColor = fadedColor;
        line.endColor = fadedColor;
        float width = Mathf.Lerp(0.18f, 0.02f, progress);
        line.startWidth = width;
        line.endWidth = width;
    }
}

public class FloatingTextMotion : MonoBehaviour
{
    const float textLifetimeSeconds = 1f;
    const float riseDistance = 1.8f;
    TextMeshPro text;
    Vector3 startPosition;
    Color startColor;
    float elapsedSeconds;

    void Start()
    {
        text = GetComponent<TextMeshPro>();
        startPosition = transform.position;
        startColor = text.color;
    }

    void Update()
    {
        elapsedSeconds += Time.unscaledDeltaTime;
        float progress = Mathf.Clamp01(elapsedSeconds / textLifetimeSeconds);
        Camera mainCamera = Camera.main;
        Vector3 screenUp = mainCamera != null ? mainCamera.transform.up : Vector3.up;

        float rise = 1f - Mathf.Pow(1f - progress, 3f);
        transform.position = startPosition + screenUp * (rise * riseDistance);
        if (mainCamera != null) transform.rotation = mainCamera.transform.rotation;

        float popScale = progress < 0.15f
            ? Mathf.Lerp(0.4f, 1.3f, progress / 0.15f)
            : Mathf.Lerp(1.3f, 1f, Mathf.Clamp01((progress - 0.15f) / 0.2f));
        transform.localScale = Vector3.one * popScale;

        Color fadedColor = startColor;
        fadedColor.a = progress < 0.6f ? 1f : 1f - (progress - 0.6f) / 0.4f;
        text.color = fadedColor;

        if (progress >= 1f) Destroy(gameObject);
    }
}
