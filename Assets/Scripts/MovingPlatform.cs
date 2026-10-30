using UnityEngine;

// A platform that slides back and forth between its start and start + travel,
// easing in and out and resting a moment at each end. The pet rides it:
// PetController adds this platform's Velocity while standing on it.
// Moves level (sideways or forwards) only; the pet's own gravity handles up and down.
[RequireComponent(typeof(Rigidbody))]
public class MovingPlatform : MonoBehaviour, IResettable
{
    [SerializeField] private Vector3 travel = new Vector3(0f, 0f, 10f); // From the start position to the far end
    [SerializeField] private float speed = 2.5f;     // Average metres per second
    [SerializeField] private float waitAtEnds = 1f;  // Seconds it rests at each end

    private Rigidbody rb;
    private Vector3 startPosition;
    private float timer;

    // How fast the platform moved in the last physics step
    public Vector3 Velocity { get; private set; }

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true; // Moved by us, not pushed by physics
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        startPosition = rb.position;
    }

    // Physics movement happens in FixedUpdate, in step with the Rigidbody
    void FixedUpdate()
    {
        float dt = Time.fixedDeltaTime;
        timer += dt;

        Vector3 next = startPosition + travel * GetProgress(timer);
        Velocity = (next - rb.position) / dt;
        rb.MovePosition(next);
    }

    // 0 at the start, 1 at the far end: move, wait, move back, wait, repeat
    private float GetProgress(float time)
    {
        float moveTime = Mathf.Max(travel.magnitude / speed, 0.01f);
        float cycle = (moveTime + waitAtEnds) * 2f;
        float t = Mathf.Repeat(time, cycle);

        if (t < moveTime) return Mathf.SmoothStep(0f, 1f, t / moveTime);
        t -= moveTime;
        if (t < waitAtEnds) return 1f;
        t -= waitAtEnds;
        if (t < moveTime) return Mathf.SmoothStep(1f, 0f, t / moveTime);
        return 0f;
    }

    public void ResetRunState()
    {
        timer = 0f;
        Velocity = Vector3.zero;
        rb.position = startPosition;
        transform.position = startPosition;
    }

    // Show the path when the platform is selected
    void OnDrawGizmosSelected()
    {
        Vector3 from = Application.isPlaying ? startPosition : transform.position;
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(from, from + travel);
        Gizmos.DrawWireCube(from + travel, Vector3.one * 0.5f);
    }
}
