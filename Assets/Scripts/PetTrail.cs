using UnityEngine;

// Dust trail modelled on Kenney's 3D platformer starter kit (player.gd):
// dust only appears while the pet is on the ground and running fast.
[RequireComponent(typeof(PetController))]
public class PetTrail : MonoBehaviour, IResettable
{
    [SerializeField] private ParticleSystem dust;
    [SerializeField, Range(0f, 1f)] private float minSpeedFraction = 0.75f; // Fraction of run speed needed to kick up dust

    private PetController petController;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        petController = GetComponent<PetController>();
    }

    // Update is called once per frame
    void Update()
    {
        bool running = petController.IsGrounded
            && petController.CurrentSpeed >= petController.RunSpeed * minSpeedFraction;

        // Toggle emission instead of Play/Stop so particles already in the air fade out naturally
        var emission = dust.emission;
        if (emission.enabled != running) emission.enabled = running;
    }

    public void ResetRunState()
    {
        dust.Clear(); // No leftover dust floating where the pet used to be
    }
}
