using UnityEngine;

public class MainMenu : MonoBehaviour
{
    [SerializeField] GameObject mainPanel;
    [SerializeField] GameObject levelPanel;
    [SerializeField] MenuFocus mainFocus;
    [SerializeField] UnityEngine.UI.Selectable playButton;
    [SerializeField] int creditsIndex = 6;

    void Start() => showMain();

    void Update()
    {
        if (levelPanel.activeSelf && Input.GetKeyDown(KeyCode.Escape)) showMain();
    }

    public void startGame()
    {
        if (LevelLoader.instance != null) LevelLoader.instance.load(SaveData.unlocked);
    }

    public void showLevelSelect()
    {
        mainPanel.SetActive(false);
        levelPanel.SetActive(true);
    }

    public void showMain()
    {
        levelPanel.SetActive(false);
        mainPanel.SetActive(true);
        if (mainFocus != null && playButton != null) mainFocus.focus(playButton);
    }

    public void showCredits()
    {
        if (LevelLoader.instance != null) LevelLoader.instance.load(creditsIndex);
    }

    public void quitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
