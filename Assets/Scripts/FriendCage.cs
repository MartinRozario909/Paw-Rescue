using UnityEngine;
using UnityEngine.UI;

// A locked cage with a friend inside. Walking into it with enough coins spends them,
// opens the cage and rescues the friend: it hops for joy, the checkpoint moves here,
// then the friend goes to wait by the final gate. Needs a trigger collider around the cage.
[RequireComponent(typeof(Collider))]
public class FriendCage : MonoBehaviour, IResettable
{
    [SerializeField] private int friendIndex;              // 0, 1 or 2: which HUD icon lights up
    [SerializeField] private string friendName = "Panda";
    [SerializeField] private int cost = 10;
    [SerializeField] private GameObject cage;              // Fences and lock, hidden once opened
    [SerializeField] private Transform friend;
    [SerializeField] private Transform checkpoint;         // Where the pet respawns after a fall, once rescued
    [SerializeField] private Transform waitSpot;           // Where the friend waits after the celebration
    [SerializeField] private Text costLabel;               // World-space "10 COINS" sign above the cage
    [SerializeField] private ParticleSystem celebrateEffect;

    [Header("Celebration")]
    [SerializeField] private float celebrateTime = 2f;
    [SerializeField] private float hopHeight = 0.6f;
    [SerializeField] private float hopSpeed = 9f;
    [SerializeField] private float hintCooldown = 2f;      // Seconds between "need coins" messages

    private Vector3 friendStart;
    private Quaternion friendStartRotation;
    private Vector3 friendBase; // Where the friend stands now (the cage, then the wait spot)
    private bool opened;
    private bool cheering;     // Hops non-stop after the game is won
    private float celebrateTimer;
    private float nextHintTime;

    public bool IsOpen => opened;

    void Awake()
    {
        GetComponent<Collider>().isTrigger = true;
        friendStart = friendBase = friend.position;
        friendStartRotation = friend.rotation;
        if (costLabel != null) costLabel.text = cost + " COINS";
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameManager.Instance.StateChanged += OnStateChanged;
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null) GameManager.Instance.StateChanged -= OnStateChanged;
    }

    // Update is called once per frame
    void Update()
    {
        if (celebrateTimer > 0f)
        {
            celebrateTimer -= Time.deltaTime;
            if (celebrateTimer <= 0f) MoveFriendTo(waitSpot != null ? waitSpot.position : friendBase);
        }

        bool hopping = celebrateTimer > 0f || cheering;
        float hop = hopping ? Mathf.Abs(Mathf.Sin(Time.time * hopSpeed)) * hopHeight : 0f;
        friend.position = friendBase + Vector3.up * hop;
    }

    void OnTriggerEnter(Collider other)
    {
        TryOpen(other);
    }

    // Also while overlapping, so a pet standing at the cage opens it the moment it has enough coins
    void OnTriggerStay(Collider other)
    {
        TryOpen(other);
    }

    private void TryOpen(Collider other)
    {
        if (opened || !GameManager.Instance.IsPlaying) return;
        if (other.attachedRigidbody == null || other.attachedRigidbody.GetComponent<PetController>() == null) return;

        if (UIManager.Instance.SpendCoins(cost))
        {
            Open();
        }
        else if (Time.time >= nextHintTime)
        {
            nextHintTime = Time.time + hintCooldown;
            UIManager.Instance.ShowMessage($"Need {cost} coins to free {friendName}!");
            SoundManager.Play(Sound.Locked);
        }
    }

    private void Open()
    {
        opened = true;
        cage.SetActive(false);
        if (costLabel != null) costLabel.gameObject.SetActive(false);
        if (celebrateEffect != null) celebrateEffect.Play();
        celebrateTimer = celebrateTime;

        GameManager.Instance.RescueFriend(friendIndex);
        if (checkpoint != null) GameManager.Instance.SetCheckpoint(checkpoint.position);
        UIManager.Instance.ShowMessage($"{friendName} is free!");
        SoundManager.Play(Sound.Rescue);
    }

    private void MoveFriendTo(Vector3 position)
    {
        friendBase = position;
        friend.position = position;
        // Face back along the route, towards the pet arriving at the gate
        if (waitSpot != null) friend.rotation = waitSpot.rotation;
    }

    private void OnStateChanged(GameState state)
    {
        cheering = opened && state == GameState.Won;
    }

    public void ResetRunState()
    {
        opened = false;
        cheering = false;
        celebrateTimer = 0f;
        nextHintTime = 0f;
        cage.SetActive(true);
        if (costLabel != null) costLabel.gameObject.SetActive(true);
        if (celebrateEffect != null) celebrateEffect.Clear();

        friendBase = friendStart;
        friend.SetPositionAndRotation(friendStart, friendStartRotation);
    }
}
