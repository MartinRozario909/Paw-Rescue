using UnityEngine;

[RequireComponent(typeof(PetController))]
public class PetAnimationController : MonoBehaviour, IResettable
{
    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int IsGroundedHash = Animator.StringToHash("IsGrounded");
    private static readonly int JumpHash = Animator.StringToHash("Jump");
    private static readonly int WinHash = Animator.StringToHash("Win");
    private static readonly int LoseHash = Animator.StringToHash("Lose");

    [SerializeField] private Animator animator;
    [SerializeField] private PetController petController;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (animator == null) animator = GetComponentInChildren<Animator>();
        if (petController == null) petController = GetComponent<PetController>();

        petController.Jumped += OnJumped;
    }

    void OnDestroy()
    {
        if (petController != null) petController.Jumped -= OnJumped;
    }

    // LateUpdate runs after PetController has moved, so the Animator always gets this frame's values
    void LateUpdate()
    {
        // Use the requested speed so Idle/Run switch instantly instead of waiting for the pet to slow down
        animator.SetFloat(SpeedHash, petController.TargetSpeed);
        animator.SetBool(IsGroundedHash, petController.IsGrounded);
    }

    private void OnJumped()
    {
        animator.SetTrigger(JumpHash);
    }

    // Call these from GameManager when the level ends
    public void PlayWin()
    {
        animator.SetTrigger(WinHash);
    }

    public void PlayLose()
    {
        animator.SetTrigger(LoseHash);
    }

    public void ResetRunState()
    {
        // Win/Lose have no way out in the Animator, so restart it from Idle
        animator.Rebind();
        animator.Update(0f);
    }
}
