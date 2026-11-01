using UnityEngine;
using UnityEngine.UI;

// On-screen jump button for touch screens. Sits on the HUD, so it hides with the HUD.
// Each click makes the pet jump (tap again in the air for the double jump).
// Only shown on mobile (phones and tablets, including mobile web browsers).
public class JumpButton : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private bool showInEditor; // Tick to test the button in the Editor without the Device Simulator

    private bool pressed;

    void Awake()
    {
        if (button == null) button = GetComponent<Button>();
        button.onClick.AddListener(OnClick);

        bool show = Application.isMobilePlatform || (Application.isEditor && showInEditor);
        gameObject.SetActive(show);
    }

    void OnDestroy()
    {
        if (button != null) button.onClick.RemoveListener(OnClick);
    }

    // Forget any press when the HUD hides (pause, game over) so the pet doesn't jump on return
    void OnDisable()
    {
        pressed = false;
    }

    private void OnClick()
    {
        pressed = true;
    }

    // True once per click; the pet reads it and the flag clears
    public bool ConsumePress()
    {
        bool wasPressed = pressed;
        pressed = false;
        return wasPressed;
    }
}
