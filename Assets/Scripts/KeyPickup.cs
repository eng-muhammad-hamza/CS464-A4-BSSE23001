using UnityEngine;

public class KeyPickup : MonoBehaviour
{
    [SerializeField] Color color = JuiceFx.keyColor;
    [SerializeField] ParticleSystem pickupFx;
    [SerializeField] AudioClip pickupClip;
    bool collected;

    void OnTriggerEnter(Collider other)
    {
        if (collected || !other.CompareTag("Player")) return;
        collected = true;
        if (pickupFx != null) Instantiate(pickupFx, transform.position, Quaternion.identity);
        else JuiceFx.burst(transform.position, color, 26, 5f, 0.16f, 0.6f);
        if (SfxPlayer.instance != null) SfxPlayer.instance.play(pickupClip);
        JuiceFx.ring(transform.position, color, 2.2f, 0.45f);
        if (GameManager.instance != null) GameManager.instance.addKey();
        Destroy(gameObject);
    }
}
