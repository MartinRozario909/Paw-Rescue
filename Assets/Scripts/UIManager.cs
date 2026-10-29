using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Manages all of the game's UI: shows the right screen for each game state,
// keeps the player's coins (coins add to them; friend cages will spend them)
// and keeps the HUD's coin counter and hearts up to date as they go up and down.
public class UIManager : MonoBehaviour, IResettable
{
    public static UIManager Instance { get; private set; }

    [Header("Screens")]
    [SerializeField] private GameObject startPanel;
    [SerializeField] private GameObject hudPanel;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject winPanel;
    [SerializeField] private Button playButton;
    [SerializeField] private Button retryButton;
    [SerializeField] private Button playAgainButton;

    [Header("Coins")]
    [SerializeField] private Text coinText;

    [Header("Hearts")]
    [SerializeField] private HealthSystem playerHealth;
    [SerializeField] private GameObject heartStartPosition; // The first heart goes at this object's world position
    [SerializeField] private float heartSpacing = 64f;      // Distance from one heart to the next, in canvas units
    [SerializeField] private Image heartPrefab;
    [SerializeField] private Color fullHeartColor = Color.white;
    [SerializeField] private Color emptyHeartColor = new Color(0.2f, 0.2f, 0.2f, 0.5f);

    private readonly List<Image> hearts = new List<Image>();

    public int Coins { get; private set; }

    void Awake()
    {
        Instance = this;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playButton.onClick.AddListener(GameManager.Instance.StartGame);
        retryButton.onClick.AddListener(GameManager.Instance.Retry);
        playAgainButton.onClick.AddListener(GameManager.Instance.Retry);

        GameManager.Instance.StateChanged += OnStateChanged;
        playerHealth.HealthChanged += OnHealthChanged;

        OnStateChanged(GameManager.Instance.State);
        UpdateCoinText();
        OnHealthChanged(playerHealth.CurrentHealth, playerHealth.MaxHealth);
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null) GameManager.Instance.StateChanged -= OnStateChanged;
        if (playerHealth != null) playerHealth.HealthChanged -= OnHealthChanged;
    }

    public void AddCoins(int amount)
    {
        Coins += amount;
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

    // Retry starts again with no coins
    public void ResetRunState()
    {
        Coins = 0;
        UpdateCoinText();
    }

    private void OnStateChanged(GameState state)
    {
        startPanel.SetActive(state == GameState.Menu);
        hudPanel.SetActive(state == GameState.Playing);
        gameOverPanel.SetActive(state == GameState.GameOver);
        winPanel.SetActive(state == GameState.Won);
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
