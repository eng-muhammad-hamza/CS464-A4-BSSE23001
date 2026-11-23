using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public enum Objective { CollectCoins, ReachGoal, CollectKeys, Survive }

public class GameManager : MonoBehaviour
{
    public static GameManager instance { get; private set; }

    [Header("Objective")]
    [SerializeField] Objective objective = Objective.ReachGoal;
    [SerializeField] int targetScore = 10;
    [SerializeField] int keysRequired = 3;
    [SerializeField, TextArea] string objectiveMessage = "";
    [SerializeField] float parTime = 0f;

    [Header("Clock (0 = no clock)")]
    [SerializeField] float timeLimit = 0f;
    [SerializeField] float deathTimePenalty = 0f;

    [Header("Lives and respawn")]
    [SerializeField] int lives = 3;
    [SerializeField] float respawnDelay = 0.6f;
    [SerializeField] float spawnProtection = 1.5f;
    [SerializeField] float respawnLift = 0.6f;
    [SerializeField] Health player;
    [SerializeField] Transform respawnPoint;

    [Header("HUD (inside the prefab)")]
    [SerializeField] TMP_Text objectiveText;
    [SerializeField] TMP_Text scoreText;
    [SerializeField] TMP_Text livesText;
    [SerializeField] TMP_Text timerText;
    [SerializeField] GameObject timerPanel;
    [SerializeField] Color timerNormal = new Color32(0xF3, 0xEE, 0xFC, 255);
    [SerializeField] Color timerWarning = new Color32(0xFF, 0xD8, 0x3D, 255);

    [Header("End screens (inside the prefab)")]
    [SerializeField] TMP_Text resultText;
    [SerializeField] TMP_Text loseReasonText;
    [SerializeField] GameObject winPanel;
    [SerializeField] GameObject losePanel;
    [SerializeField] TMP_Text nextButtonLabel;
    [SerializeField] GameObject firstWinButton;
    [SerializeField] GameObject firstLoseButton;

    [Header("Sounds")]
    [SerializeField] AudioClip winClip;
    [SerializeField] AudioClip loseClip;

    int score;
    int keysCollected;
    float timeLeft;
    bool finished;

    public bool isFinished => finished;
    public int totalScore => score;
    public int livesLeft => lives;
    public bool exitUnlocked => objective != Objective.CollectKeys || keysCollected >= keysRequired;
    public bool goalOpen => exitUnlocked && score >= targetScore;
    public float elapsed => timeLimit > 0f ? timeLimit - timeLeft : Time.timeSinceLevelLoad;

    void Awake()
    {
        instance = this;
        Time.timeScale = 1f;
    }

    void OnDestroy()
    {
        if (instance == this) instance = null;
    }

    void Start()
    {
        timeLeft = timeLimit;
        if (winPanel != null) winPanel.SetActive(false);
        if (losePanel != null) losePanel.SetActive(false);
        if (objectiveText != null && !string.IsNullOrEmpty(objectiveMessage)) objectiveText.text = objectiveMessage;
        if (timerPanel != null) timerPanel.SetActive(timeLimit > 0f);
        updateTimerText();
        updateUi();
    }

    void Update()
    {
        if (timeLimit <= 0f || finished) return;
        timeLeft -= Time.deltaTime;
        if (timeLeft <= 0f)
        {
            timeLeft = 0f;
            updateTimerText();
            if (objective == Objective.Survive)
            {
                if (score >= targetScore) win();
                else lose($"Only {score} / {targetScore} coins!");
            }
            else lose("Time's up!");
            return;
        }
        updateTimerText();
    }

    void updateTimerText()
    {
        if (timerText == null || timeLimit <= 0f) return;
        string label = objective == Objective.Survive ? "SURVIVE" : "TIME";
        timerText.text = $"{label}  {timeLeft:0.0}";
        timerText.color = timeLeft < 10f && objective != Objective.Survive ? timerWarning : timerNormal;
    }

    public void addTime(float seconds)
    {
        if (finished || timeLimit <= 0f) return;
        timeLeft += seconds;
        updateTimerText();
        punchText(timerText);
    }

    public void addScore(int amount)
    {
        if (finished) return;
        score += amount;
        updateUi();
        punchText(scoreText);
        if (objective == Objective.CollectCoins && score >= targetScore) win();
    }

    public void addKey()
    {
        if (finished) return;
        keysCollected++;
        updateUi();
        punchText(scoreText);
        if (player != null)
        {
            string message = keysCollected >= keysRequired ? "EXIT OPEN!" : $"KEY {keysCollected} / {keysRequired}";
            JuiceFx.floatingText(player.transform.position + Vector3.up * 1.6f, message, JuiceFx.keyColor, 20f);
        }
    }

    public bool reachGoal()
    {
        if (finished || objective == Objective.CollectCoins || objective == Objective.Survive) return false;
        if (!goalOpen)
        {
            if (player != null)
            {
                string message = !exitUnlocked ? "FIND ALL THE KEYS" : $"NEED {targetScore - score} MORE COINS";
                JuiceFx.floatingText(player.transform.position + Vector3.up * 1.6f, message, JuiceFx.damageColor, 16f);
            }
            return false;
        }
        win();
        return true;
    }

    public void setCheckpoint(Transform point) => respawnPoint = point;

    public void updateUi()
    {
        if (scoreText != null)
        {
            scoreText.text = objective == Objective.CollectKeys
                ? $"KEYS  {keysCollected} / {keysRequired}\nCOINS  {score} / {targetScore}"
                : $"COINS  {score} / {targetScore}";
        }
        if (livesText != null) livesText.text = $"LIVES  {lives}";
    }

    static void punchText(TMP_Text text)
    {
        if (text != null && text.TryGetComponent(out UiPunch punch)) punch.punch();
    }

    public void playerDied()
    {
        if (finished) return;
        lives--;
        if (deathTimePenalty > 0f && timeLimit > 0f)
        {
            timeLeft = Mathf.Max(1f, timeLeft - deathTimePenalty);
            updateTimerText();
            if (player != null)
                JuiceFx.floatingText(player.transform.position + Vector3.up * 1.6f, $"-{deathTimePenalty:0}s", JuiceFx.damageColor, 18f);
        }
        updateUi();
        punchText(livesText);
        if (lives <= 0) lose("Out of lives!");
        else StartCoroutine(respawn());
    }

    IEnumerator respawn()
    {
        yield return new WaitForSeconds(respawnDelay);
        if (finished) yield break;
        Rigidbody body = player.GetComponent<Rigidbody>();
        body.position = respawnPoint.position + Vector3.up * respawnLift;
        body.linearVelocity = Vector3.zero;
        body.angularVelocity = Vector3.zero;
        player.restore(spawnProtection);
    }

    void win()
    {
        if (finished) return;
        finished = true;

        int level = gameObject.scene.buildIndex;
        float time = Time.timeSinceLevelLoad;
        bool best = SaveData.recordTime(level, time);
        SaveData.unlock(level + 1);
        showWinScreen(best, level, time);
    }

    void showWinScreen(bool newBest, int level, float time)
    {
        if (resultText != null)
        {
            string summary = $"TIME  {SaveData.formatTime(time)}";
            if (newBest) summary += "\nNEW BEST!";
            else if (SaveData.tryGetBest(level, out float best)) summary += $"\nBest  {SaveData.formatTime(best)}";
            if (parTime > 0f) summary += time <= parTime ? $"\nUnder par ({parTime:0} s)" : $"\nPar  {parTime:0} s";
            summary += $"\nCoins  {score} / {targetScore}    Lives left  {lives}";
            resultText.text = summary;
        }
        if (nextButtonLabel != null) nextButtonLabel.text = level >= SaveData.levelCount ? "CREDITS" : "NEXT LEVEL";

        winPanel.SetActive(true);
        selectFirstButton(firstWinButton);
        Time.timeScale = 0f;
        if (SfxPlayer.instance != null) SfxPlayer.instance.play(winClip, 1f, false);

        Vector3 center = (player != null ? player.transform.position : Vector3.zero) + Vector3.up;
        JuiceFx.burst(center, JuiceFx.goldColor, 70, 9f, 0.25f, 1.6f, true, 1.2f);
        JuiceFx.burst(center, JuiceFx.speedColor, 50, 8f, 0.22f, 1.6f, true, 1.2f);
        JuiceFx.burst(center, JuiceFx.heartColor, 50, 7f, 0.22f, 1.6f, true, 1.2f);
        JuiceFx.ring(center, JuiceFx.goldColor, 7f, 0.9f, true);
    }

    void lose(string reason)
    {
        if (finished) return;
        finished = true;
        if (loseReasonText != null) loseReasonText.text = reason;
        losePanel.SetActive(true);
        selectFirstButton(firstLoseButton);
        Time.timeScale = 0f;
        if (SfxPlayer.instance != null) SfxPlayer.instance.play(loseClip, 1f, false);
    }

    static void selectFirstButton(GameObject button)
    {
        if (button == null || EventSystem.current == null) return;
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(button);
    }
}
