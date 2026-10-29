// Anything that must go back to its starting state when the player presses Retry.
// GameManager calls ResetRunState() on every IResettable in the scene, so the level
// restarts without reloading the scene.
public interface IResettable
{
    void ResetRunState();
}
