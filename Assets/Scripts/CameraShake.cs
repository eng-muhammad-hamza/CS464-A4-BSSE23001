using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public static CameraShake instance { get; private set; }

    public Vector3 restingLocalPosition { get; set; }

    float shakeSecondsLeft;
    float shakeTotalSeconds;
    float shakeMagnitude;

    void Awake()
    {
        instance = this;
        restingLocalPosition = transform.localPosition;
    }

    public void shake(float durationSeconds, float magnitude)
    {
        if (durationSeconds <= 0f || magnitude <= 0f) return;
        if (magnitude < currentShakeStrength()) return;
        shakeSecondsLeft = durationSeconds;
        shakeTotalSeconds = durationSeconds;
        shakeMagnitude = magnitude;
    }

    float currentShakeStrength() =>
        shakeTotalSeconds <= 0f ? 0f : shakeMagnitude * Mathf.Max(0f, shakeSecondsLeft / shakeTotalSeconds);

    void LateUpdate()
    {
        Vector3 shakeOffset = Vector3.zero;
        if (shakeSecondsLeft > 0f)
        {
            shakeSecondsLeft -= Time.unscaledDeltaTime;
            Vector2 randomOffset = Random.insideUnitCircle * currentShakeStrength();
            shakeOffset = new Vector3(randomOffset.x, randomOffset.y, 0f);
        }
        transform.localPosition = restingLocalPosition + shakeOffset;
    }
}
