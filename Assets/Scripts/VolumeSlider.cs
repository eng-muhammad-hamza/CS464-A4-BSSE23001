using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class VolumeSlider : MonoBehaviour
{
    public enum Channel { Music, Sfx }

    [SerializeField] AudioMixer mixer;
    [SerializeField] Channel channel = Channel.Music;
    [SerializeField] Slider slider;

    const float minSliderValue = 0.0001f;

    string parameterKey => channel == Channel.Music ? SaveData.musicVolumeKey : SaveData.sfxVolumeKey;

    void Start()
    {
        slider.minValue = minSliderValue;
        slider.maxValue = 1f;
        float saved = SaveData.getVolume(parameterKey);
        slider.SetValueWithoutNotify(saved);
        apply(saved);
    }

    void OnEnable() => slider.onValueChanged.AddListener(onChanged);
    void OnDisable() => slider.onValueChanged.RemoveListener(onChanged);

    void onChanged(float value)
    {
        apply(value);
        SaveData.setVolume(parameterKey, value);
    }

    void apply(float value) => mixer.SetFloat(parameterKey, Mathf.Log10(Mathf.Clamp(value, minSliderValue, 1f)) * 20f);
}
