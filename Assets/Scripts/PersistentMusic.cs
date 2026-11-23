using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class PersistentMusic : MonoBehaviour
{
    static PersistentMusic instance;

    [SerializeField] AudioClip clip;
    [SerializeField, Range(0f, 1f)] float volume = 1f;
    [SerializeField] float fadeSeconds = 0.6f;

    AudioSource source;
    Coroutine fade;

    void Awake()
    {
        if (instance != null)
        {
            instance.playTrack(clip);
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);

        source = GetComponent<AudioSource>();
        source.loop = true;
        source.playOnAwake = false;
        source.spatialBlend = 0f;
        source.ignoreListenerPause = true;
        source.volume = 0f;
    }

    void Start()
    {
        if (instance != this) return;
        if (clip != null) playTrack(clip);
    }

    void OnDestroy()
    {
        if (instance == this) instance = null;
    }

    void playTrack(AudioClip next)
    {
        if (next == null || source == null) return;
        if (source.clip == next && source.isPlaying) return;
        if (fade != null) StopCoroutine(fade);
        fade = StartCoroutine(fadeTo(next));
    }

    IEnumerator fadeTo(AudioClip next)
    {
        float from = source.volume;
        if (source.isPlaying && fadeSeconds > 0f)
            for (float t = 0f; t < fadeSeconds; t += Time.unscaledDeltaTime)
            {
                source.volume = Mathf.Lerp(from, 0f, t / fadeSeconds);
                yield return null;
            }
        source.Stop();
        source.clip = next;
        source.volume = 0f;
        source.Play();
        for (float t = 0f; t < fadeSeconds; t += Time.unscaledDeltaTime)
        {
            source.volume = Mathf.Lerp(0f, volume, t / fadeSeconds);
            yield return null;
        }
        source.volume = volume;
        fade = null;
    }
}
