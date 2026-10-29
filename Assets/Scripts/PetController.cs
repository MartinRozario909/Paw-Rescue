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

    private Rigidbody rb;
    private BoxCollider box;
    private readonly Collider[] groundHits = new Collider[8];
    private Vector2 moveInput;
    private float jumpBufferTimer;
    private bool jumpReleased;
    private int jumpsUsed;
    private Vector3 startPosition;
    private Quaternion startRotation;

    // GameManager turns this off outside of gameplay (menus, game over, win)
    public bool InputEnabled { get; set; } = true;
    public float RunSpeed => runSpeed;
    public float CurrentSpeed { get; private set; }
    // Speed the player is asking for; changes the instant a key is pressed or released
    public float TargetSpeed { get; private set; }
    public bool IsGrounded { get; private set; }

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

        IsGrounded = CheckGrounded();
        if (IsGrounded && velocity.y <= 0.01f) jumpsUsed = 0;

        // Horizontal movement relative to where the camera is looking.
        // Start from the real velocity so walls can stop us.
        Vector3 planarVelocity = new Vector3(velocity.x, 0f, velocity.z);
        Vector3 targetVelocity = GetCameraRelativeDirection() * runSpeed;
        float responsiveness = IsGrounded ? groundResponsiveness : airResponsiveness;
        planarVelocity = Vector3.Lerp(planarVelocity, targetVelocity, 1f - Mathf.Exp(-responsiveness * dt));

        // Jumping (the Rigidbody's gravity pulls us back down)
        jumpBufferTimer -= dt;
        if (jumpBufferTimer > 0f && jumpsUsed < maxJumps)
        {
            float height = jumpsUsed == 0 ? jumpHeight : jumpHeight * doubleJumpMultiplier;
            velocity.y = Mathf.Sqrt(2f * height * -Physics.gravity.y);
            jumpsUsed++;
            jumpBufferTimer = 0f;
            IsGrounded = false;
            Jumped?.Invoke();
        }
        else if (jumpReleased && velocity.y > 0f)
        {
            velocity.y *= jumpCutMultiplier;
        }
        jumpReleased = false;

        velocity.y = Mathf.Max(velocity.y, -maxFallSpeed);
        rb.linearVelocity = new Vector3(planarVelocity.x, velocity.y, planarVelocity.z);

        // Face the direction we're moving
        if (planarVelocity.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(planarVelocity);
            rb.MoveRotation(Quaternion.RotateTowards(rb.rotation, targetRotation, rotationSpeed * dt));
        }

        CurrentSpeed = planarVelocity.magnitude;
    }

    private void ReadInput()
    {
        moveInput = Vector2.zero;

        Keyboard keyboard = Keyboard.current;
        if (keyboard == null || !InputEnabled) return;

        // Arrow keys are reserved for rotating the camera
        if (keyboard.aKey.isPressed) moveInput.x -= 1f;
        if (keyboard.dKey.isPressed) moveInput.x += 1f;
        if (keyboard.sKey.isPressed) moveInput.y -= 1f;
        if (keyboard.wKey.isPressed) moveInput.y += 1f;

        // Kept until the next FixedUpdate uses them
        if (keyboard.spaceKey.wasPressedThisFrame) jumpBufferTimer = jumpBufferTime;
        if (keyboard.spaceKey.wasReleasedThisFrame) jumpReleased = true;
    }

    // A thin box just under the BoxCollider: if it touches anything solid that isn't us, we're grounded
    private bool CheckGrounded()
    {
        Vector3 bottom = transform.TransformPoint(box.center + Vector3.down * (box.size.y * 0.5f));
        Vector3 halfExtents = new Vector3(box.size.x * 0.45f, groundCheckDepth, box.size.z * 0.45f);
        int count = Physics.OverlapBoxNonAlloc(bottom, halfExtents, groundHits, transform.rotation, ~0, QueryTriggerInteraction.Ignore);

        for (int i = 0; i < count; i++)
        {
            if (groundHits[i].attachedRigidbody != rb) return true;
        }
        return false;
    }

    // Put the pet back at the start, standing still (used after a fall)
    public void Respawn()
    {
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.position = startPosition;
        rb.rotation = startRotation;
        transform.SetPositionAndRotation(startPosition, startRotation);
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
