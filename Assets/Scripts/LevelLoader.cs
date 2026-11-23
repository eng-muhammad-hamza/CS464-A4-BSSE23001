using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelLoader : MonoBehaviour
{
    public static LevelLoader instance { get; private set; }

    [Header("Scene List (File > Build Profiles)")]
    [SerializeField] int menuIndex = 0;
    [SerializeField] int lastIndex = 6;

    [Header("Loading bar")]
    [SerializeField] GameObject loadingPanel;
    [SerializeField] Image barFill;
    [SerializeField] TMP_Text percentText;
    [SerializeField] float minSeconds = 0.6f;
    [SerializeField] float fillSpeed = 1.8f;

    bool loading;

    void Awake()
    {
        instance = this;
        if (loadingPanel != null) loadingPanel.SetActive(false);
    }

    void OnDestroy()
    {
        if (instance == this) instance = null;
    }

    public void load(int index)
    {
        if (loading) return;
        if (index < 0 || index >= SceneManager.sceneCountInBuildSettings)
        {
            Debug.LogError($"Scene {index} is not in the Scene List (File > Build Profiles).", this);
            return;
        }
        StartCoroutine(loadRoutine(index));
    }

    public void loadNext()
    {
        int current = SceneManager.GetActiveScene().buildIndex;
        load(current >= lastIndex ? menuIndex : current + 1);
    }

    public void reload() => load(SceneManager.GetActiveScene().buildIndex);
    public void loadMenu() => load(menuIndex);

    IEnumerator loadRoutine(int index)
    {
        loading = true;
        Time.timeScale = 0f;
        if (loadingPanel != null) loadingPanel.SetActive(true);
        setBar(0f);

        AsyncOperation op = SceneManager.LoadSceneAsync(index);
        op.allowSceneActivation = false;

        float shown = 0f;
        float elapsed = 0f;
        while (true)
        {
            elapsed += Time.unscaledDeltaTime;
            float target = Mathf.Clamp01(op.progress / 0.9f);
            shown = Mathf.MoveTowards(shown, target, fillSpeed * Time.unscaledDeltaTime);
            setBar(shown);
            if (shown >= 1f && elapsed >= minSeconds) break;
            yield return null;
        }

        Time.timeScale = 1f;
        PauseMenu.clearPauseState();
        op.allowSceneActivation = true;
        yield return op;
    }

    void setBar(float value)
    {
        if (barFill != null) barFill.fillAmount = value;
        if (percentText != null) percentText.text = $"{Mathf.RoundToInt(value * 100f)}%";
    }
}
