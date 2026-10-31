using System;
using UnityEngine;
using UnityEngine.InputSystem;

public enum GameState
{
    Menu,
    Playing,
    Paused,
    GameOver,
    Won
}

// Controls the flow of the game: start, pause, end (win or lose), retry and main menu.
// Also keeps the run's progress: friends rescued, the checkpoint and the run time.
// Retry resets every IResettable in place instead of reloading the scene.
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private PetController player;
    [SerializeField] private float killHeight = -15f; // Falling below this costs a heart
    [SerializeField] private int friendsToRescue = 3;  // The final gate opens once this many are free
    [SerializeField] private AudioSource winSFX;
    [SerializeField] private AudioSource loseSFX;

    private HealthSystem playerHealth;
    private PetAnimationController playerAnimation;
    private Vector3 checkpoint;
    private bool hasCheckpoint;

    public GameState State { get; private set; } = GameState.Menu;
    public bool IsPlaying => State == GameState.Playing;
    public int FriendsRescued { get; private set; }
    public int FriendsToRescue => friendsToRescue;
    public bool AllFriendsRescued => FriendsRescued >= friendsToRescue;
    public float RunTime { get; private set; } // Seconds spent playing this run (pauses don't count)

    // Raised every time the state changes (UI listens to this)
    public event Action<GameState> StateChanged;
    // Raised with the friend's index (0, 1, 2) when a cage is opened
    public event Action<int> FriendRescued;

    void Awake()
    {
        Instance = this;
        playerHealth = player.GetComponent<HealthSystem>();
        playerAnimation = player.GetComponent<PetAnimationController>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerHealth.Died += OnPlayerDied;
        SetState(GameState.Menu);
    }

    void OnDestroy()
    {
        if (playerHealth != null) playerHealth.Died -= OnPlayerDied;
        Time.timeScale = 1f;
    }

    // Update is called once per frame
    void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard != null && (keyboard.escapeKey.wasPressedThisFrame || keyboard.pKey.wasPressedThisFrame))
            TogglePause();

        if (!IsPlaying) return;

        RunTime += Time.deltaTime;
        if (player.transform.position.y < killHeight) OnPlayerFell();
    }

    private void OnPlayerFell()
    {
        playerHealth.TakeDamage(1);
        // If that was the last heart, OnPlayerDied has already ended the game
        if (IsPlaying)
        {
            if (hasCheckpoint) player.Respawn(checkpoint);
            else player.Respawn();
        }
    }

    private void OnPlayerDied()
    {
        EndGame(false);
    }

    public void StartGame()
    {
        if (State != GameState.Menu) return;
        ResetLevel();
        SetState(GameState.Playing);
    }

    public void EndGame(bool won)
    {
        if (!IsPlaying) return; // Only end once
        SetState(won ? GameState.Won : GameState.GameOver);
    }

    // Retry after winning or losing, or Restart from the pause menu
    public void Retry()
    {
        if (State == GameState.Menu || State == GameState.Playing) return;
        ResetLevel();
        SetState(GameState.Playing);
    }

    public void TogglePause()
    {
        if (State == GameState.Playing) SetState(GameState.Paused);
        else if (State == GameState.Paused) SetState(GameState.Playing);
    }

    // Back to the start screen with the level already reset
    public void GoToMenu()
    {
        if (State == GameState.Menu) return;
        ResetLevel();
        SetState(GameState.Menu);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // Called by a FriendCage when it opens
    public void RescueFriend(int friendIndex)
    {
        FriendsRescued++;
        FriendRescued?.Invoke(friendIndex);
    }

    // Falling off the world respawns the pet here instead of at the start
    public void SetCheckpoint(Vector3 position)
    {
        checkpoint = position;
        hasCheckpoint = true;
    }

    private void ResetLevel()
    {
        FriendsRescued = 0;
        RunTime = 0f;
        hasCheckpoint = false;

        // Only runs on Start/Retry/Menu, never per frame
        var behaviours = FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include);
        foreach (MonoBehaviour behaviour in behaviours)
        {
            if (behaviour is IResettable resettable) resettable.ResetRunState();
        }
    }

    private void SetState(GameState newState)
    {
        State = newState;
        player.InputEnabled = newState == GameState.Playing;
        Time.timeScale = newState == GameState.Paused ? 0f : 1f; // Freeze physics and timers while paused

        if (playerAnimation != null)
        {
            if (newState == GameState.Won) playerAnimation.PlayWin();
            else if (newState == GameState.GameOver) playerAnimation.PlayLose();
        }
        if (newState == GameState.Won && winSFX != null) winSFX.Play();
        else if (newState == GameState.GameOver && loseSFX != null) loseSFX.Play();

        StateChanged?.Invoke(newState);
    }
}
