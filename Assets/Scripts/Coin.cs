using UnityEngine;

public class Coin : MonoBehaviour
{
    [SerializeField] int value = 1;
    [SerializeField] float spinSpeed = 90f;
    [SerializeField] ParticleSystem pickupFx;
    [SerializeField] AudioClip pickupClip;
    bool alreadyCollected;

    void Update() => transform.Rotate(0f, spinSpeed * Time.deltaTime, 0f);

    void OnTriggerEnter(Collider other)
    {
        if (alreadyCollected || !other.CompareTag("Player")) return;
        alreadyCollected = true;

        if (pickupFx != null) Instantiate(pickupFx, transform.position, Quaternion.identity);
        else JuiceFx.burst(transform.position, JuiceFx.goldColor, 14, 4f, 0.14f, 0.5f);
        if (SfxPlayer.instance != null) SfxPlayer.instance.play(pickupClip);
        JuiceFx.ring(transform.position, JuiceFx.goldColor, 1.3f, 0.3f);
        JuiceFx.floatingText(transform.position + Vector3.up * 0.8f, $"+{value}", JuiceFx.goldColor);

        if (GameManager.instance != null) GameManager.instance.addScore(value);
        else Debug.LogError("No active GameManager in the scene.", this);
        Destroy(gameObject);
    }
}
