using UnityEngine;

public class HeartPickup : MonoBehaviour
{
    [SerializeField] int amount = 1;
    [SerializeField] ParticleSystem pickupFx;
    [SerializeField] AudioClip pickupClip;
    bool collected;

    void OnTriggerStay(Collider other)
    {
        if (collected || !other.CompareTag("Player")) return;
        if (!other.TryGetComponent(out Health health)) return;
        if (!health.heal(amount)) return;

        collected = true;
        if (pickupFx != null) Instantiate(pickupFx, transform.position, Quaternion.identity);
        else JuiceFx.burst(transform.position, JuiceFx.heartColor, 20, 4f, 0.16f, 0.5f);
        if (SfxPlayer.instance != null) SfxPlayer.instance.play(pickupClip);
        JuiceFx.ring(transform.position, JuiceFx.heartColor, 1.8f, 0.35f);
        Destroy(gameObject);
    }
}
