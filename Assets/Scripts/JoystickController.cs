using UnityEngine;
using UnityEngine.EventSystems;

// On-screen joystick for touch screens. Sits on the HUD, so it hides with the HUD.
// Drag the handle inside the border; Value is the stick position from -1 to 1 on each axis.
// Only shown on mobile (phones and tablets, including mobile web browsers).
public class JoystickController : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [SerializeField] private RectTransform border;   // The ring the handle moves inside
    [SerializeField] private RectTransform handle;
    [SerializeField] private float deadZone = 0.1f;  // Tiny drags near the middle count as no input
    [SerializeField] private bool showInEditor;      // Tick to test the joystick in the Editor without the Device Simulator

    private Canvas canvas;

    public Vector2 Value { get; private set; }

    void Awake()
    {
        canvas = GetComponentInParent<Canvas>();
        if (border == null) border = (RectTransform)transform;

        bool show = Application.isMobilePlatform || (Application.isEditor && showInEditor);
        gameObject.SetActive(show);
    }

    // Let go when the HUD hides (pause, game over) so the pet doesn't keep running
    void OnDisable()
    {
        Release();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        OnDrag(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        Camera eventCamera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay ? canvas.worldCamera : null;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(border, eventData.position, eventCamera, out Vector2 localPoint)) return;

        // How far from the middle of the ring, where the ring's edge is 1
        Vector2 radius = border.rect.size * 0.5f;
        Vector2 offset = localPoint - border.rect.center;
        Vector2 stick = Vector2.ClampMagnitude(new Vector2(offset.x / radius.x, offset.y / radius.y), 1f);

        handle.anchoredPosition = stick * radius;
        Value = stick.magnitude < deadZone ? Vector2.zero : stick;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Release();
    }

    private void Release()
    {
        Value = Vector2.zero;
        if (handle != null) handle.anchoredPosition = Vector2.zero;
    }
}
