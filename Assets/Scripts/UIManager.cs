using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Manages all of the game's UI: shows the right screen for each game state,
// keeps the player's coins (coins add to them; friend cages spend them)
// and keeps the HUD up to date: coins, hearts, rescued friends, the camp's wave counter
// and short messages like "Panda is free!".
public class UIManager : MonoBehaviour, IResettable
{
    public static UIManager Instance { get; private set; }

    [Header("Screens")]
    [SerializeField] private GameObject startPanel;
    [SerializeField] private GameObject optionsPanel;  // Controls list, opened from the start screen
    [SerializeField] private GameObject hudPanel;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject winPanel;

    [Header("Buttons")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private Button optionsButton;
    [SerializeField] private Button optionsBackButton;
    [SerializeField] private Button pauseButton;     // On the HUD
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button pauseMenuButton;
    [SerializeField] private Button retryButton;
    [SerializeField] private Button gameOverMenuButton;
    [SerializeField] private Button playAgainButton;
    [SerializeField] private Button winMenuButton;
    [SerializeField] private AudioSource clickSFX;   // Played by every button above

    [Header("Coins")]
    [SerializeField] private Text coinText;

    [Header("Hearts")]
    [SerializeField] private HealthSystem playerHealth;
    [SerializeField] private GameObject heartStartPosition; // The first heart goes at this object's world position
    [SerializeField] private float heartSpacing = 64f;      // Distance from one heart to the next, in canvas units
    [SerializeField] private Image heartPrefab;
    [SerializeField] private Color fullHeartColor = Color.white;
    [SerializeField] private Color emptyHeartColor = new Color(0.2f, 0.2f, 0.2f, 0.5f);

    [Header("Friends")]
    [SerializeField] private Image[] friendIcons;            // One per friend, in cage order
    [SerializeField] private Color rescuedColor = Color.white;
    [SerializeField] private Color lockedColor = new Color(0.12f, 0.25f, 0.45f, 0.6f); // A faded star on the blue panel

    [Header("Alien Camp")]
    [SerializeField] private ArenaWaves arena;
    [SerializeField] private GameObject waveBox;
    [SerializeField] private Text waveText;

    [Header("Messages")]
    [SerializeField] private Text messageText;
    [SerializeField] private float messageTime = 2.5f;
    [SerializeField] private Text winStatsText;

    private readonly List<Image> hearts = new List<Image>();
    private float messageTimer;

    public int Coins { get; private set; }
    public int CoinsCollected { get; private set; } // Every coin picked up this run, including spent ones

    void Awake()
    {
        Instance = this;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameManager game = GameManager.Instance;
        Bind(playButton, game.StartGame);
        Bind(quitButton, game.QuitGame);
        Bind(optionsButton, () => ShowOptions(true));
        Bind(optionsBackButton, () => ShowOptions(false));
        Bind(pauseButton, game.TogglePause);
        Bind(resumeButton, game.TogglePause);
        Bind(restartButton, game.Retry);
        Bind(pauseMenuButton, game.GoToMenu);
        Bind(retryButton, game.Retry);
        Bind(gameOverMenuButton, game.GoToMenu);
        Bind(playAgainButton, game.Retry);
        Bind(winMenuButton, game.GoToMenu);

        game.StateChanged += OnStateChanged;
        game.FriendRescued += OnFriendRescued;
        playerHealth.HealthChanged += OnHealthChanged;
        if (arena != null) arena.WaveChanged += OnWaveChanged;

        OnStateChanged(game.State);
        UpdateCoinText();
        OnHealthChanged(playerHealth.CurrentHealth, playerHealth.MaxHealth);
        ResetFriendIcons();
        OnWaveChanged(0, 0, 0);
        if (messageText != null) messageText.gameObject.SetActive(false);
    }

    // Buttons a scene doesn't have (like the older test scenes) are simply skipped
    private void Bind(Button button, UnityEngine.Events.UnityAction action)
    {
        if (button == null) return;
        button.onClick.AddListener(PlayClick);
        button.onClick.AddListener(action);
    }

    private void PlayClick()
    {
        if (clickSFX != null) clickSFX.Play();
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.StateChanged -= OnStateChanged;
            GameManager.Instance.FriendRescued -= OnFriendRescued;
        }
        if (playerHealth != null) playerHealth.HealthChanged -= OnHealthChanged;
        if (arena != null) arena.WaveChanged -= OnWaveChanged;
    }

    // Update is called once per frame
    void Update()
    {
        if (messageTimer <= 0f) return;

        messageTimer -= Time.deltaTime;
        // Fade out over the last half second
        Color color = messageText.color;
        color.a = Mathf.Clamp01(messageTimer / 0.5f);
        messageText.color = color;
        if (messageTimer <= 0f) messageText.gameObject.SetActive(false);
    }

    public void AddCoins(int amount)
    {
        Coins += amount;
        CoinsCollected += amount;
        UpdateCoinText();
    }

    // Returns false (and spends nothing) if the player can't afford it
    public bool SpendCoins(int amount)
    {
        if (Coins < amount) return false;
        Coins -= amount;
        UpdateCoinText();
        return true;
    }

    // A short message in the middle of the screen, like "Panda is free!"
    public void ShowMessage(string message)
    {
        if (messageText == null) return;
        messageText.text = message;
        messageText.color = new Color(messageText.color.r, messageText.color.g, messageText.color.b, 1f);
        messageText.gameObject.SetActive(true);
        messageTimer = messageTime;
    }

    // Retry starts again with no coins and nobody rescued
    public void ResetRunState()
    {
        Coins = 0;
        CoinsCollected = 0;
        UpdateCoinText();
        ResetFriendIcons();
        messageTimer = 0f;
        if (messageText != null) messageText.gameObject.SetActive(false);
    }

    // Swaps the start screen for the options screen and back (only used from the menu)
    public void ShowOptions(bool show)
    {
        if (optionsPanel == null) return;
        optionsPanel.SetActive(show);
        startPanel.SetActive(!show);
    }

    private void OnStateChanged(GameState state)
    {
        startPanel.SetActive(state == GameState.Menu);
        if (optionsPanel != null) optionsPanel.SetActive(false);
        hudPanel.SetActive(state == GameState.Playing || state == GameState.Paused);
        if (pausePanel != null) pausePanel.SetActive(state == GameState.Paused);
        gameOverPanel.SetActive(state == GameState.GameOver);
        winPanel.SetActive(state == GameState.Won);

        if (state == GameState.Won) UpdateWinStats();
    }

    private void UpdateWinStats()
    {
        if (winStatsText == null) return;
        int seconds = Mathf.FloorToInt(GameManager.Instance.RunTime);
        winStatsText.text = $"TIME {seconds / 60}:{seconds % 60:00}\nCOINS {CoinsCollected}";
    }

    private void OnFriendRescued(int friendIndex)
    {
        if (friendIndex >= 0 && friendIndex < friendIcons.Length) friendIcons[friendIndex].color = rescuedColor;
    }

    private void ResetFriendIcons()
    {
        foreach (Image icon in friendIcons) icon.color = lockedColor;
    }

    // The wave counter only shows once the camp fight has started
    private void OnWaveChanged(int wave, int waveCount, int aliensLeft)
    {
        if (waveBox == null) return;
        waveBox.SetActive(wave > 0);
        waveText.text = $"WAVE {wave}/{waveCount}   ALIENS {aliensLeft}";
    }

    private void UpdateCoinText()
    {
        coinText.text = Coins.ToString();
    }

    // Called whenever the player takes damage or heals
    private void OnHealthChanged(int current, int max)
    {
        if (hearts.Count != max) SetHeartCount(max);

        // Full hearts for health left, faded hearts for health lost
        for (int i = 0; i < hearts.Count; i++)
            hearts[i].color = i < current ? fullHeartColor : emptyHeartColor;
    }

    // Add or remove heart icons so there is one per point of max health
    private void SetHeartCount(int count)
    {
        while (hearts.Count > count)
        {
            Image extra = hearts[hearts.Count - 1];
            hearts.RemoveAt(hearts.Count - 1);
            Destroy(extra.gameObject);
        }

        Transform start = heartStartPosition.transform;
        // The canvas is scaled to fit the screen, so scale the spacing the same way
        Vector3 step = start.right * (heartSpacing * start.lossyScale.x);

        while (hearts.Count < count)
        {
            Image heart = Instantiate(heartPrefab, start.parent);
            heart.name = "Heart " + (hearts.Count + 1);
            heart.transform.position = start.position + step * hearts.Count;
            hearts.Add(heart);
        }
    }
}
