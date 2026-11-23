using System.Text;
using TMPro;
using UnityEngine;

public class PowerStatusHud : MonoBehaviour
{
    [SerializeField] bool showSpeedReadout = false;
    [SerializeField] GameObject panel;
    TMP_Text label;
    CanvasGroup panelGroup;
    PlayerController controller;
    readonly StringBuilder builder = new StringBuilder();

    void Awake()
    {
        label = GetComponent<TMP_Text>();
        if (panel != null)
        {
            panelGroup = panel.GetComponent<CanvasGroup>();
            if (panelGroup == null) panelGroup = panel.AddComponent<CanvasGroup>();
            panelGroup.alpha = 0f;
        }
    }
    void Start() => controller = FindAnyObjectByType<PlayerController>();

    void Update()
    {
        if (controller == null) return;
        builder.Clear();
        if (controller.isSpeedBoosted)
            builder.AppendLine($"SPEED x{controller.currentSpeedMultiplier:0.0}  {controller.speedBoostSecondsRemaining:0.0}s");
        if (showSpeedReadout)
            builder.AppendLine($"{controller.flatSpeed:0.0} m/s");
        label.text = builder.ToString();
        if (panelGroup != null) panelGroup.alpha = builder.Length > 0 ? 1f : 0f;
    }
}
