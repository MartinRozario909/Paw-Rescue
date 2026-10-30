using UnityEngine;

public enum ItemType
{
    Coin,
    Heart
}

// A pickup in the level: a coin or a heart, picked in the Inspector.
// It only spins, bobs and hides itself. The player's Backpack decides what
// picking it up does. It is hidden, not destroyed, so Retry can bring it back.
// Spin and bob are modelled on Kenney's 3D platformer starter kit (coin.gd).
[RequireComponent(typeof(Collider))]
public class Item : MonoBehaviour, IResettable
{
    [SerializeField] private ItemType type = ItemType.Coin;
    [SerializeField] private int value = 1; // Coins added, or health restored
    [SerializeField] private Transform visual;
    [SerializeField] private ParticleSystem pickupEffect;
    [SerializeField] private float spinSpeed = 120f;  // Degrees per second
    [SerializeField] private float bobHeight = 0.15f;
    [SerializeField] private float bobSpeed = 3f;

    private Collider pickupCollider;
    private Vector3 visualStartPosition;
    private float bobOffset;

    public ItemType Type => type;
    public int Value => value;
    public bool IsCollected { get; private set; }

    void Awake()
    {
        pickupCollider = GetComponent<Collider>();
        pickupCollider.isTrigger = true;
        visualStartPosition = visual.localPosition;
        bobOffset = transform.position.x + transform.position.z; // So neighbouring items don't bob in sync
    }

    // Update is called once per frame
    void Update()
    {
        if (IsCollected) return;

        visual.Rotate(Vector3.up, spinSpeed * Time.deltaTime, Space.World);
        // Bob around the start position so the item never drifts
        visual.localPosition = visualStartPosition + Vector3.up * (Mathf.Sin(Time.time * bobSpeed + bobOffset) * bobHeight);
    }

    // Called by the Backpack once it has used the item
    public void Collect()
    {
        if (IsCollected) return;

        IsCollected = true;
        visual.gameObject.SetActive(false);
        pickupCollider.enabled = false;
        if (pickupEffect != null) pickupEffect.Play();
        SoundManager.Play(type == ItemType.Coin ? Sound.Coin : Sound.Heart);
    }

    public void ResetRunState()
    {
        IsCollected = false;
        visual.gameObject.SetActive(true);
        visual.localPosition = visualStartPosition;
        pickupCollider.enabled = true;
        if (pickupEffect != null) pickupEffect.Clear();
    }
}
