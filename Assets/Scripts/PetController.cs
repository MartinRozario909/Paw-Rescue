using System;
using UnityEngine;
using UnityEngine.InputSystem;

// Movement modelled on Kenney's 3D platformer starter kit (player.gd):
// camera-relative movement, our own gravity, and a double jump.
[RequireComponent(typeof(CharacterController))]
public class PetController : MonoBehaviour
{
    [SerializeField] private float runSpeed = 6f;
    [SerializeField] private float groundResponsiveness = 12f; // How fast we reach run speed on the ground
    [SerializeField] private float airResponsiveness = 6f;     // Less control while in the air
    [SerializeField] private float rotationSpeed = 720f;
    [SerializeField] private float gravity = -25f;             // Stronger than real gravity so jumps feel snappy
    [SerializeField] private float maxFallSpeed = 30f;
    [SerializeField] private float jumpHeight = 1.5f;
    [SerializeField] private float doubleJumpMultiplier = 0.9f; // Second jump is a little weaker
    [SerializeField] private float jumpCutMultiplier = 0.5f;    // Releasing Space early gives a shorter jump
    [SerializeField] private float jumpBufferTime = 0.12f;      // Space pressed just before landing still jumps
    [SerializeField] private int maxJumps = 2;
    [SerializeField] private Transform cameraTransform;

    private CharacterController controller;
    private Vector2 moveInput;
    private Vector3 planarVelocity;
    private float verticalVelocity;
    private float jumpBufferTimer;
    private bool jumpReleased;
    private int jumpsUsed;

    public float RunSpeed => runSpeed;
    public float CurrentSpeed { get; private set; }
    // Speed the player is asking for; changes the instant a key is pressed or released
    public float TargetSpeed { get; private set; }
    public bool IsGrounded { get; private set; }

    // Raised every time the pet jumps (including the double jump)
    public event Action Jumped;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        controller = GetComponent<CharacterController>();
        if (cameraTransform == null && Camera.main != null) cameraTransform = Camera.main.transform;
    }

    // Update is called once per frame
    void Update()
    {
        float dt = Time.deltaTime;
        ReadInput();

        // isGrounded describes the result of last frame's Move
        IsGrounded = controller.isGrounded;
        if (IsGrounded && verticalVelocity < 0f)
        {
            jumpsUsed = 0;
            verticalVelocity = -2f; // Small push down keeps us stuck to the ground
        }

        // Horizontal movement relative to where the camera is looking
        Vector3 targetVelocity = GetCameraRelativeDirection() * runSpeed;
        TargetSpeed = targetVelocity.magnitude;
        float responsiveness = IsGrounded ? groundResponsiveness : airResponsiveness;
        planarVelocity = Vector3.Lerp(planarVelocity, targetVelocity, 1f - Mathf.Exp(-responsiveness * dt));

        // Jumping
        jumpBufferTimer -= dt;
        if (jumpBufferTimer > 0f && jumpsUsed < maxJumps)
        {
            float height = jumpsUsed == 0 ? jumpHeight : jumpHeight * doubleJumpMultiplier;
            verticalVelocity = Mathf.Sqrt(2f * height * -gravity);
            jumpsUsed++;
            jumpBufferTimer = 0f;
            IsGrounded = false;
            Jumped?.Invoke();
        }
        else if (jumpReleased && verticalVelocity > 0f)
        {
            verticalVelocity *= jumpCutMultiplier;
        }

        // Gravity
        verticalVelocity = Mathf.Max(verticalVelocity + gravity * dt, -maxFallSpeed);

        CollisionFlags flags = controller.Move((planarVelocity + Vector3.up * verticalVelocity) * dt);

        // Stop rising when we bump our head
        if ((flags & CollisionFlags.Above) != 0 && verticalVelocity > 0f) verticalVelocity = 0f;

        // Face the direction we're moving
        if (planarVelocity.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(planarVelocity);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * dt);
        }

        CurrentSpeed = planarVelocity.magnitude;
    }

    private void ReadInput()
    {
        moveInput = Vector2.zero;
        jumpReleased = false;

        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return;

        // Arrow keys are reserved for rotating the camera
        if (keyboard.aKey.isPressed) moveInput.x -= 1f;
        if (keyboard.dKey.isPressed) moveInput.x += 1f;
        if (keyboard.sKey.isPressed) moveInput.y -= 1f;
        if (keyboard.wKey.isPressed) moveInput.y += 1f;

        if (keyboard.spaceKey.wasPressedThisFrame) jumpBufferTimer = jumpBufferTime;
        if (keyboard.spaceKey.wasReleasedThisFrame) jumpReleased = true;
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
