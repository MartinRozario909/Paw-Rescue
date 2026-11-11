using System;
using UnityEngine;
using UnityEngine.InputSystem;

// Movement modelled on Kenney's 3D platformer starter kit (player.gd):
// camera-relative movement and a double jump. The Rigidbody handles gravity
// (strength is set in Project Settings > Physics > Gravity) and the BoxCollider
// handles the ground, walls and picking up coins.
[RequireComponent(typeof(Rigidbody), typeof(BoxCollider))]
public class PetController : MonoBehaviour, IResettable
{
    [SerializeField] private float runSpeed = 6f;
    [SerializeField] private float groundResponsiveness = 12f; // How fast we reach run speed on the ground
    [SerializeField] private float airResponsiveness = 6f;     // Less control while in the air
    [SerializeField] private float rotationSpeed = 720f;
    [SerializeField] private float maxFallSpeed = 30f;
    [SerializeField] private float jumpHeight = 1.5f;
    [SerializeField] private float doubleJumpMultiplier = 0.9f; // Second jump is a little weaker
    [SerializeField] private float jumpCutMultiplier = 0.5f;    // Releasing Space early gives a shorter jump
    [SerializeField] private float jumpBufferTime = 0.12f;      // Space pressed just before landing still jumps
    [SerializeField] private int maxJumps = 2;
    [SerializeField] private float groundCheckDepth = 0.1f;     // How far below the box still counts as standing
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private AudioSource jumpSFX;
    [SerializeField] private JoystickController joystick; // On-screen stick, used on mobile
    [SerializeField] private JumpButton jumpButton;       // On-screen jump button, used on mobile

    [Header("Enemies")]
    [SerializeField] private float knockbackDistance = 2.5f; // How far a hit pushes the pet away from the enemy
    [SerializeField] private float knockbackHeight = 0.5f;   // Small hop while being pushed
    [SerializeField] private float knockbackTime = 0.25f;    // No control for this long while being pushed
    [SerializeField] private float stompBounceHeight = 1.2f; // Bounce after jumping on an enemy

    private Rigidbody rb;
    private BoxCollider box;
    private readonly Collider[] groundHits = new Collider[8];
    private Vector2 moveInput;
    private float jumpBufferTimer;
    private bool jumpReleased;
    private int jumpsUsed;
    private float knockbackTimer;
    private Vector3 platformVelocity; // Velocity of the moving platform we stand on, added on top of our own
    private Vector3 startPosition;
    private Quaternion startRotation;

    // GameManager turns this off outside of gameplay (menus, game over, win)
    public bool InputEnabled { get; set; } = true;
    public float RunSpeed => runSpeed;
    public float CurrentSpeed { get; private set; }
    // Speed the player is asking for; changes the instant a key is pressed or released
    public float TargetSpeed { get; private set; }
    public bool IsGrounded { get; private set; }
    public bool IsKnockedBack => knockbackTimer > 0f;

    // Raised every time the pet jumps (including the double jump)
    public event Action Jumped;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        box = GetComponent<BoxCollider>();
        rb.useGravity = true;
        rb.freezeRotation = true; // Stop the pet tipping over; we rotate it ourselves
        rb.interpolation = RigidbodyInterpolation.Interpolate; // Smooth movement between physics steps

        startPosition = transform.position;
        startRotation = transform.rotation;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (cameraTransform == null && Camera.main != null) cameraTransform = Camera.main.transform;
    }

    // Update is called once per frame. Input is read here so key presses are never missed between physics steps.
    void Update()
    {
        ReadInput();
        TargetSpeed = GetCameraRelativeDirection().magnitude * runSpeed;
    }

    // Physics movement happens in FixedUpdate, in step with the Rigidbody
    void FixedUpdate()
    {
        float dt = Time.fixedDeltaTime;
        Vector3 velocity = rb.linearVelocity;

        IsGrounded = CheckGrounded(out Collider ground);
        if (IsGrounded && velocity.y <= 0.01f) jumpsUsed = 0;

        // Horizontal movement relative to where the camera is looking.
        // Start from the real velocity so walls can stop us, minus last step's ride on a platform.
        Vector3 planarVelocity = new Vector3(velocity.x, 0f, velocity.z) - platformVelocity;
        MovingPlatform platform = ground != null && ground.attachedRigidbody != null
            ? ground.attachedRigidbody.GetComponent<MovingPlatform>() : null;
        platformVelocity = platform != null ? platform.Velocity : Vector3.zero;
        if (IsKnockedBack)
        {
            // Keep sliding away from the hit, then stop so the push is exactly knockbackDistance
            knockbackTimer -= dt;
            if (knockbackTimer <= 0f) planarVelocity = Vector3.zero;
        }
        else
        {
            Vector3 targetVelocity = GetCameraRelativeDirection() * runSpeed;
            float responsiveness = IsGrounded ? groundResponsiveness : airResponsiveness;
            planarVelocity = Vector3.Lerp(planarVelocity, targetVelocity, 1f - Mathf.Exp(-responsiveness * dt));
        }

        // Jumping (the Rigidbody's gravity pulls us back down)
        jumpBufferTimer -= dt;
        if (jumpBufferTimer > 0f && jumpsUsed < maxJumps && !IsKnockedBack)
        {
            float height = jumpsUsed == 0 ? jumpHeight : jumpHeight * doubleJumpMultiplier;
            velocity.y = Mathf.Sqrt(2f * height * -Physics.gravity.y);
            jumpsUsed++;
            jumpBufferTimer = 0f;
            IsGrounded = false;
            Jumped?.Invoke();
            if (jumpSFX != null) jumpSFX.Play();
        }
        else if (jumpReleased && velocity.y > 0f)
        {
            velocity.y *= jumpCutMultiplier;
        }
        jumpReleased = false;

        velocity.y = Mathf.Max(velocity.y, -maxFallSpeed);
        rb.linearVelocity = new Vector3(planarVelocity.x + platformVelocity.x, velocity.y, planarVelocity.z + platformVelocity.z);

        // Face the direction we're moving (keep facing the enemy while being pushed back)
        if (planarVelocity.sqrMagnitude > 0.01f && !IsKnockedBack)
        {
            Quaternion targetRotation = Quaternion.LookRotation(planarVelocity);
            rb.MoveRotation(Quaternion.RotateTowards(rb.rotation, targetRotation, rotationSpeed * dt));
        }

        CurrentSpeed = planarVelocity.magnitude;
    }

    private void ReadInput()
    {
        moveInput = Vector2.zero;
        if (!InputEnabled) return;

        // On-screen joystick (mobile only); Keyboard.current is usually null on phones
        if (joystick != null && joystick.isActiveAndEnabled) moveInput += joystick.Value;

        // On-screen jump button (mobile only); kept until the next FixedUpdate like Space
        if (jumpButton != null && jumpButton.ConsumePress()) jumpBufferTimer = jumpBufferTime;

        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return;

        // Arrow keys are reserved for rotating the camera
        if (keyboard.aKey.isPressed) moveInput.x -= 1f;
        if (keyboard.dKey.isPressed) moveInput.x += 1f;
        if (keyboard.sKey.isPressed) moveInput.y -= 1f;
        if (keyboard.wKey.isPressed) moveInput.y += 1f;

        // Kept until the next FixedUpdate uses them
        if (keyboard.spaceKey.wasPressedThisFrame) jumpBufferTimer = jumpBufferTime;
        if (keyboard.spaceKey.wasReleasedThisFrame) jumpReleased = true;
    }

    // A thin box just under the BoxCollider: if it touches anything solid that isn't us, we're grounded.
    // Also hands back what we stand on, so a moving platform can carry us.
    private bool CheckGrounded(out Collider ground)
    {
        ground = null;
        Vector3 bottom = transform.TransformPoint(box.center + Vector3.down * (box.size.y * 0.5f));
        Vector3 halfExtents = new Vector3(box.size.x * 0.45f, groundCheckDepth, box.size.z * 0.45f);
        int count = Physics.OverlapBoxNonAlloc(bottom, halfExtents, groundHits, transform.rotation, ~0, QueryTriggerInteraction.Ignore);

        for (int i = 0; i < count; i++)
        {
            if (groundHits[i].attachedRigidbody == rb) continue;
            ground = groundHits[i];
            // Prefer a moving platform if we stand on the seam between it and something else
            if (ground.attachedRigidbody != null && ground.attachedRigidbody.GetComponent<MovingPlatform>() != null) break;
        }
        return ground != null;
    }

    // Push the pet away from whatever hit it (an enemy's kick)
    public void Knockback(Vector3 fromPosition)
    {
        Vector3 away = rb.position - fromPosition;
        away.y = 0f;
        if (away.sqrMagnitude < 0.0001f) away = -transform.forward;

        Vector3 push = away.normalized * (knockbackDistance / knockbackTime);
        push.y = Mathf.Sqrt(2f * knockbackHeight * -Physics.gravity.y);
        rb.linearVelocity = push;

        knockbackTimer = knockbackTime;
        jumpBufferTimer = 0f;
    }

    // Bounce up after jumping on an enemy. Counts as the first jump, so a double jump is still possible.
    public void Bounce()
    {
        Bounce(stompBounceHeight);
    }

    // Bounce up to this height (springs use a big one)
    public void Bounce(float height)
    {
        Vector3 velocity = rb.linearVelocity;
        velocity.y = Mathf.Sqrt(2f * height * -Physics.gravity.y);
        rb.linearVelocity = velocity;

        jumpsUsed = 1;
        jumpBufferTimer = 0f;
        jumpReleased = false; // Releasing Space must not cut the bounce short
        IsGrounded = false;
        Jumped?.Invoke();
    }

    // Put the pet back at the start, standing still (used after a fall)
    public void Respawn()
    {
        RespawnWithCheckPoint(startPosition);
    }

    // Put the pet back at a checkpoint, standing still and facing the way it started
    public void RespawnWithCheckPoint(Vector3 position)
    {
        knockbackTimer = 0f;
        platformVelocity = Vector3.zero;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.position = position;
        rb.rotation = startRotation;
        transform.SetPositionAndRotation(position, startRotation);
    }

    public void ResetRunState()
    {
        Respawn();

        jumpBufferTimer = 0f;
        jumpReleased = false;
        jumpsUsed = 0;
        CurrentSpeed = 0f;
        TargetSpeed = 0f;
    }

    private Vector3 GetCameraRelativeDirection()
    {
        // Normalize so diagonal movement isn't faster
        Vector2 input = Vector2.ClampMagnitude(moveInput, 1f);
        if (cameraTransform == null) return new Vector3(input.x, 0f, input.y);

        Vector3 forward = Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up).normalized;
        Vector3 right = Vector3.ProjectOnPlane(cameraTransform.right, Vector3.up).normalized;
        return Vector3.ClampMagnitude(forward * input.y + right * input.x, 1f);
    }
}
