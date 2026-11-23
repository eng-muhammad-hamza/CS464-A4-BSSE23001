using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [SerializeField] Renderer flagRenderer;
    [SerializeField] int healAmount = 1;
    [SerializeField] AudioClip reachedClip;
    static readonly Color inactiveColor = new Color(0.55f, 0.58f, 0.65f);
    bool reached;

    void Start()
    {
        if (flagRenderer != null) flagRenderer.material.color = inactiveColor;
    }

    void OnTriggerEnter(Collider other)
    {
        if (reached || !other.CompareTag("Player")) return;
        if (GameManager.instance == null) return;
        reached = true;

        GameManager.instance.setCheckpoint(transform);
        if (SfxPlayer.instance != null) SfxPlayer.instance.play(reachedClip);
        if (flagRenderer != null) flagRenderer.material.color = JuiceFx.positiveColor;
        if (healAmount > 0 && other.TryGetComponent(out Health health)) health.heal(healAmount);

        Vector3 center = transform.position + Vector3.up * 0.3f;
        JuiceFx.ring(center, JuiceFx.positiveColor, 4f, 0.6f);
        JuiceFx.burst(center + Vector3.up, JuiceFx.positiveColor, 30, 5f, 0.18f, 0.7f);
        JuiceFx.floatingText(center + Vector3.up * 2f, "CHECKPOINT", JuiceFx.positiveColor, 18f);
    }
}
