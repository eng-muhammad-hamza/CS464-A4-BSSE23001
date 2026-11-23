using UnityEngine;

public class TimePickup : MonoBehaviour
{
    [SerializeField] float seconds = 5f;
    [SerializeField] ParticleSystem pickupFx;
    [SerializeField] AudioClip pickupClip;
    bool collected;

    void OnTriggerEnter(Collider other)
    {
        if (collected || !other.CompareTag("Player")) return;
        collected = true;
        Color c = JuiceFx.timeColor;
        if (pickupFx != null) Instantiate(pickupFx, transform.position, Quaternion.identity);
        else JuiceFx.burst(transform.position, c, 24, 5f, 0.16f, 0.6f);
        if (SfxPlayer.instance != null) SfxPlayer.instance.play(pickupClip);
        JuiceFx.ring(transform.position, c, 2f, 0.4f);
        JuiceFx.floatingText(transform.position + Vector3.up * 0.8f, $"+{seconds:0}s", c, 16f);
        if (GameManager.instance != null) GameManager.instance.addTime(seconds);
        Destroy(gameObject);
    }
}
