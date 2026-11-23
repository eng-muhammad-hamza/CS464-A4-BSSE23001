using UnityEngine;

public class HitSparks : MonoBehaviour
{
    [SerializeField] Health health;
    [SerializeField] ParticleSystem sparks;

    void Awake()
    {
        if (health == null) health = GetComponent<Health>();
    }

    void OnEnable() => health.damaged += onHit;
    void OnDisable() => health.damaged -= onHit;

    void onHit(int current, int max)
    {
        if (sparks != null) sparks.Play();
    }
}
