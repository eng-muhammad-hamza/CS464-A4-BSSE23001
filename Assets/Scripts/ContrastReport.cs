using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ContrastReport : MonoBehaviour
{
    [ContextMenu("Measure text contrast")]
    void measure()
    {
        foreach (TMP_Text text in GetComponentsInChildren<TMP_Text>(true))
        {
            Image panel = text.GetComponentInParent<Image>(true);
            if (panel == null) { Debug.Log($"{text.name}: no Image behind it"); continue; }
            Color back = panel.color;
            float ratio = ColourUtil.contrast(text.color, back);
            string verdict = ratio >= 4.5f ? "OK" : (ratio >= 3f ? "large text only" : "FAIL");
            Debug.Log($"{text.name} {ColourUtil.hex(text.color)} on {panel.name} {ColourUtil.hex(back)}: {ratio:0.00} : 1  {verdict}");
        }
    }
}
