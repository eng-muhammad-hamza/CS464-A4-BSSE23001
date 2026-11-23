using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MenuSounds : MonoBehaviour
{
    [SerializeField] AudioClip clickClip;
    [SerializeField] AudioClip moveClip;

    GameObject lastSelected;

    void OnEnable() => rescan();

    void OnDisable()
    {
        foreach (Button button in GetComponentsInChildren<Button>(true))
            button.onClick.RemoveListener(playClickSound);
    }

    public void rescan()
    {
        foreach (Button button in GetComponentsInChildren<Button>(true))
        {
            button.onClick.RemoveListener(playClickSound);
            button.onClick.AddListener(playClickSound);
        }
    }

    void playClickSound()
    {
        if (SfxPlayer.instance != null) SfxPlayer.instance.play(clickClip, 1f, false);
    }

    void Update()
    {
        EventSystem es = EventSystem.current;
        if (es == null) return;
        GameObject current = es.currentSelectedGameObject;
        if (current != lastSelected)
        {
            if (current != null && lastSelected != null && SfxPlayer.instance != null)
                SfxPlayer.instance.play(moveClip, 0.7f, false);
            lastSelected = current;
        }
    }
}
