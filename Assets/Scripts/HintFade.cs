using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class HintFade : MonoBehaviour
{
    [SerializeField] float showSeconds = 6f;
    [SerializeField] float fadeSeconds = 1f;
    CanvasGroup group;

    void Awake() => group = GetComponent<CanvasGroup>();

    void Update()
    {
        float t = Time.timeSinceLevelLoad;
        if (t > showSeconds + fadeSeconds)
        {
            gameObject.SetActive(false);
            return;
        }
        if (t > showSeconds) group.alpha = 1f - (t - showSeconds) / fadeSeconds;
    }
}
