using UnityEngine;

public class Goal : MonoBehaviour
{
    [SerializeField] float pulseInterval = 1.2f;
    float nextPulse;
    bool reached;

    void Update()
    {
        if (Time.time < nextPulse) return;
        nextPulse = Time.time + pulseInterval;
        bool open = GameManager.instance != null && GameManager.instance.goalOpen;
        JuiceFx.ring(transform.position + Vector3.up * 0.1f, open ? JuiceFx.positiveColor : JuiceFx.damageColor, 2.2f, 0.9f);
    }

    void OnTriggerEnter(Collider other)
    {
        if (reached || !other.CompareTag("Player")) return;
        if (GameManager.instance != null && GameManager.instance.reachGoal()) reached = true;
    }

    void OnTriggerStay(Collider other)
    {
        if (reached || !other.CompareTag("Player")) return;
        if (GameManager.instance != null && GameManager.instance.goalOpen && GameManager.instance.reachGoal()) reached = true;
    }
}
