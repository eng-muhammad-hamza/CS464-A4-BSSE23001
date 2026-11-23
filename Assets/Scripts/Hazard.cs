using System.Collections;
using UnityEngine;

public class Hazard : MonoBehaviour
{
    [SerializeField] int damage = 1;
    [SerializeField] float knockback = 6f;
    [SerializeField] float knockTime = 0.25f;

    [Header("A crusher: give it a rest time and it strikes on a cycle (0 = always dangerous)")]
    [SerializeField] float restTime = 0f;
    [SerializeField] float warnTime = 0.7f;
    [SerializeField] float strikeTime = 0.4f;
    [SerializeField] float startDelay = 0f;
    [SerializeField] GameObject warning;

    [Header("Crusher animation (optional)")]
    [SerializeField] Transform piston;
    [SerializeField] float pistonRaisedLocalY = 3.5f;
    [SerializeField] float pistonSlammedLocalY = 0.4f;
    [SerializeField] float slamSeconds = 0.12f;
    [SerializeField] float riseSeconds = 0.6f;
    [SerializeField] float shakeSeconds = 0.3f;
    [SerializeField] float shakeMagnitude = 0.3f;

    public bool isResting { get; private set; }

    IEnumerator Start()
    {
        if (restTime <= 0f) yield break;
        Collider zone = GetComponent<Collider>();
        if (zone == null || warning == null)
        {
            Debug.LogWarning($"{name}: a crusher needs a Collider and a Warning object.", this);
            yield break;
        }

        setPistonLocalHeight(pistonRaisedLocalY);
        zone.enabled = false;
        warning.SetActive(false);
        if (startDelay > 0f) yield return new WaitForSeconds(startDelay);

        while (true)
        {
            zone.enabled = false;
            isResting = true;
            yield return new WaitForSeconds(restTime);
            isResting = false;

            warning.SetActive(true);
            yield return new WaitForSeconds(warnTime);
            warning.SetActive(false);

            zone.enabled = true;
            yield return movePistonHeight(pistonRaisedLocalY, pistonSlammedLocalY, slamSeconds, easeInQuadratic);
            if (CameraShake.instance != null) CameraShake.instance.shake(shakeSeconds, shakeMagnitude);
            Vector3 impactPoint = new Vector3(transform.position.x, transform.position.y + 0.15f, transform.position.z);
            JuiceFx.burst(impactPoint, new Color(0.8f, 0.75f, 0.7f), 36, 5f, 0.3f, 0.7f, false, 0.3f);
            JuiceFx.ring(impactPoint, JuiceFx.damageColor, 5f, 0.45f);
            float holdSeconds = piston == null ? strikeTime : Mathf.Max(0f, strikeTime - slamSeconds);
            yield return new WaitForSeconds(holdSeconds);

            zone.enabled = false;
            yield return movePistonHeight(pistonSlammedLocalY, pistonRaisedLocalY, riseSeconds, easeOutQuadratic);
        }
    }

    IEnumerator movePistonHeight(float fromLocalY, float toLocalY, float durationSeconds, System.Func<float, float> easing)
    {
        if (piston == null) yield break;
        if (durationSeconds <= 0f)
        {
            setPistonLocalHeight(toLocalY);
            yield break;
        }
        float elapsedSeconds = 0f;
        while (elapsedSeconds < durationSeconds)
        {
            elapsedSeconds += Time.deltaTime;
            float progress = easing(Mathf.Clamp01(elapsedSeconds / durationSeconds));
            setPistonLocalHeight(Mathf.Lerp(fromLocalY, toLocalY, progress));
            yield return null;
        }
        setPistonLocalHeight(toLocalY);
    }

    void setPistonLocalHeight(float localY)
    {
        if (piston == null) return;
        Vector3 localPosition = piston.localPosition;
        localPosition.y = localY;
        piston.localPosition = localPosition;
    }

    static float easeInQuadratic(float progress) => progress * progress;
    static float easeOutQuadratic(float progress) => 1f - (1f - progress) * (1f - progress);

    void OnTriggerStay(Collider other)
    {
        if (!enabled) return;
        Health health = other.GetComponentInParent<Health>();
        if (health == null) return;
        if (!health.takeDamage(damage)) return;

        Rigidbody body = other.attachedRigidbody;
        if (body == null) return;
        Vector3 away = body.position - transform.position;
        away.y = 0f;
        if (away.sqrMagnitude < 0.0001f) away = Vector3.forward;
        Vector3 push = away.normalized * knockback;

        if (body.TryGetComponent(out PlayerController controller)) controller.knockback(push, knockTime);
        else body.AddForce(push, ForceMode.Impulse);
    }
}
