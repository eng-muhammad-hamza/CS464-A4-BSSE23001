using System;
using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField] int max = 3;
    [SerializeField] float iFrames = 1f;

    int heartsLeft;
    float invulnerableUntil;
    PlayerJuice juice;
    PlayerController controller;

    public event Action<int, int> damaged;
    public event Action<int, int> healed;
    public event Action<int, int> restored;
    public event Action died;

    public int current => heartsLeft;
    public int maxHearts => max;
    public bool isInvulnerable => Time.time < invulnerableUntil;

    void Awake()
    {
        heartsLeft = max;
        juice = GetComponent<PlayerJuice>();
        controller = GetComponent<PlayerController>();
    }

    public bool takeDamage(int amount)
    {
        if (amount <= 0 || heartsLeft <= 0 || isInvulnerable) return false;
        invulnerableUntil = Time.time + iFrames;

        int previous = heartsLeft;
        heartsLeft = Mathf.Max(0, heartsLeft - amount);
        if (GameManager.instance != null) GameManager.instance.updateUi();
        damaged?.Invoke(heartsLeft, max);

        if (heartsLeft > 0)
        {
            if (juice != null) juice.tookDamage(previous - heartsLeft);
            return true;
        }
        die();
        return true;
    }

    public void kill()
    {
        if (heartsLeft <= 0) return;
        heartsLeft = 0;
        if (GameManager.instance != null) GameManager.instance.updateUi();
        damaged?.Invoke(heartsLeft, max);
        die();
    }

    void die()
    {
        if (controller != null)
        {
            controller.setControlsEnabled(false);
            controller.cancelBoost();
        }
        if (juice != null) juice.died();
        died?.Invoke();
        if (GameManager.instance != null) GameManager.instance.playerDied();
    }

    public bool heal(int amount)
    {
        if (amount <= 0 || heartsLeft <= 0 || heartsLeft >= max) return false;
        int previous = heartsLeft;
        heartsLeft = Mathf.Min(max, heartsLeft + amount);
        if (juice != null) juice.healed(heartsLeft - previous);
        if (GameManager.instance != null) GameManager.instance.updateUi();
        healed?.Invoke(heartsLeft, max);
        return true;
    }

    public void restore(float protectionSeconds)
    {
        heartsLeft = max;
        invulnerableUntil = Time.time + protectionSeconds;
        if (controller != null) controller.setControlsEnabled(true);
        if (juice != null) juice.respawned();
        if (GameManager.instance != null) GameManager.instance.updateUi();
        restored?.Invoke(heartsLeft, max);
    }
}
