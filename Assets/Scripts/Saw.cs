using UnityEngine;

// A spinning saw blade that slides back and forth along a line.
// Put a Hazard and a trigger collider on the same object so it hurts.
[RequireComponent(typeof(Rigidbody))]
public class Saw : MonoBehaviour, IResettable
{
    [SerializeField] private Transform blade;                       // Spun around its own forward axis
    [SerializeField] private Vector3 travel = new Vector3(8f, 0f, 0f); // From the start position to the far end
    [SerializeField] private float speed = 3f;                       // Metres per second
    [SerializeField] private float spinSpeed = 540f;                 // Degrees per second

    private Rigidbody rb;
    private Vector3 startPosition;
    private float timer;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true; // A moving trigger needs a kinematic Rigidbody
        startPosition = rb.position;
    }

    // Update is called once per frame
    void Update()
    {
        blade.Rotate(Vector3.forward, spinSpeed * Time.deltaTime, Space.Self);
    }

    // Physics movement happens in FixedUpdate, in step with the Rigidbody
    void FixedUpdate()
    {
        timer += Time.fixedDeltaTime;
        float length = Mathf.Max(travel.magnitude, 0.01f);
        float progress = Mathf.PingPong(timer * speed / length, 1f);
        rb.MovePosition(startPosition + travel * Mathf.SmoothStep(0f, 1f, progress));
    }

    public void ResetRunState()
    {
        timer = 0f;
        rb.position = startPosition;
        transform.position = startPosition;
    }

    // Show the path when the saw is selected
    void OnDrawGizmosSelected()
    {
        Vector3 from = Application.isPlaying ? startPosition : transform.position;
        Gizmos.color = Color.red;
        Gizmos.DrawLine(from, from + travel);
    }
}
