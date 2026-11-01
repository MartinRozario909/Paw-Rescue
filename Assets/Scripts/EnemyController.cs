using UnityEngine;

public enum EnemyState
{
    Idle,       // Standing still: at its own spot, or resting at a patrol spot
    Patrolling, // Walking to the other patrol spot
    Attacking,  // Walking up to the player and kicking
    GoingBack,  // Lost the player, walking back to its spot or patrol route
    Death
}

// A walking enemy in the style of Kenney's platformer enemies. It stands at its spot,
// or with Will Patrol ticked walks between Spot1 and Spot2, resting a random time at each.
// When the player comes within its search radius it walks over and kicks.
// The kick only hurts if the player is inside the attack collider (in front of
// its feet) while the foot is swinging out. It won't walk off ledges and goes back
// to its spot or route when the player gets away. Jumping on its head damages it.
[RequireComponent(typeof(Rigidbody))]
public class EnemyController : MonoBehaviour, IResettable
{
    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int KickHash = Animator.StringToHash("Kick");
    private static readonly int KickLeftHash = Animator.StringToHash("KickLeft");
    private static readonly int KickTagHash = Animator.StringToHash("Kick"); // Tag on the Kick Left and Kick Right states
    private static readonly int DieHash = Animator.StringToHash("Die");

    [SerializeField] private float speed = 2.5f;
    [SerializeField] private float searchRadius = 6f;    // Starts chasing when the player is this close
    [SerializeField] private float attackRange = 1.5f;   // Starts a kick when the player is this close
    [SerializeField] private BoxCollider attackCollider; // Trigger in front of the feet; the kick hits whatever is inside it
    [SerializeField] private Collider bodyCollider;      // The solid body; the player lands on top of it to stomp
    [SerializeField] private Animator animator;
    [SerializeField] private Transform player;           // Found automatically if left empty
    [SerializeField] private AudioSource stompSFX;

    [Header("Kick")]
    [SerializeField] private int damage = 1;
    [SerializeField] private float kickDuration = 0.5f;  // Length of the attack-kick-left/right clips (safety limit if the Animator is off)
    [SerializeField] private float hitStartTime = 0.2f;  // Seconds into the kick animation that the foot is out in front
    [SerializeField] private float hitEndTime = 0.4f;
    [SerializeField] private float attackCooldown = 1f;

    [Header("Health")]
    [SerializeField] private int maxHealth = 1;          // Stomps needed to defeat it
    [SerializeField] private float stompMargin = 0.25f;  // The player's feet must be at most this far below the top of the head
    [SerializeField] private float hideAfterDeath = 1f;  // Seconds the body stays after the death animation starts

    [Header("Movement")]
    [SerializeField] private float rotationSpeed = 540f;
    [SerializeField] private float ledgeCheckDistance = 0.7f; // Looks this far ahead for ground before walking

    [Header("Patrol")]
    [SerializeField] private bool willPatrol;            // Off: stands at its start spot. On: walks between Spot1 and Spot2
    [SerializeField] private Transform spot1;
    [SerializeField] private Transform spot2;
    [SerializeField] private float timeToStandMin = 1f;  // Rests at each spot for a random time between Min and Max seconds
    [SerializeField] private float timeToStandMax = 3f;

    private const float ArriveDistance = 0.1f; // Close enough to a spot to count as there

    private Rigidbody rb;
    private HealthSystem playerHealth;
    private readonly Collider[] hits = new Collider[8];
    private Vector3 startPosition;
    private Quaternion startRotation;
    private bool isKicking;
    private bool kickLanded;
    private bool kickAnimationStarted;
    private bool nextKickLeft; // Kicks alternate right, left, right...
    private float kickTimer;
    private float cooldownTimer;
    private int health;
    private float deathTimer;
    private Vector3 spot1Position; // Saved at start, so spots placed under the enemy don't move with it
    private Vector3 spot2Position;
    private bool headingToSpot1;
    private float standTimer;

    public EnemyState State { get; private set; }
    public bool IsKicking => isKicking;
    public bool IsDead => State == EnemyState.Death;
    public int Health => health;

    // The spot it is walking to, or resting at before walking to the other one
    private Vector3 PatrolTarget => headingToSpot1 ? spot1Position : spot2Position;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = true;
        rb.freezeRotation = true; // We turn the enemy ourselves
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        if (animator == null) animator = GetComponentInChildren<Animator>();
        // Hiding the dead body puts it back in its standing pose, so Retry doesn't bring it back squashed
        animator.writeDefaultValuesOnDisable = true;
        if (bodyCollider == null) bodyCollider = GetComponent<Collider>();
        attackCollider.isTrigger = true;
        attackCollider.enabled = false; // Only switched on during the kick

        health = maxHealth;
        startPosition = transform.position;
        startRotation = transform.rotation;

        if (willPatrol && (spot1 == null || spot2 == null))
        {
            Debug.LogWarning($"{name}: Will Patrol is on but Spot1 or Spot2 is empty, so it will stand still.", this);
            willPatrol = false;
        }
        if (willPatrol)
        {
            spot1Position = spot1.position;
            spot2Position = spot2.position;
        }

        EnterStartState();
    }

    // Keep Max from going below Min in the Inspector
    void OnValidate()
    {
        timeToStandMin = Mathf.Max(0f, timeToStandMin);
        timeToStandMax = Mathf.Max(timeToStandMin, timeToStandMax);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (player == null)
        {
            PetController pet = FindAnyObjectByType<PetController>();
            if (pet != null) player = pet.transform;
        }
        if (player != null) playerHealth = player.GetComponent<HealthSystem>();
    }

    // Physics movement happens in FixedUpdate, in step with the Rigidbody
    void FixedUpdate()
    {
        float dt = Time.fixedDeltaTime;

        if (State == EnemyState.Death)
        {
            // Let the death animation play, then hide the body
            if (deathTimer > 0f)
            {
                deathTimer -= dt;
                if (deathTimer <= 0f) animator.gameObject.SetActive(false);
            }
            return;
        }

        Vector3 move = Vector3.zero;
        Vector3 look = Vector3.zero;

        // Seeing the player interrupts standing, patrolling and going back
        bool seesPlayer = CanSeePlayer(out Vector3 toPlayer);
        if (seesPlayer) State = EnemyState.Attacking;
        if (!isKicking) cooldownTimer -= dt;

        switch (State)
        {
            case EnemyState.Idle:
                // Only a patrolling enemy moves on; the other one stands here until it sees the player
                if (willPatrol)
                {
                    standTimer -= dt;
                    if (standTimer <= 0f) State = EnemyState.Patrolling;
                }
                break;

            case EnemyState.Patrolling:
                move = look = FlatDirectionTo(PatrolTarget, out bool reachedSpot);
                // A ledge in the way counts as reaching the spot, so it turns round instead of getting stuck
                if (reachedSpot || !HasGroundAhead(move))
                {
                    move = look = Vector3.zero;
                    StartStanding();
                }
                break;

            case EnemyState.Attacking:
                if (isKicking)
                {
                    UpdateKick(dt); // Stand still until the kick is over, even if the player has left
                }
                else if (!seesPlayer)
                {
                    State = EnemyState.GoingBack;
                }
                else
                {
                    look = toPlayer;
                    if (toPlayer.magnitude <= attackRange)
                    {
                        if (cooldownTimer <= 0f) StartKick(toPlayer);
                    }
                    else
                    {
                        move = toPlayer.normalized;
                    }
                }
                break;

            case EnemyState.GoingBack:
                // Back to its own spot, or to the patrol spot it was heading for
                Vector3 home = willPatrol ? PatrolTarget : startPosition;
                move = look = FlatDirectionTo(home, out bool reachedHome);
                if (reachedHome || !HasGroundAhead(move))
                {
                    move = look = Vector3.zero;
                    if (willPatrol) StartStanding();
                    else State = EnemyState.Idle;
                }
                break;
        }

        if (move != Vector3.zero && !HasGroundAhead(move)) move = Vector3.zero;

        Vector3 velocity = rb.linearVelocity;
        rb.linearVelocity = new Vector3(move.x * speed, velocity.y, move.z * speed);

        if (look.sqrMagnitude > 0.001f)
            rb.MoveRotation(Quaternion.RotateTowards(rb.rotation, Quaternion.LookRotation(look), rotationSpeed * dt));

        animator.SetFloat(SpeedHash, move.magnitude * speed);
    }

    // The player landing on our head is a stomp; bumping into our sides does nothing
    void OnCollisionEnter(Collision collision)
    {
        if (IsDead || collision.rigidbody == null) return;

        PetController pet = collision.rigidbody.GetComponent<PetController>();
        if (pet == null) return;

        float headTop = bodyCollider.bounds.max.y;
        float petFeet = collision.collider.bounds.min.y;
        if (petFeet >= headTop - stompMargin) Stomped(pet);
    }

    private void Stomped(PetController pet)
    {
        pet.Bounce();
        if (stompSFX != null) stompSFX.Play();

        health--;
        if (health <= 0) Die();
    }

    private void Die()
    {
        State = EnemyState.Death;
        isKicking = false;
        attackCollider.enabled = false;
        bodyCollider.enabled = false;

        // No collider any more, so stop it falling through the floor
        rb.linearVelocity = Vector3.zero;
        rb.isKinematic = true;

        animator.SetFloat(SpeedHash, 0f);
        animator.SetTrigger(DieHash);
        deathTimer = hideAfterDeath;
    }

    // Flat direction to the player, if the game is running and the player is within the search radius
    private bool CanSeePlayer(out Vector3 toPlayer)
    {
        toPlayer = Vector3.zero;
        if (player == null) return false;
        if (GameManager.Instance != null && !GameManager.Instance.IsPlaying) return false;
        if (playerHealth != null && playerHealth.IsDead) return false;

        toPlayer = player.position - rb.position;
        toPlayer.y = 0f;
        return toPlayer.magnitude <= searchRadius;
    }

    private void StartKick(Vector3 toPlayer)
    {
        isKicking = true;
        kickLanded = false;
        kickAnimationStarted = false;
        kickTimer = 0f;
        rb.MoveRotation(Quaternion.LookRotation(toPlayer)); // Face the player before kicking

        // KickLeft picks which leg the Animator uses for this kick
        animator.SetBool(KickLeftHash, nextKickLeft);
        animator.SetTrigger(KickHash);
        nextKickLeft = !nextKickLeft;
    }

    private void UpdateKick(float dt)
    {
        kickTimer += dt;

        // Follow the Animator, not our own clock, so the enemy never walks while the kick is still playing
        AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
        bool inKickState = state.tagHash == KickTagHash;
        if (inKickState) kickAnimationStarted = true;

        float kickTime = inKickState ? state.normalizedTime * state.length : 0f;
        bool footOut = inKickState && kickTime >= hitStartTime && kickTime < hitEndTime;
        attackCollider.enabled = footOut;
        if (footOut && !kickLanded) CheckKickHit();

        // Done once the Animator has left the kick state (the timer only matters if the Animator is off)
        bool animationFinished = kickAnimationStarted && !inKickState;
        if (animationFinished || kickTimer >= kickDuration * 2f)
        {
            isKicking = false;
            attackCollider.enabled = false;
            cooldownTimer = attackCooldown;
        }
    }

    // Look inside the attack collider for anything with health (the player)
    private void CheckKickHit()
    {
        Transform box = attackCollider.transform;
        Vector3 center = box.TransformPoint(attackCollider.center);
        Vector3 halfExtents = Vector3.Scale(attackCollider.size, box.lossyScale) * 0.5f;
        int count = Physics.OverlapBoxNonAlloc(center, halfExtents, hits, box.rotation, ~0, QueryTriggerInteraction.Ignore);

        for (int i = 0; i < count; i++)
        {
            Rigidbody body = hits[i].attachedRigidbody;
            if (body == null || body == rb) continue;

            HealthSystem targetHealth = body.GetComponent<HealthSystem>();
            if (targetHealth == null) continue;

            kickLanded = true; // One hit per kick
            if (targetHealth.TakeDamage(damage))
            {
                // The pet decides how far it gets pushed (Knockback Distance on PetController)
                PetController pet = body.GetComponent<PetController>();
                if (pet != null) pet.Knockback(rb.position);
            }
            return;
        }
    }

    // A patrolling enemy starts by walking to Spot1; the other one stands at its spot
    private void EnterStartState()
    {
        headingToSpot1 = true;
        standTimer = 0f;
        State = willPatrol ? EnemyState.Patrolling : EnemyState.Idle;
    }

    // Rest at this patrol spot for a random time, then head for the other one
    private void StartStanding()
    {
        State = EnemyState.Idle;
        standTimer = Random.Range(timeToStandMin, timeToStandMax);
        headingToSpot1 = !headingToSpot1;
    }

    // Flat direction to a point, or zero once within ArriveDistance of it
    private Vector3 FlatDirectionTo(Vector3 target, out bool arrived)
    {
        Vector3 offset = target - rb.position;
        offset.y = 0f;
        arrived = offset.magnitude <= ArriveDistance;
        return arrived ? Vector3.zero : offset.normalized;
    }

    // A short ray down just ahead of the enemy; no ground means a ledge
    private bool HasGroundAhead(Vector3 direction)
    {
        Vector3 origin = rb.position + direction * ledgeCheckDistance + Vector3.up * 0.5f;
        return Physics.Raycast(origin, Vector3.down, 1.5f, ~0, QueryTriggerInteraction.Ignore);
    }

    public void ResetRunState()
    {
        // An arena enemy that was never switched on hasn't run Awake yet, so it is already as it started
        if (rb == null) return;

        EnterStartState();
        health = maxHealth;
        deathTimer = 0f;
        bodyCollider.enabled = true;
        rb.isKinematic = false;
        animator.gameObject.SetActive(true);

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.position = startPosition;
        rb.rotation = startRotation;
        transform.SetPositionAndRotation(startPosition, startRotation);

        isKicking = false;
        kickLanded = false;
        kickAnimationStarted = false;
        nextKickLeft = false;
        kickTimer = 0f;
        cooldownTimer = 0f;
        attackCollider.enabled = false;

        // Back to the standing pose first (the body may still be mid-death), then restart the Animator.
        // Rebind alone would keep whatever pose the body is in as its new default.
        if (animator.isActiveAndEnabled)
        {
            animator.WriteDefaultValues();
            animator.Rebind();
            animator.Update(0f);
        }
    }

    // Show the search radius (yellow), attack range (red) and patrol route (cyan) when the enemy is selected
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, searchRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        if (willPatrol && spot1 != null && spot2 != null)
        {
            // In Play Mode use the saved positions, since spots under the enemy move with it
            Vector3 a = Application.isPlaying ? spot1Position : spot1.position;
            Vector3 b = Application.isPlaying ? spot2Position : spot2.position;
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(a, b);
            Gizmos.DrawWireSphere(a, 0.3f);
            Gizmos.DrawWireSphere(b, 0.3f);
        }
    }
}
