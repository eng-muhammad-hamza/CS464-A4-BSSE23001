using UnityEngine;

public class HitSfx : MonoBehaviour
{
    [SerializeField] Health health;
    [SerializeField] AudioClip hitClip;
    [SerializeField] AudioClip deathClip;

    void Awake()
    {
        if (health == null) health = GetComponent<Health>();
    }

    void OnEnable()
    {
        health.damaged += onHit;
        health.died += onDied;
    }

    void OnDisable()
    {
        health.damaged -= onHit;
        health.died -= onDied;
    }

    void onHit(int current, int max)
    {
        if (current > 0 && SfxPlayer.instance != null) SfxPlayer.instance.play(hitClip);
    }

    void onDied()
    {
        if (SfxPlayer.instance != null) SfxPlayer.instance.play(deathClip);
    }
}
