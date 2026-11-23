using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Move")]
    [SerializeField] float moveForce = 10f;
    [SerializeField] float maxSpeed = 7f;
    [SerializeField] float brake = 25f;
    [SerializeField, Range(0f, 1f)] float airControl = 0.6f;

    [Header("Jump (coyote time + input buffer)")]
    [SerializeField] float jumpSpeed = 6f;
    [SerializeField] float coyoteTime = 0.12f;
    [SerializeField] float jumpBuffer = 0.12f;
    [SerializeField] float groundProbeRadius = 0.3f;
    [SerializeField] float groundProbeDistance = 0.35f;

    [Header("Ice")]
    [SerializeField] float iceDamping = 0.1f;
    [SerializeField] float iceControl = 0.5f;
    [SerializeField] float iceMaxSpeed = 7f;

    Rigidbody rb;
    PlayerJuice juice;
    Vector3 input;
    bool controlsEnabled = true;

    float speedMultiplier = 1f;
    Coroutine boost;
    float boostEndTime;

    int iceContacts;
    float normalLinearDamping;
    float normalAngularDamping;

    bool grounded;
    bool wasGrounded;
    float lastGroundedTime = -10f;
    float lastJumpPressTime = -10f;
    float lowestVerticalSpeed;

    float noControlUntil;

    public bool isSpeedBoosted => boost != null;
    public float currentSpeedMultiplier => speedMultiplier;
    public float speedBoostSecondsRemaining => boost == null ? 0f : Mathf.Max(0f, boostEndTime - Time.time);
    public bool isGrounded => grounded;
    public float flatSpeed
    {
        get
        {
            Vector3 v = rb != null ? rb.linearVelocity : Vector3.zero;
            return new Vector2(v.x, v.z).magnitude;
        }
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.sleepThreshold = 0f;
        juice = GetComponent<PlayerJuice>();
        normalLinearDamping = rb.linearDamping;
        normalAngularDamping = rb.angularDamping;
    }

    void OnDisable()
    {
        speedMultiplier = 1f;
        boost = null;
    }

    void Update()
    {
        if (PauseMenu.isPaused) return;
        if (!controlsEnabled)
        {
            input = Vector3.zero;
            return;
        }
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        input = Vector3.ClampMagnitude(new Vector3(h, 0f, v), 1f);
        if (Input.GetKeyDown(KeyCode.Space)) lastJumpPressTime = Time.time;
    }

    void FixedUpdate()
    {
        updateGrounded();
        tryJump();

        bool locked = Time.time < noControlUntil;
        bool onIce = iceContacts > 0;

        if (!locked)
        {
            float control = grounded ? 1f : airControl;
            if (onIce) control *= iceControl;
            rb.AddForce(input * (moveForce * speedMultiplier * control));
            limitFlatSpeed(onIce);
        }
    }

    void updateGrounded()
    {
        wasGrounded = grounded;
        grounded = rb.linearVelocity.y < 1f &&
                   Physics.SphereCast(rb.position, groundProbeRadius, Vector3.down, out _, groundProbeDistance,
                                      ~0, QueryTriggerInteraction.Ignore);
        if (grounded) lastGroundedTime = Time.time;

        if (!grounded) lowestVerticalSpeed = Mathf.Min(lowestVerticalSpeed, rb.linearVelocity.y);
        if (grounded && !wasGrounded)
        {
            if (lowestVerticalSpeed < -4f)
            {
                JuiceFx.burst(rb.position + Vector3.down * 0.45f, new Color(0.8f, 0.8f, 0.85f), 10, 2.5f, 0.14f, 0.35f);
                if (CameraShake.instance != null && lowestVerticalSpeed < -8f) CameraShake.instance.shake(0.1f, 0.12f);
            }
            lowestVerticalSpeed = 0f;
        }
    }

    void tryJump()
    {
        if (!controlsEnabled) return;
        bool pressedRecently = Time.time - lastJumpPressTime <= jumpBuffer;
        bool groundedRecently = Time.time - lastGroundedTime <= coyoteTime;
        if (!pressedRecently || !groundedRecently) return;

        lastJumpPressTime = -10f;
        lastGroundedTime = -10f;
        Vector3 v = rb.linearVelocity;
        rb.linearVelocity = new Vector3(v.x, jumpSpeed, v.z);
        JuiceFx.burst(rb.position + Vector3.down * 0.45f, Color.white, 8, 2.5f, 0.12f, 0.35f);
    }

    void limitFlatSpeed(bool onIce)
    {
        float cap = onIce ? iceMaxSpeed : maxSpeed * speedMultiplier;
        Vector3 v = rb.linearVelocity;
        Vector3 flat = new Vector3(v.x, 0f, v.z);
        float speed = flat.magnitude;
        if (speed <= cap) return;
        float newSpeed = Mathf.MoveTowards(speed, cap, brake * Time.fixedDeltaTime);
        flat = flat / speed * newSpeed;
        rb.linearVelocity = new Vector3(flat.x, v.y, flat.z);
    }

    public void setControlsEnabled(bool controlsAllowed)
    {
        controlsEnabled = controlsAllowed;
        if (!controlsAllowed) input = Vector3.zero;
    }

    public void knockback(Vector3 push, float seconds)
    {
        Vector3 v = rb.linearVelocity;
        rb.linearVelocity = new Vector3(0f, v.y, 0f);
        rb.angularVelocity = Vector3.zero;
        rb.AddForce(push, ForceMode.Impulse);
        noControlUntil = Time.time + seconds;
    }

    public void boostSpeed(float multiplier, float seconds)
    {
        if (multiplier <= 0f || seconds <= 0f) return;
        if (boost != null) StopCoroutine(boost);
        boost = StartCoroutine(boostRoutine(multiplier, seconds));
    }

    IEnumerator boostRoutine(float multiplier, float seconds)
    {
        speedMultiplier = multiplier;
        boostEndTime = Time.time + seconds;
        if (juice != null) juice.speedBoostStarted();
        yield return new WaitForSeconds(seconds);
        speedMultiplier = 1f;
        boost = null;
        if (juice != null) juice.speedBoostEnded();
    }

    public void cancelBoost()
    {
        if (boost == null) return;
        StopCoroutine(boost);
        boost = null;
        speedMultiplier = 1f;
        if (juice != null) juice.speedBoostEnded();
    }

    public void enterIce() { iceContacts++; applyIce(); }
    public void exitIce() { iceContacts = Mathf.Max(0, iceContacts - 1); applyIce(); }

    void applyIce()
    {
        bool onIce = iceContacts > 0;
        rb.linearDamping = onIce ? iceDamping : normalLinearDamping;
        rb.angularDamping = onIce ? iceDamping : normalAngularDamping;
    }
}
