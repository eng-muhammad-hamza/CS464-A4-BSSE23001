using UnityEngine;

public class SfxPlayer : MonoBehaviour
{
    public static SfxPlayer instance { get; private set; }

    [SerializeField] float pitchSemitones = 2f;

    AudioSource[] voices;
    int next;

    void Awake()
    {
        if (instance != null) { Destroy(gameObject); return; }
        instance = this;
        DontDestroyOnLoad(gameObject);

        voices = GetComponents<AudioSource>();
        if (voices.Length == 0) Debug.LogError("SfxPlayer needs AudioSources on the same object (4 is plenty).", this);
        foreach (AudioSource voice in voices)
        {
            voice.playOnAwake = false;
            voice.spatialBlend = 0f;
            voice.ignoreListenerPause = true;
        }
    }

    void OnDestroy()
    {
        if (instance == this) instance = null;
    }

    public void play(AudioClip clip, float volume = 1f, bool varyPitch = true)
    {
        if (clip == null || voices == null || voices.Length == 0) return;
        AudioSource voice = voices[next];
        next = (next + 1) % voices.Length;
        float semitones = varyPitch ? Random.Range(-pitchSemitones, pitchSemitones) : 0f;
        voice.pitch = Mathf.Pow(2f, semitones / 12f);
        voice.PlayOneShot(clip, volume);
    }
}
