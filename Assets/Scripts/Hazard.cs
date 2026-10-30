using UnityEngine;

// Spikes, saws and anything else that hurts on touch. Needs a trigger collider.
// Costs the pet a heart and knocks it back; the pet's short invulnerable time
// stops one touch from draining several hearts.
[RequireComponent(typeof(Collider))]
public class Hazard : MonoBehaviour
{
    [SerializeField] private int damage = 1;

    void Awake()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        TryHurt(other);
    }

    // Also while overlapping, so standing on spikes hurts again once the invulnerable time is over
    void OnTriggerStay(Collider other)
    {
        TryHurt(other);
    }

    private void TryHurt(Collider other)
    {
        if (GameManager.Instance != null && !GameManager.Instance.IsPlaying) return;
        if (other.attachedRigidbody == null) return;

        PetController pet = other.attachedRigidbody.GetComponent<PetController>();
        HealthSystem health = other.attachedRigidbody.GetComponent<HealthSystem>();
        if (pet == null || health == null) return;

        if (health.TakeDamage(damage)) pet.Knockback(transform.position);
    }
}
