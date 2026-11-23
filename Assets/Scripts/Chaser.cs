using UnityEngine;

public class Chaser : MonoBehaviour
{
    enum State { Dormant, Appear, Chase, Telegraph, Lunge, Recover }

    [Header("Spawn")]
    [SerializeField] float activateAtSeconds = 0f;
    [SerializeField] float appearSeconds = 1f;

    [Header("Movement")]
    [SerializeField] float chaseSpeed = 2.6f;
    [SerializeField] float lungeSpeed = 9f;
    [SerializeField] float lungeRange = 4.5f;
    [SerializeField] float telegraphSeconds = 0.6f;
    [SerializeField] float lungeSeconds = 0.45f;
    [SerializeField] float recoverSeconds = 1.2f;
    [SerializeField] float hoverHeight = 0.9f;
    [SerializeField] Vector2 minXZ = new Vector2(-12.5f, -12.5f);
    [SerializeField] Vector2 maxXZ = new Vector2(12.5f, 12.5f);

    [Header("Parts")]
    [SerializeField] GameObject visual;
    [SerializeField] Renderer bodyRenderer;

    static readonly Color baseColor = new Color(0.7f, 0.3f, 0.9f);
    static readonly Color telegraphColor = new Color(1f, 0.92f, 0.3f);
    static readonly Color recoverColor = new Color(0.45f, 0.25f, 0.55f);

    Collider triggerCollider;
    Hazard hazard;
    Material bodyMaterial;
    Transform player;
    Health playerHealth;
    State state = State.Dormant;
    float stateEndTime;
    Vector3 lungeDirection;

    void Awake()
    {
        triggerCollider = GetComponent<Collider>();
        hazard = GetComponent<Hazard>();
        bodyMaterial = bodyRenderer.material;
        visual.SetActive(false);
        triggerCollider.enabled = false;
        hazard.enabled = false;
    }

    void Start()
    {
        PlayerController controller = FindAnyObjectByType<PlayerController>();
        if (controller != null)
        {
            player = controller.transform;
            playerHealth = controller.GetComponent<Health>();
        }
    }

    void Update()
    {
        if (player == null || playerHealth == null) return;
        if (GameManager.instance != null && GameManager.instance.isFinished) return;

        if (state == State.Dormant)
        {
            float elapsed = GameManager.instance != null ? GameManager.instance.elapsed : Time.timeSinceLevelLoad;
            if (elapsed >= activateAtSeconds) enterState(State.Appear);
            return;
        }

        if (state == State.Appear)
        {
            visual.SetActive(Mathf.FloorToInt(Time.time * 12f) % 2 == 0);
            if (Time.time >= stateEndTime) enterState(State.Chase);
            return;
        }

        if (playerHealth.current <= 0) return;

        Vector3 toPlayer = player.position - transform.position;
        toPlayer.y = 0f;
        float distance = toPlayer.magnitude;
        Vector3 direction = distance > 0.01f ? toPlayer / distance : transform.forward;

        switch (state)
        {
            case State.Chase:
                move(direction, chaseSpeed);
                face(direction);
                if (distance < lungeRange) enterState(State.Telegraph);
                break;
            case State.Telegraph:
                face(direction);
                bodyMaterial.color = Mathf.FloorToInt(Time.time * 16f) % 2 == 0 ? telegraphColor : baseColor;
                if (Time.time >= stateEndTime)
                {
                    lungeDirection = direction;
                    enterState(State.Lunge);
                }
                break;
            case State.Lunge:
                move(lungeDirection, lungeSpeed);
                if (Time.time >= stateEndTime) enterState(State.Recover);
                break;
            case State.Recover:
                move(-direction, chaseSpeed * 0.6f);
                if (Time.time >= stateEndTime) enterState(State.Chase);
                break;
        }

        Vector3 position = transform.position;
        position.y = hoverHeight + Mathf.Sin(Time.time * 5f) * 0.08f;
        transform.position = position;
    }

    void enterState(State next)
    {
        state = next;
        switch (next)
        {
            case State.Appear:
                transform.position = new Vector3(transform.position.x, hoverHeight, transform.position.z);
                visual.SetActive(true);
                stateEndTime = Time.time + appearSeconds;
                JuiceFx.ring(transform.position, baseColor, 3f, appearSeconds);
                JuiceFx.floatingText(transform.position + Vector3.up * 1.2f, "!", JuiceFx.damageColor, 22f);
                break;
            case State.Chase:
                visual.SetActive(true);
                triggerCollider.enabled = true;
                hazard.enabled = true;
                bodyMaterial.color = baseColor;
                break;
            case State.Telegraph:
                stateEndTime = Time.time + telegraphSeconds;
                break;
            case State.Lunge:
                stateEndTime = Time.time + lungeSeconds;
                bodyMaterial.color = JuiceFx.damageColor;
                break;
            case State.Recover:
                stateEndTime = Time.time + recoverSeconds;
                bodyMaterial.color = recoverColor;
                break;
        }
    }

    void move(Vector3 direction, float speed)
    {
        Vector3 position = transform.position + direction * (speed * Time.deltaTime);
        position.x = Mathf.Clamp(position.x, minXZ.x, maxXZ.x);
        position.z = Mathf.Clamp(position.z, minXZ.y, maxXZ.y);
        transform.position = position;
    }

    void face(Vector3 direction)
    {
        if (direction.sqrMagnitude < 0.001f) return;
        transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(direction), 540f * Time.deltaTime);
    }
}
