using System;
using UnityEngine;

// The player's hearts. Takes damage (with a short invulnerable time after each hit,
// during which the pet flashes), heals from heart pickups, and raises Died when health reaches zero.
public class HealthSystem : MonoBehaviour, IResettable
{
    [SerializeField] private int maxHealth = 3;
    [SerializeField] private float invulnerableTime = 1.5f; // After a hit, can't be hurt again for this long
    [SerializeField] private bool isInvincible;            // Tick to take no damage at all (for testing)
    [SerializeField] private AudioSource hurtSFX;

    [Header("Flashing")]
    [SerializeField] private float flashInterval = 0.1f;  // Blinks off and on this often during the safe time
    [SerializeField] private Renderer[] flashRenderers;   // Left empty: every mesh on the pet

    private float invulnerableTimer;

    public int MaxHealth => maxHealth;
    public int CurrentHealth { get; private set; }
    public bool IsFull => CurrentHealth >= maxHealth;
    public bool IsDead => CurrentHealth <= 0;
    public bool IsInvulnerable => invulnerableTimer > 0f;
    public bool IsInvincible
    {
        get => isInvincible;
        set => isInvincible = value;
    }

    // Raised with (current, max) whenever health changes (UI listens to this)
    public event Action<int, int> HealthChanged;
    public event Action Died;

    void Awake()
    {
        CurrentHealth = maxHealth;

        if (flashRenderers == null || flashRenderers.Length == 0)
        {
            // Every mesh, but not particles like the running dust
            flashRenderers = System.Array.FindAll(GetComponentsInChildren<Renderer>(true),
                r => !(r is ParticleSystemRenderer));
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (invulnerableTimer <= 0f) return;

        invulnerableTimer -= Time.deltaTime;
        bool safeTimeOver = invulnerableTimer <= 0f || IsDead;
        SetVisible(safeTimeOver || Mathf.Repeat(invulnerableTimer, flashInterval * 2f) < flashInterval);
    }

    private void SetVisible(bool visible)
    {
        foreach (Renderer r in flashRenderers) r.enabled = visible;
    }

    // Returns true if the damage was taken (false while invincible, invulnerable or already dead)
    public bool TakeDamage(int amount)
    {
        if (amount <= 0 || IsDead || IsInvincible || IsInvulnerable) return false;

        CurrentHealth = Mathf.Max(CurrentHealth - amount, 0);
        invulnerableTimer = invulnerableTime;
        HealthChanged?.Invoke(CurrentHealth, maxHealth);
        if (hurtSFX != null) hurtSFX.Play();

        if (IsDead) Died?.Invoke();
        return true;
    }

    public void Heal(int amount)
    {
        if (amount <= 0 || IsDead) return;

        CurrentHealth = Mathf.Min(CurrentHealth + amount, maxHealth);
        HealthChanged?.Invoke(CurrentHealth, maxHealth);
    }

    public void ResetRunState()
    {
        CurrentHealth = maxHealth;
        invulnerableTimer = 0f;
        SetVisible(true);
        HealthChanged?.Invoke(CurrentHealth, maxHealth);
    }
}
