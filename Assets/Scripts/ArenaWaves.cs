using System;
using UnityEngine;

// The Alien Camp fight. When the pet steps into the camp (this object's trigger),
// the gate shuts behind it and the first wave of aliens appears. Each wave is a
// switched-off parent object full of enemies; when they are all defeated the next
// wave appears. Clearing the last wave wins the game.
[RequireComponent(typeof(Collider))]
public class ArenaWaves : MonoBehaviour, IResettable
{
    [SerializeField] private GameObject[] waves;         // Switched off until their turn
    [SerializeField] private FinalGate gate;             // Shut behind the pet when the fight starts
    [SerializeField] private Transform respawnPoint;     // Falling off during the fight respawns the pet here
    [SerializeField] private ParticleSystem spawnEffect; // Played at each alien as its wave appears
    [SerializeField] private AudioSource waveSFX;
    [SerializeField] private float timeBetweenWaves = 2f;
    [SerializeField] private float winDelay = 1.5f;      // Time to enjoy the last stomp before the Win screen

    private EnemyController[] currentEnemies = new EnemyController[0];
    private int currentWave = -1;
    private int aliensLeft;
    private float waitTimer;
    private bool started;
    private bool finished;

    public int WaveCount => waves.Length;
    public bool Started => started;

    // Raised with (wave number from 1, wave count, aliens left) whenever any of them changes
    public event Action<int, int, int> WaveChanged;

    void Awake()
    {
        GetComponent<Collider>().isTrigger = true;
        foreach (GameObject wave in waves) wave.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (!started || finished || !GameManager.Instance.IsPlaying) return;

        int alive = 0;
        foreach (EnemyController enemy in currentEnemies)
        {
            if (!enemy.IsDead) alive++;
        }

        if (alive != aliensLeft)
        {
            aliensLeft = alive;
            WaveChanged?.Invoke(currentWave + 1, waves.Length, aliensLeft);
            if (aliensLeft == 0) waitTimer = currentWave + 1 < waves.Length ? timeBetweenWaves : winDelay;
        }

        if (aliensLeft > 0) return;

        waitTimer -= Time.deltaTime;
        if (waitTimer > 0f) return;

        if (currentWave + 1 < waves.Length)
        {
            StartWave(currentWave + 1);
        }
        else
        {
            finished = true;
            GameManager.Instance.EndGame(true);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (started || !GameManager.Instance.IsPlaying) return;
        if (other.attachedRigidbody == null || other.attachedRigidbody.GetComponent<PetController>() == null) return;

        started = true;
        gate.Lock();
        GameManager.Instance.SetCheckpoint(respawnPoint.position);
        StartWave(0);
    }

    private void StartWave(int index)
    {
        currentWave = index;
        waves[index].SetActive(true);
        currentEnemies = waves[index].GetComponentsInChildren<EnemyController>();
        aliensLeft = currentEnemies.Length;

        if (spawnEffect != null)
        {
            foreach (EnemyController enemy in currentEnemies)
            {
                spawnEffect.transform.position = enemy.transform.position + Vector3.up;
                spawnEffect.Emit(25);
            }
        }

        UIManager.Instance.ShowMessage($"Wave {index + 1}!");
        if (waveSFX != null) waveSFX.Play();
        WaveChanged?.Invoke(currentWave + 1, waves.Length, aliensLeft);
    }

    public void ResetRunState()
    {
        started = false;
        finished = false;
        currentWave = -1;
        aliensLeft = 0;
        waitTimer = 0f;
        currentEnemies = new EnemyController[0];
        foreach (GameObject wave in waves) wave.SetActive(false);
        if (spawnEffect != null) spawnEffect.Clear();
        WaveChanged?.Invoke(0, waves.Length, 0);
    }
}
