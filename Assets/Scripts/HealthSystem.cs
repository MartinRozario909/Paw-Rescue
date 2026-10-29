using System;
using UnityEngine;

// The player's hearts. Takes damage (with a short invulnerable time after each hit),
// heals from heart pickups, and raises Died when health reaches zero.
public class HealthSystem : MonoBehaviour, IResettable
{
    [SerializeField] private int maxHealth = 3;
    [SerializeField] private float invulnerableTime = 1.5f; // After a hit, can't be hurt again for this long

    private float invulnerableTimer;

    public int MaxHealth => maxHealth;
    public int CurrentHealth { get; private set; }
    public bool IsFull => CurrentHealth >= maxHealth;
    public bool IsDead => CurrentHealth <= 0;
    public bool IsInvulnerable => invulnerableTimer > 0f;

    // Raised with (current, max) whenever health changes (UI listens to this)
    public event Action<int, int> HealthChanged;
    public event Action Died;

    void Awake()
    {
        CurrentHealth = maxHealth;
    }

    // Update is called once per frame
    void Update()
    {
        if (invulnerableTimer > 0f) invulnerableTimer -= Time.deltaTime;
    }

    // Returns true if the damage was taken (false while invulnerable or already dead)
    public bool TakeDamage(int amount)
    {
        if (amount <= 0 || IsDead || IsInvulnerable) return false;

        CurrentHealth = Mathf.Max(CurrentHealth - amount, 0);
        invulnerableTimer = invulnerableTime;
        HealthChanged?.Invoke(CurrentHealth, maxHealth);

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
        HealthChanged?.Invoke(CurrentHealth, maxHealth);
    }
}
