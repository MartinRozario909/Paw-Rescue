using UnityEngine;

// The big door in front of the Alien Camp. It stays shut until every friend is free,
// then sinks into the ground. ArenaWaves shuts it again behind the pet during the fight.
public class FinalGate : MonoBehaviour, IResettable
{
    [SerializeField] private Transform door;        // The part that moves
    [SerializeField] private Collider blocker;      // Tall invisible wall that stops the pet while shut
    [SerializeField] private GameObject lockedSign; // "Free 3 friends" sign, hidden once open
    [SerializeField] private AudioSource gateSFX;
    [SerializeField] private float openDepth = 6f;  // How far the door sinks when open
    [SerializeField] private float moveSpeed = 4f;

    private Vector3 closedPosition;
    private bool isOpen;

    public bool IsOpen => isOpen;

    void Awake()
    {
        closedPosition = door.localPosition;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameManager.Instance.FriendRescued += OnFriendRescued;
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null) GameManager.Instance.FriendRescued -= OnFriendRescued;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 target = isOpen ? closedPosition + Vector3.down * openDepth : closedPosition;
        door.localPosition = Vector3.MoveTowards(door.localPosition, target, moveSpeed * Time.deltaTime);
    }

    private void OnFriendRescued(int friendIndex)
    {
        if (!isOpen && GameManager.Instance.AllFriendsRescued) Open();
    }

    public void Open()
    {
        isOpen = true;
        blocker.enabled = false;
        if (lockedSign != null) lockedSign.SetActive(false);
        UIManager.Instance.ShowMessage("The gate is open! Clear the Alien Camp!");
        if (gateSFX != null) gateSFX.Play();
    }

    // Shut behind the pet when the camp fight starts
    public void Lock()
    {
        isOpen = false;
        blocker.enabled = true;
        if (gateSFX != null) gateSFX.Play();
    }

    public void ResetRunState()
    {
        isOpen = false;
        blocker.enabled = true;
        door.localPosition = closedPosition;
        if (lockedSign != null) lockedSign.SetActive(true);
    }
}
