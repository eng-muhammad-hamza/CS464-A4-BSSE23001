using UnityEngine;
using UnityEngine.Audio;

[RequireComponent(typeof(AudioSource))]
public class WorldSound : MonoBehaviour
{
    [SerializeField] AudioClip clip;
    [SerializeField] AudioMixerGroup output;
    [SerializeField, Range(0f, 1f)] float volume = 1f;
    [SerializeField] bool loop = true;
    [Header("3D Sound Settings")]
    [SerializeField] AudioRolloffMode rolloff = AudioRolloffMode.Logarithmic;
    [SerializeField] float minDistance = 2f;
    [SerializeField] float maxDistance = 20f;

    void Reset() => apply();
    void OnValidate() => apply();
    void Awake() => apply();

    void apply()
    {
        AudioSource source = GetComponent<AudioSource>();
        if (source == null) return;
        if (clip != null) source.clip = clip;
        if (output != null) source.outputAudioMixerGroup = output;
        source.volume = volume;
        source.loop = loop;
        source.playOnAwake = true;
        source.spatialBlend = 1f;
        source.rolloffMode = rolloff;
        source.minDistance = Mathf.Max(0.01f, minDistance);
        source.maxDistance = Mathf.Max(source.minDistance + 0.01f, maxDistance);
        source.dopplerLevel = 0f;
        source.spread = 0f;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, minDistance);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, maxDistance);
    }
}
