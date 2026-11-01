using UnityEngine;

// A platform that shakes when the pet lands on it, then drops away.
// Modelled on Kenney's 3D platformer starter kit (platform_falling.gd): the fall speeds up
// the longer it falls. It is hidden, not destroyed, so Retry can bring it back.
[RequireComponent(typeof(Rigidbody))]
public class FallingPlatform : MonoBehaviour, IResettable
{
    [SerializeField] private Transform visual;          // Shaken while waiting to fall
    [SerializeField] private float delay = 0.5f;        // Seconds of shaking before it drops
    [SerializeField] private float shakeAmount = 0.08f;
    [SerializeField] private float fallAcceleration = 20f;
    [SerializeField] private float hideAfterDrop = 15f; // Metres fallen before it is hidden
    [SerializeField] private bool comeBack = true;      // Untick to keep it gone until Retry
    [SerializeField] private float comeBackDelay = 3f;  // Seconds after it is hidden before it is back in place

    private Rigidbody rb;
    private Collider solid;
    private Vector3 startPosition;
    private Vector3 visualStart;
    private bool triggered;
    private float shakeTimer;
    private float fallSpeed;
    private float comeBackTimer;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        solid = GetComponent<Collider>();
        startPosition = rb.position;
        visualStart = visual.localPosition;
    }

    // Physics movement happens in FixedUpdate, in step with the Rigidbody
    void FixedUpdate()
    {
        float dt = Time.fixedDeltaTime;

        // Waiting to come back after falling away
        if (comeBackTimer > 0f)
        {
            comeBackTimer -= dt;
            if (comeBackTimer <= 0f) ResetRunState();
            return;
        }

        if (!triggered) return;

        if (shakeTimer > 0f)
        {
            shakeTimer -= dt;
            visual.localPosition = visualStart + Random.insideUnitSphere * shakeAmount;
            if (shakeTimer <= 0f) visual.localPosition = visualStart;
            return;
        }

        fallSpeed += fallAcceleration * dt;
        rb.MovePosition(rb.position + Vector3.down * (fallSpeed * dt));

        if (startPosition.y - rb.position.y > hideAfterDrop)
        {
            visual.gameObject.SetActive(false);
            solid.enabled = false;
            triggered = false;
            if (comeBack) comeBackTimer = comeBackDelay;
        }
    }

    // Starts the countdown when the pet lands on top (not when it bumps the side)
    void OnCollisionStay(Collision collision)
    {
        if (triggered || collision.rigidbody == null) return;
        if (collision.rigidbody.GetComponent<PetController>() == null) return;

        if (collision.collider.bounds.min.y >= solid.bounds.max.y - 0.2f)
        {
            triggered = true;
            shakeTimer = delay;
        }
    }

    public void ResetRunState()
    {
        triggered = false;
        shakeTimer = 0f;
        fallSpeed = 0f;
        comeBackTimer = 0f;
        rb.position = startPosition;
        transform.position = startPosition;
        visual.localPosition = visualStart;
        visual.gameObject.SetActive(true);
        solid.enabled = true;
    }
}
