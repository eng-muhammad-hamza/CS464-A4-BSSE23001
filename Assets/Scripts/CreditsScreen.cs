using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CreditsScreen : MonoBehaviour
{
    [SerializeField] RectTransform viewport;
    [SerializeField] TMP_Text creditsText;
    [SerializeField] float scrollSpeed = 60f;
    [SerializeField] float endPause = 1.5f;
    [SerializeField] Selectable backButton;
    [SerializeField] MenuFocus focus;

    RectTransform textRect;
    float y;
    float endY;
    float endTimer;
    bool leaving;

    void Start()
    {
        textRect = creditsText.rectTransform;
        Canvas.ForceUpdateCanvases();
        y = -viewport.rect.height;
        endY = creditsText.preferredHeight;
        textRect.anchoredPosition = new Vector2(0f, y);
        if (focus != null) focus.focus(backButton);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) returnToMenu();

        if (y < endY)
        {
            y += scrollSpeed * Time.deltaTime;
            textRect.anchoredPosition = new Vector2(0f, y);
        }
        else
        {
            endTimer += Time.deltaTime;
            if (endTimer >= endPause) returnToMenu();
        }
    }

    public void returnToMenu()
    {
        if (leaving || LevelLoader.instance == null) return;
        leaving = true;
        LevelLoader.instance.loadNext();
    }
}
