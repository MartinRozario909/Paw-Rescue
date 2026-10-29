using System;
using UnityEngine;

public enum GameState
{
    Menu,
    Playing,
    GameOver,
    Won
}

// Controls the flow of the game: start, end (win or lose) and retry.
// Retry resets every IResettable in place instead of reloading the scene.
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private PetController player;
    [SerializeField] private float killHeight = -15f; // Falling below this costs a heart

    private HealthSystem playerHealth;

    public GameState State { get; private set; } = GameState.Menu;
    public bool IsPlaying => State == GameState.Playing;

    // Raised every time the state changes (UI listens to this)
    public event Action<GameState> StateChanged;

    void Awake()
    {
        Instance = this;
        playerHealth = player.GetComponent<HealthSystem>();
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
    }

    // Update is called once per frame
    void Update()
    {
        if (IsPlaying && player.transform.position.y < killHeight) OnPlayerFell();
    }

    private void OnPlayerFell()
    {
        playerHealth.TakeDamage(1);
        // If that was the last heart, OnPlayerDied has already ended the game
        if (IsPlaying) player.Respawn();
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

    public void Retry()
    {
        if (State != GameState.GameOver && State != GameState.Won) return;
        ResetLevel();
        SetState(GameState.Playing);
    }

    private void ResetLevel()
    {
        // Only runs on Start/Retry, never per frame
        var behaviours = FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (MonoBehaviour behaviour in behaviours)
        {
            if (behaviour is IResettable resettable) resettable.ResetRunState();
        }
    }

    private void SetState(GameState newState)
    {
        State = newState;
        player.InputEnabled = newState == GameState.Playing;

        PetAnimationController petAnimation = player.GetComponent<PetAnimationController>();
        if (newState == GameState.Won && petAnimation != null) petAnimation.PlayWin();

        StateChanged?.Invoke(newState);
    }
}
