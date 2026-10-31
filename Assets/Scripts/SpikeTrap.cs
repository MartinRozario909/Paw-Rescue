using UnityEngine;

// Spikes that pop out of their plate, stay out for a while, then sink back in, over and over.
// They only hurt while out: the Hazard's trigger is switched off while they are in, so the pet
// can walk across. Just before popping out they peek up a little as a warning.
[RequireComponent(typeof(Hazard))]
public class SpikeTrap : MonoBehaviour, IResettable
{
    [SerializeField] private Transform spikes;          // The moving part (the "spikes" child of the model)
    [SerializeField] private float timeOut = 1.5f;      // Seconds the spikes stay out (and hurt)
    [SerializeField] private float timeIn = 2f;         // Seconds the spikes stay hidden (safe to walk on)
    [SerializeField] private float warningTime = 0.5f;  // Seconds they peek out before popping up
    [SerializeField] private float startDelay;          // Shifts the cycle so neighbouring traps don't move together
    [SerializeField] private float moveSpeed = 1.5f;    // Model units per second (the model is scaled up in the level)

    [Header("Spike heights (model units)")]
    [SerializeField] private float outHeight = 0f;
    [SerializeField] private float peekHeight = -0.12f;
    [SerializeField] private float inHeight = -0.17f;

    private Collider hurtZone;
    private float timer;

    // True while the spikes are up far enough to hurt
    public bool IsOut => hurtZone.enabled;

    void Awake()
    {
        hurtZone = GetComponent<Collider>();
        ResetRunState();
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;

        // One cycle: hidden, peek (warning), then out
        float t = Mathf.Repeat(timer, timeIn + timeOut);
        float target = t < timeIn - warningTime ? inHeight : t < timeIn ? peekHeight : outHeight;

        Vector3 position = spikes.localPosition;
        position.y = Mathf.MoveTowards(position.y, target, moveSpeed * Time.deltaTime);
        spikes.localPosition = position;

        // Hurts once the spikes are more than halfway from the peek to fully out
        hurtZone.enabled = position.y > (peekHeight + outHeight) * 0.5f;
    }

    public void ResetRunState()
    {
        // A trap that was never switched on hasn't run Awake yet, so it is already as it started
        if (hurtZone == null) return;

        timer = startDelay;
        Vector3 position = spikes.localPosition;
        position.y = inHeight;
        spikes.localPosition = position;
        hurtZone.enabled = false;
    }
}
