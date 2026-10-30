using UnityEngine;

// Launches the pet high into the air when it lands on top, to reach upper islands.
// The pet still has its double jump after the launch.
[RequireComponent(typeof(Collider))]
public class Spring : MonoBehaviour
{
    [SerializeField] private float bounceHeight = 6.5f; // Metres the pet rises above the spring
    [SerializeField] private Transform visual;          // Squashed for a moment on each bounce
    [SerializeField] private float squashTime = 0.25f;

    private Collider solid;
    private Vector3 visualScale;
    private float squashTimer;

    void Awake()
    {
        solid = GetComponent<Collider>();
        if (visual != null) visualScale = visual.localScale;
    }

    // Update is called once per frame
    void Update()
    {
        if (visual == null || squashTimer <= 0f) return;

        squashTimer -= Time.deltaTime;
        // Squashed flat, then springs back past its size and settles
        float t = 1f - Mathf.Clamp01(squashTimer / squashTime);
        float stretch = 1f + Mathf.Sin(t * Mathf.PI * 2f) * 0.35f * (1f - t);
        visual.localScale = new Vector3(visualScale.x, visualScale.y * stretch, visualScale.z);
    }

    void OnCollisionEnter(Collision collision)
    {
        TryBounce(collision);
    }

    void OnCollisionStay(Collision collision)
    {
        TryBounce(collision);
    }

    private void TryBounce(Collision collision)
    {
        if (collision.rigidbody == null) return;
        PetController pet = collision.rigidbody.GetComponent<PetController>();
        if (pet == null) return;

        // Only from above, and only once per landing (the pet is already rising after a bounce)
        if (collision.collider.bounds.min.y < solid.bounds.max.y - 0.2f) return;
        if (collision.rigidbody.linearVelocity.y > 1f) return;

        pet.Bounce(bounceHeight);
        squashTimer = squashTime;
        SoundManager.Play(Sound.Spring);
    }
}
