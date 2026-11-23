using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelSelect : MonoBehaviour
{
    [SerializeField] Button buttonPrefab;
    [SerializeField] Transform content;
    [SerializeField] string[] levelNames = { "Training Grounds", "Spike Quarry", "Guard Foundry", "Crusher Works", "Reactor Gauntlet" };
    [SerializeField] MenuFocus focus;
    [SerializeField] MenuSounds sounds;
    [SerializeField] Color openLabel = new Color32(0x15, 0x0E, 0x20, 255);
    [SerializeField] Color lockedLabel = new Color32(0x99, 0x8B, 0xB1, 255);

    readonly List<Button> buttons = new List<Button>();

    void OnEnable() => build();

    void build()
    {
        foreach (Button old in buttons)
        {
            if (old == null) continue;
            old.gameObject.SetActive(false);
            Destroy(old.gameObject);
        }
        buttons.Clear();

        int unlocked = SaveData.unlocked;
        for (int level = 1; level <= SaveData.levelCount; level++)
        {
            Button button = Instantiate(buttonPrefab, content);
            buttons.Add(button);
            bool open = SaveData.isUnlocked(level);
            button.interactable = open;

            string levelName = level - 1 < levelNames.Length ? levelNames[level - 1] : "";
            string second;
            if (!open) second = "LOCKED";
            else if (SaveData.tryGetBest(level, out float best)) second = "Best " + SaveData.formatTime(best);
            else second = "No time yet";

            TMP_Text label = button.GetComponentInChildren<TMP_Text>();
            label.text = $"LEVEL {level}  {levelName}\n{second}";
            label.color = open ? openLabel : lockedLabel;

            int captured = level;
            button.onClick.AddListener(() => openLevel(captured));
        }

        if (sounds != null) sounds.rescan();
        if (focus != null && buttons.Count > 0) focus.focus(buttons[unlocked - 1]);
    }

    static void openLevel(int level)
    {
        if (LevelLoader.instance != null) LevelLoader.instance.load(level);
    }
}
