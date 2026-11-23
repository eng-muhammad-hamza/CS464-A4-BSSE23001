using UnityEngine;

public class PowerUp : MonoBehaviour
{
    [SerializeField] float multiplier = 1.6f;
    [SerializeField] float duration = 5f;
    [SerializeField] ParticleSystem pickupFx;
    [SerializeField] AudioClip pickupClip;
    bool alreadyCollected;

    void OnTriggerEnter(Collider other)
    {
        if (alreadyCollected || !other.CompareTag("Player")) return;
        if (!other.TryGetComponent(out PlayerController controller)) return;
        alreadyCollected = true;

        Color color = JuiceFx.speedColor;
        if (pickupFx != null) Instantiate(pickupFx, transform.position, Quaternion.identity);
        else JuiceFx.burst(transform.position, color, 30, 5f, 0.18f, 0.7f);
        if (SfxPlayer.instance != null) SfxPlayer.instance.play(pickupClip);
        JuiceFx.ring(transform.position, color, 2.4f, 0.45f);
        JuiceFx.floatingText(transform.position + Vector3.up * 0.9f, $"SPEED {duration:0}s", color, 14f);

        controller.boostSpeed(multiplier, duration);
        Destroy(gameObject);
    }
}
