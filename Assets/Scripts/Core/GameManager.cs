using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private TaxiController taxi;

    private int score;
    private int level = 1;
    private int deliveredCount;
    private int requiredDeliveries = 3;

    public int Score => score;
    public int Level => level;
    public int DeliveredCount => deliveredCount;
    public int RequiredDeliveries => requiredDeliveries;

    public System.Action OnScoreChanged;
    public System.Action OnGameOver;
    public System.Action OnLevelComplete;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        if (taxi != null)
        {
            taxi.OnCrash += HandleCrash;
            taxi.OnPassengerDropoff += HandleDelivery;
        }
    }

    private void HandleCrash()
    {
        OnGameOver?.Invoke();
    }

    private void HandleDelivery()
    {
        deliveredCount++;
        score += 100 + Mathf.FloorToInt(taxi.CurrentFuel) * 2;
        OnScoreChanged?.Invoke();

        if (deliveredCount >= requiredDeliveries)
        {
            score += 500;
            level++;
            OnLevelComplete?.Invoke();
        }
        else
        {
            // Spawn new passenger
            PassengerSpawner spawner = FindAnyObjectByType<PassengerSpawner>();
            if (spawner != null) spawner.SpawnPassenger();
        }
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}