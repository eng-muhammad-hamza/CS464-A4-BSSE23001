using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField] Health health;
    [SerializeField] Image fill;
    [SerializeField] Image lag;
    [SerializeField] TMP_Text label;
    [SerializeField] float lagDelay = 0.4f;
    [SerializeField] float lagSpeed = 0.8f;

    float target = 1f;
    float lagValue = 1f;
    float lagWaitUntil;

    void OnEnable()
    {
        if (health == null) health = FindAnyObjectByType<Health>();
        if (health == null) { Debug.LogError("HealthBar: no Health in the scene.", this); return; }
        health.damaged += onChanged;
        health.healed += onChanged;
        health.restored += onChanged;
    }

    void OnDisable()
    {
        if (health == null) return;
        health.damaged -= onChanged;
        health.healed -= onChanged;
        health.restored -= onChanged;
    }

    void Start()
    {
        if (health == null) return;
        showHealth(health.current, health.maxHearts);
        lagValue = target;
        apply();
    }

    void onChanged(int current, int max) => showHealth(current, max);

    public void showHealth(int current, int max)
    {
        float wanted = max > 0 ? Mathf.Clamp01((float)current / max) : 0f;
        if (wanted < target) lagWaitUntil = Time.unscaledTime + lagDelay;
        else lagValue = Mathf.Max(lagValue, wanted);
        target = wanted;
        if (label != null) label.text = $"{Mathf.Max(current, 0)} / {max}";
        apply();
    }

    void Update()
    {
        if (lagValue > target && Time.unscaledTime >= lagWaitUntil)
        {
            lagValue = Mathf.MoveTowards(lagValue, target, lagSpeed * Time.unscaledDeltaTime);
            apply();
        }
    }

    void apply()
    {
        if (fill != null) fill.fillAmount = target;
        if (lag != null) lag.fillAmount = lagValue;
    }
}
