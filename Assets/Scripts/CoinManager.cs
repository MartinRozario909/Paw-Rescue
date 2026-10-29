using System;
using UnityEngine;

// Keeps track of the player's coins. Coins add to it; friend cages will spend from it.
public class CoinManager : MonoBehaviour, IResettable
{
    public static CoinManager Instance { get; private set; }

    public int Coins { get; private set; }

    // Raised with the new total whenever coins change (UI listens to this)
    public event Action<int> CoinsChanged;

    void Awake()
    {
        Instance = this;
    }

    public void AddCoins(int amount)
    {
        Coins += amount;
        CoinsChanged?.Invoke(Coins);
    }

    // Returns false (and spends nothing) if the player can't afford it
    public bool SpendCoins(int amount)
    {
        if (Coins < amount) return false;
        Coins -= amount;
        CoinsChanged?.Invoke(Coins);
        return true;
    }

    public void ResetRunState()
    {
        Coins = 0;
        CoinsChanged?.Invoke(Coins);
    }
}
