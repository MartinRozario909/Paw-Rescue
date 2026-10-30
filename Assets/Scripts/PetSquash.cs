using UnityEngine;

// Squash and stretch, modelled on Kenney's 3D platformer starter kit (player.gd):
// the pet's body stretches when it jumps and squashes when it lands, then springs back.
// Visual only: it scales the Body Root, never the collider.
[RequireComponent(typeof(PetController))]
public class PetSquash : MonoBehaviour, IResettable
{
    [SerializeField] private Transform body;              // Body Root, the parent of the model
    [SerializeField] private float feetHeight = 1.2f;     // Distance from the pet's pivot down to its feet
    [SerializeField] private Vector3 jumpStretch = new Vector3(0.85f, 1.2f, 0.85f);
    [SerializeField] private Vector3 landSquash = new Vector3(1.2f, 0.8f, 1.2f);
    [SerializeField] private float recoverSpeed = 10f;    // How fast the body returns to its normal shape

    private PetController petController;
    private Vector3 bodyStartPosition;
    private bool wasGrounded = true;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        petController = GetComponent<PetController>();
        bodyStartPosition = body.localPosition;
        petController.Jumped += OnJumped;
    }

    void OnDestroy()
    {
        if (petController != null) petController.Jumped -= OnJumped;
    }

    // LateUpdate runs after PetController has moved, so landing is seen the same frame
    void LateUpdate()
    {
        if (petController.IsGrounded && !wasGrounded) body.localScale = landSquash;
        wasGrounded = petController.IsGrounded;

        body.localScale = Vector3.Lerp(body.localScale, Vector3.one, 1f - Mathf.Exp(-recoverSpeed * Time.deltaTime));
        // Scaling happens around the pivot, so move the body down to keep the feet on the ground
        body.localPosition = bodyStartPosition + Vector3.down * (feetHeight * (1f - body.localScale.y));
    }

    private void OnJumped()
    {
        body.localScale = jumpStretch;
    }

    public void ResetRunState()
    {
        if (body == null) return;
        body.localScale = Vector3.one;
        if (petController != null) body.localPosition = bodyStartPosition;
        wasGrounded = true;
    }
}
