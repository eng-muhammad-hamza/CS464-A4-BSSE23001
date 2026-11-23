using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    public static bool isPaused { get; private set; }

    [SerializeField] GameObject panel;
    [SerializeField] Selectable firstButton;

    LevelLoader loader => LevelLoader.instance;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void resetStatics() => isPaused = false;

    public static void clearPauseState()
    {
        isPaused = false;
        AudioListener.pause = false;
    }

    void Start()
    {
        if (panel != null) panel.SetActive(false);
    }

    void OnDestroy()
    {
        if (isPaused) clearPauseState();
    }

    void Update()
    {
        if (!Input.GetKeyDown(KeyCode.Escape)) return;
        if (isPaused) resumeGame();
        else if (canPause()) pauseGame();
    }

    static bool canPause() => GameManager.instance == null || !GameManager.instance.isFinished;

    public void pauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;
        AudioListener.pause = true;
        panel.SetActive(true);
        selectFirstButton(firstButton);
    }

    public void resumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;
        AudioListener.pause = false;
        if (panel != null) panel.SetActive(false);
    }

    public void restartLevel()
    {
        resumeGame();
        if (loader != null) loader.reload();
    }

    public void quitToMenu()
    {
        resumeGame();
        if (loader != null) loader.loadMenu();
    }

    public void nextLevel()
    {
        resumeGame();
        if (loader != null) loader.loadNext();
    }

    static void selectFirstButton(Selectable button)
    {
        if (button == null || EventSystem.current == null) return;
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(button.gameObject);
    }
}
