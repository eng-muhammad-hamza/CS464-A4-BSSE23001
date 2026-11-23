using System.Collections;
using UnityEngine;

public class ExitDoor : MonoBehaviour
{
    [SerializeField] Transform gate;
    [SerializeField] Renderer gateRenderer;
    [SerializeField] Collider gateCollider;
    [SerializeField] float sinkDistance = 2f;
    [SerializeField] float openSeconds = 0.8f;
    bool opened;

    void Start()
    {
        if (gateRenderer != null) gateRenderer.material.color = JuiceFx.damageColor;
    }

    void Update()
    {
        if (opened || GameManager.instance == null || !GameManager.instance.exitUnlocked) return;
        opened = true;
        StartCoroutine(openDoorRoutine());
    }

    IEnumerator openDoorRoutine()
    {
        if (gateCollider != null) gateCollider.enabled = false;
        if (gateRenderer != null) gateRenderer.material.color = JuiceFx.positiveColor;
        JuiceFx.ring(transform.position + Vector3.up * 0.1f, JuiceFx.positiveColor, 3f, 0.6f);

        Vector3 from = gate.position;
        Vector3 to = from + Vector3.down * sinkDistance;
        float elapsed = 0f;
        while (elapsed < openSeconds)
        {
            elapsed += Time.deltaTime;
            float u = Mathf.Clamp01(elapsed / openSeconds);
            gate.position = Vector3.Lerp(from, to, u * u * (3f - 2f * u));
            yield return null;
        }
        gate.gameObject.SetActive(false);
    }
}
