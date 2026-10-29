using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

// Third-person orbit camera modelled on Kenney's 3D platformer starter kit (view.gd):
// smoothly follows the pet, rotates with arrow keys, mouse drag or touch drag.
public class CameraFollow : MonoBehaviour, IResettable
{
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 targetOffset = new Vector3(0f, 0.2f, 0f);
    [SerializeField] private float distance = 8f;
    [SerializeField] private float followSmoothing = 8f;
    [SerializeField] private float rotationSmoothing = 12f;
    [SerializeField] private float snapDistance = 10f; // Further than this from the pet, jump straight to it

    [Header("Rotation")]
    [SerializeField] private float keyRotationSpeed = 120f;      // Degrees per second for arrow keys
    [SerializeField] private float mouseSensitivity = 0.25f;     // Degrees per pixel dragged
    [SerializeField] private float touchSensitivity = 0.2f;      // Degrees per pixel dragged
    [SerializeField] private float minPitch = 5f;
    [SerializeField] private float maxPitch = 70f;

    [Header("Collision")]
    [SerializeField] private float collisionRadius = 0.3f;
    [SerializeField] private LayerMask collisionLayers = ~0;

    private float yaw;
    private float pitch = 25f;
    private float startYaw;
    private float startPitch;
    private bool snapToTarget;
    private float smoothYaw;
    private float smoothPitch;
    private Vector3 followPosition;
    private int lookTouchId = -1;
    private bool mouseDragging;
    private readonly RaycastHit[] hits = new RaycastHit[8];

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Begin looking the same way the camera already faces in the scene
        yaw = smoothYaw = startYaw = transform.eulerAngles.y;
        smoothPitch = startPitch = pitch;
        if (target != null) followPosition = target.position + targetOffset;
    }

    public void ResetRunState()
    {
        // Snap straight back behind the pet instead of swinging across the level
        yaw = smoothYaw = startYaw;
        pitch = smoothPitch = startPitch;
        lookTouchId = -1;
        mouseDragging = false;
        snapToTarget = true; // The pet may not have moved back yet, so snap on the next LateUpdate
    }

    // LateUpdate runs after the pet has moved this frame, so the camera never lags a frame behind
    void LateUpdate()
    {
        if (target == null) return;
        float dt = Time.deltaTime;

        ReadRotationInput(dt);

        smoothYaw = Mathf.LerpAngle(smoothYaw, yaw, 1f - Mathf.Exp(-rotationSmoothing * dt));
        smoothPitch = Mathf.Lerp(smoothPitch, pitch, 1f - Mathf.Exp(-rotationSmoothing * dt));
        followPosition = Vector3.Lerp(followPosition, target.position + targetOffset, 1f - Mathf.Exp(-followSmoothing * dt));
        // Also snap when the pet teleports far away (respawn after a fall)
        if (snapToTarget || (target.position + targetOffset - followPosition).sqrMagnitude > snapDistance * snapDistance)
        {
            followPosition = target.position + targetOffset;
            snapToTarget = false;
        }

        Quaternion rotation = Quaternion.Euler(smoothPitch, smoothYaw, 0f);
        Vector3 back = rotation * Vector3.back;
        transform.SetPositionAndRotation(followPosition + back * GetUnblockedDistance(back), rotation);
    }

    private void ReadRotationInput(float dt)
    {
        Vector2 look = Vector2.zero;

        Keyboard keyboard = Keyboard.current;
        if (keyboard != null)
        {
            if (keyboard.leftArrowKey.isPressed) look.x += keyRotationSpeed * dt;
            if (keyboard.rightArrowKey.isPressed) look.x -= keyRotationSpeed * dt;
            if (keyboard.upArrowKey.isPressed) look.y -= keyRotationSpeed * dt;
            if (keyboard.downArrowKey.isPressed) look.y += keyRotationSpeed * dt;
        }

        // Touch takes priority: on touch screens Windows also moves the mouse
        Vector2 touchDelta = ReadTouchDrag();
        if (lookTouchId != -1) look += touchDelta * touchSensitivity;
        else look += ReadMouseDrag() * mouseSensitivity;

        yaw += look.x;
        pitch = Mathf.Clamp(pitch - look.y, minPitch, maxPitch);
    }

    private Vector2 ReadMouseDrag()
    {
        Mouse mouse = Mouse.current;
        if (mouse == null) return Vector2.zero;

        // Only start dragging when the click isn't on a UI button
        if (mouse.leftButton.wasPressedThisFrame || mouse.rightButton.wasPressedThisFrame)
            mouseDragging = !IsPointerOverUI();
        if (!mouse.leftButton.isPressed && !mouse.rightButton.isPressed)
            mouseDragging = false;

        // Mouse delta is already "pixels this frame", so it is not multiplied by deltaTime
        return mouseDragging ? mouse.delta.ReadValue() : Vector2.zero;
    }

    private Vector2 ReadTouchDrag()
    {
        Touchscreen touchscreen = Touchscreen.current;
        if (touchscreen == null)
        {
            lookTouchId = -1;
            return Vector2.zero;
        }

        Vector2 delta = Vector2.zero;
        bool lookTouchStillDown = false;

        foreach (var touch in touchscreen.touches)
        {
            int id = touch.touchId.ReadValue();
            if (id == 0) continue; // Unused touch slot

            bool down = touch.press.isPressed;

            // The first finger that touches outside the UI owns the camera until it lifts
            if (lookTouchId == -1 && touch.press.wasPressedThisFrame && !IsPointerOverUI())
                lookTouchId = id;

            if (id == lookTouchId && down)
            {
                lookTouchStillDown = true;
                delta = touch.delta.ReadValue();
            }
        }

        if (!lookTouchStillDown) lookTouchId = -1;
        return delta;
    }

    private static bool IsPointerOverUI()
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }

    // Pull the camera in when a wall or island is between it and the pet
    private float GetUnblockedDistance(Vector3 direction)
    {
        int count = Physics.SphereCastNonAlloc(followPosition, collisionRadius, direction, hits, distance,
            collisionLayers, QueryTriggerInteraction.Ignore);

        float closest = distance;
        for (int i = 0; i < count; i++)
        {
            if (hits[i].transform.IsChildOf(target)) continue; // Ignore the pet itself
            if (hits[i].distance > 0f) closest = Mathf.Min(closest, hits[i].distance);
        }
        return closest;
    }
}
