using UnityEngine;

// Sits on the player. When the player's BoxCollider touches an Item's trigger,
// the Backpack checks what kind of Item it is and uses it.
[RequireComponent(typeof(HealthSystem))]
public class Backpack : MonoBehaviour
{
    private HealthSystem health;

    void Awake()
    {
        health = GetComponent<HealthSystem>();
    }

    void OnTriggerEnter(Collider other)
    {
        TryPickUp(other);
    }

    // Also check while overlapping, so a heart skipped at full health
    // can still be picked up after taking damage without stepping off it
    void OnTriggerStay(Collider other)
    {
        TryPickUp(other);
    }

    private void TryPickUp(Collider other)
    {
        Item item = other.GetComponent<Item>();
        if (item == null || item.IsCollected) return;
        if (GameManager.Instance != null && !GameManager.Instance.IsPlaying) return;

        switch (item.Type)
        {
            case ItemType.Coin:
                UIManager.Instance.AddCoins(item.Value);
                item.Collect();
                break;

            case ItemType.Heart:
                if (health.IsFull) return; // Leave the heart for later
                health.Heal(item.Value);
                item.Collect();
                break;
        }
    }
}
