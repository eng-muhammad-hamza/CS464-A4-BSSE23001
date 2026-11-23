using UnityEngine;

public enum PatrolMode { Loop, PingPong }
public enum GuardState { Patrol, Chase, Return }

[RequireComponent(typeof(Rigidbody))]
public class EnemyPatrol : MonoBehaviour
{
    [Header("Patrol")]
    [SerializeField] Transform[] points;
    [SerializeField] PatrolMode mode = PatrolMode.Loop;
    [SerializeField] float speed = 2f;
    [SerializeField] float waitTime = 0f;
    [SerializeField] float turnSpeed = 300f;
    [SerializeField] float arriveDistance = 0.1f;

    [Header("Chase (chaseRange 0 = never chases)")]
    [SerializeField] float chaseSpeed = 3.5f;
    [SerializeField] float chaseRange = 6f;
    [SerializeField] float giveUpRange = 9f;

    [Header("Look (optional)")]
    [SerializeField] Renderer bodyRenderer;
    [SerializeField] Color patrolColor = new Color(0.55f, 0.1f, 0.1f);
    [SerializeField] Color chaseColor = new Color(1f, 0.25f, 0.15f);

    Rigidbody rb;
    Transform player;
    Health playerHealth;
    Material bodyMaterial;
    GuardState state = GuardState.Patrol;
    int index;
    int step = 1;
    float waitUntil;

    public GuardState currentState => state;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        if (bodyRenderer != null)
        {
            bodyMaterial = bodyRenderer.material;
            bodyMaterial.color = patrolColor;
        }
    }

    void Start()
    {
        PlayerController controller = FindAnyObjectByType<PlayerController>();
        if (controller == null) return;
        player = controller.transform;
        playerHealth = controller.GetComponent<Health>();
    }

    void FixedUpdate()
    {
        if (points == null || points.Length == 0) return;
        updateState();

        switch (state)
        {
            case GuardState.Chase:
                moveToward(player.position, chaseSpeed);
                break;
            case GuardState.Return:
                Vector3 home = points[index].position;
                if (arrived(home)) setState(GuardState.Patrol);
                else moveToward(home, speed);
                break;
            default:
                patrol();
                break;
        }
    }

    void updateState()
    {
        if (state == GuardState.Chase)
        {
            if (!inRange(giveUpRange)) setState(GuardState.Return);
        }
        else if (inRange(chaseRange))
        {
            setState(GuardState.Chase);
        }
    }

    void setState(GuardState next)
    {
        if (next == state) return;
        state = next;
        Vector3 above = transform.position + Vector3.up * 2f;
        if (next == GuardState.Chase)
        {
            JuiceFx.floatingText(above, "!", JuiceFx.damageColor, 22f);
            JuiceFx.ring(transform.position, JuiceFx.damageColor, 2.5f, 0.35f);
            if (bodyMaterial != null) bodyMaterial.color = chaseColor;
        }
        else if (next == GuardState.Return)
        {
            JuiceFx.floatingText(above, "?", JuiceFx.slowColor, 18f);
        }
        if (next != GuardState.Chase && bodyMaterial != null) bodyMaterial.color = patrolColor;
    }

    void patrol()
    {
        if (Time.time < waitUntil) return;
        Vector3 target = points[index].position;
        if (arrived(target))
        {
            index = nextIndex();
            waitUntil = Time.time + waitTime;
            return;
        }
        moveToward(target, speed);
    }

    bool arrived(Vector3 target) => flatDistance(rb.position, target) < arriveDistance;

    bool inRange(float range)
    {
        if (player == null || range <= 0f) return false;
        if (playerHealth != null && playerHealth.current <= 0) return false;
        return flatDistance(rb.position, player.position) < range;
    }

    static float flatDistance(Vector3 a, Vector3 b)
    {
        a.y = 0f;
        b.y = 0f;
        return Vector3.Distance(a, b);
    }

    void moveToward(Vector3 target, float moveSpeed)
    {
        Vector3 from = rb.position;
        target.y = from.y;
        Vector3 direction = target - from;
        rb.MovePosition(Vector3.MoveTowards(from, target, moveSpeed * Time.fixedDeltaTime));
        if (direction.sqrMagnitude > 0.0001f)
        {
            Quaternion look = Quaternion.LookRotation(direction);
            rb.MoveRotation(Quaternion.RotateTowards(rb.rotation, look, turnSpeed * Time.fixedDeltaTime));
        }
    }

    int nextIndex()
    {
        if (points.Length == 1) return 0;
        if (mode == PatrolMode.Loop) return (index + 1) % points.Length;
        if (index + step < 0 || index + step >= points.Length) step = -step;
        return index + step;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, giveUpRange);
    }
}
