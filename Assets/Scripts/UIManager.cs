using UnityEngine;
using UnityEngine.UI;

// Shows the right screen for each game state and keeps the HUD up to date.
// It only reacts to events; GameManager, CoinManager and HealthSystem own the actual data.
public class UIManager : MonoBehaviour
{
    [SerializeField] private GameObject startPanel;
    [SerializeField] private GameObject hudPanel;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject winPanel;
    [SerializeField] private Text coinText;
    [SerializeField] private HealthSystem playerHealth;
    [SerializeField] private Image[] heartIcons;
    [SerializeField] private Color emptyHeartColor = new Color(0.2f, 0.2f, 0.2f, 0.5f);
    [SerializeField] private Button playButton;
    [SerializeField] private Button retryButton;
    [SerializeField] private Button playAgainButton;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playButton.onClick.AddListener(GameManager.Instance.StartGame);
        retryButton.onClick.AddListener(GameManager.Instance.Retry);
        playAgainButton.onClick.AddListener(GameManager.Instance.Retry);

        GameManager.Instance.StateChanged += OnStateChanged;
        CoinManager.Instance.CoinsChanged += OnCoinsChanged;
        playerHealth.HealthChanged += OnHealthChanged;

        OnStateChanged(GameManager.Instance.State);
        OnCoinsChanged(CoinManager.Instance.Coins);
        OnHealthChanged(playerHealth.CurrentHealth, playerHealth.MaxHealth);
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null) GameManager.Instance.StateChanged -= OnStateChanged;
        if (CoinManager.Instance != null) CoinManager.Instance.CoinsChanged -= OnCoinsChanged;
        if (playerHealth != null) playerHealth.HealthChanged -= OnHealthChanged;
    }

    private void OnStateChanged(GameState state)
    {
        startPanel.SetActive(state == GameState.Menu);
        hudPanel.SetActive(state == GameState.Playing);
        gameOverPanel.SetActive(state == GameState.GameOver);
        winPanel.SetActive(state == GameState.Won);
    }

    private void OnCoinsChanged(int coins)
    {
        coinText.text = coins.ToString();
    }

    private void OnHealthChanged(int current, int max)
    {
        // Full hearts for health left, faded hearts for health lost
        for (int i = 0; i < heartIcons.Length; i++)
        {
            heartIcons[i].gameObject.SetActive(i < max);
            heartIcons[i].color = i < current ? Color.white : emptyHeartColor;
        }
    }
}
