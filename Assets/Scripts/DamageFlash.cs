using UnityEngine;
using UnityEngine.UI;

public class DamageFlash : MonoBehaviour
{
    [SerializeField] Health health;
    [SerializeField] Image flash;
    [SerializeField, Range(0f, 1f)] float peakAlpha = 0.35f;
    [SerializeField] float seconds = 0.25f;

    float remaining;

    void OnEnable()
    {
        if (health == null) health = FindAnyObjectByType<Health>();
        if (health == null) return;
        health.damaged += onHit;
    }

    void OnDisable()
    {
        if (health != null) health.damaged -= onHit;
    }

    void Start() => setAlpha(0f);

    void onHit(int current, int max) => remaining = seconds;

    void Update()
    {
        if (remaining <= 0f) return;
        remaining -= Time.unscaledDeltaTime;
        setAlpha(peakAlpha * Mathf.Clamp01(remaining / seconds));
    }

    void setAlpha(float alpha)
    {
        if (flash == null) return;
        Color c = flash.color;
        c.a = alpha;
        flash.color = c;
    }
}
